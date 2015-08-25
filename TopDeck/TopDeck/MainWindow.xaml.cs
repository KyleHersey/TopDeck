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
using System.Reflection;
using System.Windows.Markup;

namespace TopDeck
{
    public partial class MainWindow : Window
    {
        DecklistManager DLMan;
        DatabaseManager db;
        List<string> recentFiles;
        string theme;

        public MainWindow()
        {

            InitializeComponent();
            this.Closed += MainWindow_Closed;

            DLMan = new DecklistManager(DeckTab.CardList, DeckTab.DeckStats);

            db = new DatabaseManager();

            RecentFilesTabSetup();

            // assign the database manager to relevant classes
            FiltersTab.setDatabaseManager(db);
            DeckTab.CardList.setDatabaseManager(db);
            DeckTab.DeckStats.DBMan = db;

            // assign deck list manager to relevant classes
            DeckTab.CardList.DLMan = DLMan;
            DeckTab.DeckStats.DLMan = DLMan;
            FiltersTab.FilterListPanel.DLMan = DLMan;

            DeckTab.CardList.RightPanel = DeckTab.CardView;
            DeckTab.CardView.SetList.Visibility = System.Windows.Visibility.Collapsed;

            if (File.Exists("theme.txt"))
            {
                using (StreamReader file = new StreamReader("theme.txt"))
                {
                    if (!file.EndOfStream && file.ReadLine().Equals("LightTheme.xaml"))
                    {
                        ChangeTheme(0);
                    }
                    else
                    {
                        ChangeTheme(1);
                    }
                }
            }
            else
            {
                ChangeTheme(1);
            }

            DLMan.update();

            // start out the application with a new deck
            NewDeck_Click(null, null);
        }

        // on close, updated the recent files for the next run
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

            if (File.Exists("theme.txt"))
            {
                File.Delete("theme.txt");
            }

            using (StreamWriter file = new StreamWriter("theme.txt"))
            {
                file.WriteLine(theme);
            }
        }

        // populate the recents buttons
        public void RecentFilesTabSetup()
        {
            recentFiles = new List<string>();
            if (File.Exists("recentFiles.txt"))
            {
                using (StreamReader file = new StreamReader("recentFiles.txt"))
                {
                    while (!file.EndOfStream)
                    {
                        recentFiles.Add(file.ReadLine());
                    }
                }
            }
            else
            {
                File.Create("recentFiles.txt");
            }

            // hide the ability to pick recents until we know that we
            // have recent files to select
            Recents.Visibility = System.Windows.Visibility.Collapsed;
            RecentFileOne.Visibility = System.Windows.Visibility.Collapsed;
            RecentFileTwo.Visibility = System.Windows.Visibility.Collapsed;
            RecentFileThree.Visibility = System.Windows.Visibility.Collapsed;

            List<TextBlock> recentTextBlocks = new List<TextBlock>();
            recentTextBlocks.Add(RecentFileOne);
            recentTextBlocks.Add(RecentFileTwo);
            recentTextBlocks.Add(RecentFileThree);

            char[] delimiters = { '/', '\\' };

            // populate each recent file button
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

        // populate deck with cards from previously saved deck
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

                List<LocalTuple> sideboardCardNames = GetSideboardFromFile(path);
                DLMan.sideboard = new ObservableCollection<LocalTuple>(sideboardCardNames);

                DLMan.update();
            }

            AddToRecentFiles();
            DeckTab.DeckStats.updateStats();
            HideOrShowProxiesButton();
        }

        // get cards in the sideboard from the file
        public List<LocalTuple> GetSideboardFromFile(string fileName)
        {
            List<LocalTuple> cardNames = new List<LocalTuple>();
            StreamReader input = new StreamReader(fileName);

            while (input.EndOfStream == false)
            {
                String tempString = input.ReadLine();
                string[] words = tempString.Split(' ');

                if (!words[0].Equals("SB:"))
                {
                    continue;
                }

                Console.WriteLine(words[2]);

                int multiverseID = -1;
                bool mvIDquery = false;

                if (words.Length == 3)   //if there's a possibility we have a multiverse id...
                {
                    try
                    {
                        Debug.WriteLine(words[2]);
                        multiverseID = Convert.ToInt32(words[2]);   //got multiverseID
                    }
                    catch
                    {
                        try
                        {
                            if (db.GetHighestMultiverseId(words[2]) != null)
                            {
                                //queried for multiverseID
                                multiverseID = Convert.ToInt32(db.GetHighestMultiverseId(words[2]));    //only first word of a cardname
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
                    if (mvIDquery)  //if words[2] is a name
                    {
                        cardNames.Add(new LocalTuple(Convert.ToInt32(words[1]), words[2], multiverseID.ToString()));
                    }
                    else            //if words[2] is a number
                    {
                        cardNames.Add(new LocalTuple(Convert.ToInt32(words[1]), db.GetAName(words[2].ToString()), Convert.ToString(words[1])));
                    }
                }
                else //if we didn't, assume #, cardname
                {
                    int num = Convert.ToInt32(words[1]);
                    string name = words[2];
                    for (int i = 3; i < words.Length; i++)
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

        // get the list of card names form the file
        public List<LocalTuple> GetCardnamesFromFile(string fileName)
        {
            List<LocalTuple> cardNames = new List<LocalTuple>();
            StreamReader input = new StreamReader(fileName);

            while (input.EndOfStream == false)
            {
                String tempString = input.ReadLine();
                string[] words = tempString.Split(' ');

                if (words[0].Equals("SB:"))
                {
                    continue;
                }

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

        // save the deck if it has been previously saved
        // if it has not, bring up the save as dialog
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

        // add card to recents list
        private void AddToRecentFiles()
        {
            // if we have more than 3 recent files, remove the oldest one
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

        // save the deck under a different name
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

        // clear all fields to create a new deck
        private void NewDeck_Click(object sender, RoutedEventArgs e)
        {
            DLMan.currentDeckName = null;

            DLMan.currentDeck = new ObservableCollection<LocalTuple>();
            DLMan.sideboard = new ObservableCollection<LocalTuple>();

            DLMan.update();
            DeckTab.CardView.Clear();

            HideOrShowProxiesButton();
        }

        // updates the database
        private void Update_Click(object sender, RoutedEventArgs e)
        {
            db.UpdateDB();
        }

        // exports the deck to cockatrice
        private void Export_Click(object senter, RoutedEventArgs e)
        {
            // files have .cod extension
            if (DLMan.currentDeck.Count > 0)
            {
                // show a dialog to allow user to select file name and path
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

                        foreach (LocalTuple card in DLMan.currentDeck)
                        {
                            outputFile.WriteLine("        <card number=\"" + card.Count + "\" price=\"0\" name=\"" + card.Name + "\"/>");
                        }
                        outputFile.WriteLine("    </zone>");
                        outputFile.WriteLine("    <zone name=\"side\">");
                        if (DLMan.sideboard.Count > 0)
                        {
                            foreach (LocalTuple card in DLMan.sideboard)
                            {
                                outputFile.WriteLine("        <card number=\"" + card.Count + "\" price=\"0\" name=\"" + card.Name + "\"/>");
                            }
                        }
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

        // open one of the recent files and update UI
        private void OpenRecentFile(int index)
        {
            List<LocalTuple> cardNames = GetCardnamesFromFile(recentFiles[index]);

            DLMan.currentDeck = new ObservableCollection<LocalTuple>(cardNames);

            DLMan.update();

            DeckTab.CardView.Clear();
            HideOrShowProxiesButton();
        }

        // export proxies to html file and open that file in browser
        private void PrintProxies_Click(object sender, RoutedEventArgs e)
        {
            // show dialog to allow user to select name and path
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

        // creates html file of images
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
                foreach (LocalTuple card in DLMan.sideboard)
                {
                    for (int i = 0; i < card.Count; i++)
                    {
                        outputFile.WriteLine("<img src=\"http://gatherer.wizards.com/Handlers/Image.ashx?multiverseid=" + card.MultiverseId + "&type=card\" width=\"222\" height=\"319\"/>");
                    }
                }
                outputFile.WriteLine("</body>");
                outputFile.WriteLine("</html>");
            }

            // open the html file in the browser
            System.Diagnostics.Process.Start(fileName);
        }

        // if no cards are in the deck, don't allow user to generate proxies
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

        private void LightTheme_Click(object sender, RoutedEventArgs e)
        {
            ChangeTheme(0);
        }

        private void DarkTheme_Click(object sender, RoutedEventArgs e)
        {
            ChangeTheme(1);
        }

        private void ChangeTheme(int themeChoice)
        {
            ResourceDictionary dict;
            var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly();
            var path = System.IO.Path.GetDirectoryName(assembly.Location);
            path = path.Replace("bin\\Debug", "");
            path = path.Replace("bin\\Release", "");

            if (themeChoice == 0)
            {
                dict = (ResourceDictionary)XamlReader.Load(new FileStream(System.IO.Path.Combine(path, "LightTheme.xaml"), FileMode.Open));
                theme = "LightTheme.xaml";
            }
            else
            {
                dict = (ResourceDictionary)XamlReader.Load(new FileStream(System.IO.Path.Combine(path, "DarkTheme.xaml"), FileMode.Open));
                theme = "DarkTheme.xaml";
            }

            Application.Current.Resources.MergedDictionaries.Clear();
            Application.Current.Resources.MergedDictionaries.Add(dict);

        }
    }
}