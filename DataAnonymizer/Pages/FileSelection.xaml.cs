﻿using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using System.IO;
using System.Text.Json;
using Anonymizer.Consts;
using Anonymizer.Extensions;
using Anonymizer.Utilities;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Anonymizer.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class FileSelection : Page
    {
        internal readonly App app;

        public FileSelection()
        {
            InitializeComponent();
            app = Application.Current as App;
            DataFilePath.Text = app.dataFilePath;
            KeyFilePath.Text = app.keyFilePath;
        }

        private async void SelectDataFile_ClickAsync(object sender, RoutedEventArgs e)
        {
            var filePath = await FilePickers.PickFileAsync(".csv");
            DataFilePath.Text = filePath;
            app.dataFilePath = filePath;
        }

        private async void SelectKeyFile_ClickAsync(object sender, RoutedEventArgs e)
        {
            var filePath = await FilePickers.PickFileAsync(".json");
            KeyFilePath.Text = filePath;
            app.keyFilePath = filePath;
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            var window = (MainWindow) app.m_window;

            if (!FileIsValid(app.dataFilePath))
            {
                window.AddMessage(new InfoBar
                {
                    Severity = InfoBarSeverity.Warning,
                    Title = $"Could not find a .csv file at: \"{app.dataFilePath}\"",
                    IsOpen = true
                });
                return;
            }

            var parseResult = CsvParser.ParseFile(app.dataFilePath);

            if (parseResult.IsFailure)
            {
                window.AddMessage(new InfoBar
                {
                    Severity = InfoBarSeverity.Error,
                    Title = $"An error occurred trying to load the data file: {parseResult.Error}",
                    IsOpen = true
                });
                return;
            }

            app.data = parseResult.Value;

            if (string.IsNullOrWhiteSpace(app.keyFilePath))
            {
                Frame.Navigate(typeof(ColumnTypeSelection), null, TransitionInfo.Default);
                return;
            }

            if (!FileIsValid(app.keyFilePath))
            {
                window.AddMessage(new InfoBar
                {
                    Severity = InfoBarSeverity.Warning,
                    Title = $"Could not find a key file at: \"{app.keyFilePath}\"",
                    IsOpen = true
                });
                return;
            }

            var isEncryptedResult = app.idDictionaryHandler.LoadIdDictionary(app.keyFilePath);

            if (isEncryptedResult.IsFailure)
            {
                window.AddMessage(new InfoBar
                {
                    Severity = InfoBarSeverity.Error,
                    Title = $"Failed to load the key file: {isEncryptedResult.Error}",
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
                app.idDictionaryHandler.GenerateNewEncryptionKey();
                var dictionaryResult = app.idDictionaryHandler.GetIdDictionary();

                if (dictionaryResult.IsFailure)
                {
                    window.AddMessage(new InfoBar
                    {
                        Severity = InfoBarSeverity.Error,
                        Title = $"Failed to read the key file content: {dictionaryResult.Error}",
                        IsOpen = true
                    });
                    return;
                }

                app.idDictionary = dictionaryResult.Value;

                Frame.Navigate(typeof(ColumnTypeSelection), null, TransitionInfo.Default);
            }
        }

        private static bool FileIsValid(string path)
        {
            return !string.IsNullOrWhiteSpace(path) && File.Exists(path);
        }
    }
}