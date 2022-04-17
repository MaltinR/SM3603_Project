using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System;
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
        public FileInfo[] CurrentFiles { get; private set; }
        public DirectoryInfo[] CurrentFolders { get; private set; }
        int firstIndex;
        List<File_Subject> subjects;
        public File_SlideBar SlideBar { get; private set; }
        public int RowSize { get; private set; } = 18;
        public int highlighting = -1;
        public int MaxRowCount;
        public int SlideBarHeight;
        public double SlideBarPercent;
        int backFolderCount;

        public App_FileExplorer()
        {
            Image_Normal = new BitmapImage(new Uri("Images/Icon_FileExplorer_Normal.png", UriKind.Relative));
            Image_Selecting = new BitmapImage(new Uri("Images/Icon_FileExplorer_Selecting.png", UriKind.Relative));

            PosX = 200;//For testing
            PosY = 100;//For testing
            LocalEdgeControl = new LocalEdgeControl(this);
            Rect = new Rect(PosX, PosY, Width, Height);

            SlideBar = new File_SlideBar(this);
            subjects = new List<File_Subject>();

            //Starting path: File location
            CurrentPath = Directory.GetCurrentDirectory();
            DirectoryInfo dirInfo = new DirectoryInfo(CurrentPath);
            CurrentFiles = dirInfo.GetFiles();
            CurrentFolders = dirInfo.GetDirectories();

            backFolderCount = dirInfo.Root.FullName != dirInfo.FullName ? 1 : 0;

            int curY = 50;
            //According current possible count
            MaxRowCount = (Height - 50) / RowSize;

            for (int i = 0; i < MaxRowCount; i++)
            {
                subjects.Add(new File_Subject(this, i, curY, RowSize, i <= CurrentFiles.Length + CurrentFolders.Length));
                curY += RowSize;
            }
        }

        public override void Print()
        {
            MainWindow.RenderManager.DrawingContext.DrawRectangle(Brushes.White, null, Rect);

            int checkingIndex = 0;
            int currentY = PosY + 50;

            bool isExceeded = false;

            #region FullPath
            FormattedText pathFormattedText = new FormattedText(CurrentPath,
                CultureInfo.GetCultureInfo("en-us"),
                FlowDirection.LeftToRight,
                new Typeface("Verdana"),
                10,
                Brushes.Black, 30);
            pathFormattedText.MaxTextWidth = Width - 10;
            pathFormattedText.MaxTextHeight = 28;
            pathFormattedText.Trimming = TextTrimming.CharacterEllipsis;

            MainWindow.RenderManager.DrawingContext.DrawText(pathFormattedText, new Point(PosX + 5, PosY + 10)
            );
            #endregion

            if (firstIndex == 0)
            {
                #region ...
                DirectoryInfo dirInfo = new DirectoryInfo(CurrentPath);
                if (dirInfo.Root.FullName != dirInfo.FullName)
                {
                    FormattedText formattedText = new FormattedText("...",
                        CultureInfo.GetCultureInfo("en-us"),
                        FlowDirection.LeftToRight,
                        new Typeface("Verdana"),
                        RowSize - 2,
                        Brushes.Black, 30);
                    formattedText.MaxTextWidth = Width - 40;
                    formattedText.MaxTextHeight = RowSize + 2;
                    formattedText.Trimming = TextTrimming.CharacterEllipsis;

                    if (checkingIndex == highlighting)
                    {
                        MainWindow.RenderManager.DrawingContext.DrawRectangle(Brushes.Gray, null, new Rect(PosX + 20, currentY, Width - 40, RowSize));
                    }
                    MainWindow.RenderManager.DrawingContext.DrawImage(
                        FileExtensionDictionary.GetImage(FileExtensionDictionary.AppEnum.FileExplorer, checkingIndex == highlighting), new Rect(PosX + 20, currentY, RowSize - 2, RowSize - 2)
                    );

                    MainWindow.RenderManager.DrawingContext.DrawText(formattedText, new Point(PosX + 40, currentY)
                    );
                    checkingIndex++;
                    currentY += RowSize;
                }
                #endregion
            }
            for (int i = firstIndex >= backFolderCount ? firstIndex - backFolderCount : 0; i < CurrentFolders.Length && i + backFolderCount < firstIndex + MaxRowCount; i++)
            {
                FormattedText formattedText = new FormattedText(CurrentFolders[i].Name,
                    CultureInfo.GetCultureInfo("en-us"),
                    FlowDirection.LeftToRight,
                    new Typeface("Verdana"),
                    RowSize - 2,
                    Brushes.Black, 30);
                formattedText.MaxTextWidth = Width - 40;
                formattedText.MaxTextHeight = RowSize + 2;
                formattedText.Trimming = TextTrimming.CharacterEllipsis;

                if (checkingIndex == highlighting)
                {
                    MainWindow.RenderManager.DrawingContext.DrawRectangle(Brushes.Gray, null, new Rect(PosX + 20, currentY, Width - 40, RowSize));
                }
                MainWindow.RenderManager.DrawingContext.DrawImage(
                    FileExtensionDictionary.GetImage(FileExtensionDictionary.AppEnum.FileExplorer, checkingIndex == highlighting), new Rect(PosX + 20, currentY, RowSize - 2, RowSize - 2)
                );

                MainWindow.RenderManager.DrawingContext.DrawText(formattedText, new Point(PosX + 40, currentY)
                );
                checkingIndex++;
                currentY += RowSize;

                //Depends on the height of the app
                //If exceeds, break
                if (currentY + RowSize > PosY + Height || currentY + RowSize > MainWindow.Drawing_Height - 50)
                {
                    isExceeded = true;
                    break;
                }
            }
            if (!isExceeded)
            {
                int previousCount = backFolderCount + CurrentFolders.Length;
                for (int i = firstIndex >= previousCount? firstIndex - previousCount:0; i < CurrentFiles.Length && i + previousCount < firstIndex + MaxRowCount; i++)
                {
                    FormattedText formattedText = new FormattedText(CurrentFiles[i].Name,
                        CultureInfo.GetCultureInfo("en-us"),
                        FlowDirection.LeftToRight,
                        new Typeface("Verdana"),
                        RowSize - 2,
                        Brushes.Black, 30);
                    formattedText.MaxTextWidth = Width - 40;
                    formattedText.MaxTextHeight = RowSize + 2;
                    formattedText.Trimming = TextTrimming.CharacterEllipsis;

                    if (checkingIndex == highlighting)
                    {
                        MainWindow.RenderManager.DrawingContext.DrawRectangle(Brushes.Gray, null, new Rect(PosX + 20, currentY, Width - 40, RowSize));
                    }

                    FileExtensionDictionary.AppEnum appEnum = FileExtensionDictionary.GetEnum(CurrentFiles[i].Name.Substring(CurrentFiles[i].Name.LastIndexOf('.') + 1));

                    MainWindow.RenderManager.DrawingContext.DrawImage(
                        FileExtensionDictionary.GetImage(appEnum, checkingIndex == highlighting), new Rect(PosX + 20, currentY, RowSize - 2, RowSize - 2)
                    );

                    MainWindow.RenderManager.DrawingContext.DrawText(formattedText, new Point(PosX + 40, currentY)
                    );
                    checkingIndex++;
                    currentY += RowSize;

                    //Depends on the height of the app
                    //If exceeds, break
                    if (currentY + RowSize > PosY + Height || currentY + RowSize > MainWindow.Drawing_Height - 50)
                        break;
                }
            }

            SlideBar.Print();
            LocalEdgeControl.Print();
        }

        public override void Update(bool isFocusing, int orderList, Point point, MouseButtonState mouseState)
        {
            base.Update(isFocusing, orderList, point, mouseState);

            highlighting = -1;

            if (!isFocusing) return;

            int clampedX = point.X < 0 ? 0 : point.X > MainWindow.Drawing_Width ? MainWindow.Drawing_Width : (int)point.X;
            int clampedY = point.Y < 0 ? 0 : point.Y > MainWindow.Drawing_Height ? MainWindow.Drawing_Height : (int)point.Y;
            foreach (File_Subject subject in subjects)
            {
                subject.IsHoveringOrDragging(clampedX, clampedY, orderList, mouseState);
            }
            if (MainWindow.dragging == SlideBar)
                SlideBar.IsHoveringOrDragging(clampedX, clampedY, 0, Mouse.LeftButton);
        }

        public void Open(int index)
        {
            //First index + index

            int sumIndex = firstIndex + index;

            if(sumIndex == 0)//Back if it is not the root
            {
                CurrentPath = Directory.GetParent(CurrentPath).FullName;
                Reload();
            }
            else if(sumIndex <= CurrentFolders.Length)
            {
                CurrentPath = new DirectoryInfo(CurrentPath + @"/" + CurrentFolders[sumIndex - 1].Name).FullName;
                Reload();
            }
            else//File
            {
                string fileName = CurrentFiles[sumIndex - 1 - CurrentFolders.Length].Name;

                FileExtensionDictionary.AppEnum appEnum = FileExtensionDictionary.GetEnum(fileName.Substring(fileName.LastIndexOf('.') + 1));

                switch(appEnum)
                {
                    case FileExtensionDictionary.AppEnum.ImageEditor:
                        //TODO: ImageEditor
                        MainWindow.Manager.AddApp(new App_ImageEditor(CurrentPath + @"/" + fileName));
                        break;
                    case FileExtensionDictionary.AppEnum.Other:
                        //Nothing
                        break;
                    case FileExtensionDictionary.AppEnum.TextEditor:
                        //TODO: TextEditor
                        MainWindow.Manager.AddApp(new App_TextEditor(CurrentPath + @"/" + fileName));
                        break;
                    case FileExtensionDictionary.AppEnum.VideoPlayer:
                        MainWindow.Manager.AddApp(new App_VideoPlayer(CurrentPath + @"/" + fileName));
                        //MainWindow.Manager.AddApp(new App_FileExplorer());
                        break;
                }
            }
        }

        void Reload()
        {
            DirectoryInfo dirInfo = new DirectoryInfo(CurrentPath);
            backFolderCount = dirInfo.Root.FullName != dirInfo.FullName ? 1 : 0;
            CurrentFiles = dirInfo.GetFiles();
            CurrentFolders = dirInfo.GetDirectories();

            Trace.WriteLine("MaxRowCount: " + MaxRowCount);

            //Root
            //subjects[0].SetHoveringTime(dirInfo.Root.FullName != dirInfo.FullName);
            //Others
            for (int i = 1;i < subjects.Count;i++)
            {
                subjects[i].SetHoveringTime(i <= MaxRowCount && i < backFolderCount + CurrentFiles.Length + CurrentFolders.Length);
            }

            firstIndex = 0;
        }

        public void Scroll()
        {
            //The change when the user scrolls

            //TODO: Change firstIndex
            DirectoryInfo dirInfo = new DirectoryInfo(CurrentPath);
            int sumInDir = CurrentFiles.Length + CurrentFolders.Length + (dirInfo.Root.FullName != dirInfo.FullName ? 1:0);
            firstIndex = (int)(SlideBarPercent * (sumInDir - MaxRowCount));
        }

        public override void UpdateRect()
        {
            base.UpdateRect();
            foreach (File_Subject subject in subjects)
            {
                subject.UpdateRect();
            }

            MaxRowCount = (Height - 50) / RowSize;
            //TODO add subject/ disable hovering function
        }
    }
}
