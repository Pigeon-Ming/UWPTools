using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using UWPTools.Controls;

namespace UWPTools.Pages.ControlPages
{
    public sealed partial class ImageViewerControlPage : Page
    {
        private ObservableCollection<ImageSource> _lightboxSources = new ObservableCollection<ImageSource>();
        public ImageViewerControlPage()
        {
            InitializeComponent();
            ThumbsView.ItemsSource = _lightboxSources;
        }

        private async void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            var picker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.Thumbnail,
                SuggestedStartLocation = PickerLocationId.PicturesLibrary
            };
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".bmp");
            picker.FileTypeFilter.Add(".gif");

            var files = await picker.PickMultipleFilesAsync();
            if (files == null || files.Count == 0) return;

            _lightboxSources.Clear();
            foreach (var file in files)
            {
                using (var stream = await file.OpenAsync(FileAccessMode.Read))
                {
                    var bitmap = new BitmapImage();
                    await bitmap.SetSourceAsync(stream);
                    _lightboxSources.Add(bitmap);
                }
            }
        }

        private void ThumbsView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (_lightboxSources.Count == 0) return;
            var clicked = e.ClickedItem as ImageSource;
            var index = _lightboxSources.IndexOf(clicked);
            if (index < 0) index = 0;
            ImageLightboxControl.Show(_lightboxSources, index);
        }
    }
}
