using System.Data.Common;
using MySqlConnector;

namespace ClinicaSePrise.Datos;

public static class Db
{
    public static async Task<List<T>> Query<T>(string sql, Func<DbDataReader, T> map, params MySqlParameter[] ps)
    {
        using var cn = Conexion.getInstancia().CrearConexion();
        await cn.OpenAsync();
        using var cmd = new MySqlCommand(sql, cn);
        cmd.Parameters.AddRange(ps);
        using var rd = await cmd.ExecuteReaderAsync();
        var lista = new List<T>();
        while (await rd.ReadAsync()) lista.Add(map(rd));
        return lista;
    }
}