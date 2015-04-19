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
using System.Collections.ObjectModel;

namespace TopDeck
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string currentDeckName;
        ObservableCollection<LocalTuple> currentDeck;
        DatabaseManager db;
        List<string> recentFiles;

        public MainWindow()
        {

            InitializeComponent();
            this.Closed += MainWindow_Closed;

            currentDeck = new ObservableCollection<LocalTuple>();

            db = new DatabaseManager();

            RecentFilesTabSetup();

            FiltersTab.setDatabaseManager(db);
            DeckTab.CardList.setDatabaseManager(db);
            DeckTab.CardList.RightPanel = DeckTab.CardView;
            DeckTab.CardView.SetList.Visibility = System.Windows.Visibility.Collapsed;
            FiltersTab.FilterListPanel.CurrentDeck = currentDeck;
            DeckTab.CardList.CurrentDeck = currentDeck;
            DeckTab.CardList.setItemsSource();
            DeckTab.DeckStats.deck = currentDeck;
            DeckTab.DeckStats.DBMan = db;

            NewDeck_Click(null, null);

        }

        void MainWindow_Closed(object sender, EventArgs e)
        {
            if (File.Exists("recentFiles.txt"))
            {
                File.Delete("recentFiles.txt");
            }
            using (StreamWriter file = new StreamWriter("recentFiles.txt"))
            {
                foreach (string recentFile in recentFiles)
                {
                    file.WriteLine(recentFile);
                }
            }
        }

        public void RecentFilesTabSetup()
        {
            recentFiles = new List<string>();
            using (StreamReader file = new StreamReader("recentFiles.txt"))
            {
                while (!file.EndOfStream)
                {
                    recentFiles.Add(file.ReadLine());
                }
            }

            Recents.Visibility = System.Windows.Visibility.Collapsed;
            RecentFileOne.Visibility = System.Windows.Visibility.Collapsed;
            //RecentFileTwo.Visibility = System.Windows.Visibility.Collapsed;
            //RecentFileThree.Visibility = System.Windows.Visibility.Collapsed;

            List<TextBlock> recentTextBlocks = new List<TextBlock>();
            recentTextBlocks.Add(RecentFileOne);
            recentTextBlocks.Add(RecentFileTwo);
            recentTextBlocks.Add(RecentFileThree);

            char[] delimiters = { '/', '\\' };

            int index = 0;
            foreach (string recentfile in recentFiles)
            {
                string[] fileSections = recentfile.Split(delimiters);
                recentTextBlocks[index].Text = fileSections[fileSections.Length - 1];

                Recents.Visibility = System.Windows.Visibility.Visible;
                recentTextBlocks[index].Visibility = System.Windows.Visibility.Visible;

                index++;
            }
        }

        private void OpenDeckFile_Click(object sender, RoutedEventArgs e)
        {
            string path = "";
            OpenFileDialog file = new OpenFileDialog();
            bool? userClickedOK = file.ShowDialog();
            if (userClickedOK == true)
            {
                currentDeckName = file.SafeFileName;

                path = file.FileName;

                List<LocalTuple> cardNames = GetCardnamesFromFile(path);

                currentDeck = new ObservableCollection<LocalTuple>(cardNames);

                FiltersTab.FilterListPanel.CurrentDeck = currentDeck;
                DeckTab.CardList.CurrentDeck = currentDeck;
                DeckTab.DeckStats.deck = currentDeck;

                DeckTab.CardList.setItemsSource();
            }

            Regex checkExtension = new Regex(".*\\.dec");
            if (checkExtension.IsMatch(path))
            {
                Debug.WriteLine("yes");
            }

            AddToRecentFiles();
            DeckTab.DeckStats.updateStats();
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

            input.Close();

            return cardNames;
        }

        private void SaveDeck_Click(object sender, RoutedEventArgs e)
        {
            if (currentDeckName == null)
            {
                SaveAsDeckFile_Click(sender, e);
                return;
            }
            else
            {
                if (File.Exists(currentDeckName))
                {
                    File.Delete(currentDeckName);
                }

                using (StreamWriter file = new StreamWriter(currentDeckName))
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
            AddToRecentFiles();
        }

        private void AddToRecentFiles()
        {
            if (!recentFiles.Contains(currentDeckName) && recentFiles.Count < 3)
            {
                recentFiles.Add(currentDeckName);
            }
            else if (!recentFiles.Contains(currentDeckName) && recentFiles.Count > 3)
            {
                recentFiles.RemoveAt(0);
                recentFiles.Add(currentDeckName);
            }
        }

        private void SaveAsDeckFile_Click(object sender, RoutedEventArgs e)
        {
            string fileName = "";
            SaveFileDialog file = new SaveFileDialog();
            file.OverwritePrompt = true;

            file.FileName = "*";
            file.DefaultExt = "dec";
            file.Filter =
                "Deck files (*.dec)|*.dec|All files (*.*)|*.*";
            bool? userClickedOK = file.ShowDialog();
            if (userClickedOK == true)
            {
                fileName = file.FileName;

                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }

                currentDeckName = fileName;
                using (StreamWriter outputFile = new StreamWriter(fileName))
                {
                    foreach (LocalTuple card in currentDeck)
                    {
                        if (card.MultiverseId != null)
                        {
                            outputFile.WriteLine(card.Count + " " + card.MultiverseId);
                        }
                        else
                        {
                            outputFile.WriteLine(card.Count + " " + card.Name);
                        }
                    }
                }
            }
            AddToRecentFiles();
        }

        private void NewDeck_Click(object sender, RoutedEventArgs e)
        {
            // don't forget to ask if they want to save?
            currentDeckName = null;

            // in here we want to create a new list and set all the things to reference it
            currentDeck = new ObservableCollection<LocalTuple>();
            FiltersTab.FilterListPanel.CurrentDeck = currentDeck;
            DeckTab.CardList.CurrentDeck = currentDeck;
            DeckTab.CardList.setItemsSource();

            DeckTab.DeckStats.updateStats();

        }

        private void Update_Click(object sender, RoutedEventArgs e)
        {
            db.UpdateDB();
        }

        private void Export_Click(object senter, RoutedEventArgs e)
        {
            // files have .cod extension
            if (currentDeck.Count > 0)
            {
                string fileName = "";
                SaveFileDialog file = new SaveFileDialog();
                file.OverwritePrompt = true;

                file.FileName = "*";
                file.DefaultExt = "cod";
                file.Filter =
                    "Cockatrice files (*.cod)|*.cod|All files (*.*)|*.*";
                bool? userClickedOK = file.ShowDialog();
                if (userClickedOK == true)
                {
                    fileName = file.FileName;

                    if (File.Exists(fileName))
                    {
                        File.Delete(fileName);
                    }
                    using (StreamWriter outputFile = new StreamWriter(fileName))
                    {
                        outputFile.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                        outputFile.WriteLine("<cockatrice_deck version=\"1\">");
                        outputFile.WriteLine("    <deckname></deckname>");
                        outputFile.WriteLine("    <comments></comments>");
                        outputFile.WriteLine("    <zone name=\"main\">");
                        //        <card number="1" price="0" name="Leyline of the Void"/>
                        foreach (LocalTuple card in currentDeck)
                        {
                            outputFile.WriteLine("        <card number=\"" + card.Count + "\" price=\"0\" name=\"" + card.Name + "\"/>");
                        }
                        outputFile.WriteLine("    </zone>");
                        outputFile.WriteLine("    <zone name=\"side\">");
                        outputFile.WriteLine("    </zone>");
                        outputFile.WriteLine("</cockatrice_deck>");
                    }
                }
            }
        }

        private void RecentFileOne_Click(object sender, RoutedEventArgs e)
        {
            OpenRecentFile(0);
        }

        private void RecentFileTwo_Click(object sender, RoutedEventArgs e)
        {
            OpenRecentFile(1);
        }

        private void RecentFileThree_Click(object sender, RoutedEventArgs e)
        {
            OpenRecentFile(2);
        }

        private void OpenRecentFile(int index)
        {
            List<LocalTuple> cardNames = GetCardnamesFromFile(recentFiles[index]);
            currentDeck = new ObservableCollection<LocalTuple>(cardNames);

            FiltersTab.FilterListPanel.CurrentDeck = currentDeck;
            DeckTab.CardList.CurrentDeck = currentDeck;
            DeckTab.CardList.setItemsSource();
        }
    }
}