using ClinicaSePrise.Datos;
using ClinicaSePrise.Entidades;
using MySqlConnector;

namespace ClinicaSePrise.Vistas;

public partial class LaboratorioPage : ContentPage
{
    OrdenLab sel;
    int idPac;

    public LaboratorioPage()
    {
        InitializeComponent();

        // --- LÓGICA DE SALUDO DINÁMICO ---
        string nombreCompleto = $"{Sesion.Nombre} {Sesion.Apellido}".Trim();
        string rolActual = Sesion.Rol?.Trim() ?? "";

        bool esRolConNombre = rolActual.Equals("Paciente", StringComparison.OrdinalIgnoreCase) ||
                              rolActual.Equals("Medico", StringComparison.OrdinalIgnoreCase) ||
                              rolActual.Equals("Administracion", StringComparison.OrdinalIgnoreCase);

        if (esRolConNombre)
        {
            string usuario = !string.IsNullOrEmpty(nombreCompleto) ? nombreCompleto : "Usuario";
            lblBienvenida.Text = $"Bienvenido/a, {usuario} ({rolActual})";
        }
        else if (!string.IsNullOrEmpty(rolActual))
        {
            lblBienvenida.Text = $"Bienvenido/a, {rolActual}";
        }
        else
        {
            lblBienvenida.Text = "Bienvenido/a";
        }
        // ---------------------------------
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CargarPracticas();
        await Cargar();
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
        await Db.Query("CALL LlamarPacienteLab(@id)", r => 0, new MySqlParameter("@id", sel.IdOrden));
        await DisplayAlert("Llamado", $"Llamando a {sel.Paciente}", "OK");
        await Cargar();
    }

    async void OnAtender(object sender, EventArgs e)
    {
        string res = (await Db.Query("CALL AtenderOrdenLab(@id)",
            r => r.FieldCount == 1
                ? r.GetString(0)
                : $"N° {r.GetInt32(0)} Retirar a partir del {r.GetDateTime(2):dd/MM/yyyy}",
            new MySqlParameter("@id", sel.IdOrden))).First();

        if (res.StartsWith("Error"))
        {
            await DisplayAlert("Laboratorio", res, "OK");
            return;
        }
        lblRetiro.Text = res;
        bdComprobante.IsVisible = true;
        await Cargar();
    }

    async void OnBuscarPaciente(object sender, EventArgs e)
    {
        string dni = entDni.Text?.Trim();
        if (string.IsNullOrEmpty(dni))
        {
            await DisplayAlert("Laboratorio", "Ingrese un DNI para buscar", "OK");
            return;
        }

        var p = (await Db.Query("CALL BuscarPacientePorDni(@d)",
            r => (r.GetInt32(0), r.GetString(1)),
            new MySqlParameter("@d", dni))).FirstOrDefault();

        idPac = p.Item1;

        if (idPac == 0)
        {
            // Paciente no registrado: Mostramos formulario de carga rápida
            lblPacNuevo.Text = "Paciente no registrado. Complete los datos:";
            lblPacNuevo.TextColor = Colors.Red;
            stFormularioPaciente.IsVisible = true;
        }
        else
        {
            // Paciente encontrado: Ocultamos el formulario
            lblPacNuevo.Text = $"Encontrado: {p.Item2}";
            lblPacNuevo.TextColor = Colors.Green;
            stFormularioPaciente.IsVisible = false;
        }
    }

    async void OnRegistrar(object sender, EventArgs e)
    {
        // Validaciones básicas de prácticas, monto y medio de pago
        if (!decimal.TryParse(entMonto.Text, out var monto) || pkMedio.SelectedItem == null || cvPracticas.SelectedItems.Count == 0)
        {
            await DisplayAlert("Laboratorio", "Seleccione prácticas, ingrese el monto y el medio de pago", "OK");
            return;
        }

        string ids = string.Join(",", cvPracticas.SelectedItems.Cast<PracticaLab>().Select(x => x.IdEstudio));
        string res = "";

        // Si el paciente no estaba registrado (idPac == 0), debemos registrarlo primero con los datos del formulario
        if (idPac == 0)
        {
            string nombre = entNombre.Text?.Trim();
            string apellido = entApellido.Text?.Trim();
            string dni = entDni.Text?.Trim();
            string sexo = pkSexo.SelectedItem?.ToString();
            string fechaNacStr = entFechaNacimiento.Text?.Trim();
            string modalidad = pkModalidadCobertura.SelectedItem?.ToString();
            string obraSocial = pkObraSocial.SelectedItem?.ToString();

            if (string.IsNullOrEmpty(nombre) || string.IsNullOrEmpty(apellido) || string.IsNullOrEmpty(sexo) || string.IsNullOrEmpty(modalidad) || string.IsNullOrEmpty(fechaNacStr))
            {
                await DisplayAlert("Laboratorio", "Complete todos los datos del nuevo paciente", "OK");
                return;
            }

            // Validar que la fecha escrita a mano tenga el formato correcto (dd/MM/yyyy)
            if (!DateTime.TryParseExact(fechaNacStr, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime fechaNac))
            {
                await DisplayAlert("Laboratorio", "Formato de fecha inválido. Ingrese la fecha en formato DD/MM/AAAA", "OK");
                return;
            }

            // Ejecutamos procedimiento para registrar paciente y obtener su ID
            var resultadoReg = await Db.Query("CALL RegistrarPacienteLaboratorio(@nom, @ape, @dni, @sex, @fnac, @mod, @os)",
                r => (r.GetString(0), r.GetInt32(1)),
                new MySqlParameter("@nom", nombre),
                new MySqlParameter("@ape", apellido),
                new MySqlParameter("@dni", dni),
                new MySqlParameter("@sex", sexo),
                new MySqlParameter("@fnac", fechaNac),
                new MySqlParameter("@mod", modalidad),
                new MySqlParameter("@os", string.IsNullOrEmpty(obraSocial) ? (object)DBNull.Value : obraSocial));

            var primerRes = resultadoReg.FirstOrDefault();
            if (primerRes.Item1 != "OK")
            {
                await DisplayAlert("Laboratorio", primerRes.Item1, "OK");
                return;
            }
            idPac = primerRes.Item2;
        }

        // Una vez asegurado el idPac, registramos la orden de laboratorio
        res = (await Db.Query("CALL RegistrarOrdenLab(@p,@i,@m,@mp)", r => r.GetString(0),
            new MySqlParameter("@p", idPac),
            new MySqlParameter("@i", ids),
            new MySqlParameter("@m", monto),
            new MySqlParameter("@mp", pkMedio.SelectedItem.ToString()))).First();

        await DisplayAlert("Laboratorio", res == "OK" ? "Orden registrada con éxito" : res, "OK");
        if (res != "OK") return;

        // Limpiar controles tras el éxito
        idPac = 0;
        entDni.Text = entMonto.Text = lblPacNuevo.Text = entNombre.Text = entApellido.Text = entFechaNacimiento.Text = "";
        pkSexo.SelectedIndex = pkModalidadCobertura.SelectedIndex = pkObraSocial.SelectedIndex = pkMedio.SelectedIndex = -1;
        pkSexo.SelectedIndex = pkModalidadCobertura.SelectedIndex = pkMedio.SelectedIndex = -1;
        stFormularioPaciente.IsVisible = false;
        cvPracticas.SelectedItems.Clear();
        await Cargar();
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