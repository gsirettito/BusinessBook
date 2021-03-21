using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace SiretT.Converters {
    [ValueConversion(typeof(ListView), typeof(bool?))]
    public sealed class IsSelectedAllListViewConverter : IValueConverter {

        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var list = (ListView)value;
            if (list.SelectedItems.Count != 0 && list.SelectedItems.Count == list.Items.Count) return true;
            else if (list.SelectedItems.Count == 0) return false;

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return Binding.DoNothing;
        }

        #endregion
    }
}
