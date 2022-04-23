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
    //Menu
    public class Menu : Proj_Application
    {
        public bool IsShowed { get; protected set; }
        public bool IsDragging { get; protected set; }
        public ControlUnit[] controlUnits;
        public double pendingPercent;
        public BotCenterMenu botCenterMenu;

        public Menu()
        {
            Rect = new Rect(0, 0, MainWindow.Drawing_Width, MainWindow.Drawing_Height);

            //App
            controlUnits = new ControlUnit[] { new Menu_Calculator(this), new Menu_FileExplorer(this) };
            botCenterMenu = new BotCenterMenu();
        }

        public override void Print()
        {
            if(pendingPercent >= 0)
            {
                if (!IsShowed)
                {
                    MainWindow.RenderManager.DrawingContext.DrawRectangle(Brushes.Gray, null, new Rect(0, MainWindow.Drawing_Height * pendingPercent, MainWindow.Drawing_Width, MainWindow.Drawing_Height - MainWindow.Drawing_Height * pendingPercent));
                }
                else
                {
                    MainWindow.RenderManager.DrawingContext.DrawRectangle(Brushes.Gray, null, new Rect(0, 0, MainWindow.Drawing_Width, MainWindow.Drawing_Height * pendingPercent));
                }
            }

            if (!((!IsShowed) || (IsDragging)))
            {
                MainWindow.RenderManager.DrawingContext.DrawRectangle(Brushes.Gray, null, Rect);

                foreach (ControlUnit unit in controlUnits)
                {
                    unit.Show(MainWindow.RenderManager.DrawingContext);
                }
            }

            botCenterMenu.Show(MainWindow.RenderManager.DrawingContext);
        }

        public override void Update(bool isFocusing, Point point, Microsoft.Kinect.HandState handState)
        {
            int clampedX = point.X < 0 ? 0 : point.X > MainWindow.Drawing_Width ? MainWindow.Drawing_Width : (int)point.X;
            int clampedY = point.Y < 0 ? 0 : point.Y > MainWindow.Drawing_Height ? MainWindow.Drawing_Height : (int)point.Y;

            botCenterMenu.IsHoveringOrDragging(clampedX, clampedY, 0, handState);

            if ((!IsShowed) || (IsDragging)) return;

            foreach (ControlUnit unit in controlUnits)
            {
                unit.IsHoveringOrDragging(clampedX, clampedY, 0, handState);
            }
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
