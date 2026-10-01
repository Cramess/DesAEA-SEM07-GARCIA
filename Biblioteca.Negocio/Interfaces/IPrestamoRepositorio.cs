using Biblioteca.Entidades;
namespace Biblioteca.Negocio.Interfaces;
public interface IPrestamoRepositorio
{
    Task<List<Prestamo>> ListarAsync();
    Task<List<Prestamo>> ListarPorFechasAsync(DateTime desde, DateTime hasta);
    Task<Prestamo> ObtenerPorIdAsync(int prestamoId);
    Task<int> ContarLibrosPendientesPorSocioAsync(int socioId);
    Task<List<DetallePrestamo>> ListarDetallesAsync(int prestamoId);
    Task<int> RegistrarPrestamoTransaccionAsync(Prestamo prestamo, List<int> librosIds);
    Task RegistrarDevolucionAsync(int prestamoId, int libroId,
                                  DateTime fechaDevolucion, decimal multa);
    Task MarcarDevueltoAsync(int prestamoId);
}
