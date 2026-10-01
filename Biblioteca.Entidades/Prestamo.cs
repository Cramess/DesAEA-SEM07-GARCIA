namespace Biblioteca.Entidades;
public class Prestamo
{
    public int PrestamoId { get; set; }
    public int SocioId { get; set; }
    public DateTime FechaPrestamo { get; set; }
    public DateTime FechaLimite { get; set; }
    public string Estado { get; set; }
    public string NombreSocio { get; set; }
    public List<DetallePrestamo> Detalles { get; set; } = new();
    public string ResumenLibros => Detalles.Count > 0 
        ? string.Join(" | ", Detalles.Select(d => d.TituloLibro)) 
        : "—";
}
