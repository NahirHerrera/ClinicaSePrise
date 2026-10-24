    using ClinicaSePrise.Datos;
    using ClinicaSePrise.Entidades;
    using MySqlConnector;

    namespace ClinicaSePrise.Vistas;

public partial class Guardia_InternacionPage : ContentPage
{
    public Guardia_InternacionPage()
    {
        InitializeComponent();
        Derivaciones.ItemsSource = CargarDerivaciones();
    }
    private void OnEmitirDerivacionClicked(object sender, EventArgs e)
    {
        // Método que responde al botón de emitir derivación
    }

    private List<Derivacion> CargarDerivaciones()
    {
        var lista = new List<Derivacion>();
        var conexion = Conexion.getInstancia();

        using var conn = conexion.CrearConexion();
        conn.Open();

        using var cmd = new MySqlCommand("ListaDerivaciones", conn)
        {
            CommandType = System.Data.CommandType.StoredProcedure
        };

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            int Ord(string name)
            {
                try { return reader.GetOrdinal(name); }
                catch { return -1; }
            }

            int ordIdDer = Ord("idDerivacion");
            int ordIdPac = Ord("idPaciente");
            int ordNombre = Ord("Nombre");
            int ordApellido = Ord("Apellido");
            int ordOrigen = Ord("origen");
            int ordIdEst = Ord("idEstudio");
            int ordEstudioNombre = Ord("EstudioNombre");
            int ordEstado = Ord("estado");


            var ent = new Derivacion
            {
                IdDerivacion = (ordIdDer >= 0 && !reader.IsDBNull(ordIdDer)) ? reader.GetInt32(ordIdDer) : 0,
                IdPaciente = (ordIdPac >= 0 && !reader.IsDBNull(ordIdPac)) ? (int?)reader.GetInt32(ordIdPac) : null,
                Nombre = (ordNombre >= 0 && !reader.IsDBNull(ordNombre)) ? reader.GetString(ordNombre) : string.Empty,
                Apellido = (ordApellido >= 0 && !reader.IsDBNull(ordApellido)) ? reader.GetString(ordApellido) : string.Empty,
                Origen = (ordOrigen >= 0 && !reader.IsDBNull(ordOrigen)) ? reader.GetString(ordOrigen) : string.Empty,
                IdEstudio = (ordIdEst >= 0 && !reader.IsDBNull(ordIdEst)) ? (int?)reader.GetInt32(ordIdEst) : null,
                EstudioNombre = (ordEstudioNombre >= 0 && !reader.IsDBNull(ordEstudioNombre)) ? reader.GetString(ordEstudioNombre) : string.Empty,
                Estado = (ordEstado >= 0 && !reader.IsDBNull(ordEstado)) ? reader.GetString(ordEstado) : string.Empty
                };

            lista.Add(ent);
        }

        return lista;
    }
}