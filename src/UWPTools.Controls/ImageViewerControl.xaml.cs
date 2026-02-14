using System;
using Windows.Foundation;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace UWPTools.Controls
{
    public sealed partial class ImageViewerControl : UserControl
    {
        private bool _isDragging;
        private Point _dragStart;
        private double _startOffsetX;
        private double _startOffsetY;

        public ImageViewerControl()
        {
            InitializeComponent();
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        public ImageSource Source
        {
            get => (ImageSource)GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register(nameof(Source), typeof(ImageSource), typeof(ImageViewerControl),
                new PropertyMetadata(null, OnSourceChanged));

        public double MinZoom
        {
            get => (double)GetValue(MinZoomProperty);
            set => SetValue(MinZoomProperty, value);
        }

        public static readonly DependencyProperty MinZoomProperty =
            DependencyProperty.Register(nameof(MinZoom), typeof(double), typeof(ImageViewerControl),
                new PropertyMetadata(0.1, OnZoomRangeChanged));

        public double MaxZoom
        {
            get => (double)GetValue(MaxZoomProperty);
            set => SetValue(MaxZoomProperty, value);
        }

        public static readonly DependencyProperty MaxZoomProperty =
            DependencyProperty.Register(nameof(MaxZoom), typeof(double), typeof(ImageViewerControl),
                new PropertyMetadata(10.0, OnZoomRangeChanged));

        public Stretch Stretch
        {
            get => (Stretch)GetValue(StretchProperty);
            set => SetValue(StretchProperty, value);
        }

        public static readonly DependencyProperty StretchProperty =
            DependencyProperty.Register(nameof(Stretch), typeof(Stretch), typeof(ImageViewerControl),
                new PropertyMetadata(Stretch.None, OnStretchChanged));

        private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (ImageViewerControl)d;
            control.ImageElement.Source = (ImageSource)e.NewValue;
            control.Scroller.ChangeView(0, 0, 1.0f, true);
        }

        private static void OnZoomRangeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (ImageViewerControl)d;
            if (control.Scroller != null)
            {
                //control.Scroller.MinZoom = (float)control.MinZoom;
                //control.Scroller.MaxZoom = (float)control.MaxZoom;
            }
        }

        private static void OnStretchChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (ImageViewerControl)d;
            control.ImageElement.Stretch = (Stretch)e.NewValue;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Root.PointerWheelChanged += OnPointerWheelChanged;
            Root.DoubleTapped += OnDoubleTapped;
            Root.PointerPressed += OnPointerPressed;
            Root.PointerMoved += OnPointerMoved;
            Root.PointerReleased += OnPointerReleased;
            Root.PointerCanceled += OnPointerReleased;
            //Scroller.MinZoom = (float)MinZoom;
            //Scroller.MaxZoom = (float)MaxZoom;
            ImageElement.Stretch = Stretch;
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            Root.PointerWheelChanged -= OnPointerWheelChanged;
            Root.DoubleTapped -= OnDoubleTapped;
            Root.PointerPressed -= OnPointerPressed;
            Root.PointerMoved -= OnPointerMoved;
            Root.PointerReleased -= OnPointerReleased;
            Root.PointerCanceled -= OnPointerReleased;
        }

        private void OnPointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            var keyState = Window.Current.CoreWindow.GetKeyState(VirtualKey.Control);
            if (!keyState.HasFlag(CoreVirtualKeyStates.Down))
            {
                return;
            }

            e.Handled = true;
            var pointerPoint = e.GetCurrentPoint(Scroller);
            var delta = pointerPoint.Properties.MouseWheelDelta;
            var currentZoom = Scroller.ZoomFactor;
            var factor = delta > 0 ? 1.1f : 0.9f;
            var targetZoom = Math.Max((float)MinZoom, Math.Min((float)MaxZoom, currentZoom * factor));
            if (Math.Abs(targetZoom - currentZoom) < 0.001f)
            {
                return;
            }

            var pos = pointerPoint.Position;
            var targetH = (Scroller.HorizontalOffset + pos.X) / currentZoom * targetZoom - pos.X;
            var targetV = (Scroller.VerticalOffset + pos.Y) / currentZoom * targetZoom - pos.Y;
            Scroller.ChangeView(targetH, targetV, targetZoom, true);
        }

        public void Reset()
        {
            Scroller.ChangeView(0, 0, 1.0f, true);
        }

        public void FitToViewport()
        {
            if (!(ImageElement.Source is BitmapSource bmp))
            {
                RoutedEventHandler opened = null;
                opened = (s, e) =>
                {
                    ImageElement.ImageOpened -= opened;
                    FitToViewport();
                };
                ImageElement.ImageOpened += opened;
                return;
            }

            if (bmp.PixelWidth == 0 || bmp.PixelHeight == 0 || Scroller.ViewportWidth == 0 || Scroller.ViewportHeight == 0)
            {
                SizeChangedEventHandler sized = null;
                sized = (s, e) =>
                {
                    Scroller.SizeChanged -= sized;
                    FitToViewport();
                };
                Scroller.SizeChanged += sized;
                return;
            }

            var imageW = (double)bmp.PixelWidth;
            var imageH = (double)bmp.PixelHeight;
            var viewW = Scroller.ViewportWidth;
            var viewH = Scroller.ViewportHeight;

            var scaleX = viewW / imageW;
            var scaleY = viewH / imageH;
            var targetZoom = (float)Math.Max(MinZoom, Math.Min(MaxZoom, Math.Min(scaleX, scaleY)));

            var contentW = imageW * targetZoom;
            var contentH = imageH * targetZoom;
            var targetH = Math.Max(0, (contentW - viewW) / 2);
            var targetV = Math.Max(0, (contentH - viewH) / 2);

            Scroller.ChangeView(targetH, targetV, targetZoom, true);
        }

        private float GetFitZoom()
        {
            var bmp = ImageElement.Source as BitmapSource;
            if (bmp == null) return 1.0f;
            if (bmp.PixelWidth == 0 || bmp.PixelHeight == 0 || Scroller.ViewportWidth == 0 || Scroller.ViewportHeight == 0)
            {
                return 1.0f;
            }
            var imageW = (double)bmp.PixelWidth;
            var imageH = (double)bmp.PixelHeight;
            var viewW = Scroller.ViewportWidth;
            var viewH = Scroller.ViewportHeight;
            var scaleX = viewW / imageW;
            var scaleY = viewH / imageH;
            return (float)Math.Max(MinZoom, Math.Min(MaxZoom, Math.Min(scaleX, scaleY)));
        }

        private void OnDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            var fit = GetFitZoom();
            var current = Scroller.ZoomFactor;
            var pos = e.GetPosition(Scroller);
            if (current <= fit + 0.01f)
            {
                var targetZoom = Math.Min((float)MaxZoom, Math.Max(fit * 2.0f, current * 2.0f));
                var targetH = (Scroller.HorizontalOffset + pos.X) / current * targetZoom - pos.X;
                var targetV = (Scroller.VerticalOffset + pos.Y) / current * targetZoom - pos.Y;
                Scroller.ChangeView(targetH, targetV, targetZoom, false);
            }
            else
            {
                var targetZoom = fit;
                var image = ImageElement.Source as BitmapSource;
                if (image != null && Scroller.ViewportWidth > 0 && Scroller.ViewportHeight > 0)
                {
                    var imageW = (double)image.PixelWidth;
                    var imageH = (double)image.PixelHeight;
                    var contentW = imageW * targetZoom;
                    var contentH = imageH * targetZoom;
                    var targetH = Math.Max(0, (contentW - Scroller.ViewportWidth) / 2);
                    var targetV = Math.Max(0, (contentH - Scroller.ViewportHeight) / 2);
                    Scroller.ChangeView(targetH, targetV, targetZoom, false);
                }
                else
                {
                    Scroller.ChangeView(0, 0, targetZoom, false);
                }
            }
        }

        private void OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var p = e.GetCurrentPoint(Root);
            if (!p.Properties.IsLeftButtonPressed)
            {
                return;
            }
            if (Window.Current.CoreWindow.GetKeyState(VirtualKey.Control).HasFlag(CoreVirtualKeyStates.Down))
            {
                return;
            }
            if (Scroller.ScrollableWidth <= 0 && Scroller.ScrollableHeight <= 0)
            {
                return;
            }
            _isDragging = true;
            _dragStart = p.Position;
            _startOffsetX = Scroller.HorizontalOffset;
            _startOffsetY = Scroller.VerticalOffset;
            e.Handled = true;
        }

        private void OnPointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (!_isDragging) return;
            var p = e.GetCurrentPoint(Root);
            if (!p.Properties.IsLeftButtonPressed)
            {
                _isDragging = false;
                return;
            }
            var dx = p.Position.X - _dragStart.X;
            var dy = p.Position.Y - _dragStart.Y;
            var targetH = _startOffsetX - dx;
            var targetV = _startOffsetY - dy;
            Scroller.ChangeView(targetH, targetV, null, true);
            e.Handled = true;
        }

        private void OnPointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (_isDragging)
            {
                _isDragging = false;
                e.Handled = true;
            }
        }
    }
}
