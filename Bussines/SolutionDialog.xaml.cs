using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Bussines {
    /// <summary>
    /// Lógica de interacción para SolutionDialog.xaml
    /// </summary>
    public partial class SolutionDialog : Window {
        private Entrance _entrance;
        private bool isReady;
        private bool _isExpert;
        public SolutionDialog(Entrance entrance, bool isExpert) {
            InitializeComponent();
            if ((_isExpert = isExpert)) {
                dateP.Visibility = System.Windows.Visibility.Visible;
                if (entrance.Equip.Solution != null)
                    dateP.SelectedDate = entrance.Equip.Solution.SolutionDateTime;
                else dateP.SelectedDate = DateTime.Now;
            }
            this.Entrance = entrance;
            if (entrance.Equip.Solution != null) {
                isReady = true;
                cost.Text = entrance.Equip.Solution.Cost.ToString();
                solu.Text = entrance.Equip.Solution.Description;
            }
        }

        private void Ok_Click(object sender, RoutedEventArgs e) {
            this.Entrance.Equip.Solution = new Solution() {
                Cost = Convert.ToDouble(cost.Text),
                Description = solu.Text,
            };
            if (!isReady || _isExpert) {
                this.Entrance.Equip.Solution.SolutionDateTime = _isExpert ? (DateTime)dateP.SelectedDate : DateTime.Now;
            }
            this.DialogResult = true;
            Close();
        }

        public Entrance Entrance { get; set; }
    }
}
