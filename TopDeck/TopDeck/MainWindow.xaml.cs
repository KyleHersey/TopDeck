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
        DecklistManager DLMan;
        DatabaseManager db;
        List<string> recentFiles;

        public MainWindow()
        {

            InitializeComponent();
            this.Closed += MainWindow_Closed;

            DLMan = new DecklistManager(DeckTab.CardList, DeckTab.DeckStats);

            db = new DatabaseManager();

            RecentFilesTabSetup();

            FiltersTab.setDatabaseManager(db);
            DeckTab.CardList.setDatabaseManager(db);
            DeckTab.DeckStats.DBMan = db;

            DeckTab.CardList.DLMan = DLMan;

            DeckTab.CardList.RightPanel = DeckTab.CardView;
            DeckTab.CardView.SetList.Visibility = System.Windows.Visibility.Collapsed;

            FiltersTab.FilterListPanel.CurrentDeck = DLMan.currentDeck;
            FiltersTab.FilterListPanel.Sideboard = DLMan.sideboard;

            DLMan.update();
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
            RecentFileTwo.Visibility = System.Windows.Visibility.Collapsed;
            RecentFileThree.Visibility = System.Windows.Visibility.Collapsed;

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
                DLMan.currentDeckName = file.FileName;

                path = file.FileName;

                List<LocalTuple> cardNames = GetCardnamesFromFile(path);

                DLMan.currentDeck = new ObservableCollection<LocalTuple>(cardNames);

                FiltersTab.FilterListPanel.CurrentDeck = DLMan.currentDeck;
                FiltersTab.FilterListPanel.Sideboard = DLMan.sideboard;

                DLMan.update();
            }

            Regex checkExtension = new Regex(".*\\.dec");
            if (checkExtension.IsMatch(path))
            {
                Debug.WriteLine("yes");
            }

            AddToRecentFiles();
            DeckTab.DeckStats.updateStats();
            HideOrShowProxiesButton();
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
            if (DLMan.currentDeckName == null)
            {
                SaveAsDeckFile_Click(sender, e);
                return;
            }
            else
            {
                if (File.Exists(DLMan.currentDeckName))
                {
                    File.Delete(DLMan.currentDeckName);
                }

                using (StreamWriter file = new StreamWriter(DLMan.currentDeckName))
                {
                    foreach (LocalTuple currentCard in DLMan.currentDeck)
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
            if (!recentFiles.Contains(DLMan.currentDeckName) && recentFiles.Count < 3)
            {
                recentFiles.Add(DLMan.currentDeckName);
            }
            else if (!recentFiles.Contains(DLMan.currentDeckName) && recentFiles.Count >= 3)
            {
                recentFiles.RemoveAt(2);
                recentFiles.Insert(0, DLMan.currentDeckName);
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

                DLMan.currentDeckName = fileName;
                using (StreamWriter outputFile = new StreamWriter(fileName))
                {
                    foreach (LocalTuple card in DLMan.currentDeck)
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
            DLMan.currentDeckName = null;

            // in here we want to create a new list and set all the things to reference it
            DLMan.currentDeck = new ObservableCollection<LocalTuple>();
            DLMan.sideboard = new ObservableCollection<LocalTuple>();

            FiltersTab.FilterListPanel.CurrentDeck = DLMan.currentDeck;
            FiltersTab.FilterListPanel.Sideboard = DLMan.sideboard;

            DLMan.update();
            DeckTab.CardView.Clear();

            HideOrShowProxiesButton();
        }

        private void Update_Click(object sender, RoutedEventArgs e)
        {
            db.UpdateDB();
        }

        private void Export_Click(object senter, RoutedEventArgs e)
        {
            // files have .cod extension
            if (DLMan.currentDeck.Count > 0)
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
                        foreach (LocalTuple card in DLMan.currentDeck)
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

            DLMan.currentDeck = new ObservableCollection<LocalTuple>(cardNames);

            FiltersTab.FilterListPanel.CurrentDeck = DLMan.currentDeck;
            FiltersTab.FilterListPanel.Sideboard = DLMan.sideboard;

            DLMan.update();

            DeckTab.CardView.Clear();
            HideOrShowProxiesButton();
        }

        private void PrintProxies_Click(object sender, RoutedEventArgs e)
        {
            string fileName = "";
            SaveFileDialog file = new SaveFileDialog();
            file.OverwritePrompt = true;

            file.FileName = "*";
            file.DefaultExt = "html";
            file.Filter =
                "html files (*.html)|*.html|All files (*.*)|*.*";
            bool? userClickedOK = file.ShowDialog();
            if (userClickedOK == true)
            {
                fileName = file.FileName;

                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }

                CreateProxyFile(fileName);
            }
        }

        private void CreateProxyFile(string fileName)
        {
            using (StreamWriter outputFile = new StreamWriter(fileName))
            {
                outputFile.WriteLine("<!DOCTYPE html>");
                outputFile.WriteLine("<html>");
                outputFile.WriteLine("<body>");
                outputFile.WriteLine("<style type=\"text/css\">@media print{@page {size: landscape}} img {margin-bottom: -5px; margin-right: -5px}</style>");

                foreach (LocalTuple card in DLMan.currentDeck)
                {
                    for (int i = 0; i < card.Count; i++)
                    {
                        outputFile.WriteLine("<img src=\"http://gatherer.wizards.com/Handlers/Image.ashx?multiverseid=" + card.MultiverseId + "&type=card\" width=\"222\" height=\"319\"/>");
                    }
                }
                outputFile.WriteLine("</body>");
                outputFile.WriteLine("</html>");
            }
            System.Diagnostics.Process.Start(fileName);
        }

        private void HideOrShowProxiesButton()
        {
            if (DLMan.currentDeck.Count > 0)
            {
                ProxiesButton.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                ProxiesButton.Visibility = System.Windows.Visibility.Collapsed;
            }
        }
    }
}