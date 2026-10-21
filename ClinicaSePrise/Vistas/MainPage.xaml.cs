using System;
using Microsoft.Maui.Controls;
using ClinicaSePrise.Entidades;
using ClinicaSePrise.Vistas;

namespace ClinicaSePrise.Vistas
{
    public partial class MainPage : ContentPage
    {
        static bool EsPaciente => (Sesion.Rol ?? "").ToLowerInvariant().Contains("paciente");

        public MainPage(Page? paginaInicial = null)
        {
            InitializeComponent();
            ConfigurarNavbar();

            if (paginaInicial != null)
                SetContentFromPage(paginaInicial is HomePage && EsPaciente ? new PacientePage() : paginaInicial);
        }

        void ConfigurarNavbar()
        {
            LblDisplayNombre.Text = Sesion.DisplayNombre ?? "";
            LblRol.Text = Sesion.Rol ?? "";

            string rol = (Sesion.Rol ?? "").ToLowerInvariant();

            BtnHome.IsVisible = rol.Contains("paciente") || rol.Contains("recepcion") || rol.Contains("médico") ||
                                rol.Contains("laboratorio") || rol.Contains("asistencial") || rol.Contains("administracion");
            BtnPaciente.IsVisible = rol.Contains("administracion");
            BtnRecepcion.IsVisible = rol.Contains("recepcion") || rol.Contains("administracion");
            BtnMedico.IsVisible = rol.Contains("medico") || rol.Contains("médico");
            BtnAdministracion.IsVisible = rol.Contains("administracion");
            BtnGuardia.IsVisible = rol.Contains("guardia") || rol.Contains("internacion") || rol.Contains("administracion");
            BtnLaboratorio.IsVisible = rol.Contains("laboratorio") || rol.Contains("administracion");
            BtnAsistencial.IsVisible = rol.Contains("asistencial") || rol.Contains("administracion");

            BtnHome.Clicked += (_, __) => SetContentFromPage(EsPaciente ? new PacientePage() : new HomePage());
            BtnPaciente.Clicked += (_, __) => SetContentFromPage(new PacientePage());
            BtnRecepcion.Clicked += (_, __) => SetContentFromPage(new RecepcionPage());
            BtnMedico.Clicked += (_, __) => SetContentFromPage(new MedicoPage());
            BtnAdministracion.Clicked += (_, __) => SetContentFromPage(new AdministracionPage());
            BtnGuardia.Clicked += (_, __) => SetContentFromPage(new Guardia_InternacionPage());
            BtnLaboratorio.Clicked += (_, __) => SetContentFromPage(new LaboratorioPage());
            BtnAsistencial.Clicked += (_, __) => SetContentFromPage(new AsistencialPage());

            BtnCerrarSesion.Clicked += async (_, __) =>
            {
                Sesion.Limpiar();
                Application.Current.MainPage = new NavigationPage(new LoginPage());
                await Task.CompletedTask;
            };
        }

        void SetContentFromPage(Page page)
        {
            if (page == null)
            {
                ContentArea.Content = new Label { Text = "Página nula", HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center };
                return;
            }

            if (page is ContentPage cp)
            {
                if (cp.Content != null)
                {
                    cp.Content.BindingContext = cp.BindingContext;
                    ContentArea.Content = cp.Content;
                    return;
                }
            }

            ContentArea.Content = new Label { Text = "La página no tiene contenido visual para mostrar acá.", HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center };
        }
    }
}