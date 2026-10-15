using ClinicaSePrise.Entidades;
using MySqlConnector;

namespace ClinicaSePrise.Datos;

public class Conexion
{
    const string Cadena = "Server=localhost;Database=ClinicaSePrise;User=root;Password=1234;";

    static int Contar(MySqlConnection cn, string sql) =>
        Convert.ToInt32(new MySqlCommand(sql, cn).ExecuteScalar());

    // NUEVO MÉTODO PARA VALIDAR EL LOGIN Y RETORNAR EL ROL Y VÍNCULOS
    public UsuarioDto ValidarLogin(string usuario, string password)
    {
        using var cn = new MySqlConnection(Cadena);
        cn.Open();

        string query = @"SELECT u.idUsuario, u.usuario, r.nombre AS rol, u.idRol, u.idPaciente, u.idProfesional 
                         FROM Usuario u 
                         JOIN Rol r ON u.idRol = r.idRol 
                         WHERE u.usuario = @u AND u.contrasenia = @p";

        using var cmd = new MySqlCommand(query, cn);
        cmd.Parameters.AddWithValue("@u", usuario);
        cmd.Parameters.AddWithValue("@p", password);

        using var rd = cmd.ExecuteReader();
        if (rd.Read())
        {
            return new UsuarioDto
            {
                IdUsuario = rd.GetInt32("idUsuario"),
                Usuario = rd.GetString("usuario"),
                Rol = rd.GetString("rol"),
                IdRol = rd.GetInt32("idRol"),
                IdPaciente = rd.IsDBNull(rd.GetOrdinal("idPaciente")) ? (int?)null : rd.GetInt32("idPaciente"),
                IdProfesional = rd.IsDBNull(rd.GetOrdinal("idProfesional")) ? (int?)null : rd.GetInt32("idProfesional")
            };
        }

        return null; // Si no coincide el usuario o contraseña
    }

    public Dashboard Obtener(string usuario)
    {
        using var cn = new MySqlConnection(Cadena);
        cn.Open();

        var d = new Dashboard { Usuario = char.ToUpper(usuario[0]) + usuario[1..] };

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
        using (var rd = cmd.ExecuteReader())
            while (rd.Read()) d.Pantallas.Add(rd.GetString(0));

        d.TurnosHoy = Contar(cn, "SELECT COUNT(*) FROM Turno WHERE DATE(fechaHora) = CURDATE()");
        d.Atendidos = Contar(cn, "SELECT COUNT(*) FROM Atencion WHERE DATE(fechaHoraFin) = CURDATE()");
        d.EnEspera = Contar(cn, "SELECT COUNT(*) FROM Turno WHERE DATE(fechaHora) = CURDATE() AND estado = 'En espera'");
        d.InsumosCriticos = Contar(cn, "SELECT COUNT(*) FROM Insumo WHERE stock <= stockMinimo");

        using (var rd = new MySqlCommand(@"SELECT t.fechaHora, t.duracion, CONCAT(p.nombre, ' ', p.apellido), p.dni, CONCAT(pr.nombre, ' ', pr.apellido), t.estado
                FROM Turno t
                JOIN Paciente p ON t.idPaciente = p.idPaciente
                JOIN Profesional pr ON t.idProfesional = pr.idProfesional
                WHERE DATE(t.fechaHora) = CURDATE() ORDER BY t.fechaHora", cn).ExecuteReader())
            while (rd.Read())
                d.Turnos.Add(new()
                {
                    Hora = rd.GetDateTime(0).ToString("HH:mm"),
                    Duracion = $"{rd.GetInt32(1)} min",
                    Paciente = rd.GetString(2),
                    Dni = "DNI " + rd.GetString(3),
                    Profesional = rd.GetString(4),
                    Estado = rd.GetString(5)
                });

        using (var rd = new MySqlCommand(@"SELECT i.nombre, e.responsable, i.stock
                FROM Insumo i JOIN Estudio e ON i.idEstudio = e.idEstudio
                WHERE i.stock <= i.stockMinimo", cn).ExecuteReader())
            while (rd.Read())
                d.Stock.Add(new() { Nombre = rd.GetString(0), Sector = rd.GetString(1), Stock = rd.GetInt32(2) });

        return d;
    }
}

// Clase auxiliar de transferencia de datos para el usuario logueado
public class UsuarioDto
{
    public int IdUsuario { get; set; }
    public string Usuario { get; set; }
    public string Rol { get; set; }
    public int IdRol { get; set; }
    public int? IdPaciente { get; set; }
    public int? IdProfesional { get; set; }
}