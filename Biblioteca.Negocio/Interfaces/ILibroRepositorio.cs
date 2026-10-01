using Biblioteca.Entidades;
namespace Biblioteca.Negocio.Interfaces;
public interface ILibroRepositorio
{
    Task<List<Libro>> ListarAsync();
    Task<List<Libro>> BuscarAsync(string texto);
    Task<Libro> ObtenerPorIdAsync(int libroId);
    Task<bool> ExisteISBNAsync(string isbn, int excluirLibroId);
    Task<bool> TienePrestamoPendienteAsync(int libroId);
    Task InsertarAsync(Libro libro);
    Task ActualizarAsync(Libro libro);
    Task EliminarLogicoAsync(int libroId);
    Task IncrementarEjemplarAsync(int libroId);
}
