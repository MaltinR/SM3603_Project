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

        //For visual
        public virtual void Print()
        {

        }
    }

}
