using DataAnonymizer.Consts;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using DataAnonymizer.Utilities;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace DataAnonymizer.Pages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class EncryptionKey : Page
{
    private readonly App _app;

    public EncryptionKey()
    {
        InitializeComponent();
        _app = Application.Current as App;
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
            if (string.IsNullOrWhiteSpace(Key.Text))
            {
                window.AddMessage(new InfoBar
                {
                    Severity = InfoBarSeverity.Warning,
                    Title = "Please enter the encryption key before proceeding.",
                    IsOpen = true
                });
                return;
            }

            _app.idDictionaryHandler.SetEncryptionKey(Key.Text);

            var idDictionaryResult = _app.idDictionaryHandler.GetIdDictionary();

            if (idDictionaryResult.IsFailure)
            {
                window.AddMessage(new InfoBar
                {
                    Severity = InfoBarSeverity.Warning,
                    Title = $"Was unable to decrypt the key file: {idDictionaryResult.Error}.",
                    IsOpen = true
                });
                return;
            }

            _app.idDictionary = idDictionaryResult.Value;

            Frame.Navigate(typeof(ColumnTypeSelection), null, TransitionInfo.Default);
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