using Ninject.Modules;

namespace TopDeck
{
    class TopdeckModule : NinjectModule
    {
        private DecklistPanel decklistPanel;
        private StatsPanel statsPanel;
        private ListPanel listPanel;
        private CardPanel cardPanel;

        public TopdeckModule(DecklistPanel dlPanel, StatsPanel sPanel, ListPanel lPanel, CardPanel cPanel)
        {
            decklistPanel = dlPanel;
            statsPanel = sPanel;
            listPanel = lPanel;
            cardPanel = cPanel;
        }
        public override void Load()
        {
            DatabaseManager db = new DatabaseManager();
            Bind<DatabaseManager>().ToConstant(db);

            //Bind<ListPanel>().ToConstant(listPanel);
            //Bind<DecklistPanel>().ToConstant(decklistPanel);
            //Bind<StatsPanel>().ToConstant(statsPanel);
            //Bind<CardPanel>().ToConstant(cardPanel);

            DecklistManager dl = new DecklistManager(decklistPanel, statsPanel);
            Bind<DecklistManager>().ToConstant(dl);
        }
    }
}
