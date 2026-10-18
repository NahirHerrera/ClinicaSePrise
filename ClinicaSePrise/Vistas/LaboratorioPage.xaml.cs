namespace ClinicaSePrise.Vistas;

public partial class LaboratorioPage : ContentPage
{
	public LaboratorioPage()
	{
		InitializeComponent();
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