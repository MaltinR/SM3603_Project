using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;
using System.Windows.Input;

namespace Project
{
    //Manage the local (application window) control unit
    public class LocalEdgeControl
    {
        NonDesktopApplication parent;
        public const int showUpRange = 75;
        //TopLeftScale
        //BottomRightScale
        //TopCenterDrag
        ControlUnit[] controlUnits;

        public LocalEdgeControl(NonDesktopApplication _parent)
        {
            parent = _parent;
            controlUnits = new ControlUnit[] {
                new TopLeftScale(parent), new BotRightScale(parent),
                new TopCenterDrag(parent)
            };
        }

        public void UpdateRect()
        {
            foreach(ControlUnit unit in controlUnits)
            {
                (unit as LocalControlUnit).UpdateRect();
            }
        }

        //Return true if the control point is close to the edge
        public ControlUnit CheckShow(Point point, DrawingContext dc, MouseButtonState mouseState)//The parameter may be changed to Class to carry more information
        {
            int clampedX = point.X < 0 ? 0 : point.X > MainWindow.Drawing_Width ? MainWindow.Drawing_Width: (int)point.X;
            int clampedY = point.Y < 0 ? 0 : point.Y > MainWindow.Drawing_Height ? MainWindow.Drawing_Height : (int)point.Y;
            //Get pos and check is hovering
            bool isHovering = false;

            foreach (ControlUnit unit in controlUnits)
            {
                isHovering |= unit.IsHovering(clampedX, clampedY, mouseState);
                
            }

            if (isHovering)
            {
                foreach (ControlUnit unit in controlUnits)
                {
                    unit.Show(dc);
                }
            }

            return null;
        }
    }
}
