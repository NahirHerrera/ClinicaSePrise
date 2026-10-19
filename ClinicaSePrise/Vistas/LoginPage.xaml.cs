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
            // 2. Llamamos al procedimiento almacenado IngresoLogin actualizado
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
                // Obtenemos todos los datos devueltos en un solo paso
                int idUsuario = Convert.ToInt32(reader["idUsuario"]);
                int idRol = Convert.ToInt32(reader["idRol"]);
                string rol = reader["rol"]?.ToString() ?? "";

                int? idPaciente = reader["idPaciente"] == DBNull.Value ? null : Convert.ToInt32(reader["idPaciente"]);
                int? idProfesional = reader["idProfesional"] == DBNull.Value ? null : Convert.ToInt32(reader["idProfesional"]);

                string nombre = reader["nombre"] != DBNull.Value ? reader["nombre"].ToString() : "";
                string apellido = reader["apellido"] != DBNull.Value ? reader["apellido"].ToString() : "";

                // Guardamos los datos en la clase estática Sesion
                Sesion.IdUsuario = idUsuario;
                Sesion.IdRol = idRol;
                Sesion.Rol = rol;
                Sesion.Nombre = nombre;
                Sesion.Apellido = apellido;

                if (idPaciente.HasValue)
                    Sesion.IdPaciente = idPaciente.Value;

                if (idProfesional.HasValue)
                    Sesion.IdProfesional = idProfesional.Value;

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

    private async void OnForgotPasswordTapped(object sender, EventArgs e)
    {
        // 1. Solicita el correo electrónico de forma limpia en una ventana emergente nativa
        string email = await DisplayPromptAsync(
            title: "Recuperar Contraseña",
            message: "Ingresa el correo electrónico con el cual te registraste:",
            placeholder: "ejemplo@correo.com",
            accept: "Enviar",
            cancel: "Cancelar");

        // 2. Si el usuario ingresó un correo y presionó enviar, simula la respuesta
        if (!string.IsNullOrWhiteSpace(email))
        {
            await DisplayAlert(
                title: "Enlace Enviado",
                message: $"Se ha enviado un link de recuperación al correo:\n{email}\n\nPor favor, revisa tu bandeja de entrada o la carpeta de spam.",
                cancel: "Entendido");
        }
    }

    private void MostrarError(string mensaje)
    {
        LblError.Text = mensaje;
        FrameError.IsVisible = true;
    }

    void OnCrearCuentaTapped(object sender, TappedEventArgs e) => Window.Page = new RegistroPage();
}