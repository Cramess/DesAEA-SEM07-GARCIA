using Biblioteca.Entidades;
namespace Biblioteca.Negocio.Interfaces;
public interface IPrestamoNegocio
{
    int MaxLibrosPendientes { get; }
    int DiasPrestamo { get; }
    decimal MultaPorDia { get; }
    Task<List<Prestamo>> ListarAsync();
    Task<List<Prestamo>> ListarPorFechasAsync(DateTime desde, DateTime hasta);
    Task<Prestamo> ObtenerConDetallesAsync(int prestamoId);
    Task RegistrarAsync(Prestamo prestamo, List<int> librosIds);
    Task<decimal> RegistrarDevolucionAsync(int prestamoId, int libroId);
}
