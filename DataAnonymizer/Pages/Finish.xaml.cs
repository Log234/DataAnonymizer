using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace DataAnonymizer.Pages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class Finish : Page
{
    private readonly App _app;

    public Finish()
    {
        InitializeComponent();
        _app = (App)Application.Current;

        if (_app.encryptKey && !_app.idDictionaryHandler.KeyWasSet)
        {
            EncryptionInfo.Visibility = Visibility.Visible;
            EncryptionKey.Text = _app.idDictionaryHandler.GetEncryptionKey();
        }
    }
}