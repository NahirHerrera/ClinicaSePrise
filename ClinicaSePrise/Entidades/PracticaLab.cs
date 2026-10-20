namespace ClinicaSePrise.Entidades;
public class PracticaLab
{
    public int IdEstudio { get; set; }
    public string Nombre { get; set; } = "";
    public bool RequiereAyuno { get; set; }
    public string Descripcion => RequiereAyuno ? $"{Nombre} (ayuno, hasta las 11)" : Nombre;
}