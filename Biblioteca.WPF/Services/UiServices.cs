using Biblioteca.Negocio;
using Wpf.Ui;
using Wpf.Ui.Controls;
namespace Biblioteca.WPF.Services;
public static class UiServices
{
    private static readonly SnackbarService    _snackbar = new();
    private static readonly ContentDialogService _dialogs = new();
    public static void Inicializar(SnackbarPresenter snackbar, ContentDialogHost dialogHost)
    {
        _snackbar.SetSnackbarPresenter(snackbar);
        _dialogs.SetDialogHost(dialogHost);
    }
    public static void Ok(string msg) =>
        _snackbar.Show("Listo", msg, ControlAppearance.Success,
            null, TimeSpan.FromSeconds(3));
    public static void Aviso(string msg) =>
        _snackbar.Show("Aviso", msg, ControlAppearance.Caution,
            null, TimeSpan.FromSeconds(4));
    public static void Error(string msg) =>
        _snackbar.Show("Error", msg, ControlAppearance.Danger,
            null, TimeSpan.FromSeconds(6));
    public static async Task<bool> ConfirmarAsync(string titulo, string detalle,
        string boton = "Confirmar")
    {
        var dlg = new ContentDialog {
            Title                   = titulo,
            Content                 = detalle,
            PrimaryButtonText       = boton,
            PrimaryButtonAppearance = ControlAppearance.Danger,
            CloseButtonText         = "Cancelar"
        };
        return await _dialogs.ShowAsync(dlg, CancellationToken.None) == ContentDialogResult.Primary;
    }
    public static async Task<bool> RunAsync(Func<Task> accion,
        Action<bool> setBusy = null)
    {
        try
        {
            setBusy?.Invoke(true);
            await accion();
            return true;
        }
        catch (ReglaNegocioException ex) { Aviso(ex.Message); }
        catch (Exception ex)             { Error(ex.Message); }
        finally { setBusy?.Invoke(false); }
        return false;
    }
}
