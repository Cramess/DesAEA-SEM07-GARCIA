using System.Windows;
using System.Windows.Controls;
using Biblioteca.Entidades;
using Biblioteca.Negocio.Interfaces;
using Biblioteca.WPF.Services;
namespace Biblioteca.WPF.Views;
public partial class PrestamosPage : Page
{
    private readonly IPrestamoNegocio _prestamos = ServiceFactory.CrearPrestamoNegocio();
    private readonly ISocioNegocio    _socios    = ServiceFactory.CrearSocioNegocio();
    private readonly ILibroNegocio    _libros    = ServiceFactory.CrearLibroNegocio();
    private int _prestamoId = 0;
    private int _libroId    = 0;
    public PrestamosPage()
    {
        InitializeComponent();
        Loaded += async (_, _) => await CargarAsync();
    }
    private async Task CargarAsync()
    {
        await UiServices.RunAsync(async () =>
        {
            var lista = await _prestamos.ListarAsync();
            dgPrestamos.ItemsSource = lista;
            vacios.Visibility = lista.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            cmbSocio.ItemsSource  = await _socios.ListarAsync();
            lstLibros.ItemsSource = (await _libros.ListarAsync()).Where(l => l.Ejemplares > 0).ToList();
        }, busy => spinner.Visibility = busy ? Visibility.Visible : Visibility.Collapsed);
    }
    private async Task CargarDetalleAsync(int prestamoId)
    {
        await UiServices.RunAsync(async () =>
        {
            var p = await _prestamos.ObtenerConDetallesAsync(prestamoId);
            dgDetalle.ItemsSource = p.Detalles;
            lblDetalle.Text = $"Detalle — {p.NombreSocio}";
        });
    }
    private async void dgPrestamos_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (dgPrestamos.SelectedItem is not Prestamo p) return;
        _prestamoId = p.PrestamoId;
        _libroId    = 0;
        btnDevolver.IsEnabled = false;
        await CargarDetalleAsync(p.PrestamoId);
    }
    private void dgDetalle_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (dgDetalle.SelectedItem is not DetallePrestamo d) return;
        _libroId = d.LibroId;
        btnDevolver.IsEnabled = !d.FechaDevolucion.HasValue;
    }
    private async void btnRefrescar_Click(object sender, RoutedEventArgs e)
    {
        dgDetalle.ItemsSource = null;
        lblDetalle.Text = "Detalle del prestamo";
        btnDevolver.IsEnabled = false;
        await CargarAsync();
    }
    private async void btnRegistrar_Click(object sender, RoutedEventArgs e)
    {
        if (cmbSocio.SelectedValue is not int socioId) {
            UiServices.Aviso("Selecciona un socio."); return;
        }
        var librosIds = lstLibros.SelectedItems.Cast<Libro>().Select(l => l.LibroId).ToList();
        if (librosIds.Count == 0) {
            UiServices.Aviso("Selecciona al menos un libro."); return;
        }
        bool ok = await UiServices.RunAsync(async () =>
        {
            await _prestamos.RegistrarAsync(new Prestamo { SocioId = socioId }, librosIds);
        }, busy => spinner.Visibility = busy ? Visibility.Visible : Visibility.Collapsed);
        if (ok) {
            UiServices.Ok($"Prestamo por {_prestamos.DiasPrestamo} dias registrado.");
            cmbSocio.SelectedIndex = -1;
            lstLibros.SelectedItems.Clear();
            await CargarAsync();
        }
    }
    private async void btnDevolver_Click(object sender, RoutedEventArgs e)
    {
        if (_prestamoId <= 0 || _libroId <= 0) return;
        if (!await UiServices.ConfirmarAsync("Devolucion", "Se registrara la devolucion del libro seleccionado.")) return;
        bool ok = await UiServices.RunAsync(async () =>
        {
            decimal multa = await _prestamos.RegistrarDevolucionAsync(_prestamoId, _libroId);
            if (multa > 0)
                UiServices.Aviso($"Devolucion registrada. Se ha generado una multa de S/ {multa:0.00} por retraso.");
            else
                UiServices.Ok("Devolucion registrada sin multa.");
        }, busy => spinner.Visibility = busy ? Visibility.Visible : Visibility.Collapsed);
        if (ok) {
            btnDevolver.IsEnabled = false;
            await CargarDetalleAsync(_prestamoId);
            dgPrestamos.ItemsSource = await _prestamos.ListarAsync();
        }
    }
}
