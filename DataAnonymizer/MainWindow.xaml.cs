using System;
using System.Runtime.InteropServices;
using DataAnonymizer.Pages;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WinRT;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace DataAnonymizer
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        internal static IntPtr hwnd;

        public MainWindow()
        {
            this.InitializeComponent();
            hwnd = this.As<IWindowNative>().WindowHandle;
            RootFrame.Navigate(typeof(FileSelection));
        }

        public void AddMessage(InfoBar message)
        {
            Messages.Children.Add(message);
        }

        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("EECDBF0E-BAE9-4CB6-A68E-9598E1CB57BB")]
        internal interface IWindowNative
        {
            IntPtr WindowHandle { get; }
        }
    }
}