using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Threading;

namespace Bussines {
    public class EntrancesListViewModel : INotifyPropertyChanged {
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(String info) {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        #endregion

        public EntrancesListViewModel() {
            Items = new ObservableCollection<Entrance>();
        }

        #region nonModifiedCode

        private ObservableCollection<Entrance> ientrance;
        public ObservableCollection<Entrance> Items {
            get { return ientrance; }
            set {
                ientrance = value;
                OnPropertyChanged("Items");
            }
        }

        #endregion

        public Entrance SelectedItem { get; set; }

        public void SetItems(List<Entrance> list) {
            Items = new ObservableCollection<Entrance>(list);
        }
    }

    public class CustomGridViewColumn : GridViewColumn {
        public static readonly DependencyProperty DisplaySourceProperty = DependencyProperty.Register(
            "DisplaySource", typeof(object), typeof(CustomGridViewColumn), new PropertyMetadata());

        public object DisplaySource {
            get { return GetValue(DisplaySourceProperty); }
            set { SetValue(DisplaySourceProperty, value); }
        }

        public static readonly DependencyProperty FilterMenuProperty = DependencyProperty.Register(
            "FilterMenu", typeof(ContextMenu), typeof(CustomGridViewColumn), new PropertyMetadata());

        public ContextMenu FilterMenu {
            get { return (ContextMenu)GetValue(FilterMenuProperty); }
            set { SetValue(FilterMenuProperty, value); }
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e) {
            base.OnPropertyChanged(e);
            if (e.Property == CellTemplateProperty) {
                var cellt = (e.NewValue as DataTemplate);
            }
        }
    }

    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Fluent.RibbonWindow {
        private GridViewColumnHeader listViewSortCol = null;
        private SortAdorner listViewSortAdorner = null;
        private List<Entrance> iEntrance;
        private EntrancesListViewModel viewItems;

        public MainWindow() {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
            this.Closing += MainWindow_Closing;
            viewItems = new EntrancesListViewModel();
            viewItems.PropertyChanged += viewItems_PropertyChanged;
            this.DataContext = viewItems;
            list.SelectionChanged += list_SelectionChanged;
        }

        private enum Theme {
            Office2010,
            Office2013
        }

        private Theme? currentTheme;

        private void ChangeTheme(Theme theme, string color) {
            this.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, (ThreadStart)(() => {
                var owner = Window.GetWindow(this);
                if (owner != null) {
                    owner.Resources.BeginInit();

                    if (owner.Resources.MergedDictionaries.Count > 0) {
                        owner.Resources.MergedDictionaries.RemoveAt(0);
                    }

                    if (string.IsNullOrEmpty(color) == false) {
                        owner.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri(color) });
                    }

                    owner.Resources.EndInit();
                }

                if (this.currentTheme != theme) {
                    Application.Current.Resources.BeginInit();
                    switch (theme) {
                        case Theme.Office2010:
                            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("pack://application:,,,/Fluent;component/Themes/Generic.xaml") });
                            Application.Current.Resources.MergedDictionaries.RemoveAt(0);
                            break;
                        case Theme.Office2013:
                            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("pack://application:,,,/Fluent;component/Themes/Office2013/Generic.xaml") });
                            Application.Current.Resources.MergedDictionaries.RemoveAt(0);
                            break;
                    }

                    this.currentTheme = theme;
                    Application.Current.Resources.EndInit();

                    if (owner != null) {
                        owner.Style = null;
                        owner.Style = owner.FindResource("RibbonWindowStyle") as Style;
                        owner.Style = null;
                    }
                }
            }));
        }

        void viewItems_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            this.DataContext = null;
            this.DataContext = viewItems;
            if (list.ItemsSource != null)
                CollectionViewSource.GetDefaultView(list.ItemsSource).Filter = UserFilter;
        }

        private bool UserFilter(object item) {
            if (String.IsNullOrEmpty(filter.Text))
                return true;

            Entrance iShell = (Entrance)item;

            return (iShell.Equip.Name.IndexOf(filter.Text, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private void filter_TextInput(object sender, TextChangedEventArgs e) {
            CollectionViewSource.GetDefaultView(list.ItemsSource).Refresh();
        }

        void list_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            double tot = 0d;
            foreach (Entrance i in list.SelectedItems)
                tot += (i.Equip.Solution != null) ? i.Equip.Solution.Cost : 0d;
            selc.Text = tot.ToString();
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e) {
            Settings.Instance.Initalise("settings.xml");
            Entrance[] entrances = new Entrance[Settings.Instance.Entrances.Count];
            Settings.Instance.Entrances.CopyTo(entrances, 0);
            iEntrance = new List<Entrance>();
            iEntrance.AddRange(entrances);
            viewItems.SetItems(iEntrance);
            //list.ItemsSource = Settings.Instance.Entrances;
            CollectionViewSource.GetDefaultView(list.ItemsSource).Filter = UserFilter;
            GridViewColumnHeader_Click(sortByEntranceDateTime, e);
            gview.Columns.CollectionChanged += Columns_CollectionChanged;
            //ColumnsUpdate();
        }

        private string defaultCellTemplate = @"<DataTemplate xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
             xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
             xmlns:stc=""clr-namespace:SiretT.Converters;assembly=Bussines"">
            <DataTemplate.Resources>
                <stc:LogicToVisibilityConverter x:Key=""LogicConverter"" LogicOperator=""OR""/>
            </DataTemplate.Resources>
            <Grid>
                <!--<Grid.ContextMenu>
                    <ContextMenu>
                        <MenuItem Click=""EditEntrance_Click"" Header=""Editar entrada""/>
                        <MenuItem Click=""EditSolution_Click"" Header=""Editar salida""/>
                        <MenuItem Click=""Delete_Click"" Header=""Eliminar"" IsEnabled=""[Binding IsChecked, ElementName=expert]""/>
                    </ContextMenu>
                </Grid.ContextMenu>-->
                <Grid.ColumnDefinitions>
                    <ColumnDefinition Width=""18""/>
                    <ColumnDefinition/>
                </Grid.ColumnDefinitions>
                <CheckBox VerticalAlignment=""Center""
                          IsChecked=""[Binding IsSelected, RelativeSource=[RelativeSource AncestorType=[x:Type ListViewItem]]]""
                                Margin=""0,0,3,0""
                                SnapsToDevicePixels=""True"">
                    <CheckBox.Visibility>
                        <MultiBinding Converter=""[StaticResource LogicConverter]"" >
                            <Binding Path=""IsChecked"" RelativeSource=""[RelativeSource Self]""/>
                            <Binding Path=""IsMouseOver"" RelativeSource=""[RelativeSource AncestorType=[x:Type ListViewItem]]""/>
                        </MultiBinding>
                    </CheckBox.Visibility>
                </CheckBox>
                <TextBlock Grid.Column=""1""
                           HorizontalAlignment=""Left""
                           TextTrimming=""CharacterEllipsis""
                           Foreground=""#000""
                           Text=""[Binding {0}]""
                           SnapsToDevicePixels=""True""/>
            </Grid>
        </DataTemplate>";

        private void Columns_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            ColumnsUpdate();
        }

        private void ColumnsUpdate() {
            for (int i = 1; i < gview.Columns.Count; i++) {
                gview.Columns[i].DisplayMemberBinding = new Binding((gview.Columns[i].Header as GridViewColumnHeader).Tag.ToString());
                gview.Columns[i].CellTemplate = null;
                gview.Columns[i].HeaderTemplate = null;
            }

            DataTemplate fcht = this.FindResource("FirstColumnHeaderTemplate") as DataTemplate;
            string xaml = string.Format(defaultCellTemplate, (gview.Columns[0].Header as GridViewColumnHeader).Tag);
            xaml = xaml.Replace('[', '{').Replace(']', '}');
            DataTemplate cellTemplate = (DataTemplate)XamlReader.Parse(xaml);
            gview.Columns[0].DisplayMemberBinding = null;
            gview.Columns[0].CellTemplate = cellTemplate;
            gview.Columns[0].HeaderTemplate = fcht;
            list.ApplyTemplate();
        }

        void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            Settings.Instance.Save("settings.xml");
        }

        private void Add_Click(object sender, RoutedEventArgs e) {
            AddDialog add = new AddDialog((bool)expert.IsChecked);
            if ((bool)(add.ShowDialog())) {
                list.ItemsSource = null;
                list.ItemsSource = Settings.Instance.Entrances;
            }
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            if (e.ClickCount == 2) {
                EditSolution_Click(sender, new RoutedEventArgs());
            }
        }

        private void EditEntrance_Click(object sender, RoutedEventArgs e) {
            AddDialog add = new AddDialog((Entrance)list.SelectedItem, (bool)expert.IsChecked);
            if ((bool)(add.ShowDialog())) {
                list.ItemsSource = null;
                list.ItemsSource = Settings.Instance.Entrances;
            }
        }

        private void EditSolution_Click(object sender, RoutedEventArgs e) {
            SolutionDialog sd = new SolutionDialog((Entrance)list.SelectedItem, (bool)expert.IsChecked);
            if ((bool)(sd.ShowDialog())) {
                list.ItemsSource = null;
                list.ItemsSource = Settings.Instance.Entrances;
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e) {
            if ((bool)(new DeleteDialog().ShowDialog())) {
                Settings.Instance.Entrances.Remove((Entrance)list.SelectedItem);
                list.ItemsSource = null;
                list.ItemsSource = Settings.Instance.Entrances;
            }
        }

        private void selectAll_Click(object sender, System.Windows.RoutedEventArgs e) {
            list.SelectAll();
        }

        private void selectNone_Click(object sender, System.Windows.RoutedEventArgs e) {
            list.SelectedItems.Clear();
        }

        private void selectInvert_Click(object sender, System.Windows.RoutedEventArgs e) {
            foreach (var i in list.Items)
                if (list.SelectedItems.Contains(i))
                    list.SelectedItems.Remove(i);
                else list.SelectedItems.Add(i);
        }

        private void remove_Click(object sender, System.Windows.RoutedEventArgs e) {
            for (int i = 0; i < list.SelectedItems.Count; i++) {
                int index = -1;
                if ((index = viewItems.Items.IndexOf(list.SelectedItems[i] as Entrance)) >= 0) {
                    viewItems.Items.RemoveAt(index);
                    iEntrance.RemoveAt(index);
                    i--;
                }
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e) {

        }

        private void GridViewColumnHeader_Click(object sender, RoutedEventArgs e) {
            GridViewColumnHeader column = (sender as GridViewColumnHeader);
            string sortBy = column.Tag.ToString();
            if (listViewSortCol != null) {
                AdornerLayer.GetAdornerLayer(listViewSortCol).Remove(listViewSortAdorner);
                list.Items.SortDescriptions.Clear();
            }

            ListSortDirection newDir = ListSortDirection.Ascending;
            if (listViewSortCol == column && listViewSortAdorner.Direction == newDir)
                newDir = ListSortDirection.Descending;

            listViewSortCol = column;
            listViewSortAdorner = new SortAdorner(listViewSortCol, newDir);
            AdornerLayer.GetAdornerLayer(listViewSortCol).Add(listViewSortAdorner);
            list.Items.SortDescriptions.Add(new SortDescription(sortBy, newDir));
        }

        private void CheckColumnAll_Checked(object sender, RoutedEventArgs e) {
            list.SelectAll();
        }

        private void CheckColumnAll_Unchecked(object sender, RoutedEventArgs e) {
            list.SelectedItems.Clear();
        }

        private void MenuItem_Checked(object sender, RoutedEventArgs e) {
            MenuItem mi = (MenuItem)sender;
            string property = ((ContextMenu)mi.Parent).Tag.ToString();

            bool isChecked = false;
            CollectionViewSource.GetDefaultView(list.ItemsSource).Filter = (item) => {
                Entrance iEntrance = (Entrance)item;
                string text = "";
                switch (property) {
                    case "Equip.Name":
                        text = iEntrance.Equip.Name;
                        break;
                    case "Equip.Client":
                        text = iEntrance.Equip.Client;
                        break;
                }
                string tag = mi.Tag.ToString();
                bool bd = false;
                foreach (MenuItem i in ((ContextMenu)mi.Parent).Items) {
                    if (i.IsChecked) {
                        isChecked = true;
                        string itag = i.Tag.ToString();
                        if (text[0] >= itag[0] && text[0] <= itag[1]) {
                            bd = true;
                            break;
                        }
                    }
                }
                return bd;
            };

            if (!isChecked)
                CollectionViewSource.GetDefaultView(list.ItemsSource).Filter = UserFilter;
            CollectionViewSource.GetDefaultView(list.ItemsSource).Refresh();
        }

        private void MenuItem_Unchecked(object sender, RoutedEventArgs e) {
            CollectionViewSource.GetDefaultView(list.ItemsSource).Filter = UserFilter;
            CollectionViewSource.GetDefaultView(list.ItemsSource).Refresh();
        }
    }
}
