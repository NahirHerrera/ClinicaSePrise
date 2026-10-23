using System.Globalization;
using ClinicaSePrise.Datos;
using ClinicaSePrise.Entidades;
using ClinicaSePrise.Graficos;
using MySqlConnector;

namespace ClinicaSePrise.Vistas;

public partial class AdministracionPage : ContentPage
{
    static readonly CultureInfo Ar = new("es-AR");

    // Todo se calcula sobre el mes en curso
    readonly int anio = DateTime.Today.Year;
    readonly int mes = DateTime.Today.Month;
    string Periodo => $"{Ar.TextInfo.ToTitleCase(Ar.DateTimeFormat.GetMonthName(mes))} {anio}";

    readonly GraficoBarras grAtendidos = new() { ColorBarra = Color.FromArgb("#2563EB") };
    readonly GraficoBarras grFacturado = new() { ColorBarra = Color.FromArgb("#16A34A") };
    readonly GraficoBarras grEstados = new() { ColorBarra = Color.FromArgb("#0EA5E9") };

    // Lo último que se calculó, para registrar exactamente eso
    ProfesionalItem? medicoCalculado;
    decimal porcentajeCalculado;
    bool ocupado;

    public AdministracionPage()
    {
        InitializeComponent();
        lblBienvenida.Text = $"Bienvenido/a, {Sesion.DisplayNombre}";
        lblPeriodo.Text = $"Período: {Periodo}";
        gvAtendidos.Drawable = grAtendidos;
        gvFacturado.Drawable = grFacturado;
        gvEstados.Drawable = grEstados;
        dpFalta.Date = DateTime.Today;

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
            await Avisos.Mostrar("Administración", ex.Message);
        }
        finally
        {
            ocupado = false;
        }
    }

    MySqlParameter[] Mes() => new[] { new MySqlParameter("@a", anio), new MySqlParameter("@m", mes) };

    static string Plata(decimal monto) => $"$ {monto.ToString("N0", Ar)}";

    async Task CargarTodo()
    {
        var medicos = await Db.Query("CALL ProfesionalesListado()", r => new ProfesionalItem(r.GetInt32(0), r.GetString(1)));
        pkMedicoHonorario.ItemsSource = medicos;
        pkMedicoFalta.ItemsSource = medicos;

        await CargarLiquidaciones();
        await CargarGraficos();
        await CargarFaltas();
    }

    // ================== GRÁFICOS ==================

    async Task CargarGraficos()
    {
        var porMedico = await Db.Query("CALL AdminPorProfesional(@a, @m)",
            r => (Nombre: r.GetString(0), Atendidos: Convert.ToInt32(r.GetValue(1)), Facturado: r.GetDecimal(2)), Mes());

        Mostrar(gvAtendidos, grAtendidos, porMedico.Where(d => d.Atendidos > 0).OrderByDescending(d => d.Atendidos)
            .Select(d => new DatoGrafico(d.Nombre, d.Atendidos)).ToList());
        Mostrar(gvFacturado, grFacturado, porMedico.Where(d => d.Facturado > 0).OrderByDescending(d => d.Facturado)
            .Select(d => new DatoGrafico(d.Nombre, (double)d.Facturado, Plata(d.Facturado))).ToList());
        Mostrar(gvEstados, grEstados, await Db.Query("CALL AdminTurnosPorEstado(@a, @m)",
            r => new DatoGrafico(r.GetString(0), Convert.ToInt32(r.GetValue(1))), Mes()));
    }

    static void Mostrar(GraphicsView vista, GraficoBarras grafico, List<DatoGrafico> datos)
    {
        grafico.Datos = datos;
        vista.HeightRequest = grafico.AltoNecesario;
        vista.Invalidate();
    }

    // ================== LIQUIDACIÓN DE HONORARIOS ==================

    async Task CargarLiquidaciones()
    {
        BindableLayout.SetItemsSource(stLiquidaciones, await Db.Query("CALL HonorariosLiquidados(@per)", r => new HonorarioLiquidado
        {
            Profesional = r.GetString(0),
            Pacientes = Convert.ToInt32(r.GetValue(1)),
            Facturado = r.GetDecimal(2),
            Monto = r.GetDecimal(3)
        }, new MySqlParameter("@per", Periodo)));
    }

    async void OnCalcularHonorario(object? sender, EventArgs e) => await Hacer(async () =>
    {
        if (pkMedicoHonorario.SelectedItem is not ProfesionalItem medico ||
            !decimal.TryParse(txtPorcentaje.Text?.Replace(',', '.'), NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture, out var porcentaje) || porcentaje <= 0 || porcentaje > 100)
        {
            await Avisos.Mostrar("Honorarios", "Elegí un médico y un porcentaje entre 1 y 100.");
            return;
        }

        var c = (await Db.Query("CALL CalcularHonorario(@p, @a, @m, @pct, @per)",
            r => (Facturado: r.GetDecimal(0), Atenciones: Convert.ToInt32(r.GetValue(1)),
                  Honorario: r.GetDecimal(2), YaLiquidado: Convert.ToInt32(r.GetValue(3)) > 0),
            new MySqlParameter("@p", medico.idProfesional),
            new MySqlParameter("@a", anio),
            new MySqlParameter("@m", mes),
            new MySqlParameter("@pct", porcentaje),
            new MySqlParameter("@per", Periodo))).First();

        // Detalle: cada consulta cobrada y si el médico ya la atendió en su pantalla
        var detalle = await Db.Query("CALL DetalleHonorario(@p, @a, @m)", r => new ConsultaLiquidada
        {
            Fecha = r.GetDateTime(0),
            Paciente = r.GetString(1),
            Monto = r.GetDecimal(2),
            EstadoTurno = r.GetString(3)
        }, new MySqlParameter("@p", medico.idProfesional), new MySqlParameter("@a", anio), new MySqlParameter("@m", mes));

        lblCalculo.Text =
            $"Facturado: {Plata(c.Facturado)} · Pacientes atendidos: {c.Atenciones} · " +
            $"Honorario ({porcentaje}%): {Plata(c.Honorario)}" +
            (c.YaLiquidado ? "\nEste médico ya tiene liquidado el mes." : "");
        BindableLayout.SetItemsSource(stDetalle, detalle);
        btnRegistrarLiquidacion.IsVisible = !c.YaLiquidado && c.Facturado > 0;
        stCalculo.IsVisible = true;
        medicoCalculado = medico;
        porcentajeCalculado = porcentaje;
    });

    async void OnRegistrarLiquidacion(object? sender, EventArgs e) => await Hacer(async () =>
    {
        if (medicoCalculado == null ||
            !await Avisos.Confirmar("Registrar liquidación", $"¿Registrar la liquidación de {medicoCalculado.nombre} ({Periodo})?"))
            return;

        string res = (await Db.Query("CALL RegistrarLiquidacion(@p, @a, @m, @pct, @per)",
            r => r.FieldCount == 1 ? r.GetString(0) : $"Liquidación N° {r.GetValue(1)} registrada.",
            new MySqlParameter("@p", medicoCalculado.idProfesional),
            new MySqlParameter("@a", anio),
            new MySqlParameter("@m", mes),
            new MySqlParameter("@pct", porcentajeCalculado),
            new MySqlParameter("@per", Periodo))).First();

        await Avisos.Mostrar("Honorarios", res);
        stCalculo.IsVisible = false;
        await CargarLiquidaciones();
    });

    // ================== FALTAS DE MÉDICOS ==================

    async Task CargarFaltas()
    {
        BindableLayout.SetItemsSource(stFaltas, await Db.Query("CALL AusenciasProfesionales(@a, @m)", r => new AusenciaProfesional
        {
            Fecha = r.GetDateTime(0),
            Profesional = r.GetString(1),
            Motivo = r.GetString(2)
        }, Mes()));
    }

    async void OnRegistrarFalta(object? sender, EventArgs e) => await Hacer(async () =>
    {
        if (pkMedicoFalta.SelectedItem is not ProfesionalItem medico)
        {
            await Avisos.Mostrar("Faltas", "Elegí el médico que faltó.");
            return;
        }

        string res = (await Db.Query("CALL RegistrarAusenciaProfesional(@p, @f, @mot)", r => r.GetString(0),
            new MySqlParameter("@p", medico.idProfesional),
            new MySqlParameter("@f", (dpFalta.Date ?? DateTime.Today).Date),
            new MySqlParameter("@mot", txtMotivoFalta.Text?.Trim() ?? ""))).First();

        if (res != "OK")
        {
            await Avisos.Mostrar("Faltas", res);
            return;
        }
        txtMotivoFalta.Text = "";
        await CargarFaltas();
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
