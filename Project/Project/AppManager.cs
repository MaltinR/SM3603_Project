using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Project
{
    public class AppManager
    {
        public MiddleRightSlideBar MiddleRightSlideBar { get; private set; }
        public List<NonDesktopApplication> RunningApps { get; private set; }
        public List<NonDesktopApplication> List_ToBeRemoved { get; private set; }
        public List<MiddleRightElement> RunningAppIcons { get; private set; }
        public Proj_Application OnFocusApp { get; private set; }
        public App_Desktop Desktop { get; private set; }

        public AppManager()
        {
            RunningApps = new List<NonDesktopApplication>();
            RunningAppIcons = new List<MiddleRightElement>();
            List_ToBeRemoved = new List<NonDesktopApplication>();
            MiddleRightSlideBar = new MiddleRightSlideBar();

            //OnFocusApp = new App_Desktop();

        }

        public void AddApp(NonDesktopApplication app)
        {
            RunningApps.Add(app);
            RunningAppIcons.Add(new MiddleRightElement(app.Image_Normal, app.Image_Selecting));

            foreach(MiddleRightElement element in RunningAppIcons)
            {
                element.UpdateRect();
            }
        }

        public void RemoveApp(NonDesktopApplication app)
        {
            List_ToBeRemoved.Add(app);
        }

        public void MoveOrder(NonDesktopApplication app, int index_To)
        {
            //TODO: 
        }

        public void Update(int clampedX, int clampedY)
        {
            Trace.WriteLine("RunningAppIcons.Count: " + RunningAppIcons.Count);

            foreach (MiddleRightElement element in RunningAppIcons)
            {
                element.IsHovering(clampedX, clampedY, Mouse.LeftButton);
                element.Show(MainWindow.DrawingContext);
            }
            MiddleRightSlideBar.IsHovering(clampedX, clampedY, Mouse.LeftButton);
        }

        public void LateProcess()
        {
            Late_RemoveApp();
        }

        void Late_RemoveApp()
        {
            for (int i = List_ToBeRemoved.Count - 1; i >= 0; i--)
            {
                int index = RunningApps.IndexOf(List_ToBeRemoved[i]);
                RunningApps.RemoveAt(index);
                RunningAppIcons.RemoveAt(index);
                List_ToBeRemoved.RemoveAt(i);
            }

            foreach (MiddleRightElement element in RunningAppIcons)
            {
                element.UpdateRect();
            }
        }

        public void SetFocus(Proj_Application app)
        {
            OnFocusApp = app;

            //TODO: change the order of the list

        }
    }
}
