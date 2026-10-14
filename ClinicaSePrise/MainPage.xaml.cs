using ClinicaSePrise.Services;

namespace ClinicaSePrise;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
        BindingContext = new DashboardService().Obtener("andrea"); // provisorio hasta tener el login
    }
    private async void OnNuevoTurno(object sender, EventArgs e) =>
        await DisplayAlertAsync("Turno", "Nuevo turno", "OK");
}