using System.Globalization;
using ClinicaSePrise.Datos;
using ClinicaSePrise.Entidades;
using MySqlConnector;

namespace ClinicaSePrise.Vistas;

public partial class LaboratorioPage : ContentPage
{
    const decimal PrecioDeterminacion = 10000m;
    static readonly CultureInfo AR = new("es-AR");

    OrdenLab sel;
    int idPac;

    static Task Aviso(string titulo, string msg) => Application.Current!.Windows[0].Page!.DisplayAlertAsync(titulo, msg, "OK");

    bool EsObraSocial => pkModalidadCobertura.SelectedItem?.ToString() == "Obra Social";
    decimal Monto => EsObraSocial ? 0 : cvPracticas.SelectedItems.Count * PrecioDeterminacion;

    public LaboratorioPage()
    {
        InitializeComponent();
        _ = Iniciar();
    }

    async Task Iniciar()
    {
        try
        {
            await CargarPracticas();
            await Cargar();
        }
        catch (Exception ex) { await Aviso("Error", ex.Message); }
    }

    void Recalcular()
    {
        pkObraSocial.IsVisible = EsObraSocial;
        pkMedio.IsVisible = !EsObraSocial;
        entMonto.Text = Monto.ToString("N0", AR);
    }

    void OnCoberturaChanged(object sender, EventArgs e) => Recalcular();

    void OnPracticasChanged(object sender, SelectionChangedEventArgs e) => Recalcular();

    void OnFechaNacTextChanged(object sender, TextChangedEventArgs e)
    {
        var d = new string((e.NewTextValue ?? "").Where(char.IsDigit).Take(8).ToArray());
        var t = d.Length > 4 ? $"{d[..2]}/{d[2..4]}/{d[4..]}" : d.Length > 2 ? $"{d[..2]}/{d[2..]}" : d;
        if (t != e.NewTextValue) entFechaNac.Text = t;
    }

    async Task Cargar()
    {
        cvEspera.ItemsSource = await Db.Query("CALL ListaEsperaLab()", r => new OrdenLab
        {
            IdOrden = r.GetInt32(0),
            Paciente = r.GetString(1),
            Dni = r.GetString(2),
            Estado = r.GetString(3),
            Practicas = r.GetString(4)
        });
        sel = null;
        btnLlamar.IsEnabled = btnAtender.IsEnabled = false;
    }

    async Task CargarPracticas()
    {
        cvPracticas.ItemsSource = await Db.Query("CALL ListarPracticasLab()", r => new PracticaLab
        {
            IdEstudio = r.GetInt32(0),
            Nombre = r.GetString(1),
            RequiereAyuno = r.GetBoolean(2)
        });
    }

    void OnSeleccion(object sender, SelectionChangedEventArgs e)
    {
        sel = e.CurrentSelection.FirstOrDefault() as OrdenLab;
        if (sel == null) return;
        lblPaciente.Text = $"{sel.Paciente} (DNI {sel.Dni})";
        lblPracticas.Text = sel.Practicas;
        btnLlamar.IsEnabled = true;
        btnAtender.IsEnabled = sel.Estado == "Llamada";
        bdComprobante.IsVisible = false;
    }

    async void OnLlamar(object sender, EventArgs e)
    {
        try
        {
            await Db.Query("CALL LlamarPacienteLab(@id)", r => 0, new MySqlParameter("@id", sel.IdOrden));
            await Aviso("Llamado", $"Llamando a {sel.Paciente}");
            await Cargar();
        }
        catch (Exception ex) { await Aviso("Error", ex.Message); }
    }

    async void OnAtender(object sender, EventArgs e)
    {
        try
        {
            string res = (await Db.Query("CALL AtenderOrdenLab(@id)",
                r => r.FieldCount == 1
                    ? r.GetString(0)
                    : $"N° {r.GetInt32(0)} Retirar a partir del {r.GetDateTime(2):dd/MM/yyyy}",
                new MySqlParameter("@id", sel.IdOrden))).First();

            if (res.StartsWith("Error"))
            {
                await Aviso("Laboratorio", res);
                return;
            }

            string cobertura = (await Db.Query("CALL CoberturaPaciente(@d)", r => r.GetString(0),
                new MySqlParameter("@d", sel.Dni))).FirstOrDefault() ?? "Particular";

            lblRetiro.Text = $"Paciente: {sel.Paciente}\nCobertura: {cobertura}\nPrácticas: {sel.Practicas}\n{res}";
            bdComprobante.IsVisible = true;
            await Cargar();
        }
        catch (Exception ex) { await Aviso("Error", ex.Message); }
    }

    async void OnBuscarPaciente(object sender, EventArgs e)
    {
        string dni = entDni.Text?.Trim();
        if (string.IsNullOrEmpty(dni))
        {
            await Aviso("Laboratorio", "Ingrese un DNI para buscar");
            return;
        }

        try
        {
            var p = (await Db.Query("CALL BuscarPacientePorDni(@d)",
                r => (r.GetInt32(0), r.GetString(1)),
                new MySqlParameter("@d", dni))).FirstOrDefault();

            idPac = p.Item1;

            if (idPac == 0)
            {
                lblPacNuevo.Text = "Paciente no registrado. Complete los datos:";
                lblPacNuevo.TextColor = Colors.Red;
                stFormularioPaciente.IsVisible = true;
            }
            else
            {
                lblPacNuevo.Text = $"Encontrado: {p.Item2}";
                lblPacNuevo.TextColor = Colors.Green;
                stFormularioPaciente.IsVisible = false;
            }
        }
        catch (Exception ex) { await Aviso("Error", ex.Message); }
    }

    async void OnRegistrar(object sender, EventArgs e)
    {
        if (cvPracticas.SelectedItems.Count == 0 || pkModalidadCobertura.SelectedItem == null)
        {
            await Aviso("Laboratorio", "Seleccione las prácticas y la cobertura");
            return;
        }
        if (EsObraSocial && pkObraSocial.SelectedItem == null)
        {
            await Aviso("Laboratorio", "Seleccione la obra social");
            return;
        }
        if (!EsObraSocial && pkMedio.SelectedItem == null)
        {
            await Aviso("Laboratorio", "Seleccione el medio de pago");
            return;
        }

        try
        {
            string ids = string.Join(",", cvPracticas.SelectedItems.Cast<PracticaLab>().Select(x => x.IdEstudio));
            string medio = EsObraSocial ? "Obra Social" : pkMedio.SelectedItem.ToString();
            string res;

            if (idPac == 0)
            {
                string nombre = entNombre.Text?.Trim();
                string apellido = entApellido.Text?.Trim();
                string sexo = pkSexo.SelectedItem?.ToString();

                if (string.IsNullOrEmpty(nombre) || string.IsNullOrEmpty(apellido) || string.IsNullOrEmpty(sexo))
                {
                    await Aviso("Laboratorio", "Complete todos los datos del nuevo paciente");
                    return;
                }
                if (!DateTime.TryParseExact(entFechaNac.Text, "dd/MM/yyyy", AR, DateTimeStyles.None, out var fechaNac) || fechaNac > DateTime.Today)
                {
                    await Aviso("Laboratorio", "Ingrese una fecha de nacimiento válida (dd/mm/aaaa)");
                    return;
                }

                res = (await Db.Query("CALL RegistrarPacienteYOrdenLab(@nom,@ape,@dni,@sex,@fnac,@mod,@os,@i,@m,@mp)", r => r.GetString(0),
                    new MySqlParameter("@nom", nombre),
                    new MySqlParameter("@ape", apellido),
                    new MySqlParameter("@dni", entDni.Text.Trim()),
                    new MySqlParameter("@sex", sexo),
                    new MySqlParameter("@fnac", fechaNac),
                    new MySqlParameter("@mod", pkModalidadCobertura.SelectedItem.ToString()),
                    new MySqlParameter("@os", EsObraSocial ? pkObraSocial.SelectedItem.ToString() : DBNull.Value),
                    new MySqlParameter("@i", ids),
                    new MySqlParameter("@m", Monto),
                    new MySqlParameter("@mp", medio))).First();
            }
            else
            {
                res = (await Db.Query("CALL RegistrarOrdenLab(@p,@i,@m,@mp)", r => r.GetString(0),
                    new MySqlParameter("@p", idPac),
                    new MySqlParameter("@i", ids),
                    new MySqlParameter("@m", Monto),
                    new MySqlParameter("@mp", medio))).First();
            }

            await Aviso("Laboratorio", res == "OK" ? "Orden registrada con éxito" : res);
            if (res != "OK") return;

            idPac = 0;
            entDni.Text = lblPacNuevo.Text = entNombre.Text = entApellido.Text = entFechaNac.Text = "";
            pkSexo.SelectedIndex = pkModalidadCobertura.SelectedIndex = pkObraSocial.SelectedIndex = pkMedio.SelectedIndex = -1;
            stFormularioPaciente.IsVisible = false;
            cvPracticas.SelectedItems.Clear();
            Recalcular();
            await Cargar();
        }
        catch (Exception ex) { await Aviso("Error", ex.Message); }
    }

    private async void OnCerrarSesionClicked(object sender, EventArgs e)
    {
        if (await Application.Current!.Windows[0].Page!.DisplayAlertAsync("Cerrar Sesión", "¿Estás seguro de que deseas salir?", "Sí", "Cancelar"))
        {
            Sesion.Limpiar();
            Application.Current!.Windows[0].Page = new NavigationPage(new LoginPage());
        }
    }
}