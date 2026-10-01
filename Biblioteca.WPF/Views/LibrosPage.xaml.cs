using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Biblioteca.Entidades;
using Biblioteca.Negocio.Interfaces;
using Biblioteca.WPF.Services;
namespace Biblioteca.WPF.Views;
public partial class LibrosPage : Page
{
    private readonly ILibroNegocio _negocio = ServiceFactory.CrearLibroNegocio();
    private int  _libroId   = 0;
    private bool _editando  = false;
    public LibrosPage()
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
            dgLibros.ItemsSource   = lista;
            vacios.Visibility      = lista.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            if (cmbAutor.Items.Count == 0)
                cmbAutor.ItemsSource = await _negocio.ListarAutoresAsync();
        }, busy => spinner.Visibility = busy ? Visibility.Visible : Visibility.Collapsed);
    }
    private void Limpiar()
    {
        _libroId = 0; _editando = false;
        txtISBN.Text  = txtTitulo.Text = string.Empty;
        cmbAutor.SelectedIndex = -1;
        numEjemplares.Value    = 1;
        lblForm.Text           = "Nuevo libro";
        btnEliminar.IsEnabled  = false;
        dgLibros.SelectedItem  = null;
    }
    private async Task GuardarAsync()
    {
        var libro = new Libro {
            LibroId    = _libroId,
            ISBN       = txtISBN.Text,
            Titulo     = txtTitulo.Text,
            AutorId    = (int)(cmbAutor.SelectedValue ?? 0),
            Ejemplares = (int)(numEjemplares.Value ?? 1)
        };
        bool ok = await UiServices.RunAsync(async () =>
        {
            if (_editando) await _negocio.ActualizarAsync(libro);
            else           await _negocio.RegistrarAsync(libro);
        }, busy => spinner.Visibility = busy ? Visibility.Visible : Visibility.Collapsed);
        if (ok) {
            UiServices.Ok(_editando ? "Libro actualizado." : "Libro registrado.");
            Limpiar();
            await CargarAsync();
        }
    }
    private async void txtBuscar_TextChanged(object sender, TextChangedEventArgs e) =>
        await CargarAsync(txtBuscar.Text);
    private async void btnRefrescar_Click(object sender, RoutedEventArgs e)
    {
        txtBuscar.Text = string.Empty;
        await CargarAsync();
    }
    private void dgLibros_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (dgLibros.SelectedItem is not Libro l) return;
        _libroId = l.LibroId; _editando = true;
        txtISBN.Text           = l.ISBN;
        txtTitulo.Text         = l.Titulo;
        cmbAutor.SelectedValue = l.AutorId;
        numEjemplares.Value    = l.Ejemplares;
        lblForm.Text           = $"Editar — {l.Titulo}";
        btnEliminar.IsEnabled  = true;
    }
    private void btnNuevo_Click(object sender, RoutedEventArgs e) => Limpiar();
    private async void btnGuardar_Click(object sender, RoutedEventArgs e) =>
        await GuardarAsync();
    private async void btnEliminar_Click(object sender, RoutedEventArgs e)
    {
        if (_libroId <= 0) return;
        if (!await UiServices.ConfirmarAsync("Eliminar libro",
            "Se dara de baja el libro seleccionado (eliminacion logica).")) return;
        bool ok = await UiServices.RunAsync(
            async () => await _negocio.EliminarAsync(_libroId),
            busy => spinner.Visibility = busy ? Visibility.Visible : Visibility.Collapsed);
        if (ok) { UiServices.Ok("Libro eliminado."); Limpiar(); await CargarAsync(); }
    }
}
