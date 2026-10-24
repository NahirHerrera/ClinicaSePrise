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
        CargarEstudiosEnPicker();
    }
    private async void OnEmitirDerivacionClicked(object sender, EventArgs e)
    {
        var dni = EntryDNI.Text?.Trim();
        var origen = PickerOrigen.SelectedItem as string;
        var estudio = PickerEstudio.SelectedItem as Estudio;

        if (string.IsNullOrWhiteSpace(dni))
        {
            await Avisos.Mostrar("Error", "Ingrese el DNI del paciente.");
            return;
        }
        if (origen == null)
        {
            await Avisos.Mostrar("Error", "Seleccione el origen.");
            return;
        }
        if (estudio == null)
        {
            await Avisos.Mostrar("Error", "Seleccione el estudio.");
            return;
        }

        try
        {
            InsertarDerivacion(dni, origen, estudio.idEstudio);

            EntryDNI.Text = string.Empty;
            PickerOrigen.SelectedItem = null;
            PickerEstudio.SelectedItem = null;
            Derivaciones.ItemsSource = CargarDerivaciones();

            await Avisos.Mostrar("Éxito", "Derivación creada correctamente.");
        }
        catch (MySqlException mex)
        {
            await Avisos.Mostrar("Error", mex.Message);
        }
        catch (Exception ex)
        {
            await Avisos.Mostrar("Error", ex.Message);
        }
    }
    private void InsertarDerivacion(string dni, string origen, int idEstudio)
    {
        using var conn = Conexion.getInstancia().CrearConexion();
        conn.Open();

        using var cmd = new MySqlCommand("CrearDerivacion", conn)
        {
            CommandType = System.Data.CommandType.StoredProcedure
        };
        cmd.Parameters.AddWithValue("p_dni", dni);
        cmd.Parameters.AddWithValue("p_origen", origen);
        cmd.Parameters.AddWithValue("p_idEstudio", idEstudio);
        cmd.Parameters.AddWithValue("p_estado", "Pendiente");
        cmd.ExecuteNonQuery();
    }

    private void CargarEstudiosEnPicker()
    {
        try
        {
            PickerEstudio.ItemsSource = CargarEstudios();
        }
        catch (Exception ex)
        {
            PickerEstudio.Title = "Error al cargar estudios: " + ex.Message;
        }
    }

    private List<Estudio> CargarEstudios()
    {
        var lista = new List<Estudio>();
        var conexion = Conexion.getInstancia();

        using var conn = conexion.CrearConexion();
        conn.Open();

        using var cmd = new MySqlCommand("ListaEstudios", conn)
        {
            CommandType = System.Data.CommandType.StoredProcedure
        };

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            lista.Add(new Estudio
            {
                idEstudio = reader.GetInt32(reader.GetOrdinal("idEstudio")),
                nombre = reader.GetString(reader.GetOrdinal("nombre"))
            });
        }

        return lista;
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