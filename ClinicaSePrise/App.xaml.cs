using Microsoft.Extensions.DependencyInjection;

namespace ClinicaSePrise
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            // Establecer la ventana inicial apuntando al Login de forma limpia
            MainPage = new NavigationPage(new LoginPage());
        }
    }
}