using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Biblioteca.Entidades;
using Biblioteca.Negocio.Interfaces;
using Biblioteca.WPF.Services;
namespace Biblioteca.WPF.Views;
public partial class SociosPage : Page
{
    private readonly ISocioNegocio _negocio = ServiceFactory.CrearSocioNegocio();
    private int  _socioId  = 0;
    private bool _editando = false;
    public SociosPage()
    {
        InitializeComponent();
        Loaded  += async (_, _) => await CargarAsync();
        KeyDown += (_, e) =>
        {
            if (e.Key == Key.N && Keyboard.Modifiers == ModifierKeys.Control) Limpiar();
            if (e.Key == Key.S && Keyboard.Modifiers == ModifierKeys.Control) _ = GuardarAsync();
            if (e.Key == Key.Escape) Limpiar();
        };
    }
    private async Task CargarAsync(string busqueda = null)
    {
        await UiServices.RunAsync(async () =>
        {
            var lista = string.IsNullOrWhiteSpace(busqueda)
                ? await _negocio.ListarAsync()
                : await _negocio.BuscarAsync(busqueda);
            dgSocios.ItemsSource = lista;
            vacios.Visibility    = lista.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }, busy => spinner.Visibility = busy ? Visibility.Visible : Visibility.Collapsed);
    }
    private void Limpiar()
    {
        _socioId = 0; _editando = false;
        txtDNI.Text = txtNombre.Text = txtEmail.Text = string.Empty;
        lblForm.Text          = "Nuevo socio";
        btnEliminar.IsEnabled = false;
        dgSocios.SelectedItem = null;
    }
    private async Task GuardarAsync()
    {
        var socio = new Socio {
            SocioId = _socioId,
            DNI     = txtDNI.Text,
            Nombre  = txtNombre.Text,
            Email   = string.IsNullOrWhiteSpace(txtEmail.Text) ? null : txtEmail.Text
        };
        bool ok = await UiServices.RunAsync(async () =>
        {
            if (_editando) await _negocio.ActualizarAsync(socio);
            else           await _negocio.RegistrarAsync(socio);
        }, busy => spinner.Visibility = busy ? Visibility.Visible : Visibility.Collapsed);
        if (ok) {
            UiServices.Ok(_editando ? "Socio actualizado." : "Socio registrado.");
            Limpiar(); await CargarAsync();
        }
    }
    private async void txtBuscar_TextChanged(object sender, TextChangedEventArgs e) =>
        await CargarAsync(txtBuscar.Text);
    private async void btnRefrescar_Click(object sender, RoutedEventArgs e)
    {
        txtBuscar.Text = string.Empty;
        await CargarAsync();
    }
    private void dgSocios_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (dgSocios.SelectedItem is not Socio s) return;
        _socioId = s.SocioId; _editando = true;
        txtDNI.Text   = s.DNI;
        txtNombre.Text = s.Nombre;
        txtEmail.Text = s.Email ?? string.Empty;
        lblForm.Text  = $"Editar — {s.Nombre}";
        btnEliminar.IsEnabled = true;
    }
    private void btnNuevo_Click(object sender, RoutedEventArgs e) => Limpiar();
    private async void btnGuardar_Click(object sender, RoutedEventArgs e) =>
        await GuardarAsync();
    private async void btnEliminar_Click(object sender, RoutedEventArgs e)
    {
        if (_socioId <= 0) return;
        if (!await UiServices.ConfirmarAsync("Eliminar socio",
            "Se dara de baja el socio. Debe no tener prestamos pendientes.")) return;
        bool ok = await UiServices.RunAsync(
            async () => await _negocio.EliminarAsync(_socioId),
            busy => spinner.Visibility = busy ? Visibility.Visible : Visibility.Collapsed);
        if (ok) { UiServices.Ok("Socio eliminado."); Limpiar(); await CargarAsync(); }
    }
}
