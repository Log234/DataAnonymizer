using DataAnonymizer.Consts;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace DataAnonymizer.Pages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class EncryptionKey : Page
{
    internal readonly App app;

    public EncryptionKey()
    {
        InitializeComponent();
        app = Application.Current as App;
    }

    private void Previous_Click(object sender, RoutedEventArgs e)
    {
        Frame.GoBack(TransitionInfo.Default);
    }

    private void Next_Click(object sender, RoutedEventArgs e)
    {
        var window = (MainWindow)app.m_window;

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

        app.idDictionaryHandler.SetEncryptionKey(Key.Text);

        var idDictionaryResult = app.idDictionaryHandler.GetIdDictionary();

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

        app.idDictionary = idDictionaryResult.Value;

        Frame.Navigate(typeof(ColumnTypeSelection), null, TransitionInfo.Default);
    }
}
