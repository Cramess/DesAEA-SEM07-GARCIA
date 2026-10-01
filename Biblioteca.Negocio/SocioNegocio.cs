using System.Net.Mail;
using Biblioteca.Entidades;
using Biblioteca.Negocio.Interfaces;
namespace Biblioteca.Negocio;
public class SocioNegocio : ISocioNegocio
{
    private readonly ISocioRepositorio _socios;
    public SocioNegocio(ISocioRepositorio socios)
    {
        _socios = socios;
    }
    public Task<List<Socio>> ListarAsync() => _socios.ListarAsync();
    public Task<List<Socio>> BuscarAsync(string texto) =>
        string.IsNullOrWhiteSpace(texto) ? _socios.ListarAsync() : _socios.BuscarAsync(texto.Trim());
    public async Task RegistrarAsync(Socio socio)
    {
        Validar(socio);
        if (await _socios.ExisteDNIAsync(socio.DNI, 0))
            throw new ReglaNegocioException($"Ya existe un socio con DNI '{socio.DNI}'.");
        await _socios.InsertarAsync(socio);
    }
    public async Task ActualizarAsync(Socio socio)
    {
        if (socio.SocioId <= 0)
            throw new ReglaNegocioException("Debe seleccionar un socio para editar.");
        Validar(socio);
        if (await _socios.ExisteDNIAsync(socio.DNI, socio.SocioId))
            throw new ReglaNegocioException($"Ya existe un socio con DNI '{socio.DNI}'.");
        await _socios.ActualizarAsync(socio);
    }
    public async Task EliminarAsync(int socioId)
    {
        if (socioId <= 0)
            throw new ReglaNegocioException("Debe seleccionar un socio.");
        if (await _socios.TienePrestamoPendienteAsync(socioId))
            throw new ReglaNegocioException(
                "No se puede eliminar un socio con prestamos pendientes.");
        await _socios.EliminarLogicoAsync(socioId);
    }
    private static void Validar(Socio socio)
    {
        socio.DNI    = socio.DNI?.Trim().ToUpperInvariant();
        socio.Nombre = socio.Nombre?.Trim();
        socio.Email  = string.IsNullOrWhiteSpace(socio.Email) ? null : socio.Email.Trim();
        if (string.IsNullOrWhiteSpace(socio.DNI))
            throw new ReglaNegocioException("El DNI es obligatorio.");
        if (socio.DNI.Length > 15)
            throw new ReglaNegocioException("El DNI no puede superar los 15 caracteres.");
        if (string.IsNullOrWhiteSpace(socio.Nombre))
            throw new ReglaNegocioException("El nombre del socio es obligatorio.");
        if (socio.Nombre.Length > 100)
            throw new ReglaNegocioException("El nombre no puede superar los 100 caracteres.");
        if (socio.Email != null && !EsEmailValido(socio.Email))
            throw new ReglaNegocioException("El formato del email no es valido.");
    }
    private static bool EsEmailValido(string email) =>
        MailAddress.TryCreate(email, out var addr) && addr.Address == email;
}
