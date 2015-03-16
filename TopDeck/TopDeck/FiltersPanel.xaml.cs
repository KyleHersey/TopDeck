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
    /// Interaction logic for FiltersPanel.xaml
    /// </summary>
    public partial class FiltersPanel : UserControl
    {

        public List<String> RarityList
        {
            get{
                List<String> tempList = new List<String>();
                tempList.Add("Rarity");
                tempList.Add("Rare");
                tempList.Add("Uncommon");
                tempList.Add("Common");

                return tempList;
            }
        }

        public FiltersPanel()
        {
            InitializeComponent();

            
        }

        private void TypeFilterField_KeyUp(object sender, KeyEventArgs e)
        {
            FilterRowTextBox sentBy = (FilterRowTextBox)sender;
                if (sentBy.Input.Text == "Creature")
                {
                    PowerFilterField.Visibility = System.Windows.Visibility.Visible;
                    ToughnessFilterField.Visibility = System.Windows.Visibility.Visible;
                }
                else
                {
                    PowerFilterField.Visibility = System.Windows.Visibility.Collapsed;
                    ToughnessFilterField.Visibility = System.Windows.Visibility.Collapsed;
                }
        }
    }
}
