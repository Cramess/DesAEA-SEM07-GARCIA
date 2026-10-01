using Biblioteca.Entidades;
namespace Biblioteca.Negocio.Interfaces;
public interface ISocioNegocio
{
    Task<List<Socio>> ListarAsync();
    Task<List<Socio>> BuscarAsync(string texto);
    Task RegistrarAsync(Socio socio);
    Task ActualizarAsync(Socio socio);
    Task EliminarAsync(int socioId);
}
