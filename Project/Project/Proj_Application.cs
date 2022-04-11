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
        //Run per frame
        public abstract void Update(bool isFocusing, Point point, MouseButtonState mouseState);

        public virtual void Print()
        {
        }
    }
}
