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
        Calculator_Functions[] Calculator_Functions;

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
                                "plus","minus","negative","times","multiply","over","divide","point","dot","percent","answer","equal","equals","reset","clear"}) };

            foreach (Microsoft.Speech.Recognition.Grammar grammar in Grammars)
            {
                MainWindow.mainWindow.BuildNewGrammar(grammar);
            }

            MinimumHeight = 400;
            MinimumWidth = 320;
            Height = MinimumHeight;
            Width = MinimumWidth;
            PosX = 100;//For testing
            PosY = 100;//For testing
            Calculator_Functions = new Calculator_Functions[] { new Calculator_Functions(this, '0'), new Calculator_Functions(this, '1'), new Calculator_Functions(this, '2'),
                                                new Calculator_Functions(this, '3'), new Calculator_Functions(this, '4'), new Calculator_Functions(this, '5'), new Calculator_Functions(this, '6'),
                                                new Calculator_Functions(this, '7'), new Calculator_Functions(this, '8'), new Calculator_Functions(this, '9'), new Calculator_Functions(this, '+'),
                                                new Calculator_Functions(this, 'x'), new Calculator_Functions(this, '/'), new Calculator_Functions(this, '='), new Calculator_Functions(this, '.'),
                                                new Calculator_Functions(this, '-'), new Calculator_Functions(this, '%'), new Calculator_Functions(this, 'C') /*ON/OFF*/ };

            LocalEdgeControl = new LocalEdgeControl(this);
            Rect = new Rect(PosX, PosY, Width, Height);
        }

        public override void Print()
        {
            //base.Print();
            MainWindow.RenderManager.DrawingContext.DrawRectangle(Brushes.Black, null, Rect);

            foreach (Calculator_Functions unit in Calculator_Functions)
            {
                unit.Print();
            }

            FormattedText pathFormattedText = new FormattedText(formula + unfinishedFormula == "" ? "0" : formula + unfinishedFormula,
                CultureInfo.GetCultureInfo("en-us"),
                FlowDirection.LeftToRight,
                new Typeface("Verdana"),
                36,
                Brushes.White, 30);
            pathFormattedText.TextAlignment = TextAlignment.Right;
            pathFormattedText.Trimming = TextTrimming.CharacterEllipsis;
            MainWindow.RenderManager.DrawingContext.DrawText(pathFormattedText, new Point(PosX + 312, PosY + 28));
            LocalEdgeControl.Print();
        }

        public override void UpdateRect()
        {
            base.UpdateRect();

            foreach (Calculator_Functions unit in Calculator_Functions)
            {
                unit.UpdateRect();
            }
        }

        public override void Update(bool isFocusing, int listOrder, Point point, Microsoft.Kinect.HandState handState, string command, string gesture)
        {
            base.Update(isFocusing, listOrder, point, handState, command, gesture);

            if (!isFocusing) return;

            int clampedX = point.X < 0 ? 0 : point.X > MainWindow.Drawing_Width ? MainWindow.Drawing_Width : (int)point.X;
            int clampedY = point.Y < 0 ? 0 : point.Y > MainWindow.Drawing_Height ? MainWindow.Drawing_Height : (int)point.Y;
                        
            foreach (Calculator_Functions unit in Calculator_Functions)
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
            if (numberString.Length == 0) return 0;
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
        public bool keyPressed = false;

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
                    if (!keyPressed && !CheckAvailableInput(1)) break;
                    if (displayingAnswer)
                    {
                        formula = "";
                        displayingAnswer = false;
                    }
                    tempValueString += "nine ";
                    unfinishedFormula = ConvertToNumbers(tempValueString).ToString();
                    break;
                case "ten":
                    if (!keyPressed && !CheckAvailableInput(2)) break;
                    if (displayingAnswer)
                    {
                        formula = "";
                        displayingAnswer = false;
                    }
                    tempValueString += "ten ";
                    unfinishedFormula = ConvertToNumbers(tempValueString).ToString();
                    break;
                case "eleven":
                    if (!keyPressed && !CheckAvailableInput(2)) break;
                    if (displayingAnswer)
                    {
                        formula = "";
                        displayingAnswer = false;
                    }
                    tempValueString += "eleven ";
                    unfinishedFormula = ConvertToNumbers(tempValueString).ToString();
                    break;
                case "twelve":
                    if (!keyPressed && !CheckAvailableInput(2)) break;
                    if (displayingAnswer)
                    {
                        formula = "";
                        displayingAnswer = false;
                    }
                    tempValueString += "twelve ";
                    unfinishedFormula = ConvertToNumbers(tempValueString).ToString();
                    break;
                case "thirteen":
                    if (!keyPressed && !CheckAvailableInput(2)) break;
                    if (displayingAnswer)
                    {
                        formula = "";
                        displayingAnswer = false;
                    }
                    tempValueString += "thirteen ";
                    unfinishedFormula = ConvertToNumbers(tempValueString).ToString();
                    break;
                case "fourteen":
                    if (!keyPressed && !CheckAvailableInput(2)) break;
                    if (displayingAnswer)
                    {
                        formula = "";
                        displayingAnswer = false;
                    }
                    tempValueString += "fourteen ";
                    unfinishedFormula = ConvertToNumbers(tempValueString).ToString();
                    break;
                case "fifteen":
                    if (!keyPressed && !CheckAvailableInput(2)) break;
                    if (displayingAnswer)
                    {
                        formula = "";
                        displayingAnswer = false;
                    }
                    tempValueString += "fifteen ";
                    unfinishedFormula = ConvertToNumbers(tempValueString).ToString();
                    break;
                case "sixteen":
                    if (!keyPressed && !CheckAvailableInput(2)) break;
                    if (displayingAnswer)
                    {
                        formula = "";
                        displayingAnswer = false;
                    }
                    tempValueString += "sixteen ";
                    unfinishedFormula = ConvertToNumbers(tempValueString).ToString();
                    break;
                case "seventeen":
                    if (!keyPressed && !CheckAvailableInput(2)) break;
                    if (displayingAnswer)
                    {
                        formula = "";
                        displayingAnswer = false;
                    }
                    tempValueString += "seventeen ";
                    unfinishedFormula = ConvertToNumbers(tempValueString).ToString();
                    break;
                case "eighteen":
                    if (!keyPressed && !CheckAvailableInput(2)) break;
                    if (displayingAnswer)
                    {
                        formula = "";
                        displayingAnswer = false;
                    }
                    tempValueString += "eighteen ";
                    unfinishedFormula = ConvertToNumbers(tempValueString).ToString();
                    break;
                case "nineteen":
                    if (!keyPressed && !CheckAvailableInput(2)) break;
                    if (displayingAnswer)
                    {
                        formula = "";
                        displayingAnswer = false;
                    }
                    tempValueString += "nineteen ";
                    unfinishedFormula = ConvertToNumbers(tempValueString).ToString();
                    break;
                case "twenty":
                    if (!keyPressed && !CheckAvailableInput(2)) break;
                    if (displayingAnswer)
                    {
                        formula = "";
                        displayingAnswer = false;
                    }
                    tempValueString += "twenty ";
                    unfinishedFormula = ConvertToNumbers(tempValueString).ToString();
                    break;
                case "thirty":
                    if (!keyPressed && !CheckAvailableInput(2)) break;
                    if (displayingAnswer)
                    {
                        formula = "";
                        displayingAnswer = false;
                    }
                    tempValueString += "thirty ";
                    unfinishedFormula = ConvertToNumbers(tempValueString).ToString();
                    break;
                case "fourty":
                    if (!keyPressed && !CheckAvailableInput(2)) break;
                    if (displayingAnswer)
                    {
                        formula = "";
                        displayingAnswer = false;
                    }
                    tempValueString += "fourty ";
                    unfinishedFormula = ConvertToNumbers(tempValueString).ToString();
                    break;
                case "fifty":
                    if (!keyPressed && !CheckAvailableInput(2)) break;
                    if (displayingAnswer)
                    {
                        formula = "";
                        displayingAnswer = false;
                    }
                    tempValueString += "fifty ";
                    unfinishedFormula = ConvertToNumbers(tempValueString).ToString();
                    break;
                case "sixty":
                    if (!keyPressed && !CheckAvailableInput(2)) break;
                    if (displayingAnswer)
                    {
                        formula = "";
                        displayingAnswer = false;
                    }
                    tempValueString += "sixty ";
                    unfinishedFormula = ConvertToNumbers(tempValueString).ToString();
                    break;
                case "seventy":
                    if (!keyPressed && !CheckAvailableInput(2)) break;
                    if (displayingAnswer)
                    {
                        formula = "";
                        displayingAnswer = false;
                    }
                    tempValueString += "seventy ";
                    unfinishedFormula = ConvertToNumbers(tempValueString).ToString();
                    break;
                case "eighty":
                    if (!keyPressed && !CheckAvailableInput(2)) break;
                    if (displayingAnswer)
                    {
                        formula = "";
                        displayingAnswer = false;
                    }
                    tempValueString += "eighty ";
                    unfinishedFormula = ConvertToNumbers(tempValueString).ToString();
                    break;
                case "ninety":
                    if (!keyPressed && !CheckAvailableInput(2)) break;
                    if (displayingAnswer)
                    {
                        formula = "";
                        displayingAnswer = false;
                    }
                    tempValueString += "ninety ";
                    unfinishedFormula = ConvertToNumbers(tempValueString).ToString();
                    break;
                case "hundred":
                    if (!keyPressed && !CheckAvailableInput(3)) break;
                    if (displayingAnswer)
                    {
                        formula = "";
                        displayingAnswer = false;
                    }
                    tempValueString += "hundred ";
                    unfinishedFormula = ConvertToNumbers(tempValueString).ToString();
                    break;
                case "thousand":
                    if (!keyPressed && !CheckAvailableInput(4)) break;
                    if (displayingAnswer)
                    {
                        formula = "";
                        displayingAnswer = false;
                    }
                    tempValueString += "thousand ";
                    unfinishedFormula = ConvertToNumbers(tempValueString).ToString();
                    break;
                case "million":
                    if (!keyPressed && !CheckAvailableInput(7)) break;
                    if (displayingAnswer)
                    {
                        formula = "";
                        displayingAnswer = false;
                    }
                    tempValueString += "million ";
                    unfinishedFormula = ConvertToNumbers(tempValueString).ToString();
                    break;
                case "billion":
                    if (!keyPressed && !CheckAvailableInput(10)) break;
                    if (displayingAnswer)
                    {
                        formula = "";
                        displayingAnswer = false;
                    }
                    tempValueString += "billion ";
                    unfinishedFormula = ConvertToNumbers(tempValueString).ToString();
                    break;
                case "trillion":
                    if (!keyPressed && !CheckAvailableInput(13)) break;
                    if (displayingAnswer)
                    {
                        formula = "";
                        displayingAnswer = false;
                    }
                    tempValueString += "trillion ";
                    unfinishedFormula = ConvertToNumbers(tempValueString).ToString();
                    break;
                case "quadrillion":
                    if (!keyPressed && !CheckAvailableInput(16)) break;
                    if (displayingAnswer)
                    {
                        formula = "";
                        displayingAnswer = false;
                    }
                    tempValueString += "quadrillion ";
                    unfinishedFormula = ConvertToNumbers(tempValueString).ToString();
                    break;
                case "quintillion":
                    if (!keyPressed && !CheckAvailableInput(19)) break;
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
                    
                    if (!keyPressed)
                    {
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
                    }
                    else
                    {
                        values.Add(int.Parse(unfinishedFormula));
                    }
                    tempValueString = "";
                    unfinishedFormula = "";
                    isNegative = false;
                    hasDecimal = false;
                    isPercent = false;
                    keyPressed = false;
                    operators.Add('+');
                    break;
                case "minus":
                    if (formula.EndsWith(" - ")) break;
                    formula += unfinishedFormula + " - ";
                    
                    if (!keyPressed)
                    {
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
                    }
                    else
                    {
                        values.Add(int.Parse(unfinishedFormula));
                    }
                    unfinishedFormula = "";
                    tempValueString = "";
                    isNegative = false;
                    hasDecimal = false;
                    isPercent = false;
                    keyPressed = false;
                    operators.Add('-');
                    break;
                case "times":
                case "multiply":
                    if (formula.EndsWith(" x ")) break;
                    formula += unfinishedFormula + " x ";
                    
                    if (!keyPressed)
                    {
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
                    }
                    else
                    {
                        values.Add(int.Parse(unfinishedFormula));
                    }
                    unfinishedFormula = "";
                    tempValueString = "";
                    isNegative = false;
                    hasDecimal = false;
                    keyPressed = false;
                    isPercent = false;
                    operators.Add('*');
                    break;
                case "over":
                case "divide":
                    if (formula.EndsWith(" / ")) break;
                    formula += unfinishedFormula + " / ";
                    
                    if (!keyPressed)
                    {
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
                    }
                    else
                    {
                        values.Add(int.Parse(unfinishedFormula));
                    }
                    tempValueString = "";
                    unfinishedFormula = "";
                    isNegative = false;
                    hasDecimal = false;
                    keyPressed = false;
                    isPercent = false;
                    operators.Add('/');
                    break;
                case "percent":
                    if (!isPercent && unfinishedFormula.Length > 0)
                    {
                        unfinishedFormula += "%";
                        isPercent = true;
                    }
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
                    if (hasDecimal) break;
                    if (unfinishedFormula == "")
                    {
                        unfinishedFormula = "0";
                        tempValueString += "zero ";
                    }
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
                    if (!keyPressed)
                    {
                        if (hasDecimal)
                        {
                            double tempDecimal = ConvertToNumbers(tempValueString);
                            values.Add((tempValue + tempDecimal * Math.Pow(10, -tempDecimal.ToString().Length)) * (isPercent ? 0.01f : 1));
                            tempValue = 0;
                        }
                        else values.Add(ConvertToNumbers(tempValueString) * (isNegative ? -1 : 1) * (isPercent ? 0.01f : 1));
                    }
                    else
                    {
                        values.Add(int.Parse(unfinishedFormula));
                    }
                    tempValueString = "";
                    unfinishedFormula = "";
                    isNegative = false;
                    keyPressed = false;
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
                    keyPressed = false;
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
        public void Calculator_Buttons_Functions(char functionKey)
        {
            switch (functionKey)
            {
                case '0':
                    unfinishedFormula += "0";
                    keyPressed = true;
                    break;
                case '1':
                    unfinishedFormula += "1";
                    keyPressed = true;
                    break;
                case '2':
                    unfinishedFormula += "2";
                    keyPressed = true;
                    break;
                case '3':
                    unfinishedFormula += "3";
                    keyPressed = true;
                    break;
                case '4':
                    unfinishedFormula += "4";
                    keyPressed = true;
                    break;
                case '5':
                    unfinishedFormula += "5";
                    keyPressed = true;
                    break;
                case '6':
                    unfinishedFormula += "6";
                    keyPressed = true;
                    break;
                case '7':
                    unfinishedFormula += "7";
                    keyPressed = true;
                    break;
                case '8':
                    unfinishedFormula += "8";
                    keyPressed = true;
                    break;
                case '9':
                    unfinishedFormula += "9";
                    keyPressed = true;
                    break;
                case '.':
                    if (unfinishedFormula == "" || hasDecimal) break;
                    unfinishedFormula += ".";
                    tempValue = ConvertToNumbers(tempValueString) * (isNegative ? -1 : 1);
                    tempValueString = "";
                    isNegative = false;
                    keyPressed = true;
                    keyPressed = true;
                    hasDecimal = true;
                    break;
                case '=':
                    if (operators.Count() == 0) break;
                    formula += unfinishedFormula + " = ";
                    if (!keyPressed)
                    {
                        if (hasDecimal)
                        {
                            double tempDecimal = ConvertToNumbers(tempValueString);
                            values.Add((tempValue + tempDecimal * Math.Pow(10, -tempDecimal.ToString().Length)) * (isPercent ? 0.01f : 1));
                            tempValue = 0;
                        }
                        else values.Add(ConvertToNumbers(tempValueString) * (isNegative ? -1 : 1) * (isPercent ? 0.01f : 1));
                    }
                    else
                    {
                        values.Add(int.Parse(unfinishedFormula));
                    }
                    tempValueString = "";
                    unfinishedFormula = "";
                    isNegative = false;
                    hasDecimal = false;
                    keyPressed = false;
                    isPercent = false;
                    // todo: output answer
                    answer = CalculateAnswer();
                    formula = answer.ToString();
                    displayingAnswer = true;
                    values.Clear();
                    operators.Clear();
                    break;
                case '+':
                    if (formula.EndsWith(" + ")) break;
                    formula += unfinishedFormula + " + ";
                    
                    if (!keyPressed)
                    {
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
                    }
                    else
                    {
                        values.Add(int.Parse(unfinishedFormula));
                    }
                    unfinishedFormula = "";
                    tempValueString = "";
                    isNegative = false;
                    keyPressed = false;
                    hasDecimal = false;
                    isPercent = false;
                    operators.Add('+');
                    break;
                case '-':
                    if (formula.EndsWith(" - ")) break;
                    formula += unfinishedFormula + " - ";
                    
                    if (!keyPressed)
                    {
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
                    }
                    else
                    {
                        values.Add(int.Parse(unfinishedFormula));
                    }
                    unfinishedFormula = "";
                    tempValueString = "";
                    isNegative = false;
                    hasDecimal = false;
                    keyPressed = false;
                    isPercent = false;
                    operators.Add('-');
                    break;
                case 'x':
                    if (formula.EndsWith(" x ")) break;
                    formula += unfinishedFormula + " x ";
                    
                    if (!keyPressed)
                    {
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
                    }
                    else
                    {
                        values.Add(int.Parse(unfinishedFormula));
                    }
                    unfinishedFormula = "";
                    tempValueString = "";
                    isNegative = false;
                    hasDecimal = false;
                    isPercent = false;
                    keyPressed = false;
                    operators.Add('*');
                    break;
                case '/':
                    if (formula.EndsWith(" / ")) break;
                    formula += unfinishedFormula + " / ";
                    
                    if (!keyPressed)
                    {
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
                    }
                    else
                    {
                        values.Add(int.Parse(unfinishedFormula));
                    }
                    unfinishedFormula = "";
                    tempValueString = "";
                    isNegative = false;
                    hasDecimal = false;
                    isPercent = false;
                    keyPressed = false;
                    operators.Add('/');
                    break;
                case '%':
                    if (!isPercent && unfinishedFormula.Length > 0)
                    {
                        unfinishedFormula += "%";
                        isPercent = true;
                        keyPressed = true;
                    }
                    break;
                case 'C':
                    // todo: reset
                    tempValueString = "";
                    formula = "";
                    unfinishedFormula = "";
                    isNegative = false;
                    hasDecimal = false;
                    displayingAnswer = false;
                    keyPressed = false;
                    isPercent = false;
                    values.Clear();
                    operators.Clear();
                    answer = 0;
                    break;
                default:
                    break;
            }
        }
    }

}
