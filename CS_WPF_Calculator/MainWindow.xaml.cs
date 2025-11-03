using System.Windows;
using System.Windows.Controls;

namespace Calculator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        double lastNumber, result, newNumber;
        SelectedOperator selectedOperator;

        public MainWindow()
        {
            InitializeComponent();


            acButton.Click += AcButton_Click;
            negativeButton.Click += NegativeButton_Click;
            percentageButton.Click += PercentageButton_Click;
            equalButton.Click += EqualButton_Click;
        }

        private void EqualButton_Click(object sender, RoutedEventArgs e)
        {

            if (double.TryParse(resultLabel.Content.ToString(), out newNumber))
            {
                switch (selectedOperator)
                {
                    case SelectedOperator.Addition:
                        result = SimpleMath.Add(lastNumber, newNumber);
                        break;
                    case SelectedOperator.Sustraction:
                        result = SimpleMath.Sustraction(lastNumber, newNumber);
                        break;
                    case SelectedOperator.Multiplication:
                        result = SimpleMath.Multiply(lastNumber, newNumber);
                        break;
                    case SelectedOperator.Division:
                        result = SimpleMath.Division(lastNumber, newNumber);
                        break;
                }
            }

            resultLabel.Content = result.ToString();
        }

        private void PercentageButton_Click(object sender, RoutedEventArgs e)
        {
            double tempNumber;
            if (double.TryParse(resultLabel.Content.ToString(), out tempNumber))
            {
                tempNumber = (tempNumber / 100);
                if (lastNumber != 0)
                    tempNumber *= lastNumber;
                resultLabel.Content = tempNumber.ToString();
            }
        }

        private void NegativeButton_Click(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(resultLabel.Content.ToString(), out lastNumber))
            {
                lastNumber = lastNumber * -1;
                resultLabel.Content = lastNumber.ToString();
            }
        }

        private void AcButton_Click(object sender, RoutedEventArgs e)
        {
            resultLabel.Content = "0";
        }
        private void operationButton_click(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(resultLabel.Content.ToString(), out lastNumber))
            {

                //resultLabel.Content = $"{resultLabel.Content}{(sender as Button).Content}";
                resultLabel.Content = "0";
            }

            if (sender == multiplicationButton)
                selectedOperator = SelectedOperator.Multiplication;
            if (sender == divisionButton)
                selectedOperator = SelectedOperator.Division;
            if (sender == plusButton)
                selectedOperator = SelectedOperator.Addition;
            if (sender == minusButton)
                selectedOperator = SelectedOperator.Sustraction;
        }

        private void pointButton_click(object sender, RoutedEventArgs e)
        {
            if (resultLabel.Content.ToString().Contains("."))
            {
                // do nothing
            }
            else
            {
                resultLabel.Content = $"{resultLabel.Content}.";
            }
        }

        private void numberButton_Click(object sender, RoutedEventArgs e)
        {
            int selectedValue = int.Parse((sender as Button).Content.ToString());

            if (resultLabel.Content.ToString() == "0")
            {
                resultLabel.Content = $"{selectedValue}";
            }
            else
            {
                // 在标签的现有内容后面追加一个 "7"
                // resultLabel.Content = resultLabel.Content + "7";
                resultLabel.Content = $"{resultLabel.Content}{selectedValue}";
            }
        }

        //private void numberButton_Click(object sender, RoutedEventArgs e)
        //{
        //    var selectedButton = sender as Button;
        //    if (resultLabel.Content.ToString() == "0")
        //    {
        //        resultLabel.Content = $"{selectedButton.Content}";
        //    }
        //    else
        //    {
        //        resultLabel.Content = $"{resultLabel.Content}{selectedButton.Content}";
        //    }
        //}

        public enum SelectedOperator
        {
            Addition,
            Sustraction,
            Multiplication,
            Division
        }

        public class SimpleMath
        {
            public static double Add(double n1, double n2)
            {
                return n1 + n2;
            }

            public static double Sustraction(double n1, double n2)
            {
                return n1 - n2;
            }
            public static double Multiply(double n1, double n2)
            {
                return n1 * n2;
            }

            public static double Division(double n1, double n2)
            {
                if (n2 == 0)
                {
                    MessageBox.Show(
                        "Division by 0 is not supported",
                        "Wrong operation",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);

                    return 0;
                }

                return n1 / n2;
            }

        }

    }
}