using Biblioteca.Entidades;
using Biblioteca.Negocio.Interfaces;
using Microsoft.Data.SqlClient;
namespace Biblioteca.Datos;
public class AutorRepositorio : IAutorRepositorio
{
    private const string SelectCols =
        "SELECT AutorId, Nombre, Nacionalidad, Activo FROM Autores";
    public async Task<List<Autor>> ListarAsync()
    {
        const string sql = SelectCols + " WHERE Activo = 1 ORDER BY Nombre";
        using var cn  = ConexionFactory.Crear();
        using var cmd = new SqlCommand(sql, cn);
        await cn.OpenAsync();
        return await LeerTodosAsync(cmd);
    }
    public async Task<Autor> ObtenerPorIdAsync(int autorId)
    {
        const string sql = SelectCols + " WHERE AutorId = @Id AND Activo = 1";
        using var cn  = ConexionFactory.Crear();
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.Add(new SqlParameter("@Id", autorId));
        await cn.OpenAsync();
        return (await LeerTodosAsync(cmd)).FirstOrDefault();
    }
    public async Task InsertarAsync(Autor autor)
    {
        const string sql = "INSERT INTO Autores (Nombre, Nacionalidad) VALUES (@Nombre, @Nacionalidad)";
        using var cn  = ConexionFactory.Crear();
        using var cmd = new SqlCommand(sql, cn);
        AgregarParams(cmd, autor);
        await cn.OpenAsync();
        await cmd.ExecuteNonQueryAsync();
    }
    public async Task ActualizarAsync(Autor autor)
    {
        const string sql = "UPDATE Autores SET Nombre=@Nombre, Nacionalidad=@Nacionalidad WHERE AutorId=@Id";
        using var cn  = ConexionFactory.Crear();
        using var cmd = new SqlCommand(sql, cn);
        AgregarParams(cmd, autor);
        cmd.Parameters.Add(new SqlParameter("@Id", autor.AutorId));
        await cn.OpenAsync();
        await cmd.ExecuteNonQueryAsync();
    }
    public async Task EliminarLogicoAsync(int autorId)
    {
        const string sql = @"UPDATE Libros  SET Activo = 0 WHERE AutorId = @Id;
                             UPDATE Autores SET Activo = 0 WHERE AutorId = @Id;";
        using var cn  = ConexionFactory.Crear();
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.Add(new SqlParameter("@Id", autorId));
        await cn.OpenAsync();
        await cmd.ExecuteNonQueryAsync();
    }
    private static void AgregarParams(SqlCommand cmd, Autor a)
    {
        cmd.Parameters.Add(new SqlParameter("@Nombre",       a.Nombre));
        cmd.Parameters.Add(new SqlParameter("@Nacionalidad", (object)a.Nacionalidad ?? DBNull.Value));
    }
    private static async Task<List<Autor>> LeerTodosAsync(SqlCommand cmd)
    {
        var lista = new List<Autor>();
        await using var dr = await cmd.ExecuteReaderAsync();
        while (await dr.ReadAsync())
            lista.Add(new Autor {
                AutorId      = dr.GetInt32(0),
                Nombre       = dr.GetString(1),
                Nacionalidad = dr.IsDBNull(2) ? null : dr.GetString(2),
                Activo       = dr.GetBoolean(3)
            });
        return lista;
    }
}
