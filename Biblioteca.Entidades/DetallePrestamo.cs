namespace Biblioteca.Entidades;
public class DetallePrestamo
{
    public int PrestamoId { get; set; }
    public int LibroId { get; set; }
    public DateTime? FechaDevolucion { get; set; }
    public decimal? Multa { get; set; }
    public string TituloLibro { get; set; }
    public string ISBN { get; set; }
    public string EstadoDetalle => FechaDevolucion.HasValue ? "Devuelto" : "Pendiente";
    public string MultaTexto    => Multa.HasValue && Multa > 0 ? $"S/ {Multa:0.00}" : "—";
}
