using DataAnonymizer.Consts;
using DataAnonymizer.Utilities;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.IO;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace DataAnonymizer.Pages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class FileSave : Page
{
    private readonly App _app;

    public FileSave()
    {
        InitializeComponent();
        _app = (App)Application.Current;
        DataFilePath.Text = _app.dataFileSavePath;
        KeyFilePath.Text = _app.keyFileSavePath;
    }

    private async void SaveDataFile_ClickAsync(object sender, RoutedEventArgs eventArgs)
    {
        var window = (MainWindow)_app.m_window;
        try
        {
            var filePath = await FilePickers.SaveFileAsync("CSV", ".csv");
            DataFilePath.Text = filePath;
            _app.dataFileSavePath = filePath;
        }
        catch (Exception e)
        {
            window.AddMessage(new InfoBar
            {
                Severity = InfoBarSeverity.Error,
                Title = $"An unexpected exception occurred: {ExceptionUtilities.AggregateMessages(e)}",
                IsOpen = true
            });
        }
    }

    private async void SaveKeyFile_ClickAsync(object sender, RoutedEventArgs eventArgs)
    {
        var window = (MainWindow)_app.m_window;
        try
        {
            var filePath = await FilePickers.SaveFileAsync("JSON", ".json");
            KeyFilePath.Text = filePath;
            _app.keyFileSavePath = filePath;
        }
        catch (Exception e)
        {
            window.AddMessage(new InfoBar
            {
                Severity = InfoBarSeverity.Error,
                Title = $"An unexpected exception occurred: {ExceptionUtilities.AggregateMessages(e)}",
                IsOpen = true
            });
        }
    }

    private void Previous_Click(object sender, RoutedEventArgs eventArgs)
    {
        var window = (MainWindow)_app.m_window;
        try
        {
            Frame.GoBack(TransitionInfo.Default);
        }
        catch (Exception e)
        {
            window.AddMessage(new InfoBar
            {
                Severity = InfoBarSeverity.Error,
                Title = $"An unexpected exception occurred: {ExceptionUtilities.AggregateMessages(e)}",
                IsOpen = true
            });
        }
    }

    private void Next_Click(object sender, RoutedEventArgs eventArgs)
    {
        var window = (MainWindow)_app.m_window;
        try
        {
            if (string.IsNullOrWhiteSpace(_app.dataFileSavePath))
            {
                window.AddMessage(new InfoBar
                {
                    Severity = InfoBarSeverity.Warning,
                    Title = "Please select where to save the data file.",
                    IsOpen = true
                });
                return;
            }

            if (string.IsNullOrWhiteSpace(_app.keyFileSavePath))
            {
                window.AddMessage(new InfoBar
                {
                    Severity = InfoBarSeverity.Warning,
                    Title = "Please select where to save the key file.",
                    IsOpen = true
                });
                return;
            }

            _app.encryptKey = ShouldEncrypt.IsOn;

            var dataDirPath = Path.GetDirectoryName(_app.dataFileSavePath);
            var keyDirPath = Path.GetDirectoryName(_app.keyFileSavePath);

            Directory.CreateDirectory(dataDirPath);
            Directory.CreateDirectory(keyDirPath);

            CsvWriter.Write(_app.data, _app.dataFileSavePath);

            _app.idDictionaryHandler.SaveDictionary(_app.idDictionary, _app.keyFileSavePath, _app.encryptKey);

            Frame.Navigate(typeof(Finish), null, TransitionInfo.Default);
        }
        catch (Exception e)
        {
            window.AddMessage(new InfoBar
            {
                Severity = InfoBarSeverity.Error,
                Title = $"An unexpected exception occurred: {ExceptionUtilities.AggregateMessages(e)}",
                IsOpen = true
            });
        }
    }
}