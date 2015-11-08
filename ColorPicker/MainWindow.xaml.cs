using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

/*
    Author: Andreas Lill
    License: Apache License 2.0

    http://www.apache.org/licenses/LICENSE-2.0
*/
namespace ColorPicker
{
    public delegate void ApplyEventHandler(object sender, EventArgs e);

    public partial class MainWindow : Window
    {
        // Event handler for Apply button.
        public event ApplyEventHandler Apply;

        private ColorHSB colorHSB = new ColorHSB();
        private Color selectedColor;

        private bool isMovingColorPick;
        private bool isMovingGradient;
        private bool ignoreTextChangeEvent;

        public MainWindow()
        {
            InitializeComponent();
            LoadColor(Colors.Red);
        }


        /// <summary>
        /// Load the pre-selected color into all controls.
        /// </summary>
        /// <param name="color">The color to load.</param>
        public void LoadColor(Color color)
        {
            colorHSB = ColorHSB.RGBtoHSB(color);

            Canvas.SetLeft(ColorPickPointer, Canvas.GetLeft(ColorMapImage) + (colorHSB.Hue / 360) * 256);
            Canvas.SetTop(ColorPickPointer, Canvas.GetTop(ColorMapImage) + (256 - colorHSB.Saturation * 256));
            Canvas.SetTop(GradientPointer, Canvas.GetTop(GradientRectangle) + (256 - colorHSB.Brightness * 256));

            ConstraintColorPickPointer();
            ConstraintGradientPointer();

            UpdateGradient(color);
            UpdateColor();
            UpdateInfo();
        }

        /// <summary>
        /// Get the selected color as brush.
        /// </summary>
        /// <returns>Selected brush</returns>
        public Brush GetBrush()
        {
            return new SolidColorBrush(selectedColor);
        }

        /// <summary>
        /// Get the selected color.
        /// </summary>
        /// <returns>Selected color</returns>
        public Color GetColor()
        {
            return selectedColor;
        }

        /// <summary>
        /// Get the hex representation of the color as string.
        /// </summary>
        /// <returns>The hex string</returns>
        public override string ToString()
        {
            return new BrushConverter().ConvertToString(selectedColor).Remove(1, 2);
        }

        /// <summary>
        /// Event handler for Apply button.
        /// </summary>
        /// <param name="e">Event arguments</param>
        protected virtual void ApplyEvent(EventArgs e)
        {
            if (Apply != null)
            {
                Apply(this, e);
            }
        }

        private void ColorMapImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                // Set color pick pointer position.
                Canvas.SetLeft(ColorPickPointer, Mouse.GetPosition(ColorPickerCanvas).X);
                Canvas.SetTop(ColorPickPointer, Mouse.GetPosition(ColorPickerCanvas).Y);

                // Constraint pointer to image.
                ConstraintColorPickPointer();

                UpdateColorPicker();
                UpdateColor();
                UpdateInfo();

                // Capture mouse events for this control.
                isMovingColorPick = true;
                Mouse.Capture(ColorMapImage);
            }
        }

        private void ColorMapImage_MouseUp(object sender, MouseButtonEventArgs e)
        {
            // Release mouse capture.
            isMovingColorPick = false;
            Mouse.Capture(null);
        }

        private void ColorMapImage_MouseMove(object sender, MouseEventArgs e)
        {
            if (isMovingColorPick && e.LeftButton == MouseButtonState.Pressed)
            {
                // Update all information when moving mouse.
                Canvas.SetLeft(ColorPickPointer, Mouse.GetPosition(ColorPickerCanvas).X);
                Canvas.SetTop(ColorPickPointer, Mouse.GetPosition(ColorPickerCanvas).Y);

                ConstraintColorPickPointer();

                UpdateColorPicker();
                UpdateColor();
                UpdateInfo();
            }
        }

        private void GradientRectangle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                // Set gradient pointer position.
                Canvas.SetTop(GradientPointer, Mouse.GetPosition(ColorPickerCanvas).Y);

                // Constraint pointer to image.
                ConstraintGradientPointer();

                UpdateGradientPicker();
                UpdateColor();
                UpdateInfo();

                // Capture mouse events for this control.
                isMovingGradient = true;
                Mouse.Capture(GradientRectangle);
            }
        }

        private void GradientRectangle_MouseUp(object sender, MouseButtonEventArgs e)
        {
            // Release mouse capture.
            isMovingGradient = false;
            Mouse.Capture(null);
        }

        private void GradientRectangle_MouseMove(object sender, MouseEventArgs e)
        {
            if (isMovingGradient && e.LeftButton == MouseButtonState.Pressed)
            {
                // Update all information when moving mouse.
                Canvas.SetTop(GradientPointer, Mouse.GetPosition(ColorPickerCanvas).Y);
                ConstraintGradientPointer();

                UpdateGradientPicker();
                UpdateColor();
                UpdateInfo();
            }
        }

        /// <summary>
        /// Constraints the color pick pointer to the bounds of the color map image.
        /// </summary>
        private void ConstraintColorPickPointer()
        {
            double left = Canvas.GetLeft(ColorMapImage);
            double top = Canvas.GetTop(ColorMapImage);
            double width = ColorMapImage.Width;
            double height = ColorMapImage.Height;

            if (Canvas.GetLeft(ColorPickPointer) < left)
            {
                Canvas.SetLeft(ColorPickPointer, left);
            }
            if (Canvas.GetLeft(ColorPickPointer) + ColorPickPointer.Width > left + width)
            {
                Canvas.SetLeft(ColorPickPointer, left + width - ColorPickPointer.Width);
            }
            if (Canvas.GetTop(ColorPickPointer) < top)
            {
                Canvas.SetTop(ColorPickPointer, top);
            }
            if (Canvas.GetTop(ColorPickPointer) + ColorPickPointer.Height > top + height)
            {
                Canvas.SetTop(ColorPickPointer, top + height - ColorPickPointer.Height);
            }

            Canvas.SetLeft(ColorPickPointerGFX, Canvas.GetLeft(ColorPickPointer) - ColorPickPointerGFX.Width / 2);
            Canvas.SetTop(ColorPickPointerGFX, Canvas.GetTop(ColorPickPointer) - ColorPickPointerGFX.Height / 2);
        }

        /// <summary>
        /// Constraints the gradient pointer to the bounds of the gradient rectangle.
        /// </summary>
        private void ConstraintGradientPointer()
        {
            double top = Canvas.GetTop(GradientRectangle);
            double height = GradientRectangle.Height;

            if (Canvas.GetTop(GradientPointer) < top)
            {
                Canvas.SetTop(GradientPointer, top);
            }
            if (Canvas.GetTop(GradientPointer) > top + height)
            {
                Canvas.SetTop(GradientPointer, top + height);
            }

            Canvas.SetLeft(GradientPointerGFX, Canvas.GetLeft(GradientPointer) - GradientPointerGFX.Width / 2);
            Canvas.SetTop(GradientPointerGFX, Canvas.GetTop(GradientPointer) - GradientPointerGFX.Height / 2);
        }

        /// <summary>
        /// Get the position of the color pick pointer.
        /// </summary>
        /// <returns>Color pick pointer position</returns>
        private Point GetColorPickPointer()
        {
            double x = Canvas.GetLeft(ColorPickPointer) - Canvas.GetLeft(ColorMapImage);
            double y = Canvas.GetTop(ColorPickPointer) - Canvas.GetTop(ColorMapImage);

            return new Point(x, y);
        }

        /// <summary>
        /// Draw a gradient from selected color to black.
        /// </summary>
        /// <param name="color">The color to start from</param>
        private void UpdateGradient(Color color)
        {
            LinearGradientBrush gradientBrush = new LinearGradientBrush();

            GradientStop startGradient = new GradientStop();

            // If the hue is grayscale, set gradient color to white.
            if (color.R == color.G && color.G == color.B && color.B == color.R)
            {
                startGradient.Color = Colors.White;
            }
            else
            {
                startGradient.Color = color;
            }

            startGradient.Offset = 0;
            gradientBrush.GradientStops.Add(startGradient);

            GradientStop endGradient = new GradientStop();
            endGradient.Color = Colors.Black;
            endGradient.Offset = 1;
            gradientBrush.GradientStops.Add(endGradient);

            GradientRectangle.Fill = gradientBrush;
        }

        /// <summary>
        /// Update the color hue and saturation from the position of the color picker.
        /// </summary>
        private void UpdateColorPicker()
        {
            colorHSB.Hue = ((Canvas.GetLeft(ColorPickPointer) - Canvas.GetLeft(ColorMapImage)) / 255) * 360;
            colorHSB.Saturation = 1 - ((Canvas.GetTop(ColorPickPointer) - Canvas.GetTop(ColorMapImage)) / 255);

            ColorHSB gradientHSB = new ColorHSB(colorHSB.Hue, colorHSB.Saturation, 1);

            UpdateGradient(ColorHSB.HSBtoRGB(gradientHSB));
        }

        /// <summary>
        /// Update the color brightness from the position of the gradient picker.
        /// </summary>
        private void UpdateGradientPicker()
        {
            double offset = 1 - (Canvas.GetTop(GradientPointer) - Canvas.GetTop(GradientRectangle)) / GradientRectangle.Height;
            colorHSB.Brightness = offset;
        }

        /// <summary>
        /// Update the selected color to the preview box.
        /// </summary>
        private void UpdateColor()
        {
            ColorHSB selectedHSB = new ColorHSB(colorHSB.Hue, colorHSB.Saturation, colorHSB.Brightness);
            selectedColor = ColorHSB.HSBtoRGB(selectedHSB);

            ColorPreviewBox.Background = GetBrush();
        }

        /// <summary>
        /// Send the color information to the controls.
        /// </summary>
        private void UpdateInfo()
        {
            // Ignore textchange events sent programmatically to prevent double calls.
            ignoreTextChangeEvent = true;

            if (!ColorHex.IsFocused)
                ColorHex.Text = this.ToString();
            if (!RgbTextBoxR.IsFocused)
                RgbTextBoxR.Text = selectedColor.R.ToString();
            if (!RgbTextBoxG.IsFocused)
                RgbTextBoxG.Text = selectedColor.G.ToString();
            if (!RgbTextBoxB.IsFocused)
                RgbTextBoxB.Text = selectedColor.B.ToString();

            ignoreTextChangeEvent = false;
        }

        /// <summary>
        /// Filter a string using regex, removing all characters not valid by the regex.
        /// </summary>
        /// <param name="text">The text to filter</param>
        /// <param name="regex">The regex to filter by</param>
        /// <returns>The filtered string</returns>
        private string FilterByRegex(string text, string regex)
        {
            string filtered = text;

            foreach (char ch in text)
            {
                if (!Regex.IsMatch(Char.ToString(ch), regex))
                {
                    filtered = filtered.Replace(Char.ToString(ch), "");
                }
            }

            return filtered;
        }

        private void RgbTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ignoreTextChangeEvent)
                return;

            TextBox textBox = sender as TextBox;

            if (!String.IsNullOrEmpty(textBox.Text))
            {
                // Make sure the text is numeric only.
                if (!Regex.IsMatch(textBox.Text, "^[0-9]+$"))
                {
                    textBox.Text = FilterByRegex(textBox.Text, "^[0-9]+$");
                    textBox.CaretIndex = textBox.Text.Length;
                    return;
                }

                int value = int.Parse(textBox.Text);

                // Make sure max value is always 255.
                if (value > 255)
                {
                    textBox.Text = "255";
                    textBox.CaretIndex = textBox.Text.Length;
                    return;
                }
            }

            // Update RGB from text.
            byte r, g, b;

            if (!String.IsNullOrEmpty(RgbTextBoxR.Text))
                r = Byte.Parse(RgbTextBoxR.Text);
            else
                r = 0;
            if (!String.IsNullOrEmpty(RgbTextBoxG.Text))
                g = Byte.Parse(RgbTextBoxG.Text);
            else
                g = 0;
            if (!String.IsNullOrEmpty(RgbTextBoxB.Text))
                b = Byte.Parse(RgbTextBoxB.Text);
            else
                b = 0;

            LoadColor(Color.FromRgb(r, g, b));
        }

        private void ColorHex_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ignoreTextChangeEvent)
                return;

            TextBox textBox = sender as TextBox;

            if (String.IsNullOrEmpty(textBox.Text))
            {
                // Make sure the # is always there if string is empty.
                textBox.Text = "#";
                textBox.CaretIndex = textBox.Text.Length;
                return;
            }

            if (!String.IsNullOrEmpty(textBox.Text))
            {
                // Make sure the text is numeric only.
                if (!Regex.IsMatch(textBox.Text, "^#([a-fA-F0-9]{0,6})$"))
                {
                    textBox.Text = FilterByRegex(textBox.Text, "^#([a-fA-F0-9]{0,6})$");
                    textBox.CaretIndex = textBox.Text.Length;
                    return;
                }
            }

            string hex = textBox.Text;

            while (hex.Length < 7)
            {
                hex += "0";
            }

            LoadColor((Color)ColorConverter.ConvertFromString(hex));
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Always focus the canvas on mouse clicks outside controls.
            ColorPickerCanvas.Focus();
            UpdateInfo();
            UpdateColor();
        }

        private void ApplyButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                // Apply the selected color to button border.
                ApplyButton.BorderBrush = this.GetBrush();
                ApplyButton.BorderThickness = new Thickness(2);

                // Capture mouse events for this control.
                Mouse.Capture(ApplyButton);
            }
        }

        private void ApplyButton_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Point mouse = Mouse.GetPosition(ColorPickerCanvas);

            double buttonX = Canvas.GetLeft(ApplyButton);
            double buttonY = Canvas.GetTop(ApplyButton);

            // Make sure the mouse is within the bounds of the button to call event.
            if (mouse.X >= buttonX && mouse.X <= buttonX + ApplyButton.Width &&
                mouse.Y >= buttonY && mouse.Y <= buttonY + ApplyButton.Height)
            {
                ApplyEvent(EventArgs.Empty);
            }

            ApplyButton.BorderBrush = Brushes.Black;
            ApplyButton.BorderThickness = new Thickness(1);

            Mouse.Capture(null);
        }
    }
}
