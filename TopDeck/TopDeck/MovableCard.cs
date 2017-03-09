using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace TopDeck
{
    class MovableCard : Canvas
    {
        protected override void OnDrop(DragEventArgs e)
        {
            Console.WriteLine("onDrop event");
        }

        protected override void OnDragEnter(DragEventArgs e)
        {
            Console.WriteLine("onDragEnter event");
        }

        protected override void OnDragLeave(DragEventArgs e)
        {
            Console.WriteLine("onDragLeave event");
        }
    }
}
