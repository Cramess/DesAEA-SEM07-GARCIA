using Biblioteca.Entidades;
using Biblioteca.Negocio.Interfaces;
namespace Biblioteca.Negocio;
public class PrestamoNegocio : IPrestamoNegocio
{
    public const string EstadoPendiente = "Pendiente";
    public const string EstadoDevuelto  = "Devuelto";
    private readonly IPrestamoRepositorio _prestamos;
    private readonly ISocioRepositorio    _socios;
    private readonly ILibroRepositorio    _libros;
    public PrestamoNegocio(IPrestamoRepositorio prestamos,
                           ISocioRepositorio    socios,
                           ILibroRepositorio    libros)
    {
        _prestamos = prestamos;
        _socios    = socios;
        _libros    = libros;
    }
    public int     MaxLibrosPendientes => NegocioConfig.MaxLibrosPendientes;
    public int     DiasPrestamo        => NegocioConfig.DiasPrestamo;
    public decimal MultaPorDia         => NegocioConfig.MultaPorDia;
    public Task<List<Prestamo>> ListarAsync() => _prestamos.ListarAsync();
    public Task<List<Prestamo>> ListarPorFechasAsync(DateTime desde, DateTime hasta) =>
        _prestamos.ListarPorFechasAsync(desde, hasta);
    public async Task<Prestamo> ObtenerConDetallesAsync(int prestamoId)
    {
        var p = await _prestamos.ObtenerPorIdAsync(prestamoId)
            ?? throw new ReglaNegocioException("No se encontro el prestamo.");
        p.Detalles = await _prestamos.ListarDetallesAsync(prestamoId);
        return p;
    }
    public async Task RegistrarAsync(Prestamo prestamo, List<int> librosIds)
    {
        if (prestamo.SocioId <= 0)
            throw new ReglaNegocioException("Debe seleccionar un socio.");
        var socio = await _socios.ObtenerPorIdAsync(prestamo.SocioId);
        if (socio == null || !socio.Activo)
            throw new ReglaNegocioException("El socio seleccionado no existe o esta inactivo.");
        int pendientes = await _prestamos.ContarLibrosPendientesPorSocioAsync(prestamo.SocioId);
        if (pendientes + librosIds.Count > NegocioConfig.MaxLibrosPendientes)
            throw new ReglaNegocioException(
                $"El socio ya tiene {pendientes} libro(s) pendiente(s). " +
                $"El maximo es {NegocioConfig.MaxLibrosPendientes}. " +
                $"Solo puede tomar {NegocioConfig.MaxLibrosPendientes - pendientes} libro(s) mas.");
        if (librosIds == null || librosIds.Count == 0)
            throw new ReglaNegocioException("Debe seleccionar al menos un libro.");
        foreach (int libroId in librosIds)
        {
            var libro = await _libros.ObtenerPorIdAsync(libroId);
            if (libro == null || !libro.Activo)
                throw new ReglaNegocioException(
                    $"El libro con id {libroId} no existe o esta inactivo.");
            if (libro.Ejemplares <= 0)
                throw new ReglaNegocioException(
                    $"El libro '{libro.Titulo}' no tiene ejemplares disponibles.");
        }
        prestamo.FechaPrestamo = DateTime.Today;
        prestamo.FechaLimite   = DateTime.Today.AddDays(NegocioConfig.DiasPrestamo);
        prestamo.Estado        = EstadoPendiente;
        await _prestamos.RegistrarPrestamoTransaccionAsync(prestamo, librosIds);
    }
    public async Task<decimal> RegistrarDevolucionAsync(int prestamoId, int libroId)
    {
        if (prestamoId <= 0)
            throw new ReglaNegocioException("Debe seleccionar un prestamo.");
        if (libroId <= 0)
            throw new ReglaNegocioException("Debe seleccionar un libro a devolver.");
        var prestamo = await _prestamos.ObtenerPorIdAsync(prestamoId)
            ?? throw new ReglaNegocioException("Prestamo no encontrado.");
        DateTime hoy   = DateTime.Today;
        DateTime limite = prestamo.FechaLimite;
        decimal multa  = 0m;
        if (hoy > limite)
            multa = Math.Round((hoy - limite).Days * NegocioConfig.MultaPorDia, 2);
        await _prestamos.RegistrarDevolucionAsync(prestamoId, libroId, hoy, multa);
        await _libros.IncrementarEjemplarAsync(libroId);
        var detalles = await _prestamos.ListarDetallesAsync(prestamoId);
        if (detalles.All(d => d.FechaDevolucion.HasValue))
            await _prestamos.MarcarDevueltoAsync(prestamoId);
        return multa;
    }
}
