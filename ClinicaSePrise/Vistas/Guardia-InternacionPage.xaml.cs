namespace ClinicaSePrise.Vistas;

public partial class Guardia_InternacionPage : ContentPage
{
    public Guardia_InternacionPage()
    {
        InitializeComponent();
    }

    void OnCerrarSesionClicked(object sender, EventArgs e) => Window.Page = new LoginPage();

    private void OnEmitirDerivacionClicked(object sender, EventArgs e)
    {
        // Método que responde al botón de emitir derivación
    }
}