using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Project
{
    public class App_VideoPlayer : NonDesktopApplication
    {
        public MediaPlayer player { get; protected set; }
        double app_AspectRatio;
        double video_AspectRatio;
        LocalControlUnit[] controlZones;

        public App_VideoPlayer(string path)
        {
            Image_Normal = new BitmapImage(new Uri("Images/Icon_VideoPlayer_Normal.png", UriKind.Relative));
            Image_Selecting = new BitmapImage(new Uri("Images/Icon_VideoPlayer_Selecting.png", UriKind.Relative));

            PosX = 100;//For testing
            LocalEdgeControl = new LocalEdgeControl(this);
            Rect = new Rect(PosX, PosY, Width, Height);

            //Please enter the needed speech
            Grammars = new Microsoft.Speech.Recognition.Grammar[] { MainWindow.GetGrammar("", new string[] { "stop", "pause", "play" }), MainWindow.GetGrammar("volume", new string[] { "up", "down", "mute", "max" }), MainWindow.GetGrammar("volume level", new string[] { "zero", "one", "two", "three", "four", "five" }) };

            foreach (Microsoft.Speech.Recognition.Grammar grammar in Grammars)
            {
                MainWindow.mainWindow.BuildNewGrammar(grammar);
            }

            controlZones = new LocalControlUnit[] { new Video_Timespan(this), new Video_Volume(this) };

            player = new MediaPlayer();
            player.Open(new Uri(path));
            player.Play();

            video_AspectRatio = 0;
            app_AspectRatio = Width / (double)Height;
        }

        public override void Update(bool isFocusing, int listOrder, Point point, MouseButtonState mouseState, string command)
        {
            base.Update(isFocusing, listOrder, point, mouseState, command);

            if (!isFocusing) return;

            int clampedX = point.X < 0 ? 0 : point.X > MainWindow.Drawing_Width ? MainWindow.Drawing_Width : (int)point.X;
            int clampedY = point.Y < 0 ? 0 : point.Y > MainWindow.Drawing_Height ? MainWindow.Drawing_Height : (int)point.Y;

            foreach (LocalControlUnit unit in controlZones)
            {
                unit.IsHoveringOrDragging(clampedX, clampedY, listOrder, mouseState);
            }

            VoiceControl(command);
        }

        public override void VoiceControl(string command)
        {
            switch (command)
            {
                case "stop":
                    player.Stop();
                    break;
                case "pause":
                    player.Pause();
                    break;
                case "play":
                    player.Play();
                    break;
                case "volume up":
                    player.Volume += 0.2;
                    break;
                case "volume down":
                    player.Volume -= 0.2;
                    break;
                case "volume mute":
                    player.Volume = 0;
                    break;
                case "volume max":
                    player.Volume = 1;
                    break;
                case "volume level zero":
                    player.Volume = 0;
                    break;
                case "volume level one":
                    player.Volume = 0.2;
                    break;
                case "volume level two":
                    player.Volume = 0.4;
                    break;
                case "volume level three":
                    player.Volume = 0.6;
                    break;
                case "volume level four":
                    player.Volume = 0.8;
                    break;
                case "volume level five":
                    player.Volume = 1;
                    break;
                case "close app":
                    OnClose();
                    MainWindow.Manager.RemoveApp(this);
                    break;
                default:
                    break;
            }
        }

        public override void Print()
        {
            MainWindow.RenderManager.DrawingContext.DrawRectangle(Brushes.Black, null, Rect);

            //Cal the resolution, since in the contructor it hasn't been calculated
            if (video_AspectRatio == 0)
            {
                if(player.NaturalVideoWidth != 0)
                    video_AspectRatio = player.NaturalVideoWidth / (double)player.NaturalVideoHeight;
            }

            if (video_AspectRatio != 0)
            {
                if (video_AspectRatio > app_AspectRatio)
                {
                    //Up and Down

                    //Cal the video height
                    double video_Height = Width / video_AspectRatio;
                    MainWindow.RenderManager.DrawingContext.DrawVideo(player, new Rect(PosX, PosY + (Height - video_Height) / 2, Width, video_Height));
                }
                else
                {
                    //Left and Right

                    //Cal the video width
                    double video_Width = Height * video_AspectRatio;
                    MainWindow.RenderManager.DrawingContext.DrawVideo(player, new Rect(PosX + (Width - video_Width) / 2, PosY, video_Width, Height));
                }
            }

            foreach (LocalControlUnit unit in controlZones)
            {
                unit.Print();
            }

            LocalEdgeControl.Print();
        }

        public override void UpdateRect()
        {
            base.UpdateRect();
            app_AspectRatio = Width / (double)Height;
            foreach(LocalControlUnit unit in controlZones)
            {
                unit.UpdateRect();
            }
        }

        public override void OnClose()
        {
            player.Stop();
        }
    }
}
