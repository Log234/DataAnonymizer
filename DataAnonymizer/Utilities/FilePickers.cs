using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using WinRT;

namespace Anonymizer.Extensions;

public class FilePickers
{
    public static async Task<string> PickFileAsync(string filter)
    {
        var filePicker = new FileOpenPicker();

        var initializedWithWindow = filePicker.As<IInitializeWithWindow>();
        initializedWithWindow.Initialize(MainWindow.hwnd);

        filePicker.FileTypeFilter.Add(filter);

        var file = await filePicker.PickSingleFileAsync();
        return file?.Path ?? string.Empty;
    }

    public static async Task<string> SaveFileAsync(string extensionName, string extension)
    {
        var filePicker = new FileSavePicker();
        filePicker.FileTypeChoices.Add(extensionName, new List<string> { extension });

        var initializedWithWindow = filePicker.As<IInitializeWithWindow>();
        initializedWithWindow.Initialize(MainWindow.hwnd);

        var file = await filePicker.PickSaveFileAsync();
        return file?.Path ?? string.Empty;
    }

    [ComImport]
    [Guid("3E68D4BD-7135-4D10-8018-9FB6D9F33FA1")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IInitializeWithWindow
    {
        void Initialize(IntPtr hwnd);
    }
}
