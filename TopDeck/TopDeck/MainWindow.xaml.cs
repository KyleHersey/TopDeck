using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<LocalTuple> currentDeck;
        DatabaseManager db;

        public MainWindow()
        {
            InitializeComponent();

            currentDeck = new List<LocalTuple>();

            db = new DatabaseManager();

            FiltersTab.setDatabaseManager(db);
            DeckTab.CardList.setDatabaseManager(db);
            DeckTab.CardList.RightPanel = DeckTab.CardView;
            DeckTab.CardView.SetList.Visibility = System.Windows.Visibility.Collapsed;

        }

        private void OpenDeckFile_Click(object sender, RoutedEventArgs e)
        {
            string path = "";
            OpenFileDialog file = new OpenFileDialog();
            bool? userClickedOK = file.ShowDialog();
            if (userClickedOK == true)
            {
                path = file.SafeFileName;

                currentDeck = GetCardnamesFromFile(path);
                FiltersTab.FilterListPanel.CurrentDeck = currentDeck;
                DeckTab.CardList.CurrentDeck = currentDeck;
                DeckTab.CardList.setItemsSource();
            }

            Regex checkExtension = new Regex(".*\\.dec");
            if (checkExtension.IsMatch(path))
            {
                Debug.WriteLine("yes");
            }
        }

        public List<LocalTuple> GetCardnamesFromFile(string fileName)
        {
            List<LocalTuple> cardNames = new List<LocalTuple>();
            StreamReader input = new StreamReader(fileName);

            while (input.EndOfStream == false)
            {
                String tempString = input.ReadLine();
                string[] words = tempString.Split(' ');
                // int[] nums = new int[words.Length];           used in previous parsing method
                int multiverseID = -1;
                bool mvIDquery = false;

                if (words.Length == 2)   //if there's a possibility we have a multiverse id...
                {
                    try
                    {
                        Debug.WriteLine(words[1]);
                        multiverseID = Convert.ToInt32(words[1]);   //got multiverseID
                    }
                    catch
                    {
                        try
                        {
                            if (db.GetHighestMultiverseId(words[1]) != null)
                            {
                                //queried for multiverseID
                                multiverseID = Convert.ToInt32(db.GetHighestMultiverseId(words[1]));    //only first word of a cardname
                                mvIDquery = true;
                            }
                        }
                        catch
                        {
                            multiverseID = -1;
                        }
                    }
                }

                if (multiverseID != -1) //if we got a multiverseID
                {
                    if (mvIDquery)  //if words[1] is a name
                    {
                        cardNames.Add(new LocalTuple(Convert.ToInt32(words[0]), words[1], multiverseID.ToString()));
                    }
                    else            //if words[1] is a number
                    {
                        cardNames.Add(new LocalTuple(Convert.ToInt32(words[0]), db.GetAName(words[1].ToString()), Convert.ToString(words[1])));
                    }
                }
                else //if we didn't, assume #, cardname
                {
                    int num = Convert.ToInt32(words[0]);
                    string name = words[1];
                    for (int i = 2; i < words.Length; i++)
                    {
                        name += " ";
                        name += words[i];
                    }
                    cardNames.Add(new LocalTuple(num, name, db.GetHighestMultiverseId(name)));
                }
            }

            return cardNames;
        }

        private void SaveDeck_Click(object sender, RoutedEventArgs e)
        {
            // will need to check if deck has not been saved before. 
            // for now, save as default name
            string deckName = "emeraldsdeck.dec";
            using (StreamWriter file = new StreamWriter(deckName))
            {
                foreach (LocalTuple currentCard in currentDeck)
                {
                    if (currentCard.MultiverseId != null)
                    {
                        file.WriteLine(currentCard.Count + " " + currentCard.MultiverseId);
                    }
                    else
                    {
                        file.WriteLine(currentCard.Count + " " + currentCard.Name);
                    }
                }
            }
        }

        private void SaveAsDeckFile_Click(object sender, RoutedEventArgs e)
        {
            string path = "";
            SaveFileDialog file = new SaveFileDialog();
            file.OverwritePrompt = true;

            file.FileName = "myDeck";
            file.DefaultExt = "dec";
            file.Filter =
                "Deck files (*.dec)|*.dec|All files (*.*)|*.*";
            bool? userClickedOK = file.ShowDialog();
            if (userClickedOK == true)
            {
                path = file.SafeFileName;
                Debug.WriteLine(path);
            }
        }

        private void NewDeck_Click(object sender, RoutedEventArgs e)
        {
            // don't forget to ask if they want to save?


            // in here we want to create a new list and set all the things to reference it
            currentDeck = new List<LocalTuple>();
            FiltersTab.FilterListPanel.CurrentDeck = currentDeck;
            DeckTab.CardList.CurrentDeck = currentDeck;

        }
    }
}