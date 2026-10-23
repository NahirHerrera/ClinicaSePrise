namespace ClinicaSePrise.Vistas;

// Las vistas de cada rol se muestran dentro de MainPage (MainPage copia solo su Content),
// así que la página del rol nunca se abre y su propio DisplayAlert no aparece.
// Estos avisos se muestran sobre la página que está abierta en la ventana.
static class Avisos
{
    static Page? Visible => Application.Current?.Windows.FirstOrDefault()?.Page;

    public static Task Mostrar(string titulo, string mensaje) =>
        Visible?.DisplayAlertAsync(titulo, mensaje, "OK") ?? Task.CompletedTask;

    public static Task<bool> Confirmar(string titulo, string mensaje) =>
        Visible?.DisplayAlertAsync(titulo, mensaje, "Sí", "No") ?? Task.FromResult(false);
}
