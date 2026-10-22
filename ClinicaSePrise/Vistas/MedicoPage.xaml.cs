using ClinicaSePrise.Datos;
using ClinicaSePrise.Entidades;
using MySqlConnector;
using System.Text.Json;

namespace ClinicaSePrise.Vistas;

public partial class MedicoPage : ContentPage
{
    // TODO: usá la misma cadena de conexión que ya usan tus clases de Datos.
    private const string ConnStr =
        "Server=localhost;Port=3306;Database=ClinicaSePrise;User ID=root;Password=1234;";

    // Sin tocar la DB: el texto de la evolución se guarda en un archivo local de la app.
    private static readonly string RutaNotas =
        Path.Combine(FileSystem.AppDataDirectory, "historia_notas.json");

    // TODO: ajustá si en tu clase Sesion el campo se llama distinto.
    private int IdProfesional => Convert.ToInt32(Sesion.IdProfesional);

    private PacienteHoy? _seleccionado;

    public MedicoPage()
    {
        InitializeComponent();

        // La vista se muestra dentro de MainPage y OnAppearing no se dispara: cargamos acá
        _ = CargarPacientesHoy();
    }

    // ================== PACIENTES DE HOY ==================
    private async Task CargarPacientesHoy()
    {
        var lista = new List<PacienteHoy>();
        try
        {
            await using var conn = new MySqlConnection(ConnStr);
            await conn.OpenAsync();

            const string sql = @"
                SELECT t.idTurno, t.idPaciente,
                       CONCAT(p.nombre, ' ', p.apellido) AS nombre,
                       p.dni, p.nroHistoriaClinica, t.fechaHora, t.estado
                FROM Turno t
                JOIN Paciente p ON p.idPaciente = t.idPaciente
                WHERE t.idProfesional = @prof
                  AND DATE(t.fechaHora) = CURDATE()
                  AND t.estado <> 'Cancelado'
                ORDER BY t.fechaHora";

            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@prof", IdProfesional);
            await using var rd = await cmd.ExecuteReaderAsync();
            while (await rd.ReadAsync())
            {
                lista.Add(new PacienteHoy
                {
                    IdTurno = rd.GetInt32("idTurno"),
                    IdPaciente = rd.GetInt32("idPaciente"),
                    NombrePaciente = rd.GetString("nombre"),
                    Dni = rd.GetString("dni"),
                    NroHistoriaClinica = rd.IsDBNull(rd.GetOrdinal("nroHistoriaClinica"))
                                         ? "" : rd.GetString("nroHistoriaClinica"),
                    Hora = rd.GetDateTime("fechaHora").ToString("HH:mm"),
                    Estado = rd.GetString("estado")
                });
            }
        }
        catch (Exception ex)
        {
            await Avisos.Mostrar("Error", $"No se pudieron cargar los pacientes: {ex.Message}");
        }

        cvPacientesHoy.ItemsSource = lista;
        lblSinPacientes.IsVisible = lista.Count == 0;
    }

    private async void OnPacienteSeleccionado(object sender, SelectionChangedEventArgs e)
    {
        _seleccionado = e.CurrentSelection.FirstOrDefault() as PacienteHoy;
        if (_seleccionado == null) return;

        lblPacienteActual.Text = _seleccionado.NombrePaciente;
        lblDatosPaciente.Text = $"DNI {_seleccionado.Dni}  ·  HC {_seleccionado.NroHistoriaClinica}  ·  Turno {_seleccionado.Hora} ({_seleccionado.Estado})";
        btnFinalizar.IsEnabled = _seleccionado.Estado != "Atendido";

        LimpiarFormulario();
        await CargarHistoria(_seleccionado.IdPaciente);
    }

    // ================== HISTORIA CLÍNICA ==================
    private async Task CargarHistoria(int idPaciente)
    {
        var historia = new List<HistoriaItem>();
        var notas = LeerNotas();

        try
        {
            await using var conn = new MySqlConnection(ConnStr);
            await conn.OpenAsync();

            const string sql = @"
                SELECT a.idAtencion, a.fechaHoraFin, a.indicacionNuevoTurno,
                       CONCAT(pr.nombre, ' ', pr.apellido) AS profesional,
                       pr.especialidad
                FROM Atencion a
                JOIN Profesional pr ON pr.idProfesional = a.idProfesional
                WHERE a.idPaciente = @pac
                ORDER BY a.fechaHoraFin DESC";

            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@pac", idPaciente);
            await using var rd = await cmd.ExecuteReaderAsync();
            while (await rd.ReadAsync())
            {
                int idAtencion = rd.GetInt32("idAtencion");
                bool nuevoTurno = rd.GetBoolean("indicacionNuevoTurno");
                var nota = notas.FirstOrDefault(n => n.IdAtencion == idAtencion && n.IdPaciente == idPaciente);

                historia.Add(new HistoriaItem
                {
                    Titulo = rd.GetDateTime("fechaHoraFin").ToString("dd/MM/yyyy HH:mm"),
                    Detalle = $"{rd.GetString("profesional")} · {(rd.IsDBNull(rd.GetOrdinal("especialidad")) ? "" : rd.GetString("especialidad"))} · " +
                              (nuevoTurno ? "Indicó nuevo turno de control" : "Sin nuevo turno"),
                    Evolucion = string.IsNullOrWhiteSpace(nota?.Evolucion) ? "" : $"Evolución: {nota!.Evolucion}",
                    Indicaciones = string.IsNullOrWhiteSpace(nota?.Indicaciones) ? "" : $"Indicaciones: {nota!.Indicaciones}"
                });
            }
        }
        catch (Exception ex)
        {
            await Avisos.Mostrar("Error", $"No se pudo cargar la historia: {ex.Message}");
        }

        BindableLayout.SetItemsSource(layoutHistoria, historia);
        lblSinHistoria.IsVisible = historia.Count == 0;
    }

    // ================== GUARDAR ATENCIÓN ==================
    private async void OnFinalizarConsultaClicked(object sender, EventArgs e)
    {
        if (_seleccionado == null)
        {
            await Avisos.Mostrar("Atención", "Seleccione un paciente primero.");
            return;
        }
        if (_seleccionado.Estado == "Atendido")
        {
            await Avisos.Mostrar("Atención", "Este paciente ya fue atendido.");
            return;
        }
        if (string.IsNullOrWhiteSpace(edEvolucion.Text))
        {
            await Avisos.Mostrar("Atención", "Cargue la evolución clínica antes de finalizar.");
            return;
        }

        try
        {
            long idAtencion;
            await using (var conn = new MySqlConnection(ConnStr))
            {
                await conn.OpenAsync();
                await using var tx = await conn.BeginTransactionAsync();

                await using (var ins = new MySqlCommand(
                    @"INSERT INTO Atencion (idPaciente, idProfesional, fechaHoraFin, indicacionNuevoTurno)
                      VALUES (@pac, @prof, NOW(), @nuevo)", conn, tx))
                {
                    ins.Parameters.AddWithValue("@pac", _seleccionado.IdPaciente);
                    ins.Parameters.AddWithValue("@prof", IdProfesional);
                    ins.Parameters.AddWithValue("@nuevo", chkNuevoTurno.IsChecked);
                    await ins.ExecuteNonQueryAsync();
                    idAtencion = ins.LastInsertedId;
                }

                await using (var upd = new MySqlCommand(
                    "UPDATE Turno SET estado = 'Atendido' WHERE idTurno = @t", conn, tx))
                {
                    upd.Parameters.AddWithValue("@t", _seleccionado.IdTurno);
                    await upd.ExecuteNonQueryAsync();
                }

                await tx.CommitAsync();
            }

            // Texto clínico: archivo local (la DB actual no tiene columnas para esto)
            var notas = LeerNotas();
            notas.Add(new NotaLocal
            {
                IdAtencion = (int)idAtencion,
                IdPaciente = _seleccionado.IdPaciente,
                Evolucion = edEvolucion.Text?.Trim() ?? "",
                Indicaciones = edIndicaciones.Text?.Trim() ?? ""
            });
            File.WriteAllText(RutaNotas, JsonSerializer.Serialize(notas));

            await Avisos.Mostrar("Listo", "Atención guardada.");

            int idPacienteActual = _seleccionado.IdPaciente;
            await CargarPacientesHoy();
            await CargarHistoria(idPacienteActual);
            LimpiarFormulario();
            btnFinalizar.IsEnabled = false;
            _seleccionado = null;
        }
        catch (Exception ex)
        {
            await Avisos.Mostrar("Error", $"No se pudo guardar la atención: {ex.Message}");
        }
    }

    // ================== AUXILIARES ==================
    private void LimpiarFormulario()
    {
        edEvolucion.Text = string.Empty;
        edIndicaciones.Text = string.Empty;
        chkNuevoTurno.IsChecked = false;
    }

    private static List<NotaLocal> LeerNotas()
    {
        try
        {
            if (!File.Exists(RutaNotas)) return new List<NotaLocal>();
            return JsonSerializer.Deserialize<List<NotaLocal>>(File.ReadAllText(RutaNotas))
                   ?? new List<NotaLocal>();
        }
        catch
        {
            return new List<NotaLocal>();
        }
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

// ================== MODELOS DE LA VISTA ==================
public class PacienteHoy
{
    public int IdTurno { get; set; }
    public int IdPaciente { get; set; }
    public string NombrePaciente { get; set; } = "";
    public string Dni { get; set; } = "";
    public string NroHistoriaClinica { get; set; } = "";
    public string Hora { get; set; } = "";
    public string Estado { get; set; } = "";
}

public class HistoriaItem
{
    public string Titulo { get; set; } = "";
    public string Detalle { get; set; } = "";
    public string Evolucion { get; set; } = "";
    public string Indicaciones { get; set; } = "";
    public bool TieneEvolucion => !string.IsNullOrEmpty(Evolucion);
    public bool TieneIndicaciones => !string.IsNullOrEmpty(Indicaciones);
}

public class NotaLocal
{
    public int IdAtencion { get; set; }
    public int IdPaciente { get; set; }
    public string Evolucion { get; set; } = "";
    public string Indicaciones { get; set; } = "";
}