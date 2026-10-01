using Biblioteca.Entidades;
namespace Biblioteca.Negocio.Interfaces;
public interface IAutorRepositorio
{
    Task<List<Autor>> ListarAsync();
    Task<Autor> ObtenerPorIdAsync(int autorId);
    Task InsertarAsync(Autor autor);
    Task ActualizarAsync(Autor autor);
    Task EliminarLogicoAsync(int autorId);
}
