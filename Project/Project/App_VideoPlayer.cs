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

            controlZones = new LocalControlUnit[] { new Video_Timespan(this), new Video_Volume(this) };

            player = new MediaPlayer();
            player.Open(new Uri("E:/School/CityU/221/SM3603/SM3603_Project/SampleVideos/277865651_138940418661949_6096681436469973289_n.mp4"));
            player.Play();

            video_AspectRatio = 0;
            app_AspectRatio = Width / (double)Height;
        }

        public override void Update(bool isFocusing, Point point, MouseButtonState mouseState)
        {
            base.Update(isFocusing, point, mouseState);

            int clampedX = point.X < 0 ? 0 : point.X > MainWindow.Drawing_Width ? MainWindow.Drawing_Width : (int)point.X;
            int clampedY = point.Y < 0 ? 0 : point.Y > MainWindow.Drawing_Height ? MainWindow.Drawing_Height : (int)point.Y;

            foreach (LocalControlUnit unit in controlZones)
            {
                unit.IsHovering(clampedX, clampedY, mouseState);
            }
        }

        public override void Print()
        {
            MainWindow.DrawingContext.DrawRectangle(Brushes.Black, null, Rect);

            //Cal the resolution, since in the contructor it hasn't been calculated
            if (video_AspectRatio == 0)
            {
                if(player.NaturalVideoWidth != 0)
                    video_AspectRatio = player.NaturalVideoWidth / (double)player.NaturalVideoHeight;
                else
                    return;
            }

            if (video_AspectRatio > app_AspectRatio)
            {
                //Up and Down

                //Cal the video height
                double video_Height = Width / video_AspectRatio;
                MainWindow.DrawingContext.DrawVideo(player, new Rect(PosX, PosY + (Height - video_Height) / 2, Width, video_Height));
            }
            else
            {
                //Left and Right

                //Cal the video width
                double video_Width = Height * video_AspectRatio;
                MainWindow.DrawingContext.DrawVideo(player, new Rect(PosX + (Width - video_Width) / 2, PosY, video_Width, Height));
            }

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
