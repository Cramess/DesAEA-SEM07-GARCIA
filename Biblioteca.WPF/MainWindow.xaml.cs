using System.Windows;
using Biblioteca.WPF.Services;
using Wpf.Ui.Appearance;
namespace Biblioteca.WPF;
public partial class MainWindow : Wpf.Ui.Controls.FluentWindow
{
    private bool _oscuro = true;
    public MainWindow()
    {
        InitializeComponent();
        ApplicationThemeManager.Apply(ApplicationTheme.Dark);
        UiServices.Inicializar(SnackbarPresenter, DialogHost);
    }
    private void btnTema_Click(object sender, RoutedEventArgs e)
    {
        _oscuro = !_oscuro;
        ApplicationThemeManager.Apply(_oscuro ? ApplicationTheme.Dark : ApplicationTheme.Light);
    }
}
