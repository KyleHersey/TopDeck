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
    /// Interaction logic for StatsPanel.xaml
    /// </summary>
    public partial class StatsPanel : UserControl
    {
        public KeyValuePair<string, int>[] ColorSource
        {
            get;
            set;
        }
        
        public StatsPanel()
        {
            InitializeComponent();

            ColorSource = new KeyValuePair<string, int>[]{
                new KeyValuePair<string, int>("White", 1),
                new KeyValuePair<string, int>("Blue", 2),
                new KeyValuePair<string, int>("Black", 4),
                new KeyValuePair<string, int>("Red", 8),
                new KeyValuePair<string, int>("Green", 16)
            };

            ColorPie.DataContext = this;
        }
    }
}
