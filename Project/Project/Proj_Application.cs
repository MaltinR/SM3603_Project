using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Threading.Tasks;

namespace Project
{
    public abstract class Proj_Application
    {
        public Rect Rect { get; protected set; }
        //Run per frame

        //For interactive
        public abstract void Update(bool isFocusing, Point point, MouseButtonState mouseState);

        //For still elements
        public virtual void Print()
        {

        }
    }

    //Menu
    public class Menu : Proj_Application
    {
        public bool IsShowed { get; protected set; }
        public bool IsDragging { get; protected set; }

        public Menu()
        {
            Rect = new Rect(0, 0, MainWindow.Drawing_Width, MainWindow.Drawing_Height);
        }

        public override void Print()
        {
            if ((!IsShowed) || (IsDragging)) return;
            MainWindow.DrawingContext.DrawRectangle(Brushes.Gray, null, Rect);
        }

        public override void Update(bool isFocusing, Point point, MouseButtonState mouseState)
        {
            if ((!IsShowed) || (IsDragging)) return;

            //TODO: Icon to open
        }

        public void SetShow(bool isShow)
        {
            IsShowed = isShow;
        }

        public void SetDrag(bool isDrag)
        {
            IsDragging = isDrag;
        }
    }
}
