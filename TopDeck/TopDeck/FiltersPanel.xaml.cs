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

        private void UserControl_KeyDown(Object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                FilterQuery(null, null);
            }
        }

        // called when "search" is clicked
        // accumulates all of the filters to send to the DBMan
        private void FilterQuery(object sender, RoutedEventArgs e)
        {
            List<string> colors = new List<string>();
            bool requireMulticolor = false;
            bool excludeUnselected = false;
            string legality = "";
            Debug.WriteLine("clicked");
            if (RedColor.isChecked() == true)
            {
                colors.Add("red");
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
            if (ExcludeUnselected.isChecked() == true)
            {
                excludeUnselected = true;
            }

            List<string> types = new List<string>();
            if (ArtifactCheckbox.isChecked())
            {
                types.Add("Artifact");
            }
            if (CreatureCheckbox.isChecked())
            {
                types.Add("Creature");
            }
            if (EnchantmentCheckbox.isChecked())
            {
                types.Add("Enchantment");
            }
            if (InstantCheckbox.isChecked())
            {
                types.Add("Instant");
            }
            if (SorceryCheckbox.isChecked())
            {
                types.Add("Sorcery");
            }
            if (LandCheckbox.isChecked())
            {
                types.Add("Land");
            }
            if (PlaneswalkerCheckbox.isChecked())
            {
                types.Add("Planeswalker");
            }

            List<string> subtypes = SubtypeTextbox.Input.Text.Split(' ').ToList<string>();
            if (subtypes[0].Equals("")) { subtypes.RemoveAt(0); }

            List<string> supertypes = Supertypes.Input.Text.Split(' ').ToList<string>();
            if (supertypes[0].Equals("")) { supertypes.RemoveAt(0); }

            List<string> rarities = new List<string>();
            if (Common.isChecked())
            {
                rarities.Add("Common");
            }
            if (Uncommon.isChecked())
            {
                rarities.Add("Uncommon");
            }
            if (Rare.isChecked())
            {
                rarities.Add("Rare");
            }
            if(MythicRare.isChecked())
            {
                rarities.Add("Mythic");
            }

            switch (Legality.SelectedIndex)
            {
                case 0:
                    legality = "";
                    break;
                case 1:
                    legality = "modern";
                    break;
                case 2:
                    legality = "legacy";
                    break;
                case 3:
                    legality = "vintage";
                    break;
                case 4:
                    legality = "freeform";
                    break;
                case 5:
                    legality = "prismatic";
                    break;
                case 6:
                    legality = "tribal";
                    break;
                case 7:
                    legality = "singleton";
                    break;
                case 8:
                    legality = "commander";
                    break;
                default: break;
            }

            List<string> setNames = Sets.Input.Text.Split(',').ToList<string>();
            for (int i = 0; i < setNames.Count; i++)
            {
                setNames[i] = setNames[i].Trim();
            }

            MiddlePanel.setItemsSource(DBMan.GetCards(
                NameFilterField.Input.Text,                 //Name
                ToughnessFilterField.Input.Text,            //Toughness
                "",                                         //Hand - not implemented
                ConvertedManaCostFilterField.Input.Text,    //Converted Mana Cost
                MultiverseID.Input.Text,                    //Multiverse ID
                LoyaltyFilterField.Input.Text,              //Loyalty
                rarities,                                   //Rarity
                Flavor.Input.Text,                          //Flavor
                Artist.Input.Text,                          //Artist
                PowerFilterField.Input.Text,                //Power
                Text.Input.Text,                            //Card Text
                types,                                      //Types
                Reserved.isChecked(),
                requireMulticolor,                          //Require Multicolored
                excludeUnselected,                          //Exclude Unselected
                legality,                                   //Legality
                setNames,
                colors,                                     //Colors
                subtypes,                                   //Subtype
                supertypes));                               //Supertype
        }

        public void setDatabaseManager(DatabaseManager db)
        {
            DBMan = db;
        }
    }
}
