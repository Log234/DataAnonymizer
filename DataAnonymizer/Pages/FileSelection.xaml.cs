using DataAnonymizer.Consts;
using DataAnonymizer.Utilities;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.IO;
using System.Linq;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace DataAnonymizer.Pages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class FileSelection : Page
{
    private readonly App _app;

    public FileSelection()
    {
        InitializeComponent();
        _app = (App)Application.Current;
        DataFilePath.Text = _app.dataFilePath;
        KeyFilePath.Text = _app.keyFilePath;
    }

    private async void SelectDataFile_ClickAsync(object sender, RoutedEventArgs eventArgs)
    {
        var window = (MainWindow)_app.m_window;
        try
        {
            var filePath = await FilePickers.PickFileAsync(".csv");
            DataFilePath.Text = filePath;
            _app.dataFilePath = filePath;
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

    private async void SelectKeyFile_ClickAsync(object sender, RoutedEventArgs eventArgs)
    {
        var window = (MainWindow)_app.m_window;
        try
        {
            var filePath = await FilePickers.PickFileAsync(".json");
            KeyFilePath.Text = filePath;
            _app.keyFilePath = filePath;
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
            if (!FileIsValid(_app.dataFilePath))
            {
                window.AddMessage(new InfoBar
                {
                    Severity = InfoBarSeverity.Warning,
                    Title = $"Could not find a .csv file at: \"{_app.dataFilePath}\"",
                    IsOpen = true
                });
                return;
            }

            var parseResult = CsvParser.ParseFile(_app.dataFilePath);

            if (parseResult.IsFailure)
            {
                window.AddMessage(new InfoBar
                {
                    Severity = InfoBarSeverity.Error,
                    Title = $"An error occurred trying to load the data file:\n{parseResult.Error}",
                    IsOpen = true
                });
                return;
            }

            if (!parseResult.Value.Any())
            {
                window.AddMessage(new InfoBar
                {
                    Severity = InfoBarSeverity.Warning,
                    Title = "It seems the data file does not contain any columns. Check the document formatting.",
                    IsOpen = true
                });
                return;
            }

            if (parseResult.Value.Any(column => column.Count == 0))
            {
                window.AddMessage(new InfoBar
                {
                    Severity = InfoBarSeverity.Warning,
                    Title = "It seems the data file does not contain any rows. Check the document formatting.",
                    IsOpen = true
                });
                return;
            }

            if (parseResult.Value.Any(column => column.Count == 1))
            {
                window.AddMessage(new InfoBar
                {
                    Severity = InfoBarSeverity.Warning,
                    Title = "It seems the data file only contains a header row, no actual data. Check the document formatting.",
                    IsOpen = true
                });
                return;
            }

            _app.data = parseResult.Value;
            _app.columnTypeDict = new (bool, ColumnTypes)[parseResult.Value.Count];

            if (string.IsNullOrWhiteSpace(_app.keyFilePath))
            {
                Frame.Navigate(typeof(ColumnTypeSelection), null, TransitionInfo.Default);
                return;
            }

            if (!FileIsValid(_app.keyFilePath))
            {
                window.AddMessage(new InfoBar
                {
                    Severity = InfoBarSeverity.Warning,
                    Title = $"Could not find a key file at: \"{_app.keyFilePath}\"",
                    IsOpen = true
                });
                return;
            }

            var isEncryptedResult = _app.idDictionaryHandler.LoadIdDictionary(_app.keyFilePath);

            if (isEncryptedResult.IsFailure)
            {
                window.AddMessage(new InfoBar
                {
                    Severity = InfoBarSeverity.Error,
                    Title = $"Failed to load the key file:\n{isEncryptedResult.Error}",
                    IsOpen = true
                });
                return;
            }

            if (isEncryptedResult.Value)
            {
                Frame.Navigate(typeof(EncryptionKey), null, TransitionInfo.Default);
            }
            else
            {
                _app.idDictionaryHandler.GenerateNewEncryptionKey();
                var dictionaryResult = _app.idDictionaryHandler.GetIdDictionary();

                if (dictionaryResult.IsFailure)
                {
                    window.AddMessage(new InfoBar
                    {
                        Severity = InfoBarSeverity.Error,
                        Title = $"Failed to read the key file content:\n{dictionaryResult.Error}",
                        IsOpen = true
                    });
                    return;
                }

                _app.idDictionary = dictionaryResult.Value;

                Frame.Navigate(typeof(ColumnTypeSelection), null, TransitionInfo.Default);
            }
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

    private static bool FileIsValid(string path)
    {
        return !string.IsNullOrWhiteSpace(path) && File.Exists(path);
    }
}