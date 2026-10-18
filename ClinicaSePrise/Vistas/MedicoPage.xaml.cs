namespace ClinicaSePrise.Vistas;

public partial class MedicoPage : ContentPage
{
    public MedicoPage()
    {
        InitializeComponent();
    }
    private void OnFinalizarConsultaClicked(object sender, EventArgs e)
    {
        // Método para finalizar la consulta médica
    }
    private async void OnCerrarSesionClicked(object sender, EventArgs e)
    {
        bool confirmar = await DisplayAlert("Cerrar Sesión", "¿Estás seguro de que deseas salir?", "Sí", "Cancelar");
        if (confirmar)
        {
            Application.Current.MainPage = new NavigationPage(new LoginPage());
        }
    }
}