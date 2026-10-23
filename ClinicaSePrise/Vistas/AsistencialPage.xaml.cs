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

    Task Alerta(string titulo, string mensaje) =>
        Application.Current.Windows[0].Page.DisplayAlert(titulo, mensaje, "OK");

    async Task CargarTodo()
    {
        await CargarImagenes();
        try { await stock.Cargar(); }
        catch (Exception ex) { await Alerta("Error en stock", ex.Message); }
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
        catch (Exception ex) { await Alerta("Error", ex.Message); }
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
        catch (Exception ex) { b.IsEnabled = true; await Alerta("Error", ex.Message); }
    }


    private async void OnAtender(object sender, EventArgs e)
    {
        if (sender is not Button b || b.BindingContext is not TurnoImagen t)
            return;

        b.IsEnabled = false;

        try
        {
            var resultado = (await Db.Query(
                "CALL AtenderTurnoImagen(@t)",
                r => r.GetName(0) == "mensaje"
                    ? (ok: false, texto: r.GetString(0))
                    : (ok: true, texto:
                        $"Comprobante N° {r.GetInt32(0)}\n" +
                        $"Paciente: {r.GetString(1)}\n" +
                        $"Estudio: {r.GetString(2)}\n" +
                        $"Retirar el informe a partir del: {r.GetDateTime(4):dd/MM/yyyy}"),
                new MySqlParameter("@t", t.idTurno)
            )).FirstOrDefault();

            if (resultado == default)
                return;

            await CargarTodo();

            // Solo muestra el comprobante cuando la atención fue exitosa.
            if (resultado.ok)
            {
                await DisplayAlert(
                    "Atención registrada",
                    resultado.texto,
                    "Aceptar"
                );
            }
        }
        catch (Exception)
        {
            // No mostrar mensajes técnicos en pantalla.
            await CargarTodo();
        }
        finally
        {
            b.IsEnabled = true;
        }
    }

    private async void OnCerrarSesionClicked(object sender, EventArgs e)
    {
        bool confirmar = await Application.Current.Windows[0].Page.DisplayAlert("Cerrar Sesión", "¿Estás seguro de que deseas salir?", "Sí", "Cancelar");
        if (confirmar)
            Application.Current.MainPage = new NavigationPage(new LoginPage());
    }
}

public record TurnoImagen(int idTurno, string paciente, string dni, string estudio, DateTime fechaHora, string indicacion, string estado)
{
    public bool pendiente => estado == "Pendiente";
    public bool llamado => estado == "Llamado";
}

public record EstudioAtendido(int idTurno, string paciente, string dni, string estudio, DateTime fechaAtencion, string indicacion);