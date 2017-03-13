using System.Windows.Controls;

namespace TopDeck
{
    public partial class FilterRowTextBox : UserControl
    {

        public string FilterText
        {
            get;
            set;
        }

        public FilterRowTextBox()
        {
            InitializeComponent();
            FilterType.DataContext = this;
        }
    }
}
