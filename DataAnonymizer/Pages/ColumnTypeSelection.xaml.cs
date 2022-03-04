using System;
using System.Linq;
using DataAnonymizer.Consts;
using DataAnonymizer.Controls;
using DataAnonymizer.Utilities;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

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
            var example = _app.data[index].Skip(1).FirstOrDefault(value => !string.IsNullOrWhiteSpace(value)) ?? "";

            ColumnContainer.Children.Add(new DataTypeSelector(index, columnName, example.Trim()));
        }
    }

    private void Previous_Click(object sender, RoutedEventArgs eventArgs)
    {
        var window = (MainWindow)_app.m_window;
        try
        {
            StoreSelection();
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
            StoreSelection();
            var cleaningResult = DataCleaner.Clean(_app.data, _app.columnTypeDict, _app.idDictionary);

            if (cleaningResult.IsFailure)
            {
                window.AddMessage(new InfoBar
                {
                    Severity = InfoBarSeverity.Error,
                    Title = $"Failed to clean the data:\n{cleaningResult.Error}",
                    IsOpen = true
                });
                return;
            }

            _app.data = cleaningResult.Value;



            Frame.Navigate(typeof(FileSave), null, TransitionInfo.Default);
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

    private void StoreSelection()
    {
        foreach (DataTypeSelector column in ColumnContainer.Children.Where(child => child is DataTypeSelector))
        {
            _app.columnTypeDict[column.index] = (column.ShouldAnonymize, column.ColumnType);
        }
    }
}