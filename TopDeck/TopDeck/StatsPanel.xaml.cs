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

        public DecklistManager DLMan
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

        public void removeCard(String name) { }

        public void addCard(String name)
        {

            Card card = DBMan.GetCard(name);

            //update colors
            foreach (String color in card.Colors)
            {
                for (int i = 0; i < ColorSource.Count; i++)
                {
                    if (ColorSource[i].Key.Equals(color))
                    {
                        KeyValuePair<String, int> newKV = new KeyValuePair<string, int>(ColorSource[i].Key, ColorSource[i].Value + 1);
                        ColorSource.RemoveAt(i);
                        ColorSource.Insert(i, newKV);
                    }
                }
            }
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

            foreach (LocalTuple tup in DLMan.currentDeck)
            {
                Card card = DBMan.GetACardFromMultiverseId(tup.MultiverseId);
                int intCost = Convert.ToInt32(card.CMC);
                if (costDict.Keys.Contains(intCost))
                {
                    costDict[intCost] += tup.Count;
                }
                else
                {
                    costDict.Add(intCost, tup.Count);
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

            foreach (LocalTuple tup in DLMan.currentDeck)
            {
                Card card = DBMan.GetACardFromMultiverseId(tup.MultiverseId);
                foreach (String type in card.Types)
                {
                    if (typeDict.Keys.Contains(type))
                    {
                        typeDict[type] += tup.Count;
                    }
                    else
                    {
                        typeDict.Add(type, tup.Count);
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

            //add each card from deck into dictionary
            foreach(LocalTuple tup in DLMan.currentDeck){
                Card card = DBMan.GetACardFromMultiverseId(tup.MultiverseId);
                if(card.Colors.Count == 0){
                    if (colorDict.Keys.Contains("Colorless"))
                    {
                        colorDict["Colorless"] += tup.Count;
                    }
                    else
                    {
                        colorDict.Add("Colorless", tup.Count);
                    }              
                }
                foreach (String color in card.Colors)
                {
                    if (colorDict.Keys.Contains(color))
                    {
                        colorDict[color] += tup.Count;
                    }
                    else
                    {
                        colorDict.Add(color, tup.Count);
                    }
                }
            }

            //go through dictionary
            foreach (String color in colorDict.Keys)
            {
                    ColorSource.Add(new KeyValuePair<string, int>(color, colorDict[color]));
            }
        }
    }
}
