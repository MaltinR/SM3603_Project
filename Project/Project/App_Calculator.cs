using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Threading.Tasks;

namespace Project
{
    public class App_Calculator : NonDesktopApplication
    {
        public App_Calculator()
        {
            Image_Normal = new BitmapImage(new Uri("Images/Icon_Calculator_Normal.png", UriKind.Relative));
            Image_Selecting = new BitmapImage(new Uri("Images/Icon_Calculator_Selecting.png", UriKind.Relative));

            PosX = 100;//For testing
            LocalEdgeControl = new LocalEdgeControl(this);
            Rect = new Rect(PosX, PosY, Width, Height);
        }

        public override void Print()
        {
            base.Print();
        }

        public override void Update(bool isFocusing, int listOrder, Point point, MouseButtonState mouseState)
        {
            base.Update(isFocusing, listOrder, point, mouseState);

            if (!isFocusing) return;
            //To be implemented
        }
    }
}
