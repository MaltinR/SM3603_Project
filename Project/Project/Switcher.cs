using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Project
{
    public class Switcher : Proj_Application
    {
        public int FocusIndex { get; private set; }
        public int Start { get; private set; }
        public double percent;
        public double height;
        public BitmapImage Image_Hightlight { get; private set; }
        public MiddleRightSlideBar MiddleRightSlideBar { get; private set; }
        public List<MiddleRightElement> RunningAppIcons { get; private set; }

        public Switcher()
        {
            MiddleRightSlideBar = new MiddleRightSlideBar();
            RunningAppIcons = new List<MiddleRightElement>();

            Image_Hightlight = new BitmapImage(new Uri("Images/AreaSelection_Menu_Normal.png", UriKind.Relative));
        }
        public override void Update(bool isFocusing, Point point, MouseButtonState mouseState)
        {
            int clampedX = point.X < 0 ? 0 : point.X > MainWindow.Drawing_Width ? MainWindow.Drawing_Width : (int)point.X;
            int clampedY = point.Y < 0 ? 0 : point.Y > MainWindow.Drawing_Height ? MainWindow.Drawing_Height : (int)point.Y;

            if (RunningAppIcons.Count > 5)
            {
                CalculateStartIndex();
                for (int i = Start; i < Start + 5; i++)
                {
                    //RunningAppIcons[i].UpdateRect(new Rect(RunningAppIcons[i].PosX, MiddleRightSlideBar.PosY + ((i - Start) * 50), 50, 50));
                    RunningAppIcons[i].UpdateRect();
                    RunningAppIcons[i].IsHoveringOrDragging(clampedX, clampedY, 0, Mouse.LeftButton);
                }
            }
            else
            {
                foreach (MiddleRightElement element in RunningAppIcons)
                {
                    element.IsHoveringOrDragging(clampedX, clampedY, 0, Mouse.LeftButton);
                }
            }
            if (MainWindow.dragging == MiddleRightSlideBar)
                MiddleRightSlideBar.IsHoveringOrDragging(clampedX, clampedY, 0, Mouse.LeftButton);
        }

        public override void Print()
        {
            //1: Highlight
            //2: Icons
            //3: ScrollBar

            //Max is 5
            //Calculate what is first, then we can get the answer

            if (RunningAppIcons.Count > 5)
            {

                FocusIndex = RunningAppIcons.IndexOf(RunningAppIcons.Find(x => x.CorrespondingApp == MainWindow.Manager.OnFocusApp));
                CalculateStartIndex();

                for (int i = Start; i < Start + 5; i++)
                {
                    Rect rect = new Rect(RunningAppIcons[i].PosX, MiddleRightSlideBar.PosY + ((i - Start) * 50), 50, 50);

                    if (FocusIndex == i)
                        MainWindow.RenderManager.DrawingContext.DrawImage(Image_Hightlight, rect);

                    //RunningAppIcons[i].UpdateRect(rect);
                    RunningAppIcons[i].UpdateRect();
                    RunningAppIcons[i].Show(MainWindow.RenderManager.DrawingContext);
                }
            }
            else
            {
                foreach (MiddleRightElement element in RunningAppIcons)
                {
                    if (element.CorrespondingApp == MainWindow.Manager.OnFocusApp)
                        MainWindow.RenderManager.DrawingContext.DrawImage(Image_Hightlight, element.Rect);

                    element.UpdateRect();
                    element.Show(MainWindow.RenderManager.DrawingContext);
                }
            }
            MiddleRightSlideBar.Print();
        }

        public void CalculateStartIndex()
        {
            Start = (int)((RunningAppIcons.Count - 5) * percent);
        }
    }
}
