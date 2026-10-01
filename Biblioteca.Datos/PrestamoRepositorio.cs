using Biblioteca.Entidades;
using Biblioteca.Negocio.Interfaces;
using Microsoft.Data.SqlClient;
namespace Biblioteca.Datos;
public class PrestamoRepositorio : IPrestamoRepositorio
{
    private const string SelectPrestamo = @"
        SELECT p.PrestamoId, p.SocioId, p.FechaPrestamo, p.FechaLimite, p.Estado,
               s.Nombre AS NombreSocio
        FROM Prestamos p
        INNER JOIN Socios s ON s.SocioId = p.SocioId";
    public async Task<List<Prestamo>> ListarAsync()
    {
        const string sql = SelectPrestamo + " ORDER BY p.FechaPrestamo DESC";
        using var cn  = ConexionFactory.Crear();
        using var cmd = new SqlCommand(sql, cn);
        await cn.OpenAsync();
        return await LeerTodosAsync(cmd);
    }
    public async Task<List<Prestamo>> ListarPorFechasAsync(DateTime desde, DateTime hasta)
    {
        const string sql = SelectPrestamo + @"
            WHERE p.FechaPrestamo BETWEEN @Desde AND @Hasta
            ORDER BY p.FechaPrestamo DESC";
        using var cn  = ConexionFactory.Crear();
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.Add(new SqlParameter("@Desde", desde.Date));
        cmd.Parameters.Add(new SqlParameter("@Hasta", hasta.Date));
        await cn.OpenAsync();
        var prestamos = await LeerTodosAsync(cmd);
        foreach (var p in prestamos)
            p.Detalles = await ListarDetallesAsync(p.PrestamoId);
        return prestamos;
    }
    public async Task<Prestamo> ObtenerPorIdAsync(int prestamoId)
    {
        const string sql = SelectPrestamo + " WHERE p.PrestamoId = @Id";
        using var cn  = ConexionFactory.Crear();
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.Add(new SqlParameter("@Id", prestamoId));
        await cn.OpenAsync();
        return (await LeerTodosAsync(cmd)).FirstOrDefault();
    }
    public async Task<int> ContarLibrosPendientesPorSocioAsync(int socioId)
    {
        const string sql = @"
            SELECT COUNT(1)
            FROM DetallePrestamo dp
            INNER JOIN Prestamos p ON p.PrestamoId = dp.PrestamoId
            WHERE p.SocioId = @SocioId
              AND dp.FechaDevolucion IS NULL
              AND p.Estado = 'Pendiente'";
        using var cn  = ConexionFactory.Crear();
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.Add(new SqlParameter("@SocioId", socioId));
        await cn.OpenAsync();
        return (int)await cmd.ExecuteScalarAsync();
    }
    public async Task<List<DetallePrestamo>> ListarDetallesAsync(int prestamoId)
    {
        const string sql = @"
            SELECT dp.PrestamoId, dp.LibroId, dp.FechaDevolucion, dp.Multa,
                   l.Titulo, l.ISBN
            FROM DetallePrestamo dp
            INNER JOIN Libros l ON l.LibroId = dp.LibroId
            WHERE dp.PrestamoId = @Id";
        using var cn  = ConexionFactory.Crear();
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.Add(new SqlParameter("@Id", prestamoId));
        await cn.OpenAsync();
        var lista = new List<DetallePrestamo>();
        await using var dr = await cmd.ExecuteReaderAsync();
        while (await dr.ReadAsync())
            lista.Add(new DetallePrestamo {
                PrestamoId      = dr.GetInt32(0),
                LibroId         = dr.GetInt32(1),
                FechaDevolucion = dr.IsDBNull(2) ? null : dr.GetDateTime(2),
                Multa           = dr.IsDBNull(3) ? null : dr.GetDecimal(3),
                TituloLibro     = dr.GetString(4),
                ISBN            = dr.GetString(5)
            });
        return lista;
    }
    public async Task<int> RegistrarPrestamoTransaccionAsync(Prestamo prestamo, List<int> librosIds)
    {
        using var cn = ConexionFactory.Crear();
        await cn.OpenAsync();
        await using var tx = (SqlTransaction)await cn.BeginTransactionAsync();
        try
        {
            const string sqlCabecera = @"
                INSERT INTO Prestamos (SocioId, FechaPrestamo, FechaLimite, Estado)
                VALUES (@SocioId, @FechaPrestamo, @FechaLimite, @Estado);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";
            using var cmdCabecera = new SqlCommand(sqlCabecera, cn, tx);
            cmdCabecera.Parameters.Add(new SqlParameter("@SocioId",       prestamo.SocioId));
            cmdCabecera.Parameters.Add(new SqlParameter("@FechaPrestamo", prestamo.FechaPrestamo));
            cmdCabecera.Parameters.Add(new SqlParameter("@FechaLimite",   prestamo.FechaLimite));
            cmdCabecera.Parameters.Add(new SqlParameter("@Estado",        prestamo.Estado));
            int nuevoId = (int)await cmdCabecera.ExecuteScalarAsync();
            foreach (int libroId in librosIds)
            {
                using var cmdDetalle = new SqlCommand(@"
                    INSERT INTO DetallePrestamo (PrestamoId, LibroId)
                    VALUES (@PrestamoId, @LibroId)", cn, tx);
                cmdDetalle.Parameters.Add(new SqlParameter("@PrestamoId", nuevoId));
                cmdDetalle.Parameters.Add(new SqlParameter("@LibroId",    libroId));
                await cmdDetalle.ExecuteNonQueryAsync();
                using var cmdDesc = new SqlCommand(
                    "UPDATE Libros SET Ejemplares = Ejemplares - 1 WHERE LibroId = @Id", cn, tx);
                cmdDesc.Parameters.Add(new SqlParameter("@Id", libroId));
                await cmdDesc.ExecuteNonQueryAsync();
            }
            await tx.CommitAsync();
            return nuevoId;
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }
    public async Task RegistrarDevolucionAsync(int prestamoId, int libroId,
                                               DateTime fechaDevolucion, decimal multa)
    {
        const string sql = @"
            UPDATE DetallePrestamo
            SET FechaDevolucion = @Fecha, Multa = @Multa
            WHERE PrestamoId = @PrestamoId AND LibroId = @LibroId";
        using var cn  = ConexionFactory.Crear();
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.Add(new SqlParameter("@Fecha",      fechaDevolucion));
        cmd.Parameters.Add(new SqlParameter("@Multa",      multa));
        cmd.Parameters.Add(new SqlParameter("@PrestamoId", prestamoId));
        cmd.Parameters.Add(new SqlParameter("@LibroId",    libroId));
        await cn.OpenAsync();
        await cmd.ExecuteNonQueryAsync();
    }
    public async Task MarcarDevueltoAsync(int prestamoId)
    {
        const string sql = "UPDATE Prestamos SET Estado='Devuelto' WHERE PrestamoId=@Id";
        using var cn  = ConexionFactory.Crear();
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.Add(new SqlParameter("@Id", prestamoId));
        await cn.OpenAsync();
        await cmd.ExecuteNonQueryAsync();
    }
    private static async Task<List<Prestamo>> LeerTodosAsync(SqlCommand cmd)
    {
        var lista = new List<Prestamo>();
        await using var dr = await cmd.ExecuteReaderAsync();
        while (await dr.ReadAsync())
            lista.Add(new Prestamo {
                PrestamoId    = dr.GetInt32(0),
                SocioId       = dr.GetInt32(1),
                FechaPrestamo = dr.GetDateTime(2),
                FechaLimite   = dr.GetDateTime(3),
                Estado        = dr.GetString(4),
                NombreSocio   = dr.GetString(5)
            });
        return lista;
    }
}
