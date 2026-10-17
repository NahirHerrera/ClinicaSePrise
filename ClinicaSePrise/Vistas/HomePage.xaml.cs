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