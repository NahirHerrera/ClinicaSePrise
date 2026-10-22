namespace ClinicaSePrise.Graficos;

// Un color por estado de turno, igual en las listas y en los gráficos
public static class ColoresEstado
{
    // Texto de la etiqueta del estado
    public static Color Texto(string estado) => Color.FromArgb(estado switch
    {
        "Atendido" => "#15803D",
        "En espera" => "#0369A1",
        "Confirmado" => "#1D4ED8",
        "Pendiente" => "#B45309",
        "Ausente" => "#B91C1C",
        _ => "#475569"
    });

    // Fondo suave de la etiqueta del estado
    public static Color Fondo(string estado) => Color.FromArgb(estado switch
    {
        "Atendido" => "#DCFCE7",
        "En espera" => "#E0F2FE",
        "Confirmado" => "#DBEAFE",
        "Pendiente" => "#FEF3C7",
        "Ausente" => "#FEE2E2",
        _ => "#F1F5F9"
    });

    // Color fuerte para los gráficos
    public static Color Grafico(string estado) => Color.FromArgb(estado switch
    {
        "Atendido" => "#16A34A",
        "En espera" => "#0EA5E9",
        "Confirmado" => "#2563EB",
        "Pendiente" => "#F59E0B",
        "Ausente" => "#DC2626",
        _ => "#94A3B8"
    });
}
