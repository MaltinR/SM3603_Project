using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Threading.Tasks;
using System.Windows;

namespace Project
{
    public class App_ImageEditor : NonDesktopApplication
    {
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

        public App_ImageEditor(string path)
        {
            Image_Normal = new BitmapImage(new Uri("Images/Icon_Image_Normal.png", UriKind.Relative));
            Image_Selecting = new BitmapImage(new Uri("Images/Icon_Image_Selecting.png", UriKind.Relative));

            FilePath = path;
            LocalEdgeControl = new LocalEdgeControl(this);
            Rect = new Rect(PosX, PosY, Width, Height);
            rectImage = new Rect(PosX, PosY, Width, Height);//To be set to fixed ratio
            Sliders = new Image_ChannelSlider[] { new Image_ChannelSliderR(this), new Image_ChannelSliderG(this), new Image_ChannelSliderB(this), new Image_ChannelSliderSize(this) };
            Tools = new LocalControlUnit[] { new Image_ChannelR(this, Sliders[0]), new Image_ChannelG(this, Sliders[1]) , new Image_ChannelB(this, Sliders[2]), new Image_ChannelSize(this, Sliders[3]) };

            BrushSize = 10;

            currentImage = new BitmapImage(new Uri(path));
            Color = Color.FromRgb(0, 0, 0);
            Paint(50, 100, 150, 150);
            rect_Color = new Rect(PosX, PosY + 170, 30, 30);
        }

        public override void Print()
        {
            MainWindow.RenderManager.DrawingContext.DrawRectangle(Brushes.Black, null, Rect);
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

        //Test first
        //Reference: https://stackoverflow.com/questions/63959181/how-do-you-draw-a-line-in-a-pixel-array
        void Paint(int startX, int startY, int endX, int endY)
        {
            System.Diagnostics.Trace.WriteLine(startX + " " + startY + " " + endX + " " + endY);
            System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(currentImage.UriSource.AbsolutePath);

            int dirX = endX - startX >0? -1 : endX - startX < 0 ? +1 : 0;
            int dirY = endY - startY >0? -1 : endY - startY < 0 ? +1 : 0;

            System.Drawing.Color color = System.Drawing.Color.FromArgb(255, Color.R, Color.G, Color.B);

            for (int i = startX - BrushSize / 2; i < startX + BrushSize / 2; i++)
            {
                for (int j = startY - BrushSize / 2; j < startY + BrushSize / 2; j++)
                {
                    bitmap.SetPixel(i, j, color);
                }
            }

            if (dirX < 0)
            {
                for (int i = -BrushSize / 2; i <= BrushSize / 2; i++)
                {
                    DrawSingleLine(startX + BrushSize / 2, startY + i, endX + BrushSize / 2, endY + i, color, ref bitmap);
                }
            }
            else if(dirX > 0)
            {
                for (int i = -BrushSize / 2; i <= BrushSize / 2; i++)
                {
                    DrawSingleLine(startX - BrushSize / 2, startY + i, endX - BrushSize / 2, endY + i, color, ref bitmap);
                }
            }

            if (dirY < 0)
            {
                for (int i = -BrushSize / 2; i <= BrushSize / 2; i++)
                {
                    DrawSingleLine(startX + i, startY + BrushSize / 2, endX + i, endY + BrushSize / 2, color, ref bitmap);
                }
            }
            else if (dirY > 0)
            {
                for (int i = -BrushSize / 2; i <= BrushSize / 2; i++)
                {
                    DrawSingleLine(startX + i, startY - BrushSize / 2, endX + i, endY - BrushSize / 2, color, ref bitmap);
                }
            }

            previewImage = GetBitmapImage(bitmap);
        }

        static void DrawSingleLine(int startX, int startY, int endX, int endY, System.Drawing.Color color, ref System.Drawing.Bitmap bitmap)
        {
            int distX = endX - startX, distY = endY - startY;
            System.Diagnostics.Trace.WriteLine(distX + " " + distY);

            for (int x = startX<0?0: startX; x < bitmap.Width && x < endX; x++)
            {
                int y = startY + distY * (x - startX) / distX;
                if (y < 0 || y >= bitmap.Height) break;

                bitmap.SetPixel(x, y, color);
            }
        }

        //Reference: https://blog.csdn.net/wangshubo1989/article/details/47296339
        public static BitmapImage GetBitmapImage(System.Drawing.Bitmap bitmap)
        {

            BitmapImage bitmapImage = new BitmapImage();
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                System.Diagnostics.Trace.WriteLine("bitmap: " + (ms == null));

                bitmap.Save(ms, bitmap.RawFormat);
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = ms;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();
            }

            return bitmapImage;
        }

        public override void Update(bool isFocusing, int listOrder, Point point, MouseButtonState mouseState)
        {
            base.Update(isFocusing, listOrder, point, mouseState);

            if (!isFocusing) return;

            int clampedX = point.X < 0 ? 0 : point.X > MainWindow.Drawing_Width ? MainWindow.Drawing_Width : (int)point.X;
            int clampedY = point.Y < 0 ? 0 : point.Y > MainWindow.Drawing_Height ? MainWindow.Drawing_Height : (int)point.Y;

            foreach (LocalControlUnit unit in Tools)
            {
                unit.IsHoveringOrDragging(clampedX, clampedY, listOrder, mouseState);
            }
            foreach (LocalControlUnit unit in Sliders)
            {
                if (MainWindow.dragging == unit)
                {
                    unit.IsHoveringOrDragging(clampedX, clampedY, listOrder, mouseState);
                    break;
                }
            }
        }
        public override void UpdateRect()
        {
            base.UpdateRect();

            rectImage = new Rect(PosX, PosY, Width, Height);//To be set to fixed ratio
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
