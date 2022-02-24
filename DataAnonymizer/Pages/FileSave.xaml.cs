using System.IO;
using DataAnonymizer.Consts;
using DataAnonymizer.Utilities;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace DataAnonymizer.Pages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class FileSave : Page
{
    internal readonly App app;

    public FileSave()
    {
        InitializeComponent();
        app = Application.Current as App;
        DataFilePath.Text = app.dataFileSavePath;
        KeyFilePath.Text = app.keyFileSavePath;
    }

    private async void SaveDataFile_ClickAsync(object sender, RoutedEventArgs e)
    {
        var filePath = await FilePickers.SaveFileAsync("CSV", ".csv");
        DataFilePath.Text = filePath;
        app.dataFileSavePath = filePath;
    }

    private async void SaveKeyFile_ClickAsync(object sender, RoutedEventArgs e)
    {
        var filePath = await FilePickers.SaveFileAsync("JSON", ".json");
        KeyFilePath.Text = filePath;
        app.keyFileSavePath = filePath;
    }

    private void Previous_Click(object sender, RoutedEventArgs e)
    {
        Frame.GoBack(TransitionInfo.Default);
    }

    private void Next_Click(object sender, RoutedEventArgs e)
    {
        var window = (MainWindow)app.m_window;

        if (string.IsNullOrWhiteSpace(app.dataFileSavePath))
        {
            window.AddMessage(new InfoBar
            {
                Severity = InfoBarSeverity.Warning,
                Title = "Please select where to save the data file.",
                IsOpen = true
            });
            return;
        }

        if (string.IsNullOrWhiteSpace(app.keyFileSavePath))
        {
            window.AddMessage(new InfoBar
            {
                Severity = InfoBarSeverity.Warning,
                Title = "Please select where to save the key file.",
                IsOpen = true
            });
            return;
        }

        app.encryptKey = ShouldEncrypt.IsOn;

        var dataDirPath = Path.GetDirectoryName(app.dataFileSavePath);
        var keyDirPath = Path.GetDirectoryName(app.keyFileSavePath);

        Directory.CreateDirectory(dataDirPath);
        Directory.CreateDirectory(keyDirPath);

        CsvWriter.Write(app.data, app.dataFileSavePath);

        app.idDictionaryHandler.SaveDictionary(app.idDictionary, app.keyFileSavePath, app.encryptKey);

        Frame.Navigate(typeof(Finish), null, TransitionInfo.Default);
    }
}