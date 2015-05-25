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
        }

        //update panel to show changes to the list
        public void updatePanel()
        {
            setItemsSource();
            setSideboardItemsSource();
        }

        //set the list of card names
        public void setItemsSource()
        {
            theList.ItemsSource = DLMan.currentDeck;
        }

        //set the sideboard list of card names
        public void setSideboardItemsSource()
        {
            theSideboard.ItemsSource = DLMan.sideboard;
        }

        public void setDatabaseManager(DatabaseManager db)
        {
            DBMan = db;
        }

        //update the CardPanel to show the selected card
        private void theList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // display the correct card based on whether they want to see one from the sideboard
            // or from the main deck
            if (theSideboard.SelectedItem != null)
            {
                theSideboard.SelectedItem = null;
            }
            if (theList.SelectedItem != null)
            {
                LocalTuple tempTuple = (LocalTuple) theList.SelectedItem;
                Card c = DBMan.GetACardFromMultiverseId(tempTuple.MultiverseId);
                RightPanel.setCard(c);

                // display non-default image if the card has images associated with it
                if (c.MultiverseIds.Count > 0)
                {
                    RightPanel.UpdateImage(tempTuple.MultiverseId);
                }
            }
        }

        //increment the selected card
        private void PlusButton_Click(object sender, RoutedEventArgs e)
        {

            if (theList.SelectedItem != null)
            {
                LocalTuple cardTuple = (LocalTuple)theList.SelectedItem;
                DLMan.addCard(cardTuple.Name);
            }
            else if (theSideboard.SelectedItem != null)
            {
                LocalTuple cardTuple = (LocalTuple)theSideboard.SelectedItem;
                DLMan.addCardToSideboard(cardTuple.Name);
            }
        }

        //decrement the selected card. remove if zero
        private void MinusButton_Click(object sender, RoutedEventArgs e)
        {
            if (theList.SelectedItem != null)
            {
                LocalTuple cardTuple = (LocalTuple)theList.SelectedItem;
                cardTuple.Count--;
                theList.Items.Refresh();
                if (cardTuple.Count <= 0)
                {
                    DLMan.currentDeck.Remove(cardTuple);
                }
                DLMan.removeCard(cardTuple.Name);
            } 
            else if (theSideboard.SelectedItem != null)
            {
                LocalTuple cardTuple = (LocalTuple)theSideboard.SelectedItem;
                cardTuple.Count--;
                theSideboard.Items.Refresh();
                if (cardTuple.Count <= 0)
                {
                    DLMan.sideboard.Remove(cardTuple);
                }
            }
        }

        //update CardPanel to show selected card
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

        //increment/add card in sideboard
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
            }
            theSideboard.Items.Refresh();
        }
    }
}
