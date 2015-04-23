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
using System.Diagnostics;

namespace TopDeck
{
    /// <summary>
    /// Interaction logic for FiltersPanel.xaml
    /// </summary>
    public partial class FiltersPanel : UserControl
    {

        public DatabaseManager DBMan
        {
            get;
            set;
        }
        
        public ListPanel MiddlePanel
        {
            get;
            set;
        }

        public FiltersPanel()
        {
            InitializeComponent();

        }

        private void FilterQuery(object sender, RoutedEventArgs e)
        {
            List<string> colors = new List<string>();
            bool requireMulticolor = false;
            Debug.WriteLine("clicked");
            if (RedColor.isChecked() == true)
            {
                Debug.WriteLine("stuff");
                colors.Add("red");
                //MiddlePanel.theList.ItemsSource
            }
            if (BlueColor.isChecked() == true)
            {
                colors.Add("blue");
            }
            if (WhiteColor.isChecked() == true)
            {
                colors.Add("white");
            }
            if (GreenColor.isChecked() == true)
            {
                colors.Add("green");
            }
            if (BlackColor.isChecked() == true)
            {
                colors.Add("black");
            }
            if (RequireMulticolored.isChecked() == true)
            {
                requireMulticolor = true;
            }
            // what about colorless?
            MiddlePanel.setItemsSource(DBMan.GetCards(
                NameFilterField.Input.Text,                 //Name
                ToughnessFilterField.Input.Text,            //Toughness
                "",                                         //Hand
                ConvertedManaCostFilterField.Input.Text,    //Converted Mana Cost
                "",                                         //Release Date
                "",                                         //Border
                "",                                         //Watermark
                "",                                         //Multiverse ID
                LoyaltyFilterField.Input.Text,              //Loyalty
                "",                                         //Rarity
                Flavor.Input.Text,                          //Flavor
                Artist.Input.Text,                          //Artist
                "",                                         //Number
                PowerFilterField.Input.Text,                //Power
                "",                                         //Life
                "",                                         //Image Name
                Text.Input.Text,                            //Card Text
                "",                                         //Types
                null,                                       //Timeshifted
                requireMulticolor,                          //Require Multicolored
                colors,                                     //Colors
                "",                                         //subtype
                ""));                                       //supertype
        }

        public void setDatabaseManager(DatabaseManager db)
        {
            DBMan = db;
        }
    }
}
