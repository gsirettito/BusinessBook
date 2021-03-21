using System.ComponentModel;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace Bussines {
    public class SortAdorner : Adorner {
        private static Geometry ascGeometry =
            Geometry.Parse("M 0 4 3.5 0 7 4");

        private static Geometry descGeometry =
            Geometry.Parse("M 0 0 3.5 4 7 0");

        public ListSortDirection Direction { get; private set; }

        public SortAdorner(UIElement element, ListSortDirection dir)
            : base(element) {
            this.Direction = dir;
        }

        protected override void OnRender(DrawingContext drawingContext) {
            base.OnRender(drawingContext);

            if (AdornedElement.RenderSize.Width < 20)
                return;

            TranslateTransform transform = new TranslateTransform(AdornedElement.RenderSize.Width / 2.0 - 6.0, 1.0);
            drawingContext.PushTransform(transform);

            Geometry geometry = ascGeometry;
            if (this.Direction == ListSortDirection.Descending)
                geometry = descGeometry;
            drawingContext.DrawGeometry(null, new Pen(Brushes.Black, 1), geometry);

            drawingContext.Pop();
        }
    }
}
