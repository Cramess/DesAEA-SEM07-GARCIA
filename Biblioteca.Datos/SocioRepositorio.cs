using Biblioteca.Entidades;
using Biblioteca.Negocio.Interfaces;
using Microsoft.Data.SqlClient;
namespace Biblioteca.Datos;
public class SocioRepositorio : ISocioRepositorio
{
    private const string SelectCols =
        "SELECT SocioId, DNI, Nombre, Email, Activo FROM Socios";
    public async Task<List<Socio>> ListarAsync()
    {
        const string sql = SelectCols + " WHERE Activo = 1 ORDER BY Nombre";
        using var cn  = ConexionFactory.Crear();
        using var cmd = new SqlCommand(sql, cn);
        await cn.OpenAsync();
        return await LeerTodosAsync(cmd);
    }
    public async Task<List<Socio>> BuscarAsync(string texto)
    {
        const string sql = SelectCols + @"
            WHERE Activo = 1
              AND (DNI LIKE @T OR Nombre LIKE @T)
            ORDER BY Nombre";
        using var cn  = ConexionFactory.Crear();
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.Add(new SqlParameter("@T", $"%{texto}%"));
        await cn.OpenAsync();
        return await LeerTodosAsync(cmd);
    }
    public async Task<Socio> ObtenerPorIdAsync(int socioId)
    {
        const string sql = SelectCols + " WHERE SocioId = @Id AND Activo = 1";
        using var cn  = ConexionFactory.Crear();
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.Add(new SqlParameter("@Id", socioId));
        await cn.OpenAsync();
        return (await LeerTodosAsync(cmd)).FirstOrDefault();
    }
    public async Task<bool> ExisteDNIAsync(string dni, int excluirId)
    {
        const string sql = "SELECT COUNT(1) FROM Socios WHERE DNI=@DNI AND SocioId<>@Id";
        using var cn  = ConexionFactory.Crear();
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.Add(new SqlParameter("@DNI", dni));
        cmd.Parameters.Add(new SqlParameter("@Id",  excluirId));
        await cn.OpenAsync();
        return (int)await cmd.ExecuteScalarAsync() > 0;
    }
    public async Task<bool> TienePrestamoPendienteAsync(int socioId)
    {
        const string sql = @"SELECT COUNT(1) FROM Prestamos
                             WHERE SocioId = @Id AND Estado = 'Pendiente'";
        using var cn  = ConexionFactory.Crear();
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.Add(new SqlParameter("@Id", socioId));
        await cn.OpenAsync();
        return (int)await cmd.ExecuteScalarAsync() > 0;
    }
    public async Task InsertarAsync(Socio socio)
    {
        const string sql = "INSERT INTO Socios (DNI, Nombre, Email) VALUES (@DNI, @Nombre, @Email)";
        using var cn  = ConexionFactory.Crear();
        using var cmd = new SqlCommand(sql, cn);
        AgregarParams(cmd, socio);
        await cn.OpenAsync();
        await cmd.ExecuteNonQueryAsync();
    }
    public async Task ActualizarAsync(Socio socio)
    {
        const string sql = "UPDATE Socios SET DNI=@DNI, Nombre=@Nombre, Email=@Email WHERE SocioId=@Id";
        using var cn  = ConexionFactory.Crear();
        using var cmd = new SqlCommand(sql, cn);
        AgregarParams(cmd, socio);
        cmd.Parameters.Add(new SqlParameter("@Id", socio.SocioId));
        await cn.OpenAsync();
        await cmd.ExecuteNonQueryAsync();
    }
    public async Task EliminarLogicoAsync(int socioId)
    {
        const string sql = "UPDATE Socios SET Activo = 0 WHERE SocioId = @Id";
        using var cn  = ConexionFactory.Crear();
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.Add(new SqlParameter("@Id", socioId));
        await cn.OpenAsync();
        await cmd.ExecuteNonQueryAsync();
    }
    private static void AgregarParams(SqlCommand cmd, Socio s)
    {
        cmd.Parameters.Add(new SqlParameter("@DNI",    s.DNI));
        cmd.Parameters.Add(new SqlParameter("@Nombre", s.Nombre));
        cmd.Parameters.Add(new SqlParameter("@Email",  (object)s.Email ?? DBNull.Value));
    }
    private static async Task<List<Socio>> LeerTodosAsync(SqlCommand cmd)
    {
        var lista = new List<Socio>();
        await using var dr = await cmd.ExecuteReaderAsync();
        while (await dr.ReadAsync())
            lista.Add(new Socio {
                SocioId = dr.GetInt32(0),
                DNI     = dr.GetString(1),
                Nombre  = dr.GetString(2),
                Email   = dr.IsDBNull(3) ? null : dr.GetString(3),
                Activo  = dr.GetBoolean(4)
            });
        return lista;
    }
}
