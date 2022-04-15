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
        protected bool isHovering;
        public int HoveringTime { get; protected set; } = -1;//-1 == no stay event

        //Call when hovering
        protected virtual void Hovering(int mousePosX, int mousePosY)
        {

        }

        protected virtual void Dragging(int mousePosX, int mousePosY)
        {

        }

        protected virtual void OnRelease(int mousePosX, int mousePosY)
        {

        }

        protected virtual void HoverTimesUp(int mousePosX, int mousePosY)
        {

        }

        public virtual bool InRange(int mousePosX, int mousePosY)
        {
            return mousePosX > PosX && mousePosX < PosX + Width &&
                mousePosY > PosY && mousePosY < PosY + Height;
        }

        public virtual bool IsHovering(int mousePosX, int mousePosY, MouseButtonState mouseState)
        {
            isHovering = false;
            if (MainWindow.dragging == this)
            {
                if (mouseState == MouseButtonState.Pressed)
                {
                    Dragging(mousePosX, mousePosY);
                }
                else
                {
                    OnRelease(mousePosX, mousePosY);
                    MainWindow.dragging = null;
                }
            }

            //Please make sure it doesnt have overlap problem
            //Otherwise MainWindow.hovering will have a incorrect value
            if ((MainWindow.hovering == null || MainWindow.hovering == this) && MainWindow.dragging == null && InRange(mousePosX, mousePosY))
            {
                //TODO each point should have an individual timer and class
                if (HoveringTime > 0 && MainWindow.hovering == this && ++MainWindow.timerOfHovering >= HoveringTime)
                {
                    MainWindow.timerOfHovering = 0;
                    HoverTimesUp(mousePosX, mousePosY);
                }

                isHovering = true;
                Hovering(mousePosX, mousePosY);

                if (MainWindow.dragging == null && mouseState == MouseButtonState.Pressed)
                {
                    MainWindow.dragging = this;
                }
                else
                {
                    MainWindow.hovering = this;
                }
                return true;
            }
            else
            {
                //Release it
                if (MainWindow.hovering == this)
                {
                    MainWindow.timerOfHovering = 0;
                    MainWindow.hovering = null;
                }
            }
            return false;
        }

        public void Show(DrawingContext dc)
        {
            dc.DrawImage(isHovering ? Image_Selecting : Image_Normal, Rect);
        }
        public virtual void UpdateRect()
        {
            Rect = new Rect(PosX, PosY, Width, Height);
        }
    }

    public class LocalControlUnit : ControlUnit
    {
        public NonDesktopApplication Parent { get; protected set; }

        public override bool InRange(int mousePosX, int mousePosY)
        {
            return mousePosX > PosX + Parent.PosX && mousePosX < PosX + Parent.PosX + Width &&
                mousePosY > PosY + Parent.PosY && mousePosY < PosY + Parent.PosY + Height;
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
    }

    public class TopCenterDrag : LocalControlUnit
    {
        //TODO: when dragging, show TopRightFull & MiddleRightClose and check them
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

        protected override void HoverTimesUp(int mousePosX, int mousePosY)
        {
            Parent.OnClose();
            MainWindow.Manager.RemoveApp(Parent);
            //Trace.WriteLine("Close");
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

            MainWindow.menu.SetDrag(true);

            //Print Menu
            if (!MainWindow.menu.IsShowed)
                MainWindow.DrawingContext.DrawRectangle(Brushes.Gray, null, new Rect(0, mousePosY, MainWindow.Drawing_Width, MainWindow.Drawing_Height - mousePosY));
            else
                MainWindow.DrawingContext.DrawRectangle(Brushes.Gray, null, new Rect(0, 0, MainWindow.Drawing_Width, mousePosY));

            Rect = new Rect(PosX, mousePosY - Height / 2, Width, Height);
        }

        protected override void OnRelease(int mousePosX, int mousePosY)
        {
            Trace.WriteLine("mousePosY: " + mousePosY + " Drawing_Height: " + MainWindow.Drawing_Height + " /2 : " + (MainWindow.Drawing_Height / 2));

            MainWindow.menu.SetDrag(false);

            //Check if is half
            if (mousePosY < MainWindow.Drawing_Height / 2)
            {
                //Show menu
                MainWindow.menu.SetShow(!MainWindow.menu.IsShowed);

                if (MainWindow.menu.IsShowed)
                    MainWindow.Manager.SetFocus(MainWindow.menu);
            }

            Rect = new Rect(PosX, PosY, Width, Height);
        }

        //TODO: needs UpdateRect as well when changing the window
    }

    public class MiddleRightElement : GlobalControlUnit
    {
        //Changable in the image

        public MiddleRightElement(BitmapImage image_Normal, BitmapImage image_Selecting)
        {
            HoveringTime = 60;

            Image_Normal = image_Normal;
            Image_Selecting = image_Selecting;
        }

        public MiddleRightElement(string uri_Noraml, string uri_Selecting)
        {
            HoveringTime = 60;

            Image_Normal = new BitmapImage(new Uri(uri_Noraml, UriKind.Relative));
            Image_Selecting = new BitmapImage(new Uri(uri_Selecting, UriKind.Relative));
        }

        public override void UpdateRect()
        {
            Width = 50;
            Height = 50;
            PosX = MainWindow.Drawing_Width - Width;
            PosY = MainWindow.Drawing_Height / 2 - Height * MainWindow.Manager.RunningApps.Count / 2;

            Rect = new Rect(PosX, PosY, Width, Height);
        }

        protected override void Dragging(int mousePosX, int mousePosY)
        {
            //Change the dragging target to slide bar
            MainWindow.dragging = MainWindow.Manager.MiddleRightSlideBar;
        }
    }

    public class MiddleRightSlideBar : GlobalControlUnit
    {
        //Wake when drag on the MiddleLeftElement
        public MiddleRightSlideBar()
        {
            Width = 50;
            Height = 250;//Flexible to the number of running app
            PosX = MainWindow.Drawing_Width - 50;
            PosY = MainWindow.Drawing_Height / 2 - Height / 2;

            UpdateRect();
        }

        protected override void Dragging(int mousePosX, int mousePosY)
        {
            //TODO: Code of slide, return if dont have to slide
        }
    }

    public class Video_Timespan : LocalControlUnit
    {
        Rect rect_TimeLine;

        public Video_Timespan(App_VideoPlayer parent)
        {
            Parent = parent;
            PosX = 40;
            PosY = Parent.Height / 2;
            Width = Parent.Width - 80;
            Height = Parent.Height / 2 - 40;

            UpdateRect();
        }

        protected override void Dragging(int mousePosX, int mousePosY)
        {
            //Get the percentage
            double timePos = mousePosX - (Parent.PosX + PosX);
            timePos = timePos < 0 ? 0 : timePos > Width ? Width : timePos;
            double percent = timePos / (double)Width;

            (Parent as App_VideoPlayer).player.Position = TimeSpan.FromSeconds(percent * (Parent as App_VideoPlayer).player.NaturalDuration.TimeSpan.TotalSeconds);

            MainWindow.DrawingContext.DrawRectangle(Brushes.White, null, rect_TimeLine);
            MainWindow.DrawingContext.DrawRectangle(Brushes.Gray, null,
                new Rect((Parent.PosX + PosX) + (percent * Width - 4),
                (Parent.PosY + PosY) + Height / 2 + 20 - 6, 4, 12)
                );
        }

        public override void UpdateRect()
        {
            PosY = Parent.Height / 2;
            Width = Parent.Width - 80;
            Height = Parent.Height / 2 - 40;

            base.UpdateRect();

            rect_TimeLine = new Rect((Parent.PosX + PosX), (Parent.PosY + PosY) + Height / 2 + 20 - 2, Width, 4);
        }
    }

    public class Video_Volume : LocalControlUnit
    {
        Rect rect_Volume_Icon;
        Rect rect_Volume;
        BitmapImage Image_Volume;

        public Video_Volume(App_VideoPlayer parent)
        {
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
            //Get the percentage
            double timePos = mousePosY - (Parent.PosY + PosY);
            timePos = timePos < 0 ? 0 : timePos > Height ? Height : timePos;
            double percent = timePos / (double)Height;

            (Parent as App_VideoPlayer).player.Volume = 1.0 - percent;

            MainWindow.DrawingContext.DrawImage(Image_Volume, rect_Volume_Icon);
            MainWindow.DrawingContext.DrawRectangle(Brushes.White, null, rect_Volume);
            MainWindow.DrawingContext.DrawRectangle(Brushes.Gray, null,
                new Rect((Parent.PosX + PosX) + Width * 0.75 - 6,
                (Parent.PosY + PosY) + Height * percent, 12, 4)
                );
        }

        public override void UpdateRect()
        {
            Width = Parent.Width - 80;
            Height = Parent.Height / 2 - 40;

            base.UpdateRect();

            rect_Volume = new Rect((Parent.PosX + PosX) + Width * 0.75 - 2, (Parent.PosY + PosY), 4, Height);
            rect_Volume_Icon = new Rect((Parent.PosX + PosX) + Width * 0.25 - 40, (Parent.PosY + PosY), 80, 80);
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
    }
}
