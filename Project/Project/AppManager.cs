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
        public List<NonDesktopApplication> RunningApps { get; private set; }
        public List<NonDesktopApplication> List_ToBeAdded { get; private set; }
        public List<NonDesktopApplication> List_ToBeRemoved { get; private set; }
        public Proj_Application OnFocusApp { get; private set; }
        public App_Desktop Desktop { get; private set; }
        public Menu Menu { get; private set; }
        public Switcher Switcher { get; private set; }
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
            List_ToBeAdded = new List<NonDesktopApplication>();
            List_ToBeRemoved = new List<NonDesktopApplication>();
            pendingOrders = new List<OrderRequest>();
            Menu = new Menu();
            Switcher = new Switcher();

            //OnFocusApp = new App_Desktop();

        }

        public void AddApp(NonDesktopApplication app)
        {
            List_ToBeAdded.Add(app);
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

        public void Update(int clampedX, int clampedY, Point mousePos, Microsoft.Kinect.HandState handState, Microsoft.Speech.Recognition.SpeechRecognizedEventArgs lastFrameSpeech, Microsoft.Kinect.VisualGestureBuilder.Gesture lastFrameGesture)
        {
            //Trace.WriteLine("==UpdateA==");

            Menu.Update(OnFocusApp == Menu, new Point(clampedX, clampedY), handState);

            Switcher.Update(false, new Point(clampedX, clampedY), handState);

            Trace.WriteLine("OnFocusApp != Menu: " + (OnFocusApp != Menu));
            if (OnFocusApp != Menu)
            {
                //Trace.WriteLine("Update CheckPoint B");
                //Update app
                for (int i = 0;i < RunningApps.Count;i++)
                {
                    //Trace.WriteLine("Update CheckPoint C1");
                    //MousePos will be subtituded by handPos and MouseOnClicked will be subtituded by gesture
                    RunningApps[i].Update(RunningApps[i] == OnFocusApp, i, mousePos, handState, 
                        lastFrameSpeech != null && RunningApps[i] == OnFocusApp? lastFrameSpeech.Result.Text: "",
                        lastFrameGesture != null && RunningApps[i] == OnFocusApp ? lastFrameGesture.Name.ToLower() : "");
                    //Trace.WriteLine("Update CheckPoint C2");
                }
            }
            //Trace.WriteLine("==UpdateD==");
        }

        public void LateProcess()
        {
            Late_RemoveApp();
            Late_OrderChange();
            Late_AddApp();
        }

        void Late_OrderChange()
        {
            for (int i = 0; i < pendingOrders.Count; i++)
            {
                MoveOrder(pendingOrders[0].app, pendingOrders[0].targetOrder);
                pendingOrders.RemoveAt(0);
            }
        }

        void Late_AddApp()
        {
            if (List_ToBeAdded.Count > 0)
            {
                for (int i = 0; i < List_ToBeAdded.Count; i++)
                {
                    RunningApps.Insert(0, List_ToBeAdded[i]);
                    Switcher.RunningAppIcons.Add(new MiddleRightElement(List_ToBeAdded[i].Image_Normal, List_ToBeAdded[i].Image_Selecting, List_ToBeAdded[i]));
                    MainWindow.RenderManager.RenderList.Insert(1, new RenderManager.RenderClass(List_ToBeAdded[i]));
                    //Menu is always the first

                    foreach (MiddleRightElement element in Switcher.RunningAppIcons)
                    {
                        element.UpdateRect();
                    }
                }
                OnFocusApp = RunningApps[0];
                List_ToBeAdded.Clear();
            }
        }

        void Late_RemoveApp()
        {
            if (List_ToBeRemoved.Count > 0)
            {
                for (int i = List_ToBeRemoved.Count - 1; i >= 0; i--)
                {
                    int index = RunningApps.IndexOf(List_ToBeRemoved[i]);
                    RunningApps.RemoveAt(index);
                    Switcher.RunningAppIcons.Remove(Switcher.RunningAppIcons.Find(x => x.CorrespondingApp == List_ToBeRemoved[i]));
                    List_ToBeRemoved.RemoveAt(i);

                    MainWindow.hovering = null;//To remove topright close from hovering
                }

                if (RunningApps.Count > 0)
                    OnFocusApp = RunningApps[0];
                else
                    OnFocusApp = null;
                if (RunningApps.Count > 5)
                {
                    Switcher.CalculateStartIndex();
                    for (int i = Switcher.Start; i < Switcher.Start + 5; i++)
                    {
                        Switcher.RunningAppIcons[i].UpdateRect();
                    }
                }
                else
                { 
                    foreach (MiddleRightElement element in Switcher.RunningAppIcons)
                    {
                        element.UpdateRect();
                    }
                }
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
