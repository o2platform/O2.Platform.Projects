using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using O2.DotNetWrappers.ExtensionMethods;
using O2.DotNetWrappers.DotNet;
using O2.DotNetWrappers.Network;
using O2.External.IE.Interfaces;
using O2.External.IE.ExtensionMethods;
using O2.External.IE.WebObjects;
using O2.Kernel;

using O2.Views.ASCX.classes.MainGUI;

namespace O2.External.IE.Wrapper
{
    public class O2BrowserIE : ExtendedWebBrowser, IO2Browser        
    {
        public event Action<IO2HtmlPage> onDocumentCompleted;
        public bool DebugMode { get; set;}                      // false by default
        public IO2HtmlPage HtmlPage { get; set;}
        public AutoResetEvent documentCompleted;

        public O2BrowserIE()
        {
            //  Navigated += O2BrowserIE_Navigated;
            //  Navigating += O2BrowserIE_Navigating;

            //this.Navigated += O2BrowserIE_Navigated;

            DocumentComplete += O2BrowserIE_DocumentComplete;
            AllowWebBrowserDrop = false;
            documentCompleted = new AutoResetEvent(false);
            

            /*DocumentCompleted +=
                (sender, e)
                =>
                    {
                        PublicDI.log.debug("Document Complete:{0}" , e.Url);
                        if (onDocumentCompleted != null)
                            onDocumentCompleted(new IE_HtmlPage((O2BrowserIE)sender, e.Url));
                    };*/
            
        }

        public static O2BrowserIE openAsForm()
        {
            var panel = O2Gui.open<System.Windows.Forms.Panel>("Web Browser", 600, 500);
            return (O2BrowserIE)panel.add_WebBrowserWithLocationBar();
        }
       

        /* void O2BrowserIE_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
         //   PublicDI.log.info("Navigating: {0} ({1}", e.Url, e.TargetFrameName);
            //throw new NotImplementedException();
        }

        void O2BrowserIE_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
         //   PublicDI.log.info("Navigated: {0}", e.Url);
        }*/

        void O2BrowserIE_DocumentComplete(object sender, DocumentCompleteEventArgs e)
        {
            
            if (DebugMode)
                PublicDI.log.debug("in O2BrowserIE_DocumentComplete for:{0}", e.Url); 
            ////var uri = (Uri) e.Url;            
            try
            {
                HtmlPage = new IE_HtmlPage(e.DocumentClass);
                documentCompleted.Set();
                if (onDocumentCompleted != null)
                    onDocumentCompleted(HtmlPage);
            }
            catch (Exception ex)
            {
                PublicDI.log.ex(ex,"O2BrowserIE_DocumentComplete:",  true);
                documentCompleted.Set();
            }                        
        }

        public void open(string url)
        {            
           open(url.uri());            
        }

        public void open(Uri uri)
        {
            if (uri != null)
            {
                try
                {
                    if (DebugMode)
                        PublicDI.log.debug("[O2BrowserIE] opening: {0}", uri.ToString());
                    Navigate(uri);
                }
                catch (Exception ex)
                {
                    DI.log.error("in O2BrowserIE.open: {0}", ex.Message);
                    documentCompleted.Set();

                }
            }
            HtmlPage = null; 
        }
        
        /// <summary>
        /// _note: this only works well for pages with no frames (since on those cases the returned IO2HtmlPage will be the first one loaded)
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        /// 
        public IO2HtmlPage openSync(string url)
        {
            return openSync(url.uri());
        }

        public IO2HtmlPage openSync(Uri uri)
        {
            /*documentCompleted.Reset();
            O2Thread.mtaThread(() => open(url));
            documentCompleted.WaitOne();
            return HtmlPage;*/

            documentCompleted.Reset();
            O2Thread.mtaThread(() => open(uri));
            while (documentCompleted.WaitOne(1000*60))  // maxwait is 1 minute
            {
                // hack to handle the case where about:blank was being fired 
                if (HtmlPage.PageUri.str() != "about:blank" && uri.ToString() != "about:blank")
                    return HtmlPage;
            }
            return null; // I should add a time out to this loop                        
        }
        
        public bool HtmlEditMode
        {
            get
            {
                if (Document != null)
                {
                    var doc = (mshtml.IHTMLDocument2) Document.DomDocument;
                    return (doc.designMode == "On");
                }
                O2.Kernel.PublicDI.log.error("in DesignMode.get Document == null");
                return false;
            }
            set
            {
                if (Document != null)
                {
                    var doc = (mshtml.IHTMLDocument2)Document.DomDocument;
                    doc.designMode = value ? "On" : "Off";
                }
                else
                    PublicDI.log.error("in DesignMode.get Document == null");
            }
        }

        public void submitRequest_POST(string url, string targetFrame, Dictionary<string, string> parameters)
        {
            var postString = "";
            if (parameters != null)
                foreach (var parameter in parameters.Keys)
                    postString += string.Format("{0}={1}&", parameter, WebEncoding.urlEncode(parameters[parameter]));
            submitRequest_POST(url, targetFrame, postString);
        }

        public void submitRequest_POST(string url, string targetFrame, string postString)
        {
            byte[] postData = Encoding.ASCII.GetBytes(postString);
            submitRequest_POST(url, targetFrame, postData);
        }

        public void submitRequest_POST(string url, string targetFrame, byte[] postData)
        {
            try
            {
                var uri = new Uri(url);
                const string additionalHeaders = "Content-Type: application/x-www-form-urlencoded";
                Navigate(uri, targetFrame, postData, additionalHeaders);
            }
            catch (Exception ex)
            {
                PublicDI.log.error("in submitRequest_POST", ex.Message);
            }
            
        }

        public void submitRequest_GET(string url)
        {
            submitRequest_GET(url, "", "");
        }

        public void submitRequest_GET(string url, string targetFrame, Dictionary<string, string> parameters)
        {
            var parametersString = "";
            if (parameters != null)            
                foreach (var parameter in parameters.Keys)
                    parametersString += string.Format("{0}={1}&", parameter, WebEncoding.urlEncode(parameters[parameter]));
            submitRequest_GET(url, targetFrame, parametersString);              
        }   
     
        public void submitRequest_GET(string url, string targetFrame, string parametersString)
        {
            try
            {
                if (parametersString.valid())
                    url += "?" + parametersString;
                var uri = new Uri(url);
                Navigate(uri, targetFrame);    
            }
            catch (Exception ex)
            {
                PublicDI.log.error("in submitRequest_GET", ex.Message);                
            }
            
        }

        public IO2HtmlPage submitRequest_POST_Sync(string url, string targetFrame, Dictionary<string, string> parameters)
        {
            documentCompleted.Reset();
            O2Thread.mtaThread(() => submitRequest_POST(url, targetFrame, parameters));
            documentCompleted.WaitOne();
            return HtmlPage;
        }

        public IO2HtmlPage submitRequest_POST_Sync(string url, string targetFrame, string postString)
        {
            documentCompleted.Reset();
            O2Thread.mtaThread(() => submitRequest_POST(url, targetFrame, postString));
            documentCompleted.WaitOne();
            return HtmlPage;
        }

        public IO2HtmlPage submitRequest_POST_Sync(string url, string targetFrame, byte[] postData)
        {
            documentCompleted.Reset();
            O2Thread.mtaThread(() => submitRequest_POST(url, targetFrame, postData));
            documentCompleted.WaitOne();
            return HtmlPage;
        }

        public IO2HtmlPage submitRequest_GET_Sync(string url, string targetFrame, string parametersString)
        {
            documentCompleted.Reset();
            O2Thread.mtaThread(() => submitRequest_GET(url, targetFrame, parametersString));
            documentCompleted.WaitOne();
            return HtmlPage;
        }

        public IO2HtmlPage submitRequest_GET_Sync(string url, string targetFrame, Dictionary<string, string> parameters)
        {
            documentCompleted.Reset();
            O2Thread.mtaThread(() => submitRequest_GET(url, targetFrame, parameters));
            documentCompleted.WaitOne();
            return HtmlPage;
        }


    }
}