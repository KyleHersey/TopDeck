using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace TopDeck
{
    public class DecklistManager
    {
        public string currentDeckName;
        public ObservableCollection<LocalTuple> currentDeck
        {
            get;
            set;
        }

        public ObservableCollection<LocalTuple> sideboard
        {
            get;
            set;
        }

        public DecklistPanel decklistPanel
        {
            get;
            set;
        }

        public StatsPanel statsPanel
        {
            get;
            set;
        }

        public DecklistManager(DecklistPanel dlPan, StatsPanel sPan)
        {
            currentDeckName = "";
            currentDeck = new ObservableCollection<LocalTuple>();
            sideboard = new ObservableCollection<LocalTuple>();
            decklistPanel = dlPan;
            statsPanel = sPan;

            decklistPanel.theList.ItemsSource = currentDeck;
            decklistPanel.theSideboard.ItemsSource = sideboard;
        }

        public void update(){
            decklistPanel.setItemsSource(); 
            decklistPanel.setSideboardItemsSource();
            decklistPanel.theList.Items.Refresh();
            decklistPanel.theSideboard.Items.Refresh();

            statsPanel.updateStats();
        }

        public void removeCard(String name){
            statsPanel.removeCard(name);
        }

        public void addCard(String name)
        {
            foreach (LocalTuple card in currentDeck)
            {
                if (card.Name.Equals(name))
                {
                    card.Count++;
                }
            }
            decklistPanel.theList.Items.Refresh();
            statsPanel.addCard(name);
        }

        public void addCardToSideboard(String name)
        {
            foreach (LocalTuple card in sideboard)
            {
                if (card.Name.Equals(name))
                {
                    card.Count++;
                }
            }
            decklistPanel.theSideboard.Items.Refresh();
        }
    }
}
