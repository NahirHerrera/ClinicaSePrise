using ClinicaSePrise.Datos;
using ClinicaSePrise.Entidades;
using MySqlConnector;

namespace ClinicaSePrise.Vistas;

public partial class StockPage : ContentView
{
    public string Sector { get; set; }

    public StockPage() => InitializeComponent();

    public async Task Cargar()
    {
        var datos = await Db.Query("CALL StockInsumos(@s)",
            r => new InsumoStock(
                r.GetInt32(0),
                r.GetString(1),
                r.IsDBNull(2) ? null : r.GetInt32(2),
                r.IsDBNull(3) ? null : r.GetInt32(3),
                r.GetInt32(4),
                r.GetString(5)),
            new MySqlParameter("@s", Sector));
        lista.ItemsSource = datos;
        lblResumen.Text = $"{datos.Count(d => d.estadoReposicion == "Reposición")} insumo(s) para reponer";
    }

    async void Reponer(object sender, EventArgs e)
    {
        var i = (InsumoStock)((Button)sender).BindingContext;
        if (!int.TryParse(await Application.Current.Windows[0].Page.DisplayPromptAsync("Reponer", $"¿Cuántas unidades de {i.nombre}?", keyboard: Keyboard.Numeric), out var n) || n <= 0) return;
        await Db.Query("CALL ReponerInsumo(@i,@n)", r => 0, new MySqlParameter("@i", i.idInsumo), new MySqlParameter("@n", n));
        await Cargar();
    }
}