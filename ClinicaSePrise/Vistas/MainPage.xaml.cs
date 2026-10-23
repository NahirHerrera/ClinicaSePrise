using System;
using Microsoft.Maui.Controls;
using ClinicaSePrise.Entidades;
using ClinicaSePrise.Vistas;

namespace ClinicaSePrise.Vistas
{
    public partial class MainPage : ContentPage
    {
        static string Rol => (Sesion.Rol ?? "").ToLowerInvariant();
        static bool EsPaciente => Rol.Contains("paciente");
        static bool EsRecepcion => Rol.Contains("recepcion") || Rol.Contains("recepción");
        static bool EsMedico => Rol.Contains("medico") || Rol.Contains("médico");
        static bool EsLaboratorio => Rol.Contains("laboratorio");
        static bool EsAsistencial => Rol.Contains("asistencial");
        static bool EsGuardia => Rol.Contains("guardia") || Rol.Contains("internacion");
        static bool EsAdmin => Rol.Contains("administracion") || Rol.Contains("administración");

        static Page HomeSegunRol() =>
            EsPaciente ? new PacientePage() :
            EsRecepcion ? new RecepcionPage() :
            EsMedico ? new MedicoPage() :
            EsLaboratorio ? new LaboratorioPage() :
            EsAsistencial ? new AsistencialPage() :
            EsGuardia ? new Guardia_InternacionPage() :
            EsAdmin ? new AdministracionPage() :
            new HomePage();

        public MainPage(Page? paginaInicial = null)
        {
            InitializeComponent();
            ConfigurarNavbar();

            SetContentFromPage(paginaInicial == null || paginaInicial is HomePage ? HomeSegunRol() : paginaInicial);
        }

        void ConfigurarNavbar()
        {
            LblDisplayNombre.Text = Sesion.DisplayNombre ?? "";
            LblRol.Text = Sesion.Rol ?? "";

            BtnHome.IsVisible = EsAdmin;
            BtnPaciente.IsVisible = EsAdmin;
            BtnRecepcion.IsVisible = EsAdmin;
            BtnMedico.IsVisible = EsAdmin;
            BtnGuardia.IsVisible = EsAdmin;
            BtnLaboratorio.IsVisible = EsAdmin;
            BtnAsistencial.IsVisible = EsAdmin;

            BtnHome.Clicked += (_, __) => SetContentFromPage(HomeSegunRol());
            BtnPaciente.Clicked += (_, __) => SetContentFromPage(new PacientePage());
            BtnRecepcion.Clicked += (_, __) => SetContentFromPage(new RecepcionPage());
            BtnMedico.Clicked += (_, __) => SetContentFromPage(new MedicoPage());
            BtnGuardia.Clicked += (_, __) => SetContentFromPage(new Guardia_InternacionPage());
            BtnLaboratorio.Clicked += (_, __) => SetContentFromPage(new LaboratorioPage());
            BtnAsistencial.Clicked += (_, __) => SetContentFromPage(new AsistencialPage());

            BtnCerrarSesion.Clicked += (_, __) =>
            {
                Sesion.Limpiar();
                Application.Current.MainPage = new NavigationPage(new LoginPage());
            };
        }

        void SetContentFromPage(Page page)
        {
            if (page is ContentPage cp && cp.Content != null)
            {
                cp.Content.BindingContext = cp.BindingContext;
                ContentArea.Content = cp.Content;
                return;
            }

            ContentArea.Content = new Label { Text = "La página no tiene contenido visual para mostrar acá.", HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center };
        }
    }
}