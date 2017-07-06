using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.IO;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Net.Http;
using System.Configuration;
using System.Security.AccessControl;    //MutexAccessRule
using System.Security.Principal;        //SecurityIdentifier
using System.Threading.Tasks;
using System.Reflection;
using System.Security.Cryptography;
using System.Net.Http.Headers;

namespace Lifeguard
{
    class LifeguardBackgroundApp
    {
        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);


        //TODO: these might be different between free and paid accounts, to save on hosting costs
        private const int MIN_DELAY = 6 * 60 * 1000;
        private const int MAX_DELAY = 10 * 60 * 1000;
        private const double MIN_DIFFERENCE = .05d;

        //TODO: just look for quotes and move all the strings to a string table
        private static String LifeguardMainMutexGuid = "C7FCF167-4792-4FAF-ACBF-D9F0BB3356F9";


        private NotifyIcon trayIcon;
        private ContextMenu trayMenu;

        private Boolean PostScreenshots = false;
        private LifeguardConfiguration Config;
        private String LastHash = "";
        private Bitmap LastBitmap = null;

        private static readonly log4net.ILog log =
                    log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);



        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                //TODO: start this via a service that starts automatically, and periodically ensures the app is still running
                new LifeguardBackgroundApp().Start();
            }
            catch (Exception e) {
                Logger.LogException(e);
            }
        }


            

        protected void Start() {
            //quick and dirty window hider
            try
            {
                Process p = Process.GetCurrentProcess();
                if (p != null)
                {
                    IntPtr handle = p.MainWindowHandle;

                    if (handle != IntPtr.Zero)
                        ShowWindow(handle, 0);
                }
            }
            catch (Exception e) {
                Logger.LogException(e);
            }


            Application.EnableVisualStyles();

            CreateTrayIcon();

            //check if logged in
            Config = ConfigRepo.GetConfig();
            if (String.IsNullOrEmpty(Config.Token))
            {
                //  if not, pop login box
                ShowLoginForm();
            }
            else {
                PostScreenshots = true;
            }

            DoScreenshotLoop();
        }


        private void DoScreenshotLoop() {
            var r = new Random((int)DateTime.UtcNow.Ticks);

            //RunMainLoop may get set to false by a logout
            while (true)
            {
                if (PostScreenshots && !String.IsNullOrEmpty(Config.Token))
                {
                    try
                    {
                        //capture screenshot to temp folder
                        using (Bitmap newImage = CaptureScreenshot())
                        {
                            if (newImage != null)
                                PostScreenshot(newImage, Config.Token, Config.MachineID);
                        }

                        //wait between MIN_DELAY and MAX_DELAY milliseconds
                        Thread.Sleep(r.Next(MIN_DELAY, MAX_DELAY));
                    }
                    catch (Exception e)
                    {
                        Logger.LogException(e);
                        Thread.Sleep(r.Next(MIN_DELAY, MAX_DELAY));
                    }
                }
            }
        }

        private void CreateTrayIcon() {
            Task.Run(() =>
            {
                try
                {
                    trayMenu = new ContextMenu();
                    trayMenu.MenuItems.Add("Lifeguard", OnShowForm);

                    trayIcon = new NotifyIcon();
                    trayIcon.Text = "Lifeguard Accountability";

                    string[] resourceNames = Assembly.GetExecutingAssembly().GetManifestResourceNames();

                    var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Lifeguard.TrayIcon.ico");
                    trayIcon.Icon = new Icon(stream);
                    trayIcon.Click += new System.EventHandler(OnShowForm);

                    // Add menu to tray icon and show it.
                    trayIcon.ContextMenu = trayMenu;
                    trayIcon.Visible = true;
                    Application.Run();
                }
                catch (Exception e)
                {
                    Logger.LogException(e);
                }
            });
        }

        private void OnShowForm(object sender, EventArgs e)
        {
            ShowLoginForm();
        }

        private void ShowLoginForm() { 
            Task.Run(() => {
                try
                {
                    var loggedIn = !String.IsNullOrEmpty(Config.Token);
                    var loginForm = new LoginForm(Config.Username, loggedIn, Config.Token, LoginHappened, LogoutHappened, ShutdownRequested);
                    Application.Run(loginForm);
                }
                catch (Exception ex) {
                    Logger.LogException(ex);
                }
            });
        }

        protected void LoginHappened(String username, String token) {
            Config.Username = username;
            Config.Token = token;
            ConfigRepo.SaveConfig(Config);
            PostScreenshots = true;
        }

        protected void LogoutHappened() {
            PostScreenshots = false;
            Config.Token = "";
            ConfigRepo.SaveConfig(Config);
            PostScreenshots = false;           
        }

        protected void ShutdownRequested()
        {
            Application.Exit();
        }

        protected void PostScreenshot(Bitmap screenshot, string token, string machineId) {
            var client = new HttpClient();

            UTF8Encoding encoding = new UTF8Encoding();
            System.IO.MemoryStream stream = new System.IO.MemoryStream();
            try
            {
                screenshot.Save(stream, System.Drawing.Imaging.ImageFormat.Jpeg);
                byte[] imageBytes = stream.ToArray();
                string newHash = "";

                using (SHA1CryptoServiceProvider sha1 = new SHA1CryptoServiceProvider())
                {
                    newHash = Convert.ToBase64String(sha1.ComputeHash(imageBytes));
                }

                if (newHash == LastHash) {
                    //images are identical, no need to post a screenshot
                    return;
                }

                LastHash = newHash;

                //hashes didn't match, need to check pixels for percentage difference
                //compare new and old images

                //make sure we have a lastbitmap, and that user didn't change screen res on us
                if (LastBitmap != null && LastBitmap.Width == screenshot.Width && LastBitmap.Height == screenshot.Height)
                {
                    int step = 4;
                    double numDifferentPixels = 0;
                    double diffRatio = 1f;
                    double totalPixels = Math.Floor((double)screenshot.Width/(double)step) * Math.Floor((double)screenshot.Height / (double)step);
                    for (int x = 0; x < screenshot.Width; x += step) {
                        for (int y = 0; y < screenshot.Height; y += step)
                        {
                            totalPixels++;
                            if (LastBitmap.GetPixel(x, y).GetHashCode() != screenshot.GetPixel(x, y).GetHashCode())
                            {
                                numDifferentPixels++;
                            }
                        }
                    }

                    if (totalPixels > 0 && (numDifferentPixels / totalPixels) < MIN_DIFFERENCE)
                        return;
                }

                LastBitmap = screenshot;

                string base64String = Convert.ToBase64String(imageBytes);
                //base64String = "/9j/4QAYRXhpZgAASUkqAAgAAAAAAAAAAAAAAP/sABFEdWNreQABAAQAAAA8AAD/4QMxaHR0cDovL25zLmFkb2JlLmNvbS94YXAvMS4wLwA8P3hwYWNrZXQgYmVnaW49Iu+7vyIgaWQ9Ilc1TTBNcENlaGlIenJlU3pOVGN6a2M5ZCI/PiA8eDp4bXBtZXRhIHhtbG5zOng9ImFkb2JlOm5zOm1ldGEvIiB4OnhtcHRrPSJBZG9iZSBYTVAgQ29yZSA1LjYtYzEzOCA3OS4xNTk4MjQsIDIwMTYvMDkvMTQtMDE6MDk6MDEgICAgICAgICI+IDxyZGY6UkRGIHhtbG5zOnJkZj0iaHR0cDovL3d3dy53My5vcmcvMTk5OS8wMi8yMi1yZGYtc3ludGF4LW5zIyI+IDxyZGY6RGVzY3JpcHRpb24gcmRmOmFib3V0PSIiIHhtbG5zOnhtcD0iaHR0cDovL25zLmFkb2JlLmNvbS94YXAvMS4wLyIgeG1sbnM6eG1wTU09Imh0dHA6Ly9ucy5hZG9iZS5jb20veGFwLzEuMC9tbS8iIHhtbG5zOnN0UmVmPSJodHRwOi8vbnMuYWRvYmUuY29tL3hhcC8xLjAvc1R5cGUvUmVzb3VyY2VSZWYjIiB4bXA6Q3JlYXRvclRvb2w9IkFkb2JlIFBob3Rvc2hvcCBDQyAyMDE3IChNYWNpbnRvc2gpIiB4bXBNTTpJbnN0YW5jZUlEPSJ4bXAuaWlkOkZFMzI4MkY5NTQ4QjExRTc5RENBRjI2NzNCOEJBMjYwIiB4bXBNTTpEb2N1bWVudElEPSJ4bXAuZGlkOkZFMzI4MkZBNTQ4QjExRTc5RENBRjI2NzNCOEJBMjYwIj4gPHhtcE1NOkRlcml2ZWRGcm9tIHN0UmVmOmluc3RhbmNlSUQ9InhtcC5paWQ6RkUzMjgyRjc1NDhCMTFFNzlEQ0FGMjY3M0I4QkEyNjAiIHN0UmVmOmRvY3VtZW50SUQ9InhtcC5kaWQ6RkUzMjgyRjg1NDhCMTFFNzlEQ0FGMjY3M0I4QkEyNjAiLz4gPC9yZGY6RGVzY3JpcHRpb24+IDwvcmRmOlJERj4gPC94OnhtcG1ldGE+IDw/eHBhY2tldCBlbmQ9InIiPz7/7gAOQWRvYmUAZMAAAAAB/9sAhAAGBAQEBQQGBQUGCQYFBgkLCAYGCAsMCgoLCgoMEAwMDAwMDBAMDg8QDw4MExMUFBMTHBsbGxwfHx8fHx8fHx8fAQcHBw0MDRgQEBgaFREVGh8fHx8fHx8fHx8fHx8fHx8fHx8fHx8fHx8fHx8fHx8fHx8fHx8fHx8fHx8fHx8fHx//wAARCAAIAAgDAREAAhEBAxEB/8QASgABAAAAAAAAAAAAAAAAAAAACAEBAAAAAAAAAAAAAAAAAAAAABABAAAAAAAAAAAAAAAAAAAAABEBAAAAAAAAAAAAAAAAAAAAAP/aAAwDAQACEQMRAD8AVIP/2Q==";
                // Create the HttpContent for the form to be posted.
                var requestContent = new FormUrlEncodedContent(new[] {
                    new KeyValuePair<string, string>("machineid", machineId),
                    new KeyValuePair<string, string>("imagedata", base64String)
                });

                //                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Authorization", "Token " + token);
                client.DefaultRequestHeaders.Add("Authorization", "Token " + token);
                var response = client.PostAsync(ConfigRepo.GetScreenshotUri(), requestContent).Result;

                // Get the response content.
                HttpContent responseContent = response.Content;

                // Get the stream of the content.
                using (var reader = new StreamReader(responseContent.ReadAsStreamAsync().Result))
                {
                    // Write the output.
                    var output = reader.ReadToEndAsync().Result;
                    Console.WriteLine(output);
                }
            }
            catch (Exception e) {
                Logger.LogException(e);
            }
        }

       

        protected Bitmap CaptureScreenshot()
        {
            //TODO: properly capture layered windows: http://stackoverflow.com/questions/3072349/capture-screenshot-including-semitransparent-windows-in-net/3072580#3072580

            //TODO: this might be different between free and paid accounts, to save on hosting costs.  Paid might have full-res.
            var totalWidth = 0;
            var height = 0;

            foreach (var screen in Screen.AllScreens) {
                totalWidth += screen.Bounds.Width;
                height = Math.Max(height, screen.Bounds.Height);
            }

            var scale = .2f;
            int shrankenWidth = (int)Math.Floor((float)totalWidth * scale);
            int shrankenHeight = (int)Math.Floor((float)height * scale);
            if (shrankenWidth == 0 || shrankenHeight == 0)
            {
                Logger.LogError("Bitmap has 0 width or height " + shrankenWidth + "x" + shrankenHeight);
                return null; 
            }

            try
            {
                Bitmap bmpShranken = new Bitmap(shrankenWidth, shrankenHeight);

                using (var graph = Graphics.FromImage(bmpShranken))
                {
                    graph.InterpolationMode = InterpolationMode.High;
                    graph.CompositingQuality = CompositingQuality.HighQuality;
                    graph.SmoothingMode = SmoothingMode.AntiAlias;
                    graph.FillRectangle(new SolidBrush(Color.Black), new RectangleF(0, 0, shrankenWidth, shrankenHeight));

                    float x = 0.0f;

                    foreach (var screen in Screen.AllScreens)
                    {
                        using (Bitmap bmpScreenCapture = new Bitmap(screen.Bounds.Width, screen.Bounds.Height))
                        {
                            try
                            {
                                using (Graphics g = Graphics.FromImage(bmpScreenCapture))
                                {
                                    g.CopyFromScreen(screen.Bounds.X,
                                                     screen.Bounds.Y,
                                                     0, 0,
                                                     bmpScreenCapture.Size,
                                                     CopyPixelOperation.SourceCopy);

                                    float screenShrankenWidth = scale * bmpScreenCapture.Width;
                                    float screenShrankenHeight = scale * bmpScreenCapture.Height;
                                    graph.DrawImage(bmpScreenCapture,
                                                    new Rectangle(
                                                        (int)Math.Floor(x),
                                                        0,
                                                        (int)Math.Floor(screenShrankenWidth),
                                                        (int)Math.Floor(screenShrankenHeight))
                                                    );
                                    x += screenShrankenWidth;
                                }

                            }
                            catch (Exception e)
                            {
                            }
                        }
                    }
                }

                return bmpShranken;
            }
            catch (Exception e)
            {
                Logger.LogException(e);
                return null;
            }
        }

    }
}
