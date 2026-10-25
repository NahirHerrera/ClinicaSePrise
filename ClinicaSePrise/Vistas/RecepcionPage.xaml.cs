using System.Data.Common;
using System.Globalization;
using ClinicaSePrise.Datos;
using ClinicaSePrise.Entidades;
using MySqlConnector;

namespace ClinicaSePrise.Vistas;

public partial class RecepcionPage : ContentPage
{
    List<TurnoRecepcion> turnosHoy = new();
    TurnoRecepcion? sel;
    bool ocupado;

    public RecepcionPage()
    {
        InitializeComponent();
        lblBienvenida.Text = $"Bienvenido/a, {Sesion.DisplayNombre}";

        // La vista se muestra dentro de MainPage y OnAppearing no se dispara: cargamos acá
        _ = Hacer(CargarTodo);
    }

    // Una acción a la vez (un doble clic no la repite) y con los errores en un aviso
    async Task Hacer(Func<Task> accion)
    {
        if (ocupado) return;
        ocupado = true;
        try
        {
            await accion();
        }
        catch (Exception ex)
        {
            await Avisos.Mostrar("Recepción", ex.Message);
        }
        finally
        {
            ocupado = false;
            // Si un botón se oculta, el foco pasaría al siguiente ("Confirmó" de un llamado)
            // y un Enter lo confirmaría sin querer: el foco vuelve siempre al buscador
            sbPaciente.Focus();
        }
    }

    // Mismo mapeo para consultorios e imágenes: el procedimiento de imágenes trae una 11ª columna (esImagen)
    static TurnoRecepcion Leer(DbDataReader r) => new()
    {
        IdTurno = r.GetInt32(0),
        FechaHora = r.GetDateTime(1),
        Estado = r.GetString(2),
        Paciente = r.GetString(3),
        Dni = r.GetString(4),
        Cobertura = r.GetString(5),
        ObraSocial = r.GetString(6),
        Profesional = r.GetString(7),
        Especialidad = r.GetString(8),
        Consultorio = r.GetString(9),
        EsImagen = r.FieldCount > 10
    };

    async Task CargarTodo()
    {
        var consultorios = await Db.Query("CALL RecepcionTurnosHoy()", Leer);
        var imagenes = await Db.Query("CALL RecepcionImagenesHoy()", Leer);
        turnosHoy = consultorios.Concat(imagenes).OrderBy(t => t.FechaHora).ToList();
        Filtrar();
        BindableLayout.SetItemsSource(stSalaEspera, turnosHoy.Where(t => t.Estado == "En espera").ToList());

        BindableLayout.SetItemsSource(stLlamados, await Db.Query("CALL RecepcionLlamadosPendientes()", r => new LlamadoPendiente
        {
            IdTurno = r.GetInt32(0),
            FechaHora = r.GetDateTime(1),
            Paciente = r.GetString(2),
            Telefono = r.GetString(3),
            Profesional = r.GetString(4)
        }));

        // Si había un turno elegido, se muestra con su estado actualizado (los ids de consultorio e imagen pueden coincidir)
        sel = turnosHoy.FirstOrDefault(t => t.IdTurno == sel?.IdTurno && t.EsImagen == sel?.EsImagen);
        MostrarSeleccion();
    }

    void Filtrar()
    {
        string buscado = sbPaciente.Text?.Trim().ToLower() ?? "";
        cvTurnosHoy.ItemsSource = turnosHoy.Where(t => t.Paciente.ToLower().Contains(buscado) || t.Dni.Contains(buscado)).ToList();
    }

    void OnBuscar(object? sender, TextChangedEventArgs e) => Filtrar();

    async void OnActualizar(object? sender, EventArgs e) => await Hacer(CargarTodo);

    void OnTurnoSeleccionado(object? sender, SelectionChangedEventArgs e)
    {
        // Al recargar la lista la selección se borra sola: en ese caso se mantiene el turno elegido
        if (e.CurrentSelection.FirstOrDefault() is not TurnoRecepcion turno) return;
        sel = turno;
        lblComprobante.IsVisible = false;
        txtMonto.Text = "";
        pkMedioPago.SelectedIndex = -1;
        chkObraSocialAlDia.IsChecked = false;
        MostrarSeleccion();
    }

    void MostrarSeleccion()
    {
        lblSinSeleccion.IsVisible = sel == null;
        stAcreditar.IsVisible = sel != null;
        if (sel == null) return;

        lblPacienteSel.Text = sel.Paciente;
        lblTurnoSel.Text = $"DNI {sel.Dni} · {sel.Hora} con {sel.Profesional} · {sel.Lugar}";
        lblCoberturaSel.Text = sel.VienePorObraSocial ? $"Cobertura: Obra Social – {sel.ObraSocial}" : "Cobertura: Particular";

        // Solo se muestra lo que se puede hacer con el turno según su estado
        stObraSocial.IsVisible = sel.VienePorObraSocial && sel.PuedeAcreditar;
        txtMonto.IsVisible = btnAcreditar.IsVisible = sel.PuedeAcreditar;
        btnAcreditar.Text = sel.EsImagen ? "Acreditar y pasar a Imágenes" : "Acreditar y pasar a sala";

        // Obra social al día: el paciente no abona nada, la consulta se le factura a la obra social
        pkMedioPago.IsVisible = sel.PuedeAcreditar && !PagaObraSocial;
        txtMonto.Placeholder = PagaObraSocial ? "Valor de la consulta (lo paga la obra social)" : "Monto a cobrar (sin puntos de miles)";
        btnAusente.IsVisible = sel.PuedeMarcarAusente;
        lblAvisoEstado.Text = sel.PuedeAcreditar ? "" : $"Este turno está {sel.Estado.ToLower()}: ya no se puede acreditar.";
    }

    bool PagaObraSocial => sel != null && sel.VienePorObraSocial && chkObraSocialAlDia.IsChecked;

    void OnObraSocialCambiada(object? sender, CheckedChangedEventArgs e) => MostrarSeleccion();

    async void OnAcreditarClicked(object? sender, EventArgs e) => await Hacer(Acreditar);

    async Task Acreditar()
    {
        if (sel == null) return;
        string? medioPago = PagaObraSocial ? "Obra Social" : pkMedioPago.SelectedItem as string;
        if (!decimal.TryParse(txtMonto.Text?.Replace(',', '.'), NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture, out var monto) || medioPago == null)
        {
            await Avisos.Mostrar("Recepción", "Ingresá el monto y el medio de pago.");
            return;
        }

        string paciente = sel.Paciente;
        bool imagen = sel.EsImagen;
        var res = (await Db.Query($"CALL {(imagen ? "AcreditarTurnoImagen" : "AcreditarTurno")}(@t, @m, @mp, @os)",
            r => r.FieldCount == 1 ? r.GetString(0) : $"Factura N° {r.GetString(1)} · {r.GetString(2)} · {r.GetString(3)}",
            new MySqlParameter("@t", sel.IdTurno),
            new MySqlParameter("@m", monto),
            new MySqlParameter("@mp", medioPago),
            new MySqlParameter("@os", chkObraSocialAlDia.IsChecked))).First();

        if (res.StartsWith("Error"))
        {
            await Avisos.Mostrar("Recepción", res);
            return;
        }

        string total = $"$ {monto.ToString("N2", new CultureInfo("es-AR"))}";
        lblComprobante.Text = $"{res}\n" +
            (medioPago == "Obra Social" ? $"El paciente no abona: {total} a cargo de la obra social. " : $"Total: {total} ({medioPago}). ") +
            $"{paciente} " + (imagen ? "quedó pendiente en Imágenes." : "pasó a la sala de espera.");
        lblComprobante.IsVisible = true;
        await CargarTodo();
    }

    async void OnMarcarAusente(object? sender, EventArgs e) => await Hacer(async () =>
    {
        if (sel == null || !await Avisos.Confirmar("Marcar ausente", $"¿{sel.Paciente} no vino al turno de las {sel.Hora}?"))
            return;
        await Db.Query("CALL MarcarAusente(@t)", r => 0, new MySqlParameter("@t", sel.IdTurno));
        await CargarTodo();
    });

    async void OnResultadoLlamado(object? sender, EventArgs e)
    {
        if (sender is not Button { BindingContext: LlamadoPendiente llamado, CommandParameter: string resultado }) return;
        await Hacer(async () =>
        {
            bool confirma = resultado == "Confirmó";
            if (!confirma && !await Avisos.Confirmar("Cancelar turno", $"¿{llamado.Paciente} cancela el turno del {llamado.FechaTxt}?"))
                return;
            await Db.Query("CALL ResultadoLlamado(@t, @c)", r => 0,
                new MySqlParameter("@t", llamado.IdTurno),
                new MySqlParameter("@c", confirma));
            await CargarTodo();
        });
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