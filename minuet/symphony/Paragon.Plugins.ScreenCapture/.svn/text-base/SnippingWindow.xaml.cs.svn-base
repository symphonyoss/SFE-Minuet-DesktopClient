using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Ink;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Paragon.Plugins.ScreenCapture
{
    /// <summary>
    /// Interaction logic for SnippingWindow.xaml
    /// </summary>
    public partial class SnippingWindow : Window
    {
        private readonly DrawingAttributes[] highlightColors;
        private readonly DrawingAttributes[] penColors;
        private DrawingAttributes selectedHighlightColor;
        private DrawingAttributes selectedPenColor;

        public SnippingWindow()
        {
            InitializeComponent();

            penColors = ((DrawingAttributes[]) FindResource("PenColors"));
            highlightColors = ((DrawingAttributes[]) FindResource("HighlightColors"));

            selectedPenColor = penColors.First();
            selectedHighlightColor = highlightColors.First();

            Loaded += OnLoaded;
        }

        public ScreenSnippet Snippet { get; set; }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            var result = SnippingTool.TakeSnippet();

            if (result == null)
            {
                Close();
                return;
            }

            WindowState = WindowState.Normal;

            var rect = result.SelectedRectangle;

            // reposition the window so there's a neat effect of showing 
            // the screenshot edit window in place of the selected region
            var top = rect.Top - 80;
            var left = rect.Left - 80;

            Top = top > 0 ? top : 0;
            Left = left > 0 ? left : 0;

            var image = result.Image;

            ImageBrush.ImageSource = result.ImageToBitmapSource();

            var width = 400;
            // adjust window size to be slightly larger than 
            // the image so nothing is cropped
            if (image.Width > width && image.Width > Width)
            {
                width = image.Width + 100;
            }

            var height = 300;

            if (image.Height > height && image.Height > Height)
            {
                height = image.Height + 100;
            }

            Width = width;
            Height = height;

            // adjust canvas that is hosting the image to match the image size
            InkCanvas.Width = image.Width;
            InkCanvas.Height = image.Height;
        }

        private void OnColorClick(object sender, RoutedEventArgs e)
        {
            var menuItem = (MenuItem) sender;
            var color = (DrawingAttributes) menuItem.Header;

            InkCanvas.DefaultDrawingAttributes = color;

            if (penColors.Contains(color))
            {
                selectedPenColor = color;
            }
            else
            {
                selectedHighlightColor = color;
            }
        }

        private void OnPenClick(object sender, RoutedEventArgs e)
        {
            InkCanvas.EditingMode = InkCanvasEditingMode.Ink;
            InkCanvas.DefaultDrawingAttributes = selectedPenColor;

            ShowContextMenu(sender as ToggleButton, PenContextMenu);
        }

        private void OnHighlightClick(object sender, RoutedEventArgs e)
        {
            InkCanvas.EditingMode = InkCanvasEditingMode.Ink;
            InkCanvas.DefaultDrawingAttributes = selectedHighlightColor;

            ShowContextMenu(sender as ToggleButton, HighlightContextMenu);
        }

        private void OnEraseClick(object sender, RoutedEventArgs e)
        {
            InkCanvas.EditingMode = InkCanvasEditingMode.EraseByStroke;
        }

        private void OnDoneClick(object sender, RoutedEventArgs e)
        {
            var renderTargetBitmap = new RenderTargetBitmap(
                (int) InkCanvas.Width,
                (int) InkCanvas.Height,
                96d,
                96d,
                PixelFormats.Default);

            renderTargetBitmap.Render(InkCanvas);

            var jpegEncoder = new JpegBitmapEncoder();
            jpegEncoder.QualityLevel = 70;
            jpegEncoder.Frames.Add(BitmapFrame.Create(renderTargetBitmap));

            using (var stream = new MemoryStream())
            {
                jpegEncoder.Save(stream);

                var snippet = new ScreenSnippet();
                snippet.Bytes = stream.ToArray();
                snippet.ImageType = "jpeg";

                Snippet = snippet;
            }

            Close();
        }

        private void ShowContextMenu(ToggleButton sender, ContextMenu contextMenu)
        {
            contextMenu.Placement = PlacementMode.Bottom;
            contextMenu.PlacementTarget = sender;
            contextMenu.IsOpen = true;
        }
    }
}