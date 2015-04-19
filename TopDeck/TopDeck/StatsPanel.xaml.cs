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
using System.Collections.ObjectModel;

namespace TopDeck
{
    /// <summary>
    /// Interaction logic for StatsPanel.xaml
    /// </summary>
    public partial class StatsPanel : UserControl
    {
        public DatabaseManager DBMan
        {
            get;
            set;
        }

        public ObservableCollection<LocalTuple> deck
        {
            get;
            set;
        }

        public ObservableCollection<KeyValuePair<string, int>> ColorSource
        {
            get;
            set;
        }
        
        public StatsPanel()
        {
            InitializeComponent();

            ColorSource = new ObservableCollection<KeyValuePair<string, int>>{
                new KeyValuePair<string, int>("White", 1),
                new KeyValuePair<string, int>("Blue", 2),
                new KeyValuePair<string, int>("Black", 1),
                new KeyValuePair<string, int>("Red", 8),
                new KeyValuePair<string, int>("Green", 16)
            };

            ColorPie.DataContext = this;
        }

        public void updateStats(){
            updateColorChart();
            Console.WriteLine("updateStats was called");
        }

        public void updateColorChart()
        {
            while (ColorSource.Count > 0)
            {
                ColorSource.RemoveAt(0);
            }

            //initialize list before going through deck
            Dictionary<String, int> colorDict = new Dictionary<String, int>();
            colorDict.Add("White", 0);
            colorDict.Add("Blue", 0);
            colorDict.Add("Black", 0);
            colorDict.Add("Red", 0);
            colorDict.Add("Green", 0);
            colorDict.Add("Colorless", 0);

            //add each card from deck into dictionary
            foreach(LocalTuple tup in deck){
                Card tempCard = DBMan.GetACardFromMultiverseId(tup.MultiverseId);
                if(tempCard.Colors.Count == 0){
                    colorDict["Colorless"]++;
                }
                foreach (String color in tempCard.Colors)
                {
                    colorDict[color] += tup.Count;
                }
            }

            //go through dictionary
            ObservableCollection<KeyValuePair<String, int>> newColors = new ObservableCollection<KeyValuePair<string, int>>();
            foreach (String color in colorDict.Keys)
            {
                if (colorDict[color] > 0)
                {
                    ColorSource.Add(new KeyValuePair<string, int>(color, colorDict[color]));
                }
            }
        }
    }
}
