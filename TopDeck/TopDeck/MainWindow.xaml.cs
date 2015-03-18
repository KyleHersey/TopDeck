using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        
            DatabaseManager m = new DatabaseManager();
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

        }
    }
}