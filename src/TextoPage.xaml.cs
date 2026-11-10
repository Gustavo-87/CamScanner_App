namespace CamScanner;

public partial class TextoPage : ContentPage
{
    /// <summary>Se dispara al guardar, con el texto ya corregido por el usuario.</summary>
    public event Action<string>? Guardado;

    public TextoPage(string nombreDocumento, string texto)
    {
        InitializeComponent();
        NombreLabel.Text = nombreDocumento;
        TextoEditor.Text = texto;
    }

    /// <summary>Copia la selección; si no hay nada seleccionado, copia todo el texto.</summary>
    private async void OnCopiarClicked(object? sender, EventArgs e)
    {
        var texto = TextoEditor.Text ?? string.Empty;
        if (string.IsNullOrEmpty(texto))
            return;

        var inicio = TextoEditor.CursorPosition;
        var largo = TextoEditor.SelectionLength;
        var haySeleccion = largo > 0 && inicio >= 0 && inicio + largo <= texto.Length;

        await Clipboard.Default.SetTextAsync(haySeleccion ? texto.Substring(inicio, largo) : texto);
        await DisplayAlertAsync("Copiado", haySeleccion ? "Se copió el texto seleccionado." : "Se copió todo el texto.", "OK");
    }

    private async void OnGuardarClicked(object? sender, EventArgs e)
    {
        Guardado?.Invoke(TextoEditor.Text ?? string.Empty);
        await Navigation.PopAsync();
    }
}