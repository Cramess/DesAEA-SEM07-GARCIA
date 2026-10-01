using Biblioteca.Entidades;
namespace Biblioteca.Negocio.Interfaces;
public interface ILibroNegocio
{
    Task<List<Libro>> ListarAsync();
    Task<List<Libro>> BuscarAsync(string texto);
    Task<List<Autor>> ListarAutoresAsync();
    Task RegistrarAsync(Libro libro);
    Task ActualizarAsync(Libro libro);
    Task EliminarAsync(int libroId);
}
