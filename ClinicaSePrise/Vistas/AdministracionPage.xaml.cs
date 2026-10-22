using System.Globalization;
using ClinicaSePrise.Datos;
using ClinicaSePrise.Entidades;
using ClinicaSePrise.Graficos;
using MySqlConnector;

namespace ClinicaSePrise.Vistas;
public partial class AdministracionPage : ContentPage
{
    static readonly CultureInfo Ar = new("es-AR");

    readonly GraficoBarras grAtendidos = new() { ColorBarra = Color.FromArgb("#2563EB") };
    readonly GraficoBarras grFacturado = new() { ColorBarra = Color.FromArgb("#16A34A") };
    readonly GraficoBarras grTurnos = new() { ColorBarra = Color.FromArgb("#0284C7") };
    readonly GraficoBarras grAusentes = new() { ColorBarra = Color.FromArgb("#DC2626"), MensajeVacio = "Ningún paciente faltó en el período" };
    readonly GraficoBarras grDisponibles = new() { ColorBarra = Color.FromArgb("#0EA5E9"), MensajeVacio = "No hay turnos libres en los próximos 7 días" };
    readonly GraficoBarras grFaltas = new() { ColorBarra = Color.FromArgb("#D97706"), MensajeVacio = "Ningún médico faltó en el período" };
    readonly GraficoDona grEstados = new();

    List<ProfesionalItem> profesionales = new();
    Dictionary<int, string> nombresCortos = new();
    int anio = DateTime.Today.Year;
    int mes = DateTime.Today.Month;
    bool listo;

    // Lo último que se calculó, para registrar exactamente eso
    (ProfesionalItem Medico, decimal Porcentaje)? calculo;

    string Periodo => $"{Ar.TextInfo.ToTitleCase(Ar.DateTimeFormat.GetMonthName(mes))} {anio}";

    public AdministracionPage()
    {
        InitializeComponent();
        lblBienvenida.Text = $"Bienvenido/a, {Sesion.DisplayNombre}";

        gvAtendidos.Drawable = grAtendidos;
        gvFacturado.Drawable = grFacturado;
        gvTurnos.Drawable = grTurnos;
        gvAusentes.Drawable = grAusentes;
        gvDisponibles.Drawable = grDisponibles;
        gvFaltas.Drawable = grFaltas;
        gvEstados.Drawable = grEstados;

        pkMes.ItemsSource = Enumerable.Range(1, 12)
            .Select(m => Ar.TextInfo.ToTitleCase(Ar.DateTimeFormat.GetMonthName(m))).ToList();
        pkAnio.ItemsSource = Enumerable.Range(anio - 2, 4).Select(a => a.ToString()).ToList();
        pkMes.SelectedIndex = mes - 1;
        pkAnio.SelectedItem = anio.ToString();
        dpFalta.Date = DateTime.Today;
        listo = true;

        // La vista se muestra dentro de MainPage y OnAppearing no se dispara: cargamos acá
        _ = Iniciar();
    }

    async Task Iniciar()
    {
        await CargarPeriodo();
        await CargarDisponibles();   // es lo que más tarda, va al final
    }

    async void OnCambiarPeriodo(object? sender, EventArgs e)
    {
        if (!listo || pkMes.SelectedIndex < 0 || pkAnio.SelectedItem is not string a) return;
        mes = pkMes.SelectedIndex + 1;
        anio = int.Parse(a);
        await CargarPeriodo();
    }

    async Task CargarPeriodo()
    {
        try
        {
            if (profesionales.Count == 0)
            {
                profesionales = await Db.Query("CALL ProfesionalesListado()",
                    r => new ProfesionalItem(r.GetInt32(0), r.GetString(1)));
                pkMedicoHonorario.ItemsSource = profesionales;
                pkMedicoFalta.ItemsSource = profesionales;
            }

            lblPeriodoLiquidacion.Text =
                $"Período {Periodo}. Al médico se le paga un porcentaje de lo cobrado por sus consultas en el mes.";
            bdCalculo.IsVisible = bdComprobanteLiquidacion.IsVisible = false;
            calculo = null;

            await CargarResumen();
            await CargarPorProfesional();
            await CargarEstados();
            await CargarLiquidaciones();
            await CargarFaltas();
            await CargarObrasSociales();
        }
        catch (Exception ex)
        {
            await Avisos.Mostrar("Administración", $"No se pudieron cargar los datos: {ex.Message}");
        }
    }

    MySqlParameter[] ParametrosPeriodo() => new[]
    {
        new MySqlParameter("@a", anio),
        new MySqlParameter("@m", mes)
    };

    static string Plata(decimal monto) => $"$ {monto.ToString("N0", Ar)}";

    static int Entero(System.Data.Common.DbDataReader r, int i) => Convert.ToInt32(r.GetValue(i));

    // ================== TARJETAS ==================

    async Task CargarResumen()
    {
        var res = (await Db.Query("CALL AdminResumen(@a, @m)", r => new
        {
            Facturado = r.GetDecimal(0),
            Atenciones = Entero(r, 1),
            Turnos = Entero(r, 2),
            TurnosAtendidos = Entero(r, 3),
            Ausentes = Entero(r, 4),
            Faltas = Entero(r, 5),
            Insumos = Entero(r, 6)
        }, ParametrosPeriodo())).First();

        lblFacturado.Text = Plata(res.Facturado);
        lblAtendidos.Text = res.Atenciones.ToString();
        lblTurnos.Text = res.Turnos.ToString();
        lblFaltas.Text = res.Faltas.ToString();
        lblInsumos.Text = res.Insumos.ToString();

        // Ausentismo: de los turnos que ya pasaron (atendidos + ausentes), cuántos faltaron
        int pasados = res.TurnosAtendidos + res.Ausentes;
        lblAusentismo.Text = pasados == 0 ? "—" : $"{100.0 * res.Ausentes / pasados:0}%";
        lblAusentismoDetalle.Text = $"{res.Ausentes} de {pasados} turnos ya pasados";
    }

    // ================== GRÁFICOS ==================

    async Task CargarPorProfesional()
    {
        var datos = await Db.Query("CALL AdminPorProfesional(@a, @m)", r => new EstadisticaProfesional
        {
            IdProfesional = r.GetInt32(0),
            Nombre = r.GetString(1),
            Turnos = Entero(r, 2),
            Atendidos = Entero(r, 3),
            Ausentes = Entero(r, 4),
            Facturado = r.GetDecimal(5),
            Faltas = Entero(r, 6)
        }, ParametrosPeriodo());

        nombresCortos = datos.ToDictionary(d => d.IdProfesional, d => d.Nombre);

        Mostrar(gvAtendidos, grAtendidos, Barras(datos, d => d.Atendidos));
        Mostrar(gvFacturado, grFacturado, Barras(datos, d => (double)d.Facturado, d => Plata(d.Facturado)));
        Mostrar(gvTurnos, grTurnos, Barras(datos, d => d.Turnos));
        Mostrar(gvAusentes, grAusentes, Barras(datos, d => d.Ausentes));
        Mostrar(gvFaltas, grFaltas, Barras(datos, d => d.Faltas,
            d => $"{d.Faltas} {(d.Faltas == 1 ? "día" : "días")}"));
    }

    // Solo los médicos con algo para mostrar, de mayor a menor
    static List<DatoGrafico> Barras(List<EstadisticaProfesional> datos, Func<EstadisticaProfesional, double> valor,
                                    Func<EstadisticaProfesional, string>? texto = null) =>
        datos.Where(d => valor(d) > 0)
             .OrderByDescending(valor)
             .Select(d => new DatoGrafico(d.Nombre, valor(d), texto?.Invoke(d)))
             .ToList();

    static void Mostrar(GraphicsView vista, GraficoBarras grafico, List<DatoGrafico> datos)
    {
        grafico.Datos = datos;
        vista.HeightRequest = grafico.AltoNecesario;
        vista.Invalidate();
    }

    async Task CargarEstados()
    {
        grEstados.Datos = await Db.Query("CALL AdminTurnosPorEstado(@a, @m)", r =>
        {
            string estado = r.GetString(0);
            return new DatoGrafico(estado, Entero(r, 1), Color: ColoresEstado.Grafico(estado));
        }, ParametrosPeriodo());
        gvEstados.Invalidate();
    }

    // Turnos libres de cada médico en los próximos 7 días, con el mismo cálculo que ve el paciente
    async Task CargarDisponibles()
    {
        try
        {
            var datos = new List<DatoGrafico>();
            foreach (var p in profesionales)
            {
                int libres = 0;
                for (int dia = 0; dia < 7; dia++)
                {
                    var turnos = await Db.Query("CALL TurnosDisponibles(@p, @f)", r => r.GetString(2),
                        new MySqlParameter("@p", p.idProfesional),
                        new MySqlParameter("@f", DateTime.Today.AddDays(dia)));
                    libres += turnos.Count(t => t == "Normal");
                }
                if (libres > 0)
                    datos.Add(new DatoGrafico(nombresCortos.GetValueOrDefault(p.idProfesional, p.nombre), libres));
            }

            Mostrar(gvDisponibles, grDisponibles, datos.OrderByDescending(d => d.Valor).ToList());
            lblDisponibles.Text = $"Del {DateTime.Today:dd/MM} al {DateTime.Today.AddDays(6):dd/MM}, " +
                                  $"{datos.Sum(d => d.Valor):N0} turnos libres en total.";
        }
        catch (Exception ex)
        {
            lblDisponibles.Text = $"No se pudieron calcular: {ex.Message}";
        }
    }

    // ================== LIQUIDACIÓN DE HONORARIOS ==================

    async Task CargarLiquidaciones()
    {
        var lista = await Db.Query("CALL HonorariosLiquidados(@per)", r => new HonorarioLiquidado
        {
            Profesional = r.GetString(0),
            Pacientes = Entero(r, 1),
            Facturado = r.IsDBNull(2) ? null : r.GetDecimal(2),
            Porcentaje = r.IsDBNull(3) ? null : r.GetDecimal(3),
            Monto = r.GetDecimal(4)
        }, new MySqlParameter("@per", Periodo));

        BindableLayout.SetItemsSource(stLiquidaciones, lista);
        lblSinLiquidaciones.IsVisible = lista.Count == 0;
    }

    // Si cambia el médico o el %, el cálculo anterior ya no vale
    void OnCambioLiquidacion(object? sender, EventArgs e)
    {
        calculo = null;
        bdCalculo.IsVisible = false;
    }

    bool ocupado;

    // Una acción a la vez: un doble clic no la ejecuta dos veces
    async Task Hacer(string titulo, string queSeHace, Func<Task> accion)
    {
        if (ocupado) return;
        ocupado = true;
        try
        {
            await accion();
        }
        catch (Exception ex)
        {
            await Avisos.Mostrar(titulo, $"No se pudo {queSeHace}: {ex.Message}");
        }
        finally
        {
            ocupado = false;
        }
    }

    async void OnCalcularHonorario(object? sender, EventArgs e) =>
        await Hacer("Honorarios", "calcular", CalcularHonorario);

    async Task CalcularHonorario()
    {
        if (pkMedicoHonorario.SelectedItem is not ProfesionalItem medico)
        {
            await Avisos.Mostrar("Honorarios", "Elegí un médico.");
            return;
        }
        if (!Montos.TryLeer(txtPorcentaje.Text, out var porcentaje) || porcentaje <= 0 || porcentaje > 100)
        {
            await Avisos.Mostrar("Honorarios", "El porcentaje tiene que ser un número entre 1 y 100.");
            return;
        }

        var c = (await Db.Query("CALL CalcularHonorario(@p, @a, @m, @pct, @per)", r => new
        {
            Facturado = r.GetDecimal(0),
            Facturas = Entero(r, 1),
            Atenciones = Entero(r, 2),
            Honorario = r.GetDecimal(3),
            YaLiquidado = Entero(r, 4) > 0
        },
            new MySqlParameter("@p", medico.idProfesional),
            new MySqlParameter("@a", anio),
            new MySqlParameter("@m", mes),
            new MySqlParameter("@pct", porcentaje),
            new MySqlParameter("@per", Periodo))).First();

        lblCalculo.Text =
            $"{medico.nombre} · {Periodo}\n" +
            $"Facturado: {Plata(c.Facturado)} ({c.Facturas} {(c.Facturas == 1 ? "consulta cobrada" : "consultas cobradas")})\n" +
            $"Pacientes atendidos: {c.Atenciones}\n" +
            $"Honorario ({porcentaje.ToString("0.##", Ar)}%): {Plata(c.Honorario)}";

        bool sePuede = !c.YaLiquidado && c.Facturado > 0;
        string aviso = c.YaLiquidado ? "Este médico ya tiene liquidado el período."
            : c.Facturado == 0 ? "No tiene consultas cobradas en el período: no hay nada para liquidar."
            : EsMesEnCurso ? "Ojo: el mes todavía no terminó. Lo que se cobre después no se suma a esta liquidación."
            : "";
        lblCalculoAviso.Text = aviso;
        lblCalculoAviso.IsVisible = aviso != "";
        btnRegistrarLiquidacion.IsVisible = sePuede;

        await CargarDetalle(medico);

        calculo = (medico, porcentaje);
        bdCalculo.IsVisible = true;
        bdComprobanteLiquidacion.IsVisible = false;
    }

    // Cada consulta cobrada del médico en el mes, y si ya la atendió en su pantalla
    async Task CargarDetalle(ProfesionalItem medico)
    {
        var consultas = await Db.Query("CALL DetalleHonorario(@p, @a, @m)", r => new ConsultaLiquidada
        {
            Fecha = r.GetDateTime(0),
            Paciente = r.GetString(1),
            Monto = r.GetDecimal(2),
            MedioPago = r.GetString(3),
            EstadoTurno = r.GetString(4)
        },
            new MySqlParameter("@p", medico.idProfesional),
            new MySqlParameter("@a", anio),
            new MySqlParameter("@m", mes));

        int atendidas = consultas.Count(c => c.Atendida);
        lblDetalle.Text = consultas.Count == 0
            ? "No hay consultas cobradas en el período."
            : $"Consultas cobradas: {consultas.Count} · ya atendidas por el médico: {atendidas}";
        BindableLayout.SetItemsSource(stDetalle, consultas);
    }

    bool EsMesEnCurso => anio == DateTime.Today.Year && mes == DateTime.Today.Month;

    async void OnRegistrarLiquidacion(object? sender, EventArgs e) =>
        await Hacer("Honorarios", "registrar la liquidación", RegistrarLiquidacion);

    async Task RegistrarLiquidacion()
    {
        if (calculo is not { } c) return;
        var (medico, porcentaje) = c;
        if (!await Avisos.Confirmar("Registrar liquidación",
                $"¿Registrar la liquidación de {medico.nombre} para {Periodo}? Después no se puede volver a liquidar ese mes."))
            return;

        var res = (await Db.Query("CALL RegistrarLiquidacion(@p, @a, @m, @pct, @per)",
            r => r.FieldCount == 1
                ? (Mensaje: r.GetString(0), Comprobante: "")
                : (Mensaje: "OK", Comprobante:
                    $"Liquidación N° {Entero(r, 1)} · {r.GetDateTime(5):dd/MM/yyyy HH:mm}\n" +
                    $"Médico: {medico.nombre}\n" +
                    $"Período: {Periodo}\n" +
                    $"Facturado por sus consultas: $ {r.GetDecimal(2).ToString("N2", Ar)}\n" +
                    $"Pacientes atendidos: {Entero(r, 3)}\n" +
                    $"Porcentaje: {porcentaje.ToString("0.##", Ar)}%\n" +
                    $"Total a pagar: $ {r.GetDecimal(4).ToString("N2", Ar)}"),
            new MySqlParameter("@p", medico.idProfesional),
            new MySqlParameter("@a", anio),
            new MySqlParameter("@m", mes),
            new MySqlParameter("@pct", porcentaje),
            new MySqlParameter("@per", Periodo))).First();

        if (res.Mensaje != "OK")
        {
            await Avisos.Mostrar("Honorarios", res.Mensaje);
            return;
        }

        lblComprobanteLiquidacion.Text = res.Comprobante;
        bdComprobanteLiquidacion.IsVisible = true;
        bdCalculo.IsVisible = false;
        calculo = null;
        await CargarLiquidaciones();
    }

    // ================== FALTAS DE MÉDICOS ==================

    async Task CargarFaltas()
    {
        var faltas = await Db.Query("CALL AusenciasProfesionales(@a, @m)", r => new AusenciaProfesional
        {
            Fecha = r.GetDateTime(0),
            Profesional = r.GetString(1),
            Motivo = r.GetString(2)
        }, ParametrosPeriodo());

        BindableLayout.SetItemsSource(stFaltas, faltas);
        lblSinFaltas.IsVisible = faltas.Count == 0;
    }

    async void OnRegistrarFalta(object? sender, EventArgs e) =>
        await Hacer("Faltas", "registrar la falta", RegistrarFalta);

    async Task RegistrarFalta()
    {
        if (pkMedicoFalta.SelectedItem is not ProfesionalItem medico)
        {
            await Avisos.Mostrar("Faltas", "Elegí el médico que faltó.");
            return;
        }
        DateTime fecha = dpFalta.Date ?? DateTime.Today;

        var res = (await Db.Query("CALL RegistrarAusenciaProfesional(@p, @f, @mot)",
            r => (Mensaje: r.GetString(0), Turnos: r.FieldCount == 1 ? 0 : Entero(r, 1)),
            new MySqlParameter("@p", medico.idProfesional),
            new MySqlParameter("@f", fecha.Date),
            new MySqlParameter("@mot", txtMotivoFalta.Text?.Trim() ?? ""))).First();

        if (res.Mensaje != "OK")
        {
            await Avisos.Mostrar("Faltas", res.Mensaje);
            return;
        }

        string mensaje = $"Falta de {medico.nombre} registrada para el {fecha:dd/MM/yyyy}.";
        if (res.Turnos > 0)
            mensaje += res.Turnos == 1
                ? "\n\nEse día tiene 1 turno sin atender: avisale a Recepción para reprogramarlo."
                : $"\n\nEse día tiene {res.Turnos} turnos sin atender: avisale a Recepción para reprogramarlos.";
        await Avisos.Mostrar("Faltas", mensaje);

        txtMotivoFalta.Text = "";
        pkMedicoFalta.SelectedIndex = -1;
        await CargarResumen();
        await CargarPorProfesional();
        await CargarFaltas();
    }

    // ================== OBRAS SOCIALES ==================

    async Task CargarObrasSociales()
    {
        var lista = await Db.Query("CALL ObrasSociales()", r => new ObraSocialConvenio
        {
            IdObraSocial = r.GetInt32(0),
            Nombre = r.GetString(1),
            AlDia = r.GetBoolean(2),
            Pacientes = Entero(r, 3)
        });
        BindableLayout.SetItemsSource(stObrasSociales, lista);
    }

    async void OnCambiarEstadoObraSocial(object? sender, EventArgs e)
    {
        if (sender is not Button { BindingContext: ObraSocialConvenio os }) return;
        await Hacer("Obras sociales", "cambiar el estado", async () =>
        {
            string pregunta = os.AlDia
                ? $"¿{os.Nombre} tiene deuda con la clínica? Desde ahora sus pacientes pagan como particulares."
                : $"¿{os.Nombre} regularizó su deuda? Desde ahora se le vuelve a facturar.";
            if (!await Avisos.Confirmar("Obras sociales", pregunta)) return;

            await Db.Query("CALL CambiarEstadoObraSocial(@id, @alDia)", r => 0,
                new MySqlParameter("@id", os.IdObraSocial),
                new MySqlParameter("@alDia", !os.AlDia));
            await CargarObrasSociales();
        });
    }

    async void OnAgregarObraSocial(object? sender, EventArgs e) =>
        await Hacer("Obras sociales", "agregar la obra social", async () =>
        {
            string mensaje = (await Db.Query("CALL AgregarObraSocial(@n)", r => r.GetString(0),
                new MySqlParameter("@n", txtNuevaObraSocial.Text?.Trim() ?? ""))).First();
            if (mensaje != "OK")
            {
                await Avisos.Mostrar("Obras sociales", mensaje);
                return;
            }
            txtNuevaObraSocial.Text = "";
            await CargarObrasSociales();
        });

    private async void OnCerrarSesionClicked(object sender, EventArgs e)
    {
        bool confirmar = await DisplayAlert("Cerrar Sesión", "¿Estás seguro de que deseas salir?", "Sí", "Cancelar");
        if (confirmar)
        {
            Application.Current.MainPage = new NavigationPage(new LoginPage());
        }
    }
}
