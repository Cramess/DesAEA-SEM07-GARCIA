using System.Configuration;
using Microsoft.Data.SqlClient;
namespace Biblioteca.Datos;
public static class ConexionFactory
{
    public static SqlConnection Crear()
    {
        string cadena = ConfigurationManager.ConnectionStrings["BibliotecaDB"]?.ConnectionString
            ?? throw new InvalidOperationException(
                "No se encontro la cadena de conexion 'BibliotecaDB' en App.config.");
        return new SqlConnection(cadena);
    }
}
