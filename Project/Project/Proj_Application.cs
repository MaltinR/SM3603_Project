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
        public ControlUnit[] controlUnits;

        public Menu()
        {
            Rect = new Rect(0, 0, MainWindow.Drawing_Width, MainWindow.Drawing_Height);

            //App
            controlUnits = new ControlUnit[] { new Menu_Calculator(this), new Menu_FileExplorer(this) };
        }

        public override void Print()
        {
            if ((!IsShowed) || (IsDragging)) return;
            MainWindow.DrawingContext.DrawRectangle(Brushes.Gray, null, Rect);
        }

        public override void Update(bool isFocusing, Point point, MouseButtonState mouseState)
        {
            if ((!IsShowed) || (IsDragging)) return;

            int clampedX = point.X < 0 ? 0 : point.X > MainWindow.Drawing_Width ? MainWindow.Drawing_Width : (int)point.X;
            int clampedY = point.Y < 0 ? 0 : point.Y > MainWindow.Drawing_Height ? MainWindow.Drawing_Height : (int)point.Y;

            foreach (ControlUnit unit in controlUnits)
            {
                unit.IsHovering(clampedX, clampedY, mouseState);
                unit.Show(MainWindow.DrawingContext);
            }
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
