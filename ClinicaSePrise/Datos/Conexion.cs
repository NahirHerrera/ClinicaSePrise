using ClinicaSePrise.Entidades;
using MySqlConnector;

namespace ClinicaSePrise.Datos;

internal class Conexion
{
    // Variables de configuracion de la base de datos
    private string servidor = "localhost";
    private string? puerto;
    private string? usuario;
    private string? password = "1234";
    private string baseDatos = "ClinicaSePrise";

    // Variable estática de la clase Conexión
    private static Conexion? instancia = null;

    private Conexion() { }

    // Método estático de la clase Conexión
    public static Conexion getInstancia()
    {
        if (instancia == null)
        {
            instancia = new Conexion();
        }
        return instancia;
    }

    // Método para recibir los valores desde el formulario
    public void SetCredenciales(string puerto, string usuario, string password)
    {
        this.puerto = puerto;
        this.usuario = usuario;
        this.password = password;
    }

    // Método para crear la conexión con los datos actuales
    public MySqlConnection CrearConexion()
    {
        var cadena = new MySqlConnection
        {
            ConnectionString = $"datasource={servidor};port={puerto};username={usuario};password={password};Database={baseDatos}"
        };
        return cadena;
    }
}
