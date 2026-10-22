using System.Globalization;
using ClinicaSePrise.Datos;
using ClinicaSePrise.Entidades;
using MySqlConnector;

namespace ClinicaSePrise.Vistas;

public partial class RecepcionPage : ContentPage
{
    static readonly CultureInfo Ar = new("es-AR");

    List<TurnoRecepcion> turnosHoy = new();
    TurnoRecepcion? sel;

    public RecepcionPage()
    {
        InitializeComponent();
        lblBienvenida.Text = $"Bienvenido/a, {Sesion.DisplayNombre}";
        string fecha = DateTime.Now.ToString("dddd d 'de' MMMM 'de' yyyy", Ar);
        lblFecha.Text = char.ToUpper(fecha[0]) + fecha[1..];

        // La vista se muestra dentro de MainPage y OnAppearing no se dispara: cargamos acá
        _ = CargarTodo();
    }

    async Task CargarTodo()
    {
        try
        {
            await CargarTurnosHoy();
            await CargarLlamados();
        }
        catch (Exception ex)
        {
            await Avisos.Mostrar("Recepción", $"No se pudieron cargar los datos: {ex.Message}");
        }
    }

    // ================== TURNOS DE HOY Y SALA DE ESPERA ==================

    async Task CargarTurnosHoy()
    {
        turnosHoy = await Db.Query("CALL RecepcionTurnosHoy()", r => new TurnoRecepcion
        {
            IdTurno = r.GetInt32(0),
            FechaHora = r.GetDateTime(1),
            Estado = r.GetString(2),
            Paciente = r.GetString(3),
            Dni = r.GetString(4),
            Telefono = r.GetString(5),
            Cobertura = r.GetString(6),
            ObraSocial = r.GetString(7),
            Profesional = r.GetString(8),
            Especialidad = r.GetString(9),
            Consultorio = r.GetString(10),
            EstadoCobertura = r.GetString(11)
        });

        Filtrar();

        var enEspera = turnosHoy.Where(t => t.Estado == "En espera").ToList();
        BindableLayout.SetItemsSource(stSalaEspera, enEspera);
        lblSalaVacia.IsVisible = enEspera.Count == 0;

        // Si había un turno elegido, se muestra con su estado actualizado
        sel = sel == null ? null : turnosHoy.FirstOrDefault(t => t.IdTurno == sel.IdTurno);
        MostrarSeleccion();
    }

    void Filtrar()
    {
        string buscado = sbPaciente.Text?.Trim().ToLowerInvariant() ?? "";
        cvTurnosHoy.ItemsSource = buscado == ""
            ? turnosHoy
            : turnosHoy.Where(t => t.Paciente.ToLowerInvariant().Contains(buscado) || t.Dni.Contains(buscado)).ToList();
    }

    void OnBuscar(object? sender, TextChangedEventArgs e) => Filtrar();

    async void OnActualizar(object? sender, EventArgs e) => await CargarTodo();

    void OnTurnoSeleccionado(object? sender, SelectionChangedEventArgs e)
    {
        // Al recargar la lista la selección se borra sola: en ese caso se mantiene el turno elegido
        if (e.CurrentSelection.FirstOrDefault() is not TurnoRecepcion turno) return;
        sel = turno;
        bdComprobante.IsVisible = false;
        txtMonto.Text = "";
        pkMedioPago.SelectedIndex = -1;
        MostrarSeleccion();
    }

    void MostrarSeleccion()
    {
        lblSinSeleccion.IsVisible = sel == null;
        stAcreditar.IsVisible = sel != null;
        if (sel == null) return;

        lblPacienteSel.Text = sel.Paciente;
        lblTurnoSel.Text = $"{sel.DniTxt} · {sel.Hora} con {sel.Profesional} · {sel.Lugar}";
        lblCoberturaSel.Text = $"Cobertura: {sel.CoberturaTxt}";
        lblAvisoCobertura.Text = sel.AvisoCobertura;
        lblAvisoCobertura.TextColor = sel.ColorAvisoCobertura;
        bdAvisoCobertura.BackgroundColor = sel.ColorFondoAvisoCobertura;
        txtMonto.Placeholder = sel.CobraComoParticular ? "Monto de la consulta particular" : "Copago que paga el paciente";

        // Solo se muestra lo que se puede hacer con el turno según su estado
        txtMonto.IsVisible = pkMedioPago.IsVisible = btnAcreditar.IsVisible = sel.PuedeAcreditar;
        btnAusente.IsVisible = sel.PuedeMarcarAusente;
        lblAvisoEstado.IsVisible = !sel.PuedeAcreditar;
        lblAvisoEstado.Text = $"Este turno está {sel.Estado.ToLower()}: ya no se puede acreditar.";
    }

    // ================== ACREDITAR Y COBRAR ==================

    bool ocupado;

    // Una acción a la vez: un doble clic no la ejecuta dos veces
    async Task Hacer(string queSeHace, Func<Task> accion)
    {
        if (ocupado) return;
        ocupado = true;
        try
        {
            await accion();
        }
        catch (Exception ex)
        {
            await Avisos.Mostrar("Recepción", $"No se pudo {queSeHace}: {ex.Message}");
        }
        finally
        {
            ocupado = false;
            // Al ocultarse un botón, Windows pasa el foco al siguiente, que puede ser "Confirmó" de un llamado:
            // un Enter o Espacio confirmaría ese turno sin querer. El foco vuelve siempre al buscador.
            sbPaciente.Focus();
        }
    }

    async void OnAcreditarClicked(object? sender, EventArgs e) => await Hacer("acreditar", Acreditar);

    async Task Acreditar()
    {
        if (sel == null) return;

        if (!Montos.TryLeer(txtMonto.Text, out var monto))
        {
            await Avisos.Mostrar("Recepción", "Ingresá un monto válido. Puede ser 0 si la obra social cubre toda la consulta.");
            return;
        }
        if (pkMedioPago.SelectedItem is not string medioPago)
        {
            await Avisos.Mostrar("Recepción", "Elegí el medio de pago.");
            return;
        }

        // Viene por obra social pero no se le puede facturar: Recepción le avisa que paga como particular
        if (sel.VienePorObraSocial && sel.CobraComoParticular &&
            !await Avisos.Confirmar("Cobra como particular",
                $"{sel.AvisoCobertura}\n\n¿Ya le avisaste a {sel.Paciente} y acepta pagar como particular?"))
            return;

        string paciente = sel.Paciente;
        var res = (await Db.Query("CALL AcreditarTurno(@t, @m, @mp)",
            r => r.FieldCount == 1
                ? (Mensaje: r.GetString(0), Comprobante: "")
                : (Mensaje: "OK", Comprobante:
                    $"N° {r.GetString(1)} · {r.GetDateTime(5):dd/MM/yyyy HH:mm}\n" +
                    $"Paciente: {paciente}\n" +
                    $"Concepto: {r.GetString(2)}\n" +
                    $"Cobertura: {r.GetString(6)}\n" +
                    $"Total: $ {r.GetDecimal(3).ToString("N2", Ar)} ({r.GetString(4)})"),
            new MySqlParameter("@t", sel.IdTurno),
            new MySqlParameter("@m", monto),
            new MySqlParameter("@mp", medioPago))).First();

        if (res.Mensaje != "OK")
        {
            await Avisos.Mostrar("Recepción", res.Mensaje);
            return;
        }

        // Antes de que se oculte el botón de acreditar (ver Hacer)
        sbPaciente.Focus();
        lblComprobante.Text = res.Comprobante + $"\n{paciente} pasó a la sala de espera.";
        bdComprobante.IsVisible = true;
        txtMonto.Text = "";
        pkMedioPago.SelectedIndex = -1;
        await CargarTurnosHoy();
    }

    async void OnMarcarAusente(object? sender, EventArgs e) => await Hacer("marcar ausente", MarcarAusente);

    async Task MarcarAusente()
    {
        if (sel == null) return;
        if (!await Avisos.Confirmar("Marcar ausente", $"¿{sel.Paciente} no vino al turno de las {sel.Hora}?")) return;

        long marcado = (await Db.Query("CALL MarcarAusente(@t)", r => Convert.ToInt64(r.GetValue(0)),
            new MySqlParameter("@t", sel.IdTurno))).First();

        if (marcado == 0)
            await Avisos.Mostrar("Recepción",
                "Solo se puede marcar ausente un turno pendiente o confirmado cuya hora ya pasó.");

        // El ausente pasa a la lista de llamados para contactarlo
        await CargarTodo();
    }

    // ================== LLAMADOS TELEFÓNICOS ==================

    async Task CargarLlamados()
    {
        var llamados = await Db.Query("CALL RecepcionLlamadosPendientes()", r => new LlamadoPendiente
        {
            IdTurno = r.GetInt32(0),
            Motivo = r.GetString(1),
            FechaHora = r.GetDateTime(2),
            Paciente = r.GetString(3),
            Telefono = r.GetString(4),
            Profesional = r.GetString(5),
            Especialidad = r.GetString(6),
            Intentos = Convert.ToInt32(r.GetValue(7)),
            UltimoResultado = r.GetString(8)
        });

        BindableLayout.SetItemsSource(stLlamados, llamados);
        lblSinLlamados.IsVisible = llamados.Count == 0;
    }

    async void OnResultadoLlamado(object? sender, EventArgs e)
    {
        if (sender is not Button { BindingContext: LlamadoPendiente llamado, CommandParameter: string resultado }) return;
        await Hacer("registrar el llamado", () => RegistrarLlamado(llamado, resultado));
    }

    async Task RegistrarLlamado(LlamadoPendiente llamado, string resultado)
    {
        if (resultado == "Canceló" &&
            !await Avisos.Confirmar("Cancelar turno",
                $"¿{llamado.Paciente} cancela el turno del {llamado.FechaTxt}? El horario queda libre para otro paciente."))
            return;

        string mensaje = (await Db.Query("CALL RegistrarLlamado(@t, @r)", r => r.GetString(0),
            new MySqlParameter("@t", llamado.IdTurno),
            new MySqlParameter("@r", resultado))).First();

        if (mensaje != "OK")
            await Avisos.Mostrar("Recepción", mensaje);

        await CargarTodo();
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
