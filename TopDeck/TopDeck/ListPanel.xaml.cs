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
                RightPanel.setCard(DBMan.GetCard(theList.SelectedItem.ToString()));
            }
        }
    }
}
