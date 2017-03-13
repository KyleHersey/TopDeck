using Ninject;
using System.Windows.Controls;

namespace TopDeck
{
    public partial class ListCardsView : UserControl
    {
        [Inject]
        public DatabaseManager DBMan
        {
            get;
            set;
        }

        public ListCardsView()
        {
            InitializeComponent();
            FilterFiltersPanel.MiddlePanel = FilterListPanel;
            FilterListPanel.RightPanel = CardPanel;
        }
    }
}
