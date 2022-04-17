using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Kinect;
using System.Diagnostics;

namespace Project
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        DrawingGroup drawingGroup;
        DrawingImage drawingImg;

        public static MainWindow mainWindow;
        public static int Drawing_Width;
        public static int Drawing_Height;
        //To be wrapped up
        public static int timerOfHovering = 0;

        //Color frame
        byte[] blackScreenData = null;
        KinectSensor sensor = null;
        WriteableBitmap colorImageBitmap = null;
        FrameDescription colorFrameDescription = null;
        byte[] colorData = null;

        //Debug
        int frameLoop = 0;

        //Applications
        public static AppManager Manager;

        public static RenderManager RenderManager;

        //Temp (Will be wrapped as a class)
        public static ControlUnit dragging;//To point class (Hand)
        public static ControlUnit hovering;//To point class (Hand)

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            mainWindow = this;

            Drawing_Width = (int)DrawingPlane.Width;
            Drawing_Height = (int)DrawingPlane.Height;

            sensor = KinectSensor.GetDefault(); // get the default Kinect sensor 
            sensor.Open();

            ColorFrameInit();
            DrawingGroupInit();

            Manager = new AppManager();
            RenderManager = new RenderManager();

            RenderManager.RenderList.Add(new RenderManager.RenderClass(Manager.Menu));

            //Debug
            //Manager.AddApp(new App_VideoPlayer("E:/School/CityU/221/SM3603/SM3603_Project/SampleVideos/277957136_1030137967586408_6026758252614106551_n.mp4"));
            //Manager.AddApp(new App_VideoPlayer("E:/School/CityU/221/SM3603/SM3603_Project/SampleVideos/277957136_1030137967586408_6026758252614106551_n.mp4"));
            //Manager.AddApp(new App_FileExplorer());
            Manager.AddApp(new App_ImageEditor("E:/School/CityU/221/SM3603/SM3603_Project/Test.png"));
        }

        //This following function is borrowed from course's slides
        void ColorFrameInit()
        {
            ColorFrameReader colorFrameReader = sensor.ColorFrameSource.OpenReader();

            colorFrameReader.FrameArrived += ColorFrameReader_FrameArrived;

            colorFrameDescription = sensor.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Bgra);

            colorData = new byte[colorFrameDescription.LengthInPixels * colorFrameDescription.BytesPerPixel];

            colorImageBitmap = new WriteableBitmap(
                      colorFrameDescription.Width,
                      colorFrameDescription.Height,
                      96, // dpi-x
                      96, // dpi-y
                      PixelFormats.Bgr32, // pixel format  
                      null);


            ColorCam.Source = colorImageBitmap;

            blackScreenData = new byte[colorFrameDescription.LengthInPixels * colorFrameDescription.BytesPerPixel];
        }

        // The following function is borrowed from SM3603-Topic06
        private void DrawingGroupInit() // called in Window_Loaded 
        {
            drawingGroup = new DrawingGroup();
            drawingImg = new DrawingImage(drawingGroup);
            DrawingPlane.Source = drawingImg;
            // prevent drawing outside of our render area
            drawingGroup.ClipGeometry = new RectangleGeometry(
                                        new Rect(0.0, 0.0,
                                        SystemParameters.PrimaryScreenWidth, 
                                        SystemParameters.PrimaryScreenHeight));
                                        //DrawingPlane.Width,
                                        //DrawingPlane.Height));
        }

        private void ColorFrameReader_FrameArrived(object sender, ColorFrameArrivedEventArgs e)
        {

            using (DrawingContext dc = drawingGroup.Open())
            {
                RenderManager.DrawingContext = dc;

                // draw a transparent background to set the render size
                dc.DrawRectangle(Brushes.Transparent, null,
                        new Rect(0.0, 0.0, DrawingPlane.Width, DrawingPlane.Height));

                Point mousePos = Mouse.GetPosition(DrawingPlane);

                int clampedX = mousePos.X < 0 ? 0 : mousePos.X > Drawing_Width ? Drawing_Width : (int)mousePos.X;
                int clampedY = mousePos.Y < 0 ? 0 : mousePos.Y > Drawing_Height ? Drawing_Height : (int)mousePos.Y;

                Manager.Update(clampedX, clampedY, mousePos);

                RenderManager.Render();

                if (timerOfHovering > 0)
                {
                    int scale_Hold = (int)(timerOfHovering / (double)hovering.HoveringTime * 50.0);

                    int cursor_ClampedX = mousePos.X < 25 ? 25 : mousePos.X > Drawing_Width - 25 ? Drawing_Width - 25 : (int)mousePos.X;
                    int cursor_ClampedY = mousePos.Y < 25 ? 25 : mousePos.Y > Drawing_Height - 25 ? Drawing_Height - 25 : (int)mousePos.Y;

                    dc.DrawImage(Select_Hold.Source, new Rect(cursor_ClampedX - scale_Hold / 2, cursor_ClampedY - scale_Hold / 2, scale_Hold, scale_Hold));
                    dc.DrawImage(Select_Outline.Source, new Rect(cursor_ClampedX - 50 / 2, cursor_ClampedY - 50 / 2, 50, 50));
                }

                //LateProcess (RemoveApp)
                Manager.LateProcess();
            }

            frameLoop++;
            if (frameLoop >= 30) frameLoop = 0;
        }
    }
}
