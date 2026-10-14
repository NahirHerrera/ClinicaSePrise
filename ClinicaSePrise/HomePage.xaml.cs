using ClinicaSePrise.Models;

namespace ClinicaSePrise
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
    }
}