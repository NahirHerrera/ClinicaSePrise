using System.Globalization;
using System.Text.RegularExpressions;

namespace ClinicaSePrise.Vistas;

// Lee montos y porcentajes escritos a mano, con o sin separador de miles:
// "22000", "22.000", "22000,50", "22.000,50" o "22000.50"
static class Montos
{
    public static bool TryLeer(string? texto, out decimal valor)
    {
        valor = 0;
        string t = (texto ?? "").Replace("$", "").Replace(" ", "").Trim();
        if (t == "") return false;

        bool punto = t.Contains('.');
        bool coma = t.Contains(',');
        if (punto && coma)
            t = t.Replace(".", "").Replace(',', '.');      // 22.000,50
        else if (coma)
            t = t.Replace(',', '.');                         // 22000,50
        else if (punto && Regex.IsMatch(t, @"^\d{1,3}(\.\d{3})+$"))
            t = t.Replace(".", "");                          // 22.000

        // Sin signo: un monto negativo no es válido
        return decimal.TryParse(t, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out valor);
    }
}
