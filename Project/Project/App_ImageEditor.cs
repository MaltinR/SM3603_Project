using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Threading.Tasks;
using System.Windows;
using System.IO;

namespace Project
{
    public class App_ImageEditor : NonDesktopApplication
    {
        public enum Tool
        {
            Pen,
            Line,
            Picker
        }

        List<BitmapImage> _historyImages;
        int _historyIndex;
        public BitmapImage previewImage;
        public BitmapImage currentImage;
        Rect rectImage;
        public string FilePath { get; private set; }
        public Image_ChannelSlider[] Sliders { get; private set; }
        public LocalControlUnit[] Tools { get; private set; }
        int _channel_R, _channel_G, _channel_B;
        int _brushSize;
        public int Channel_R 
        { 
            get { return _channel_R; } 
            set { 
                _channel_R = value;
                Color = Color.FromRgb((byte)_channel_R, (byte)_channel_G, (byte)_channel_B);
            } 
        }
        public int Channel_G 
        { 
            get { return _channel_G; } 
            set { 
                _channel_G = value;
                Color = Color.FromRgb((byte)_channel_R, (byte)_channel_G, (byte)_channel_B);
            } 
        }
        public int Channel_B 
        { 
            get { return _channel_B; } 
            set { 
                _channel_B = value;
                Color = Color.FromRgb((byte)_channel_R, (byte)_channel_G, (byte)_channel_B);
            } 
        }
        public int BrushSize 
        { 
            get { return _brushSize; }
            set {
                _brushSize = value;
            }
        }
        public Rect rect_Color;
        SolidColorBrush _brushColor;
        double ratioImage;
        double app_Ratio;
        double ratioImageCurOri;
        Color _color;
        public Color Color 
        { 
            get { return _color; } 
            private set
            {
                _color = value;
                _brushColor = new SolidColorBrush(_color);
            }
        }
        public Point Point_PreviousFrame; //Point from previous frame
        public Point Point_ClickFrame;//Point from previous click(down) frame
        public Point Point_CurrentFrame;//Point from previous click(down) frame
        public bool IsPaint;
        System.Drawing.Bitmap bitmap_Current;
        public Tool UsingTool { get; set; }

        public App_ImageEditor(string path)
        {
            Image_Normal = new BitmapImage(new Uri("Images/Icon_Image_Normal.png", UriKind.Relative));
            Image_Selecting = new BitmapImage(new Uri("Images/Icon_Image_Selecting.png", UriKind.Relative));

            MinimumHeight = 250;
            MinimumWidth = 350;
            Height = 250;
            Width = 350;

            //Please enter the needed speech
            Grammars = new Microsoft.Speech.Recognition.Grammar[] { MainWindow.GetGrammar("", new string[] { "undo", "redo", "save" }), MainWindow.GetGrammar("change tool to", new string[] { "pen", "line", "picker" }), MainWindow.GetGrammar("change color to", new string[] { "red", "orange", "yellow", "green", "blue", "purple", "black", "white", "gray" }), MainWindow.GetGrammar("brush size", new string[] { "up", "down", "max", "min", "level one", "level two", "level three", "level four", "level five"}) };

            foreach (Microsoft.Speech.Recognition.Grammar grammar in Grammars)
            {
                MainWindow.mainWindow.BuildNewGrammar(grammar);
            }

            FilePath = path;
            LocalEdgeControl = new LocalEdgeControl(this);
            Rect = new Rect(PosX, PosY, Width, Height);
            Sliders = new Image_ChannelSlider[] { new Image_ChannelSliderR(this), new Image_ChannelSliderG(this), new Image_ChannelSliderB(this), new Image_ChannelSliderSize(this) };
            Tools = new LocalControlUnit[] { new Image_ChannelR(this, Sliders[0]), new Image_ChannelG(this, Sliders[1]), new Image_ChannelB(this, Sliders[2]), new Image_ChannelSize(this, Sliders[3]), new Image_PaintBoard(this), new Image_Tool(this, Tool.Pen), new Image_Tool(this, Tool.Line), new Image_Tool(this, Tool.Picker) };

            BrushSize = 10;
            UsingTool = Tool.Pen;

            currentImage = new BitmapImage(new Uri(path));
            previewImage = new BitmapImage(new Uri(path));
            _historyIndex = 0;
            _historyImages = new List<BitmapImage>();
            _historyImages.Add(currentImage);
            bitmap_Current = new System.Drawing.Bitmap(path);
            Color = Color.FromRgb(0, 0, 0);
            rect_Color = new Rect(PosX, PosY + 170, 30, 30);
            ratioImage = currentImage.Width / (double)currentImage.Height;
            app_Ratio = (Width - 90) / (double)(Height - 100);
            if(app_Ratio > 1)
            {
                rectImage = new Rect(PosX + 40, PosY + 50, (Height - 100) * ratioImage, (Height - 100));
                ratioImageCurOri = (Height - 100) * ratioImage/(double)bitmap_Current.Width;
            }
            else
            {
                rectImage = new Rect(PosX + 40, PosY + 50, (Width - 90), (Width - 90) / ratioImage);
                ratioImageCurOri = (Width - 90) / (double)bitmap_Current.Width;
            }
        }

        public override void Print()
        {
            MainWindow.RenderManager.DrawingContext.DrawRectangle(Brushes.DarkGray, null, Rect);
            MainWindow.RenderManager.DrawingContext.DrawImage(previewImage, rectImage);
            LocalEdgeControl.Print();

            foreach (LocalControlUnit unit in Tools)
            {
                unit.Print();
            }
            foreach (LocalControlUnit unit in Sliders)
            {
                if (MainWindow.dragging == unit)
                {
                    unit.Print();
                    break;
                }
            }
            //Color
            MainWindow.RenderManager.DrawingContext.DrawRectangle(_brushColor, null, rect_Color);
        }

        void Undo()
        {
            if (_historyIndex > 0)
            {
                _historyIndex--;
                previewImage = _historyImages[_historyIndex];
                currentImage = previewImage;
            }
        }

        void Redo()
        {
            if (_historyIndex < _historyImages.Count - 1)
            {
                _historyIndex++;
                previewImage = _historyImages[_historyIndex];
                currentImage = previewImage;
            }
        }

        //Reference: https://stackoverflow.com/questions/35804375/how-do-i-save-a-bitmapimage-from-memory-into-a-file-in-wpf-c
        void Save()
        {
            BitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(currentImage));

            string path = FilePath.Substring(0, FilePath.LastIndexOf('.'));
            //Check if exist
            int attempt = 1;
            while (File.Exists(path + "(" + attempt + ")" + FilePath.Substring(FilePath.LastIndexOf('.'))))
            {
                attempt++;
            }

            using (var fileStream = new System.IO.FileStream(path + "(" + attempt + ")" + FilePath.Substring(FilePath.LastIndexOf('.')), System.IO.FileMode.Create))
            {
                encoder.Save(fileStream);
            }
        }

        public void PaintEnd()
        {
            if (UsingTool != Tool.Picker)
            {
                currentImage = previewImage;

                //Clear the next
                int totalCount = _historyImages.Count;
                for(int i = _historyIndex + 1; i < totalCount;i++)
                {
                    _historyImages.RemoveAt(_historyIndex + 1);
                }

                _historyImages.Add(currentImage);
                _historyIndex++;
            }
            IsPaint = false;
        }

        //Reference: https://stackoverflow.com/questions/63959181/how-do-you-draw-a-line-in-a-pixel-array
        void Paint(int startX, int startY, int endX, int endY)
        {
            System.Diagnostics.Trace.WriteLine(startX + " " + startY + " " + endX + " " + endY);
            System.Drawing.Bitmap bitmap = GetBitmap(currentImage);

            int dirX = endX - startX >0? -1 : endX - startX < 0 ? +1 : 0;
            int dirY = endY - startY >0? -1 : endY - startY < 0 ? +1 : 0;

            bool drew = false;

            System.Drawing.Color color = System.Drawing.Color.FromArgb(255, Color.R, Color.G, Color.B);

            for (int i = startX - BrushSize / 2; i < startX + BrushSize / 2; i++)
            {
                if (i < 0 || i >= bitmap.Width) continue;
                for (int j = startY - BrushSize / 2; j < startY + BrushSize / 2; j++)
                {
                    if (j < 0 || j >= bitmap.Height) continue;
                    bitmap.SetPixel(i, j, color);
                    drew |= true;
                }
            }

            if (dirX < 0)
            {
                for (int i = -BrushSize / 2; i <= BrushSize / 2; i++)
                {
                    drew |= DrawSingleLine(startX + BrushSize / 2, startY + i, endX + BrushSize / 2, endY + i, true, color, ref bitmap);
                }
            }
            else if(dirX > 0)
            {
                for (int i = -BrushSize / 2; i <= BrushSize / 2; i++)
                {
                    drew |= DrawSingleLine(startX - BrushSize / 2, startY + i, endX - BrushSize / 2, endY + i, true, color, ref bitmap);
                }
            }

            if (dirY < 0)
            {
                for (int i = -BrushSize / 2; i <= BrushSize / 2; i++)
                {
                    drew |= DrawSingleLine(startX + i, startY + BrushSize / 2, endX + i, endY + BrushSize / 2, false, color, ref bitmap);
                }
            }
            else if (dirY > 0)
            {
                for (int i = -BrushSize / 2; i <= BrushSize / 2; i++)
                {
                    drew |= DrawSingleLine(startX + i, startY - BrushSize / 2, endX + i, endY - BrushSize / 2, false, color, ref bitmap);
                }
            }

            if (drew)
            {
                bitmap_Current = bitmap;
                //It is preview
                previewImage = GetBitmapImage(bitmap);
                if (UsingTool.Equals(Tool.Pen))
                {
                    currentImage = previewImage;
                }
            }
        }

        static bool DrawSingleLine(int startX, int startY, int endX, int endY, bool isXBased, System.Drawing.Color color, ref System.Drawing.Bitmap bitmap)
        {
            bool drew = false;
            int distX = endX - startX, distY = endY - startY;
            //System.Diagnostics.Trace.WriteLine(distX + " " + distY);

            if (isXBased)
            {
                int leftX = startX < endX ? startX : endX;
                int rightX = startX < endX ? endX : startX;

                for (int x = leftX < 0 ? 0 : leftX; x < bitmap.Width && x < rightX; x++)
                {
                    int y = startY + distY * (x - startX) / distX;
                    if (y < 0 || y >= bitmap.Height) break;

                    bitmap.SetPixel(x, y, color);
                    drew |= true;
                    //Console.WriteLine("LineA: " + x + " , " + y);
                }
            }
            else
            {
                int topY = startY < endY ? startY : endY;
                int botY = startY < endY ? endY : startY;

                for (int y = topY < 0 ? 0 : topY; y < bitmap.Height && y < botY; y++)
                {
                    //int y = startY + distY * (x - startX) / distX;
                    int x = startX + distX * (y -startY) / distY;

                    if (x < 0 || x >= bitmap.Width) break;

                    bitmap.SetPixel(x, y, color);
                    drew |= true;
                }
            }

            return drew;
        }

        //Reference: https://blog.csdn.net/wangshubo1989/article/details/47296339
        public static BitmapImage GetBitmapImage(System.Drawing.Bitmap bitmap)
        {

            BitmapImage bitmapImage = new BitmapImage();
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = ms;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();
            }

            return bitmapImage;
        }

        //Reference: https://stackoverflow.com/questions/6484357/converting-bitmapimage-to-bitmap-and-vice-versa
        public static System.Drawing.Bitmap GetBitmap(BitmapImage bitmapImage)
        {
            using (System.IO.MemoryStream outStream = new System.IO.MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(outStream);

                return bitmap;
                //return new System.Drawing.Bitmap(bitmap);
            }
        }
        public override void Update(bool isFocusing, int listOrder, Point point, Microsoft.Kinect.HandState handState, string command, string gesture)
        {
            base.Update(isFocusing, listOrder, point, handState, command, gesture);

            if (!isFocusing) return;

            int clampedX = point.X < 0 ? 0 : point.X > MainWindow.Drawing_Width ? MainWindow.Drawing_Width : (int)point.X;
            int clampedY = point.Y < 0 ? 0 : point.Y > MainWindow.Drawing_Height ? MainWindow.Drawing_Height : (int)point.Y;

            foreach (LocalControlUnit unit in Tools)
            {
                unit.IsHoveringOrDragging(clampedX, clampedY, listOrder, handState);
            }
            foreach (LocalControlUnit unit in Sliders)
            {
                if (MainWindow.dragging == unit)
                {
                    unit.IsHoveringOrDragging(clampedX, clampedY, listOrder, handState);
                    break;
                }
            }
            //Preview Click to current
            if (IsPaint)
            {
                switch(UsingTool)
                {
                    case Tool.Line:
                        Paint((int)(Point_ClickFrame.X / ratioImageCurOri), (int)(Point_ClickFrame.Y / ratioImageCurOri), (int)(Point_CurrentFrame.X / ratioImageCurOri), (int)(Point_CurrentFrame.Y / ratioImageCurOri));
                        break;
                    case Tool.Pen:
                        Paint((int)(Point_PreviousFrame.X / ratioImageCurOri), (int)(Point_PreviousFrame.Y / ratioImageCurOri), (int)(Point_CurrentFrame.X / ratioImageCurOri), (int)(Point_CurrentFrame.Y / ratioImageCurOri));
                        break;
                    case Tool.Picker:
                        int targetX = (int)(Point_PreviousFrame.X / ratioImageCurOri);
                        int targetY = (int)(Point_PreviousFrame.Y / ratioImageCurOri);
                        if (targetX >= 0 && targetX < bitmap_Current.Width && targetY >= 0 && targetY < bitmap_Current.Height)
                        {
                            System.Drawing.Color pixelColor = bitmap_Current.GetPixel(targetX, targetY);
                            _channel_R = pixelColor.R;
                            _channel_G = pixelColor.G;
                            _channel_B = pixelColor.B;
                            Color = Color.FromRgb(pixelColor.R, pixelColor.G, pixelColor.B);
                        }
                        break;
                }
            }

            GestureControl(gesture);

            VoiceControl(command);
        }

        public override void GestureControl(string gesture)
        {
            switch (gesture)
            {
                case "handninety_left":
                    Undo();
                    MainWindow.mainWindow.DebugLine.Text = "Last Gesture Action: Undo";
                    MainWindow.mainWindow.ResetGestureTimer();
                    break;
                case "handninety_right":
                    Redo();
                    MainWindow.mainWindow.DebugLine.Text = "Last Gesture Action: Redo";
                    MainWindow.mainWindow.ResetGestureTimer();
                    break;
            }
        }
        public override void VoiceControl(string command)
        {
            switch (command)
            {
                case "undo":
                    Undo();
                    break;
                case "redo":
                    Redo();
                    break;
                case "change tool to pen":
                    UsingTool = Tool.Pen;
                    break;
                case "change tool to line":
                    UsingTool = Tool.Line;
                    break;
                case "change tool to picker":
                    UsingTool = Tool.Picker;
                    break;
                case "change color to red":
                    _channel_R = 255;
                    _channel_G = 0;
                    _channel_B = 0;
                    Color = Color.FromRgb((byte)_channel_R, (byte)_channel_G, (byte)_channel_B);
                    break;
                case "change color to orange":
                    _channel_R = 255;
                    _channel_G = 165;
                    _channel_B = 0;
                    Color = Color.FromRgb((byte)_channel_R, (byte)_channel_G, (byte)_channel_B);
                    break;
                case "change color to yellow":
                    _channel_R = 255;
                    _channel_G = 255;
                    _channel_B = 0;
                    Color = Color.FromRgb((byte)_channel_R, (byte)_channel_G, (byte)_channel_B);
                    break;
                case "change color to green":
                    _channel_R = 0;
                    _channel_G = 255;
                    _channel_B = 0;
                    Color = Color.FromRgb((byte)_channel_R, (byte)_channel_G, (byte)_channel_B);
                    break;
                case "change color to blue":
                    _channel_R = 0;
                    _channel_G = 0;
                    _channel_B = 255;
                    Color = Color.FromRgb((byte)_channel_R, (byte)_channel_G, (byte)_channel_B);
                    break;
                case "change color to purple":
                    _channel_R = 128;
                    _channel_G = 0;
                    _channel_B = 128;
                    Color = Color.FromRgb((byte)_channel_R, (byte)_channel_G, (byte)_channel_B);
                    break;
                case "change color to black":
                    _channel_R = 0;
                    _channel_G = 0;
                    _channel_B = 0;
                    Color = Color.FromRgb((byte)_channel_R, (byte)_channel_G, (byte)_channel_B);
                    break;
                case "change color to white":
                    _channel_R = 255;
                    _channel_G = 255;
                    _channel_B = 255;
                    Color = Color.FromRgb((byte)_channel_R, (byte)_channel_G, (byte)_channel_B);
                    break;
                case "change color to gray":
                    _channel_R = 128;
                    _channel_G = 128;
                    _channel_B = 128;
                    Color = Color.FromRgb((byte)_channel_R, (byte)_channel_G, (byte)_channel_B);
                    break;
                case "brush size up":
                    BrushSize += 5;
                    BrushSize = BrushSize > 100 ? 100 : BrushSize;
                    break;
                case "brush size down":
                    BrushSize -= 5;
                    BrushSize = BrushSize < 2 ? 2 : BrushSize;
                    break;
                case "brush size max":
                    BrushSize = 100;
                    break;
                case "brush size min":
                    BrushSize = 2;
                    break;
                case "brush size level one":
                    BrushSize = 10;
                    break;
                case "brush size level two":
                    BrushSize = 30;
                    break;
                case "brush size level three":
                    BrushSize = 50;
                    break;
                case "brush size level four":
                    BrushSize = 70;
                    break;
                case "brush size level five":
                    BrushSize = 90;
                    break;
                case "save":
                    Save();
                    break;
                case "close app":
                    OnClose();
                    MainWindow.Manager.RemoveApp(this);
                    break;
                default:
                    break;
            }
        }

        public override void UpdateRect()
        {
            base.UpdateRect();

            app_Ratio = (Width - 90) / (double)(Height - 100);
            if (app_Ratio > 1)
            {
                rectImage = new Rect(PosX + 40, PosY + 50, (Height - 100) * ratioImage, (Height - 100));
                ratioImageCurOri = (Height - 100) * ratioImage / (double)bitmap_Current.Width;
            }
            else
            {
                rectImage = new Rect(PosX + 40, PosY + 50, (Width - 90), (Width - 90) / ratioImage);
                ratioImageCurOri = (Width - 90) / (double)bitmap_Current.Width;
            }
            rect_Color = new Rect(PosX, PosY + 170, 30, 30);

            foreach (LocalControlUnit unit in Sliders)
            {
                unit.UpdateRect();
            }
            foreach (LocalControlUnit unit in Tools)
            {
                unit.UpdateRect();
            }
        }
    }
}
