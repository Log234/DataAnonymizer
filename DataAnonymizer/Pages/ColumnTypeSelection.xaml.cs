using System.Collections.Generic;
using System.Linq;
using DataAnonymizer.Consts;
using DataAnonymizer.Controls;
using DataAnonymizer.Utilities;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace DataAnonymizer.Pages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class ColumnTypeSelection : Page
{
    private readonly App _app;

    public ColumnTypeSelection()
    {
        InitializeComponent();
        _app = (App)Application.Current;
        ColumnContainer.Children.Clear();

        if (_app.data.Count < 2)
            return;

        for (var index = 0; index < _app.data.Count; index++)
        {
            var columnName = _app.data[index][0];
            var example = _app.data[index][1] ?? "";

            ColumnContainer.Children.Add(new DataTypeSelector(index, columnName, example.Trim()));
        }
    }

    private void Previous_Click(object sender, RoutedEventArgs e)
    {
        StoreSelection();
        Frame.GoBack(TransitionInfo.Default);
    }

    private void Next_Click(object sender, RoutedEventArgs e)
    {
        var window = (MainWindow)_app.m_window;

        StoreSelection();
        var cleaningResult = DataCleaner.Clean(_app.data, _app.columnTypeDict, _app.idDictionary);

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

        _app.data = cleaningResult.Value;



        Frame.Navigate(typeof(FileSave), null, TransitionInfo.Default);
    }

    private void StoreSelection()
    {
        foreach (DataTypeSelector column in ColumnContainer.Children.Where(child => child is DataTypeSelector))
        {
            _app.columnTypeDict[column.index] = (column.ShouldAnonymize, column.ColumnType);
        }
    }
}