using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.IO;
using System.Diagnostics;
using System.Drawing.Drawing2D;

namespace Lifeguard
{
    class LifeguardBackgroundApp
    {
        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        //TODO: these might be different between free and paid accounts, to save on hosting costs
        private const int MIN_DELAY = 4 * 60 * 1000;
        private const int MAX_DELAY = 7 * 60 * 1000;

        private Thread _workerThread;

        static void Main(string[] args)
        {
            //TODO: start this via a service that starts automatically, and periodically ensures the app is still running
            new LifeguardBackgroundApp().Start();
        }

        protected void Start() {
            //quick and dirty window hider
            Process p = Process.GetCurrentProcess();
            if (p != null)
            {
                IntPtr handle = p.MainWindowHandle;

                if (handle != IntPtr.Zero)
                    ShowWindow(handle, 0);
            }

            //TODO: some way to shut this thing off
            while (true)
            {
                //capture screenshot to temp folder
                var newImagePath = CaptureScreenshotToTemp();

                //wait 10 minutes, +/- a minute
                var r = new Random((int)DateTime.UtcNow.Ticks);
                Thread.Sleep(r.Next(MIN_DELAY, MAX_DELAY));
            }
        }


        protected String CaptureScreenshotToTemp()
        {
            //TODO: properly capture layered windows: http://stackoverflow.com/questions/3072349/capture-screenshot-including-semitransparent-windows-in-net/3072580#3072580

            //TODO: this might be different between free and paid accounts, to save on hosting costs.  Paid might have full-res.
            var scale = .2f;
            var origWidth = Screen.PrimaryScreen.Bounds.Width;
            var origHeight = Screen.PrimaryScreen.Bounds.Height;
            int shrankenWidth = (int)Math.Floor((float)origWidth * scale);
            int shrankenHeight = (int)Math.Floor((float)origHeight * scale);

            using (Bitmap bmpScreenCapture = new Bitmap(origWidth, origHeight))
            using (Bitmap bmpShranken = new Bitmap(shrankenWidth, shrankenHeight))
            {
                try
                {
                    using (Graphics g = Graphics.FromImage(bmpScreenCapture))
                    {
                        g.CopyFromScreen(Screen.PrimaryScreen.Bounds.X,
                                         Screen.PrimaryScreen.Bounds.Y,
                                         0, 0,
                                         bmpScreenCapture.Size,
                                         CopyPixelOperation.SourceCopy);
                    }
                }
                catch (Exception e)
                {
                }

                var qualityEncoder = System.Drawing.Imaging.Encoder.Quality;
                var quality = (long)60;
                var ratio = new EncoderParameter(qualityEncoder, quality);
                var codecParams = new EncoderParameters(1);
                codecParams.Param[0] = ratio;
                var jpegCodecInfo = ImageCodecInfo.GetImageEncoders()[0];
                var newPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".jpg");

                using (var graph = Graphics.FromImage(bmpShranken))
                {
                    // uncomment for higher quality output
                    graph.InterpolationMode = InterpolationMode.High;
                    graph.CompositingQuality = CompositingQuality.HighQuality;
                    graph.SmoothingMode = SmoothingMode.AntiAlias;

                    graph.FillRectangle(new SolidBrush(Color.Black), new RectangleF(0, 0, shrankenWidth, shrankenHeight));
                    graph.DrawImage(bmpScreenCapture, new Rectangle(0, 0, shrankenWidth, shrankenHeight));
                }

                //TODO: upload this bizzle to the backend and delete the temp file
                //TODO: check for duplicate of last snapshot via hashing (or better yet imagemagick so they can be 1% different, like if just the clock changed)
                bmpShranken.Save(newPath, jpegCodecInfo, codecParams); // Save to JPG
                return newPath;
            }
        }

    }
}
