using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project
{
    public class HandControl
    {
        public ControlUnit dragging;
        public ControlUnit hovering;
        //Point
        //Gesture
        public HandControl()
        {
            dragging = null;
            hovering = null;
        }
    }
}
