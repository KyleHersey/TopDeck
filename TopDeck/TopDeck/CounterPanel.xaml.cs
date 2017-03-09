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
    public partial class CounterPanel : UserControl
    {
        public DecklistManager DLMan
        {
            get;
            set;
        }

        public DatabaseManager DBMan
        {
            get;
            set;
        }

        public HandPanel HandPanel
        {
            get;
            set;
        }

        public CounterPanel()
        {
            InitializeComponent();
        }

        private void Draw(object sender, RoutedEventArgs e)
        {
            Random random = new Random();
            LocalTuple card = DLMan.currentDeck[random.Next(DLMan.currentDeck.Count)];
            Console.WriteLine(card.Name);
            HandPanel.UpdateImage(card.MultiverseId);
        }
    }
}
