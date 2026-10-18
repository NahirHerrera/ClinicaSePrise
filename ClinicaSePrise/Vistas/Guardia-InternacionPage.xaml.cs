using ClinicaSePrise.Datos;
using ClinicaSePrise.Entidades;

namespace ClinicaSePrise.Vistas;
public partial class Guardia_InternacionPage : ContentPage
{
    public Guardia_InternacionPage()
    {
        InitializeComponent();

        // --- LÓGICA DE SALUDO DINÁMICO ---
        string nombreCompleto = $"{Sesion.Nombre} {Sesion.Apellido}".Trim();
        string rolActual = Sesion.Rol?.Trim() ?? "";

        bool esRolConNombre = rolActual.Equals("Paciente", StringComparison.OrdinalIgnoreCase) ||
                              rolActual.Equals("Medico", StringComparison.OrdinalIgnoreCase) ||
                              rolActual.Equals("Administracion", StringComparison.OrdinalIgnoreCase);

        if (esRolConNombre)
        {
            string usuario = !string.IsNullOrEmpty(nombreCompleto) ? nombreCompleto : "Usuario";
            lblBienvenida.Text = $"Bienvenido/a, {usuario} ({rolActual})";
        }
        else if (!string.IsNullOrEmpty(rolActual))
        {
            lblBienvenida.Text = $"Bienvenido/a, {rolActual}";
        }
        else
        {
            lblBienvenida.Text = "Bienvenido/a";
        }
        // ---------------------------------
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