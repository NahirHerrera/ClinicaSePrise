using Microsoft.Extensions.DependencyInjection;

namespace ClinicaSePrise
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }
        protected override Window CreateWindow(IActivationState? activationState) =>
            new Window(new MainPage()) { Width = 1400, Height = 850 };
    }
}