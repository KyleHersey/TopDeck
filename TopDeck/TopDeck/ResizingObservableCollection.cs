using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace TopDeck
{
    public class ResizingObservableCollection : ObservableCollection<Image>
    {
        private ItemsControl itemControl;

        public ResizingObservableCollection(ItemsControl itemControl)
            : base()
        {
            this.itemControl = itemControl;
        }

        public void AddImage(Image image)
        {
            base.Add(image);
            Console.WriteLine("Panel width is " + itemControl.ActualWidth);
            int space = (int) itemControl.ActualWidth / Count;
            Console.WriteLine("space is " + space);
            // 223
        }
    }
}
