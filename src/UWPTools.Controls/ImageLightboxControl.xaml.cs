using System;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using System.Collections.Generic;

namespace UWPTools.Controls
{
    public sealed partial class ImageLightboxControl : UserControl
    {
        private Popup _popup;
        private WindowSizeChangedEventHandler _sizeChangedHandler;

        public ImageLightboxControl()
        {
            InitializeComponent();
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        public IList<ImageSource> Sources
        {
            get => (IList<ImageSource>)GetValue(SourcesProperty);
            set => SetValue(SourcesProperty, value);
        }

        public static readonly DependencyProperty SourcesProperty =
            DependencyProperty.Register(nameof(Sources), typeof(IList<ImageSource>), typeof(ImageLightboxControl),
                new PropertyMetadata(null, OnSourcesChanged));

        public int SelectedIndex
        {
            get => (int)GetValue(SelectedIndexProperty);
            set => SetValue(SelectedIndexProperty, value);
        }

        public static readonly DependencyProperty SelectedIndexProperty =
            DependencyProperty.Register(nameof(SelectedIndex), typeof(int), typeof(ImageLightboxControl),
                new PropertyMetadata(0, OnSelectedIndexChanged));

        public static void Show(ImageSource source)
        {
            var lb = new ImageLightboxControl { Sources = new List<ImageSource> { source }, SelectedIndex = 0 };
            var popup = new Popup { Child = lb, IsLightDismissEnabled = false };
            lb._popup = popup;
            lb.Width = Window.Current.Bounds.Width;
            lb.Height = Window.Current.Bounds.Height;
            lb._sizeChangedHandler = (w, args) =>
            {
                lb.Width = args.Size.Width;
                lb.Height = args.Size.Height;
                lb.FitCurrent();
                lb.UpdatePageIndicator();
                lb.UpdateNavButtonsVisibility();
            };
            Window.Current.SizeChanged += lb._sizeChangedHandler;
            popup.IsOpen = true;
            lb.Focus(FocusState.Programmatic);
        }

        public static void Show(IList<ImageSource> sources, int startIndex = 0)
        {
            var lb = new ImageLightboxControl { Sources = sources, SelectedIndex = Math.Max(0, Math.Min(startIndex, sources?.Count - 1 ?? 0)) };
            var popup = new Popup { Child = lb, IsLightDismissEnabled = false };
            lb._popup = popup;
            lb.Width = Window.Current.Bounds.Width;
            lb.Height = Window.Current.Bounds.Height;
            lb._sizeChangedHandler = (w, args) =>
            {
                lb.Width = args.Size.Width;
                lb.Height = args.Size.Height;
                lb.FitCurrent();
                lb.UpdatePageIndicator();
                lb.UpdateNavButtonsVisibility();
            };
            Window.Current.SizeChanged += lb._sizeChangedHandler;
            popup.IsOpen = true;
            lb.Focus(FocusState.Programmatic);
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            ShowSb.Begin();
            Root.PointerWheelChanged += Root_PointerWheelChanged;
            if (Flip != null)
            {
                Flip.ItemsSource = Sources;
                Flip.SelectedIndex = SelectedIndex;
                FitCurrent();
                UpdatePageIndicator();
                HideFlipViewNavigation();
                UpdateNavButtonsVisibility();
                Flip.LayoutUpdated += Flip_LayoutUpdated;
            }
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            if (_sizeChangedHandler != null)
            {
                Window.Current.SizeChanged -= _sizeChangedHandler;
                _sizeChangedHandler = null;
            }
            Root.PointerWheelChanged -= Root_PointerWheelChanged;
            if (Flip != null)
            {
                Flip.LayoutUpdated -= Flip_LayoutUpdated;
            }
        }

        private void Close()
        {
            HideSb.Completed += HideSb_Completed;
            HideSb.Begin();
        }

        private void HideSb_Completed(object sender, object e)
        {
            HideSb.Completed -= HideSb_Completed;
            if (_popup != null)
            {
                _popup.IsOpen = false;
                _popup.Child = null;
                _popup = null;
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Root_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Escape)
            {
                e.Handled = true;
                Close();
            }
            else if (e.Key == VirtualKey.Left)
            {
                e.Handled = true;
                Navigate(-1);
            }
            else if (e.Key == VirtualKey.Right)
            {
                e.Handled = true;
                Navigate(1);
            }
        }

        private static void OnSourcesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var c = (ImageLightboxControl)d;
            if (c.Flip != null)
            {
                c.Flip.ItemsSource = (IList<ImageSource>)e.NewValue;
                c.Flip.SelectedIndex = c.SelectedIndex < (c.Sources?.Count ?? 0) ? c.SelectedIndex : 0;
                c.FitCurrent();
                c.UpdatePageIndicator();
                c.HideFlipViewNavigation();
                c.UpdateNavButtonsVisibility();
            }
        }

        private static void OnSelectedIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var c = (ImageLightboxControl)d;
            if (c.Flip != null)
            {
                c.Flip.SelectedIndex = (int)e.NewValue;
                c.FitCurrent();
                c.UpdatePageIndicator();
                c.UpdateNavButtonsVisibility();
            }
        }

        private void PrevButton_Click(object sender, RoutedEventArgs e) => Navigate(-1);
        private void NextButton_Click(object sender, RoutedEventArgs e) => Navigate(1);

        private void Navigate(int delta)
        {
            if (Sources == null || Sources.Count == 0) return;
            var idx = Flip?.SelectedIndex ?? 0;
            var next = Math.Max(0, Math.Min(idx + delta, Sources.Count - 1));
            if (next != idx)
            {
                Flip.SelectedIndex = next;
            }
            UpdateNavButtonsVisibility();
        }

        private void Flip_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedIndex = Flip.SelectedIndex;
            FitCurrent();
            UpdatePageIndicator();
            UpdateNavButtonsVisibility();
        }

        private void ItemViewer_Loaded(object sender, RoutedEventArgs e)
        {
            var viewer = sender as ImageViewerControl;
            viewer?.FitToViewport();
        }

        private void FitCurrent()
        {
            if (Flip == null) return;
            var container = Flip.ContainerFromIndex(Flip.SelectedIndex) as DependencyObject;
            if (container == null) return;
            ImageViewerControl viewer = null;
            var queue = new Queue<DependencyObject>();
            queue.Enqueue(container);
            while (queue.Count > 0)
            {
                var d = queue.Dequeue();
                viewer = d as ImageViewerControl;
                if (viewer != null) break;
                var count = VisualTreeHelper.GetChildrenCount(d);
                for (int i = 0; i < count; i++)
                {
                    queue.Enqueue(VisualTreeHelper.GetChild(d, i));
                }
            }
            viewer?.FitToViewport();
        }

        private void Root_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            var keyState = Window.Current.CoreWindow.GetKeyState(VirtualKey.Control);
            if (keyState.HasFlag(CoreVirtualKeyStates.Down)) return;
            var point = e.GetCurrentPoint(this);
            var delta = point.Properties.MouseWheelDelta;
            if (delta > 0) Navigate(-1);
            else if (delta < 0) Navigate(1);
            e.Handled = true;
        }

        private void HideFlipViewNavigation()
        {
            if (Flip == null) return;
            var queue = new Queue<DependencyObject>();
            queue.Enqueue(Flip);
            while (queue.Count > 0)
            {
                var d = queue.Dequeue();
                var btnBase = d as ButtonBase;
                if (btnBase != null)
                {
                    var fe = btnBase as FrameworkElement;
                    var name = fe?.Name ?? string.Empty;
                    if (name == "PreviousButtonHorizontal" || name == "NextButtonHorizontal" ||
                        name == "PreviousButtonVertical" || name == "NextButtonVertical" ||
                        btnBase is Button || btnBase is RepeatButton)
                    {
                        btnBase.Visibility = Visibility.Collapsed;
                        btnBase.IsHitTestVisible = false;
                        btnBase.Opacity = 0;
                    }
                }
                var count = VisualTreeHelper.GetChildrenCount(d);
                for (int i = 0; i < count; i++)
                {
                    queue.Enqueue(VisualTreeHelper.GetChild(d, i));
                }
            }
        }

        private void UpdateNavButtonsVisibility()
        {
            var total = Sources?.Count ?? 0;
            var idx = Flip?.SelectedIndex ?? 0;
            if (PrevButton != null)
                PrevButton.Visibility = (idx <= 0 || total <= 1) ? Visibility.Collapsed : Visibility.Visible;
            if (NextButton != null)
                NextButton.Visibility = (total == 0 || idx >= total - 1) ? Visibility.Collapsed : Visibility.Visible;
        }

        private void Flip_Loaded(object sender, RoutedEventArgs e)
        {
            HideFlipViewNavigation();
            UpdateNavButtonsVisibility();
            if (Flip != null)
            {
                Flip.LayoutUpdated += Flip_LayoutUpdated;
            }
        }

        private void Flip_LayoutUpdated(object sender, object e)
        {
            HideFlipViewNavigation();
        }

        private void UpdatePageIndicator()
        {
            var total = Sources?.Count ?? 0;
            var current = (Flip?.SelectedIndex ?? 0) + 1;
            if (current < 1 && total > 0) current = 1;
            if (PageText != null)
            {
                PageText.Text = total > 0 ? $"{current}/{total}" : string.Empty;
            }
        }
    }
}
