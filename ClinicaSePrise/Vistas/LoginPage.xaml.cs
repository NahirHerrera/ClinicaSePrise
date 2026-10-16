using ClinicaSePrise.Datos;
using ClinicaSePrise.Entidades;
using System.Data;

namespace ClinicaSePrise.Vistas;

public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();
    }
    private async void OnIngresarClicked(object sender, EventArgs e)
    {
        string usuario = TxtUsuario.Text?.Trim();
        string password = TxtPassword.Text;

        // 1. Validar que los campos no estén vacíos
        if (string.IsNullOrEmpty(usuario) || string.IsNullOrEmpty(password))
        {
            MostrarError("Por favor, complete todos los campos.");
            return;
        }

        // Mostrar indicador de carga y bloquear botón
        IndicadorCarga.IsVisible = true;
        IndicadorCarga.IsRunning = true;
        BtnIngresar.IsEnabled = false;
        FrameError.IsVisible = false;

        try
        {
            // 2. Llamamos al método que agregamos en tu clase Conexion
            using var cn = Conexion.getInstancia().CrearConexion();
            await cn.OpenAsync();

            using var cmd = cn.CreateCommand();
            cmd.CommandText = "IngresoLogin";
            cmd.CommandType = System.Data.CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("p_Usu", usuario);
            cmd.Parameters.AddWithValue("p_Pass", password);

            using var reader = await cmd.ExecuteReaderAsync();

            bool datosUsuario = await reader.ReadAsync();

            if (datosUsuario)
            {
                // TODO: restaurar SesionActual y el switch cuando existan las vistas
                Page paginaDestino = new HomePage(new Dashboard());

                // Cambiamos la pantalla raíz de la aplicación
                Application.Current.MainPage = new NavigationPage(paginaDestino);
            }
            else
            {
                MostrarError("Usuario o contraseña incorrectos.");
            }
        }
        catch (Exception ex)
        {
            MostrarError("Error al conectar con la base de datos.");
        }
        finally
        {
            IndicadorCarga.IsVisible = false;
            IndicadorCarga.IsRunning = false;
            BtnIngresar.IsEnabled = true;
        }
    }

    private void MostrarError(string mensaje)
    {
        LblError.Text = mensaje;
        FrameError.IsVisible = true;
    }
}