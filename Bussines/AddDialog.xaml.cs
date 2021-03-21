using System;
using System.Windows;

namespace Bussines {
    /// <summary>
    /// Lógica de interacción para Add.xaml
    /// </summary>
    public partial class AddDialog : Window {
        private bool _isExpert;
        private bool isEditing;
        private Entrance _entrance;
        public AddDialog(bool isExpert) {
            InitializeComponent();
            if ((_isExpert = isExpert)) {
                dateP.Visibility = System.Windows.Visibility.Visible;
                dateP.SelectedDate = DateTime.Now;
            }
            foreach (var i in Settings.Instance.Entrances) {
                if (i.Equip != null) {
                    if (!equip.Items.Contains(i.Equip.Name))
                        equip.Items.Add(i.Equip.Name);
                    if (!prop.Items.Contains(i.Equip.Client))
                        prop.Items.Add(i.Equip.Client);
                }
            }
        }

        public AddDialog(Entrance entrance, bool isExpert) {
            InitializeComponent();
            this.Title = "Editing";
            _entrance = entrance;
            isEditing = true;
            if ((_isExpert = isExpert)) {
                dateP.Visibility = System.Windows.Visibility.Visible;
                dateP.SelectedDate = entrance.EntranceDateTime;
            }
            foreach (var i in Settings.Instance.Entrances) {
                if (i.Equip != null) {
                    if (!equip.Items.Contains(i.Equip.Name))
                        equip.Items.Add(i.Equip.Name);
                    if (!prop.Items.Contains(i.Equip.Client))
                        prop.Items.Add(i.Equip.Client);
                }
            }

            this.equip.SelectedItem = entrance.Equip.Name;
            this.prop.SelectedItem = entrance.Equip.Client;
            this.prob.Text = entrance.Equip.Problem;
        }

        private void Ok_Click(object sender, RoutedEventArgs e) {
            if (!isEditing)
                Settings.Instance.Entrances.Add(new Entrance() {
                    EntranceDateTime = _isExpert ? (DateTime)dateP.SelectedDate : DateTime.Now,
                    Equip = new Equip() {
                        Name = equip.Text,
                        Client = prop.Text,
                        Problem = prob.Text
                    }
                });
            else {
                if (_isExpert)
                    _entrance.EntranceDateTime = (DateTime)dateP.SelectedDate;
                _entrance.Equip.Name = equip.Text;
                _entrance.Equip.Client = prop.Text;
                _entrance.Equip.Problem = prob.Text;
            }
            this.DialogResult = true;
            Close();
        }
    }
}
