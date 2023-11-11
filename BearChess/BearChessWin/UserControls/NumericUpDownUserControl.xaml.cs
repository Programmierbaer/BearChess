using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace www.SoLaNoSoft.com.BearChessWin
{
    /// <summary>
    /// Interaktionslogik für NumericUpDownUserControl.xaml
    /// </summary>
    public partial class NumericUpDownUserControl : UserControl
    {
        public event EventHandler<int> ValueChanged;

        private int _maxValue;
        private int _minValue;

        public NumericUpDownUserControl()
        {
            InitializeComponent();
            MaxValue = int.MaxValue - 1;
            MinValue = int.MinValue + 1;
            Value = 0;
            textBlockNumber.Text = "0";
            Rotate = false;
        }

        public int MaxValue
        {
            get => _maxValue;
            set
            {
                _maxValue = value < int.MaxValue ? value : int.MaxValue - 1;
                scrollBarNumber.Maximum = value + 1;
                if (scrollBarNumber.Maximum < int.MaxValue && scrollBarNumber.Minimum > int.MinValue)
                {
                    scrollBarNumber.ToolTip = $"{_minValue} to {_maxValue}";
                    textBlockNumber.ToolTip = $"{_minValue} to {_maxValue}";
                }
                else
                {
                    scrollBarNumber.ToolTip = null;
                    textBlockNumber.ToolTip = null;
                }
            }
        }

        public int MinValue
        {
            get => _minValue;
            set
            {
                _minValue = value > int.MinValue ? value : int.MinValue;
                scrollBarNumber.Minimum = value - 1;
                if (scrollBarNumber.Maximum < int.MaxValue && scrollBarNumber.Minimum > int.MinValue)
                {
                    scrollBarNumber.ToolTip = $"{_minValue} to {_maxValue}";
                    textBlockNumber.ToolTip = $"{_minValue} to {_maxValue}";
                }
                else
                {
                    scrollBarNumber.ToolTip = null;
                    textBlockNumber.ToolTip = null;
                }
            }
        }

        public bool Rotate { get; set; }

        public int Value
        {
            get => (int)scrollBarNumber.Value;
            set => scrollBarNumber.Value = value;
        }

        public string TextWidthProperty
        {
            get => (string)GetValue(TextWidthPropertyProperty);
            set => SetValue(TextWidthPropertyProperty, value);
        }
        public static readonly DependencyProperty TextWidthPropertyProperty =
            DependencyProperty.Register("TextWidthProperty", typeof(string), typeof(NumericUpDownUserControl),
                new PropertyMetadata("20", TextValueChanged));



        private static void TextValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is NumericUpDownUserControl control && double.TryParse(control.TextWidthProperty, out double newValue))
            {
                control.TextBoxWidth = newValue;
            }
        }
        protected double TextBoxWidth
        {
            set => textBlockNumber.Width = value;
        }

        private void ScrollBarMajor_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int newValue = (int)e.NewValue;
            if (newValue > MaxValue)
            {
                scrollBarNumber.Value = Rotate ? MinValue : MaxValue;
            }
            else
            {
                if (newValue < MinValue)
                {
                    scrollBarNumber.Value = Rotate ? MaxValue : MinValue;
                }
                else
                {
                    scrollBarNumber.Value = newValue;
                }
            }
            textBlockNumber.Text = scrollBarNumber.Value.ToString(CultureInfo.InvariantCulture);
            OnValueChanged(Value);
        }

        private void TextBlockNumber_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (scrollBarNumber == null)
            {
                return;
            }
            string currentValue =  Value.ToString(CultureInfo.InvariantCulture);
            if (textBlockNumber.Text.Equals(currentValue))
            {
                return;
            }

            if (int.TryParse(textBlockNumber.Text, out int newValue))
            {
                Value = newValue;
            }
            else
            {
                textBlockNumber.Text = currentValue;
            }
        }


        protected virtual void OnValueChanged(int e)
        {
            ValueChanged?.Invoke(this, e);
        }

        private void TextBlockNumber_OnLostFocus(object sender, RoutedEventArgs e)
        {
            if (scrollBarNumber == null)
            {
                return;
            }
            string currentValue = Value.ToString(CultureInfo.InvariantCulture);
            if (textBlockNumber.Text.Equals(currentValue))
            {
                return;
            }

            if (int.TryParse(textBlockNumber.Text, out int newValue))
            {
                Value = newValue;
            }
            else
            {
                textBlockNumber.Text = currentValue;
            }
        }
    }
}
