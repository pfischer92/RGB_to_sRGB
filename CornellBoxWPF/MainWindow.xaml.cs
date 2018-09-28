using System;
using System.Diagnostics;
using System.Numerics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CornellBoxWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        WriteableBitmap gradient;
        WriteableBitmap gradient2;
        WriteableBitmap animImg;

        byte[] pixels;
        int width, height, stride, heightAnim;
        Int32Rect fullimage;
        Stopwatch sw = new Stopwatch();

        Vector3 startColor = new Vector3(0, 1, 0); //green
        Vector3 endColor = new Vector3(1, 0, 0); //red

        float gamma = 2.2f;

        int framecount = -1;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;

            Vector3 v = new Vector3(1, 1, 1);
        }

        private byte[] ToGammaCorrectedBytes(Vector3 color)
        {
            float x = (float)Math.Pow(color.X, 1f / gamma);
            float y = (float)Math.Pow(color.Y, 1f / gamma);
            float z = (float)Math.Pow(color.Z, 1f / gamma);

            byte[] c = { ConvertAndClamp8(x), ConvertAndClamp8(y), ConvertAndClamp8(z) };
            return c;
        }

        private byte[] NotCorrectedBytes(Vector3 color)
        {
            byte[] c = { ConvertAndClamp8(color.X), ConvertAndClamp8(color.Y), ConvertAndClamp8(color.Z) };
            return c;
        }

        private byte ConvertAndClamp8(float n)
        {
            int x = (int)Math.Round(255 * n, 0);

            if (x > 255)
                return 255;

            if (x < 0)
                return 0;

            return (byte)x;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            width = 1000;
            height = 200;
            heightAnim = 500;

            // Rgb24 is sRGB
            gradient = new WriteableBitmap(width, height, 300, 300, PixelFormats.Rgb24, null);
            gradient2 = new WriteableBitmap(width, height, 300, 300, PixelFormats.Rgb24, null);
            animImg = new WriteableBitmap(width, heightAnim, 300, 300, PixelFormats.Rgb24, null);

            for (int y = 0; y < gradient.PixelHeight; y++)
            {
                for (int x = 0; x < gradient.PixelWidth; x++)
                {
                    Vector3 p = startColor - (1f / gradient.PixelWidth * startColor) * x +
                                (1f / gradient.PixelWidth * endColor) * x;

                    SetPixel(x, y, ToGammaCorrectedBytes(p));
                    SetPixel2(x, y, NotCorrectedBytes(p));
                }
            }

            for (int i = 0; i < animImg.PixelHeight; i++)
            {
                for (int u = 0; u < animImg.PixelWidth; u++)
                {
                    Vector3 p = new Vector3((float)i / animImg.PixelHeight, (float)u / animImg.PixelWidth, (float)i / animImg.PixelHeight);
                    SetPixelAnim(u, i, ToGammaCorrectedBytes(p));
                }
            }

            img.Source = gradient;
            img2.Source = gradient2;
            anim.Source = animImg;

            stride = width * ((animImg.Format.BitsPerPixel + 7) / 8);
            pixels = new byte[heightAnim * stride];
            gradient.CopyPixels(pixels, stride, 0);

            fullimage = new Int32Rect(
                0,
                0,
                width,
                heightAnim);

            CompositionTarget.Rendering += CompositionTarget_Rendering;
        }

        private void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            if (framecount++ == 0)
            {
                // Starting timing.
                sw.Start();
            }

            long frameRate = (long)(framecount / this.sw.Elapsed.TotalSeconds);
            if (frameRate > 0)
            {
                // Update elapsed time, number of frames, and frame rate.
                frames.Content = frameRate.ToString();
            }

            for (int i = 0; i < pixels.Length; i++)
            {
                if (i % 2 == 0)
                {
                    pixels[i] += 1;
                }
                else
                {
                    pixels[i] -= 1;
                }
            }

            animImg.WritePixels(fullimage, pixels, stride, 0);
        }

        private void SetPixel(int x, int y, byte[] color)
        {
            Int32Rect pxl = new Int32Rect(
                    x,
                    y,
                    1,
                    1);

            gradient.WritePixels(pxl, color, 4, 0);
        }

        private void SetPixel2(int x, int y, byte[] color)
        {
            Int32Rect pxl = new Int32Rect(
                    x,
                    y,
                    1,
                    1);

            gradient2.WritePixels(pxl, color, 4, 0);
        }

        private void SetPixelAnim(int x, int y, byte[] color)
        {
            Int32Rect pxl = new Int32Rect(
                    x,
                    y,
                    1,
                    1);

            animImg.WritePixels(pxl, color, 4, 0);
        }
    }
}