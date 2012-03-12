using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using O2.External.IE.Wrapper;

namespace O2.External.IE.ExtensionMethods
{
    public static class IEScreenshot_ExtensionMethods
    {
        public static Bitmap screenshot(this Uri uri)
        {
            return uri.takeScreenshot();
        }
        
        public static Bitmap takeScreenshot(this Uri uri)
        {
            return O2BrowserIE_Screenshot.open(uri);
        }

        public static Bitmap screenshot(this Uri uri, int width, int height)
        {
            return O2BrowserIE_Screenshot.open(uri.ToString(), width, height);
        }

        public static Bitmap screenshot(this Uri uri, int width, int height, int thumbnailWidth, int thumbnailHeight)
        {
            return O2BrowserIE_Screenshot.open(uri.ToString(), width, height, thumbnailWidth, thumbnailHeight);
        }
    }
}
