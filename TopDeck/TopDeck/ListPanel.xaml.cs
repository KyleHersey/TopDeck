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
    /// Interaction logic for ListPanel.xaml
    /// </summary>
    public partial class ListPanel : UserControl
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

        public List<LocalTuple> CurrentDeck
        {
            get;
            set;
        }

        public ListPanel()
        {
            InitializeComponent();
        }

        public void openFile(string path)
        {
            theList.ItemsSource = DLMan.GetCardnamesFromFile(path);
        }

        public void setItemsSource(List<string> names){
            theList.ItemsSource = names;
            names.Sort();
        }

        public void setDatabaseManager(DatabaseManager db)
        {
            DBMan = db;
            DLMan = new DecklistManager(db);
        }

        private void theList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (theList.SelectedItem != null)
            {
                Card c = DBMan.GetCard(theList.SelectedItem.ToString());
                RightPanel.setCard(c);
                if (c.MultiverseIds.Count > 0)
                    RightPanel.UpdateImage(c.MultiverseIds[0].Item2);
            }
        }

        private void AddToDeck_Click(object sender, RoutedEventArgs e)
        {
            // if it is in the list, increase the tuple with that count.
            // if it is not in the list, create the tuple and add to the list
            // ignore multiverse id for now
            bool foundItem = false;
            if (CurrentDeck.Count > 0 && theList.SelectedItem != null && RightPanel.CurrentMultiverseId() != null)
            {
                foreach (LocalTuple currentTuple in CurrentDeck)
                {
                    if (currentTuple.Name.Equals(theList.SelectedItem) && currentTuple.MultiverseId.Equals(RightPanel.CurrentMultiverseId()))
                    {
                        foundItem = true;
                        currentTuple.Count++;
                    }
                }
            }
            else if (!foundItem && theList.SelectedItem != null && RightPanel.CurrentMultiverseId() != null)
            {
                CurrentDeck.Add(new LocalTuple(1, theList.SelectedItem.ToString(), RightPanel.CurrentMultiverseId()));
            }
            else if (!foundItem && theList.SelectedItem != null)
            {
                if (RightPanel.selectedCard.MultiverseIds.Count > 0)
                {
                    CurrentDeck.Add(new LocalTuple(1, theList.SelectedItem.ToString(), RightPanel.selectedCard.MultiverseIds[0].Item2));
                }
                else
                {
                    CurrentDeck.Add(new LocalTuple(1, theList.SelectedItem.ToString(), null));
                }
            }
        }
    }
}
