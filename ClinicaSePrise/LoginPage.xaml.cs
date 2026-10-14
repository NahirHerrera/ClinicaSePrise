using ClinicaSePrise.Services;

namespace ClinicaSePrise
{
    public partial class LoginPage : ContentPage
    {
        private readonly DashboardService _dashboardService;

        public LoginPage()
        {
            InitializeComponent();
            _dashboardService = new DashboardService();
        }

        private async void OnIngresarClicked(object sender, EventArgs e)
        {
            string usuario = TxtUsuario.Text?.Trim() ?? string.Empty;
            string password = TxtPassword.Text?.Trim() ?? string.Empty;

            if (string.IsNullOrEmpty(usuario) || string.IsNullOrEmpty(password))
            {
                MostrarError("Por favor ingrese su usuario y contraseña.");
                return;
            }

            // Ocultar errores previos y mostrar animación de carga
            FrameError.IsVisible = false;
            BtnIngresar.IsVisible = false;
            IndicadorCarga.IsVisible = true;
            IndicadorCarga.IsRunning = true;

            try
            {
                // Consultamos el Dashboard o la validación del usuario
                var datosDashboard = await Task.Run(() => _dashboardService.Obtener(usuario));

                if (datosDashboard != null)
                {
                    // Guardar el usuario activo o navegar a la MainPage pasando los datos del usuario
                    Application.Current.MainPage = new NavigationPage(new HomePage(datosDashboard));
                }
                else
                {
                    MostrarError("Usuario o clave incorrectos.");
                }
            }
            catch (Exception ex)
            {
                MostrarError("Error de conexión con la base de datos.");
            }
            finally
            {
                IndicadorCarga.IsRunning = false;
                IndicadorCarga.IsVisible = false;
                BtnIngresar.IsVisible = true;
            }
        }

        private void MostrarError(string mensaje)
        {
            LblError.Text = mensaje;
            FrameError.IsVisible = true;
        }
    }
}