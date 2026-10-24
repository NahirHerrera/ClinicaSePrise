using ClinicaSePrise.Datos;
using ClinicaSePrise.Entidades;
using MySqlConnector;

namespace ClinicaSePrise.Vistas;

public partial class PacientePage : ContentPage
{
    private bool _esTurnoImagen = false;

    public PacientePage()
    {
        InitializeComponent();

        if (pkTipoAtencion != null)
        {
            pkTipoAtencion.ItemsSource = new List<string>
            {
                "Atención Médica (Consultorio)",
                "Diagnóstico por Imágenes"
            };
            pkTipoAtencion.SelectedIndex = 0;
        }

        dpFecha.MinimumDate = DateTime.Today;
        dpFecha.Date = DateTime.Today;

        if (dpFechaImagen != null)
        {
            dpFechaImagen.MinimumDate = DateTime.Today;
            dpFechaImagen.Date = DateTime.Today;
        }

        _ = CargarTodo();
    }

    async Task CargarTodo()
    {
        await CargarEspecialidades();
        await CargarEstudiosImagen();
        await CargarTurnos();
        await CargarFacturas();
        await CargarEstudios();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CargarTodo();
    }

    void OnCerrarSesionClicked(object sender, EventArgs e) => Application.Current!.Windows[0].Page = new LoginPage();

    private void OnTipoAtencionChanged(object sender, EventArgs e)
    {
        _esTurnoImagen = pkTipoAtencion?.SelectedIndex == 1;

        if (layoutMedico != null) layoutMedico.IsVisible = !_esTurnoImagen;
        if (layoutImagen != null) layoutImagen.IsVisible = _esTurnoImagen;

        BindableLayout.SetItemsSource(ListaSlots, null);
        if (SinSlots != null) SinSlots.IsVisible = false;
    }

    void OnPracticaImagenChanged(object sender, EventArgs e)
    {
        if (pkPracticaImagen.SelectedItem is not Estudio estudio) return;
        lblIndicacion.Text = $"Indicación: {estudio.requisitosPrevios}";
        lblIndicacion.IsVisible = true;
        BuscarSlotsImagen(this, EventArgs.Empty);
    }

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
            if (SinTurnos != null) SinTurnos.IsVisible = turnos.Count == 0;
        }
        catch (Exception ex) { await Application.Current!.Windows[0].Page!.DisplayAlertAsync("Error", ex.Message, "OK"); }
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
            if (SinFacturas != null) SinFacturas.IsVisible = facturas.Count == 0;
        }
        catch (Exception ex) { await Application.Current!.Windows[0].Page!.DisplayAlertAsync("Error", ex.Message, "OK"); }
    }

    async Task CargarEstudios()
    {
        try
        {
            var estudios = await Db.Query("CALL MisComprobantesEstudio(@id)",
                r => new ComprobanteEstudio(r.GetDateTime(0), r.GetString(1), Convert.ToBoolean(r[2])),
                new MySqlParameter("@id", Sesion.IdPaciente));

            BindableLayout.SetItemsSource(ListaEstudios, estudios);
            if (SinEstudios != null) SinEstudios.IsVisible = estudios.Count == 0;
        }
        catch (Exception ex) { await Application.Current!.Windows[0].Page!.DisplayAlertAsync("Error", ex.Message, "OK"); }
    }

    async Task CargarEspecialidades()
    {
        try
        {
            if (pkEspecialidad != null)
            {
                pkEspecialidad.ItemsSource = await Db.Query("CALL ListarEspecialidades()", r => r.GetString(0));
            }
        }
        catch (Exception ex) { await Application.Current!.Windows[0].Page!.DisplayAlertAsync("Error", ex.Message, "OK"); }
    }

    async Task CargarEstudiosImagen()
    {
        try
        {
            if (pkPracticaImagen != null)
            {
                var lista = await Db.Query("CALL ListarPracticasImagen()",
                    r => new Estudio(
                        r.GetInt32(0),
                        r.GetString(1),
                        "Diagnóstico por imágenes",
                        r.IsDBNull(2) ? "" : r.GetString(2),
                        "",
                        r.IsDBNull(4) ? "" : r.GetString(4)
                    ));
                pkPracticaImagen.ItemsSource = lista;
            }
        }
        catch (Exception ex) { await Application.Current!.Windows[0].Page!.DisplayAlertAsync("Error", ex.Message, "OK"); }
    }

    async void OnEspecialidad(object sender, EventArgs e)
    {
        if (pkEspecialidad.SelectedItem is not string esp) return;
        BindableLayout.SetItemsSource(ListaSlots, null);
        if (SinSlots != null) SinSlots.IsVisible = false;

        try
        {
            if (pkProfesional != null)
            {
                pkProfesional.ItemsSource = await Db.Query("CALL ProfesionalesPorEspecialidad(@e)",
                    r => new ProfesionalItem(r.GetInt32(0), r["nombre"].ToString()),
                    new MySqlParameter("@e", esp));
            }
        }
        catch (Exception ex) { await Application.Current!.Windows[0].Page!.DisplayAlertAsync("Error", ex.Message, "OK"); }
    }

    async void BuscarSlots(object sender, EventArgs e)
    {
        if (_esTurnoImagen) return;
        if (pkProfesional.SelectedItem is not ProfesionalItem p) return;
        try
        {
            var slots = await Db.Query("CALL TurnosDisponibles(@p, @f)",
                r => new Slot(r.GetDateTime(0), Convert.ToInt32(r[1]), r.GetString(2)),
                new MySqlParameter("@p", p.idProfesional), new MySqlParameter("@f", dpFecha.Date));

            BindableLayout.SetItemsSource(ListaSlots, slots);
            if (SinSlots != null) SinSlots.IsVisible = slots.Count == 0;
        }
        catch (Exception ex) { await Application.Current!.Windows[0].Page!.DisplayAlertAsync("Error", ex.Message, "OK"); }
    }

    async void BuscarSlotsImagen(object sender, EventArgs e)
    {
        if (!_esTurnoImagen) return;
        if (pkPracticaImagen?.SelectedItem is not Estudio estudio) return;

        try
        {
            var slots = await Db.Query("CALL TurnosDisponiblesImagen(@e, @f)",
                r => new Slot(r.GetDateTime(0), Convert.ToInt32(r[1]), r.GetString(2)),
                new MySqlParameter("@e", estudio.idEstudio), new MySqlParameter("@f", dpFechaImagen.Date));

            BindableLayout.SetItemsSource(ListaSlots, slots);
            if (SinSlots != null) SinSlots.IsVisible = slots.Count == 0;
        }
        catch (Exception ex) { await Application.Current!.Windows[0].Page!.DisplayAlertAsync("Error", ex.Message, "OK"); }
    }

    async void OnSlotClicked(object sender, EventArgs e)
    {
        if (sender is not Button btn || btn.BindingContext is not Slot s) return;

        if (_esTurnoImagen)
        {
            if (pkPracticaImagen?.SelectedItem is not Estudio estudio) return;
            if (!await Application.Current!.Windows[0].Page!.DisplayAlertAsync("Confirmar Estudio", $"Estudio: {estudio.nombre}\nFecha: {s.fechaHora:dd/MM/yyyy HH:mm}\nIndicación: {estudio.requisitosPrevios}", "Reservar", "Volver")) return;

            try
            {
                var error = await Db.Query("CALL AgendarTurnoImagen(@a,@b,@d)", r => r.GetString(0),
                    new MySqlParameter("@a", Sesion.IdPaciente),
                    new MySqlParameter("@b", estudio.idEstudio),
                    new MySqlParameter("@d", s.fechaHora));

                await Application.Current!.Windows[0].Page!.DisplayAlertAsync(error.Count == 0 ? "Listo" : "No se pudo", error.Count == 0 ? "Estudio reservado con éxito." : error[0], "OK");
                await CargarTurnos();
                BuscarSlotsImagen(this, EventArgs.Empty);
            }
            catch (Exception ex) { await Application.Current!.Windows[0].Page!.DisplayAlertAsync("Error", ex.Message, "OK"); }
        }
        else
        {
            if (pkProfesional?.SelectedItem is not ProfesionalItem p || pkEspecialidad?.SelectedItem is not string esp) return;
            var sobre = s.tipo == "Sobreturno";
            if (!await Application.Current!.Windows[0].Page!.DisplayAlertAsync(sobre ? "Confirmar sobreturno" : "Confirmar turno", $"{esp} · {s.fechaHora:dd/MM/yyyy HH:mm}", "Reservar", "Volver")) return;

            try
            {
                var error = await Db.Query($"CALL {(sobre ? "AgendarSobreturno" : "AgendarTurno")}(@a,@b,@c,@d,@f)", r => r.GetString(0),
                    new MySqlParameter("@a", Sesion.IdPaciente), new MySqlParameter("@b", p.idProfesional),
                    new MySqlParameter("@c", esp), new MySqlParameter("@d", s.fechaHora), new MySqlParameter("@f", s.duracion));

                await Application.Current!.Windows[0].Page!.DisplayAlertAsync(error.Count == 0 ? "Listo" : "No se pudo", error.Count == 0 ? (sobre ? "Sobreturno reservado." : "Turno reservado.") : error[0], "OK");
                await CargarTurnos();
                BuscarSlots(this, EventArgs.Empty);
            }
            catch (Exception ex) { await Application.Current!.Windows[0].Page!.DisplayAlertAsync("Error", ex.Message, "OK"); }
        }
    }

    async void Cancelar_Clicked(object sender, EventArgs e)
    {
        if (sender is not Button btn || btn.BindingContext is not Turno t) return;
        if (!await Application.Current!.Windows[0].Page!.DisplayAlertAsync("Cancelar turno", $"¿Cancelar el turno de {t.especialidad}?", "Sí", "No")) return;

        var proc = t.tipoTurno == "Estudio" ? "CancelarTurnoImagen" : "CancelarTurno";
        var ok = (await Db.Query($"CALL {proc}(@t,@p)", r => Convert.ToInt32(r[0]),
            new MySqlParameter("@t", t.idTurno), new MySqlParameter("@p", Sesion.IdPaciente))).First() == 1;

        await Application.Current!.Windows[0].Page!.DisplayAlertAsync(ok ? "Listo" : "No se pudo", ok ? "Turno cancelado." : "Ese turno ya no se puede cancelar.", "OK");
        await CargarTurnos();
        if (_esTurnoImagen)
            BuscarSlotsImagen(this, EventArgs.Empty);
        else
            BuscarSlots(this, EventArgs.Empty);
    }
}