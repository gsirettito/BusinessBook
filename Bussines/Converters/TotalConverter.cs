using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace Bussines.Converters {
    public class TotalConverter : IValueConverter {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            double total = 0;
            if (value == null) return null;
            if (value is ObservableCollection<Entrance>) {
                foreach (var i in (ObservableCollection<Entrance>)value) {
                    if (i.Equip.Solution != null) {
                        total += i.Equip.Solution.Cost;
                    }
                } return total;
            }
            if (value is ItemCollection) {
                foreach (Entrance i in value as ItemCollection) {
                    if (i.Equip.Solution != null) {
                        total += i.Equip.Solution.Cost;
                    }
                }
                return total;
            }
            if (value is ListView)
                foreach (Entrance i in ((ListView)value).SelectedItems) {
                    if (i.Equip.Solution != null) {
                        total += i.Equip.Solution.Cost;
                    }
                }
            return total;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return Binding.DoNothing;
        }
    }

    public class TotalConverter_MB : IMultiValueConverter {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
            double total = 0;
            if (values[1] == null) return null;
            if (values[1] is ObservableCollection<Entrance>) {
                foreach (var i in (ObservableCollection<Entrance>)values[1]) {
                    if (i.Equip.Solution != null) {
                        total += i.Equip.Solution.Cost;
                    }
                }
                return total;
            }
            if (values[1] is ItemCollection) {
                foreach (Entrance i in values[1] as ItemCollection) {
                    if (i.Equip.Solution != null) {
                        total += i.Equip.Solution.Cost;
                    }
                }
                return total.ToString();
            }
            if (values[1] is ListView)
                foreach (Entrance i in ((ListView)values[1]).SelectedItems) {
                    if (i.Equip.Solution != null) {
                        total += i.Equip.Solution.Cost;
                    }
                }
            return total;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }

    public class EmptyEntrancesConverter : IMultiValueConverter {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
            int count = 0;
            if (values[1] == null) return null;
            if (values[1] is ObservableCollection<Entrance>) {
                foreach (var i in (ObservableCollection<Entrance>)values[1]) {
                    if (i.Equip.Solution == null || i.Equip.Solution.Cost == 0) {
                        count++;
                    }
                }
                return count;
            }
            if (values[1] is ItemCollection) {
                foreach (Entrance i in values[1] as ItemCollection) {
                    if (i.Equip.Solution == null || i.Equip.Solution.Cost == 0) {
                        count++;
                    }
                }
                return count.ToString();
            }
            if (values[1] is ListView)
                foreach (Entrance i in ((ListView)values[1]).SelectedItems) {
                    if (i.Equip.Solution == null || i.Equip.Solution.Cost == 0) {
                        count++;
                    }
                }
            return count;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
