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

        public ObservableCollection<KeyValuePair<string, int>> TypeSource
        {
            get;
            set;
        }

        public ObservableCollection<KeyValuePair<int, int>> CostSource
        {
            get;
            set;
        }
        
        public StatsPanel()
        {
            InitializeComponent();

            ColorSource = new ObservableCollection<KeyValuePair<string, int>>();
            TypeSource = new ObservableCollection<KeyValuePair<string, int>>();
            CostSource = new ObservableCollection<KeyValuePair<int, int>>();

            TypesPie.DataContext = this;
            ColorPie.DataContext = this;
            CostBar.DataContext = this;
        }

        public void updateStats(){
            updateColorChart();
            updateTypesChart();
            updateCostChart();
            Console.WriteLine("updateStats was called");
        }

        public void updateCostChart()
        {
            while (CostSource.Count > 0)
            {
                CostSource.RemoveAt(0);
            }

            Dictionary<int, int> costDict = new Dictionary<int, int>();

            foreach (LocalTuple tup in deck)
            {
                Card card = DBMan.GetACardFromMultiverseId(tup.MultiverseId);
                int intCost = Convert.ToInt32(card.CMC);
                if (costDict.Keys.Contains(intCost))
                {
                    costDict[intCost]++;
                }
                else
                {
                    costDict.Add(intCost, 1);
                }
            }

            List<int> costs = costDict.Keys.ToList<int>();
            costs.Sort();

            foreach (int cost in costs)
            {
                CostSource.Add(new KeyValuePair<int, int>(cost, costDict[cost]));
            }
        }

        public void updateTypesChart(){
            while (TypeSource.Count > 0)
            {
                TypeSource.RemoveAt(0);
            }

            Dictionary<String, int> typeDict = new Dictionary<string, int>();

            foreach (LocalTuple tup in deck)
            {
                Card card = DBMan.GetACardFromMultiverseId(tup.MultiverseId);
                foreach (String type in card.Types)
                {
                    if (typeDict.Keys.Contains(type))
                    {
                        typeDict[type]++;
                    }
                    else
                    {
                        typeDict.Add(type, 1);
                    }
                }
            }

            foreach (String type in typeDict.Keys)
            {
                TypeSource.Add(new KeyValuePair<string, int>(type, typeDict[type]));
            }
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
