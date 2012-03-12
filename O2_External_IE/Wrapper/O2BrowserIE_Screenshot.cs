using System.Windows.Forms;
using System.Drawing;
using System.Net;
using mshtml;
using System.Reflection;
using System.Runtime.InteropServices;
using System;
using System.Drawing.Drawing2D;
using O2.Kernel.ExtensionMethods;
using O2.DotNetWrappers.ExtensionMethods;
using O2.External.IE.ExtensionMethods;
using O2.DotNetWrappers.DotNet;

namespace O2.External.IE.Wrapper
{

    // [Dinis Cruz] Screenshot Feature (allow the creation of a screenshot of the loaded page (works even if loaded in background))
    // code sample from http://www.wincustomize.com/articles.aspx?aid=136426&c=1
    // most comments below are from Adam's original article

    /// Code by Adam Najmanowicz
    /// http://www.codeproject.com/script/Membership/Profiles.aspx?mid=923432
    /// http://blog.najmanowicz.com/
    /// 
    /// Some improvements suggested by Frank Herget
    /// http://www.artviper.net/
    /// 

    public class O2BrowserIE_Screenshot : O2BrowserIE
    {
        public static int default_Width = 1024;
        public static int default_Height = 800;
        public static int wait = 25000;
        public new string Url { get; set; }
        public int Width_Browser { get; set; }
        public int Height_Browser { get; set; }
        public int Width_Bitmap { get; set; }
        public int Height_Bitmap { get; set; }        

        //bool ScrollBarsEnabled { get; set; }
        
        // making this private so that we don't invoke it by accident (since this must be in a STA Thread)
        private O2BrowserIE_Screenshot(string url, int width, int height)
        {
            //this.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(documentCompletedEventHandler);
            Width_Browser = width;
            Height_Browser = height;
            Width_Bitmap = width;           // by default we don't create tumbnails
            Height_Bitmap = height;
            //wait = 20000;                  // by default don't wait for more than 20sec
            Url = url;            
            this.Size = new Size(width, height);
            this.ScrollBarsEnabled = false;
            this.silent(true);
        }

        private O2BrowserIE_Screenshot(string url)
            : this(url, default_Width , default_Height)
        {
        }

        private O2BrowserIE_Screenshot()
            : this("about:blank")
        {            
        }

        public Bitmap getScreenshot()
        { 
            return getScreenshot(Url);
        }

        public Bitmap getTumbnail()
        {
            Width_Bitmap = 100;
            Height_Bitmap = 100;
            return getScreenshot(Url);
        }

        public new static Bitmap open(Uri uri)
        {
            return getScreenshot(uri.ToString());
        }

        public new static Bitmap open(string url)
        {
            return getScreenshot(url);
        }

        public static Bitmap open(string url, int width, int height)
        {
            return getScreenshot(url, width, height, width, height);
        }

        public static Bitmap open(string url, int width, int height, int thumbnailWidth, int thumbnailHeight)
        {
            return getScreenshot(url, width, height, thumbnailWidth, thumbnailHeight);
        }

        public static Bitmap getScreenshot(string url)
        {
            return getScreenshot(url, O2BrowserIE_Screenshot.default_Width, O2BrowserIE_Screenshot.default_Height,
                                      O2BrowserIE_Screenshot.default_Width, O2BrowserIE_Screenshot.default_Height); 
        }

        public static Bitmap getScreenshot(string url, int width_Browser, int height_Browser ,
                                                       int width_Bitmap, int height_Bitmap)
        {
            Bitmap bitmap = null ;
            var thread = O2Thread.staThread(
                () =>
                {
                    var browser = new O2BrowserIE_Screenshot();
                    browser.Width_Browser = width_Browser;
                    browser.Height_Browser = height_Browser;
                    browser.Width_Bitmap = width_Bitmap;
                    browser.Height_Bitmap = height_Bitmap;

                    browser.fetchWebPage(url);
                    bitmap =  browser.GetBitmap();
                });
            var result = thread.Join(wait);
            if (result.isFalse())
                "thread result: {0}".format(result).debug();
            if (bitmap == null)
                "in O2BrowserIE_Screenshot.getScreenshot, failed to get screenshot for: {0}".format(url).error();
            return bitmap;
        }

        internal Bitmap GetBitmap()
        {
            IHTMLDocument2 rawDoc = (IHTMLDocument2)this.Document.DomDocument;
            IHTMLElement rawBody = rawDoc.body;
            IHTMLElementRender2 render = (IHTMLElementRender2)rawBody;

            Bitmap bitmap = new Bitmap(Width_Browser, Height_Browser);
            Rectangle bitmapRect = new Rectangle(0, 0, Width_Browser, Height_Browser);

            // Interesting thing that despite using the renderer later 
            // this following line is still necessary or 
            // the background may not be painted on some websites.
            this.DrawToBitmap(bitmap, bitmapRect);

            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                IntPtr graphicshdc = graphics.GetHdc();
                render.DrawToDC(graphicshdc);

                graphics.ReleaseHdc(graphicshdc);
                graphics.Dispose();

                if (Height_Bitmap == Height_Browser && Width_Bitmap == Width_Browser)
                {
                    return bitmap;
                }
                else
                {
                    Bitmap thumbnail = new Bitmap(Width_Bitmap, Height_Bitmap);
                    using (Graphics gfx = Graphics.FromImage(thumbnail))
                    {
                        // high quality image sizing
                        gfx.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                        gfx.InterpolationMode = InterpolationMode.HighQualityBicubic;                                                                       // make it look pretty 
                        gfx.DrawImage(bitmap, new Rectangle(0, 0, Width_Bitmap, Height_Bitmap), bitmapRect, GraphicsUnit.Pixel);
                    }
                    bitmap.Dispose();
                    return thumbnail;
                }
            }            
        }

        public bool fetchWebPage(string url)
        {
            try
            {
                this.Navigate(url);
                while (this.ReadyState != WebBrowserReadyState.Complete)
                {
                    System.Windows.Forms.Application.DoEvents();
                }
                return true;
            }
            catch (Exception ex)
            {
                ex.log("in O2BrowserIE_Screenshot.fetchWebPage");
            }

            return false;
        }
        
    }



    /// <summary>
    /// Thanks for the solution to the "sometimes not painting sites to Piers Lawson
    /// Who performed some extensive research regarding the origianl implementation.
    /// You can find his codeproject profile here:
    /// http://www.codeproject.com/script/Articles/MemberArticles.aspx?amid=39324
    /// </summary>
    [InterfaceType(1)]
    [Guid("3050F669-98B5-11CF-BB82-00AA00BDCE0B")]
    public interface IHTMLElementRender2
    {
        void DrawToDC(IntPtr hdc);
        void SetDocumentPrinter(string bstrPrinterName, ref _RemotableHandle hdc);
    }
}
