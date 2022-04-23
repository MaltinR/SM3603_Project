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
using System.Diagnostics;
using Microsoft.Speech.AudioFormat;
using Microsoft.Speech.Recognition;
using Microsoft.Kinect;
using Microsoft.Kinect.VisualGestureBuilder;


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

        //Voice Reg
        private RecognizerInfo kinectRecognizerInfo;
        public static SpeechRecognitionEngine Recognizer { get; private set; }

        //Debug
        int frameLoop = 0;

        public int GestureDetectTimer { get; private set; } = 0;

        //Applications
        public static AppManager Manager;

        public static RenderManager RenderManager;

        //Temp (Will be wrapped as a class)
        public static ControlUnit dragging;//To point class (Hand)
        public static ControlUnit hovering;//To point class (Hand)

        private VisualGestureBuilderFrameSource vgbFrameSource;
        private VisualGestureBuilderDatabase vgbDb;
        private VisualGestureBuilderFrameReader vgbFrameReader;

        private bool isDebug = true;
        public HandControl[] HandControls { get; private set; }
        //private Body[] bodies;

        Gesture _lastFrameGesture;
        SpeechRecognizedEventArgs _lastFrameSpeech;
        Point _hand_Pos;//Depends on which one is closer

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            mainWindow = this;

            Drawing_Width = (int)DrawingPlane.Width;
            Drawing_Height = (int)DrawingPlane.Height;

            sensor = KinectSensor.GetDefault(); // get the default Kinect sensor 
            sensor.Open();

            _hand_Pos = new Point(0, 0);

            HandInit();
            GestureInit();

            ColorFrameInit();
            DrawingGroupInit();

            Manager = new AppManager();
            RenderManager = new RenderManager();

            RenderManager.RenderList.Add(new RenderManager.RenderClass(Manager.Menu));

            kinectRecognizerInfo = FindKinectRecognizerInfo();
            if (kinectRecognizerInfo != null)
            {
                Recognizer = new SpeechRecognitionEngine(kinectRecognizerInfo);
            }

            //BuildCommands();
            BuildGrammar();

            IReadOnlyList<AudioBeam> audioBeamList = sensor.AudioSource.AudioBeams;
            System.IO.Stream audioStream = audioBeamList[0].OpenInputStream();

            T11_VoiceControl.KinectAudioStream kinectAudioStream = new T11_VoiceControl.KinectAudioStream(audioStream);
            // let the convertStream know speech is going active
            kinectAudioStream.SpeechActive = true;

            if (isDebug)//Debug: true = normal mic, false = kinect mic
            {
                Recognizer.SetInputToDefaultAudioDevice();
            }
            else
            {
                Recognizer.SetInputToAudioStream(kinectAudioStream, new SpeechAudioFormatInfo(EncodingFormat.Pcm, 16000, 16, 1, 32000, 2, null));
            }

            // recognize words repeatedly and asynchronously
            Recognizer.RecognizeAsync(RecognizeMode.Multiple);

            Recognizer.SpeechRecognized += Recognizer_SpeechRecognized;

            BodyFrameReaderInit();



            //Debug
            //Manager.AddApp(new App_VideoPlayer("E:/School/CityU/221/SM3603/SM3603_Project/SampleVideos/277957136_1030137967586408_6026758252614106551_n.mp4"));
            //Manager.AddApp(new App_VideoPlayer("E:/School/CityU/221/SM3603/SM3603_Project/SampleVideos/277957136_1030137967586408_6026758252614106551_n.mp4"));
            Manager.AddApp(new App_FileExplorer());
            //Manager.AddApp(new App_ImageEditor("E:/School/CityU/221/SM3603/SM3603_Project/Test.png"));
        }

        public void ResetGestureTimer()
        {
            _lastFrameGesture = null;
            GestureDetectTimer = 30;
        }

        private void GestureInit()
        {
            vgbFrameSource = new VisualGestureBuilderFrameSource(sensor, 0);

            vgbDb = new VisualGestureBuilderDatabase(@".\Gestures\ProjectGestures.gbd");
            //Console.WriteLine("vgbDb.AvailableGestures.Count: " + vgbDb.AvailableGestures.Count);
            vgbFrameSource.AddGestures(vgbDb.AvailableGestures);

            vgbFrameReader = vgbFrameSource.OpenReader();
            vgbFrameReader.FrameArrived += VgbFrameReader_FrameArrived;
        }

        private void HandInit()
        {
            if (isDebug)
            {
                HandControls = new HandControl[1];
            }
            else
            {
                HandControls = new HandControl[2];
            }
        }

        private void VgbFrameReader_FrameArrived(object sender, VisualGestureBuilderFrameArrivedEventArgs e)
        {
            if (GestureDetectTimer > 0)
            {
                GestureDetectTimer--;
                return;
            }

            using (VisualGestureBuilderFrame vgbFrame = e.FrameReference.AcquireFrame())
            {
                if (vgbFrame == null) return;

                IReadOnlyDictionary<Gesture, DiscreteGestureResult> results =
                    vgbFrame.DiscreteGestureResults;
                if (results != null)
                {
                    float highest_Score = 0.15f;

                    Gesture highest_gesture = null;

                    // Check if any of the gestures is recognized 
                    foreach (Gesture gesture in results.Keys)
                    {
                        DiscreteGestureResult result = results[gesture];
                        if (result.Detected)
                        {
                            if (result.Confidence > highest_Score)
                            {
                                highest_gesture = gesture;
                                highest_Score = result.Confidence;
                            }
                            //Console.WriteLine(gesture.Name + " gesture recognized; confidence: " + result.Confidence);
                        }
                    }

                    _lastFrameGesture = highest_gesture;
                    /*
                    if (highest_gesture != null)
                    {
                        DebugLine.Text = (highest_gesture.Name + " : " + highest_Score);
                    }
                    */
                }
            }

        }

        private void Recognizer_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            if (e.Result.Confidence > 0.6)
                _lastFrameSpeech = e;
            Trace.WriteLine(e.Result.Text + " (" + e.Result.Confidence + ")");
        }

        //This following function is borrowed from course's slides (SM3603-Topic06)
        private void BodyFrameReaderInit()
        {
            BodyFrameReader bodyFrameReader = sensor.BodyFrameSource.OpenReader();
            bodyFrameReader.FrameArrived += BodyFrameReader_FrameArrived;

            // BodyCount: maximum number of bodies that can be tracked at one time
            //bodies = new Body[sensor.BodyFrameSource.BodyCount];
            //bodies = new Body[1];//In our project one is ok
        }

        private void BodyFrameReader_FrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            using (BodyFrame bodyFrame = e.FrameReference.AcquireFrame())
            {
                Body body = null;
                //if (bodyFrame == null) return;
                if (bodyFrame != null)
                {
                    //bodyFrame.GetAndRefreshBodyData(bodies);

                    body = GetClosestBody(bodyFrame);
                    if (body != null)
                        vgbFrameSource.TrackingId = body.TrackingId;

                }
                //Play every frame anyway

                using (DrawingContext dc = drawingGroup.Open())
                {
                    RenderManager.DrawingContext = dc;

                    // draw a transparent background to set the render size
                    dc.DrawRectangle(Brushes.Transparent, null,
                            new Rect(0.0, 0.0, DrawingPlane.Width, DrawingPlane.Height));

                    HandState handState = HandState.Unknown;

                    Point hand_L = new Point(), hand_R = new Point();
                    if (body != null)
                    {
                        //Get the closest Hand
                        hand_L = MapCameraPointToColorSpace(body, JointType.HandLeft);
                        hand_R = MapCameraPointToColorSpace(body, JointType.HandRight);

                        if(body.Joints[JointType.HandLeft].Position.Z < body.Joints[JointType.HandRight].Position.Z)
                        {
                            _hand_Pos = hand_L;
                            handState = body.HandLeftState;
                        }
                        else
                        {
                        _hand_Pos = hand_R;
                        handState = body.HandRightState;
                        }
                    }

                    if (isDebug)
                    {
                        _hand_Pos = Mouse.GetPosition(DrawingPlane);

                        handState = Mouse.LeftButton == MouseButtonState.Pressed ? HandState.Closed : HandState.Open;
                    }

                    int clampedX = _hand_Pos.X < 25 ? 25 : _hand_Pos.X > Drawing_Width - 25 ? Drawing_Width - 25: (int)_hand_Pos.X;
                    int clampedY = _hand_Pos.Y < 25 ? 25 : _hand_Pos.Y > Drawing_Height - 25? Drawing_Height - 25: (int)_hand_Pos.Y;

                    Manager.Update(clampedX, clampedY, _hand_Pos, handState, _lastFrameSpeech, _lastFrameGesture);

                    RenderManager.Render();

                    if (timerOfHovering > 0)
                    //if (timerOfHovering > 0 && hovering != null)
                    {
                        int scale_Hold = (int)(timerOfHovering / (double)hovering.HoveringTime * 50.0);

                        int cursor_ClampedX = _hand_Pos.X < 25 ? 25 : _hand_Pos.X > Drawing_Width - 25 ? Drawing_Width - 25 : (int)_hand_Pos.X;
                        int cursor_ClampedY = _hand_Pos.Y < 25 ? 25 : _hand_Pos.Y > Drawing_Height - 25 ? Drawing_Height - 25 : (int)_hand_Pos.Y;

                        dc.DrawImage(Select_Hold.Source, new Rect(cursor_ClampedX - scale_Hold / 2, cursor_ClampedY - scale_Hold / 2, scale_Hold, scale_Hold));
                        dc.DrawImage(Select_Outline.Source, new Rect(cursor_ClampedX - 50 / 2, cursor_ClampedY - 50 / 2, 50, 50));
                    }
                    if (body != null)
                    {
                        if(hand_L == _hand_Pos)
                        {
                            //Draw Right (Green)
                            //DrawPoint(body, dc, JointType.HandRight, Brushes.Green);

                            if(timerOfHovering <= 0)
                                DrawPoint(body, dc, JointType.HandLeft, Brushes.Red);
                        }
                        else
                        {
                            //Draw Left (Red)
                            //DrawPoint(body, dc, JointType.HandLeft, Brushes.Red);
                            if (timerOfHovering <= 0)
                                DrawPoint(body, dc, JointType.HandRight, Brushes.Green);
                        }
                    }

                    //LateProcess (RemoveApp)
                    Manager.LateProcess();

                    //After used, to prevent next frame repeat
                    _lastFrameSpeech = null;
                }

                frameLoop++;
                if (frameLoop >= 30) frameLoop = 0;
            }

        }

        void DrawPoint(Body body, DrawingContext dc, JointType jt, SolidColorBrush color)
        {
            Point pt = MapCameraPointToColorSpace(body, jt);
            Console.WriteLine(pt.X + " , " + pt.Y);
            dc.DrawEllipse(color, null, new Point(pt.X < 10? 10:pt.X >= Drawing_Width - 10? Drawing_Width - 10:pt.X, pt.Y < 10 ? 10 : pt.Y >= Drawing_Height - 10 ? Drawing_Height - 10 : pt.Y), 10, 10);
        }

        //Borrowed from course's slides
        private Point MapCameraPointToColorSpace(Body body, JointType jointType)
        {
            ColorSpacePoint rawPoint = sensor.CoordinateMapper.MapCameraPointToColorSpace(body.Joints[jointType].Position);

            //Console.WriteLine(rawPoint.X + ", " + rawPoint.Y);
            //DebugLine.Text = rawPoint.X + ", " + rawPoint.Y;

            float margin_X = Drawing_Width * 0.2f;
            float margin_Y = Drawing_Height * 0.2f;

            float scale_RawToShow = 1920 / (float)Drawing_Width;
            float scale_ShowToCal = Drawing_Width/(Drawing_Width - 2 * margin_X);

            float x = rawPoint.X / scale_RawToShow;
            float y = rawPoint.Y / scale_RawToShow;

            x -= margin_X;
            y -= margin_Y;

            x *= scale_ShowToCal;
            y *= scale_ShowToCal;

            x = x < 0 ? 0 : x >= Drawing_Width ? Drawing_Width: x;
            y = y < 0 ? 0 : y >= Drawing_Height ? Drawing_Height: y;


            return new Point(x,y);
        }

        //This following function is borrowed from course's slides (SM3603-Topic09)
        private Body GetClosestBody(BodyFrame bodyFrame)
        {
            Body[] bodies = new Body[6];
            bodyFrame.GetAndRefreshBodyData(bodies);

            Body closestBody = null;
            foreach (Body b in bodies)
            {
                if (b.IsTracked)
                {
                    if (closestBody == null) closestBody = b;
                    else
                    {
                        Joint newHeadJoint = b.Joints[JointType.Head];
                        Joint oldHeadJoint = closestBody.Joints[JointType.Head];
                        if (newHeadJoint.TrackingState == TrackingState.Tracked &&
                        newHeadJoint.Position.Z < oldHeadJoint.Position.Z)
                        {
                            closestBody = b;
                        }
                    }
                }
            }
            return closestBody;
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


            //ColorCam.Source = colorImageBitmap;

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
            //Moved to bodyFrame
        }

        //Reference: Course Slide: SM3603-Topic11
        private RecognizerInfo FindKinectRecognizerInfo()
        {
            var recognizers = SpeechRecognitionEngine.InstalledRecognizers();

            foreach (RecognizerInfo recInfo in recognizers)
            {
                // look at each recognizer info value 
                // to find the one that works for Kinect
                if (recInfo.AdditionalInfo.ContainsKey("Kinect"))
                {
                    string details = recInfo.AdditionalInfo["Kinect"];
                    if (details == "True" && recInfo.Culture.Name == "en-US")
                    {
                        // If we get here we have found 
                        // the info we want to use
                        return recInfo;
                    }
                }
            }
            return null;
        }


        public void BuildNewGrammar(Grammar grammar) // call it Window_Loaded()
        {
            Recognizer.LoadGrammar(grammar);
        }

        public static Grammar GetGrammar(string str_GrammarBuilder, string[] options)
        {
            GrammarBuilder grammarBuilder;
            if (str_GrammarBuilder.Length == 0)
            {
                grammarBuilder = new GrammarBuilder();
            }
            else
            {
                grammarBuilder = new GrammarBuilder(str_GrammarBuilder);
            }

            // the same culture as the recognizer (US English)
            grammarBuilder.Culture = MainWindow.mainWindow.kinectRecognizerInfo.Culture;

            //String[] codes = { "Apple", "Watermelon", "Banana" };

            Choices choices = new Choices(options);
            //choices.Add(codes);

            grammarBuilder.Append(choices);

            return new Grammar(grammarBuilder);

            //Grammar grammar = new Grammar(grammarBuilder);

            //recognizer.LoadGrammar(grammar);
        }

        private void BuildGrammar() // call it Window_Loaded()
        {
            GrammarBuilder grammarBuilder = new GrammarBuilder();

            Choices basicCommands = new Choices();
            String[] basicNames = { "close app" };
            basicCommands.Add(basicNames);

            // the same culture as the recognizer (US English)
            grammarBuilder.Culture = kinectRecognizerInfo.Culture;
            grammarBuilder.Append(basicCommands);

            Grammar grammar = new Grammar(grammarBuilder);

            Recognizer.LoadGrammar(grammar);
        }
    }
}
