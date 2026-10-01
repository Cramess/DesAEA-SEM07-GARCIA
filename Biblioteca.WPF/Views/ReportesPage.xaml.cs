using System.Windows;
using System.Windows.Controls;
using Biblioteca.Entidades;
using Biblioteca.Negocio.Interfaces;
using Biblioteca.WPF.Services;
namespace Biblioteca.WPF.Views;
public partial class ReportesPage : Page
{
    private readonly IPrestamoNegocio _prestamos = ServiceFactory.CrearPrestamoNegocio();
    public ReportesPage()
    {
        InitializeComponent();
        dpDesde.SelectedDate = DateTime.Today.AddDays(-30);
        dpHasta.SelectedDate = DateTime.Today;
        Loaded += async (_, _) => await GenerarReporteAsync();
    }
    private async Task GenerarReporteAsync()
    {
        if (!dpDesde.SelectedDate.HasValue || !dpHasta.SelectedDate.HasValue) return;
        DateTime desde = dpDesde.SelectedDate.Value;
        DateTime hasta = dpHasta.SelectedDate.Value;
        if (desde > hasta)
        {
            UiServices.Aviso("La fecha 'Desde' no puede ser mayor que 'Hasta'.");
            return;
        }
        await UiServices.RunAsync(async () =>
        {
            var lista = await _prestamos.ListarPorFechasAsync(desde, hasta);
            dgReporte.ItemsSource = lista;
            vacios.Visibility = lista.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }, busy => spinner.Visibility = busy ? Visibility.Visible : Visibility.Collapsed);
    }
    private async void btnGenerar_Click(object sender, RoutedEventArgs e)
    {
        await GenerarReporteAsync();
    }
}
