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
        public enum Command
        {
            None,
            Cut,
            Copy
        }

        public string CurrentPath { get; private set; }
        public DirectoryInfo CurrentDirectory { get; private set; }
        List<DirectoryInfo> _historyDirectories;
        int _historyIndex;
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
        Command command;
        List<FileSystemInfo> selectedFiles;//Store for copy and cut
        List<FileSystemInfo> selectingFiles;//current selecting files, once need to copy, it will copy to selectedFiles
        BitmapImage blankImage;

        public App_FileExplorer()
        {
            Image_Normal = new BitmapImage(new Uri("Images/Icon_FileExplorer_Normal.png", UriKind.Relative));
            Image_Selecting = new BitmapImage(new Uri("Images/Icon_FileExplorer_Selecting.png", UriKind.Relative));
            blankImage = new BitmapImage(new Uri("Images/Blank.png", UriKind.Relative));

            PosX = 200;//For testing
            PosY = 100;//For testing
            LocalEdgeControl = new LocalEdgeControl(this);
            Rect = new Rect(PosX, PosY, Width, Height);

            _historyDirectories = new List<DirectoryInfo>();
            selectedFiles = new List<FileSystemInfo>();
            selectingFiles = new List<FileSystemInfo>();

            //Please enter the needed speech
            Grammars = new Microsoft.Speech.Recognition.Grammar[] { MainWindow.GetGrammar("", new string[] { "previous page", "next page", "copy", "paste", "cut", "select all", "delete the files" }), MainWindow.GetGrammar("create", new string[] { "text file", "image file"}) };

            foreach (Microsoft.Speech.Recognition.Grammar grammar in Grammars)
            {
                MainWindow.mainWindow.BuildNewGrammar(grammar);
            }

            SlideBar = new File_SlideBar(this);
            subjects = new List<File_Subject>();

            //Starting path: File location
            CurrentPath = Directory.GetCurrentDirectory();
            DirectoryInfo dirInfo = new DirectoryInfo(CurrentPath);
            CurrentDirectory = dirInfo;
            _historyDirectories.Add(dirInfo);
            _historyIndex = 0;
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

                bool isContain = selectingFiles.Contains(CurrentFolders[i]);
                if (checkingIndex == highlighting || isContain)
                {
                    MainWindow.RenderManager.DrawingContext.DrawRectangle(checkingIndex == highlighting?Brushes.Gray:null, isContain?new Pen(Brushes.DarkGray, 2) :null, new Rect(PosX + 20, currentY, Width - 40, RowSize));
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

                    bool isContain = selectingFiles.Contains(CurrentFiles[i]);
                    if (checkingIndex == highlighting || isContain)
                    {
                        MainWindow.RenderManager.DrawingContext.DrawRectangle(checkingIndex == highlighting ? Brushes.Gray : null, isContain ? new Pen(Brushes.DarkGray, 2) : null, new Rect(PosX + 20, currentY, Width - 40, RowSize));
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

        public override void Update(bool isFocusing, int orderList, Point point, MouseButtonState mouseState, string command)
        {
            base.Update(isFocusing, orderList, point, mouseState, command);

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

            Trace.WriteLine("hovering: " + MainWindow.hovering);
            VoiceControl(command);
        }

        public override void VoiceControl(string command)
        {
            switch (command)
            {
                case "previous page":
                    PreviousPage();
                    break;
                case "next page":
                    NextPage();
                    break;
                case "copy":
                    Copy();
                    break;
                case "paste":
                    Paste();
                    break;
                case "cut":
                    Cut();
                    break;
                case "select all":
                    SelectAll();
                    break;
                case "delete the files":
                    Delete();
                    break;
                case "create text file":
                    CreateText();
                    break;
                case "create image file":
                    CreateImage();
                    break;
                case "close app":
                    OnClose();
                    MainWindow.Manager.RemoveApp(this);
                    break;
                default:
                    break;
            }
        }

        void CreateText()
        {
            string fileName = "NewTextFile";
            //Check if exist
            if (File.Exists(CurrentPath + "/" + fileName + ".txt"))
            {
                int attempt = 1;
                while (File.Exists(CurrentPath + "/" + fileName + "(" + attempt + ").txt"))
                {
                    attempt++;
                }
                File.Create(CurrentPath + "/" + fileName + "(" + attempt + ").txt");
            }
            else
            {
                File.Create(CurrentPath + "/" + fileName + ".txt");
            }

            Reload();
        }

        //Reference: https://stackoverflow.com/questions/35804375/how-do-i-save-a-bitmapimage-from-memory-into-a-file-in-wpf-c
        void CreateImage()
        {
            string fileName = "NewImageFile";
            //Check if exist
            if (File.Exists(CurrentPath + "/" + fileName + ".png"))
            {
                int attempt = 1;
                while (File.Exists(CurrentPath + "/" + fileName + "(" + attempt + ").png"))
                {
                    attempt++;
                }
                fileName = CurrentPath + "/" + fileName + "(" + attempt + ").png";
            }
            else
            {
                fileName = CurrentPath + "/" + fileName + ".png";
            }

            BitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(blankImage));

            using (var fileStream = new System.IO.FileStream(fileName, System.IO.FileMode.Create))
            {
                encoder.Save(fileStream);
            }

            Reload();

        }

        void PreviousPage()
        {
            if (_historyIndex > 0)
            {
                //Check if it exists
                //If not, remove from history

                --_historyIndex;
                while (_historyDirectories.Count > 0 && !_historyDirectories[_historyIndex].Exists)
                {
                    _historyDirectories.RemoveAt(_historyIndex);
                    if (_historyIndex > 0)
                        --_historyIndex;
                }

                CurrentDirectory = _historyDirectories[_historyIndex];
                CurrentPath = CurrentDirectory.FullName;
                Reload();
            }
        }

        void NextPage()
        {
            if (_historyIndex < _historyDirectories.Count - 1)
            {
                //Check if it exists
                //If not, remove from history

                ++_historyIndex;
                while (_historyDirectories.Count > _historyIndex && !_historyDirectories[_historyIndex].Exists)
                {
                    _historyDirectories.RemoveAt(_historyIndex);
                    if (_historyIndex < _historyDirectories.Count - 1)
                        ++_historyIndex;
                }

                CurrentDirectory = _historyDirectories[_historyIndex];
                CurrentPath = CurrentDirectory.FullName;
                Reload();
            }
        }

        void SelectAll()
        {
            selectingFiles.Clear();

            selectingFiles.AddRange(CurrentFolders);
            selectingFiles.AddRange(CurrentFiles);
        }

        void Copy()
        {
            //Get the selection
            //Save it to memory
            //Change enum

            selectedFiles = selectingFiles.ToList();
            command = Command.Copy;
        }

        void Paste()
        {
            //Paste the contents of the selectedFiles 
            //If enum is cut, delete from the source

            foreach (FileSystemInfo fi in selectedFiles)
            {
                //Directory
                if (fi is DirectoryInfo)
                {
                    CopyAll((fi as DirectoryInfo), CurrentDirectory);
                }

                //File
                else if (fi is FileInfo)
                {
                    (fi as FileInfo).CopyTo(Path.Combine(CurrentDirectory.FullName, fi.Name), true);
                }
            }

            //Since the source has no target files anymore
            if (command == Command.Cut)
            {
                //Delete the source
                foreach (FileSystemInfo fi in selectedFiles)
                {
                    fi.Delete();
                }
                selectedFiles.Clear();

                command = Command.None;
            }
            Reload();
        }

        void Delete()
        {
            //Delete the source
            foreach (FileSystemInfo fi in selectingFiles)
            {
                fi.Delete();
            }
            selectingFiles.Clear();

            

            //command = Command.None;

            Reload();
        }

        void Cut()
        {
            //Get the selection
            //Save it to memory
            //Change enum

            selectedFiles = selectingFiles.ToList();
            command = Command.Cut;
        }

        public void Open(int index)
        {
            //First index + index

            int sumIndex = firstIndex + index;

            if(sumIndex == 0)//Back if it is not the root
            {
                CurrentPath = Directory.GetParent(CurrentPath).FullName;
                //Trim the rest
                int loopTime = _historyDirectories.Count;
                for (int i = _historyIndex + 1;i < loopTime; i++)
                {
                    _historyDirectories.RemoveAt(_historyIndex + 1);
                }

                selectingFiles.Clear();
                CurrentDirectory = new DirectoryInfo(CurrentPath);
                _historyDirectories.Add(CurrentDirectory);
                _historyIndex++;
                Reload();
            }
            else if(sumIndex <= CurrentFolders.Length)
            {
                CurrentPath = new DirectoryInfo(CurrentPath + @"/" + CurrentFolders[sumIndex - 1].Name).FullName;
                //Trim the rest
                int loopTime = _historyDirectories.Count;
                for (int i = _historyIndex + 1; i < loopTime; i++)
                {
                    _historyDirectories.RemoveAt(_historyIndex + 1);
                }

                selectingFiles.Clear();
                CurrentDirectory = new DirectoryInfo(CurrentPath);
                _historyDirectories.Add(CurrentDirectory);
                _historyIndex++;
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
                //MainWindow.dragging = null;
                //MainWindow.hovering = null;
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

        //Reference: https://stackoverflow.com/questions/58744/copy-the-entire-contents-of-a-directory-in-c-sharp
        static void Copy(string sourceDirectory, string targetDirectory)
        {
            DirectoryInfo diSource = new DirectoryInfo(sourceDirectory);
            DirectoryInfo diTarget = new DirectoryInfo(targetDirectory);

            CopyAll(diSource, diTarget);
        }

        //Reference: https://stackoverflow.com/questions/58744/copy-the-entire-contents-of-a-directory-in-c-sharp
        static void CopyAll(DirectoryInfo source, DirectoryInfo target)
        {
            string sourcePath = source.FullName;
            DirectoryInfo parent_Target = new DirectoryInfo(target.FullName);
            while(parent_Target.Parent != null)
            {
                if (sourcePath == parent_Target.FullName) return;
                parent_Target = parent_Target.Parent;
            }

            Directory.CreateDirectory(target.FullName);

            // Copy each file into the new directory.
            foreach (FileInfo fi in source.GetFiles())
            {
                //Console.WriteLine(@"Copying {0}\{1}", target.FullName, fi.Name);
                fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);
            }

            // Copy each subdirectory using recursion.
            foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
            {
                DirectoryInfo nextTargetSubDir =
                    target.CreateSubdirectory(diSourceSubDir.Name);
                CopyAll(diSourceSubDir, nextTargetSubDir);
            }
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
