using Biblioteca.Entidades;
using Biblioteca.Negocio.Interfaces;
using Microsoft.Data.SqlClient;
namespace Biblioteca.Datos;
public class LibroRepositorio : ILibroRepositorio
{
    private const string SelectJoin = @"
        SELECT l.LibroId, l.Titulo, l.ISBN, l.AutorId, l.Ejemplares, l.Activo,
               a.Nombre AS NombreAutor
        FROM Libros l
        INNER JOIN Autores a ON a.AutorId = l.AutorId";
    public async Task<List<Libro>> ListarAsync()
    {
        const string sql = SelectJoin + " WHERE l.Activo = 1 ORDER BY l.Titulo";
        using var cn  = ConexionFactory.Crear();
        using var cmd = new SqlCommand(sql, cn);
        await cn.OpenAsync();
        return await LeerTodosAsync(cmd);
    }
    public async Task<List<Libro>> BuscarAsync(string texto)
    {
        const string sql = SelectJoin + @"
            WHERE l.Activo = 1
              AND (l.Titulo LIKE @T OR l.ISBN LIKE @T OR a.Nombre LIKE @T)
            ORDER BY l.Titulo";
        using var cn  = ConexionFactory.Crear();
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.Add(new SqlParameter("@T", $"%{texto}%"));
        await cn.OpenAsync();
        return await LeerTodosAsync(cmd);
    }
    public async Task<Libro> ObtenerPorIdAsync(int libroId)
    {
        const string sql = SelectJoin + " WHERE l.LibroId = @Id AND l.Activo = 1";
        using var cn  = ConexionFactory.Crear();
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.Add(new SqlParameter("@Id", libroId));
        await cn.OpenAsync();
        return (await LeerTodosAsync(cmd)).FirstOrDefault();
    }
    public async Task<bool> ExisteISBNAsync(string isbn, int excluirId)
    {
        const string sql = "SELECT COUNT(1) FROM Libros WHERE ISBN=@ISBN AND LibroId<>@Id AND Activo=1";
        using var cn  = ConexionFactory.Crear();
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.Add(new SqlParameter("@ISBN", isbn));
        cmd.Parameters.Add(new SqlParameter("@Id",   excluirId));
        await cn.OpenAsync();
        return (int)await cmd.ExecuteScalarAsync() > 0;
    }
    public async Task<bool> TienePrestamoPendienteAsync(int libroId)
    {
        const string sql = @"SELECT COUNT(1) FROM DetallePrestamo dp
                             INNER JOIN Prestamos p ON p.PrestamoId = dp.PrestamoId
                             WHERE dp.LibroId = @Id
                               AND dp.FechaDevolucion IS NULL
                               AND p.Estado = 'Pendiente'";
        using var cn  = ConexionFactory.Crear();
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.Add(new SqlParameter("@Id", libroId));
        await cn.OpenAsync();
        return (int)await cmd.ExecuteScalarAsync() > 0;
    }
    public async Task InsertarAsync(Libro libro)
    {
        const string sql = @"INSERT INTO Libros (Titulo, ISBN, AutorId, Ejemplares)
                             VALUES (@Titulo, @ISBN, @AutorId, @Ejemplares)";
        using var cn  = ConexionFactory.Crear();
        using var cmd = new SqlCommand(sql, cn);
        AgregarParams(cmd, libro);
        await cn.OpenAsync();
        await cmd.ExecuteNonQueryAsync();
    }
    public async Task ActualizarAsync(Libro libro)
    {
        const string sql = @"UPDATE Libros
                             SET Titulo=@Titulo, ISBN=@ISBN, AutorId=@AutorId, Ejemplares=@Ejemplares
                             WHERE LibroId=@Id";
        using var cn  = ConexionFactory.Crear();
        using var cmd = new SqlCommand(sql, cn);
        AgregarParams(cmd, libro);
        cmd.Parameters.Add(new SqlParameter("@Id", libro.LibroId));
        await cn.OpenAsync();
        await cmd.ExecuteNonQueryAsync();
    }
    public async Task EliminarLogicoAsync(int libroId)
    {
        const string sql = "UPDATE Libros SET Activo = 0 WHERE LibroId = @Id";
        using var cn  = ConexionFactory.Crear();
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.Add(new SqlParameter("@Id", libroId));
        await cn.OpenAsync();
        await cmd.ExecuteNonQueryAsync();
    }
    public async Task IncrementarEjemplarAsync(int libroId)
    {
        const string sql = "UPDATE Libros SET Ejemplares = Ejemplares + 1 WHERE LibroId = @Id";
        using var cn  = ConexionFactory.Crear();
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.Add(new SqlParameter("@Id", libroId));
        await cn.OpenAsync();
        await cmd.ExecuteNonQueryAsync();
    }
    private static void AgregarParams(SqlCommand cmd, Libro l)
    {
        cmd.Parameters.Add(new SqlParameter("@Titulo",     l.Titulo));
        cmd.Parameters.Add(new SqlParameter("@ISBN",       l.ISBN));
        cmd.Parameters.Add(new SqlParameter("@AutorId",    l.AutorId));
        cmd.Parameters.Add(new SqlParameter("@Ejemplares", l.Ejemplares));
    }
    private static async Task<List<Libro>> LeerTodosAsync(SqlCommand cmd)
    {
        var lista = new List<Libro>();
        await using var dr = await cmd.ExecuteReaderAsync();
        while (await dr.ReadAsync())
            lista.Add(new Libro {
                LibroId     = dr.GetInt32(0),
                Titulo      = dr.GetString(1),
                ISBN        = dr.GetString(2),
                AutorId     = dr.GetInt32(3),
                Ejemplares  = dr.GetInt32(4),
                Activo      = dr.GetBoolean(5),
                NombreAutor = dr.GetString(6)
            });
        return lista;
    }
}
