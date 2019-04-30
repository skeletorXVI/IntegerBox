using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

/// <summary>
/// A simple IntegerBox for WPF that supports Maximum, Minimum and (Left)Pad.
/// </summary>
public class IntegerBox : UserControl
{
    /// <summary>
    /// The dependency property for the integer value entered in the TextBox.
    /// </summary>
    public static readonly DependencyProperty IntegerValueProperty = DependencyProperty.Register(
        nameof(IntegerValue),
        typeof(int?),
        typeof(IntegerBox),
        new FrameworkPropertyMetadata(default(int?),
            FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
            IntegerValuePropertyChangedCallback,
            CoerceIntegerValueCallback));

    /// <summary>
    /// The dependency property for the left pad.
    /// </summary>
    public static readonly DependencyProperty PadProperty = DependencyProperty.Register(
        nameof(Pad),
        typeof(int),
        typeof(IntegerBox),
        new FrameworkPropertyMetadata(default(int),
            FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
            PadPropertyChangedCallback));

    /// <summary>
    /// The dependency property for the minimum value.
    /// </summary>
    public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register(
        nameof(Minimum),
        typeof(int?),
        typeof(IntegerBox),
        new FrameworkPropertyMetadata(default(int?),
            FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
            MinimumPropertyChangedCallback,
            CoerceMinimumPropertyCallback));

    /// <summary>
    /// The dependency property for the maximum value.
    /// </summary>
    public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register(
        nameof(Maximum),
        typeof(int?),
        typeof(IntegerBox),
        new FrameworkPropertyMetadata(default(int?),
            FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
            MaximumPropertyChangedCallback,
            CoerceMaximumPropertyCallback));

    /// <summary>
    /// The integer value entered through the IntegerBox.
    /// </summary>
    /// <remarks>
    /// The value can be <value>null</value> if <see cref="Pad"/> is <value>0</value>.
    /// </remarks>
    public int? IntegerValue
    {
        get => (int?) GetValue(IntegerValueProperty);
        set => SetValue(IntegerValueProperty, value);
    }

    /// <summary>
    /// The left pad for the TextBox. Determines the minimum number of digits displayed in the TextBox.
    /// </summary>
    /// <remarks>
    /// The Pad property is ignored while the IntegerBox or it's children have keyboard focus.
    /// This makes the TextBox easier to use.
    /// </remarks>
    public int Pad
    {
        get => (int) GetValue(PadProperty);
        set => SetValue(PadProperty, value);
    }

    /// <summary>
    /// The minimum value that can be entered. <value>null</value> equals infinite.
    /// </summary>
    public int? Minimum
    {
        get => (int?) GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }
    
    /// <summary>
    /// The maximum value that can be entered. <value>null</value> equals infinite.
    /// </summary>
    public int? Maximum
    {
        get => (int?) GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    public IntegerBox()
    {
        OnApplyTemplate();
    }

    /// <summary>
    /// Validate that a string is a integer number.
    /// </summary>
    /// <param name="text">The string to test.</param>
    /// <returns>
    /// <value>true</value> if the string is a integer number.
    /// </returns>
    /// <remarks>
    /// The method does not range check. The number can be out of bounds.
    /// </remarks>
    private static bool ValidateText(string text)
    {
        return new Regex("^-?[0-9]*$").IsMatch(text);
    }

    public override void OnApplyTemplate()
    {
        // Attempt to find the TextBox internally used for input
        if (Template?.FindName("PART_TextBox", this) is TextBox partTextBox)
        {
            partTextBox.PreviewTextInput += OnPreviewTextInput;
//            partTextBox.PreviewKeyDown += OnPreviewKeyDown;
            partTextBox.LostFocus += OnLostFocus;
            partTextBox.TextChanged += OnTextChanged;
            DataObject.AddPastingHandler(partTextBox, OnPaste);
            RefreshText();
        }

        base.OnApplyTemplate();
    }

    /// <summary>
    /// Handle <see cref="IntegerValue"/> value changed. Refreshes Text.
    /// </summary>
    private static void IntegerValuePropertyChangedCallback(DependencyObject d,
        DependencyPropertyChangedEventArgs e)
    {
        var integerBox = (IntegerBox) d;
        integerBox.RefreshText();
    }

    /// <summary>
    /// Handle <see cref="Minimum"/> value changed. Coerces <see cref="IntegerValue"/>.
    /// </summary>
    private static void MinimumPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var integerBox = (IntegerBox) d;

        integerBox.CoerceValue(IntegerValueProperty);
    }

    /// <summary>
    /// Handle <see cref="Maximum"/> value changed. Coerces <see cref="IntegerValue"/>.
    /// </summary>
    private static void MaximumPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var integerBox = (IntegerBox) d;

        integerBox.CoerceValue(IntegerValueProperty);
    }

    /// <summary>
    /// Handle <see cref="Pad"/> value changed. Refreshes Text of the TextBox.
    /// </summary>
    private static void PadPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var integerBox = (IntegerBox) d;
        integerBox.RefreshText();
    }

    /// <summary>
    /// Coerces <see cref="IntegerValue"/>. Ensures that the integer value stays within bounds.
    /// </summary>
    /// <param name="d">The dependency object.</param>
    /// <param name="baseValue">The input value.</param>
    /// <returns>Returns the coerced value.</returns>
    private static object CoerceIntegerValueCallback(DependencyObject d, object baseValue)
    {
        var integerBox = (IntegerBox) d;
        var value = (int?) baseValue;

        if (value == null) return null;
        if (integerBox.Minimum != null && integerBox.Minimum > value) value = integerBox.Minimum;
        if (integerBox.Maximum != null && integerBox.Maximum < value) value = integerBox.Maximum;

        return value;
    }

    /// <summary>
    /// Coerces <see cref="Minimum"/>. Ensures that the minimum value stays below the maximum.
    /// </summary>
    /// <param name="d">The dependency object.</param>
    /// <param name="baseValue">The input value.</param>
    /// <returns>Returns the coerced value.</returns>
    private static object CoerceMinimumPropertyCallback(DependencyObject d, object baseValue)
    {
        var integerBox = (IntegerBox) d;
        var value = (int?) baseValue;

        if (value == null) return null;
        if (integerBox.Maximum != null && integerBox.Maximum < value) value = integerBox.Maximum;

        return value;
    }

    /// <summary>
    /// Coerces <see cref="Maximum"/>. Ensures that the minimum value stays above the minimum.
    /// </summary>
    /// <param name="d">The dependency object.</param>
    /// <param name="baseValue">The input value.</param>
    /// <returns>Returns the coerced value.</returns>
    private static object CoerceMaximumPropertyCallback(DependencyObject d, object baseValue)
    {
        var integerBox = (IntegerBox) d;
        var value = (int?) baseValue;

        if (value == null) return null;
        if (integerBox.Minimum != null && integerBox.Minimum > value) value = integerBox.Minimum;

        return value;
    }

    /// <summary>
    /// Handle <see cref="TextBox.TextChanged"/> event of the TextBox used internally for the input.
    /// </summary>
    private void OnTextChanged(object sender, TextChangedEventArgs e)
    {
        if (!(sender is TextBox textBox)) return;

        UpdateValue(textBox.Text);
    }

    /// <summary>
    /// Handle OnPaste event of the TextBox used internally for the input.
    /// Cancels the operation if the inserted text would result in the updated text not being a valid number.
    /// </summary>
    private void OnPaste(object sender, DataObjectPastingEventArgs e)
    {
        if (!(sender is TextBox textBox)) return;
        
        if (!e.SourceDataObject.GetDataPresent(DataFormats.Text))
        {
            return;
        }

        var clipboard = Convert.ToString(e.DataObject.GetData(DataFormats.Text));
        
        var text = textBox.SelectionLength > 0
            ? textBox.Text.Remove(textBox.SelectionStart, textBox.SelectionLength)
            : textBox.Text;
        
        text = text.Insert(textBox.CaretIndex, clipboard);
        
        if (!ValidateText(text))
        {
            e.CancelCommand();
        }
    }

    /// <summary>
    /// Handle PreviewTextInput event of the TextBox used internally for the input.
    /// Cancels the operation if the inserted text would result in the updated text not being a valid number.
    /// </summary>
    private void OnPreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        if (!(sender is TextBox textBox)) return;

        var text = textBox.SelectionLength > 0
            ? textBox.Text.Remove(textBox.SelectionStart, textBox.SelectionLength)
            : textBox.Text;

        text = text.Insert(textBox.CaretIndex, e.Text);
        e.Handled = !ValidateText(text);
    }

    /// <summary>
    /// Handle LostFocus event of the TextBox used internally for the input.
    /// Refreshes the text.
    /// </summary>
    private void OnLostFocus(object sender, RoutedEventArgs e)
    {
        if (!(sender is TextBox _)) return;
        RefreshText();
    }

    /// <summary>
    /// Refreshes the text of the TextBox used internally for the input.
    /// </summary>
    private void RefreshText()
    {
        if (Template?.FindName("PART_TextBox", this) is TextBox partTextBox)
        {
            // If the integer value is null set the text to empty
            if (!IntegerValue.HasValue)
            {
                partTextBox.Text = "";
                return;
            }

            var number = Math.Abs(IntegerValue.Value).ToString();
            
            // If the IntegerBox or a child has keyboard focus only refresh text without pad
            if (IsKeyboardFocusWithin)
            {
                var caretIndex = partTextBox.CaretIndex;
                partTextBox.Text = number;
                if (caretIndex > 0 && number.Length <= Pad)
                {
                    partTextBox.CaretIndex = caretIndex;
                }
            }
            // Set text with pad
            else
            {
                partTextBox.Text = IntegerValue.Value < 0
                    ? "-" + number.PadLeft(Pad - 1, '0')
                    : number.PadLeft(Pad, '0');
            }
        }
    }

    /// <summary>
    /// Update <see cref="IntegerValue"/> by string.
    /// </summary>
    private void UpdateValue(string text)
    {
        // If parsable just use value
        if (int.TryParse(text, out var integer))
        {
            IntegerValue = integer;
        }
        // If not parsable to int, check if input is a number and just overflows 
        else if (Regex.IsMatch(text, "^-?[0-9]+$"))
        {
            IntegerValue = text[0] == '-'
                ? int.MinValue
                : int.MaxValue;
        }
        else
        {
            IntegerValue = null;
        }
        RefreshText();
    }
}