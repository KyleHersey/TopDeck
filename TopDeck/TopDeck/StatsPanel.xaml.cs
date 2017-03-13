using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using Ninject;

namespace TopDeck
{
    public partial class StatsPanel : UserControl
    {
        [Inject]
        public DatabaseManager DBMan
        {
            get;
            set;
        }

        [Inject]
        public DecklistManager DLMan
        {
            get;
            set;
        }

        [Inject]
        public ObservableCollection<KeyValuePair<string, int>> ColorSource
        {
            get;
            set;
        }

        [Inject]
        public ObservableCollection<KeyValuePair<string, int>> TypeSource
        {
            get;
            set;
        }

        [Inject]
        public ObservableCollection<KeyValuePair<int, int>> CostSource
        {
            get;
            set;
        }
        
        public StatsPanel()
        {
            InitializeComponent();

            //ColorSource = new ObservableCollection<KeyValuePair<string, int>>();
            //TypeSource = new ObservableCollection<KeyValuePair<string, int>>();
            //CostSource = new ObservableCollection<KeyValuePair<int, int>>();

            TypesPie.DataContext = this;
            ColorPie.DataContext = this;
            CostBar.DataContext = this;
        }

        public void removeCard(string name) {
            Card card = DBMan.GetCard(name);

            //update colors
            if (card.Colors.Count == 0)
            {
                for (int i = 0; i < ColorSource.Count; i++)
                {
                    if (ColorSource[i].Key.Equals("Colorless"))
                    {
                        KeyValuePair<string, int> newKV = new KeyValuePair<string, int>(ColorSource[i].Key, ColorSource[i].Value - 1);
                        ColorSource.RemoveAt(i);
                        ColorSource.Insert(i, newKV);
                    }
                }
            }
            else
            {
                foreach (string color in card.Colors)
                {
                    for (int i = 0; i < ColorSource.Count; i++)
                    {
                        if (ColorSource[i].Key.Equals(color))
                        {
                            KeyValuePair<string, int> newKV = new KeyValuePair<string, int>(ColorSource[i].Key, ColorSource[i].Value - 1);
                            ColorSource.RemoveAt(i);
                            ColorSource.Insert(i, newKV);
                        }
                    }
                }
            }

            //update types
            foreach (string type in card.Types)
            {
                for (int i = 0; i < TypeSource.Count; i++)
                {
                    if (TypeSource[i].Key.Equals(type))
                    {
                        KeyValuePair<string, int> newKV = new KeyValuePair<string, int>(TypeSource[i].Key, TypeSource[i].Value - 1);
                        TypeSource.RemoveAt(i);
                        TypeSource.Insert(i, newKV);
                    }
                }
            }

            //update CMC
            for (int i = 0; i < CostSource.Count; i++)
            {
                if (CostSource[i].Key == Convert.ToInt32(card.CMC))
                {
                    KeyValuePair<int, int> newKV = new KeyValuePair<int, int>(CostSource[i].Key, CostSource[i].Value - 1);
                    CostSource.RemoveAt(i);
                    CostSource.Insert(i, newKV);
                }
            }
        }

        public void addCard(string name)
        {
            Card card = DBMan.GetCard(name);

            //update colors
            if (card.Colors.Count == 0)
            {
                for (int i = 0; i < ColorSource.Count; i++)
                {
                    if (ColorSource[i].Key.Equals("Colorless"))
                    {
                        KeyValuePair<string, int> newKV = new KeyValuePair<string, int>(ColorSource[i].Key, ColorSource[i].Value + 1);
                        ColorSource.RemoveAt(i);
                        ColorSource.Insert(i, newKV);
                    }
                }
            }
            else
            {
                foreach (String color in card.Colors)
                {
                    for (int i = 0; i < ColorSource.Count; i++)
                    {
                        if (ColorSource[i].Key.Equals(color))
                        {
                            KeyValuePair<string, int> newKV = new KeyValuePair<string, int>(ColorSource[i].Key, ColorSource[i].Value + 1);
                            ColorSource.RemoveAt(i);
                            ColorSource.Insert(i, newKV);
                        }
                    }
                }
            }

            //update types
            foreach (string type in card.Types)
            {
                for (int i = 0; i < TypeSource.Count; i++)
                {
                    if (TypeSource[i].Key.Equals(type))
                    {
                        KeyValuePair<string, int> newKV = new KeyValuePair<string, int>(TypeSource[i].Key, TypeSource[i].Value + 1);
                        TypeSource.RemoveAt(i);
                        TypeSource.Insert(i, newKV);
                    }
                }
            }

            //update CMC
            for (int i = 0; i < CostSource.Count; i++)
            {
                if (CostSource[i].Key == Convert.ToInt32(card.CMC))
                {
                    KeyValuePair<int, int> newKV = new KeyValuePair<int, int>(CostSource[i].Key, CostSource[i].Value + 1);
                    CostSource.RemoveAt(i);
                    CostSource.Insert(i, newKV);
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

            Dictionary<string, int> typeDict = new Dictionary<string, int>();

            foreach (LocalTuple tup in DLMan.currentDeck)
            {
                Card card = DBMan.GetACardFromMultiverseId(tup.MultiverseId);
                foreach (string type in card.Types)
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

            foreach (string type in typeDict.Keys)
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
            Dictionary<string, int> colorDict = new Dictionary<string, int>();

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
                foreach (string color in card.Colors)
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
            foreach (string color in colorDict.Keys)
            {
                    ColorSource.Add(new KeyValuePair<string, int>(color, colorDict[color]));
            }
        }
    }
}
