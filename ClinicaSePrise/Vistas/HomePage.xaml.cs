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


        }

        private void OnNuevoTurno(object sender, EventArgs e)
        {

        }
    }
}
