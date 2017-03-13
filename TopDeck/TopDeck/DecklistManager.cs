using Ninject;
using System;
using System.Collections.ObjectModel;

namespace TopDeck
{
    public class DecklistManager
    {
        public string currentDeckName;  //name of the currently loaded deck

        //list describing the current deck
        public ObservableCollection<LocalTuple> currentDeck
        {
            get;
            set;
        }

        //list describing the current sideboard
        public ObservableCollection<LocalTuple> sideboard
        {
            get;
            set;
        }

        //reference to the Decklist Panel
        public DecklistPanel decklistPanel
        {
            get;
            set;
        }

        //reference to the Stats Panel
        [Inject]
        public StatsPanel statsPanel
        {
            get;
            set;
        }

        public DecklistManager(DecklistPanel dlPan)
        //public DecklistManager()
        {
            //no deck currently loaded
            currentDeckName = "";
            currentDeck = new ObservableCollection<LocalTuple>();
            sideboard = new ObservableCollection<LocalTuple>();

            //set references to gui
            decklistPanel = dlPan;
            decklistPanel.theList.ItemsSource = currentDeck;
            decklistPanel.theSideboard.ItemsSource = sideboard;
        }

        //update the gui with decklist information
        public void update(){
            decklistPanel.setItemsSource(); 
            decklistPanel.setSideboardItemsSource();
            decklistPanel.theList.Items.Refresh();
            decklistPanel.theSideboard.Items.Refresh();
            decklistPanel.UpdateCardCounts();

            statsPanel.updateStats();
        }

        //tell the stats panel to remove a card from the deck
        public void removeCard(String name){
            statsPanel.removeCard(name);
        }

        //tell the stats panel to add a card from the deck
        public void addCard(LocalTuple selectedCard)
        {
            foreach (LocalTuple card in currentDeck)
            {
                if (card.Name.Equals(selectedCard.Name) && card.MultiverseId.Equals(selectedCard.MultiverseId))
                {
                    card.Count++;
                }
            }
            decklistPanel.theList.Items.Refresh();
            statsPanel.addCard(selectedCard.Name);
        }

        public void addCardToSideboard(LocalTuple selectedCard)
        {
            foreach (LocalTuple card in sideboard)
            {
                if (card.Name.Equals(selectedCard.Name) && card.MultiverseId.Equals(selectedCard.MultiverseId))
                {
                    card.Count++;
                }
            }
            decklistPanel.theSideboard.Items.Refresh();
        }
    }
}
