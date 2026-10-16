using ClinicaSePrise.Entidades;
using MySqlConnector;

namespace ClinicaSePrise.Datos;

public class Conexion
{
    static string Cadena;

    public static bool Configurar(string user, string pass)
    {
        Cadena = $"Server=localhost;Database=ClinicaSePrise;User={user};Password={pass};";
        try { using var cn = Abrir(); return true; }
        catch { return false; }
    }

    static MySqlConnection Abrir()
    {
        var cn = new MySqlConnection(Cadena);
        cn.Open();
        return cn;
    }

    static int Contar(MySqlConnection cn, string sql) =>
        Convert.ToInt32(new MySqlCommand(sql, cn).ExecuteScalar());

    public UsuarioDto ValidarLogin(string usuario, string password)
    {
        using var cn = Abrir();
        var cmd = new MySqlCommand(@"SELECT u.idUsuario, u.usuario, r.nombre AS rol, u.idRol, u.idPaciente, u.idProfesional
                                     FROM Usuario u JOIN Rol r ON u.idRol = r.idRol
                                     WHERE u.usuario = @u AND u.contrasenia = @p", cn);
        cmd.Parameters.AddWithValue("@u", usuario);
        cmd.Parameters.AddWithValue("@p", password);

        using var rd = cmd.ExecuteReader();
        if (!rd.Read()) return null;

        return new UsuarioDto
        {
            IdUsuario = rd.GetInt32("idUsuario"),
            Usuario = rd.GetString("usuario"),
            Rol = rd.GetString("rol"),
            IdRol = rd.GetInt32("idRol"),
            IdPaciente = rd.IsDBNull(rd.GetOrdinal("idPaciente")) ? null : rd.GetInt32("idPaciente"),
            IdProfesional = rd.IsDBNull(rd.GetOrdinal("idProfesional")) ? null : rd.GetInt32("idProfesional")
        };
    }

    public Dashboard Obtener(string usuario)
    {
        using var cn = Abrir();
        var d = new Dashboard { Usuario = char.ToUpper(usuario[0]) + usuario[1..] };

        CargarRol(cn, d, usuario);

        d.TurnosHoy = Contar(cn, "SELECT COUNT(*) FROM Turno WHERE DATE(fechaHora) = CURDATE()");
        d.Atendidos = Contar(cn, "SELECT COUNT(*) FROM Atencion WHERE DATE(fechaHoraFin) = CURDATE()");
        d.EnEspera = Contar(cn, "SELECT COUNT(*) FROM Turno WHERE DATE(fechaHora) = CURDATE() AND estado = 'En espera'");
        d.InsumosCriticos = Contar(cn, "SELECT COUNT(*) FROM Insumo WHERE stock <= stockMinimo");

        CargarTurnos(cn, d);
        CargarStock(cn, d);
        return d;
    }

    static void CargarRol(MySqlConnection cn, Dashboard d, string usuario)
    {
        var cmd = new MySqlCommand("SELECT r.idRol, r.nombre FROM Usuario u JOIN Rol r ON u.idRol = r.idRol WHERE u.usuario = @u", cn);
        cmd.Parameters.AddWithValue("@u", usuario);
        int idRol;
        using (var rd = cmd.ExecuteReader())
        {
            rd.Read();
            idRol = rd.GetInt32(0);
            d.Rol = rd.GetString(1);
        }

        cmd = new MySqlCommand("SELECT pantalla FROM RolPantalla WHERE idRol = @r ORDER BY FIELD(pantalla,'Dashboard','Turnos','Agenda','Pacientes','Estudios','Insumos','Comprobantes','Honorarios')", cn);
        cmd.Parameters.AddWithValue("@r", idRol);
        using var rd2 = cmd.ExecuteReader();
        while (rd2.Read()) d.Pantallas.Add(rd2.GetString(0));
    }

    static void CargarTurnos(MySqlConnection cn, Dashboard d)
    {
        using var rd = new MySqlCommand(@"SELECT t.idTurno, t.idPaciente, t.idProfesional, t.especialidad, t.fechaHora, t.tipoTurno, t.duracion, t.modalidadSolicitud, t.estado,
            CONCAT(p.nombre, ' ', p.apellido), p.dni, CONCAT(pr.nombre, ' ', pr.apellido)
            FROM Turno t
            JOIN Paciente p ON t.idPaciente = p.idPaciente
            JOIN Profesional pr ON t.idProfesional = pr.idProfesional
            WHERE DATE(t.fechaHora) = CURDATE() ORDER BY t.fechaHora", cn).ExecuteReader();
        while (rd.Read())
            d.Turnos.Add(new Turno(
                rd.GetInt32(0),
                rd.IsDBNull(1) ? null : rd.GetInt32(1),
                rd.IsDBNull(2) ? null : rd.GetInt32(2),
                rd.IsDBNull(3) ? null : rd.GetString(3),
                rd.IsDBNull(4) ? null : rd.GetDateTime(4),
                rd.IsDBNull(5) ? null : rd.GetString(5),
                rd.IsDBNull(6) ? null : rd.GetInt32(6),
                rd.IsDBNull(7) ? null : rd.GetString(7),
                rd.IsDBNull(8) ? null : rd.GetString(8))
            {
                nombrePaciente = rd.GetString(9),
                dniPaciente = rd.GetString(10),
                nombreProfesional = rd.GetString(11)
            });
    }

    static void CargarStock(MySqlConnection cn, Dashboard d)
    {
        using var rd = new MySqlCommand(@"SELECT i.idInsumo, i.nombre, i.idEstudio, i.stock, i.stockMinimo, i.estadoReposicion, e.responsable
            FROM Insumo i JOIN Estudio e ON i.idEstudio = e.idEstudio
            WHERE i.stock <= i.stockMinimo", cn).ExecuteReader();
        while (rd.Read())
            d.Stock.Add(new Insumo(
                rd.GetInt32(0),
                rd.GetString(1),
                rd.IsDBNull(2) ? null : rd.GetInt32(2),
                rd.IsDBNull(3) ? null : rd.GetInt32(3),
                rd.GetInt32(4),
                rd.IsDBNull(5) ? null : rd.GetString(5))
            {
                sector = rd.GetString(6)
            });
    }
}

public class UsuarioDto
{
    public int IdUsuario { get; set; }
    public string Usuario { get; set; }
    public string Rol { get; set; }
    public int IdRol { get; set; }
    public int? IdPaciente { get; set; }
    public int? IdProfesional { get; set; }
}