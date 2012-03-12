using System;
using System.Windows.Forms;
using System.ComponentModel;
using System.Runtime.InteropServices;
using mshtml;
using O2.DotNetWrappers.DotNet;
using O2.Kernel;
using O2.Kernel.CodeUtils;

// this file uses code from: http://blogs.artinsoft.net/mrojas/archive/2008/09/18/newwindow2-events-in-the-c-webbrowsercontrol.aspx

namespace O2.External.IE.Wrapper
{
    public class NewWindow2EventArgs : CancelEventArgs
    {

        object ppDisp;

        public object PPDisp
        {
            get { return ppDisp; }
            set { ppDisp = value; }
        }


        public NewWindow2EventArgs(ref object ppDisp, ref bool cancel)
            : base()
        {
            this.ppDisp = ppDisp;
            this.Cancel = cancel;
        }
    }

    public class DocumentCompleteEventArgs : EventArgs
    {
        public ExtendedWebBrowser.IWebBrowser2 WebBrowser2 { get; set; }        // DC
        public HTMLDocumentClass DocumentClass { get; set; }                    // DC
        public string Url { get; set; }                                         // DC
        public string PageSource { get; set; }                                  // DC

        //private object ppDisp;
        //private object url;
                                

        public DocumentCompleteEventArgs(object ppDisp, object url)
        {
            Url = (string)url;
            if (ppDisp is ExtendedWebBrowser.IWebBrowser2)
            {
                WebBrowser2 = (ExtendedWebBrowser.IWebBrowser2) ppDisp;
                
                if (WebBrowser2.Document is HTMLDocumentClass)
                {
                    DocumentClass = (HTMLDocumentClass) WebBrowser2.Document;
                    
                    if (DocumentClass.documentElement is HTMLHtmlElementClass)
                    {
                        var documentElement = (HTMLHtmlElementClass)DocumentClass.documentElement;
                        PageSource = documentElement.outerHTML;
                        //if (DocumentClass.body != null)
                        //PageSource = DocumentClass.body.outerHTML;
                    }
                }
            }
        }
    }

    public class CommandStateChangeEventArgs : EventArgs
    {
        private long command;
        private bool enable;
        public CommandStateChangeEventArgs(long command, ref bool enable)
        {
            this.command = command;
            this.enable = enable;
        }

        public long Command
        {
            get { return command; }
            set { command = value; }
        }

        public bool Enable
        {
            get { return enable; }
            set { enable = value; }
        }
    }

    public class ExtendedWebBrowser : WebBrowser
    {
        AxHost.ConnectionPointCookie cookie;
        WebBrowserExtendedEvents events;


        //This method will be called to give you a chance to create your own event sink
        protected override void CreateSink()
        {
            //MAKE SURE TO CALL THE BASE or the normal events won't fire
            base.CreateSink();
            events = new WebBrowserExtendedEvents(this);
            cookie = new AxHost.ConnectionPointCookie(this.ActiveXInstance, events, typeof(DWebBrowserEvents2));

        }

        public object Application
        {
            get
            {
                IWebBrowser2 axWebBrowser = this.ActiveXInstance as IWebBrowser2;
                if (axWebBrowser != null)
                {
                    return axWebBrowser.Application;
                }
                else
                    return null;
            }
        }

        protected override void DetachSink()
        {
            if (null != cookie)
            {
                cookie.Disconnect();
                cookie = null;
            }
            base.DetachSink();
        }

        //This new event will fire for the NewWindow2
        public event EventHandler<NewWindow2EventArgs> NewWindow2;

        protected void OnNewWindow2(ref object ppDisp, ref bool cancel)
        {
            EventHandler<NewWindow2EventArgs> h = NewWindow2;
            NewWindow2EventArgs args = new NewWindow2EventArgs(ref ppDisp, ref cancel);
            if (null != h)
            {
                h(this, args);
            }
            //Pass the cancellation chosen back out to the events
            //Pass the ppDisp chosen back out to the events
            cancel = args.Cancel;
            ppDisp = args.PPDisp;
        }


        //This new event will fire for the DocumentComplete
        public event EventHandler<DocumentCompleteEventArgs> DocumentComplete;

        protected void OnDocumentComplete(object ppDisp, object url)
        {            
            EventHandler<DocumentCompleteEventArgs> h = DocumentComplete;
            var args = new DocumentCompleteEventArgs(ppDisp, url);
            if (null != h)
            {
                h(this, args);
            }
            //Pass the ppDisp chosen back out to the events
            ppDisp = args.WebBrowser2;                         
        }

        //This new event will fire for the DocumentComplete
        public event EventHandler<CommandStateChangeEventArgs> CommandStateChange;

        protected void OnCommandStateChange(long command, ref bool enable)
        {
            EventHandler<CommandStateChangeEventArgs> h = CommandStateChange;
            CommandStateChangeEventArgs args = new CommandStateChangeEventArgs(command, ref enable);
            if (null != h)
            {
                h(this, args);
            }
        }

        //DC
        // not sure how to make this an event since I want the references be passed to the caller
        public event Action<object, object, object, object> BeforeNavigate;

        internal void OnBeforeNavigate2(object pDisp, ref object URL, ref object flags, ref object targetFrameName, ref object postData, ref object headers, ref bool cancel)
        {           
            var methodParameters = new [] {URL, flags, postData, headers};
            Callbacks.raiseRegistedCallbacks(BeforeNavigate, methodParameters);            
        }

        public class BeforeNavigateEventArgs
        {
            public object URL;
        }

        //This class will capture events from the WebBrowser
        public class WebBrowserExtendedEvents : StandardOleMarshalObject, DWebBrowserEvents2
        {
            ExtendedWebBrowser _Browser;
            public WebBrowserExtendedEvents(ExtendedWebBrowser browser)
            { _Browser = browser; }

            //Implement whichever events you wish
            public void NewWindow2(ref object pDisp, ref bool cancel)
            {
                _Browser.OnNewWindow2(ref pDisp, ref cancel);
            }

            //Implement whichever events you wish
            public void DocumentComplete(object pDisp, ref object url)
            {
                _Browser.OnDocumentComplete(pDisp, url);
            }

            //Implement whichever events you wish
            public void CommandStateChange(long command, bool enable)
            {
                _Browser.OnCommandStateChange(command, ref enable);
            }

            public void NavigateComplete2(object pDisp, ref object url)
            {
                //PublicDI.log.debug("***** NavigateComplete2: {0}", url);
                //PublicDI.log.debug("***** type: {0}", PublicDI.reflection.getComObjectTypeName(pDisp));                                
            }

            public void BeforeNavigate2(object pDisp, ref object URL, ref object flags, ref object targetFrameName, ref object postData,  ref object headers, ref bool cancel)
            {
                _Browser.OnBeforeNavigate2(pDisp, ref URL, ref flags, ref targetFrameName, ref postData, ref headers,
                                           ref cancel);
            }
        }
        [ComImport, Guid("34A715A0-6587-11D0-924A-0020AFC7AC4D"), InterfaceType(ComInterfaceType.InterfaceIsIDispatch), TypeLibType(TypeLibTypeFlags.FHidden)]
        public interface DWebBrowserEvents2
        {
            [DispId(0x69)]
            void CommandStateChange([In] long command, [In] bool enable);
            [DispId(0x103)]
            void DocumentComplete([In, MarshalAs(UnmanagedType.IDispatch)] object pDisp, [In] ref object URL);
            [DispId(0xfb)]
            void NewWindow2([In, Out, MarshalAs(UnmanagedType.IDispatch)] ref object pDisp, [In, Out] ref bool cancel);

            //DC 
            [DispId(0xfc)]
            void NavigateComplete2([In, MarshalAs(UnmanagedType.IDispatch)] object pDisp, [In] ref object URL);

            [DispId(250)]
            void BeforeNavigate2([In, MarshalAs(UnmanagedType.IDispatch)] object pDisp, [In] ref object URL,
                                 [In] ref object flags, [In] ref object targetFrameName, [In] ref object postData,
                                 [In] ref object headers, [In, Out] ref bool cancel);
        }

        [ComImport, Guid("D30C1661-CDAF-11d0-8A3E-00C04FC9E26E"), TypeLibType(TypeLibTypeFlags.FOleAutomation | TypeLibTypeFlags.FDual | TypeLibTypeFlags.FHidden)]
        public interface IWebBrowser2
        {
            [DispId(100)]
            void GoBack();
            [DispId(0x65)]
            void GoForward();
            [DispId(0x66)]
            void GoHome();
            [DispId(0x67)]
            void GoSearch();
            [DispId(0x68)]
            void Navigate([In] string Url, [In] ref object flags, [In] ref object targetFrameName, [In] ref object postData, [In] ref object headers);
            [DispId(-550)]
            void Refresh();
            [DispId(0x69)]
            void Refresh2([In] ref object level);
            [DispId(0x6a)]
            void Stop();
            [DispId(200)]
            object Application { [return: MarshalAs(UnmanagedType.IDispatch)] get; }
            [DispId(0xc9)]
            object Parent { [return: MarshalAs(UnmanagedType.IDispatch)] get; }
            [DispId(0xca)]
            object Container { [return: MarshalAs(UnmanagedType.IDispatch)] get; }
            [DispId(0xcb)]
            object Document { [return: MarshalAs(UnmanagedType.IDispatch)] get; }
            [DispId(0xcc)]
            bool TopLevelContainer { get; }
            [DispId(0xcd)]
            string Type { get; }
            [DispId(0xce)]
            int Left { get; set; }
            [DispId(0xcf)]
            int Top { get; set; }
            [DispId(0xd0)]
            int Width { get; set; }
            [DispId(0xd1)]
            int Height { get; set; }
            [DispId(210)]
            string LocationName { get; }
            [DispId(0xd3)]
            string LocationURL { get; }
            [DispId(0xd4)]
            bool Busy { get; }
            [DispId(300)]
            void Quit();
            [DispId(0x12d)]
            void ClientToWindow(out int pcx, out int pcy);
            [DispId(0x12e)]
            void PutProperty([In] string property, [In] object vtValue);
            [DispId(0x12f)]
            object GetProperty([In] string property);
            [DispId(0)]
            string Name { get; }
            [DispId(-515)]
            int HWND { get; }
            [DispId(400)]
            string FullName { get; }
            [DispId(0x191)]
            string Path { get; }
            [DispId(0x192)]
            bool Visible { get; set; }
            [DispId(0x193)]
            bool StatusBar { get; set; }
            [DispId(0x194)]
            string StatusText { get; set; }
            [DispId(0x195)]
            int ToolBar { get; set; }
            [DispId(0x196)]
            bool MenuBar { get; set; }
            [DispId(0x197)]
            bool FullScreen { get; set; }
            [DispId(500)]
            void Navigate2([In] ref object URL, [In] ref object flags, [In] ref object targetFrameName, [In] ref object postData, [In] ref object headers);
            [DispId(0x1f7)]
            void ShowBrowserBar([In] ref object pvaClsid, [In] ref object pvarShow, [In] ref object pvarSize);
            [DispId(-525)]
            WebBrowserReadyState ReadyState { get; }
            [DispId(550)]
            bool Offline { get; set; }
            [DispId(0x227)]
            bool Silent { get; set; }
            [DispId(0x228)]
            bool RegisterAsBrowser { get; set; }
            [DispId(0x229)]
            bool RegisterAsDropTarget { get; set; }
            [DispId(0x22a)]
            bool TheaterMode { get; set; }
            [DispId(0x22b)]
            bool AddressBar { get; set; }
            [DispId(0x22c)]
            bool Resizable { get; set; }
        }        
    }
}