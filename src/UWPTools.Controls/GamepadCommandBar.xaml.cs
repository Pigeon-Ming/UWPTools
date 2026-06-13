using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.Gaming.Input;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Markup;

namespace UWPTools.Controls
{
    [ContentProperty(Name = nameof(Items))]
    public sealed partial class GamepadCommandBar : UserControl
    {
        private readonly DispatcherTimer pollingTimer;
        private readonly Dictionary<Gamepad, GamepadReading> previousReadings = new Dictionary<Gamepad, GamepadReading>();

        public GamepadCommandBar()
        {
            InitializeComponent();

            Items = new ObservableCollection<GamepadCommandBarItem>();
            CommandItemsControl.ItemsSource = Items;

            pollingTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(50)
            };
            pollingTimer.Tick += PollingTimer_Tick;

            Loaded += GamepadCommandBar_Loaded;
            Unloaded += GamepadCommandBar_Unloaded;
        }

        public ObservableCollection<GamepadCommandBarItem> Items { get; }

        public bool IsKeyboardInputEnabled
        {
            get => (bool)GetValue(IsKeyboardInputEnabledProperty);
            set => SetValue(IsKeyboardInputEnabledProperty, value);
        }

        public static readonly DependencyProperty IsKeyboardInputEnabledProperty =
            DependencyProperty.Register(nameof(IsKeyboardInputEnabled), typeof(bool), typeof(GamepadCommandBar), new PropertyMetadata(true));

        public bool IsGamepadInputEnabled
        {
            get => (bool)GetValue(IsGamepadInputEnabledProperty);
            set => SetValue(IsGamepadInputEnabledProperty, value);
        }

        public static readonly DependencyProperty IsGamepadInputEnabledProperty =
            DependencyProperty.Register(nameof(IsGamepadInputEnabled), typeof(bool), typeof(GamepadCommandBar),
                new PropertyMetadata(true, OnIsGamepadInputEnabledChanged));

        public TimeSpan PollingInterval
        {
            get => (TimeSpan)GetValue(PollingIntervalProperty);
            set => SetValue(PollingIntervalProperty, value);
        }

        public static readonly DependencyProperty PollingIntervalProperty =
            DependencyProperty.Register(nameof(PollingInterval), typeof(TimeSpan), typeof(GamepadCommandBar),
                new PropertyMetadata(TimeSpan.FromMilliseconds(50), OnPollingIntervalChanged));

        public double AnalogInputThreshold
        {
            get => (double)GetValue(AnalogInputThresholdProperty);
            set => SetValue(AnalogInputThresholdProperty, value);
        }

        public static readonly DependencyProperty AnalogInputThresholdProperty =
            DependencyProperty.Register(nameof(AnalogInputThreshold), typeof(double), typeof(GamepadCommandBar), new PropertyMetadata(0.65));

        private static void OnIsGamepadInputEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (GamepadCommandBar)d;
            control.UpdatePollingState();
        }

        private static void OnPollingIntervalChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (GamepadCommandBar)d;
            control.pollingTimer.Interval = (TimeSpan)e.NewValue;
        }

        private void GamepadCommandBar_Loaded(object sender, RoutedEventArgs e)
        {
            Window.Current.CoreWindow.KeyDown += CoreWindow_KeyDown;
            UpdatePollingState();
        }

        private void GamepadCommandBar_Unloaded(object sender, RoutedEventArgs e)
        {
            Window.Current.CoreWindow.KeyDown -= CoreWindow_KeyDown;
            pollingTimer.Stop();
            previousReadings.Clear();
        }

        private void UpdatePollingState()
        {
            if (!IsLoaded)
            {
                return;
            }

            if (IsGamepadInputEnabled)
            {
                pollingTimer.Start();
            }
            else
            {
                pollingTimer.Stop();
                previousReadings.Clear();
            }
        }

        private void PollingTimer_Tick(object sender, object e)
        {
            foreach (var gamepad in Gamepad.Gamepads)
            {
                var reading = gamepad.GetCurrentReading();
                GamepadReading oldReading;
                previousReadings.TryGetValue(gamepad, out oldReading);

                foreach (var item in Items)
                {
                    if (item == null || !item.IsEnabled)
                    {
                        continue;
                    }

                    var isPressed = IsItemPressed(reading, item);
                    var wasPressed = IsItemPressed(oldReading, item);
                    if (isPressed && !wasPressed)
                    {
                        ExecuteItem(item);
                    }
                }

                previousReadings[gamepad] = reading;
            }
        }

        private void CoreWindow_KeyDown(CoreWindow sender, KeyEventArgs args)
        {
            if (!IsKeyboardInputEnabled)
            {
                return;
            }

            var keyModifiers = GetCurrentKeyModifiers(sender);
            foreach (var item in Items)
            {
                if (item == null || !item.IsEnabled || item.Key == VirtualKey.None)
                {
                    continue;
                }

                if (item.Key == args.VirtualKey && item.KeyModifiers == keyModifiers)
                {
                    ExecuteItem(item);
                    args.Handled = true;
                    return;
                }
            }
        }

        private static GamepadCommandBarKeyModifiers GetCurrentKeyModifiers(CoreWindow window)
        {
            var modifiers = GamepadCommandBarKeyModifiers.None;
            if (window.GetKeyState(VirtualKey.Control).HasFlag(CoreVirtualKeyStates.Down))
            {
                modifiers |= GamepadCommandBarKeyModifiers.Control;
            }
            if (window.GetKeyState(VirtualKey.Shift).HasFlag(CoreVirtualKeyStates.Down))
            {
                modifiers |= GamepadCommandBarKeyModifiers.Shift;
            }
            if (window.GetKeyState(VirtualKey.Menu).HasFlag(CoreVirtualKeyStates.Down))
            {
                modifiers |= GamepadCommandBarKeyModifiers.Menu;
            }
            if (window.GetKeyState(VirtualKey.LeftWindows).HasFlag(CoreVirtualKeyStates.Down) ||
                window.GetKeyState(VirtualKey.RightWindows).HasFlag(CoreVirtualKeyStates.Down))
            {
                modifiers |= GamepadCommandBarKeyModifiers.Windows;
            }

            return modifiers;
        }

        private bool IsItemPressed(GamepadReading reading, GamepadCommandBarItem item)
        {
            if (item.Button != GamepadCommandBarButton.None)
            {
                return IsButtonPressed(reading, item.Button);
            }

            if (item.GamepadButton == GamepadButtons.None)
            {
                return false;
            }

            return (reading.Buttons & item.GamepadButton) == item.GamepadButton;
        }

        private bool IsButtonPressed(GamepadReading reading, GamepadCommandBarButton button)
        {
            switch (button)
            {
                case GamepadCommandBarButton.A:
                    return HasButton(reading, GamepadButtons.A);
                case GamepadCommandBarButton.B:
                    return HasButton(reading, GamepadButtons.B);
                case GamepadCommandBarButton.X:
                    return HasButton(reading, GamepadButtons.X);
                case GamepadCommandBarButton.Y:
                    return HasButton(reading, GamepadButtons.Y);
                case GamepadCommandBarButton.DPadUp:
                    return HasButton(reading, GamepadButtons.DPadUp);
                case GamepadCommandBarButton.DPadDown:
                    return HasButton(reading, GamepadButtons.DPadDown);
                case GamepadCommandBarButton.DPadLeft:
                    return HasButton(reading, GamepadButtons.DPadLeft);
                case GamepadCommandBarButton.DPadRight:
                    return HasButton(reading, GamepadButtons.DPadRight);
                case GamepadCommandBarButton.LeftShoulder:
                    return HasButton(reading, GamepadButtons.LeftShoulder);
                case GamepadCommandBarButton.RightShoulder:
                    return HasButton(reading, GamepadButtons.RightShoulder);
                case GamepadCommandBarButton.LeftTrigger:
                    return reading.LeftTrigger >= AnalogInputThreshold;
                case GamepadCommandBarButton.RightTrigger:
                    return reading.RightTrigger >= AnalogInputThreshold;
                case GamepadCommandBarButton.View:
                    return HasButton(reading, GamepadButtons.View);
                case GamepadCommandBarButton.Menu:
                    return HasButton(reading, GamepadButtons.Menu);
                case GamepadCommandBarButton.LeftThumbstickButton:
                    return HasButton(reading, GamepadButtons.LeftThumbstick);
                case GamepadCommandBarButton.RightThumbstickButton:
                    return HasButton(reading, GamepadButtons.RightThumbstick);
                case GamepadCommandBarButton.LeftThumbstickUp:
                    return reading.LeftThumbstickY >= AnalogInputThreshold;
                case GamepadCommandBarButton.LeftThumbstickDown:
                    return reading.LeftThumbstickY <= -AnalogInputThreshold;
                case GamepadCommandBarButton.LeftThumbstickLeft:
                    return reading.LeftThumbstickX <= -AnalogInputThreshold;
                case GamepadCommandBarButton.LeftThumbstickRight:
                    return reading.LeftThumbstickX >= AnalogInputThreshold;
                case GamepadCommandBarButton.RightThumbstickUp:
                    return reading.RightThumbstickY >= AnalogInputThreshold;
                case GamepadCommandBarButton.RightThumbstickDown:
                    return reading.RightThumbstickY <= -AnalogInputThreshold;
                case GamepadCommandBarButton.RightThumbstickLeft:
                    return reading.RightThumbstickX <= -AnalogInputThreshold;
                case GamepadCommandBarButton.RightThumbstickRight:
                    return reading.RightThumbstickX >= AnalogInputThreshold;
                case GamepadCommandBarButton.Paddle1:
                    return HasButton(reading, GamepadButtons.Paddle1);
                case GamepadCommandBarButton.Paddle2:
                    return HasButton(reading, GamepadButtons.Paddle2);
                case GamepadCommandBarButton.Paddle3:
                    return HasButton(reading, GamepadButtons.Paddle3);
                case GamepadCommandBarButton.Paddle4:
                    return HasButton(reading, GamepadButtons.Paddle4);
                default:
                    return false;
            }
        }

        private static bool HasButton(GamepadReading reading, GamepadButtons button)
        {
            return (reading.Buttons & button) == button;
        }

        private void CommandButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            ExecuteItem(button?.DataContext as GamepadCommandBarItem);
        }

        private void ExecuteItem(GamepadCommandBarItem item)
        {
            if (item == null)
            {
                return;
            }

            item.RaiseClick();

            if (item.Command == null)
            {
                return;
            }

            var parameter = item.CommandParameter;
            if (item.Command.CanExecute(parameter))
            {
                item.Command.Execute(parameter);
            }
        }
    }
}
