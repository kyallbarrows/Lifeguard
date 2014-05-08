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
using System.Net;
using System.Text.RegularExpressions;
using System.Collections.Specialized;
using Newtonsoft.Json;

namespace Lifeguard
{
    class LifeguardBackgroundApp
    {
        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        //TODO: these might be different between free and paid accounts, to save on hosting costs
        private const int MIN_DELAY = 8 * 60 * 1000;
        private const int MAX_DELAY = 11 * 60 * 1000;

        private string _accessToken;

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

            _accessToken = GetAccessToken();

            //TODO: some way to shut this thing off
            while (true)
            {
                //capture screenshot to temp folder
                var newImagePath = CaptureScreenshotToTemp();

                //wait between MIN_DELAY and MAX_DELAY milliseconds
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
                AttemptImgurUpload(newPath, _accessToken);
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

        private string ClientId = "b60c423c4c3cc91";
        private string ClientSecret = "32bada41201f47856f416b3240bdd10a0b7b389d";
        //hard-coded refresh token.  Do I win some kind of prize?
        private string RefreshToken = "fd539ca9ecec7978b42ce3e80d0060ca268ed1c7";

        private string GetAccessToken()
        {
            var endpoint = "https://api.imgur.com/oauth2/token";
            string response = "";
            using (var wb = new WebClient())
            {
                var data = new NameValueCollection();
                data["refresh_token"] = RefreshToken;
                data["client_id"] = ClientId;
                data["client_secret"] = ClientSecret;
                data["grant_type"] = "refresh_token";

                response = Encoding.ASCII.GetString(wb.UploadValues(endpoint, "POST", data));

            }

            RefreshTokenResponse rtr = JsonConvert.DeserializeObject<RefreshTokenResponse>(response);

            return rtr.access_token;
        }

        public class RefreshTokenResponse {
            public String access_token;
            public int expires_in;
            public string token_type;
            public string scope;
            public string refresh_token;
            public string account_username;
        }

        //sends to imgur with no validation of success
        private string AttemptImgurUpload(string imagePath, string accessToken, bool allowGetNewAccessToken = true)
        {
            
            WebClient w = new WebClient();
            w.Headers.Add("Authorization", "Bearer " + accessToken);
//            w.Headers.Add("Authorization", "Bearer " + accessToken); 
            System.Collections.Specialized.NameValueCollection Keys = new System.Collections.Specialized.NameValueCollection(); 
            try {
                Keys.Add("image", Convert.ToBase64String(File.ReadAllBytes(imagePath)));
                Keys.Add("album", "YWMzE");
                byte[] responseArray = w.UploadValues("https://api.imgur.com/3/image", Keys); 
                dynamic result = Encoding.ASCII.GetString(responseArray); 
                System.Text.RegularExpressions.Regex reg = new System.Text.RegularExpressions.Regex("link\":\"(.*?)\""); 
                Match match = reg.Match(result); 
                string url = match.ToString().Replace("link\":\"", "").Replace("\"", "").Replace("\\/", "/"); 
                return url; 
            } 
            catch (WebException we) { 
                //try it again with new token
                _accessToken = GetAccessToken();
                return AttemptImgurUpload(imagePath, _accessToken, false);
            } 
            catch (Exception e){
                return "";
            }

        }
    }
}
