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

        //Call when hovering
        protected virtual void Hovering(int mousePosX, int mousePosY)
        {

        }

        protected virtual void Dragging(int mousePosX, int mousePosY)
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
                    MainWindow.dragging = null;
                }
            }

            if (InRange(mousePosX, mousePosY))
            {
                isHovering = true;
                Hovering(mousePosX, mousePosY);
                if(MainWindow.dragging == null && mouseState == MouseButtonState.Pressed)
                {
                    MainWindow.dragging = this;
                }
                return true;
            }
            return false;
        }

        public void Show(DrawingContext dc)
        {
            dc.DrawImage(isHovering?Image_Selecting:Image_Normal, Rect);
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

        public virtual void UpdateRect()
        {
            Rect = new Rect(PosX + Parent.PosX, PosY + Parent.PosY, Width, Height);
        }
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
            int distX = mousePosX - (PosX + Parent.PosX + Width/2);
            int distY = mousePosY - (PosY + Parent.PosY + Height/2);

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
        LocalControlUnit[] localControlUnits;
        public TopCenterDrag(NonDesktopApplication parent)
        {
            Parent = parent;
            localControlUnits = new LocalControlUnit[] { 
                new TopRightFull(), new MiddleRightClose()
            };

            Image_Normal = new BitmapImage(new Uri("Images/AreaSelection_Drag_Normal.png", UriKind.Relative));
            Image_Selecting = new BitmapImage(new Uri("Images/AreaSelection_Drag_Selecting.png", UriKind.Relative));

            Width = 50;
            Height = 50;
            PosX = (Parent.Width - Width)/2;
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

    //Appears when dragging
    public class TopRightFull : LocalControlUnit
    {

    }

    //Appears when dragging
    public class MiddleRightClose : LocalControlUnit
    {

    }
}
