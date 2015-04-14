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

        public MainWindow()
        {
            InitializeComponent();

            currentDeck = new List<LocalTuple>();

            DatabaseManager db = new DatabaseManager();

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
                Debug.WriteLine(path);
                DeckTab.CardList.openFile(path);
            }

            Regex checkExtension = new Regex(".*\\.dec");
            if (checkExtension.IsMatch(path))
            {
                Debug.WriteLine("yes");
            }
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


        }
    }
}