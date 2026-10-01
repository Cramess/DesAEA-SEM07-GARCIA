using Biblioteca.Datos;
using Biblioteca.Negocio;
using Biblioteca.Negocio.Interfaces;
namespace Biblioteca.WPF.Services;
public static class ServiceFactory
{
    public static ILibroNegocio CrearLibroNegocio() =>
        new LibroNegocio(new LibroRepositorio(), new AutorRepositorio());
    public static ISocioNegocio CrearSocioNegocio() =>
        new SocioNegocio(new SocioRepositorio());
    public static IPrestamoNegocio CrearPrestamoNegocio() =>
        new PrestamoNegocio(new PrestamoRepositorio(),
                            new SocioRepositorio(),
                            new LibroRepositorio());
}
