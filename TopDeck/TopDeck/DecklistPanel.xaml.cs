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
using System.Collections.ObjectModel;

namespace TopDeck
{
    /// <summary>
    /// Interaction logic for DecklistPanel.xaml
    /// </summary>
    public partial class DecklistPanel : UserControl
    {

        public ObservableCollection<LocalTuple> CurrentDeck
        {
            get;
            set;
        }

        public DatabaseManager DBMan
        {
            get;
            set;
        }

        public CardPanel RightPanel
        {
            get;
            set;
        }

        public DecklistPanel()
        {
            InitializeComponent();
            setItemsSource();

        }

        public void setItemsSource()
        {
            theList.ItemsSource = CurrentDeck;
        }

        public void setDatabaseManager(DatabaseManager db)
        {
            DBMan = db;
        }

        private void theList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (theList.SelectedItem != null)
            {
                LocalTuple tempTuple = (LocalTuple) theList.SelectedItem;
                Card c = DBMan.GetACardFromMultiverseId(tempTuple.MultiverseId);
                RightPanel.setCard(c);
                if (c.MultiverseIds.Count > 0)
                {
                    RightPanel.UpdateImage(tempTuple.MultiverseId);
                }
            }
        }

        private void PlusButton_Click(object sender, RoutedEventArgs e)
        {
            if (theList.SelectedItem != null)
            {
                LocalTuple cardTuple = (LocalTuple)theList.SelectedItem;
                cardTuple.Count++;
            }
            theList.Items.Refresh();
        }

        private void MinusButton_Click(object sender, RoutedEventArgs e)
        {
            if (theList.SelectedItem != null)
            {
                LocalTuple cardTuple = (LocalTuple)theList.SelectedItem;
                cardTuple.Count--;
                if (cardTuple.Count <= 0)
                {
                    ObservableCollection<LocalTuple> cards = (ObservableCollection<LocalTuple>)theList.ItemsSource;
                    cards.Remove(cardTuple);
                }
                theList.Items.Refresh();
            }
        }
    }
}
