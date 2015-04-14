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
            RulingsListView.ItemsSource = c.Rulings;
            SetList.ItemsSource = c.MultiverseIds;
        }

        public void UpdateImage(string id)
        {
            BitmapImage img = new BitmapImage();
            img.BeginInit();
            // can try to use GetAMultiverseId in DBManager, but will need to check if null
            img.UriSource = new Uri("http://gatherer.wizards.com/Handlers/Image.ashx?multiverseid=" + id + "&type=card");
            img.EndInit();
            CardImage.Source = img;
        }

        public string CurrentMultiverseId()
        {
            if (SetList.SelectedItem != null)
            {
                Tuple<string, string> currentItem = (Tuple<string, string>)SetList.SelectedItem;
                return currentItem.Item2;
            }
            return null;
        }

        private void RulingsClick(object sender, RoutedEventArgs e)
        {
            RulingPopup.IsOpen = true;
        }

        private void SetList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SetList.SelectedItem != null)
            {
                Tuple<string, string> currentItem = (Tuple<string,string>) SetList.SelectedItem;
                UpdateImage(currentItem.Item2);
            }
        }
    }
}
