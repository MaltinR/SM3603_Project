using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Threading.Tasks;

namespace Project
{
    public class NonDesktopApplication : Proj_Application
    {
        public BitmapImage Image_Normal { get; protected set; }
        public BitmapImage Image_Selecting { get; protected set; }
        public int MinimumWidth { get; protected set; } = 150;
        public int MinimumHeight { get; protected set; } = 150;
        public bool IsFull { get; protected set; }
        public LocalEdgeControl LocalEdgeControl { get; protected set; }
        public int PosX { get; protected set; }//Top-left pos
        public int PosY { get; protected set; }//Top-left pos
        public int Width { get; protected set; } = 300;
        public int Height { get; protected set; } = 250;
        public Microsoft.Speech.Recognition.Grammar[] Grammars { get; protected set; }

        public void SetWidth(int _width)
        {
            //Limit
            if (_width < MinimumWidth)
            {
                Width = MinimumWidth;

            }
            else
            {
                Width = _width;
            }

            UpdateRect();
        }

        public void SetHeight(int _height)
        {
            //Limit
            if (_height < MinimumHeight)
            {
                Height = MinimumHeight;
            }
            else
            {
                Height = _height;
            }

            UpdateRect();

        }

        public void SetPosX(int _posX)
        {
            PosX = _posX;
            UpdateRect();
        }

        public void SetPosY(int _posY)
        {
            PosY = _posY;
            UpdateRect();
        }

        public virtual void Update(bool isFocusing, int listOrder, Point point, Microsoft.Kinect.HandState handState, string command, string gesture)
        {
            LocalEdgeControl.CheckShow(point, listOrder, MainWindow.RenderManager.DrawingContext, handState);
        }

        public override void Print()
        {
            MainWindow.RenderManager.DrawingContext.DrawRectangle(Brushes.Black, null, Rect);
            LocalEdgeControl.Print();
        }

        public virtual void UpdateRect()
        {
            PosX = PosX < 0 ? 0 : PosX > MainWindow.Drawing_Width - Width ? MainWindow.Drawing_Width - Width : PosX;
            PosY = PosY < 0 ? 0 : PosY > MainWindow.Drawing_Height - Height ? MainWindow.Drawing_Height - Height : PosY;

            Rect = new Rect(PosX, PosY, Width, Height);

            LocalEdgeControl.UpdateRect();
        }

        //Be called when the app close
        public virtual void OnClose()
        {
            if(Grammars != null)
            {
                foreach(Microsoft.Speech.Recognition.Grammar grammar in Grammars)
                {
                    MainWindow.Recognizer.UnloadGrammar(grammar);
                }
            }
        }

        public virtual void VoiceControl(string command)
        {

        }

        public virtual void GestureControl(string gesture)
        {

        }
    }
}
