using ClinicaSePrise.Datos;
using MySqlConnector;

namespace ClinicaSePrise.Vistas;

public partial class ConexionPage : ContentPage
{
	public ConexionPage()
	{
		InitializeComponent();
	}

	private async void Enviar_Clicked(object? sender, EventArgs e)
	{
        string puerto = txtPuerto.Text.Trim();
        string usuario = txtUsuario.Text.Trim();
        string password = txtPassword.Text;

        if (string.IsNullOrWhiteSpace(puerto) || string.IsNullOrWhiteSpace(usuario))
        {
            await DisplayAlert("Validación", "Se deben completar los campos puerto y usuario", "OK");
            return;
        }

        Conexion.getInstancia().SetCredenciales(puerto, usuario, password);

        try
        {

            // Prueba de conexión a la base de datos
            using (MySqlConnection cn = Conexion.getInstancia().CrearConexion())
            {
                // Abrir la conexión para verificar que funcione correctamente
                await cn.OpenAsync();
                await DisplayAlert("Exito","¡Conexión exitosa a la base de datos!","OK");
                await Navigation.PushAsync(new LoginPage());
            }
        }
        catch (Exception ex)
        {
            // Manejo de errores de conexión
            await DisplayAlert("Error","Error al conectar: " + ex.Message, "ERROR");
        }



    }
}