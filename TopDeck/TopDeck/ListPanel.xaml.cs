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
using System.Threading;

namespace TopDeck
{
    public partial class ListPanel : UserControl
    {
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

        public DecklistManager DLMan
        {
            get;
            set;
        }


        public ListPanel()
        {
            InitializeComponent();
        }

        public void setItemsSource(List<string> names){
            theList.ItemsSource = names;
            names.Sort();
        }

        public void setDatabaseManager(DatabaseManager db)
        {
            DBMan = db;
        }

        private void theList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (theList.SelectedItem != null)
            {
                Card c = DBMan.GetCard(theList.SelectedItem.ToString());
                RightPanel.setCard(c);
                if (c.MultiverseIds.Count > 0)
                {
                    RightPanel.UpdateImage(c.MultiverseIds[0].Item2);
                }
            }
        }

        private void AddToDeck_Click(object sender, RoutedEventArgs e)
        {
            // if it is in the list, increase the tuple with that count.
            // if it is not in the list, create the tuple and add to the list
            AddToCardList(DLMan.currentDeck);
            DLMan.update();
        }

        private void AddToSideBoard_Click(object sender, RoutedEventArgs e)
        {
            AddToCardList(DLMan.sideboard);
            DLMan.update();
        }

        private void AddToCardList(ObservableCollection<LocalTuple> list) {
            bool foundItem = false;
            if (list.Count > 0 && theList.SelectedItem != null)
            {
                foreach (LocalTuple currentTuple in list)
                {
                    // if this card is already in the deck AND 
                    // it has the same art as the card already in the deck, 
                    // and the user selected that art,
                    // increment the count of that card
                    if (RightPanel.CurrentMultiverseId() != null && currentTuple.Name.Equals(theList.SelectedItem) && currentTuple.MultiverseId.Equals(RightPanel.CurrentMultiverseId()))
                    {
                        foundItem = true;
                        currentTuple.Count++;
                    }
                    // if this card is already in the deck and does NOT have the 
                    // same art, and the user did not select the art,
                    // add a new card to the deck with that art
                    else if (RightPanel.CurrentMultiverseId() == null && currentTuple.Name.Equals(theList.SelectedItem) && RightPanel.selectedCard.MultiverseIds.Count > 0 && RightPanel.selectedCard.MultiverseIds[0].Item2.Equals(currentTuple.MultiverseId))
                    {
                        foundItem = true;
                        currentTuple.Count++;
                    }
                }
            }
            
            // if the card is not in the deck and they selected art, add the card
            if (!foundItem && theList.SelectedItem != null && RightPanel.CurrentMultiverseId() != null)
            {
                list.Add(new LocalTuple(1, theList.SelectedItem.ToString(), RightPanel.CurrentMultiverseId()));
            }
            // if the card is not in the deck and they did not select art,
            // add the card with the first art in its multiverseId list
            else if (!foundItem && theList.SelectedItem != null && RightPanel.selectedCard.MultiverseIds.Count > 0)
            {
                list.Add(new LocalTuple(1, theList.SelectedItem.ToString(), RightPanel.selectedCard.MultiverseIds[0].Item2));
            }
        }

        private void UserControl_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                // change focus to next tab
                // FIX
                Keyboard.Focus(RightPanel.SetList);
            }
            // add checks for + key
            else if (e.Key == Key.Add)
            {
                // increment the count of this card in the list
                AddToDeck_Click(null, null);
            }
            else if (e.Key == Key.A && (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
            {
                // increment the count of this card in the sideboard
                // FIX
                AddToSideBoard_Click(null, null);
            }
        }
    }
}
