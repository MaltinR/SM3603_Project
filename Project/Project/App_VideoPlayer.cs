using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Threading.Tasks;

namespace Project
{
    public class App_VideoPlayer : NonDesktopApplication
    {
        public App_VideoPlayer()
        {
            PosX = 100;//For testing
            LocalEdgeControl = new LocalEdgeControl(this);
            Rect = new Rect(PosX, PosY, Width, Height);
        }

        public override void Update(bool isFocusing, Point point, MouseButtonState mouseState)
        {
            base.Update(isFocusing, point, mouseState);
        }
    }
}
