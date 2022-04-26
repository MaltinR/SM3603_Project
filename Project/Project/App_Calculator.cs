using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Globalization;

namespace Project
{
    public class App_Calculator : NonDesktopApplication
    {
        Calculator_test[] tests;

        public App_Calculator()
        {
            Image_Normal = new BitmapImage(new Uri("Images/Icon_Calculator_Normal.png", UriKind.Relative));
            Image_Selecting = new BitmapImage(new Uri("Images/Icon_Calculator_Selecting.png", UriKind.Relative));

            //Please enter the needed speech
            Grammars = new Microsoft.Speech.Recognition.Grammar[] { MainWindow.GetGrammar("",
                new string[] {"zero", "one", "two","three","four","five","six","seven","eight","nine","ten",
                                "eleven","twelve","thirteen","fourteen","fifteen","sixteen","seventeen","eighteen","nineteen","twenty",
                                "thirty","forty","fifty","sixty","seventy","eighty","ninety","hundred","thousand",
                                "million","billion","trillion","quadrillion","quintillion",
                                "plus","minus","negative","times","multiply","divide","point","dot","percent","answer","equal","equals","reset","clear"}) };

            foreach (Microsoft.Speech.Recognition.Grammar grammar in Grammars)
            {
                MainWindow.mainWindow.BuildNewGrammar(grammar);
            }

            tests = new Calculator_test[] { new Calculator_test(this, 0), new Calculator_test(this, 1), new Calculator_test(this, 2) };

            PosX = 100;//For testing
            LocalEdgeControl = new LocalEdgeControl(this);
            Rect = new Rect(PosX, PosY, Width, Height);
        }

        public override void Print()
        {
            //base.Print();

            FormattedText pathFormattedText = new FormattedText("Calculating: " + formula + unfinishedFormula,
                CultureInfo.GetCultureInfo("en-us"),
                FlowDirection.LeftToRight,
                new Typeface("Verdana"),
                10,
                Brushes.Black, 30);

            pathFormattedText.MaxTextWidth = Width - 10;
            pathFormattedText.MaxTextHeight = 28;
            pathFormattedText.Trimming = TextTrimming.CharacterEllipsis;
            MainWindow.RenderManager.DrawingContext.DrawText(pathFormattedText, new Point(PosX + 5, PosY + 10));
            LocalEdgeControl.Print();
        }

        public override void Update(bool isFocusing, int listOrder, Point point, Microsoft.Kinect.HandState handState, string command, string gesture)
        {
            base.Update(isFocusing, listOrder, point, handState, command, gesture);

            if (!isFocusing) return;

            int clampedX = point.X < 0 ? 0 : point.X > MainWindow.Drawing_Width ? MainWindow.Drawing_Width : (int)point.X;
            int clampedY = point.Y < 0 ? 0 : point.Y > MainWindow.Drawing_Height ? MainWindow.Drawing_Height : (int)point.Y;
                        
            foreach (Calculator_test unit in tests)
            {
                unit.IsHoveringOrDragging(clampedX, clampedY, listOrder, handState);
            }

            VoiceControl(command);

            GestureControl(gesture);
        }

        // referenced from https://www.c-sharpcorner.com/blogs/convert-words-to-numbers-in-c-sharp
        private static Dictionary<string, double> numberTable = new Dictionary<string, double>{
            {"zero",0},{"one",1},{"two",2},{"three",3},{"four",4},{"five",5},{"six",6},
            {"seven",7},{"eight",8},{"nine",9},{"ten",10},{"eleven",11},{"twelve",12},
            {"thirteen",13},{"fourteen",14},{"fifteen",15},{"sixteen",16},{"seventeen",17},
            {"eighteen",18},{"nineteen",19},{"twenty",20},{"thirty",30},{"forty",40},
            {"fifty",50},{"sixty",60},{"seventy",70},{"eighty",80},{"ninety",90},
            {"hundred",100},{"thousand",1000},{"million",1000000},
            {"billion",1000000000},{"trillion",1000000000000},{"quadrillion",1000000000000000},
            {"quintillion",1000000000000000000}
        };
        public static double ConvertToNumbers(string numberString)
        {
            string tempString = numberString[numberString.Length - 1] == ' ' ? numberString.Remove(numberString.Length - 1, 1) : numberString;
            var numbers = Regex.Matches(tempString, @"\w+").Cast<Match>()
                    .Select(m => m.Value.ToLowerInvariant())
                    .Where(v => numberTable.ContainsKey(v))
                    .Select(v => numberTable[v]);
            double acc = 0, total = 0L;
            foreach (var n in numbers)
            {
                if (n >= 1000)
                {
                    total += acc * n;
                    acc = 0;
                }
                else if (n >= 100)
                {
                    acc *= n;
                }
                else acc += n;
            }
            return (total + acc) * (tempString.StartsWith("minus",
                    StringComparison.InvariantCultureIgnoreCase) ? -1 : 1);
        }
        
        public double CalculateAnswer()
        {
            for (int i = 0; i < operators.Count(); i++)
            {
                if (operators[i] == '*')
                {
                    double temp_value = values[i] * values[i + 1];
                    values.RemoveAt(i);
                    values.RemoveAt(i);
                    operators.RemoveAt(i);
                    values.Insert(i, temp_value);
                    i--;
                }
                else if (operators[i] == '/')
                {
                    double temp_value = values[i] / values[i + 1];
                    values.RemoveAt(i);
                    values.RemoveAt(i);
                    operators.RemoveAt(i);
                    values.Insert(i, temp_value);
                    i--;
                }
            }
            double result = values[0];
            for (int i = 0; i < operators.Count(); i++)
            {
                if (operators[i] == '+')
                {
                    result += values[i + 1];
                }
                else if (operators[i] == '-')
                {
                    result -= values[i + 1];
                }
            }
            return result;
        }


        // output = formula + unfinishedformula;
        public string formula = ""; 
        public string unfinishedFormula = "";

        public string tempValueString = "";
        public double tempValue = 0;
        public List<double> values = new List<double>();
        public List<char> operators = new List<char>();
        public double answer = 0;
        public bool isNegative = false;
        public bool hasDecimal = false;
        public bool isPercent = false;
        public bool displayingAnswer = false;

        public bool CheckAvailableInput(int digits)
        {
            if (tempValueString.EndsWith("percent ")) return false;

            bool isAvailable = !(tempValueString.EndsWith("zero ") || tempValueString.EndsWith("one ") || tempValueString.EndsWith("two ") || tempValueString.EndsWith("three ")
                   || tempValueString.EndsWith("four ") || tempValueString.EndsWith("five ") || tempValueString.EndsWith("six ") || tempValueString.EndsWith("seven ")
                   || tempValueString.EndsWith("eight ") || tempValueString.EndsWith("nine ") || tempValueString.EndsWith("ten ") || tempValueString.EndsWith("eleven ")
                   || tempValueString.EndsWith("twelve ") || tempValueString.EndsWith("thirteen ") || tempValueString.EndsWith("fourteen ") || tempValueString.EndsWith("fifteen ")
                   || tempValueString.EndsWith("sixteen ") || tempValueString.EndsWith("seventeen ") || tempValueString.EndsWith("eighteen ") || tempValueString.EndsWith("nineteen "));
            if (!isAvailable || digits == 1) return isAvailable;

            isAvailable = !(tempValueString.EndsWith("twenty ") || tempValueString.EndsWith("thirty ") || tempValueString.EndsWith("fourty ") || tempValueString.EndsWith("fifty ")
                   || tempValueString.EndsWith("sixty ") || tempValueString.EndsWith("seventy ") || tempValueString.EndsWith("eighty ") || tempValueString.EndsWith("ninety "));
            if (!isAvailable || digits == 2) return isAvailable;

            isAvailable = !tempValueString.EndsWith("hundred ");
            if (!isAvailable || digits == 3) return isAvailable;

            isAvailable = !tempValueString.EndsWith("thousand ");
            if (!isAvailable || digits == 4) return isAvailable;

            isAvailable = !tempValueString.EndsWith("million ");
            if (!isAvailable || digits == 7) return isAvailable;

            isAvailable = !tempValueString.EndsWith("billion ");
            if (!isAvailable || digits == 10) return isAvailable;

            isAvailable = !tempValueString.EndsWith("trillion ");
            if (!isAvailable || digits == 13) return isAvailable;

            isAvailable = !tempValueString.EndsWith("quadrillion ");
            if (!isAvailable || digits == 16) return isAvailable;

            isAvailable = !tempValueString.EndsWith("quintillion ");
            if (!isAvailable || digits == 19) return isAvailable;

            return true;
        }
        
        public override void VoiceControl(string command)
        {
            switch (command)
            {
                case "answer":
                    values.Add(answer * (isNegative ? -1 : 1));
                    isNegative = false;
                    break;
                case "zero":
                    if (!CheckAvailableInput(1)) break; 
                    if (displayingAnswer)
                    {
                        formula = "";
                        displayingAnswer = false;
                    }
                    tempValueString += "zero ";
                    unfinishedFormula = ConvertToNumbers(tempValueString).ToString();
                    break;
                case "one":
                    if (!CheckAvailableInput(1)) break;
                    if (displayingAnswer)
                    {
                        formula = "";
                        displayingAnswer = false;
                    }
                    tempValueString += "one ";
                    unfinishedFormula = ConvertToNumbers(tempValueString).ToString();
                    break;
                case "two":
                    if (!CheckAvailableInput(1)) break;
                    if (displayingAnswer)
                    {
                        formula = "";
                        displayingAnswer = false;
                    }
                    tempValueString += "two ";
                    unfinishedFormula = ConvertToNumbers(tempValueString).ToString();
                    break;
                case "three":
                    if (!CheckAvailableInput(1)) break;
                    if (displayingAnswer)
                    {
                        formula = "";
                        displayingAnswer = false;
                    }
                    tempValueString += "three ";
                    unfinishedFormula = ConvertToNumbers(tempValueString).ToString();
                    break;
                case "four":
                    if (!CheckAvailableInput(1)) break;
                    if (displayingAnswer)
                    {
                        formula = "";
                        displayingAnswer = false;
                    }
                    tempValueString += "four ";
                    unfinishedFormula = ConvertToNumbers(tempValueString).ToString();
                    break;
                case "five":
                    if (!CheckAvailableInput(1)) break;
                    if (displayingAnswer)
                    {
                        formula = "";
                        displayingAnswer = false;
                    }
                    tempValueString += "five ";
                    unfinishedFormula = ConvertToNumbers(tempValueString).ToString();
                    break;
                case "six":
                    if (!CheckAvailableInput(1)) break;
                    if (displayingAnswer)
                    {
                        formula = "";
                        displayingAnswer = false;
                    }
                    tempValueString += "six ";
                    unfinishedFormula = ConvertToNumbers(tempValueString).ToString();
                    break;
                case "seven":
                    if (!CheckAvailableInput(1)) break;
                    if (displayingAnswer)
                    {
                        formula = "";
                        displayingAnswer = false;
                    }
                    tempValueString += "seven ";
                    unfinishedFormula = ConvertToNumbers(tempValueString).ToString();
                    break;
                case "eight":
                    if (!CheckAvailableInput(1)) break;
                    if (displayingAnswer)
                    {
                        formula = "";
                        displayingAnswer = false;
                    }
                    tempValueString += "eight ";
                    unfinishedFormula = ConvertToNumbers(tempValueString).ToString();
                    break;
                case "nine":
                    if (!CheckAvailableInput(1)) break;
                    if (displayingAnswer)
                    {
                        formula = "";
                        displayingAnswer = false;
                    }
                    tempValueString += "nine ";
                    unfinishedFormula = ConvertToNumbers(tempValueString).ToString();
                    break;
                case "ten":
                    if (!CheckAvailableInput(2)) break;
                    if (displayingAnswer)
                    {
                        formula = "";
                        displayingAnswer = false;
                    }
                    tempValueString += "ten ";
                    unfinishedFormula = ConvertToNumbers(tempValueString).ToString();
                    break;
                case "eleven":
                    if (!CheckAvailableInput(2)) break;
                    if (displayingAnswer)
                    {
                        formula = "";
                        displayingAnswer = false;
                    }
                    tempValueString += "eleven ";
                    unfinishedFormula = ConvertToNumbers(tempValueString).ToString();
                    break;
                case "twelve":
                    if (!CheckAvailableInput(2)) break;
                    if (displayingAnswer)
                    {
                        formula = "";
                        displayingAnswer = false;
                    }
                    tempValueString += "twelve ";
                    unfinishedFormula = ConvertToNumbers(tempValueString).ToString();
                    break;
                case "thirteen":
                    if (!CheckAvailableInput(2)) break;
                    if (displayingAnswer)
                    {
                        formula = "";
                        displayingAnswer = false;
                    }
                    tempValueString += "thirteen ";
                    unfinishedFormula = ConvertToNumbers(tempValueString).ToString();
                    break;
                case "fourteen":
                    if (!CheckAvailableInput(2)) break;
                    if (displayingAnswer)
                    {
                        formula = "";
                        displayingAnswer = false;
                    }
                    tempValueString += "fourteen ";
                    unfinishedFormula = ConvertToNumbers(tempValueString).ToString();
                    break;
                case "fifteen":
                    if (!CheckAvailableInput(2)) break;
                    if (displayingAnswer)
                    {
                        formula = "";
                        displayingAnswer = false;
                    }
                    tempValueString += "fifteen ";
                    unfinishedFormula = ConvertToNumbers(tempValueString).ToString();
                    break;
                case "sixteen":
                    if (!CheckAvailableInput(2)) break;
                    if (displayingAnswer)
                    {
                        formula = "";
                        displayingAnswer = false;
                    }
                    tempValueString += "sixteen ";
                    unfinishedFormula = ConvertToNumbers(tempValueString).ToString();
                    break;
                case "seventeen":
                    if (!CheckAvailableInput(2)) break;
                    if (displayingAnswer)
                    {
                        formula = "";
                        displayingAnswer = false;
                    }
                    tempValueString += "seventeen ";
                    unfinishedFormula = ConvertToNumbers(tempValueString).ToString();
                    break;
                case "eighteen":
                    if (!CheckAvailableInput(2)) break;
                    if (displayingAnswer)
                    {
                        formula = "";
                        displayingAnswer = false;
                    }
                    tempValueString += "eighteen ";
                    unfinishedFormula = ConvertToNumbers(tempValueString).ToString();
                    break;
                case "nineteen":
                    if (!CheckAvailableInput(2)) break;
                    if (displayingAnswer)
                    {
                        formula = "";
                        displayingAnswer = false;
                    }
                    tempValueString += "nineteen ";
                    unfinishedFormula = ConvertToNumbers(tempValueString).ToString();
                    break;
                case "twenty":
                    if (!CheckAvailableInput(2)) break;
                    if (displayingAnswer)
                    {
                        formula = "";
                        displayingAnswer = false;
                    }
                    tempValueString += "twenty ";
                    unfinishedFormula = ConvertToNumbers(tempValueString).ToString();
                    break;
                case "thirty":
                    if (!CheckAvailableInput(2)) break;
                    if (displayingAnswer)
                    {
                        formula = "";
                        displayingAnswer = false;
                    }
                    tempValueString += "thirty ";
                    unfinishedFormula = ConvertToNumbers(tempValueString).ToString();
                    break;
                case "fourty":
                    if (!CheckAvailableInput(2)) break;
                    if (displayingAnswer)
                    {
                        formula = "";
                        displayingAnswer = false;
                    }
                    tempValueString += "fourty ";
                    unfinishedFormula = ConvertToNumbers(tempValueString).ToString();
                    break;
                case "fifty":
                    if (!CheckAvailableInput(2)) break;
                    if (displayingAnswer)
                    {
                        formula = "";
                        displayingAnswer = false;
                    }
                    tempValueString += "fifty ";
                    unfinishedFormula = ConvertToNumbers(tempValueString).ToString();
                    break;
                case "sixty":
                    if (!CheckAvailableInput(2)) break;
                    if (displayingAnswer)
                    {
                        formula = "";
                        displayingAnswer = false;
                    }
                    tempValueString += "sixty ";
                    unfinishedFormula = ConvertToNumbers(tempValueString).ToString();
                    break;
                case "seventy":
                    if (!CheckAvailableInput(2)) break;
                    if (displayingAnswer)
                    {
                        formula = "";
                        displayingAnswer = false;
                    }
                    tempValueString += "seventy ";
                    unfinishedFormula = ConvertToNumbers(tempValueString).ToString();
                    break;
                case "eighty":
                    if (!CheckAvailableInput(2)) break;
                    if (displayingAnswer)
                    {
                        formula = "";
                        displayingAnswer = false;
                    }
                    tempValueString += "eighty ";
                    unfinishedFormula = ConvertToNumbers(tempValueString).ToString();
                    break;
                case "ninety":
                    if (!CheckAvailableInput(2)) break;
                    if (displayingAnswer)
                    {
                        formula = "";
                        displayingAnswer = false;
                    }
                    tempValueString += "ninety ";
                    unfinishedFormula = ConvertToNumbers(tempValueString).ToString();
                    break;
                case "hundred":
                    if (!CheckAvailableInput(3)) break;
                    if (displayingAnswer)
                    {
                        formula = "";
                        displayingAnswer = false;
                    }
                    tempValueString += "hundred ";
                    unfinishedFormula = ConvertToNumbers(tempValueString).ToString();
                    break;
                case "thousand":
                    if (!CheckAvailableInput(4)) break;
                    if (displayingAnswer)
                    {
                        formula = "";
                        displayingAnswer = false;
                    }
                    tempValueString += "thousand ";
                    unfinishedFormula = ConvertToNumbers(tempValueString).ToString();
                    break;
                case "million":
                    if (!CheckAvailableInput(7)) break;
                    if (displayingAnswer)
                    {
                        formula = "";
                        displayingAnswer = false;
                    }
                    tempValueString += "million ";
                    unfinishedFormula = ConvertToNumbers(tempValueString).ToString();
                    break;
                case "billion":
                    if (!CheckAvailableInput(10)) break;
                    if (displayingAnswer)
                    {
                        formula = "";
                        displayingAnswer = false;
                    }
                    tempValueString += "billion ";
                    unfinishedFormula = ConvertToNumbers(tempValueString).ToString();
                    break;
                case "trillion":
                    if (!CheckAvailableInput(13)) break;
                    if (displayingAnswer)
                    {
                        formula = "";
                        displayingAnswer = false;
                    }
                    tempValueString += "trillion ";
                    unfinishedFormula = ConvertToNumbers(tempValueString).ToString();
                    break;
                case "quadrillion":
                    if (!CheckAvailableInput(16)) break;
                    if (displayingAnswer)
                    {
                        formula = "";
                        displayingAnswer = false;
                    }
                    tempValueString += "quadrillion ";
                    unfinishedFormula = ConvertToNumbers(tempValueString).ToString();
                    break;
                case "quintillion":
                    if (!CheckAvailableInput(19)) break;
                    if (displayingAnswer)
                    {
                        formula = "";
                        displayingAnswer = false;
                    }
                    tempValueString += "quintillion ";
                    unfinishedFormula = ConvertToNumbers(tempValueString).ToString();
                    break;
                
                // operator
                case "plus":
                    if (formula.EndsWith(" + ")) break;
                    formula += unfinishedFormula + " + ";
                    unfinishedFormula = "";
                    if (displayingAnswer)
                    {
                        values.Add(answer);
                        displayingAnswer = false;
                    }
                    else
                    {
                        if (hasDecimal)
                        {
                            double tempDecimal = ConvertToNumbers(tempValueString);
                            values.Add((tempValue + tempDecimal * Math.Pow(10, -tempDecimal.ToString().Length)) * (isPercent ? 0.01f : 1));
                            tempValue = 0;
                        }
                        else values.Add(ConvertToNumbers(tempValueString) * (isNegative ? -1 : 1) * (isPercent ? 0.01f : 1));
                    }
                    tempValueString = "";
                    isNegative = false;
                    hasDecimal = false;
                    isPercent = false;
                    operators.Add('+');
                    break;
                case "minus":
                    if (formula.EndsWith(" - ")) break;
                    formula += unfinishedFormula + " - ";
                    unfinishedFormula = "";
                    if (displayingAnswer)
                    {
                        values.Add(answer);
                        displayingAnswer = false;
                    }
                    else
                    {
                        if (hasDecimal)
                        {
                            double tempDecimal = ConvertToNumbers(tempValueString);
                            values.Add((tempValue + tempDecimal * Math.Pow(10, -tempDecimal.ToString().Length)) * (isPercent ? 0.01f : 1));
                            tempValue = 0;
                        }
                        else values.Add(ConvertToNumbers(tempValueString) * (isNegative ? -1 : 1) * (isPercent ? 0.01f : 1));
                    }
                    tempValueString = "";
                    isNegative = false;
                    hasDecimal = false;
                    isPercent = false;
                    operators.Add('-');
                    break;
                case "times":
                case "multiply":
                    if (formula.EndsWith(" x ")) break;
                    formula += unfinishedFormula + " x ";
                    unfinishedFormula = "";
                    if (displayingAnswer)
                    {
                        values.Add(answer);
                        displayingAnswer = false;
                    }
                    else
                    {
                        if (hasDecimal)
                        {
                            double tempDecimal = ConvertToNumbers(tempValueString);
                            values.Add((tempValue + tempDecimal * Math.Pow(10, -tempDecimal.ToString().Length)) * (isPercent ? 0.01f : 1));
                            tempValue = 0;
                        }
                        else values.Add(ConvertToNumbers(tempValueString) * (isNegative ? -1 : 1) * (isPercent ? 0.01f : 1));
                    }
                    tempValueString = "";
                    isNegative = false;
                    hasDecimal = false;
                    isPercent = false;
                    operators.Add('*');
                    break;
                case "divide":
                    if (formula.EndsWith(" / ")) break;
                    formula += unfinishedFormula + " / ";
                    unfinishedFormula = "";
                    if (displayingAnswer)
                    {
                        values.Add(answer);
                        displayingAnswer = false;
                    }
                    else
                    {
                        if (hasDecimal)
                        {
                            double tempDecimal = ConvertToNumbers(tempValueString);
                            values.Add((tempValue + tempDecimal * Math.Pow(10, -tempDecimal.ToString().Length)) * (isPercent ? 0.01f : 1));
                            tempValue = 0;
                        }
                        else values.Add(ConvertToNumbers(tempValueString) * (isNegative ? -1 : 1) * (isPercent ? 0.01f : 1));
                    }
                    tempValueString = "";
                    isNegative = false;
                    hasDecimal = false;
                    isPercent = false;
                    operators.Add('/');
                    break;
                case "percent":
                    unfinishedFormula += "%";
                    isPercent = true;
                    break;
                case "negative":
                    if (!isNegative)
                    {
                        unfinishedFormula += "-";
                        isNegative = true;
                    }
                    else
                    {
                        unfinishedFormula = unfinishedFormula.Remove(unfinishedFormula.Length - 1, 1);
                        isNegative = false;
                    }
                    break;
                case "point": case "dot":
                    if (unfinishedFormula == "" || hasDecimal) break;
                    unfinishedFormula += ".";
                    tempValue = ConvertToNumbers(tempValueString) * (isNegative ? -1 : 1);
                    tempValueString = "";
                    isNegative = false;
                    hasDecimal = true;
                    break;

                //functions
                case "equal":
                case "equals":
                    if (operators.Count() == 0) break;
                    formula += unfinishedFormula + " = ";
                    if (hasDecimal)
                    {
                        double tempDecimal = ConvertToNumbers(tempValueString);
                        values.Add((tempValue + tempDecimal * Math.Pow(10, -tempDecimal.ToString().Length)) * (isPercent ? 0.01f : 1));
                        tempValue = 0;
                    }
                    else values.Add(ConvertToNumbers(tempValueString) * (isNegative ? -1 : 1) * (isPercent ? 0.01f : 1));
                    tempValueString = "";
                    unfinishedFormula = "";
                    isNegative = false;
                    hasDecimal = false;
                    isPercent = false;
                    // todo: output answer
                    answer = CalculateAnswer();
                    formula = answer.ToString();
                    displayingAnswer = true;
                    values.Clear();
                    operators.Clear();
                    break;
                case "reset": case "clear":
                    // todo: reset
                    tempValueString = "";
                    formula = "";
                    unfinishedFormula = "";
                    isNegative = false;
                    hasDecimal = false;
                    displayingAnswer = false;
                    isPercent = false;
                    values.Clear();
                    operators.Clear();
                    answer = 0;
                    break;
                default:
                    break;
            }
        }
        public void Test(int value)
        {

        }
    }

}
