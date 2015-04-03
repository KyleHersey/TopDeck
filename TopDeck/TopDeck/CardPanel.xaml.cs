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
    /// Interaction logic for CardPanel.xaml
    /// </summary>
    public partial class CardPanel : UserControl
    {

        public Card selectedCard
        {
            get;
            set;
        }

        public CardPanel()
        {
            InitializeComponent();
        }

        public void setCard(Card c)
        {
            selectedCard = c;
            RulesText.Text = c.Text;
        }
    }
}
