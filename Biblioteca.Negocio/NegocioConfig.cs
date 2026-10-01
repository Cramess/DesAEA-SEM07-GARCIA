using System.Configuration;
using System.Globalization;
namespace Biblioteca.Negocio;
public static class NegocioConfig
{
    public static int     MaxLibrosPendientes { get; } = LeerInt("MaxPrestamosPorSocio", 3);
    public static int     DiasPrestamo        { get; } = LeerInt("DiasPrestamo", 15);
    public static decimal MultaPorDia         { get; } = LeerDecimal("MultaPorDia", 1.50m);
    private static int LeerInt(string clave, int defecto)
    {
        string v = ConfigurationManager.AppSettings[clave];
        return int.TryParse(v, out int r) ? r : defecto;
    }
    private static decimal LeerDecimal(string clave, decimal defecto)
    {
        string v = ConfigurationManager.AppSettings[clave];
        return decimal.TryParse(v, NumberStyles.Number,
               CultureInfo.InvariantCulture, out decimal r) ? r : defecto;
    }
}
