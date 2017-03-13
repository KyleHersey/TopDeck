using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using Ninject;

namespace TopDeck
{
    public partial class DecklistPanel : UserControl
    {
        [Inject]
        public DecklistManager DLMan
        {
            get;
            set;
        }

        [Inject]
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

        // passing in 0 causes the count of the deck to be returned.
        // passing in 1 causes the count of the sideboard to be returned.
        public String TotalCardCount(int listToUpdate)
        {
            if (listToUpdate == 0)
            {
                if (DLMan.sideboard == null)
                {
                    return "0 Card(s)";
                }

                return CountCards(DLMan.currentDeck) + " Card(s)";
            }
            else if (listToUpdate == 1)
            {
                if (DLMan.sideboard == null)
                {
                    return "0 Card(s)";
                }

                return CountCards(DLMan.sideboard) + " Card(s)";
            }

            return "0 Card(s)";
        }

        public int CountCards(ObservableCollection<LocalTuple> listToCount)
        {
            int count = 0;
            List<LocalTuple> currentList = listToCount.ToList<LocalTuple>();
            foreach (LocalTuple currentTuple in currentList)
            {
                count += currentTuple.Count;
            }

            return count;
        }

        public void UpdateCardCounts()
        {
            DeckTotalCards.Text = TotalCardCount(0);
            SideboardTotalCards.Text = TotalCardCount(1);
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
                DLMan.addCard(cardTuple);

                DeckTotalCards.Text = TotalCardCount(0);
            }
            else if (theSideboard.SelectedItem != null)
            {
                LocalTuple cardTuple = (LocalTuple)theSideboard.SelectedItem;
                DLMan.addCardToSideboard(cardTuple);

                SideboardTotalCards.Text = TotalCardCount(1);
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

                DeckTotalCards.Text = TotalCardCount(0);
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


                SideboardTotalCards.Text = TotalCardCount(1);
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
            SideboardTotalCards.Text = TotalCardCount(1);
        }
    }
}
