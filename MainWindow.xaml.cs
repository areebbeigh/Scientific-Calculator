// General purpose calculator, written by Areeb Beigh
// github.io/areeb-beigh

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

/// <todo>
/// TODO - Fix Factorial - shows infinity for high values
/// TODO - Type into text box
/// TODO - Add exponential notation
/// </todo>

namespace Calculator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Max digits to be shown in the memory label, does not affect the actual number stored in memory
        const int maxMemoryLabelLength = 6;
        // Default font result box font size 
        const int defaultFontSize = 48;

        // True if there is an on going math operation
        bool operationCheck;
        // True if a function (sin, tan, ln, log etc) was called on the number during another mathematical operation
        bool functionCheck;
        // True if the result box is to be cleared when a number is entered
        bool clearNext;
        // True if the text in the result box is the result of some computation
        bool isResult;
        // True if the text in the result box has not been changed after clicking an operator
        bool isOldText;
        // Stores the number in memory accessed via MR
        double memory = 0;
        // Stores the text in the text box after a new math operation is selected
        string previousText;
        // Trigonometric modes
        enum trigModes
        {
            STANDARD,  // Default mode
            HYPERBOLIC,
            ARC
        }
        // Stores the current trigonometric mode
        trigModes currentTrigMode;
        // Symbols to show on the button for different trig modes
        Dictionary<trigModes, string> trigModeSymbols = new Dictionary<trigModes, string>()
        {
            { trigModes.STANDARD, "STD" },
            { trigModes.ARC, "ARC" },
            { trigModes.HYPERBOLIC, "HYP" }
        };
        // Stores the current angle unit, default is radians
        Angles.units angleUnit;
        // Symbols to show on the button for different angle units
        Dictionary<Angles.units, string> angleUnitSymbols = new Dictionary<Angles.units, string>()
            {
                { Angles.units.RADIANS, "RAD" },
                { Angles.units.DEGREES, "DEG" },
                { Angles.units.GRADIANS, "GRAD" }
            };
        // Errors that may occur
        static string OVERFLOW = "Overflow";
        static string INVALID_INPUT = "Invalid input";
        static string NOT_A_NUMBER = "NaN";
        string[] errors = { OVERFLOW, INVALID_INPUT, NOT_A_NUMBER };
        // Holds the current operation
        operations currentOperation = operations.NULL;
        // Math operations that take two operands
        enum operations
        {
            ADDITION,
            SUBTRACTION,
            DIVISION,
            MULTIPLICATION,
            POWER,
            NULL // Represents no operation (used to reset the status)
        }

        public MainWindow()
        {
            InitializeComponent();
            angle_unit_button.Content = angleUnitSymbols[angleUnit];
            trig_mode_button.Content = trigModeSymbols[currentTrigMode];
        }

        /// <summary>
        /// Displays the given text to the result box and sets the value of clearNext to true by default (false if specified).
        /// </summary>
        private void showText(string text, bool clear=true)
        {
            try
            {
                if (double.Parse(text) == 0)
                    text = "0";
            }
            catch (Exception)
            {
                showError(INVALID_INPUT);
                return;
            }

            if (text.Length > 30)
                return;
            if (text.Length > 12)
                resultBox.FontSize = 25;
            if (text.Length > 24)
                resultBox.FontSize = 20;

            clearNext = clear;
            resultBox.Text = text;
        }

        /// <summary>
        /// Displays the given text in the result box.
        /// </summary>
        private void showError(string text)
        {
            resultBox.Text = text;
            previousText = null;
            operationCheck = false;
            clearNext = true;
            updateEquationBox("");
            currentOperation = operations.NULL;
            resetFontSize();
        }

        /// <summary>
        /// Updates the equation box with the given equation string.
        /// If append is true then the given text is appended to the existing text in the equation box.
        /// </summary>
        private void updateEquationBox(string equation, bool append=false)
        {
            // Removes pointless decimals from the numbers in the equation
            equation = Regex.Replace(equation, @"(\d+)\.\s", "$1 ");
            
            if (equation.Length > 10)
                equationBox.FontSize = 18;

            if (!append)
                equationBox.Text = equation;
            else
                equationBox.Text += equation;
        }

        /// <summary>
        /// Updates the memory label text with the value in memory variable.
        /// </summary>
        private void updateMemoryLabel()
        {
            memoryLabel.Content = memory.ToString();
            if (memoryLabel.Content.ToString().Length > maxMemoryLabelLength)
                memoryLabel.Content = memoryLabel.Content.ToString().Substring(0, 5) + "...";
        }

        /// <summary>
        /// Parses the text in the text box into a double datatype and returns it.
        /// </summary>
        private double getNumber()
        {
            double number = double.Parse(resultBox.Text);
            return number;
        }

        /// <summary>
        /// Resets the result box font size to defaultSize
        /// </summary>
        private void resetFontSize()
        {
            resultBox.FontSize = defaultFontSize;
        }

        /// <summary>
        /// Calculates the result by solving the previousText and current text in the result
        /// box with the operand in currentOperation.
        /// </summary>
        private void calculateResult()
        {
            if (currentOperation == operations.NULL)
                return;

            double a = double.Parse(previousText);  // first operand
            double b = double.Parse(resultBox.Text); // second operand
            double result;

            switch(currentOperation)
            {
                case operations.DIVISION:
                    result = a / b;
                    break;
                case operations.MULTIPLICATION:
                    result = a * b;
                    break;
                case operations.ADDITION:
                    result = a + b;
                    break;
                case operations.SUBTRACTION:
                    result = a - b;
                    break;
                case operations.POWER:
                    result = Math.Pow(a, b);
                    break;
                default:
                    return;
            }

            if (errors.Contains(resultBox.Text))
                return;

            operationCheck = false;
            previousText = null;
            string equation;
            // If a function button was not clicked during a mathematical operation then the equation box will have the text with the
            // format <operand a> <operation> <operand b as a number> else <operand a> <operation> <func>(<operand b>)
            if (!functionCheck)
                equation = equationBox.Text + b.ToString();
            else
            {
                equation = equationBox.Text;
                functionCheck = false;
            }
            updateEquationBox(equation);
            showText(result.ToString());
            currentOperation = operations.NULL;
            isResult = true;
        }

        /// <summary>
        /// Appends the digit clicked to the text in the text box.
        /// If an ongoing operation has been selected then the text box value is first assigned to previousText variable and then new text 
        /// is appended to the text box after truncating the previous text.
        /// </summary>
        private void numberClick(object sender, RoutedEventArgs e)
        {
            isResult = false;
            Button button = (Button)sender;

            if (resultBox.Text == "0" || errors.Contains(resultBox.Text))
                resultBox.Clear();

            string text;

            if (clearNext)
            {
                resetFontSize();
                text = button.Content.ToString();
                isOldText = false;
            }
            else
                text = resultBox.Text + button.Content.ToString();

            if (!operationCheck && equationBox.Text != "")
                updateEquationBox("");
            showText(text, false);
        }

        /// <summary>
        /// Changes the current angle unit.
        /// </summary>
        private void angle_unit_button_Click(object sender, RoutedEventArgs e)
        {
            List<Angles.units> units = new List<Angles.units>()
            {
                Angles.units.RADIANS,
                Angles.units.DEGREES,
                Angles.units.GRADIANS
            };

            Button button = (Button)sender;
            angleUnit = units.ElementAtOrDefault(units.IndexOf(angleUnit) + 1);
            button.Content = angleUnitSymbols[angleUnit];
        }

        /// <summary>
        /// Changes the trigonometric functions mode to Normal, Hyperbolic or Arc
        /// </summary>
        private void trig_mode_button_Click(object sender, RoutedEventArgs e)
        {
            List<trigModes> modes = new List<trigModes>()
            {
                trigModes.STANDARD,
                trigModes.ARC,
                trigModes.HYPERBOLIC
            };

            Button button = (Button)sender;
            currentTrigMode = modes.ElementAtOrDefault(modes.IndexOf(currentTrigMode) + 1);
            button.Content = trigModeSymbols[currentTrigMode];

            if (currentTrigMode == trigModes.STANDARD)
            {
                sin_button.Content = "sin";
                cos_button.Content = "cos";
                tan_button.Content = "tan";
            }

            if (currentTrigMode == trigModes.HYPERBOLIC)
            {
                sin_button.Content = "sinh";
                cos_button.Content = "cosh";
                tan_button.Content = "tanh";
            }

            if (currentTrigMode == trigModes.ARC)
            {
                sin_button.Content = "asin";
                cos_button.Content = "acos";
                tan_button.Content = "atan";
            }
        }

        /// <summary>
        /// Deals with function button clicks.
        /// </summary>
        private void function(object sender, RoutedEventArgs e)
        {
            if (errors.Contains(resultBox.Text))
                return;
            
            Button button = (Button)sender;
            string buttonText = button.Content.ToString();
            double number = getNumber();
            string equation = "";
            string result = "";

            switch (buttonText)
            {
                // C# doesn't have a Math.factorial()? Who the fuck does that?!
                case "!":
                    if (number < 0 || number.ToString().Contains("."))
                    {
                        showError(INVALID_INPUT);
                        return;
                    }

                    if (number > 3248) // chose this number because the default windows calculator doesn't go beyond this number
                    {
                        showError(OVERFLOW);
                        return;
                    }
                    double res = 1;
                    if (number == 1 || number == 0)
                        result = res.ToString();
                    else
                    {
                        for (int i = 2; i <= number; i++)
                        {
                            res *= i;
                        }
                    }
                    equation = "fact(" + number.ToString() + ")";
                    result = res.ToString();
                    break;

                case "ln":
                    equation = "ln(" + number + ")";
                    result = Math.Log(number).ToString();
                    break;

                case "log":
                    equation = "log(" + number + ")";
                    result = Math.Log10(number).ToString();
                    break;

                case "√":
                    equation = "√(" + number + ")";
                    result = Math.Sqrt(number).ToString();
                    break;

                case "-n":
                    equation = "negate(" + number + ")";
                    result = decimal.Negate((decimal)number).ToString();
                    break;
            }

            if (operationCheck)
            {
                equation = equationBox.Text + equation;
                functionCheck = true;
            }

            updateEquationBox(equation);
            showText(result);
        }

        /// <summary>
        /// Deals with trigonometric function button clicks.
        /// </summary>
        private void trigFunction(object sender, RoutedEventArgs e)
        {
            if (errors.Contains(resultBox.Text))
                return;
            
            Button button = (Button)sender;
            string buttonText = button.Content.ToString();
            string equation = "";
            string result = "";
            double number = getNumber();

            switch (currentTrigMode)
            {
                // Standard trig functions
                case trigModes.STANDARD:
                    double radianAngle = Angles.Converter.radians(number, angleUnit);
                    switch (buttonText)
                    {
                        case "sin":
                            equation = "sin(" + number.ToString() + ")";
                            result = Math.Sin(radianAngle).ToString();
                            break;

                        case "cos":
                            equation = "cos(" + number.ToString() + ")";
                            result = Math.Cos(radianAngle).ToString();
                            break;

                        case "tan":
                            equation = "tan(" + number.ToString() + ")";
                            result = Math.Tan(radianAngle).ToString();
                            break;
                    }
                    break;

                // Hyperbolic trig functions
                case trigModes.HYPERBOLIC:
                    switch(buttonText)
                    {
                        case "sinh":
                            equation = "sinh(" + number + ")";
                            result = Math.Sinh(number).ToString();
                            break;

                        case "cosh":
                            equation = "cosh(" + number + ")";
                            result = Math.Cosh(number).ToString();
                            break;

                        case "tanh":
                            equation = "tanh(" + number + ")";
                            result = Math.Tanh(number).ToString();
                            break;
                    }
                    break;

                // Arc trig functions
                case trigModes.ARC:
                    switch (buttonText)
                    {
                        case "asin":
                            equation = "asin(" + number + ")";
                            result = Math.Asin(number).ToString();
                            break;

                        case "acos":
                            equation = "acos(" + number + ")";
                            result = Math.Acos(number).ToString();
                            break;

                        case "atan":
                            equation = "atan(" + number + ")";
                            result = Math.Atan(number).ToString();
                            break;
                    }
                    break;
            }

            // We need to convert the result to the given angle unit if arc trig functions are used
            if (currentTrigMode == trigModes.ARC)
            {
                switch(angleUnit)
                {
                    case Angles.units.DEGREES:
                        result = Angles.Converter.degrees(double.Parse(result), Angles.units.RADIANS).ToString();
                        break;
                    case Angles.units.GRADIANS:
                        result = Angles.Converter.gradians(double.Parse(result), Angles.units.RADIANS).ToString();
                        break;
                    default:  // 'result' is in radians by default
                        break;
                }
            }

            if (operationCheck)
            {
                equation = equationBox.Text + equation;
                functionCheck = true;
            }

            updateEquationBox(equation);
            showText(result);
        }

        /// <summary>
        /// Deals with double operand function clicks.
        /// </summary>
        private void doubleOperandFunction(object sender, RoutedEventArgs e)
        {
            if (errors.Contains(resultBox.Text))
                return;

            if (operationCheck && !isOldText)
                calculateResult();

            Button button = (Button)sender;

            operationCheck = true;
            previousText = resultBox.Text;
            string buttonText = button.Content.ToString();
            string equation = previousText + " " + buttonText + " ";
            switch(buttonText)
            {
                case "/":
                    currentOperation = operations.DIVISION;
                    break;
                case "x":
                    currentOperation = operations.MULTIPLICATION;
                    break;
                case "-":
                    currentOperation = operations.SUBTRACTION;
                    break;
                case "+":
                    currentOperation = operations.ADDITION;
                    break;
                case "^":
                    currentOperation = operations.POWER;
                    break;
            }
            updateEquationBox(equation);
            resetFontSize();
            showText(resultBox.Text);
            isOldText = true;
        }

        /// <summary>
        /// Appends a decimal point to the number in the result box on click,
        /// if the number already has a decimal point then no action is taken
        /// </summary>
        private void decimal_button_Click(object sender, RoutedEventArgs e)
        {
            if (!resultBox.Text.Contains("."))
            {
                string text = resultBox.Text += ".";
                showText(text, false);
            }
        }

        private void pi_button_Click(object sender, RoutedEventArgs e)
        {
            if (!operationCheck)
                updateEquationBox("");
            showText(Math.PI.ToString());
            isResult = true; // Constants cannot be changed
        }

        private void e_button_Click(object sender, RoutedEventArgs e)
        {
            if (!operationCheck)
                updateEquationBox("");
            showText(Math.E.ToString());
            isResult = true; // Constants cannot be changed
        }

        private void madd_button_Click(object sender, RoutedEventArgs e)
        {
            if (errors.Contains(resultBox.Text))
                return;
            memory += getNumber();
            updateMemoryLabel();
        }

        private void msub_button_Click(object sender, RoutedEventArgs e)
        {
            if (errors.Contains(resultBox.Text))
                return;
            memory -= getNumber();
            updateMemoryLabel();
        }

        private void mc_button_Click(object sender, RoutedEventArgs e)
        {
            memory = 0;
            updateMemoryLabel();
        }

        private void mr_button_Click(object sender, RoutedEventArgs e)
        {
            showText(memory.ToString());
            if (!operationCheck)
                updateEquationBox("");
        }

        private void clear_button_Click(object sender, RoutedEventArgs e)
        {
            resultBox.Text = "0";
            operationCheck = false;
            previousText = null;
            updateEquationBox("");
            resetFontSize();
        }

        private void clr_entry_button_Click(object sender, RoutedEventArgs e)
        {
            resultBox.Text = "0";
            resetFontSize();
        }

        private void equals_button_Click(object sender, RoutedEventArgs e)
        {
            calculateResult();
        }

        private void about_button_Click(object sender, RoutedEventArgs e)
        {
            AboutBox aboutForm = new AboutBox();
            aboutForm.ShowDialog();
        }

        // Copy
        private void copy_button_Click(object sender, RoutedEventArgs e)
        {
            if (errors.Contains(resultBox.Text))
                return;

            Clipboard.SetData(DataFormats.UnicodeText, resultBox.Text);
        }
        
        // Paste
        private void paste_button_Click(object sender, RoutedEventArgs e)
        {
            object clipboardData = Clipboard.GetData(DataFormats.UnicodeText);
            if (clipboardData != null)
            {
                string data = clipboardData.ToString();
                showText(data.ToString());
            }
            else
                return;
        }

        private void back_button_Click(object sender, RoutedEventArgs e)
        {
            if (isResult)
                return;

            string text;

            if (resultBox.Text.Length == 1)
                text = "0";
            else
                text = resultBox.Text.Substring(0, resultBox.Text.Length - 1);

            showText(text, false);

        }

       // private void keyboardInput(object sender, System.Windows.Input.KeyEventArgs e)
       // {
       //     string keyString = e.Key.ToString();
       //     //MessageBox.Show(keyString);
       //     Dictionary<string, Button> buttonShortcuts = new Dictionary<string, Button>()
       //     {
       //         { "D0", zero_button },
       //         { "D1", one_button },
       //         { "D2", two_button },
       //         { "D3", three_button },
       //         { "D4", four_button },
       //         { "D5", five_button },
       //         { "D6", six_button },
       //         { "D7", seven_button },
       //         { "D8", eight_button },
       //         { "D9", nine_button },
       //         { "P", pi_button },
       //         { "E", e_button },
       //         { "S", sin_button },
       //         { "C", cos_button },
       //         { "T", tan_button },
       //         { "Return", equals_button },
       //         { "Back", back_button }
       //     };
       //     string[] numberButtons = 
       //     {
       //         "D0",
       //         "D1",
       //         "D2",
       //         "D3",
       //         "D4",
       //         "D5",
       //         "D6",
       //         "D7",
       //         "D8",
       //         "D9",
       //     };

       //     if (numberButtons.Contains(keyString))
       //     numberClick(buttonShortcuts[keyString], null);
       //}
    }
}
