using ClinicaSePrise.Datos;
using ClinicaSePrise.Entidades;
using MySqlConnector;

namespace ClinicaSePrise.Vistas;

public partial class PacientePage : ContentPage
{
    public PacientePage()
    {
        InitializeComponent();
    }

    void OnCerrarSesionClicked(object sender, EventArgs e) => Window.Page = new LoginPage();

    async Task CargarTurnos()
    {
        try
        {
            var turnos = await Db.Query("CALL MisTurnos(@id)",
                r => new Turno(r.GetInt32(0), r.GetInt32(1), r.GetInt32(2), r.GetString(3), r.GetDateTime(4),
                               r.GetString(5), r.GetInt32(6), r.GetString(7), r.GetString(8))
                { nombreProfesional = r.GetString(9) },
                new MySqlParameter("@id", Sesion.IdPaciente));

            BindableLayout.SetItemsSource(ListaTurnos, turnos);
            SinTurnos.IsVisible = turnos.Count == 0;
        }
        catch (Exception ex) { await Window.Page!.DisplayAlertAsync("Error", ex.Message, "OK"); }
    }

    async Task CargarFacturas()
    {
        try
        {
            var facturas = await Db.Query("CALL MisFacturas(@id)",
                r => new Factura(r["nroComprobante"].ToString(), r.GetDateTime(1), r["concepto"].ToString(),
                                 r.GetDecimal(3), r["medioPago"].ToString(), r.GetString(5)),
                new MySqlParameter("@id", Sesion.IdPaciente));

            BindableLayout.SetItemsSource(ListaFacturas, facturas);
            SinFacturas.IsVisible = facturas.Count == 0;
        }
        catch (Exception ex) { await Window.Page!.DisplayAlertAsync("Error", ex.Message, "OK"); }
    }

    async Task CargarEstudios()
    {
        try
        {
            var estudios = await Db.Query("CALL MisComprobantesEstudio(@id)",
                r => new ComprobanteEstudio(r.GetDateTime(0), r.GetString(1), Convert.ToBoolean(r[2])),
                new MySqlParameter("@id", Sesion.IdPaciente));

            BindableLayout.SetItemsSource(ListaEstudios, estudios);
            SinEstudios.IsVisible = estudios.Count == 0;
        }
        catch (Exception ex) { await Window.Page!.DisplayAlertAsync("Error", ex.Message, "OK"); }
    }

    async Task CargarEspecialidades()
    {
        try { pkEspecialidad.ItemsSource = await Db.Query("CALL ListarEspecialidades()", r => r.GetString(0)); }
        catch (Exception ex) { await Window.Page!.DisplayAlertAsync("Error", ex.Message, "OK"); }
    }

    async void OnEspecialidad(object sender, EventArgs e)
    {
        if (pkEspecialidad.SelectedItem is not string esp) return;
        BindableLayout.SetItemsSource(ListaSlots, null);
        SinSlots.IsVisible = false;

        try
        {
            pkProfesional.ItemsSource = await Db.Query("CALL ProfesionalesPorEspecialidad(@e)",
                r => new ProfesionalItem(r.GetInt32(0), r["nombre"].ToString()),
                new MySqlParameter("@e", esp));
        }
        catch (Exception ex) { await Window.Page!.DisplayAlertAsync("Error", ex.Message, "OK"); }
    }

    async void BuscarSlots(object sender, EventArgs e)
    {
        if (pkProfesional.SelectedItem is not ProfesionalItem p) return;
        try
        {
            var slots = await Db.Query("CALL TurnosDisponibles(@p, @f)",
                r => new Slot(r.GetDateTime(0), Convert.ToInt32(r[1]), r.GetString(2)),
                new MySqlParameter("@p", p.idProfesional), new MySqlParameter("@f", dpFecha.Date));

            BindableLayout.SetItemsSource(ListaSlots, slots);
            SinSlots.IsVisible = slots.Count == 0;
        }
        catch (Exception ex) { await Window.Page!.DisplayAlertAsync("Error", ex.Message, "OK"); }
    }

    async void OnSlotClicked(object sender, EventArgs e)
    {
        var s = (Slot)((Button)sender).BindingContext;
        if (pkProfesional.SelectedItem is not ProfesionalItem p || pkEspecialidad.SelectedItem is not string esp) return;
        var sobre = s.tipo == "Sobreturno";
        if (!await Window.Page!.DisplayAlertAsync(sobre ? "Confirmar sobreturno" : "Confirmar turno", $"{esp} · {s.fechaHora:dd/MM/yyyy HH:mm}", "Reservar", "Volver")) return;

        try
        {
            var error = await Db.Query($"CALL {(sobre ? "AgendarSobreturno" : "AgendarTurno")}(@a,@b,@c,@d,@f)", r => r.GetString(0),
                new MySqlParameter("@a", Sesion.IdPaciente), new MySqlParameter("@b", p.idProfesional),
                new MySqlParameter("@c", esp), new MySqlParameter("@d", s.fechaHora), new MySqlParameter("@f", s.duracion));

            await Window.Page!.DisplayAlertAsync(error.Count == 0 ? "Listo" : "No se pudo", error.Count == 0 ? (sobre ? "Sobreturno reservado." : "Turno reservado.") : error[0], "OK");
            await CargarTurnos();
            BuscarSlots(this, EventArgs.Empty);
        }
        catch (Exception ex) { await Window.Page!.DisplayAlertAsync("Error", ex.Message, "OK"); }
    }
    async void Cancelar_Clicked(object sender, EventArgs e)
    {
        var t = (Turno)((Button)sender).BindingContext;
        if (!await Window.Page!.DisplayAlertAsync("Cancelar turno", $"¿Cancelar el turno de {t.especialidad}?", "Sí", "No")) return;

        var ok = (await Db.Query("CALL CancelarTurno(@t,@p)", r => Convert.ToInt32(r[0]),
            new MySqlParameter("@t", t.idTurno), new MySqlParameter("@p", Sesion.IdPaciente))).First() == 1;

        await Window.Page!.DisplayAlertAsync(ok ? "Listo" : "No se pudo", ok ? "Turno cancelado." : "Ese turno ya no se puede cancelar.", "OK");
        await CargarTurnos();
        BuscarSlots(this, EventArgs.Empty);
    }
}