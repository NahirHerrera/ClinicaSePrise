namespace ClinicaSePrise;
public static class Sesion
{
    public static int IdUsuario { get; set; }
    public static int IdRol { get; set; }      
    public static int IdPaciente { get; set; }
    public static int IdProfesional { get; set; } 
    public static string Nombre { get; set; } = "";
    public static string Apellido { get; set; } = "";
    public static string Rol { get; set; } = "";
}