using UWPTools.Controls;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace UWPTools.Pages.ControlPages
{
    public sealed partial class GamepadCommandBarPage : Page
    {
        public GamepadCommandBarPage()
        {
            InitializeComponent();
            InitializeCommands();
        }

        private void InitializeCommands()
        {
            DemoCommandBar.Items.Add(CreateItem("方向键上", GamepadCommandBarButton.DPadUp));
            DemoCommandBar.Items.Add(CreateItem("方向键下", GamepadCommandBarButton.DPadDown));
            DemoCommandBar.Items.Add(CreateItem("方向键左", GamepadCommandBarButton.DPadLeft));
            DemoCommandBar.Items.Add(CreateItem("方向键右", GamepadCommandBarButton.DPadRight));
            DemoCommandBar.Items.Add(CreateItem("LB肩键", GamepadCommandBarButton.LeftShoulder));
            DemoCommandBar.Items.Add(CreateItem("RB肩键", GamepadCommandBarButton.RightShoulder));
            DemoCommandBar.Items.Add(CreateItem("LT扳机", GamepadCommandBarButton.LeftTrigger));
            DemoCommandBar.Items.Add(CreateItem("RT扳机", GamepadCommandBarButton.RightTrigger));
            DemoCommandBar.Items.Add(CreateItem("视图键", GamepadCommandBarButton.View));
            DemoCommandBar.Items.Add(CreateItem("菜单键", GamepadCommandBarButton.Menu));
            DemoCommandBar.Items.Add(CreateItem("左摇杆按下", GamepadCommandBarButton.LeftThumbstickButton));
            DemoCommandBar.Items.Add(CreateItem("右摇杆按下", GamepadCommandBarButton.RightThumbstickButton));
            DemoCommandBar.Items.Add(CreateItem("左摇杆上", GamepadCommandBarButton.LeftThumbstickUp));
            DemoCommandBar.Items.Add(CreateItem("左摇杆下", GamepadCommandBarButton.LeftThumbstickDown));
            DemoCommandBar.Items.Add(CreateItem("左摇杆左", GamepadCommandBarButton.LeftThumbstickLeft));
            DemoCommandBar.Items.Add(CreateItem("左摇杆右", GamepadCommandBarButton.LeftThumbstickRight));
            DemoCommandBar.Items.Add(CreateItem("右摇杆上", GamepadCommandBarButton.RightThumbstickUp));
            DemoCommandBar.Items.Add(CreateItem("右摇杆下", GamepadCommandBarButton.RightThumbstickDown));
            DemoCommandBar.Items.Add(CreateItem("右摇杆左", GamepadCommandBarButton.RightThumbstickLeft));
            DemoCommandBar.Items.Add(CreateItem("右摇杆右", GamepadCommandBarButton.RightThumbstickRight));
            DemoCommandBar.Items.Add(CreateItem("拨片 1", GamepadCommandBarButton.Paddle1));
            DemoCommandBar.Items.Add(CreateItem("拨片 2", GamepadCommandBarButton.Paddle2));
            DemoCommandBar.Items.Add(CreateItem("拨片 3", GamepadCommandBarButton.Paddle3));
            DemoCommandBar.Items.Add(CreateItem("拨片 4", GamepadCommandBarButton.Paddle4));
        }

        private GamepadCommandBarItem CreateItem(
            string label,
            GamepadCommandBarButton button,
            VirtualKey key = VirtualKey.None,
            GamepadCommandBarKeyModifiers modifiers = GamepadCommandBarKeyModifiers.None)
        {
            return new GamepadCommandBarItem
            {
                Label = label,
                Button = button,
                Key = key,
                KeyModifiers = modifiers,
                Command = new GamepadCommand(_ => ShowAction(label))
            };
        }

        private void ShowAction(string action)
        {
            ActionTextBlock.Text = action;
        }

        private void ConfirmItem_Click(object sender, RoutedEventArgs e)
        {
            ShowAction("A键");
        }

        private void BackItem_Click(object sender, RoutedEventArgs e)
        {
            ShowAction("B键");
        }

        private void SaveItem_Click(object sender, RoutedEventArgs e)
        {
            ShowAction("X键");
        }

        private void DeleteItem_Click(object sender, RoutedEventArgs e)
        {
            ShowAction("Y键");
        }
    }
}
