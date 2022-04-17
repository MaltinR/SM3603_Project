using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Input;

namespace Project
{
    public class App_TextEditor : NonDesktopApplication
    {
        public string FilePath { get; private set; }
        public App_TextEditor(string path)
        {
            Image_Normal = new BitmapImage(new Uri("Images/Icon_TextEditor_Normal.png", UriKind.Relative));
            Image_Selecting = new BitmapImage(new Uri("Images/Icon_TextEditor_Selecting.png", UriKind.Relative));

            FilePath = path;
            LocalEdgeControl = new LocalEdgeControl(this);
            Rect = new Rect(PosX, PosY, Width, Height);
        }

        public override void Update(bool isFocusing, int listOrder, Point point, MouseButtonState mouseState)
        {
            base.Update(isFocusing, listOrder, point, mouseState);

            if (!isFocusing) return;

            //To be implemented
        }
    }
}
