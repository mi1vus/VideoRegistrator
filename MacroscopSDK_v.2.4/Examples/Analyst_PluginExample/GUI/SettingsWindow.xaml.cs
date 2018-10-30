using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Analyst_PluginExample.GUI
{
    public partial class SettingsWindow
    {
        private WriteableBitmap wBmp;

        public SettingsWindow()
        {
            InitializeComponent();
        }

        public void OnNewFrameCame(int width, int height, int offset, int stride, byte[] pixels, int bpp)
        {
            if (wBmp == null || wBmp.Width != width || wBmp.Height != height)
            {
                wBmp = new WriteableBitmap(width, height, 96.0, 96.0, PixelFormats.Bgr24, null);
                frameRender.Source = wBmp;
            }

            wBmp.WritePixels(new Int32Rect(0, 0, width, height), pixels, stride, 0);
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
