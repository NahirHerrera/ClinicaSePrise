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

    async Task CargarTodo()
    {
        await CargarPendientes();
        await CargarRealizadosHoy();
        await stock.Cargar();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CargarTodo();
    }

    async Task CargarPendientes()
    {
        try
        {
            var lista = await Db.Query("CALL TurnosImagenPendientes()", r => new TurnoImagen
            {
                idTurno = r.GetInt32(0),
                paciente = r.GetString(1),
                dni = r["dni"].ToString()!,
                estudio = r.GetString(3),
                sector = r.GetString(4),
                fechaHora = r.GetDateTime(5),
                indicaciones = r.GetString(6),
                estado = r.GetString(7)
            });

            BindableLayout.SetItemsSource(ListaPendientes, lista);
            SinPendientes.IsVisible = lista.Count == 0;
        }
        catch (Exception ex) { await Application.Current!.Windows[0].Page!.DisplayAlertAsync("Error", ex.Message, "OK"); }
    }

    async Task CargarRealizadosHoy()
    {
        try
        {
            var lista = await Db.Query("CALL TurnosImagenRealizadosHoy()", r => new TurnoImagen
            {
                idTurno = r.GetInt32(0),
                paciente = r.GetString(1),
                dni = r["dni"].ToString()!,
                estudio = r.GetString(3),
                sector = r.GetString(4),
                fechaHora = r.GetDateTime(5),
                indicaciones = r.GetString(6),
                estado = r.GetString(7)
            });

            cvEstudiosHoy.ItemsSource = lista;
        }
        catch (Exception ex) { await Application.Current!.Windows[0].Page!.DisplayAlertAsync("Error", ex.Message, "OK"); }
    }

    async void Llamar_Clicked(object sender, EventArgs e)
    {
        if (sender is not Button btn || btn.BindingContext is not TurnoImagen t) return;

        try
        {
            var ok = (await Db.Query("CALL LlamarTurnoEstudio(@t)", r => Convert.ToInt32(r[0]),
                new MySqlParameter("@t", t.idTurno))).First() == 1;

            await Application.Current!.Windows[0].Page!.DisplayAlertAsync(ok ? "Llamado" : "No se pudo", ok ? $"Se llamó a {t.paciente}." : "Ese turno ya no se puede llamar.", "OK");
            await CargarPendientes();
        }
        catch (Exception ex) { await Application.Current!.Windows[0].Page!.DisplayAlertAsync("Error", ex.Message, "OK"); }
    }

    async void Atender_Clicked(object sender, EventArgs e)
    {
        if (sender is not Button btn || btn.BindingContext is not TurnoImagen t) return;
        if (!await Application.Current!.Windows[0].Page!.DisplayAlertAsync("Atender estudio", $"{t.estudio} · {t.paciente}", "Atender", "Volver")) return;

        try
        {
            var msg = (await Db.Query("CALL AtenderTurnoEstudio(@t)",
                r => r.FieldCount == 1 ? r.GetString(0) : $"Comprobante de retiro N° {r.GetInt32(0)}\nRetiro a partir del {r.GetDateTime(2):dd/MM/yyyy}",
                new MySqlParameter("@t", t.idTurno))).First();

            await Application.Current!.Windows[0].Page!.DisplayAlertAsync(msg.StartsWith("Error") ? "No se pudo" : "Estudio atendido", msg, "OK");

            await CargarPendientes();
            await CargarRealizadosHoy(); // Refresca instantáneamente la lista inferior
            await stock.Cargar();
        }
        catch (Exception ex) { await Application.Current!.Windows[0].Page!.DisplayAlertAsync("Error", ex.Message, "OK"); }
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