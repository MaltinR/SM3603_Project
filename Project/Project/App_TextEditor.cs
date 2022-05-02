using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Timers;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Input;
using System.Windows.Media;
using System.Diagnostics;
using System.Globalization;

namespace Project
{
    public class App_TextEditor : NonDesktopApplication
    {
        TextEditor_Functions[] textEditor_Functions;

        public string FilePath { get; private set; }
        public App_TextEditor(string path)
        {
            Timer textEditCursorDisplayTimer = new Timer();
            textEditCursorDisplayTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            textEditCursorDisplayTimer.Interval = 500;
            textEditCursorDisplayTimer.Enabled = true;

            Image_Normal = new BitmapImage(new Uri("Images/Icon_TextEditor_Normal.png", UriKind.Relative));
            Image_Selecting = new BitmapImage(new Uri("Images/Icon_TextEditor_Selecting.png", UriKind.Relative));

            //Please enter the needed speech
            Grammars = new Microsoft.Speech.Recognition.Grammar[] 
            {
                MainWindow.GetGrammar("", new string[] 
                {
                    "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z",
                    "0", "1", "2", "3", "4", "5", "6", "7", "8", "9",
                    "comma", "full stop", "colon", "semicolon", "slash", "backslash", "apostrophe",
                    "open round bracket", "open square bracket", "open curly bracket", "close round bracket", "close square bracket", "close curly bracket",
                    "open guillemet", "close guillemet", "question mark", "exclamation mark", "hyphen", "equal", "plus", "asterisk", "percent sign", "ampersand", "caret", "at",
                    "number sign", "dollar sign", "vertical bar", "Enter", "Shift", "Capslock", "Insert", "Backspace", "Delete", "Left", "Right", "Up", "Down", "Space", "Save"
                })
            };

            foreach (Microsoft.Speech.Recognition.Grammar grammar in Grammars)
            {
                MainWindow.mainWindow.BuildNewGrammar(grammar);
            }

            FilePath = path;

            text = File.ReadAllLines(path);
            texts = new List<List<char>>();
            InitTexts();

            MinimumHeight = 5 + texts.Count * 20 + 300;
            MinimumWidth = 1100;
            Height = MinimumHeight;
            Width = MinimumWidth;
            PosX = 5;//For testing
            PosY = 5;//For testing

            curPos = 0;
            curLine = 0;
            isShift = false;
            isCapslock = false;
            isCaps = false;

            textEditor_Functions = new TextEditor_Functions[] { new TextEditor_Functions(this, "Save"),
                new TextEditor_Functions(this, "1"), new TextEditor_Functions(this, "2"), new TextEditor_Functions(this, "3"), new TextEditor_Functions(this, "4"),
                new TextEditor_Functions(this, "5"), new TextEditor_Functions(this, "6"), new TextEditor_Functions(this, "7"), new TextEditor_Functions(this, "8"),
                new TextEditor_Functions(this, "9"), new TextEditor_Functions(this, "0"), new TextEditor_Functions(this, "-"), new TextEditor_Functions(this, "="),
                new TextEditor_Functions(this, "Backspace"), new TextEditor_Functions(this, "Insert"), new TextEditor_Functions(this, "q"),
                new TextEditor_Functions(this, "w"), new TextEditor_Functions(this, "e"), new TextEditor_Functions(this, "r"), new TextEditor_Functions(this, "t"),
                new TextEditor_Functions(this, "y"), new TextEditor_Functions(this, "u"), new TextEditor_Functions(this, "i"), new TextEditor_Functions(this, "o"),
                new TextEditor_Functions(this, "p"), new TextEditor_Functions(this, "["), new TextEditor_Functions(this, "]"), new TextEditor_Functions(this, "\\"),
                new TextEditor_Functions(this, "Delete"), new TextEditor_Functions(this, "Capslock"), new TextEditor_Functions(this, "a"),
                new TextEditor_Functions(this, "s"), new TextEditor_Functions(this, "d"), new TextEditor_Functions(this, "f"), new TextEditor_Functions(this, "g"),
                new TextEditor_Functions(this, "h"), new TextEditor_Functions(this, "j"), new TextEditor_Functions(this, "k"), new TextEditor_Functions(this, "l"),
                new TextEditor_Functions(this, ";"), new TextEditor_Functions(this, "'"), new TextEditor_Functions(this, "Enter"), new TextEditor_Functions(this, "Left Shift"),
                new TextEditor_Functions(this, "z"), new TextEditor_Functions(this, "x"), new TextEditor_Functions(this, "c"), new TextEditor_Functions(this, "v"),
                new TextEditor_Functions(this, "b"), new TextEditor_Functions(this, "n"), new TextEditor_Functions(this, "m"), new TextEditor_Functions(this, ","),
                new TextEditor_Functions(this, "."), new TextEditor_Functions(this, "/"), new TextEditor_Functions(this, "Right Shift"), new TextEditor_Functions(this, "Up"),
                new TextEditor_Functions(this, "Space"), new TextEditor_Functions(this, "Left"), new TextEditor_Functions(this, "Down"), new TextEditor_Functions(this, "Right")
            };

            LocalEdgeControl = new LocalEdgeControl(this);
            Rect = new Rect(PosX, PosY, Width, Height);
        }

        public void InitTexts()
        {
            for (int i = 0; i < text.Length; i++)
            {
                List<char> tempTextList = new List<char>(); 
                for (int j = 0; j < text[i].Length; j++)
                {
                    tempTextList.Add(text[i][j]);
                }
                texts.Add(tempTextList);
            }
        }

        public override void Update(bool isFocusing, int listOrder, Point point, Microsoft.Kinect.HandState handState, string command, string gesture)
        {
            base.Update(isFocusing, listOrder, point, handState, command, gesture);

            if (!isFocusing) return;

            int clampedX = point.X < 0 ? 0 : point.X > MainWindow.Drawing_Width ? MainWindow.Drawing_Width : (int)point.X;
            int clampedY = point.Y < 0 ? 0 : point.Y > MainWindow.Drawing_Height ? MainWindow.Drawing_Height : (int)point.Y;

            foreach (TextEditor_Functions unit in textEditor_Functions)
            {
                unit.IsHoveringOrDragging(clampedX, clampedY, listOrder, handState);
            }

            MinimumHeight = 5 + texts.Count * 20 + 300;
            if (Height < MinimumHeight)
            {
                Height = MinimumHeight;
                Rect = new Rect(PosX, PosY, Width, Height);
            }

            GestureControl(gesture);
            //To be implemented
        }
        
        public void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            displayTextEditCursor = !displayTextEditCursor;
        }
                     
        public override void Print()
        {
            //base.Print();
            MainWindow.RenderManager.DrawingContext.DrawRectangle(Brushes.Black, null, Rect);

            foreach (TextEditor_Functions unit in textEditor_Functions)
            {
                if (unit.functionKey == "Shift") {
                    if (isShift)
                        unit.color = Brushes.LightGray;
                    else unit.color = Brushes.Gray;
                }
                else if (unit.functionKey == "Insert") {
                    if (isInsert)
                        unit.color = Brushes.LightGray;
                    else unit.color = Brushes.Gray;
                }
                else if (unit.functionKey == "CapsLk") {
                    if (isCapslock)
                        unit.color = Brushes.LightGray;
                    else unit.color = Brushes.Gray;
                }
                unit.Print(Height, Width);
            }
            for (int i = 0; i < texts.Count; i++) {
                for (int j = 0; j < texts[i].Count; j++)
                {
                    FormattedText pathFormattedText = new FormattedText(texts[i][j].ToString(),
                        CultureInfo.GetCultureInfo("en-us"),
                        FlowDirection.LeftToRight,
                        new Typeface("Verdana"),
                        20,
                        Brushes.White, 30);
                    pathFormattedText.Trimming = TextTrimming.CharacterEllipsis;
                    pathFormattedText.TextAlignment = TextAlignment.Center;
                    MainWindow.RenderManager.DrawingContext.DrawText(pathFormattedText, new Point(PosX + 18 + 16 * j, PosY + 10 + 20 * i));
                }
            }

            if (displayTextEditCursor)
                MainWindow.RenderManager.DrawingContext.DrawLine(new Pen(Brushes.White, 2), new Point(PosX + 10 + 16 * curPos, PosY + 12 + 20 * curLine), new Point(PosX + 10 + 16 * curPos, PosY + 32 + 20 * curLine));

            LocalEdgeControl.Print();
        }

        public override void UpdateRect()
        {
            base.UpdateRect();

            foreach (TextEditor_Functions unit in textEditor_Functions)
            {
                unit.UpdateRect();
            }
        }
        public string[] text;
        public List<List<char>> texts;
        public int curPos;
        public int curLine;
        public bool isShift, isCapslock, isCaps, isInsert;
        public bool displayTextEditCursor;
        public Timer displayTextEditCursorTimer;

        public string[] GenerateStringArray()
        {
            string[] output = new string[texts.Count];

            for (int i = 0; i < texts.Count; i++)
            {
                string tempString = "";
                for (int j = 0; j < texts[i].Count; j++)
                {
                    tempString += texts[i][j];
                }
                output[i] = tempString;
            }

            return output;
        }

        public override void VoiceControl(string command)
        {
            switch (command)
            {
                case "a":
                    texts[curLine].Insert(curPos, isCaps ? 'A' : 'a');
                    curPos++;
                    if (isInsert)
                        texts[curLine].RemoveAt(curPos);
                    isCaps = isCapslock;
                    isShift = false;
                    break;
                case "b":
                    texts[curLine].Insert(curPos, isCaps ? 'B' : 'b');
                    curPos++;
                    if (isInsert)
                        texts[curLine].RemoveAt(curPos);
                    isCaps = isCapslock;
                    isShift = false;
                    break;
                case "c":
                    texts[curLine].Insert(curPos, isCaps ? 'C' : 'c');
                    curPos++;
                    if (isInsert)
                        texts[curLine].RemoveAt(curPos);
                    isCaps = isCapslock;
                    isShift = false;
                    break;
                case "d":
                    texts[curLine].Insert(curPos, isCaps ? 'D' : 'd');
                    curPos++;
                    if (isInsert)
                        texts[curLine].RemoveAt(curPos);
                    isCaps = isCapslock;
                    isShift = false;
                    break;
                case "e":
                    texts[curLine].Insert(curPos, isCaps ? 'E' : 'e');
                    curPos++;
                    if (isInsert)
                        texts[curLine].RemoveAt(curPos);
                    isCaps = isCapslock;
                    isShift = false;
                    break;
                case "f":
                    texts[curLine].Insert(curPos, isCaps ? 'F' : 'f');
                    curPos++;
                    if (isInsert)
                        texts[curLine].RemoveAt(curPos);
                    isCaps = isCapslock;
                    isShift = false;
                    break;
                case "g":
                    texts[curLine].Insert(curPos, isCaps ? 'G' : 'g');
                    curPos++;
                    if (isInsert)
                        texts[curLine].RemoveAt(curPos);
                    isCaps = isCapslock;
                    isShift = false;
                    break;
                case "h":
                    texts[curLine].Insert(curPos, isCaps ? 'H' : 'h');
                    curPos++;
                    if (isInsert)
                        texts[curLine].RemoveAt(curPos);
                    isCaps = isCapslock;
                    isShift = false;
                    break;
                case "i":
                    texts[curLine].Insert(curPos, isCaps ? 'I' : 'i');
                    curPos++;
                    if (isInsert)
                        texts[curLine].RemoveAt(curPos);
                    isCaps = isCapslock;
                    isShift = false;
                    break;
                case "j":
                    texts[curLine].Insert(curPos, isCaps ? 'J' : 'j');
                    curPos++;
                    if (isInsert)
                        texts[curLine].RemoveAt(curPos);
                    isCaps = isCapslock;
                    isShift = false;
                    break;
                case "k":
                    texts[curLine].Insert(curPos, isCaps ? 'K' : 'k');
                    curPos++;
                    if (isInsert)
                        texts[curLine].RemoveAt(curPos);
                    isCaps = isCapslock;
                    isShift = false;
                    break;
                case "l":
                    texts[curLine].Insert(curPos, isCaps ? 'L' : 'l');
                    curPos++;
                    if (isInsert)
                        texts[curLine].RemoveAt(curPos);
                    isCaps = isCapslock;
                    isShift = false;
                    break;
                case "m":
                    texts[curLine].Insert(curPos, isCaps ? 'M' : 'm');
                    curPos++;
                    if (isInsert)
                        texts[curLine].RemoveAt(curPos);
                    isCaps = isCapslock;
                    isShift = false;
                    break;
                case "n":
                    texts[curLine].Insert(curPos, isCaps ? 'N' : 'n');
                    curPos++;
                    if (isInsert)
                        texts[curLine].RemoveAt(curPos);
                    isCaps = isCapslock;
                    isShift = false;
                    break;
                case "o":
                    texts[curLine].Insert(curPos, isCaps ? 'O' : 'o');
                    curPos++;
                    if (isInsert)
                        texts[curLine].RemoveAt(curPos);
                    isCaps = isCapslock;
                    isShift = false;
                    break;
                case "p":
                    texts[curLine].Insert(curPos, isCaps ? 'P' : 'p');
                    curPos++;
                    if (isInsert)
                        texts[curLine].RemoveAt(curPos);
                    isCaps = isCapslock;
                    isShift = false;
                    break;
                case "q":
                    texts[curLine].Insert(curPos, isCaps ? 'Q' : 'q');
                    curPos++;
                    if (isInsert)
                        texts[curLine].RemoveAt(curPos);
                    isCaps = isCapslock;
                    isShift = false;
                    break;
                case "r":
                    texts[curLine].Insert(curPos, isCaps ? 'R' : 'r');
                    curPos++;
                    if (isInsert)
                        texts[curLine].RemoveAt(curPos);
                    isCaps = isCapslock;
                    isShift = false;
                    break;
                case "s":
                    texts[curLine].Insert(curPos, isCaps ? 'S' : 's');
                    curPos++;
                    if (isInsert)
                        texts[curLine].RemoveAt(curPos);
                    isCaps = isCapslock;
                    isShift = false;
                    break;
                case "t":
                    texts[curLine].Insert(curPos, isCaps ? 'T' : 't');
                    curPos++;
                    if (isInsert)
                        texts[curLine].RemoveAt(curPos);
                    isCaps = isCapslock;
                    isShift = false;
                    break;
                case "u":
                    texts[curLine].Insert(curPos, isCaps ? 'U' : 'u');
                    curPos++;
                    if (isInsert)
                        texts[curLine].RemoveAt(curPos);
                    isCaps = isCapslock;
                    isShift = false;
                    break;
                case "v":
                    texts[curLine].Insert(curPos, isCaps ? 'V' : 'v');
                    curPos++;
                    if (isInsert)
                        texts[curLine].RemoveAt(curPos);
                    isCaps = isCapslock;
                    isShift = false;
                    break;
                case "w":
                    texts[curLine].Insert(curPos, isCaps ? 'W' : 'w');
                    curPos++;
                    if (isInsert)
                        texts[curLine].RemoveAt(curPos);
                    isCaps = isCapslock;
                    isShift = false;
                    break;
                case "x":
                    texts[curLine].Insert(curPos, isCaps ? 'X' : 'x');
                    curPos++;
                    if (isInsert)
                        texts[curLine].RemoveAt(curPos);
                    isCaps = isCapslock;
                    isShift = false;
                    break;
                case "y":
                    texts[curLine].Insert(curPos, isCaps ? 'Y' : 'y');
                    curPos++;
                    if (isInsert)
                        texts[curLine].RemoveAt(curPos);
                    isCaps = isCapslock;
                    isShift = false;
                    break;
                case "z":
                    texts[curLine].Insert(curPos, isCaps ? 'Z' : 'z');
                    curPos++;
                    if (isInsert)
                        texts[curLine].RemoveAt(curPos);
                    isCaps = isCapslock;
                    isShift = false;
                    break;
                case "0":
                    texts[curLine].Insert(curPos, '0');
                    curPos++;
                    if (isInsert)
                        texts[curLine].RemoveAt(curPos);
                    isCaps = isCapslock;
                    isShift = false;
                    break;
                case "1":
                    texts[curLine].Insert(curPos, '1');
                    curPos++;
                    if (isInsert)
                        texts[curLine].RemoveAt(curPos);
                    isCaps = isCapslock;
                    isShift = false;
                    break;
                case "2":
                    texts[curLine].Insert(curPos, '2');
                    curPos++;
                    if (isInsert)
                        texts[curLine].RemoveAt(curPos);
                    isCaps = isCapslock;
                    isShift = false;
                    break;
                case "3":
                    texts[curLine].Insert(curPos, '3');
                    curPos++;
                    if (isInsert)
                        texts[curLine].RemoveAt(curPos);
                    isCaps = isCapslock;
                    isShift = false;
                    break;
                case "4":
                    texts[curLine].Insert(curPos, '4');
                    curPos++;
                    if (isInsert)
                        texts[curLine].RemoveAt(curPos);
                    isCaps = isCapslock;
                    isShift = false;
                    break;
                case "5":
                    texts[curLine].Insert(curPos, '5');
                    curPos++;
                    if (isInsert)
                        texts[curLine].RemoveAt(curPos);
                    isCaps = isCapslock;
                    isShift = false;
                    break;
                case "6":
                    texts[curLine].Insert(curPos, '6');
                    curPos++;
                    if (isInsert)
                        texts[curLine].RemoveAt(curPos);
                    isCaps = isCapslock;
                    isShift = false;
                    break;
                case "7":
                    texts[curLine].Insert(curPos, '7');
                    curPos++;
                    if (isInsert)
                        texts[curLine].RemoveAt(curPos);
                    isCaps = isCapslock;
                    isShift = false;
                    break;
                case "8":
                    texts[curLine].Insert(curPos, '8');
                    curPos++;
                    if (isInsert)
                        texts[curLine].RemoveAt(curPos);
                    isCaps = isCapslock;
                    isShift = false;
                    break;
                case "9":
                    texts[curLine].Insert(curPos, '9');
                    curPos++;
                    if (isInsert)
                        texts[curLine].RemoveAt(curPos);
                    isCaps = isCapslock;
                    isShift = false;
                    break;
                case "comma":
                    texts[curLine].Insert(curPos, ',');
                    curPos++;
                    if (isInsert)
                        texts[curLine].RemoveAt(curPos);
                    isCaps = isCapslock;
                    isShift = false;
                    break;
                case "full stop":
                    texts[curLine].Insert(curPos, '.');
                    curPos++;
                    if (isInsert)
                        texts[curLine].RemoveAt(curPos);
                    isCaps = isCapslock;
                    isShift = false;
                    break;
                case "colon":
                    texts[curLine].Insert(curPos, ':');
                    curPos++;
                    if (isInsert)
                        texts[curLine].RemoveAt(curPos);
                    isCaps = isCapslock;
                    isShift = false;
                    break;
                case "semicolon":
                    texts[curLine].Insert(curPos, ';');
                    curPos++;
                    if (isInsert)
                        texts[curLine].RemoveAt(curPos);
                    isCaps = isCapslock;
                    isShift = false;
                    break;
                case "slash":
                    texts[curLine].Insert(curPos, '/');
                    curPos++;
                    if (isInsert)
                        texts[curLine].RemoveAt(curPos);
                    isCaps = isCapslock;
                    isShift = false;
                    break;
                case "backslash":
                    texts[curLine].Insert(curPos, '\\');
                    curPos++;
                    if (isInsert)
                        texts[curLine].RemoveAt(curPos);
                    isCaps = isCapslock;
                    isShift = false;
                    break;
                case "apostrophe":
                    texts[curLine].Insert(curPos, '\'');
                    curPos++;
                    if (isInsert)
                        texts[curLine].RemoveAt(curPos);
                    isCaps = isCapslock;
                    isShift = false;
                    break;
                case "open round bracket":
                    texts[curLine].Insert(curPos, '(');
                    curPos++;
                    if (isInsert)
                        texts[curLine].RemoveAt(curPos);
                    isCaps = isCapslock;
                    isShift = false;
                    break;
                case "open square bracket":
                    texts[curLine].Insert(curPos, '[');
                    curPos++;
                    if (isInsert)
                        texts[curLine].RemoveAt(curPos);
                    isCaps = isCapslock;
                    isShift = false;
                    break;
                case "open curly bracket":
                    texts[curLine].Insert(curPos, '{');
                    curPos++;
                    if (isInsert)
                        texts[curLine].RemoveAt(curPos);
                    isCaps = isCapslock;
                    isShift = false;
                    break;
                case "close round bracket":
                    texts[curLine].Insert(curPos, ')');
                    curPos++;
                    if (isInsert)
                        texts[curLine].RemoveAt(curPos);
                    isCaps = isCapslock;
                    isShift = false;
                    break;
                case "close square bracket":
                    texts[curLine].Insert(curPos, ']');
                    curPos++;
                    if (isInsert)
                        texts[curLine].RemoveAt(curPos);
                    isCaps = isCapslock;
                    isShift = false;
                    break;
                case "close curly bracket":
                    texts[curLine].Insert(curPos, '}');
                    curPos++;
                    if (isInsert)
                        texts[curLine].RemoveAt(curPos);
                    isCaps = isCapslock;
                    isShift = false;
                    break;
                case "open guillemet":
                    texts[curLine].Insert(curPos, '<');
                    curPos++;
                    if (isInsert)
                        texts[curLine].RemoveAt(curPos);
                    isCaps = isCapslock;
                    isShift = false;
                    break;
                case "close guillemet":
                    texts[curLine].Insert(curPos, '>');
                    curPos++;
                    if (isInsert)
                        texts[curLine].RemoveAt(curPos);
                    isCaps = isCapslock;
                    isShift = false;
                    break;
                case "question mark":
                    texts[curLine].Insert(curPos, '?');
                    curPos++;
                    if (isInsert)
                        texts[curLine].RemoveAt(curPos);
                    isCaps = isCapslock;
                    isShift = false;
                    break;
                case "exclamation mark":
                    texts[curLine].Insert(curPos, '!');
                    curPos++;
                    if (isInsert)
                        texts[curLine].RemoveAt(curPos);
                    isCaps = isCapslock;
                    isShift = false;
                    break;
                case "hyphen":
                    texts[curLine].Insert(curPos, '-');
                    curPos++;
                    if (isInsert)
                        texts[curLine].RemoveAt(curPos);
                    isCaps = isCapslock;
                    isShift = false;
                    break;
                case "equal":
                    texts[curLine].Insert(curPos, '=');
                    curPos++;
                    if (isInsert)
                        texts[curLine].RemoveAt(curPos);
                    isCaps = isCapslock;
                    isShift = false;
                    break;
                case "plus":
                    texts[curLine].Insert(curPos, '+');
                    curPos++;
                    if (isInsert)
                        texts[curLine].RemoveAt(curPos);
                    isCaps = isCapslock;
                    isShift = false;
                    break;
                case "asterisk":
                    texts[curLine].Insert(curPos, '*');
                    curPos++;
                    if (isInsert)
                        texts[curLine].RemoveAt(curPos);
                    isCaps = isCapslock;
                    isShift = false;
                    break;
                case "percent sign":
                    texts[curLine].Insert(curPos, '%');
                    curPos++;
                    if (isInsert)
                        texts[curLine].RemoveAt(curPos);
                    isCaps = isCapslock;
                    isShift = false;
                    break;
                case "ampersand":
                    texts[curLine].Insert(curPos, '&');
                    curPos++;
                    if (isInsert)
                        texts[curLine].RemoveAt(curPos);
                    isCaps = isCapslock;
                    isShift = false;
                    break;
                case "caret":
                    texts[curLine].Insert(curPos, '^');
                    curPos++;
                    if (isInsert)
                        texts[curLine].RemoveAt(curPos);
                    isCaps = isCapslock;
                    isShift = false;
                    break;
                case "at":
                    texts[curLine].Insert(curPos, '@');
                    curPos++;
                    if (isInsert)
                        texts[curLine].RemoveAt(curPos);
                    isCaps = isCapslock;
                    isShift = false;
                    break;
                case "number sign":
                    texts[curLine].Insert(curPos, '#');
                    curPos++;
                    if (isInsert)
                        texts[curLine].RemoveAt(curPos);
                    isCaps = isCapslock;
                    isShift = false;
                    break;
                case "dollar sign":
                    texts[curLine].Insert(curPos, '$');
                    curPos++;
                    if (isInsert)
                        texts[curLine].RemoveAt(curPos);                        
                    isCaps = isCapslock;
                    isShift = false;
                    break;
                case "vertical bar":
                    texts[curLine].Insert(curPos, '|');
                    curPos++;
                    if (isInsert)
                        texts[curLine].RemoveAt(curPos);
                    isCaps = isCapslock;
                    isShift = false;
                    break;
                case "Space":
                    texts[curLine].Insert(curPos, ' ');
                    curPos++;
                    if (isInsert)
                        texts[curLine].RemoveAt(curPos);
                    isCaps = isCapslock;
                    isShift = false;
                    break;

                // functions
                case "Enter":
                    texts.Insert(curLine + 1, new List<char>());
                    while (curPos <= texts[curLine].Count - 1)
                    {
                        texts[curLine + 1].Add(texts[curLine][curPos]);
                        texts[curLine].RemoveAt(curPos);
                    }
                    curPos = 0;
                    curLine++;
                    break;
                case "Shift":
                    isShift = !isShift;
                    isCaps = !isCaps;
                    break;
                case "Capslock":
                    isCapslock = !isCapslock;
                    isCaps = !isCaps;
                    break;
                case "Insert":
                    isInsert = !isInsert;
                    break;
                case "Backspace":
                    if (texts[curLine].Count <= 0)
                    {
                        curLine--;
                        curPos = texts[curLine].Count;
                        break;
                    }
                    if (curPos > 0)
                    {
                        texts[curLine].RemoveAt(curPos - 1);
                        curPos--;
                    }
                    else
                    {
                        if (curLine > 0)
                        {
                            curPos = texts[curLine - 1].Count;
                            for (int i = 0; i < texts[curLine].Count; i++)
                            {
                                texts[curLine - 1].Add(texts[curLine][i]);
                            }
                            texts.RemoveAt(curLine);
                            curLine--;
                        }
                    }
                    break;
                case "Delete":
                    texts[curLine].RemoveAt(curPos);
                    break;
                case "Left":
                    if (curPos > 0)
                    {
                        curPos--;
                    }
                    else
                    {
                        if (curLine == 0) break;
                        curPos = texts[curLine - 1].Count;
                        curLine--;
                    }
                    break;
                case "Right":
                    if (texts[curLine].Count - 1 >= curPos)
                        curPos++;
                    else
                    {
                        if (curLine == texts.Count - 1) break;
                        curPos = 0;
                        curLine++;
                    }
                    break;
                case "Up":
                    curLine--;
                    if (curLine < 0)
                    {
                        curLine = 0;
                        curPos = 0;
                    }
                    else if(texts[curLine].Count < curPos) curPos = texts[curLine].Count;
                    break;
                case "Down":
                    curLine++;
                    if (texts.Count <= curLine) curLine = texts.Count - 1;
                    else if (texts[curLine].Count < curPos) curPos = texts[curLine].Count;
                    break;

                case "Save":
                    File.WriteAllLines(FilePath, GenerateStringArray());

                    break;
                default:
                    break;
            }
        }

        // Gesture
        public void TextEditor_Buttons_Functions(string functionKey)
        {
            switch (functionKey)
            {
                case " ":
                    VoiceControl("Space");
                    break;
                case "-":
                    VoiceControl("hyphen");
                    break;
                case "=":
                    VoiceControl("equal");
                    break;
                case "[":
                    VoiceControl("open square bracket");
                    break;
                case "]":
                    VoiceControl("close square bracket");
                    break;
                case "\\":
                    VoiceControl("backslash");
                    break;
                case "CapsLk":
                    VoiceControl("Capslock");
                    break;
                case ";":
                    VoiceControl("colon");
                    break;
                case "'":
                    VoiceControl("apostrophe");
                    break;
                case ",":
                    VoiceControl("comma");
                    break;
                case ".":
                    VoiceControl("full stop");
                    break;
                case "/":
                    VoiceControl("slash");
                    break;
                default:
                    VoiceControl(functionKey);
                    break;

            }
        }
    }
}
