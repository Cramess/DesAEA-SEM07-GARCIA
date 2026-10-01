using System.Windows;
using Wpf.Ui.Appearance;
namespace Biblioteca.WPF;
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        ApplicationThemeManager.Apply(ApplicationTheme.Light);
    }
}
