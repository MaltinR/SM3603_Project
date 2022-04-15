using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Windows;
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
        //We won't change the order, it is depends on the open order
        public Proj_Application OnFocusApp { get; private set; }
        public App_Desktop Desktop { get; private set; }
        public Menu Menu { get; private set; }
        public List<OrderRequest> pendingOrders;

        public struct OrderRequest
        {
            public NonDesktopApplication app;
            public int targetOrder;

            public OrderRequest(NonDesktopApplication _app, int _targetOrder)
            {
                app = _app;
                targetOrder = _targetOrder;
            }
        }

        public AppManager()
        {
            RunningApps = new List<NonDesktopApplication>();
            RunningAppIcons = new List<MiddleRightElement>();
            List_ToBeRemoved = new List<NonDesktopApplication>();
            MiddleRightSlideBar = new MiddleRightSlideBar();
            pendingOrders = new List<OrderRequest>();
            Menu = new Menu();

            //OnFocusApp = new App_Desktop();

        }

        public void AddApp(NonDesktopApplication app)
        {
            RunningApps.Insert(0, app);
            RunningAppIcons.Add(new MiddleRightElement(app.Image_Normal, app.Image_Selecting, app));
            MainWindow.RenderManager.RenderList.Insert(1, new RenderManager.RenderClass(app));
            //Menu is always the first

            foreach(MiddleRightElement element in RunningAppIcons)
            {
                element.UpdateRect();
            }
        }

        public void RemoveApp(NonDesktopApplication app)
        {
            List_ToBeRemoved.Add(app);
        }

        void MoveOrder(NonDesktopApplication app, int index_To)
        {
            int tarInd = RunningApps.IndexOf(app);

            if (tarInd > 0)
            {
                for (int i = tarInd - 1; i >= index_To; i--)
                {
                    RunningApps[i + 1] = RunningApps[i];
                }
                RunningApps[index_To] = app;
            }
        }

        public void RequestOrderChange(NonDesktopApplication app, int to_Index)
        {
            pendingOrders.Add(new OrderRequest(app, to_Index));
        }

        public void Update(int clampedX, int clampedY, Point mousePos)
        {
            Menu.Update(OnFocusApp == Menu, new Point(clampedX, clampedY), Mouse.LeftButton);

            foreach (MiddleRightElement element in RunningAppIcons)
            {
                element.IsHoveringOrDragging(clampedX, clampedY, Mouse.LeftButton);
                element.Show(MainWindow.RenderManager.DrawingContext);
            }
            if(MainWindow.dragging == MiddleRightSlideBar)
                MiddleRightSlideBar.IsHoveringOrDragging(clampedX, clampedY, Mouse.LeftButton);

            Trace.WriteLine("OnFocusApp == Menu: " + (OnFocusApp == Menu) + " OnFocusApp: " + OnFocusApp);

            if (OnFocusApp != Menu)
            {
                //Update app
                foreach (Proj_Application app in RunningApps)
                {
                    //MousePos will be subtituded by handPos and MouseOnClicked will be subtituded by gesture
                    app.Update(app == OnFocusApp, mousePos, Mouse.LeftButton);
                }
            }
        }

        public void LateProcess()
        {
            Late_RemoveApp();
            Late_OrderChange();
        }

        void Late_OrderChange()
        {
            for (int i = 0; i < pendingOrders.Count; i++)
            {
                MoveOrder(pendingOrders[0].app, pendingOrders[0].targetOrder);
                pendingOrders.RemoveAt(0);
            }
        }

        void Late_RemoveApp()
        {
            for (int i = List_ToBeRemoved.Count - 1; i >= 0; i--)
            {
                int index = RunningApps.IndexOf(List_ToBeRemoved[i]);
                RunningApps.RemoveAt(index);
                RunningAppIcons.Remove(RunningAppIcons.Find(x => x.CorrespondingApp == List_ToBeRemoved[i]));
                List_ToBeRemoved.RemoveAt(i);

                MainWindow.hovering = null;//To remove topright close from hovering
            }

            if (RunningApps.Count > 0)
                OnFocusApp = RunningApps[0];
            else
                OnFocusApp = null;

            foreach (MiddleRightElement element in RunningAppIcons)
            {
                element.UpdateRect();
            }
        }

        public void SetFocus(Proj_Application app)
        {
            OnFocusApp = app;
            if(app is NonDesktopApplication)
            {
                RequestOrderChange(app as NonDesktopApplication, 0);
            }
        }
    }
}
