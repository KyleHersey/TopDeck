using System;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace TopDeck
{
    /// <summary>
    /// Interaction logic for HandPanel.xaml
    /// </summary>
    public partial class HandPanel : UserControl
    {
        public ResizingObservableCollection HandList
        {
            get;
            set;
        }

        public HandPanel()
        {
            InitializeComponent();
            HandList = new ResizingObservableCollection(HandControl);
            HandControl.ItemsSource = HandList;
        }

        public void UpdateImage(string id)
        {
            Console.WriteLine("Hand control actual width is " + HandControl.ActualWidth);
            Console.WriteLine("Hand control width is " + HandControl.Width);
            Image image = new Image();
            BitmapImage img = new BitmapImage();
            Console.WriteLine("new bmp");
            img.BeginInit();
            Console.WriteLine("begin init");

            if (id != null)
            {
                img.UriSource = new Uri("http://gatherer.wizards.com/Handlers/Image.ashx?multiverseid=" + id + "&type=card");
            }
            else
            {
                img.UriSource = new Uri("Magic_the_gathering-card_back.jpg", UriKind.Relative);
            }

            Console.WriteLine(img.ToString());
            Console.WriteLine("got uri");
            img.EndInit();
            Console.WriteLine("end init");

            image.Source = img;
            //MovableCard panel = new MovableCard();
           // panel.Children.Add(image);
            //Console.WriteLine("width is " + img.);
            Console.WriteLine("pixelwidth is " + img.PixelWidth);
            HandList.AddImage(image);
            Console.WriteLine("set image");
        }
    }
}
