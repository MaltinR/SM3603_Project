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
        public static DrawingContext DrawingContext { get; private set; }
        public static int Drawing_Width;
        public static int Drawing_Height;
        //To be wrapped up
        public static int timerOfHovering = 0;

        List<Proj_Application> list_ToBeRemoved;

        //Color frame
        byte[] blackScreenData = null;
        KinectSensor sensor = null;
        WriteableBitmap colorImageBitmap = null;
        FrameDescription colorFrameDescription = null;
        byte[] colorData = null;

        //Debug
        int frameLoop = 0;

        //Applications
        List<Proj_Application> runningApps;
        Proj_Application onFocusApp;
        public static Menu menu { get; private set; }//The only

        //Global controlunit
        public List<GlobalControlUnit> globalUnits;

        //Temp (Will be wrapped as a class)
        public static ControlUnit dragging;
        public static ControlUnit hovering;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            mainWindow = this;

            Drawing_Width = (int)DrawingPlane.Width;
            Drawing_Height = (int)DrawingPlane.Height;

            sensor = KinectSensor.GetDefault(); // get the default Kinect sensor 
            sensor.Open();

            ColorFrameInit();
            DrawingGroupInit();

            //GlobalUnits
            globalUnits = new List<GlobalControlUnit>();
            globalUnits.Add(new BotCenterMenu());

            runningApps = new List<Proj_Application>();
            list_ToBeRemoved = new List<Proj_Application>();
            //At least one application will be run, which is the desktop
            menu = new Menu();
            onFocusApp = new App_Desktop();
            runningApps.Add(onFocusApp);
            runningApps.Add(menu);

            //Debug
            runningApps.Add(new App_VideoPlayer("E:/School/CityU/221/SM3603/SM3603_Project/SampleVideos/277865651_138940418661949_6096681436469973289_n.mp4"));
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
                DrawingContext = dc;
                //Trace.WriteLine("Loaded2");

                // draw a transparent background to set the render size
                dc.DrawRectangle(Brushes.Transparent, null,
                        new Rect(0.0, 0.0, DrawingPlane.Width, DrawingPlane.Height));


                Point mousePos = Mouse.GetPosition(DrawingPlane);

                //Print
                foreach (Proj_Application app in runningApps.Reverse<Proj_Application>())
                {
                    app.Print();
                }

                //GlobalUnit
                int clampedX = mousePos.X < 0 ? 0 : mousePos.X > MainWindow.Drawing_Width ? MainWindow.Drawing_Width : (int)mousePos.X;
                int clampedY = mousePos.Y < 0 ? 0 : mousePos.Y > MainWindow.Drawing_Height ? MainWindow.Drawing_Height : (int)mousePos.Y;

                foreach (GlobalControlUnit unit in globalUnits)
                {
                    unit.IsHovering(clampedX, clampedY, Mouse.LeftButton);
                    unit.Show(dc);
                }

                //Update app
                foreach (Proj_Application app in runningApps)
                {
                    //MousePos will be subtituded by handPos and MouseOnClicked will be subtituded by gesture
                    app.Update(app == onFocusApp, mousePos, Mouse.LeftButton);
                }

                //Trace.WriteLine("hovering: " + (hovering == null) + " " + (hovering is BotCenterMenu));
                

                if (timerOfHovering > 0)
                {
                    int scale_Hold = (int)(timerOfHovering / (double)hovering.HoveringTime * 50.0);

                    dc.DrawImage(Select_Hold.Source, new Rect(mousePos.X - scale_Hold / 2, mousePos.Y - scale_Hold / 2, scale_Hold, scale_Hold));
                    dc.DrawImage(Select_Outline.Source, new Rect(mousePos.X - 50 / 2, mousePos.Y - 50 / 2, 50, 50));
                }

                //LateProcess (RemoveApp)
                LateProcess();
            }

            frameLoop++;
            if (frameLoop >= 30) frameLoop = 0;
        }

        public void RemoveFromApp(Proj_Application app)
        {
            list_ToBeRemoved.Add(app);
        }

        void LateProcess()
        {
            Late_RemoveApp();
        }

        void Late_RemoveApp()
        {
            for (int i = list_ToBeRemoved.Count - 1; i >= 0; i--)
            {
                runningApps.Remove(list_ToBeRemoved[i]);
                list_ToBeRemoved.RemoveAt(i);
            }
        }

        public void SetFocus(Proj_Application app)
        {
            onFocusApp = app;

            //TODO: change the order of the list

        }
    }
}
