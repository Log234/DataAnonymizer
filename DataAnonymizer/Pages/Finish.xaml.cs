using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace DataAnonymizer.Pages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class Finish : Page
{
    public Finish()
    {
        InitializeComponent();
        var app = (App)Application.Current;

        if (app.encryptKey && !app.idDictionaryHandler.KeyWasSet)
        {
            EncryptionInfo.Visibility = Visibility.Visible;
            EncryptionKey.Text = app.idDictionaryHandler.GetEncryptionKey();
        }
    }
}