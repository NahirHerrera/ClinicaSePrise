using ClinicaSePrise.Datos;
using ClinicaSePrise.Entidades;

namespace ClinicaSePrise.Vistas;

public partial class AsistencialPage : ContentPage
{
    public AsistencialPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await stock.Cargar();
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