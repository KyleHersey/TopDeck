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
        }

        public void openFile(string path)
        {
            theList.ItemsSource = DLMan.GetCardnamesFromFile(path);
        }

        public void setItemsSource(List<string> names)
        {
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
                Tuple<int, string, string> tempTuple = (Tuple<int, string, string>) theList.SelectedItem;
                Card c = DBMan.GetACardFromMultiverseId(tempTuple.Item3);
                RightPanel.setCard(c);
                if (c.MultiverseIds.Count > 0)
                {
                    RightPanel.UpdateImage(tempTuple.Item3);
                }
            }
        }
    }
}
