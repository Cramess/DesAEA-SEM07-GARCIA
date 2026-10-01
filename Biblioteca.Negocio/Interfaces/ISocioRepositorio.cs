using Biblioteca.Entidades;
namespace Biblioteca.Negocio.Interfaces;
public interface ISocioRepositorio
{
    Task<List<Socio>> ListarAsync();
    Task<List<Socio>> BuscarAsync(string texto);
    Task<Socio> ObtenerPorIdAsync(int socioId);
    Task<bool> ExisteDNIAsync(string dni, int excluirSocioId);
    Task<bool> TienePrestamoPendienteAsync(int socioId);
    Task InsertarAsync(Socio socio);
    Task ActualizarAsync(Socio socio);
    Task EliminarLogicoAsync(int socioId);
}
