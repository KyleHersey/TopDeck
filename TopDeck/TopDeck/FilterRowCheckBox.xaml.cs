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

namespace TopDeck
{
    /// <summary>
    /// Interaction logic for FilterRowCheckBox.xaml
    /// </summary>
    /// 

    public partial class FilterRowCheckBox : UserControl
    {
        public bool isChecked()
        {
            Console.WriteLine(Color.IsChecked.HasValue);
            Console.WriteLine(Color.IsChecked);
            return (bool)Color.IsChecked;
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
    }
}
