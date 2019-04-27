using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace IntegerBox
{
    public class IntegerBox : UserControl
    {
        public static readonly DependencyProperty IntegerValueProperty = DependencyProperty.Register(
            nameof(IntegerValue),
            typeof(int?),
            typeof(IntegerBox),
            new FrameworkPropertyMetadata(default(int?),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                IntegerValuePropertyChangedCallback,
                CoerceIntegerValueCallback));

        public static readonly DependencyProperty PadProperty = DependencyProperty.Register(
            nameof(Pad),
            typeof(int),
            typeof(IntegerBox),
            new FrameworkPropertyMetadata(default(int),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                PadPropertyChangedCallback));

        public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register(
            nameof(Minimum),
            typeof(int?),
            typeof(IntegerBox),
            new FrameworkPropertyMetadata(default(int?),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                MinimumPropertyChangedCallback,
                CoerceMinimumPropertyCallback));

        public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register(
            nameof(Maximum),
            typeof(int?),
            typeof(IntegerBox),
            new FrameworkPropertyMetadata(default(int?),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                MaximumPropertyChangedCallback,
                CoerceMaximumPropertyCallback));

        public int? IntegerValue
        {
            get => (int?) GetValue(IntegerValueProperty);
            set => SetValue(IntegerValueProperty, value);
        }

        public int Pad
        {
            get => (int) GetValue(PadProperty);
            set => SetValue(PadProperty, value);
        }

        public int? Minimum
        {
            get => (int?) GetValue(MinimumProperty);
            set => SetValue(MinimumProperty, value);
        }

        public int? Maximum
        {
            get => (int?) GetValue(MaximumProperty);
            set => SetValue(MaximumProperty, value);
        }

        public IntegerBox()
        {
            OnApplyTemplate();
        }

        private static bool ValidateText(string text)
        {
            return new Regex("^-?[0-9]*$").IsMatch(text);
        }

        public override void OnApplyTemplate()
        {
            if (Template?.FindName("PART_TextBox", this) is TextBox partTextBox)
            {
                partTextBox.PreviewTextInput += OnPreviewTextInput;
                partTextBox.PreviewKeyDown += OnPreviewKeyDown;
                partTextBox.LostFocus += OnLostFocus;
                partTextBox.TextChanged += OnTextChanged;
                DataObject.AddPastingHandler(partTextBox, OnPaste);
                partTextBox.Text = IntegerValue?.ToString() ?? "";
            }

            base.OnApplyTemplate();
        }

        private static void IntegerValuePropertyChangedCallback(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            var integerBox = (IntegerBox) d;
            integerBox.RefreshText();
        }

        private static void MinimumPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var integerBox = (IntegerBox) d;

            if (e.NewValue != null && integerBox.IntegerValue < (int) e.NewValue)
            {
                integerBox.IntegerValue = (int) e.NewValue;
                integerBox.CoerceValue(IntegerValueProperty);
            }
        }

        private static void MaximumPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var integerBox = (IntegerBox) d;

            if (e.NewValue != null && integerBox.IntegerValue > (int) e.NewValue)
            {
                integerBox.IntegerValue = (int) e.NewValue;
                integerBox.CoerceValue(IntegerValueProperty);
            }
        }

        private static object CoerceIntegerValueCallback(DependencyObject d, object baseValue)
        {
            var integerBox = (IntegerBox) d;
            var value = (int?) baseValue;

            if (value == null) return null;
            if (integerBox.Minimum != null && integerBox.Minimum > value) value = integerBox.Minimum;
            if (integerBox.Maximum != null && integerBox.Maximum < value) value = integerBox.Maximum;

            return value;
        }

        private static object CoerceMinimumPropertyCallback(DependencyObject d, object baseValue)
        {
            var integerBox = (IntegerBox) d;
            var value = (int?) baseValue;

            if (value == null) return null;
            if (integerBox.Maximum != null && integerBox.Maximum < value) value = integerBox.Maximum;

            return value;
        }

        private static object CoerceMaximumPropertyCallback(DependencyObject d, object baseValue)
        {
            var integerBox = (IntegerBox) d;
            var value = (int?) baseValue;

            if (value == null) return null;
            if (integerBox.Minimum != null && integerBox.Minimum > value) value = integerBox.Minimum;

            return value;
        }

        private static void PadPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var integerBox = (IntegerBox) d;
            integerBox.RefreshText();
        }

        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (!(sender is TextBox textBox)) return;
            IntegerValue = int.TryParse(textBox.Text, out var integer) ? (int?) integer : null;
        }

        private void OnPaste(object sender, DataObjectPastingEventArgs e)
        {
            if (!e.SourceDataObject.GetDataPresent(DataFormats.Text))
            {
                return;
            }

            var text = Convert.ToString(e.DataObject.GetData(DataFormats.Text));
            if (!ValidateText(text))
            {
                e.CancelCommand();
            }
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!(sender is TextBox textBox)) return;

            switch (e.Key)
            {
                // Handle the Backspace key
                case Key.Back:
                    if (textBox.Text.Length != 0)
                    {
                        var caretIndex = textBox.CaretIndex;
                        if (textBox.SelectionLength == 0)
                        {
                            if (textBox.SelectionStart > 0)
                            {
                                textBox.Text = textBox.Text.Remove(textBox.SelectionStart - 1, 1);
                                textBox.CaretIndex = caretIndex - 1;
                            }
                        }
                        else
                        {
                            textBox.Text = textBox.Text.Remove(textBox.SelectionStart, textBox.SelectionLength);
                            textBox.CaretIndex = caretIndex;
                        }
                    }

                    break;
                // Handle the Delete key
                case Key.Delete:
                    if (textBox.Text.Length != 0)
                    {
                        var caretIndex = textBox.CaretIndex;
                        textBox.Text = textBox.Text
                            .Remove(textBox.SelectionStart, textBox.SelectionLength == 0 ? 1 : textBox.SelectionLength)
                            .PadLeft(Pad, '0');
                        textBox.CaretIndex = caretIndex;
                    }

                    break;
                default:
                    return;
            }

            e.Handled = true;
        }

        private void OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!(sender is TextBox textBox)) return;

            var text = textBox.SelectionLength > 0
                ? textBox.Text.Remove(textBox.SelectionStart, textBox.SelectionLength)
                : textBox.Text;

            text = text.Insert(textBox.CaretIndex, e.Text);
            e.Handled = !ValidateText(text);
        }

        private void OnLostFocus(object sender, RoutedEventArgs e)
        {
            if (!(sender is TextBox textBox)) return;

            if (textBox.Text == "-")
            {
                textBox.Text = "";
            }
        }

        private void RefreshText()
        {
            if (Template?.FindName("PART_TextBox", this) is TextBox partTextBox)
            {
                if (!IntegerValue.HasValue)
                {
                    partTextBox.Text = "";
                    return;
                }

                var number = Math.Abs(IntegerValue.Value).ToString();
                partTextBox.Text = IntegerValue.Value < 0
                    ? "-" + number.PadLeft(Pad - 1, '0')
                    : number.PadLeft(Pad, '0');

            }
        }
    }
}