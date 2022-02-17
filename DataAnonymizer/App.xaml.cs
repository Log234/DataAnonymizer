using System.Collections.Generic;
using DataAnonymizer.Utilities;
using Microsoft.UI.Xaml;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace DataAnonymizer
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            dataFilePath = "";
            keyFilePath = "";
            dataFileSavePath = "";
            keyFileSavePath = "";
            encryptKey = false;
            idDictionaryHandler = new IdDictionaryHandler();
            idDictionary = new Dictionary<string, string>();
            columnTypeDict = new Dictionary<string, (bool, ColumnTypes)>();
        }

        internal string dataFilePath;
        internal string keyFilePath;
        internal string dataFileSavePath;
        internal string keyFileSavePath;

        internal bool encryptKey;

        internal IdDictionaryHandler idDictionaryHandler;
        internal Dictionary<string, string> idDictionary;
        internal Dictionary<string, List<string>> data;
        internal Dictionary<string, (bool, ColumnTypes)> columnTypeDict;

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            m_window = new MainWindow();
            m_window.Activate();
        }

        internal Window m_window;
    }
}