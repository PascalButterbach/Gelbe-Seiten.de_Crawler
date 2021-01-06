using Gelbe_Seiten_de_Crawler_WPF.ViewModels;
using System;
using System.Collections;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Gelbe_Seiten_de_Crawler_WPF.Views
{
    public partial class MainView : Window
    {
        readonly MainViewModel mainViewModel = new MainViewModel();
        public MainView()
        {
            InitializeComponent();
            this.DataContext = mainViewModel;

        }
    }

    [ValueConversion(typeof(IList), typeof(int))]
    public sealed class ItemIndexConverter : FrameworkContentElement, IValueConverter
    {
        public Object Convert(Object value, Type targetType, Object parameter, CultureInfo culture) =>
            ((IList)DataContext).IndexOf(value) + 1;

        public Object ConvertBack(Object value, Type targetType, Object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    };
}
