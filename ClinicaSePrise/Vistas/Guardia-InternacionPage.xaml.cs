using ClinicaSePrise.Datos;
using ClinicaSePrise.Entidades;

namespace ClinicaSePrise.Vistas;
public partial class Guardia_InternacionPage : ContentPage
{
    public Guardia_InternacionPage()
    {
        InitializeComponent();
    }
    private void OnEmitirDerivacionClicked(object sender, EventArgs e)
    {
        // Método que responde al botón de emitir derivación
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