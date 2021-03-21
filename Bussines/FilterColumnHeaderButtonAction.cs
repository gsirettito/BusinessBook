using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Interactivity;

namespace Bussines {
    public class ContextFilter : ContextMenu { }

    public class FilterItem : MenuItem {
        public static readonly DependencyProperty ContentProperty =
                DependencyProperty.Register("Content", typeof(object), typeof(FilterItem), new PropertyMetadata(default(object)));

        public object Content {
            get { return GetValue(ContentProperty); }
            set { SetValue(ContentProperty, value); }
        }
    }

    public class FilterColumnHeaderButtonAction : TriggerAction<DependencyObject> {
        protected override void Invoke(object parameter) {
            ContextMenu menu = (GridViewColumnHeader.Column as CustomGridViewColumn).FilterMenu;
            menu.IsOpen = true;
        }

        public static readonly DependencyProperty ListViewProperty =
            DependencyProperty.Register("ListView", typeof(ListView), typeof(FilterColumnHeaderButtonAction), new PropertyMetadata(default(ListView)));

        public ListView ListView {
            get { return (ListView)GetValue(ListViewProperty); }
            set { SetValue(ListViewProperty, value); }
        }

        public static readonly DependencyProperty GridViewColumnHeaderProperty =
            DependencyProperty.Register("GridViewColumnHeader", typeof(GridViewColumnHeader), typeof(FilterColumnHeaderButtonAction), new PropertyMetadata(default(GridViewColumnHeader)));

        public GridViewColumnHeader GridViewColumnHeader {
            get { return (GridViewColumnHeader)GetValue(GridViewColumnHeaderProperty); }
            set { SetValue(GridViewColumnHeaderProperty, value); }
        }
    }
}