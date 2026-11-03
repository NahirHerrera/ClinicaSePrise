namespace ClinicaSePrise.Graficos;

// Gráfico de barras horizontales, dibujado a mano (sin librerías externas).
// Se usa con un GraphicsView: GraphicsView.Drawable = new GraficoBarras()
public class GraficoBarras : IDrawable
{
    const float AltoFila = 30;

    public List<DatoGrafico> Datos { get; set; } = new();
    public Color ColorBarra { get; set; } = Colors.SteelBlue;

    public void Draw(ICanvas canvas, RectF r)
    {
        canvas.FontSize = 12;
        canvas.FontColor = ColorTexto();

        if (Datos.Count == 0)
        {
            canvas.DrawString("Sin datos este mes", r, HorizontalAlignment.Center, VerticalAlignment.Center);
            return;
        }

        float anchoEtiqueta = 90, anchoValor = 70;
        float anchoBarras = Math.Max(10, r.Width - anchoEtiqueta - anchoValor);
        double maximo = Datos.Max(d => d.Valor);

        for (int i = 0; i < Datos.Count; i++)
        {
            var d = Datos[i];
            float y = r.Y + i * AltoFila;
            float ancho = (float)(d.Valor / maximo) * anchoBarras;

            canvas.DrawString(d.Etiqueta, r.X, y, anchoEtiqueta - 8, AltoFila, HorizontalAlignment.Right, VerticalAlignment.Center);
            canvas.FillColor = ColorBarra;
            canvas.FillRectangle(r.X + anchoEtiqueta, y + 6, Math.Max(ancho, 4), AltoFila - 12);
            canvas.DrawString(d.Texto ?? d.Valor.ToString("N0"), r.X + anchoEtiqueta + ancho + 6, y, anchoValor, AltoFila,
                HorizontalAlignment.Left, VerticalAlignment.Center);
        }
    }

    // Alto que necesita el GraphicsView para mostrar todas las filas
    public double AltoNecesario => Math.Max(60, Datos.Count * AltoFila);

    // Texto con la paleta del tema actual (Colors.xaml)
    static Color ColorTexto()
    {
        var app = Application.Current;
        string clave = app?.RequestedTheme == AppTheme.Dark ? "TextoSuaveOscuro" : "TextoSuaveClaro";
        return app != null && app.Resources.TryGetValue(clave, out var c) && c is Color color ? color : Colors.DimGray;
    }

    // El GraphicsView no se redibuja solo al cambiar el tema: lo forzamos mientras esté en pantalla
    public static void RedibujarAlCambiarTema(GraphicsView vista)
    {
        void Redibujar(object? s, AppThemeChangedEventArgs e) => vista.Invalidate();
        vista.Loaded += (_, __) => { if (Application.Current != null) Application.Current.RequestedThemeChanged += Redibujar; };
        vista.Unloaded += (_, __) => { if (Application.Current != null) Application.Current.RequestedThemeChanged -= Redibujar; };
    }
}
