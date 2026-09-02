using A2MahleApp.Application.Features.Export;

using Microsoft.Maui.Platform;

using Windows.Storage;
using Windows.Storage.Pickers;

using WinRT.Interop;

namespace A2MahleApp.Client.WinUI.Services;

public sealed class PdfFileSaver : IPdfFileSaver
{
    public async Task SaveAsync(
        byte[] content,
        string suggestedFileName)
    {
        ArgumentNullException.ThrowIfNull(content);

        if (string.IsNullOrWhiteSpace(suggestedFileName))
        {
            throw new ArgumentException(
                "O nome sugerido do arquivo não pode ser vazio.",
                nameof(suggestedFileName));
        }

        Microsoft.Maui.Controls.Window? mauiWindow =
            Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault();

        if (mauiWindow?.Handler?.PlatformView is not MauiWinUIWindow window)
        {
            throw new InvalidOperationException(
                "Não foi possível obter a janela principal do aplicativo.");
        }

        string extension = Path.GetExtension(suggestedFileName);
        if (string.IsNullOrWhiteSpace(extension))
        {
            extension = ".png";
        }

        FileSavePicker picker = new()
        {
            SuggestedFileName = Path.GetFileNameWithoutExtension(
                suggestedFileName),
            DefaultFileExtension = extension,
            SuggestedStartLocation = PickerLocationId.DocumentsLibrary
        };

        picker.FileTypeChoices.Add(
            extension switch
            {
                ".pdf" => "Arquivo PDF",
                ".png" => "Imagem PNG",
                ".jpg" or ".jpeg" => "Imagem JPEG",
                _ => "Arquivo"
            },
            [extension]);

        InitializeWithWindow.Initialize(
            picker,
            window.WindowHandle);

        StorageFile? file =
            await picker.PickSaveFileAsync();

        if (file is null)
        {
            return;
        }

        await File.WriteAllBytesAsync(
            file.Path,
            content);
    }
}
