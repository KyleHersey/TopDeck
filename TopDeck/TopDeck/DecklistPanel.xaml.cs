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

        public CardPanel RightPanel
        {
            get;
            set;
        }

        public DecklistPanel()
        {
            InitializeComponent();
            //setItemsSource();
            //setSideboardItemsSource();
        }

        public void updatePanel()
        {
            setItemsSource();
            setSideboardItemsSource();
        }

        public void setItemsSource()
        {
            theList.ItemsSource = DLMan.currentDeck;
        }

        public void setSideboardItemsSource()
        {
            theSideboard.ItemsSource = DLMan.sideboard;
        }

        public void setDatabaseManager(DatabaseManager db)
        {
            DBMan = db;
        }

        private void theList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (theSideboard.SelectedItem != null)
            {
                theSideboard.SelectedItem = null;
            }
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
            else if (theSideboard.SelectedItem != null)
            {
                LocalTuple cardTuple = (LocalTuple)theSideboard.SelectedItem;
                cardTuple.Count++;
            }

            DLMan.update();
        }

        private void MinusButton_Click(object sender, RoutedEventArgs e)
        {
            if (theList.SelectedItem != null)
            {
                LocalTuple cardTuple = (LocalTuple)theList.SelectedItem;
                cardTuple.Count--;
                if (cardTuple.Count <= 0)
                {
                    DLMan.currentDeck.Remove(cardTuple);
                }
            } 
            else if (theSideboard.SelectedItem != null)
            {
                LocalTuple cardTuple = (LocalTuple)theSideboard.SelectedItem;
                cardTuple.Count--;
                if (cardTuple.Count <= 0)
                {
                    DLMan.sideboard.Remove(cardTuple);
                }
            }

            DLMan.update();
        }

        private void theSideboard_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (theList.SelectedItem != null)
            {
                theList.SelectedItem = null;
            }
            if (theSideboard.SelectedItem != null)
            {
                LocalTuple tempTuple = (LocalTuple)theSideboard.SelectedItem;
                Card c = DBMan.GetACardFromMultiverseId(tempTuple.MultiverseId);
                RightPanel.setCard(c);
                if (c.MultiverseIds.Count > 0)
                {
                    RightPanel.UpdateImage(tempTuple.MultiverseId);
                }
            }
        }

        private void AddSideboardButton_Click(object sender, RoutedEventArgs e)
        {
            if (theList.SelectedItem != null)
            {
                LocalTuple selectedCard = (LocalTuple) theList.SelectedItem;
                bool found = false;
                foreach (LocalTuple currentCard in DLMan.sideboard)
                {
                    if (currentCard.Name.Equals(selectedCard.Name) && currentCard.MultiverseId.Equals(selectedCard.MultiverseId))
                    {
                        found = true;
                        currentCard.Count++;
                        break;
                    }
                }
                if (!found)
                {
                    LocalTuple tupleToAdd = new LocalTuple(1, selectedCard.Name, selectedCard.MultiverseId);
                    DLMan.sideboard.Add(tupleToAdd);
                }

                DLMan.update();
            }
        }
    }
}
