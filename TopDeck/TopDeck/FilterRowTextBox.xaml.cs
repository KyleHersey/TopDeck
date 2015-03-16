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
    /// Interaction logic for FilterRowTextBox.xaml
    /// </summary>
    public partial class FilterRowTextBox : UserControl
    {

        public string FilterText
        {
            get;
            set;
        }

        public FilterRowTextBox()
        {
            InitializeComponent();
            FilterType.DataContext = this;
        }
    }
}
