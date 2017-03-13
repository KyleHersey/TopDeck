using Ninject.Modules;

namespace TopDeck
{
    class TopdeckModule : NinjectModule
    {
        private DecklistPanel decklistPanel;
        private StatsPanel statsPanel;

        public TopdeckModule(DecklistPanel dlPanel, StatsPanel sPanel)
        {
            decklistPanel = dlPanel;
            statsPanel = sPanel;
        }
        public override void Load()
        {
            DatabaseManager db = new DatabaseManager();
            Bind<DatabaseManager>().ToConstant(db);
            Bind<StatsPanel>().ToConstant(statsPanel);

            DecklistManager dl = new DecklistManager(decklistPanel);
            Bind<DecklistManager>().ToConstant(dl);
        }
    }
}
