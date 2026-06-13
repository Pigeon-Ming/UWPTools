using System.ComponentModel;
using System.Windows.Input;
using Windows.Gaming.Input;
using Windows.System;
using Windows.UI.Xaml;

namespace UWPTools.Controls
{
    public class GamepadCommandBarItem : DependencyObject, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public event RoutedEventHandler Click;

        public string Label
        {
            get => (string)GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register(nameof(Label), typeof(string), typeof(GamepadCommandBarItem), new PropertyMetadata(null));

        public GamepadCommandBarButton Button
        {
            get => (GamepadCommandBarButton)GetValue(ButtonProperty);
            set => SetValue(ButtonProperty, value);
        }

        public static readonly DependencyProperty ButtonProperty =
            DependencyProperty.Register(nameof(Button), typeof(GamepadCommandBarButton), typeof(GamepadCommandBarItem),
                new PropertyMetadata(GamepadCommandBarButton.None, OnDisplayPropertyChanged));

        public GamepadButtons GamepadButton
        {
            get => (GamepadButtons)GetValue(GamepadButtonProperty);
            set => SetValue(GamepadButtonProperty, value);
        }

        public static readonly DependencyProperty GamepadButtonProperty =
            DependencyProperty.Register(nameof(GamepadButton), typeof(GamepadButtons), typeof(GamepadCommandBarItem),
                new PropertyMetadata(GamepadButtons.None, OnDisplayPropertyChanged));

        public VirtualKey Key
        {
            get => (VirtualKey)GetValue(KeyProperty);
            set => SetValue(KeyProperty, value);
        }

        public static readonly DependencyProperty KeyProperty =
            DependencyProperty.Register(nameof(Key), typeof(VirtualKey), typeof(GamepadCommandBarItem),
                new PropertyMetadata(VirtualKey.None, OnDisplayPropertyChanged));

        public GamepadCommandBarKeyModifiers KeyModifiers
        {
            get => (GamepadCommandBarKeyModifiers)GetValue(KeyModifiersProperty);
            set => SetValue(KeyModifiersProperty, value);
        }

        public static readonly DependencyProperty KeyModifiersProperty =
            DependencyProperty.Register(nameof(KeyModifiers), typeof(GamepadCommandBarKeyModifiers), typeof(GamepadCommandBarItem),
                new PropertyMetadata(GamepadCommandBarKeyModifiers.None, OnDisplayPropertyChanged));

        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(GamepadCommandBarItem), new PropertyMetadata(null));

        public object CommandParameter
        {
            get => GetValue(CommandParameterProperty);
            set => SetValue(CommandParameterProperty, value);
        }

        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register(nameof(CommandParameter), typeof(object), typeof(GamepadCommandBarItem), new PropertyMetadata(null));

        public bool IsEnabled
        {
            get => (bool)GetValue(IsEnabledProperty);
            set => SetValue(IsEnabledProperty, value);
        }

        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.Register(nameof(IsEnabled), typeof(bool), typeof(GamepadCommandBarItem), new PropertyMetadata(true));

        public string GamepadButtonText
        {
            get
            {
                if (HasButtonGlyph(Button))
                {
                    return string.Empty;
                }

                if (Button != GamepadCommandBarButton.None)
                {
                    return GetButtonText(Button);
                }

                if (GamepadButton == GamepadButtons.None)
                {
                    return string.Empty;
                }

                return GamepadButton.ToString();
            }
        }

        public string GamepadButtonGlyph
        {
            get
            {
                switch (Button)
                {
                    case GamepadCommandBarButton.A:
                        return "\uF093";
                    case GamepadCommandBarButton.B:
                        return "\uF094";
                    case GamepadCommandBarButton.Y:
                        return "\uF095";
                    case GamepadCommandBarButton.X:
                        return "\uF096";
                    case GamepadCommandBarButton.LeftThumbstickButton:
                    case GamepadCommandBarButton.LeftThumbstickUp:
                    case GamepadCommandBarButton.LeftThumbstickDown:
                    case GamepadCommandBarButton.LeftThumbstickLeft:
                    case GamepadCommandBarButton.LeftThumbstickRight:
                        return "\uF108";
                    case GamepadCommandBarButton.RightThumbstickButton:
                    case GamepadCommandBarButton.RightThumbstickUp:
                    case GamepadCommandBarButton.RightThumbstickDown:
                    case GamepadCommandBarButton.RightThumbstickLeft:
                    case GamepadCommandBarButton.RightThumbstickRight:
                        return "\uF109";
                    case GamepadCommandBarButton.LeftTrigger:
                        return "\uF10A";
                    case GamepadCommandBarButton.RightTrigger:
                        return "\uF10B";
                    case GamepadCommandBarButton.LeftShoulder:
                        return "\uF10C";
                    case GamepadCommandBarButton.RightShoulder:
                        return "\uF10D";
                    case GamepadCommandBarButton.Menu:
                        return "\uEDE3";
                    case GamepadCommandBarButton.View:
                        return "\uEECA";
                    default:
                        return string.Empty;
                }
            }
        }

        public Visibility GamepadButtonGlyphVisibility
        {
            get { return string.IsNullOrEmpty(GamepadButtonGlyph) ? Visibility.Collapsed : Visibility.Visible; }
        }

        public Visibility GamepadButtonTextVisibility
        {
            get { return string.IsNullOrEmpty(GamepadButtonText) ? Visibility.Collapsed : Visibility.Visible; }
        }

        public Thickness IndicatorBorderThickness
        {
            get { return string.IsNullOrEmpty(GamepadButtonGlyph) ? new Thickness(1) : new Thickness(0); }
        }

        public string KeyText
        {
            get
            {
                if (Key == VirtualKey.None)
                {
                    return string.Empty;
                }

                var modifier = KeyModifiers == GamepadCommandBarKeyModifiers.None ? string.Empty : KeyModifiers + "+";
                return modifier + Key;
            }
        }

        public Visibility KeyTextVisibility
        {
            get { return string.IsNullOrEmpty(KeyText) ? Visibility.Collapsed : Visibility.Visible; }
        }

        private static void OnDisplayPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var item = (GamepadCommandBarItem)d;
            item.PropertyChanged?.Invoke(item, new PropertyChangedEventArgs(nameof(GamepadButtonGlyph)));
            item.PropertyChanged?.Invoke(item, new PropertyChangedEventArgs(nameof(GamepadButtonGlyphVisibility)));
            item.PropertyChanged?.Invoke(item, new PropertyChangedEventArgs(nameof(GamepadButtonText)));
            item.PropertyChanged?.Invoke(item, new PropertyChangedEventArgs(nameof(GamepadButtonTextVisibility)));
            item.PropertyChanged?.Invoke(item, new PropertyChangedEventArgs(nameof(IndicatorBorderThickness)));
            item.PropertyChanged?.Invoke(item, new PropertyChangedEventArgs(nameof(KeyText)));
            item.PropertyChanged?.Invoke(item, new PropertyChangedEventArgs(nameof(KeyTextVisibility)));
        }

        internal void RaiseClick()
        {
            Click?.Invoke(this, new RoutedEventArgs());
        }

        private static string GetButtonText(GamepadCommandBarButton button)
        {
            switch (button)
            {
                case GamepadCommandBarButton.DPadUp:
                    return "D-Up";
                case GamepadCommandBarButton.DPadDown:
                    return "D-Down";
                case GamepadCommandBarButton.DPadLeft:
                    return "D-Left";
                case GamepadCommandBarButton.DPadRight:
                    return "D-Right";
                case GamepadCommandBarButton.LeftShoulder:
                    return "LB";
                case GamepadCommandBarButton.RightShoulder:
                    return "RB";
                case GamepadCommandBarButton.LeftTrigger:
                    return "LT";
                case GamepadCommandBarButton.RightTrigger:
                    return "RT";
                case GamepadCommandBarButton.LeftThumbstickButton:
                    return "LS";
                case GamepadCommandBarButton.RightThumbstickButton:
                    return "RS";
                case GamepadCommandBarButton.LeftThumbstickUp:
                    return "LS Up";
                case GamepadCommandBarButton.LeftThumbstickDown:
                    return "LS Down";
                case GamepadCommandBarButton.LeftThumbstickLeft:
                    return "LS Left";
                case GamepadCommandBarButton.LeftThumbstickRight:
                    return "LS Right";
                case GamepadCommandBarButton.RightThumbstickUp:
                    return "RS Up";
                case GamepadCommandBarButton.RightThumbstickDown:
                    return "RS Down";
                case GamepadCommandBarButton.RightThumbstickLeft:
                    return "RS Left";
                case GamepadCommandBarButton.RightThumbstickRight:
                    return "RS Right";
                case GamepadCommandBarButton.Paddle1:
                    return "P1";
                case GamepadCommandBarButton.Paddle2:
                    return "P2";
                case GamepadCommandBarButton.Paddle3:
                    return "P3";
                case GamepadCommandBarButton.Paddle4:
                    return "P4";
                default:
                    return button.ToString();
            }
        }

        private static bool HasButtonGlyph(GamepadCommandBarButton button)
        {
            switch (button)
            {
                case GamepadCommandBarButton.A:
                case GamepadCommandBarButton.B:
                case GamepadCommandBarButton.X:
                case GamepadCommandBarButton.Y:
                case GamepadCommandBarButton.LeftThumbstickButton:
                case GamepadCommandBarButton.LeftThumbstickUp:
                case GamepadCommandBarButton.LeftThumbstickDown:
                case GamepadCommandBarButton.LeftThumbstickLeft:
                case GamepadCommandBarButton.LeftThumbstickRight:
                case GamepadCommandBarButton.RightThumbstickButton:
                case GamepadCommandBarButton.RightThumbstickUp:
                case GamepadCommandBarButton.RightThumbstickDown:
                case GamepadCommandBarButton.RightThumbstickLeft:
                case GamepadCommandBarButton.RightThumbstickRight:
                case GamepadCommandBarButton.LeftTrigger:
                case GamepadCommandBarButton.RightTrigger:
                case GamepadCommandBarButton.LeftShoulder:
                case GamepadCommandBarButton.RightShoulder:
                case GamepadCommandBarButton.Menu:
                case GamepadCommandBarButton.View:
                    return true;
                default:
                    return false;
            }
        }
    }
}
