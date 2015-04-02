using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        

        public MainWindow()
        {
            

            InitializeComponent();

            /* Databasey stuff */
        
            
            /*List<string> cards = m.GetCards("", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "artifact", null, "", "", "");
            foreach (string card in cards)
            {
                Debug.WriteLine(card);
            }*/
            /*Card c = m.GetCard("Sen Triplets");
            Debug.WriteLine(c.Colors.Count);
            foreach (string color in c.Colors) {
                Debug.WriteLine(color);
            }*/

            DatabaseManager db = new DatabaseManager();

            FiltersTab.setDatabaseManager(db);

        }

        private void OpenDeckFile_Click(object sender, RoutedEventArgs e)
        {
            /*Process fileExplorer = Process.Start("explorer.exe"); */
            string path = "";
            OpenFileDialog file = new OpenFileDialog();
            bool? userClickedOK = file.ShowDialog();
            if (userClickedOK == true)
            {
                path = file.SafeFileName;
                Debug.WriteLine(path);
            }

            Regex checkExtension = new Regex(".*\\.dec");
            if (checkExtension.IsMatch(path))
            {
                Debug.WriteLine("yes");
            }
        }

        private void SaveAsDeckFile_Click(object sender, RoutedEventArgs e)
        {
            /*Process fileExplorer = Process.Start("explorer.exe"); */
            string path = "";
            SaveFileDialog file = new SaveFileDialog();
            file.OverwritePrompt = true;

            // Set the file name to myText.txt, set the type filter 
            // to text files, and set the initial directory to the  
            // MyDocuments folder.
            file.FileName = "myDeck";
            // DefaultExt is only used when "All files" is selected from  
            // the filter box and no extension is specified by the user.
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
    }
}