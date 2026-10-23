using ClinicaSePrise.Datos;
using ClinicaSePrise.Entidades;
using MySqlConnector;

namespace ClinicaSePrise.Vistas;

public partial class AsistencialPage : ContentPage
{
    public AsistencialPage()
    {
        InitializeComponent();
        _ = CargarTodo();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CargarTodo();
    }

    async Task CargarTodo()
    {
        await CargarImagenes();
        await stock.Cargar();
    }

    async Task CargarPendientes()
    {
        var pendientes = await Db.Query("CALL ImagenesPendientes()",
            r => new TurnoImagen(r.GetInt32(0), r.GetString(1), r["dni"].ToString(), r.GetString(3),
                                 r.GetDateTime(4), r.GetString(5), r.GetString(6)));
        BindableLayout.SetItemsSource(ListaPendientes, pendientes);
        SinPendientes.IsVisible = pendientes.Count == 0;
    }

    async Task CargarImagenes()
    {
        try
        {
            await CargarPendientes();

            var atendidos = await Db.Query("CALL ImagenesAtendidosHoy()",
                r => new EstudioAtendido(r.GetInt32(0), r.GetString(1), r["dni"].ToString(), r.GetString(3),
                                         r.GetDateTime(4), r.GetString(5)));
            BindableLayout.SetItemsSource(ListaAtendidos, atendidos);
            SinAtendidos.IsVisible = atendidos.Count == 0;
        }
        catch (Exception ex) { await DisplayAlert("Error", ex.Message, "OK"); }
    }

    async void OnLlamar(object sender, EventArgs e)
    {
        if (sender is not Button b || b.BindingContext is not TurnoImagen t) return;
        b.IsEnabled = false;
        try
        {
            await Db.Query("CALL LlamarPacienteImagen(@t)", r => Convert.ToInt32(r[0]),
                new MySqlParameter("@t", t.idTurno));
            await CargarPendientes();
        }
        catch (Exception ex) { b.IsEnabled = true; await DisplayAlert("Error", ex.Message, "OK"); }
    }

    async void OnAtender(object sender, EventArgs e)
    {
        if (sender is not Button b || b.BindingContext is not TurnoImagen t) return;
        b.IsEnabled = false;
        try
        {
            var res = await Db.Query("CALL AtenderTurnoImagen(@t)",
                r => r.GetName(0) == "mensaje"
                    ? (false, r.GetString(0))
                    : (true, $"N° {r.GetInt32(0)}\nPaciente: {r.GetString(1)}\nEstudio: {r.GetString(2)}\nAtendido: {r.GetDateTime(3):dd/MM/yyyy HH:mm}\nRetirar el informe a partir del: {r.GetDateTime(4):dd/MM/yyyy}"),
                new MySqlParameter("@t", t.idTurno));
            var (ok, texto) = res.First();

            await DisplayAlert(ok ? "Comprobante de retiro" : "No se pudo", texto, "OK");
            await CargarTodo();
        }
        catch (Exception ex) { b.IsEnabled = true; await DisplayAlert("Error", ex.Message, "OK"); }
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

public record TurnoImagen(int idTurno, string paciente, string dni, string estudio, DateTime fechaHora, string indicacion, string estado)
{
    public bool pendiente => estado == "Pendiente";
    public bool llamado => estado == "Llamado";
}

public record EstudioAtendido(int idTurno, string paciente, string dni, string estudio, DateTime fechaAtencion, string indicacion);