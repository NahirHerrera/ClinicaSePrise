namespace ClinicaSePrise.Entidades;
public static class Sesion
{
    public static int IdUsuario { get; set; }
    public static int IdRol { get; set; }      
    public static int IdPaciente { get; set; }
    public static int IdProfesional { get; set; } 
    public static string DisplayNombre { get; set; } = "";
    public static string Rol { get; set; } = "";


    public static void Limpiar()
    {
        IdUsuario = 0;
        IdRol = 0;
        Rol = null;
        DisplayNombre = null;
        IdPaciente = 0;
        IdProfesional = 0;
    }
}