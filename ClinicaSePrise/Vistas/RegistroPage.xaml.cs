using ClinicaSePrise.Datos;
using MySqlConnector;

namespace ClinicaSePrise.Vistas;

public partial class RegistroPage : ContentPage
{
    public RegistroPage() => InitializeComponent();

    void OnVolverClicked(object sender, EventArgs e) => Window.Page = new LoginPage();

    async void OnRegistrarClicked(object sender, EventArgs e)
    {
        if (new[] { txtNombre, txtApellido, txtDni, txtTelefono, txtEmail, txtUsuario, txtPassword }.Any(x => string.IsNullOrWhiteSpace(x.Text)))
        {
            await Window.Page!.DisplayAlertAsync("Faltan datos", "Completá todos los campos.", "OK");
            return;
        }
        if (txtPassword.Text != txtPassword2.Text)
        {
            await Window.Page!.DisplayAlertAsync("Contraseña", "Las contraseñas no coinciden.", "OK");
            return;
        }

        try
        {
            var error = await Db.Query("CALL RegistrarPaciente(@n,@a,@d,@t,@e,@u,@p)", r => r.GetString(0),
                new MySqlParameter("@n", txtNombre.Text.Trim()), new MySqlParameter("@a", txtApellido.Text.Trim()),
                new MySqlParameter("@d", txtDni.Text.Trim()), new MySqlParameter("@t", txtTelefono.Text.Trim()),
                new MySqlParameter("@e", txtEmail.Text.Trim()), new MySqlParameter("@u", txtUsuario.Text.Trim()),
                new MySqlParameter("@p", txtPassword.Text));

            if (error.Count > 0)
            {
                await Window.Page!.DisplayAlertAsync("No se posible registrar este paciente", error[0], "OK");
                return;
            }
            await Window.Page!.DisplayAlertAsync("Listo", "Cuenta creada. Ya podés iniciar sesión.", "OK");
            Window.Page = new LoginPage();
        }
        catch (Exception ex) { await Window.Page!.DisplayAlertAsync("Error", ex.Message, "OK"); }
    }
}