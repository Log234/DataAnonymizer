using System;
using Anonymizer.Utilities;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Anonymizer.Controls
{
    public sealed partial class DataTypeSelector : UserControl
    {
        public readonly string ColumnName;
        public bool ShouldAnonymize => Anonymize.IsChecked ?? false;
        public ColumnTypes ColumnType { get; private set; }

        public DataTypeSelector(string name, string example)
        {
            var app = Application.Current as App;

            InitializeComponent();
            ColumnName = name;

            DataTypeName.Text = name;
            Example.Text = example;
            RbGeneral.GroupName = name;
            RbName.GroupName = name;
            RbCompany.GroupName = name;

            if (app.columnTypeDict.ContainsKey(name))
            {
                (var shouldAnonymize, ColumnType) = app.columnTypeDict[name];

                Anonymize.IsChecked = shouldAnonymize;

                switch (ColumnType)
                {
                    case ColumnTypes.General:
                        RbGeneral.IsChecked = true;
                        break;
                    case ColumnTypes.Name:
                        RbName.IsChecked = true;
                        break;
                    case ColumnTypes.Company:
                        RbCompany.IsChecked = true;
                        break;
                    default:
                        break;
                }
            }
            
            RbGeneral.Checked += (s, e) => ColumnType = ColumnTypes.General;
            RbName.Checked += (s, e) => ColumnType = ColumnTypes.Name;
            RbCompany.Checked += (s, e) => ColumnType = ColumnTypes.Company;
        }
    }
}