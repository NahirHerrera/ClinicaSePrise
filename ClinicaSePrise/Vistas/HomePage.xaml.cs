using ClinicaSePrise.Datos;
using ClinicaSePrise.Entidades;

namespace ClinicaSePrise.Vistas

    {
        public partial class HomePage : ContentPage
        {
            private Dashboard _dashboardActual;

        // Constructor que recibe los datos desde el Login

        public HomePage(Dashboard dashboard)
        {
            InitializeComponent();

            _dashboardActual = dashboard;
            BindingContext = dashboard;

            // Comprobamos el rol recibido desde el login
            string rol = dashboard.Rol?.Trim() ?? "";

            if (rol.Equals("Paciente", StringComparison.OrdinalIgnoreCase))
            {
                Sesion.IdPaciente = 1; // TEMPORAL

                // Si querés que cargue algo específico o maneje la vista, podés dejarlo así:
                VistaStaff.IsVisible = false;
                VistaPaciente.IsVisible = true;
            }
            else
            {
                VistaPaciente.IsVisible = false;
                VistaStaff.IsVisible = true;
            }
        }

        private void OnNuevoTurno(object sender, EventArgs e)
            {
                // Lógica para el botón de nuevo turno
            }

            private async void OnCerrarSesion(object? sender, EventArgs e)
            {
                bool confirmar = await DisplayAlertAsync("Cerrar sesión", "¿Querés cerrar la sesión?", "Sí", "No");
                if (!confirmar)
                    return;

                // Volvemos al login; los datos de conexión a MySQL quedan guardados en Conexion
                Window.Page = new NavigationPage(new LoginPage());
            }
        }
    }