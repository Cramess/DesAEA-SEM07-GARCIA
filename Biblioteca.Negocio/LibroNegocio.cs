using System.Net.Mail;
using Biblioteca.Entidades;
using Biblioteca.Negocio.Interfaces;
namespace Biblioteca.Negocio;
public class LibroNegocio : ILibroNegocio
{
    private readonly ILibroRepositorio _libros;
    private readonly IAutorRepositorio _autores;
    public LibroNegocio(ILibroRepositorio libros, IAutorRepositorio autores)
    {
        _libros  = libros;
        _autores = autores;
    }
    public Task<List<Libro>> ListarAsync() => _libros.ListarAsync();
    public Task<List<Libro>> BuscarAsync(string texto) =>
        string.IsNullOrWhiteSpace(texto) ? _libros.ListarAsync() : _libros.BuscarAsync(texto.Trim());
    public Task<List<Autor>> ListarAutoresAsync() => _autores.ListarAsync();
    public async Task RegistrarAsync(Libro libro)
    {
        await ValidarAsync(libro);
        await _libros.InsertarAsync(libro);
    }
    public async Task ActualizarAsync(Libro libro)
    {
        if (libro.LibroId <= 0)
            throw new ReglaNegocioException("Debe seleccionar un libro para editar.");
        await ValidarAsync(libro);
        await _libros.ActualizarAsync(libro);
    }
    public async Task EliminarAsync(int libroId)
    {
        if (libroId <= 0)
            throw new ReglaNegocioException("Debe seleccionar un libro.");
        if (await _libros.TienePrestamoPendienteAsync(libroId))
            throw new ReglaNegocioException(
                "No se puede eliminar un libro con prestamos pendientes.");
        await _libros.EliminarLogicoAsync(libroId);
    }
    private async Task ValidarAsync(Libro libro)
    {
        libro.Titulo = libro.Titulo?.Trim();
        libro.ISBN   = libro.ISBN?.Trim();
        if (string.IsNullOrWhiteSpace(libro.Titulo))
            throw new ReglaNegocioException("El titulo del libro es obligatorio.");
        if (libro.Titulo.Length > 200)
            throw new ReglaNegocioException("El titulo no puede superar los 200 caracteres.");
        if (string.IsNullOrWhiteSpace(libro.ISBN))
            throw new ReglaNegocioException("El ISBN es obligatorio.");
        if (libro.ISBN.Length > 20)
            throw new ReglaNegocioException("El ISBN no puede superar los 20 caracteres.");
        if (libro.AutorId <= 0 || await _autores.ObtenerPorIdAsync(libro.AutorId) == null)
            throw new ReglaNegocioException("Debe seleccionar un autor valido.");
        if (libro.Ejemplares < 1)
            throw new ReglaNegocioException("El numero de ejemplares debe ser al menos 1.");
        if (await _libros.ExisteISBNAsync(libro.ISBN, libro.LibroId))
            throw new ReglaNegocioException($"Ya existe un libro con ISBN '{libro.ISBN}'.");
    }
}
