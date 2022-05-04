using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Diagnostics;
using System.Globalization;

namespace Project
{
    //Drag
    public class ControlUnit
    {
        public BitmapImage Image_Normal { get; protected set; }
        public BitmapImage Image_Selecting { get; protected set; }
        public int PosX { get; protected set; }
        public int PosY { get; protected set; }
        public int Width { get; protected set; } = 50;
        public int Height { get; protected set; } = 50;
        public Rect Rect { get; protected set; }
        protected bool isHoveringOrDragging;
        public int HoveringTime { get; protected set; } = -1;//-1 == no stay event

        //Call when hovering
        protected virtual void Hovering(int mousePosX, int mousePosY, Microsoft.Kinect.HandState handState)
        {

        }

        protected virtual void Dragging(int mousePosX, int mousePosY)
        {

        }

        protected virtual void OnRelease(int mousePosX, int mousePosY)
        {

        }

        protected virtual void OnPressed(int mousePosX, int mousePosY)
        {

        }

        protected virtual bool HoverTimesUp(int mousePosX, int mousePosY)
        {
            //True: still hovering
            return true;
        }

        public virtual bool InRange(int mousePosX, int mousePosY, int listOrder)
        {
            return mousePosX > PosX && mousePosX < PosX + Width &&
                mousePosY > PosY && mousePosY < PosY + Height;
        }

        //Check is dragging and hovering
        public virtual bool IsHoveringOrDragging(int mousePosX, int mousePosY, int listOrder, Microsoft.Kinect.HandState handState)
        {
            isHoveringOrDragging = false;
            if (MainWindow.dragging == this)
            {
                if (handState == Microsoft.Kinect.HandState.Closed)
                {
                    Dragging(mousePosX, mousePosY);
                    isHoveringOrDragging = true;
                }
                else
                {
                    OnRelease(mousePosX, mousePosY);
                    MainWindow.dragging = null;
                }
            }

            //Please make sure it doesnt have overlap problem
            //Otherwise MainWindow.hovering will have a incorrect value
            if ((MainWindow.hovering == null || MainWindow.hovering == this) && MainWindow.dragging == null && InRange(mousePosX, mousePosY, listOrder))
            {
                isHoveringOrDragging = true;
                //TODO each point should have an individual timer and class
                if (HoveringTime > 0 && MainWindow.hovering == this && ++MainWindow.timerOfHovering >= HoveringTime)
                {
                    MainWindow.timerOfHovering = 0;
                    isHoveringOrDragging &= HoverTimesUp(mousePosX, mousePosY);
                    Trace.WriteLine("isHoveringOrDragging: " + isHoveringOrDragging);
                }

                Hovering(mousePosX, mousePosY, handState);

                if (isHoveringOrDragging)
                {
                    if (MainWindow.dragging == null && handState == Microsoft.Kinect.HandState.Closed)
                    {
                        MainWindow.dragging = this;
                        MainWindow.dragging.OnPressed(mousePosX, mousePosY);
                    }
                    else
                    {
                        MainWindow.hovering = this;
                    }
                }
                else
                {
                    MainWindow.dragging = null;
                    MainWindow.hovering = null;
                }
                return isHoveringOrDragging;
            }
            else
            {
                //Release it
                if (MainWindow.hovering == this)
                {
                    MainWindow.timerOfHovering = 0;
                    MainWindow.hovering = null;
                    isHoveringOrDragging = false;
                }
            }
            return isHoveringOrDragging;
        }

        public virtual void Show(DrawingContext dc)
        {
            dc.DrawImage(isHoveringOrDragging ? Image_Selecting : Image_Normal, Rect);
        }

        public virtual void UpdateRect()
        {
            Rect = new Rect(PosX, PosY, Width, Height);
        }

        public virtual void Print()
        {
            Show(MainWindow.RenderManager.DrawingContext);
        }
    }

    public class LocalControlUnit : ControlUnit
    {
        public bool IsShowThisFrame { get; protected set; } //To avoid render problem
        public NonDesktopApplication Parent { get; protected set; }

        public override bool InRange(int mousePosX, int mousePosY, int listOrder)
        {
            bool isInRange = mousePosX > PosX + Parent.PosX && mousePosX < PosX + Parent.PosX + Width &&
                mousePosY > PosY + Parent.PosY && mousePosY < PosY + Parent.PosY + Height;

            if (MainWindow.Manager.RunningApps.Count > 0)
            {
                for (int i = 0; i < listOrder; i++)
                {
                    isInRange &= (mousePosX < MainWindow.Manager.RunningApps[i].PosX || mousePosX > MainWindow.Manager.RunningApps[i].PosX + MainWindow.Manager.RunningApps[i].Width) || (mousePosY < MainWindow.Manager.RunningApps[i].PosY || mousePosY > MainWindow.Manager.RunningApps[i].PosY + MainWindow.Manager.RunningApps[i].Height);
                }
            }

            return isInRange;
        }

        public override void UpdateRect()
        {
            Rect = new Rect(PosX + Parent.PosX, PosY + Parent.PosY, Width, Height);
        }
    }

    public class GlobalControlUnit : ControlUnit
    {

    }

    public class TopLeftScale : LocalControlUnit
    {
        public TopLeftScale(NonDesktopApplication parent)
        {
            Parent = parent;
            Image_Normal = new BitmapImage(new Uri("Images/AreaSelection_TopLeft_Normal.png", UriKind.Relative));
            Image_Selecting = new BitmapImage(new Uri("Images/AreaSelection_TopLeft_Selecting.png", UriKind.Relative));

            Width = 50;
            Height = 50;
            PosX = 0;
            PosY = 0;
            UpdateRect();
        }

        protected override void Dragging(int mousePosX, int mousePosY)
        {
            if (mousePosX < Width / 2 ||
                mousePosX > MainWindow.Drawing_Width - Width / 2 ||
                mousePosY < Height / 2 ||
                mousePosY > MainWindow.Drawing_Height - Height / 2)
                return;

            //Get the lastFramePos first
            int distX = mousePosX - (PosX + Parent.PosX + Width / 2);
            int distY = mousePosY - (PosY + Parent.PosY + Height / 2);

            Parent.SetWidth(Parent.Width - distX);
            Parent.SetPosX(Parent.PosX + distX);
            Parent.SetHeight(Parent.Height - distY);
            Parent.SetPosY(Parent.PosY + distY);
        }

        protected override void OnPressed(int mousePosX, int mousePosY)
        {
            MainWindow.Manager.SetFocus(Parent);
        }
    }

    public class BotRightScale : LocalControlUnit
    {
        public BotRightScale(NonDesktopApplication parent)
        {
            Parent = parent;
            Image_Normal = new BitmapImage(new Uri("Images/AreaSelection_BotRight_Normal.png", UriKind.Relative));
            Image_Selecting = new BitmapImage(new Uri("Images/AreaSelection_BotRight_Selecting.png", UriKind.Relative));

            Width = 50;
            Height = 50;
            PosX = Parent.Width - Width;
            PosY = Parent.Height - Height;
            UpdateRect();
        }

        public override void UpdateRect()
        {
            PosX = Parent.Width - Width;
            PosY = Parent.Height - Height;
            base.UpdateRect();
        }
        protected override void Dragging(int mousePosX, int mousePosY)
        {
            if (mousePosX < Width / 2 ||
                mousePosX > MainWindow.Drawing_Width - Width / 2 ||
                mousePosY < Height / 2 ||
                mousePosY > MainWindow.Drawing_Height - Height / 2)
                return;

            //Get the lastFramePos first
            int distX = mousePosX - (PosX + Parent.PosX + Width / 2);
            int distY = mousePosY - (PosY + Parent.PosY + Height / 2);

            Parent.SetWidth(Parent.Width + distX);
            Parent.SetHeight(Parent.Height + distY);
        }
        protected override void OnPressed(int mousePosX, int mousePosY)
        {
            MainWindow.Manager.SetFocus(Parent);
        }
    }

    public class TopCenterDrag : LocalControlUnit
    {
        public TopCenterDrag(NonDesktopApplication parent)
        {
            Parent = parent;

            Image_Normal = new BitmapImage(new Uri("Images/AreaSelection_Drag_Normal.png", UriKind.Relative));
            Image_Selecting = new BitmapImage(new Uri("Images/AreaSelection_Drag_Selecting.png", UriKind.Relative));

            Width = 50;
            Height = 50;
            PosX = (Parent.Width - Width) / 2;
            PosY = 0;
            Rect = new Rect(PosX + Parent.PosX, PosY + Parent.PosY, Width, Height);
        }
        protected override void Dragging(int mousePosX, int mousePosY)
        {
            //Get the lastFramePos first
            int distX = mousePosX - (PosX + Parent.PosX + Width / 2);
            int distY = mousePosY - (PosY + Parent.PosY + Height / 2);

            Parent.SetPosX(Parent.PosX + distX);
            Parent.SetPosY(Parent.PosY + distY);
        }
        protected override void OnPressed(int mousePosX, int mousePosY)
        {
            MainWindow.Manager.SetFocus(Parent);
        }

        public override void UpdateRect()
        {
            PosX = (Parent.Width - Width) / 2;
            base.UpdateRect();
        }
    }

    public class TopRightClose : LocalControlUnit
    {
        public TopRightClose(NonDesktopApplication parent)
        {
            Parent = parent;
            Image_Normal = new BitmapImage(new Uri("Images/AreaSelection_Close_Normal.png", UriKind.Relative));
            Image_Selecting = new BitmapImage(new Uri("Images/AreaSelection_Close_Selecting.png", UriKind.Relative));
            HoveringTime = 30;

            Width = 50;
            Height = 50;
            PosX = Parent.Width - Width;
            PosY = 0;
            UpdateRect();
        }

        public override void UpdateRect()
        {
            PosX = Parent.Width - Width;
            PosY = 0;
            base.UpdateRect();
        }

        protected override bool HoverTimesUp(int mousePosX, int mousePosY)
        {
            Parent.OnClose();
            MainWindow.Manager.RemoveApp(Parent);
            return false;
        }
    }

    public class BotCenterMenu : GlobalControlUnit
    {
        public BotCenterMenu()
        {
            Image_Normal = new BitmapImage(new Uri("Images/AreaSelection_Menu_Normal.png", UriKind.Relative));
            Image_Selecting = new BitmapImage(new Uri("Images/AreaSelection_Menu_Selecting.png", UriKind.Relative));

            Width = 50;
            Height = 50;
            PosX = (MainWindow.Drawing_Width - Width) / 2;
            PosY = MainWindow.Drawing_Height - Height;

            Rect = new Rect(PosX, PosY, Width, Height);
        }

        protected override void Dragging(int mousePosX, int mousePosY)
        {
            if (mousePosX < Width / 2 ||
                mousePosX > MainWindow.Drawing_Width - Width / 2 ||
                mousePosY < Height / 2 ||
                mousePosY > MainWindow.Drawing_Height - Height / 2)
                return;

            MainWindow.Manager.Menu.SetDrag(true);

            MainWindow.Manager.Menu.pendingPercent = mousePosY / (double)MainWindow.Drawing_Height;

            Rect = new Rect(PosX, mousePosY - Height / 2, Width, Height);
        }

        protected override void OnRelease(int mousePosX, int mousePosY)
        {
            MainWindow.Manager.Menu.SetDrag(false);

            //Check if is half
            if (mousePosY < MainWindow.Drawing_Height / 2)
            {
                //Show menu
                MainWindow.Manager.Menu.SetShow(!MainWindow.Manager.Menu.IsShowed);

                if (MainWindow.Manager.Menu.IsShowed)
                {
                    MainWindow.Manager.SetFocus(MainWindow.Manager.Menu);
                }
                else
                {
                    if (MainWindow.Manager.RunningApps.Count > 0)
                    {
                        MainWindow.Manager.SetFocus(MainWindow.Manager.RunningApps[0]);
                    }
                    else
                    {
                        //Desktop
                    }
                }
            }

            Rect = new Rect(PosX, PosY, Width, Height);
        }

        public override bool IsHoveringOrDragging(int mousePosX, int mousePosY, int listOrder, Microsoft.Kinect.HandState handState)
        {
            MainWindow.Manager.Menu.pendingPercent = -1;
            return base.IsHoveringOrDragging(mousePosX, mousePosY, listOrder, handState);
        }

        //TODO: needs UpdateRect as well when changing the window
    }

    public class MiddleRightElement : GlobalControlUnit
    {
        public NonDesktopApplication CorrespondingApp { get; private set; }

        //Changable in the image

        public MiddleRightElement(BitmapImage image_Normal, BitmapImage image_Selecting, NonDesktopApplication app)
        {
            HoveringTime = 60;

            Image_Normal = image_Normal;
            Image_Selecting = image_Selecting;

            CorrespondingApp = app;
        }

        public MiddleRightElement(string uri_Noraml, string uri_Selecting, NonDesktopApplication app)
        {
            HoveringTime = 60;

            Image_Normal = new BitmapImage(new Uri(uri_Noraml, UriKind.Relative));
            Image_Selecting = new BitmapImage(new Uri(uri_Selecting, UriKind.Relative));

            CorrespondingApp = app;
        }

        protected override bool HoverTimesUp(int mousePosX, int mousePosY)
        {
            MainWindow.Manager.SetFocus(CorrespondingApp);
            return false;
        }

        public override void UpdateRect()
        {
            Width = 50;
            Height = 50;
            PosX = MainWindow.Drawing_Width - Width;

            if (MainWindow.Manager.Switcher.RunningAppIcons.Count <= 5)
            {
                PosY = MainWindow.Drawing_Height / 2 - Height * MainWindow.Manager.Switcher.RunningAppIcons.Count / 2 + MainWindow.Manager.Switcher.RunningAppIcons.IndexOf(this) * Height;
            }
            else
            {
                MainWindow.Manager.Switcher.RunningAppIcons.IndexOf(MainWindow.Manager.Switcher.RunningAppIcons.Find(x => x.CorrespondingApp == CorrespondingApp));

                PosY = MainWindow.Manager.Switcher.MiddleRightSlideBar.PosY + ((MainWindow.Manager.Switcher.RunningAppIcons.IndexOf(this) - MainWindow.Manager.Switcher.Start) * 50);
            }

            Rect = new Rect(PosX, PosY, Width, Height);
        }

        protected override void Dragging(int mousePosX, int mousePosY)
        {
            //Change the dragging target to slide bar
            MainWindow.dragging = MainWindow.Manager.Switcher.MiddleRightSlideBar;
        }
    }

    public class MiddleRightSlideBar : GlobalControlUnit
    {
        //Wake when drag on the MiddleLeftElement
        public MiddleRightSlideBar()
        {
            Width = 50;
            Height = 250;
            PosX = MainWindow.Drawing_Width - 50;
            PosY = MainWindow.Drawing_Height / 2 - Height / 2;

            UpdateRect();
        }

        public override bool IsHoveringOrDragging(int mousePosX, int mousePosY, int listOrder, Microsoft.Kinect.HandState handState)
        {
            if (MainWindow.dragging == this)
            {
                if (handState == Microsoft.Kinect.HandState.Closed)
                {
                    Dragging(mousePosX, mousePosY);
                    isHoveringOrDragging = true;
                    return true;
                }
                else
                {
                    OnRelease(mousePosX, mousePosY);
                    MainWindow.dragging = null;
                    isHoveringOrDragging = false;
                    return false;
                }
            }
            //We don't check hovering, just check if it is dragging
            if (MainWindow.hovering == this)
            {
                MainWindow.hovering = null;
                if (handState == Microsoft.Kinect.HandState.Closed && InRange(mousePosX, mousePosY, listOrder))
                {
                    Dragging(mousePosX, mousePosY);
                    isHoveringOrDragging = true;
                    MainWindow.dragging = this;
                    return true;
                }
            }
            return false;
        }

        protected override void Dragging(int mousePosX, int mousePosY)
        {
            if (MainWindow.Manager.Switcher.RunningAppIcons.Count <= 5)
                return;
            else
            {
                //Percent Height = (Total - 5)/Total*Height

                MainWindow.Manager.Switcher.height = (MainWindow.Manager.Switcher.RunningAppIcons.Count - 5) / (double)MainWindow.Manager.Switcher.RunningAppIcons.Count * Height;

                MainWindow.Manager.Switcher.percent = (mousePosY - (PosY + (Height - MainWindow.Manager.Switcher.height) / 2)) / MainWindow.Manager.Switcher.height;
                MainWindow.Manager.Switcher.percent = MainWindow.Manager.Switcher.percent < 0 ? 0 : MainWindow.Manager.Switcher.percent > 1 ? 1 : MainWindow.Manager.Switcher.percent;
            }
        }

        public override void Print()
        {
            if (MainWindow.dragging == this && MainWindow.Manager.Switcher.RunningAppIcons.Count > 5)
            {
                //Cal the size

                int height = (int)(5 / (double)MainWindow.Manager.Switcher.RunningAppIcons.Count * Height);

                MainWindow.RenderManager.DrawingContext.DrawRectangle(Brushes.Gray, null,
                new Rect(MainWindow.Drawing_Width - 54, PosY + MainWindow.Manager.Switcher.percent * MainWindow.Manager.Switcher.height, 4, height));
            }
        }
    }

    public class Image_ChannelR : Image_Channel
    {
        public Image_ChannelR(App_ImageEditor imageEditor, Image_ChannelSlider slider)
        {
            Parent = imageEditor;
            Slider = slider;
            Image_Normal = new BitmapImage(new Uri("Images/ImageTool_ChannelR_Normal.png", UriKind.Relative));
            Image_Selecting = new BitmapImage(new Uri("Images/ImageTool_ChannelR_Selecting.png", UriKind.Relative));

            PosY = 50;
            Width = 30;
            Height = 30;

            Rect = new Rect(PosX, PosY, Width, Height);
        }
    }

    public class Image_ChannelG : Image_Channel
    {
        public Image_ChannelG(App_ImageEditor imageEditor, Image_ChannelSlider slider)
        {
            Parent = imageEditor;
            Slider = slider;
            Image_Normal = new BitmapImage(new Uri("Images/ImageTool_ChannelG_Normal.png", UriKind.Relative));
            Image_Selecting = new BitmapImage(new Uri("Images/ImageTool_ChannelG_Selecting.png", UriKind.Relative));

            PosY = 80;
            Width = 30;
            Height = 30;

            Rect = new Rect(PosX, PosY, Width, Height);
        }
    }

    public class Image_ChannelB : Image_Channel
    {
        public Image_ChannelB(App_ImageEditor imageEditor, Image_ChannelSlider slider)
        {
            Parent = imageEditor;
            Slider = slider;
            Image_Normal = new BitmapImage(new Uri("Images/ImageTool_ChannelB_Normal.png", UriKind.Relative));
            Image_Selecting = new BitmapImage(new Uri("Images/ImageTool_ChannelB_Selecting.png", UriKind.Relative));

            PosY = 110;
            Width = 30;
            Height = 30;

            Rect = new Rect(PosX, PosY, Width, Height);
        }
    }

    public class Image_ChannelSize : Image_Channel
    {
        public Image_ChannelSize(App_ImageEditor imageEditor, Image_ChannelSlider slider)
        {
            Parent = imageEditor;
            Slider = slider;
            Image_Normal = new BitmapImage(new Uri("Images/ImageTool_Blank_Normal.png", UriKind.Relative));
            Image_Selecting = new BitmapImage(new Uri("Images/ImageTool_Blank_Selecting.png", UriKind.Relative));

            PosY = 140;
            Width = 30;
            Height = 30;

            Rect = new Rect(PosX, PosY, Width, Height);
        }

        public override void Print()
        {
            base.Print();
            int size = (int)((Parent as App_ImageEditor).BrushSize / 100.0 * 24);
            MainWindow.RenderManager.DrawingContext.DrawRectangle(Brushes.Black, null, new Rect(Parent.PosX + PosX + 3 + (24 - size) / 2, Parent.PosY + PosY + 3 + (24 - size) / 2, size, size));
        }
    }

    public class Image_PaintBoard : LocalControlUnit
    {
        public Image_PaintBoard(App_ImageEditor imageEditor)
        {
            Parent = imageEditor;

            PosX = 40;
            PosY = 50;
            Width = Parent.Width - 90;
            Height = Parent.Height - 100;

            UpdateRect();
        }

        protected override void OnPressed(int mousePosX, int mousePosY)
        {
            (Parent as App_ImageEditor).Point_ClickFrame = new Point(mousePosX - (PosX + Parent.PosX), mousePosY - (PosY + Parent.PosY));
            (Parent as App_ImageEditor).Point_PreviousFrame = (Parent as App_ImageEditor).Point_ClickFrame;
            (Parent as App_ImageEditor).Point_CurrentFrame = (Parent as App_ImageEditor).Point_ClickFrame;
            (Parent as App_ImageEditor).IsPaint = true;
        }

        protected override void Dragging(int mousePosX, int mousePosY)
        {
            (Parent as App_ImageEditor).Point_PreviousFrame = (Parent as App_ImageEditor).Point_CurrentFrame;
            (Parent as App_ImageEditor).Point_CurrentFrame = new Point(mousePosX - (PosX + Parent.PosX), mousePosY - (PosY + Parent.PosY));
        }

        protected override void OnRelease(int mousePosX, int mousePosY)
        {
            (Parent as App_ImageEditor).PaintEnd();
        }

        public override void UpdateRect()
        {
            Width = Parent.Width - 90;
            Height = Parent.Height - 100;
            base.UpdateRect();
        }
    }
    public class Image_ChannelSliderR : Image_ChannelSlider
    {
        public Image_ChannelSliderR(App_ImageEditor imageEditor)
        {
            Parent = imageEditor;
            Image_Normal = new BitmapImage(new Uri("Images/ImageTool_SliderBackground.png", UriKind.Relative));

            PosY = 50;
            Width = 150;
            Height = 30;

            Rect = new Rect(PosX, PosY, Width, Height);
        }

        protected override void Dragging(int mousePosX, int mousePosY)
        {
            base.Dragging(mousePosX, mousePosY);
            (Parent as App_ImageEditor).Channel_R = (int)(255 * _percent);
        }
    }
    public class Image_ChannelSliderG : Image_ChannelSlider
    {
        public Image_ChannelSliderG(App_ImageEditor imageEditor)
        {
            Parent = imageEditor;
            Image_Normal = new BitmapImage(new Uri("Images/ImageTool_SliderBackground.png", UriKind.Relative));

            PosY = 80;
            Width = 150;
            Height = 30;

            Rect = new Rect(PosX, PosY, Width, Height);
        }

        protected override void Dragging(int mousePosX, int mousePosY)
        {
            base.Dragging(mousePosX, mousePosY);
            (Parent as App_ImageEditor).Channel_G = (int)(255 * _percent);
        }
    }

    public class Image_ChannelSliderB : Image_ChannelSlider
    {
        public Image_ChannelSliderB(App_ImageEditor imageEditor)
        {
            Parent = imageEditor;
            Image_Normal = new BitmapImage(new Uri("Images/ImageTool_SliderBackground.png", UriKind.Relative));

            PosY = 110;
            Width = 150;
            Height = 30;

            Rect = new Rect(PosX, PosY, Width, Height);
        }

        protected override void Dragging(int mousePosX, int mousePosY)
        {
            base.Dragging(mousePosX, mousePosY);
            (Parent as App_ImageEditor).Channel_B = (int)(255 * _percent);
        }
    }
    public class Image_ChannelSliderSize : Image_ChannelSlider
    {
        public Image_ChannelSliderSize(App_ImageEditor imageEditor)
        {
            Parent = imageEditor;
            Image_Normal = new BitmapImage(new Uri("Images/ImageTool_SliderBackground.png", UriKind.Relative));

            PosY = 140;
            Width = 150;
            Height = 30;

            Rect = new Rect(PosX, PosY, Width, Height);
        }

        protected override void Dragging(int mousePosX, int mousePosY)
        {
            base.Dragging(mousePosX, mousePosY);
            (Parent as App_ImageEditor).BrushSize = (int)(98 * _percent) + 2;
        }
    }

    public class Image_Tool : LocalControlUnit
    {
        App_ImageEditor.Tool tool_Representing;
        public Image_Tool(App_ImageEditor app, App_ImageEditor.Tool tool)
        {
            tool_Representing = tool;
            Parent = app;
            HoveringTime = 30;

            switch (tool)
            {
                case App_ImageEditor.Tool.Line:
                    Image_Normal = new BitmapImage(new Uri("Images/ImageTool_Line_Normal.png", UriKind.Relative));
                    Image_Selecting = new BitmapImage(new Uri("Images/ImageTool_Line_Selecting.png", UriKind.Relative));
                    break;
                case App_ImageEditor.Tool.Pen:
                    Image_Normal = new BitmapImage(new Uri("Images/ImageTool_Pen_Normal.png", UriKind.Relative));
                    Image_Selecting = new BitmapImage(new Uri("Images/ImageTool_Pen_Selecting.png", UriKind.Relative));
                    break;
                case App_ImageEditor.Tool.Picker:
                    Image_Normal = new BitmapImage(new Uri("Images/ImageTool_Picker_Normal.png", UriKind.Relative));
                    Image_Selecting = new BitmapImage(new Uri("Images/ImageTool_Picker_Selecting.png", UriKind.Relative));
                    break;
            }

            PosX = ((int)tool + 1) * 30;
            PosY = Parent.Height - 30;
            Width = 30;
            Height = 30;

            UpdateRect();
        }

        protected override bool HoverTimesUp(int mousePosX, int mousePosY)
        {
            (Parent as App_ImageEditor).UsingTool = tool_Representing;
            return false;
        }

        public override void UpdateRect()
        {
            PosX = ((int)tool_Representing + 1) * 30;
            PosY = Parent.Height - 30;

            base.UpdateRect();
        }

        public override void Show(DrawingContext dc)
        {
            dc.DrawImage(isHoveringOrDragging || (Parent as App_ImageEditor).UsingTool == tool_Representing ? Image_Selecting : Image_Normal, Rect);
        }
    }
    //RGB & Size
    public class Image_Channel : LocalControlUnit
    {
        public Image_ChannelSlider Slider { get; protected set; }
        protected override void Dragging(int mousePosX, int mousePosY)
        {
            MainWindow.dragging = Slider;
        }
    }

    public class Image_ChannelSlider : LocalControlUnit_SlideBar
    {
        protected double _percent;
        protected override void Dragging(int mousePosX, int mousePosY)
        {
            _percent = (mousePosX - (Parent.PosX + 5)) / (double)(Width - 10);
            _percent = _percent < 0 ? 0 : _percent > 1 ? 1 : _percent;
        }
        public override void Print()
        {
            //MainWindow.RenderManager.DrawingContext.DrawRectangle(Brushes.DarkGray, null, Rect);
            MainWindow.RenderManager.DrawingContext.DrawImage(Image_Normal, Rect);
            //Bar
            MainWindow.RenderManager.DrawingContext.DrawRectangle(Brushes.Black, null, new Rect(Parent.PosX + PosX + 5, Parent.PosY + PosY + (Height / 2) - 2, Width - 10, 4));
            //Button
            MainWindow.RenderManager.DrawingContext.DrawRectangle(Brushes.Gray, null, new Rect(Parent.PosX + PosX + 5 + (_percent * (Width - 10 - 4)), Parent.PosY + PosY + (Height / 2) - 6, 4, 12));
        }
    }

    public class File_Subject : LocalControlUnit
    {
        public int IndexFromView { get; private set; }

        public File_Subject(App_FileExplorer parent, int index, int startPos, int height, bool enableHover)
        {
            Parent = parent;
            IndexFromView = index;

            HoveringTime = enableHover ? 45 : -1;

            PosX = 50;
            PosY = startPos;
            Width = Parent.Width - 100;
            Height = height;

            UpdateRect();
        }

        protected override void Hovering(int mousePosX, int mousePosY, Microsoft.Kinect.HandState handState)
        {
            (Parent as App_FileExplorer).highlighting = IndexFromView;

            if (handState == Microsoft.Kinect.HandState.Lasso)
            {
                (Parent as App_FileExplorer).ToggleSelect(IndexFromView);
            }
        }
        protected override void Dragging(int mousePosX, int mousePosY)
        {
            //Change the dragging target to slide bar
            MainWindow.dragging = (Parent as App_FileExplorer).SlideBar;
        }

        protected override bool HoverTimesUp(int mousePosX, int mousePosY)
        {
            //MainWindow.dragging = null;
            //MainWindow.hovering = null;
            //Open
            //Call parent's function
            (Parent as App_FileExplorer).Open(IndexFromView);
            return false;
        }

        public void SetHoveringTime(bool isHover)
        {
            HoveringTime = isHover ? 60 : -1;
        }
    }

    public class LocalControlUnit_SlideBar : LocalControlUnit
    {
        public override bool IsHoveringOrDragging(int mousePosX, int mousePosY, int listOrder, Microsoft.Kinect.HandState handState)
        {
            if (MainWindow.dragging == this)
            {
                if (handState == Microsoft.Kinect.HandState.Closed)
                {
                    Dragging(mousePosX, mousePosY);
                    isHoveringOrDragging = true;
                    return true;
                }
                else
                {
                    OnRelease(mousePosX, mousePosY);
                    MainWindow.dragging = null;
                    isHoveringOrDragging = false;
                    return false;
                }
            }
            //We don't check hovering, just check if it is dragging
            if (MainWindow.hovering == this)
            {
                MainWindow.hovering = null;
                if (handState == Microsoft.Kinect.HandState.Closed && InRange(mousePosX, mousePosY, listOrder))
                {
                    Dragging(mousePosX, mousePosY);
                    isHoveringOrDragging = true;
                    MainWindow.dragging = this;
                    return true;
                }
            }
            return false;
        }
    }

    public class File_SlideBar : LocalControlUnit_SlideBar
    {
        //Wake when drag on the MiddleLeftElement
        public File_SlideBar(App_FileExplorer parent)
        {
            Parent = parent;
            PosX = 0;
            PosY = 50;
            Width = Parent.Width - 40;
            Height = (Parent.Height - PosY) / (Parent as App_FileExplorer).RowSize * (Parent as App_FileExplorer).RowSize;

            UpdateRect();
        }

        public override bool IsHoveringOrDragging(int mousePosX, int mousePosY, int listOrder, Microsoft.Kinect.HandState handState)
        {
            if (MainWindow.dragging == this)
            {
                if (handState == Microsoft.Kinect.HandState.Closed)
                {
                    Dragging(mousePosX, mousePosY);
                    isHoveringOrDragging = true;
                    return true;
                }
                else
                {
                    OnRelease(mousePosX, mousePosY);
                    MainWindow.dragging = null;
                    isHoveringOrDragging = false;
                    return false;
                }
            }
            //We don't check hovering, just check if it is dragging
            if (MainWindow.hovering == this)
            {
                MainWindow.hovering = null;
                if (handState == Microsoft.Kinect.HandState.Closed && InRange(mousePosX, mousePosY, listOrder))
                {
                    Dragging(mousePosX, mousePosY);
                    isHoveringOrDragging = true;
                    MainWindow.dragging = this;
                    return true;
                }
            }
            return false;
        }

        protected override void Dragging(int mousePosX, int mousePosY)
        {
            int sumInDir = (Parent as App_FileExplorer).CurrentFiles.Length + (Parent as App_FileExplorer).CurrentFolders.Length;
            if (sumInDir <= (Parent as App_FileExplorer).MaxRowCount)
                return;
            else
            {
                //Percent Height = (Total - RowCount)/Total*Height

                (Parent as App_FileExplorer).SlideBarHeight = (int)((sumInDir - (Parent as App_FileExplorer).MaxRowCount) / (double)sumInDir * Height);

                (Parent as App_FileExplorer).SlideBarPercent = (mousePosY - (Parent.PosY + PosY + (Parent as App_FileExplorer).SlideBarHeight / 2)) / (double)(Parent as App_FileExplorer).SlideBarHeight;
                (Parent as App_FileExplorer).SlideBarPercent = (Parent as App_FileExplorer).SlideBarPercent < 0 ? 0 : (Parent as App_FileExplorer).SlideBarPercent > 1 ? 1 : (Parent as App_FileExplorer).SlideBarPercent;

                (Parent as App_FileExplorer).Scroll();
            }
        }

        public override void UpdateRect()
        {
            Width = Parent.Width - 40;
            Height = (Parent.Height - PosY) / (Parent as App_FileExplorer).RowSize * (Parent as App_FileExplorer).RowSize;
            base.UpdateRect();
        }

        public override void Print()
        {
            int sumInDir = (Parent as App_FileExplorer).CurrentFiles.Length + (Parent as App_FileExplorer).CurrentFolders.Length;
            if (MainWindow.dragging == this && (Parent as App_FileExplorer).MaxRowCount <= sumInDir)
            {
                //Cal the size

                int height = (int)((Parent as App_FileExplorer).MaxRowCount / (double)sumInDir * Height);

                MainWindow.RenderManager.DrawingContext.DrawRectangle(Brushes.Gray, null,
                new Rect(Parent.PosX, Parent.PosY + PosY + (Parent as App_FileExplorer).SlideBarPercent * (Parent as App_FileExplorer).SlideBarHeight, 4, height));
            }
        }
    }

    public class Video_Timespan : LocalControlUnit
    {
        Rect rect_TimeLine;
        DraggingDetail draggingDetail;

        public class DraggingDetail
        {
            public bool isShow;
            public Rect rect_TimeButton;

        }

        public Video_Timespan(App_VideoPlayer parent)
        {
            draggingDetail = new DraggingDetail();

            Parent = parent;
            PosX = 40;
            PosY = Parent.Height / 2;
            Width = Parent.Width - 80;
            Height = Parent.Height / 2 - 40;

            UpdateRect();
        }

        protected override void Dragging(int mousePosX, int mousePosY)
        {
            draggingDetail.isShow = true;

            //Get the percentage
            double timePos = mousePosX - (Parent.PosX + PosX);
            timePos = timePos < 0 ? 0 : timePos > Width ? Width : timePos;
            double percent = timePos / (double)Width;

            (Parent as App_VideoPlayer).player.Position = TimeSpan.FromSeconds(percent * (Parent as App_VideoPlayer).player.NaturalDuration.TimeSpan.TotalSeconds);

            draggingDetail.rect_TimeButton = new Rect((Parent.PosX + PosX) + (percent * Width - 4), (Parent.PosY + PosY) + Height / 2 + 20 - 6, 4, 12);
        }

        public override bool IsHoveringOrDragging(int mousePosX, int mousePosY, int listOrder, Microsoft.Kinect.HandState handState)
        {
            draggingDetail.isShow = false;
            return base.IsHoveringOrDragging(mousePosX, mousePosY, listOrder, handState);
        }

        public override void UpdateRect()
        {
            PosY = Parent.Height / 2;
            Width = Parent.Width - 80;
            Height = Parent.Height / 2 - 40;

            base.UpdateRect();

            rect_TimeLine = new Rect((Parent.PosX + PosX), (Parent.PosY + PosY) + Height / 2 + 20 - 2, Width, 4);
        }

        public override void Print()
        {
            if (!draggingDetail.isShow) return;

            base.Print();

            MainWindow.RenderManager.DrawingContext.DrawRectangle(Brushes.White, null, rect_TimeLine);
            MainWindow.RenderManager.DrawingContext.DrawRectangle(Brushes.Gray, null, draggingDetail.rect_TimeButton);
        }
    }

    public class Video_Volume : LocalControlUnit
    {
        Rect rect_Volume_Icon;
        Rect rect_Volume;
        BitmapImage Image_Volume;

        DraggingDetail draggingDetail;

        public class DraggingDetail
        {
            public bool isShow;
            public Rect rect_VolumeButton;

        }

        public Video_Volume(App_VideoPlayer parent)
        {
            draggingDetail = new DraggingDetail();

            Image_Volume = new BitmapImage(new Uri("Images/Volume.png", UriKind.Relative));

            Parent = parent;
            PosX = 40;
            PosY = 40;
            Width = Parent.Width - 80;
            Height = Parent.Height / 2 - 40;

            UpdateRect();
        }

        protected override void Dragging(int mousePosX, int mousePosY)
        {
            draggingDetail.isShow = true;

            //Get the percentage
            double timePos = mousePosY - (Parent.PosY + PosY);
            timePos = timePos < 0 ? 0 : timePos > Height ? Height : timePos;
            double percent = timePos / (double)Height;

            (Parent as App_VideoPlayer).player.Volume = 1.0 - percent;
            draggingDetail.rect_VolumeButton = new Rect((Parent.PosX + PosX) + Width * 0.75 - 6, (Parent.PosY + PosY) + (percent * Height - 4), 12, 4);
        }

        public override bool IsHoveringOrDragging(int mousePosX, int mousePosY, int listOrder, Microsoft.Kinect.HandState handState)
        {
            draggingDetail.isShow = false;
            return base.IsHoveringOrDragging(mousePosX, mousePosY, listOrder, handState);
        }

        public override void UpdateRect()
        {
            Width = Parent.Width - 80;
            Height = Parent.Height / 2 - 40;

            base.UpdateRect();

            rect_Volume = new Rect((Parent.PosX + PosX) + Width * 0.75 - 2, (Parent.PosY + PosY), 4, Height);
            rect_Volume_Icon = new Rect((Parent.PosX + PosX) + Width * 0.25 - 40, (Parent.PosY + PosY), 80, 80);
        }
        public override void Print()
        {
            if (!draggingDetail.isShow) return;

            base.Print();

            MainWindow.RenderManager.DrawingContext.DrawImage(Image_Volume, rect_Volume_Icon);
            MainWindow.RenderManager.DrawingContext.DrawRectangle(Brushes.White, null, rect_Volume);
            MainWindow.RenderManager.DrawingContext.DrawRectangle(Brushes.Gray, null, draggingDetail.rect_VolumeButton);
        }
    }

    public class MenuControlUnit : ControlUnit
    {
        protected double Margin;
        protected double Gap;
        protected Menu Parent;

        public MenuControlUnit()
        {

        }

        public void CalMargin()
        {
            Margin = MainWindow.Drawing_Width / 6;
            Gap = (MainWindow.Drawing_Width - 2 * Margin - 2 * Width);
        }
    }

    public class Menu_Calculator : MenuControlUnit
    {
        public Menu_Calculator(Menu menu)
        {
            HoveringTime = 60;

            Parent = menu;
            Image_Normal = new BitmapImage(new Uri("Images/Icon_Calculator_Normal.png", UriKind.Relative));
            Image_Selecting = new BitmapImage(new Uri("Images/Icon_Calculator_Selecting.png", UriKind.Relative));

            Width = 100;
            Height = 100;

            CalMargin();

            PosX = (int)(Margin + 0 * (Width + Gap));
            PosY = 175;
            UpdateRect();
        }

        protected override bool HoverTimesUp(int mousePosX, int mousePosY)
        {
            MainWindow.Manager.AddApp(new App_Calculator());
            Parent.SetShow(false);
            return false;
        }
    }

    public class Menu_FileExplorer : MenuControlUnit
    {
        public Menu_FileExplorer(Menu menu)
        {
            HoveringTime = 60;

            Parent = menu;
            Image_Normal = new BitmapImage(new Uri("Images/Icon_FileExplorer_Normal.png", UriKind.Relative));
            Image_Selecting = new BitmapImage(new Uri("Images/Icon_FileExplorer_Selecting.png", UriKind.Relative));

            Width = 100;
            Height = 100;

            CalMargin();

            PosX = (int)(Margin + 1 * (Width + Gap));
            PosY = 175;
            UpdateRect();
        }

        protected override bool HoverTimesUp(int mousePosX, int mousePosY)
        {
            MainWindow.Manager.AddApp(new App_FileExplorer());
            Parent.SetShow(false);
            return false;
        }
    }

    public class Calculator_Functions : LocalControlUnit{
        char functionKey;
        SolidColorBrush color;

        public Calculator_Functions(App_Calculator calculator, char functionKey)
        {
            HoveringTime = 60;

            Parent = calculator;
            this.functionKey = functionKey;
            color = Brushes.Gray;

            Height = 55;
            Width = 70;

            switch (functionKey)
            {
                case '0':
                    PosX = 8;
                    PosY = 335;
                    break;
                case '1':
                    PosX = 8;
                    PosY = 270;
                    break;
                case '2':
                    PosX = 86;
                    PosY = 270;
                    break;
                case '3':
                    PosX = 164;
                    PosY = 270;
                    break;
                case '4':
                    PosX = 8;
                    PosY = 205;
                    break;
                case '5':
                    PosX = 86;
                    PosY = 205;
                    break;
                case '6':
                    PosX = 164;
                    PosY = 205;
                    break;
                case '7':
                    PosX = 8;
                    PosY = 140;
                    break;
                case '8':
                    PosX = 86;
                    PosY = 140;
                    break;
                case '9':
                    PosX = 164;
                    PosY = 140;
                    break;
                case '.':
                    PosX = 86;
                    PosY = 335;
                    break;
                case '=':
                    PosX = 164;
                    PosY = 335;
                    break;
                case '+':
                    PosX = 242;
                    PosY = 270;
                    Height = 120;
                    break;
                case '-':
                    PosX = 242;
                    PosY = 205;
                    break;
                case 'x':
                    PosX = 242;
                    PosY = 140;
                    break;
                case '/':
                    PosX = 242;
                    PosY = 75;
                    break;
                case '%':
                    PosX = 164;
                    PosY = 75;
                    break;
                case 'C':
                    PosX = 8;
                    Width = 148;
                    PosY = 75;
                    break;
                default:
                    Width = 320;
                    Height = 225;
                    PosX = 0;
                    PosY = 0;
                    color = Brushes.Black;
                    break;
            }

            UpdateRect();
        }

        public override void Print()
        {
            //base.Print();
            MainWindow.RenderManager.DrawingContext.DrawRectangle(color, null, Rect);
            FormattedText pathFormattedText = new FormattedText(functionKey + "",
                CultureInfo.GetCultureInfo("en-us"),
                FlowDirection.RightToLeft,
                new Typeface("Verdana"),
                24,
                Brushes.White, 30);
            pathFormattedText.Trimming = TextTrimming.CharacterEllipsis;
            MainWindow.RenderManager.DrawingContext.DrawText(pathFormattedText, new Point(Rect.X + 30, Rect.Y + 20));
        }

        protected override bool HoverTimesUp(int mousePosX, int mousePosY)
        {
            //MainWindow.Manager.AddApp(new App_FileExplorer());
            // hovering to do 
            (Parent as App_Calculator).Calculator_Buttons_Functions(functionKey);
            return false;
        }
    }

    public class Menu_TextEditor : MenuControlUnit
    {
        public Menu_TextEditor(Menu menu)
        {
            HoveringTime = 60;

            Parent = menu;
            Image_Normal = new BitmapImage(new Uri("Images/Icon_TextEditor_Normal.png", UriKind.Relative));
            Image_Selecting = new BitmapImage(new Uri("Images/Icon_TextEditor_Selecting.png", UriKind.Relative));

            Width = 100;
            Height = 100;

            CalMargin();

            PosX = (int)(Margin + 0 * (Width + Gap));
            PosY = 175;
            UpdateRect();
        }

        protected override bool HoverTimesUp(int mousePosX, int mousePosY)
        {
            MainWindow.Manager.AddApp(new App_Calculator());
            Parent.SetShow(false);
            return false;
        }
    }
    
    public class TextEditor_Functions : LocalControlUnit
    {
        public string functionKey;
        int fontSize;
        TextAlignment align;
        int extraTextPosX;
        public SolidColorBrush color;
        int tempPosY;
        bool isSaveButton;

        public TextEditor_Functions(App_TextEditor textEditor, string functionKey)
        {
            HoveringTime = 30;

            Parent = textEditor;
            this.functionKey = functionKey;
            color = Brushes.Gray;

            Height = 50;
            Width = 55;
            fontSize = 16;
            align = TextAlignment.Left;
            extraTextPosX = 0;
            isSaveButton = false;

            switch (functionKey)
            {
                case "Insert":
                    PosX = 5;
                    PosY = 5;
                    fontSize = 12;
                    break;
                case "1":
                    PosX = 65;
                    PosY = 5;
                    break;
                case "2":
                    PosX = 125;
                    PosY = 5;
                    break;
                case "3":
                    PosX = 185;
                    PosY = 5;
                    break;
                case "4":
                    PosX = 245;
                    PosY = 5;
                    break;
                case "5":
                    PosX = 305;
                    PosY = 5;
                    break;
                case "6":
                    PosX = 365;
                    PosY = 5;
                    break;
                case "7":
                    PosX = 425;
                    PosY = 5;
                    break;
                case "8":
                    PosX = 485;
                    PosY = 5;
                    break;
                case "9":
                    PosX = 545;
                    PosY = 5;
                    break;
                case "0":
                    PosX = 605;
                    PosY = 5;
                    break;
                case "-":
                    PosX = 665;
                    PosY = 5;
                    break;
                case "=":
                    PosX = 725;
                    PosY = 5;
                    break;
                case "Backspace":
                    PosX = 785;
                    PosY = 5;
                    Width = 75;
                    fontSize = 12;
                    extraTextPosX = 1;
                    align = TextAlignment.Right;
                    break;

                case "Delete":
                    PosX = 5;
                    PosY = 60;
                    Width = 70;
                    fontSize = 12;
                    break;
                case "q":
                    PosX = 80;
                    PosY = 60;
                    break;
                case "w":
                    PosX = 140;
                    PosY = 60;
                    break;
                case "e":
                    PosX = 200;
                    PosY = 60;
                    break;
                case "r":
                    PosX = 260;
                    PosY = 60;
                    break;
                case "t":
                    PosX = 320;
                    PosY = 60;
                    break;
                case "y":
                    PosX = 380;
                    PosY = 60;
                    break;
                case "u":
                    PosX = 440;
                    PosY = 60;
                    break;
                case "i":
                    PosX = 500;
                    PosY = 60;
                    break;
                case "o":
                    PosX = 560;
                    PosY = 60;
                    break;
                case "p":
                    PosX = 620;
                    PosY = 60;
                    break;

                case "[":
                    PosX = 680;
                    PosY = 60;
                    break;
                case "]":
                    PosX = 740;
                    PosY = 60;
                    break;
                case "\\":
                    PosX = 800;
                    PosY = 60;
                    Width = 60;
                    break;

                case "Capslock":
                    PosX = 5;
                    PosY = 115;
                    Width = 90;
                    fontSize = 12;
                    this.functionKey = "CapsLk";
                    break;
                case "a":
                    PosX = 100;
                    PosY = 115;
                    break;
                case "s":
                    PosX = 160;
                    PosY = 115;
                    break;
                case "d":
                    PosX = 220;
                    PosY = 115;
                    break;
                case "f":
                    PosX = 280;
                    PosY = 115;
                    break;
                case "g":
                    PosX = 340;
                    PosY = 115;
                    break;
                case "h":
                    PosX = 400;
                    PosY = 115;
                    break;
                case "j":
                    PosX = 460;
                    PosY = 115;
                    break;
                case "k":
                    PosX = 520;
                    PosY = 115;
                    break;
                case "l":
                    PosX = 580;
                    PosY = 115;
                    break;
                case ";":
                    PosX = 640;
                    PosY = 115;
                    break;
                case "'":
                    PosX = 700;
                    PosY = 115;
                    break;
                case "Enter":
                    PosX = 760;
                    PosY = 115;
                    Width = 100;
                    fontSize = 12;
                    align = TextAlignment.Right;
                    extraTextPosX = 55;
                    break;

                case "Left Shift":
                    PosX = 5;
                    PosY = 170;
                    Width = 115;
                    fontSize = 12;
                    this.functionKey = "Shift";
                    break;
                case "z":
                    PosX = 125;
                    PosY = 170;
                    break;
                case "x":
                    PosX = 185;
                    PosY = 170;
                    break;
                case "c":
                    PosX = 245;
                    PosY = 170;
                    break;
                case "v":
                    PosX = 305;
                    PosY = 170;
                    break;
                case "b":
                    PosX = 365;
                    PosY = 170;
                    break;
                case "n":
                    PosX = 425;
                    PosY = 170;
                    break;
                case "m":
                    PosX = 485;
                    PosY = 170;
                    break;
                case ",":
                    PosX = 545;
                    PosY = 170;
                    break;
                case ".":
                    PosX = 605;
                    PosY = 170;
                    break;
                case "/":
                    PosX = 665;
                    PosY = 170;
                    break;
                case "Right Shift":
                    PosX = 725;
                    PosY = 170;
                    Width = 135;
                    fontSize = 12;
                    this.functionKey = "Shift";
                    align = TextAlignment.Right;
                    extraTextPosX = 93;
                    break;

                case "Up":
                    PosX = 940;
                    PosY = 170;
                    fontSize = 12;
                    extraTextPosX = 22;
                    align = TextAlignment.Center;
                    break;
                              
                case "Save":
                    isSaveButton = true;
                    PosX = 900;
                    PosY = 10;
                    Width = 70;
                    Height = 50;
                    extraTextPosX = 30;
                    align = TextAlignment.Center;
                    break; 

                case "Space":
                    PosX = 245;
                    PosY = 225;
                    Width = 275;
                    this.functionKey = " ";
                    break;

                case "Left":
                    PosX = 880;
                    PosY = 225;
                    fontSize = 12;
                    extraTextPosX = 22;
                    align = TextAlignment.Center;
                    break;
                case "Down":
                    PosX = 940;
                    PosY = 225;
                    fontSize = 12;
                    extraTextPosX = 22;
                    align = TextAlignment.Center;
                    break;
                case "Right":
                    PosX = 1000;
                    PosY = 225;
                    fontSize = 12;
                    extraTextPosX = 22;
                    align = TextAlignment.Center;
                    break;   
                    
                default:
                    break;
            }
            tempPosY = PosY;

            UpdateRect();
        }

        public void Print(int textEditorHeight, int textEditorWidth)
        {
            //base.Print();

            if (isSaveButton)
                PosX = textEditorWidth - Width - 5;
            else PosY = tempPosY + textEditorHeight - 280;
            UpdateRect();
            MainWindow.RenderManager.DrawingContext.DrawRectangle(color, null, Rect);
            FormattedText pathFormattedText = new FormattedText(functionKey + "",
                CultureInfo.GetCultureInfo("en-us"),
                align == TextAlignment.Right ? FlowDirection.RightToLeft : FlowDirection.LeftToRight,
                new Typeface("Verdana"),
                fontSize,
                Brushes.White, 30);
            pathFormattedText.Trimming = TextTrimming.CharacterEllipsis;
            pathFormattedText.TextAlignment = align;
            MainWindow.RenderManager.DrawingContext.DrawText(pathFormattedText, new Point(Rect.X + 5 + extraTextPosX, Rect.Y + 20));
        }

        protected override bool HoverTimesUp(int mousePosX, int mousePosY)
        {
            //MainWindow.Manager.AddApp(new App_FileExplorer());
            // hovering to do 
            (Parent as App_TextEditor).TextEditor_Buttons_Functions(functionKey);
            return false;
        }
    }

}
