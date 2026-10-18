
using ClinicaSePrise.Datos;
using ClinicaSePrise.Entidades;
using MySqlConnector;
using static System.Collections.Specialized.BitVector32;

namespace ClinicaSePrise.Vistas;

public partial class PacientePage : ContentPage
{
    public PacientePage()
    {
        InitializeComponent();
        Loaded += async (_, _) => await CargarTurnos();
    }

    async Task CargarTurnos()
    {
        try
        {
            var turnos = await Db.Query("CALL MisTurnos(@id)",
                r => new Turno(r.GetInt32(0), r.GetInt32(1), r.GetInt32(2), r.GetString(3), r.GetDateTime(4),
                               r.GetString(5), r.GetInt32(6), r.GetString(7), r.GetString(8))
                { nombreProfesional = r.GetString(9) },
                new MySqlParameter("@id", Sesion.IdPaciente));

            BindableLayout.SetItemsSource(ListaTurnos, turnos);
            SinTurnos.IsVisible = turnos.Count == 0;
        }
        catch (Exception ex) { await Window.Page!.DisplayAlertAsync("Error", ex.Message, "OK"); }
    }

    async void Cancelar_Clicked(object sender, EventArgs e)
    {
        var t = (Turno)((Button)sender).BindingContext;
        if (!await Window.Page!.DisplayAlertAsync("Cancelar turno", $"¿Cancelar el turno de {t.especialidad}?", "Sí", "No")) return;

        var ok = (await Db.Query("CALL CancelarTurno(@t,@p)", r => Convert.ToInt32(r[0]),
            new MySqlParameter("@t", t.idTurno), new MySqlParameter("@p", Sesion.IdPaciente))).First() == 1;

        await Window.Page!.DisplayAlertAsync(ok ? "Listo" : "No se pudo", ok ? "Turno cancelado." : "Ese turno ya no se puede cancelar.", "OK");
        await CargarTurnos();
    }
}
