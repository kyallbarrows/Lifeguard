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

namespace Lifeguard
{
    class LifeguardBackgroundApp
    {
        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        //TODO: these might be different between free and paid accounts, to save on hosting costs
        private const int MIN_DELAY = 8 * 60 * 1000;
        private const int MAX_DELAY = 11 * 60 * 1000;


        private static String LifeguardMainMutexGuid = "C7FCF167-4792-4FAF-ACBF-D9F0BB3356F9";

        private NotifyIcon trayIcon;
        private ContextMenu trayMenu;

        private static readonly log4net.ILog log =
                    log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static void LogException(Exception e) {
            if (e is AggregateException) {
            }

        }

        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                //TODO: start this via a service that starts automatically, and periodically ensures the app is still running
                new LifeguardBackgroundApp().Start();
            }
            catch (Exception e) {
                LogException(e);
            }
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

            // get application GUID as defined in AssemblyInfo.cs
            string appGuid = LifeguardMainMutexGuid;

            // unique id for global mutex - Global prefix means it is global to the machine
            string mutexId = string.Format("Global\\{{{0}}}", appGuid);

            // Need a place to store a return value in Mutex() constructor call
            bool createdNew;

            // edited by Jeremy Wiebe to add example of setting up security for multi-user usage
            // edited by 'Marc' to work also on localized systems (don't use just "Everyone") 
            var allowEveryoneRule = new MutexAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), MutexRights.FullControl, AccessControlType.Allow);
            var securitySettings = new MutexSecurity();
            securitySettings.AddAccessRule(allowEveryoneRule);

            // edited by MasonGZhwiti to prevent race condition on security settings via VanNguyen
            using (var mutex = new Mutex(false, mutexId, out createdNew, securitySettings))
            {
                // edited by acidzombie24
                var hasHandle = false;
                try
                {
                    try
                    {
                        // note, you may want to time out here instead of waiting forever
                        // edited by acidzombie24
                        // mutex.WaitOne(Timeout.Infinite, false);
                        hasHandle = mutex.WaitOne(5000, false);
                        if (hasHandle == false)
                            throw new TimeoutException("Timeout waiting for exclusive access");
                    }
                    catch (AbandonedMutexException)
                    {
                        // Log the fact that the mutex was abandoned in another process, it will still get acquired
                        hasHandle = true;
                    }

                    Application.EnableVisualStyles();

                    Task.Run(() =>
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
                    });

                    //TODO: some way to shut this thing off
                    while (true)
                    {
                        var token = ConfigRepo.GetToken();
                        if (!string.IsNullOrEmpty(token) && token.Length == 36) {
                            //capture screenshot to temp folder
                            using (Bitmap newImage = CaptureScreenshot())
                            {
                                if (newImage != null)
                                    PostScreenshot(newImage, token);
                            }
                        }

                        //wait between MIN_DELAY and MAX_DELAY milliseconds
                        var r = new Random((int)DateTime.UtcNow.Ticks);
                        Thread.Sleep(r.Next(MIN_DELAY, MAX_DELAY));
                    }

                }
                finally
                {
                    // edited by acidzombie24, added if statement
                    if (hasHandle)
                        mutex.ReleaseMutex();
                }
            }

        }

        private void OnShowForm(object sender, EventArgs e)
        {
            Task.Run(() => {
                var loginForm = new LoginForm();
                Application.Run(loginForm);
            });
        }

        protected String GetToken(String username, String password) {
            var client = new HttpClient();

            var byteArray = Encoding.ASCII.GetBytes(username + ":" + password);
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

            // Get the response.
            var response = client.GetAsync("http://lifeguard.pixelheavyindustries.com/wp-json/lifeguard/v1/token").Result;

            // Get the response content.
            HttpContent responseContent = response.Content;

            // Get the stream of the content.
            using (var reader = new StreamReader(responseContent.ReadAsStreamAsync().Result))
            {
                // Write the output.
                //strip the " that seem to come along with the token
                var output = reader.ReadToEndAsync().Result;
                output = output.Replace("\"", "");
                return output;
            }

        }

        protected void PostScreenshot(Bitmap screenshot, String token) {
            var client = new HttpClient();

            UTF8Encoding encoding = new UTF8Encoding();
            System.IO.MemoryStream stream = new System.IO.MemoryStream();
            screenshot.Save(stream, System.Drawing.Imaging.ImageFormat.Jpeg);
            byte[] imageBytes = stream.ToArray();
            string base64String = Convert.ToBase64String(imageBytes);

            // Create the HttpContent for the form to be posted.
            var requestContent = new FormUrlEncodedContent(new[] {
                new KeyValuePair<string, string>("token", token),
                new KeyValuePair<string, string>("image", base64String)
            });

            // Get the response.
            try
            {
                var response = client.PostAsync("http://lifeguard.pixelheavyindustries.com/wp-json/lifeguard/v1/screenshot",
                    requestContent).Result;

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
                //TODO log error
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
                return null;

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
                                graph.DrawImage(bmpScreenCapture, 
                                                new Rectangle(
                                                    (int)Math.Floor(x), 
                                                    0, 
                                                    (int)Math.Floor(screenShrankenWidth), 
                                                    (int)Math.Floor(scale * shrankenHeight))
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
                var quality = (long)90;
                var ratio = new EncoderParameter(qualityEncoder, quality);
                var codecParams = new EncoderParameters(1);
                codecParams.Param[0] = ratio;
                var jpegCodecInfo = GetEncoderInfo("image/jpeg");
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

                //could just pass raw bmp, but envisioning this being done later if mobile device is off internet connection
                //AttemptImgurUpload(newPath, _accessToken);
                return newPath;
            }
        }

        private static ImageCodecInfo GetEncoderInfo(String mimeType)
        {
            int j;
            ImageCodecInfo[] encoders;
            encoders = ImageCodecInfo.GetImageEncoders();
            for (j = 0; j < encoders.Length; ++j)
            {
                if (encoders[j].MimeType == mimeType)
                    return encoders[j];
            }
            return null;
        }
    }
}
