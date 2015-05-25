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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;

namespace TopDeck
{
    public partial class FilterRowCheckBox : UserControl, INotifyPropertyChanged
    {
        public bool isChecked()
        {
            return (bool)Check.IsChecked;
        }

        public string FilterText
        {
            get;
            set;
        }

        public FilterRowCheckBox()
        {
            InitializeComponent();
            FilterType.DataContext = this;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string check)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(check));
            }
        }

        private void Check_Click(object sender, RoutedEventArgs e)
        {
            CheckBox send = (CheckBox)sender;

            if (send.IsChecked == true)
            {
                NotifyPropertyChanged("checked");
            } else if(send.IsChecked == false){
                NotifyPropertyChanged("unchecked");
            }
        }
    }
}
