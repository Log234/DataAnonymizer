using System.Linq;
using DataAnonymizer.Consts;
using DataAnonymizer.Controls;
using DataAnonymizer.Utilities;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace DataAnonymizer.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ColumnTypeSelection : Page
    {
        private App app;

        public ColumnTypeSelection()
        {
            this.InitializeComponent();
            app = Application.Current as App;
            this.ColumnContainer.Children.Clear();

            foreach (var columnName in app.data.Keys)
            {
                var example = "";

                if (app.data.ContainsKey(columnName))
                    example = app.data[columnName].FirstOrDefault() ?? "";

                ColumnContainer.Children.Add(new DataTypeSelector(columnName, example.Trim()));
            }
        }

        private void Previous_Click(object sender, RoutedEventArgs e)
        {
            StoreSelection();
            Frame.GoBack(TransitionInfo.Default);
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            var window = (MainWindow)app.m_window;

            StoreSelection();
            var cleaningResult = DataCleaner.Clean(app.data, app.columnTypeDict, app.idDictionary);

            if (cleaningResult.IsFailure)
            {
                window.AddMessage(new InfoBar
                {
                    Severity = InfoBarSeverity.Error,
                    Title = $"Failed to clean the data: {cleaningResult.Error}",
                    IsOpen = true
                });
                return;
            }

            app.data = cleaningResult.Value;



            Frame.Navigate(typeof(FileSave), null, TransitionInfo.Default);
        }

        private void StoreSelection()
        {
            foreach (DataTypeSelector column in ColumnContainer.Children)
            {
                app.columnTypeDict[column.ColumnName] = (column.ShouldAnonymize, column.ColumnType);
            }
        }
    }
}