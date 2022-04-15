using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Diagnostics;
using System.Windows.Input;
using System.Globalization;

namespace Project
{
    public class App_FileExplorer : NonDesktopApplication
    {
        public string CurrentPath { get; private set; }
        FileInfo[] currentFiles;

        public App_FileExplorer()
        {
            Image_Normal = new BitmapImage(new Uri("Images/Icon_FileExplorer_Normal.png", UriKind.Relative));
            Image_Selecting = new BitmapImage(new Uri("Images/Icon_FileExplorer_Selecting.png", UriKind.Relative));

            PosX = 200;//For testing
            PosY = 100;//For testing
            LocalEdgeControl = new LocalEdgeControl(this);
            Rect = new Rect(PosX, PosY, Width, Height);

            //Starting path: File location
            CurrentPath = Directory.GetCurrentDirectory();

            DirectoryInfo dirInfo = new DirectoryInfo(CurrentPath);

            Trace.WriteLine("==============");
            currentFiles = dirInfo.GetFiles();
            foreach(FileInfo file in currentFiles)
            {
                Trace.WriteLine(file.Name);
            }
        }

        public override void Print()
        {
            MainWindow.DrawingContext.DrawRectangle(Brushes.White, null, Rect);
            //base.Print();
        }

        public override void Update(bool isFocusing, Point point, MouseButtonState mouseState)
        {
            //TODO: detect pos
            base.Update(isFocusing, point, mouseState);

            for(int i = 0;i < currentFiles.Length;i++)
            {
                FormattedText formattedText = new FormattedText(currentFiles[i].Name,
                    CultureInfo.GetCultureInfo("en-us"),
                    FlowDirection.LeftToRight,
                    new Typeface("Verdana"),
                    32,
                    Brushes.Black, 30);
                formattedText.MaxTextWidth = Width - 100;

                MainWindow.DrawingContext.DrawText(formattedText, new Point(PosX + 100, PosY + 100 + i * 18)
                );
                //Trace.WriteLine(file.Name);
                //Depends on the height of the app
                //If exceeds, break
                if (false)
                    break;
            }
        }
    }
}
