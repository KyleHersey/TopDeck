using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;


namespace TopDeck
{
    public partial class CardPanel : UserControl
    {

        //card shown in the card panel
        public Card selectedCard
        {
            get;
            set;
        }

        public CardPanel()
        {
            InitializeComponent();

            // hide rulings button unless the card has it
            RulingsButtonBorder.Visibility = System.Windows.Visibility.Hidden;
        }

        //stop displaying card
        public void Clear()
        {
            RulesText.Text = "";                    //no rules text
            UpdateImage(null);                      //image to default image
            RulingsListView.ItemsSource = null;     //no rulings
            RulingsButtonBorder.Visibility = System.Windows.Visibility.Hidden;
        }

        //display card
        public void setCard(Card c)
        {
            selectedCard = c;
            RulesText.Text = c.Text;

            // display rulings button if card has rulings
            if (c.Rulings != null && c.Rulings.Count > 0)
            {
                RulingsListView.ItemsSource = c.Rulings;
                RulingsButtonBorder.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                RulingsButtonBorder.Visibility = System.Windows.Visibility.Hidden;
            }
            SetList.ItemsSource = c.MultiverseIds;
        }

        //set the image by multiverse ID
        public void UpdateImage(string id)
        {
            BitmapImage img = new BitmapImage();
            Console.WriteLine("new bmp");
            img.BeginInit();
            Console.WriteLine("begin init");

            if (id != null)
            {
                img.UriSource = new Uri("http://gatherer.wizards.com/Handlers/Image.ashx?multiverseid=" + id + "&type=card");
            }
            else
            {
                img.UriSource = new Uri("Magic_the_gathering-card_back.jpg", UriKind.Relative);
            }

            Console.WriteLine(img.ToString());
            Console.WriteLine("got uri");
            img.EndInit();
            Console.WriteLine("end init");

            CardImage.Source = img;
            Console.WriteLine("set image");
            
        }

        //get multiverse ID by the selected set
        public string CurrentMultiverseId()
        {
            if (SetList.SelectedItem != null)
            {
                Tuple<string, string> currentItem = (Tuple<string, string>)SetList.SelectedItem;
                return currentItem.Item2;
            }
            return null;
        }

        //show ruling popup
        private void RulingsClick(object sender, RoutedEventArgs e)
        {
            RulingPopup.IsOpen = true;
        }

        //update the image when the user clicks a set name
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
