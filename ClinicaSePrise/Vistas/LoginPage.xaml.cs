using ClinicaSePrise.Datos;
using ClinicaSePrise.Entidades;
using MySqlConnector;
using System.Data;

namespace ClinicaSePrise.Vistas;

public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();
    }
    private async void OnIngresarClicked(object sender, EventArgs e)
    {
        string usuario = TxtUsuario.Text?.Trim();
        string password = TxtPassword.Text;

        // 1. Validar que los campos no estén vacíos
        if (string.IsNullOrEmpty(usuario) || string.IsNullOrEmpty(password))
        {
            MostrarError("Por favor, complete todos los campos.");
            return;
        }

        // Mostrar indicador de carga y bloquear botón
        IndicadorCarga.IsVisible = true;
        IndicadorCarga.IsRunning = true;
        BtnIngresar.IsEnabled = false;
        FrameError.IsVisible = false;

        try
        {
            // 2. Llamamos al método que agregamos en tu clase Conexion
            using var cn = Conexion.getInstancia().CrearConexion();
            await cn.OpenAsync();

            using var cmd = cn.CreateCommand();
            cmd.CommandText = "IngresoLogin";
            cmd.CommandType = System.Data.CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("p_Usu", usuario);
            cmd.Parameters.AddWithValue("p_Pass", password);

            using var reader = await cmd.ExecuteReaderAsync();

            bool datosUsuario = await reader.ReadAsync();

            if (datosUsuario)
            {
                // Obtenemos los datos del usuario desde la base
                int idUsuario = Convert.ToInt32(reader["idUsuario"]);
                int idRol = Convert.ToInt32(reader["idRol"]);

                int? idPaciente = reader["idPaciente"] == DBNull.Value
                    ? null
                    : Convert.ToInt32(reader["idPaciente"]);

                int? idProfesional = reader["idProfesional"] == DBNull.Value
                    ? null
                    : Convert.ToInt32(reader["idProfesional"]);

                // Guardamos el identificador del paciente
                if (idPaciente.HasValue)
                    Sesion.IdPaciente = idPaciente.Value;

                // Obtenemos el nombre del rol
                string rol = "";

                // 1. Cerramos la conexión si quedó abierta de la consulta anterior
                if (cn.State == System.Data.ConnectionState.Open)
                {
                    await cn.CloseAsync();
                }

                // 2. La volvemos a abrir solo para buscar el rol
                await cn.OpenAsync();

                using (var cmdRol = cn.CreateCommand())
                {
                    cmdRol.CommandText = "SELECT nombre FROM Rol WHERE idRol = @idRol;";
                    cmdRol.Parameters.AddWithValue("@idRol", idRol);

                    object resultado = await cmdRol.ExecuteScalarAsync();
                    rol = resultado?.ToString() ?? "";
                }
                // Redirigimos a la pantalla específica según el rol obtenido de la base de datos
                // Redirigimos a la pantalla específica según el rol obtenido de la base de datos
                // Redirigimos a la pantalla específica según el rol obtenido de la base de datos
                string rolLimpio = rol?.ToLower().Trim() ?? "";

                Page paginaDestino;

                if (rolLimpio.Contains("paciente"))
                {
                    paginaDestino = new PacientePage();
                }
                else if (rolLimpio.Contains("recepcion"))
                {
                    paginaDestino = new RecepcionPage();
                }
                else if (rolLimpio.Contains("medico"))
                {
                    paginaDestino = new MedicoPage();
                }
                else if (rolLimpio.Contains("administracion"))
                {
                    paginaDestino = new AdministracionPage();
                }
                else if (rolLimpio.Contains("guardia") || rolLimpio.Contains("internacion"))
                {
                    paginaDestino = new Guardia_InternacionPage();
                }
                else if (rolLimpio.Contains("laboratorio"))
                {
                    paginaDestino = new LaboratorioPage();
                }
                else if (rolLimpio.Contains("asistencial"))
                {
                    paginaDestino = new AsistencialPage();
                }
                else
                {
                    // Si entra acá, el cartel te dirá exactamente qué texto falló en la BD
                    await DisplayAlert("Depuración de Rol", $"El texto recibido de la BD fue: '{rol}'", "OK");
                    paginaDestino = new HomePage(new Dashboard());
                }

                Application.Current.MainPage = new NavigationPage(paginaDestino);

            }
            else
            {
                MostrarError("Usuario o contraseña incorrectos.");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.ToString(), "OK");
        }
        finally
        {
            IndicadorCarga.IsVisible = false;
            IndicadorCarga.IsRunning = false;
            BtnIngresar.IsEnabled = true;
        }
    }
    private void MostrarError(string mensaje)
    {
        LblError.Text = mensaje;
        FrameError.IsVisible = true;
    }

    void OnCrearCuentaTapped(object sender, TappedEventArgs e) => Window.Page = new RegistroPage();
}