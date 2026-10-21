using System;
using Microsoft.Maui.Controls;
using ClinicaSePrise.Entidades;

namespace ClinicaSePrise.Vistas
{
    public partial class HomePage : ContentPage
    {
        public HomePage()
        {
            InitializeComponent();
            lblBienvenida.Text = $"Bienvenido/a, {Sesion.DisplayNombre}";
            VistaStaff.IsVisible = Sesion.Rol != "Paciente";
            VistaPaciente.IsVisible = Sesion.Rol == "Paciente";
        }

        public HomePage(Dashboard d) : this()
        {
            BindingContext = d;
        }

        private void OnNuevoTurno(object sender, EventArgs e)
        {

        }
    }
}