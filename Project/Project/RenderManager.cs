using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Project
{
    public class RenderManager
    {
        public DrawingContext DrawingContext { get; set; }

        public class RenderClass
        {
            public Proj_Application App { get; private set; }

            public RenderClass(Proj_Application app)
            {
                App = app;
            }
        }

        public List<RenderClass> RenderList { get; private set; }

        public RenderManager()
        {
            RenderList = new List<RenderClass>();
        }

        public void Render()
        {
            //0 Top-most
            /*
            foreach(RenderClass renderClass in RenderList.Reverse<RenderClass>())
            {
                renderClass.App.Print();
            }
            */
            //App
            foreach (NonDesktopApplication app in MainWindow.Manager.RunningApps.Reverse<NonDesktopApplication>())
            {
                app.Print();
            }
            MainWindow.Manager.Menu.Print();

            //TODO: Move Print() of icon to here
            MainWindow.Manager.MiddleRightSlideBar.Print();
        }

    }
}
