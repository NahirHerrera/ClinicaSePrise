using ClinicaSePrise.Datos;
using ClinicaSePrise.Entidades;
using MySqlConnector;
using System.Data;
using Microsoft.Maui.Controls;

namespace ClinicaSePrise.Vistas
{
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

            // Validar que los campos no estén vacíos
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
                using var cn = Conexion.getInstancia().CrearConexion();
                await cn.OpenAsync();

                using var cmd = cn.CreateCommand();
                cmd.CommandText = "IngresoLogin";
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("p_Usu", usuario);
                cmd.Parameters.AddWithValue("p_Pass", password);

                using var reader = await cmd.ExecuteReaderAsync();
                bool datosUsuario = await reader.ReadAsync();

                if (datosUsuario)
                {
                    // Leer valores (idPaciente/idProfesional vienen como int (0 si no hay vínculo) desde el SP)
                    int idUsuario = reader["idUsuario"] != DBNull.Value ? Convert.ToInt32(reader["idUsuario"]) : 0;
                    int idRol = reader["idRol"] != DBNull.Value ? Convert.ToInt32(reader["idRol"]) : 0;
                    string rol = reader["rol"]?.ToString() ?? "";

                    int idPaciente = reader["idPaciente"] != DBNull.Value ? Convert.ToInt32(reader["idPaciente"]) : 0;
                    int idProfesional = reader["idProfesional"] != DBNull.Value ? Convert.ToInt32(reader["idProfesional"]) : 0;

                    string displayNombre = reader["displayNombre"] != DBNull.Value ? reader["displayNombre"].ToString() ?? "" : "";


                    Sesion.IdUsuario = idUsuario;
                    Sesion.IdRol = idRol;
                    Sesion.Rol = rol;
                    Sesion.DisplayNombre = displayNombre;
                    Sesion.IdPaciente = idPaciente;
                    Sesion.IdProfesional = idProfesional;


                    Page paginaDestino = new HomePage();


                    Application.Current.MainPage = new NavigationPage(new MainPage(paginaDestino));
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
        private async void OnCrearCuentaTapped(object sender, EventArgs e)
        {
            await DisplayAlert("Crear cuenta", "Funcionalidad en desarrollo.", "Entendido");
        }
    }
}
