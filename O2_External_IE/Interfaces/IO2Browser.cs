using System;
using System.Collections.Generic;

namespace O2.External.IE.Interfaces
{        

    public interface IO2Browser
    {
        event Action<IO2HtmlPage> onDocumentCompleted;
        bool HtmlEditMode { get; set; }
        void open(string url);
        void open(Uri uri);
        IO2HtmlPage openSync(string url);

        void submitRequest_GET(string url);
        void submitRequest_GET(string url, string targetFrame, Dictionary<string, string> parameters);
        void submitRequest_GET(string url, string targetFrame, string parametersString);

        IO2HtmlPage submitRequest_GET_Sync(string url, string targetFrame, string parametersString);
        IO2HtmlPage submitRequest_GET_Sync(string url, string targetFrame, Dictionary<string, string> parameters);

        void submitRequest_POST(string url, string targetFrame, Dictionary<string, string> parameters);
        void submitRequest_POST(string url, string targetFrame, string postString);
        void submitRequest_POST(string url, string targetFrame, byte[] postData);
                
        IO2HtmlPage submitRequest_POST_Sync(string url, string targetFrame, Dictionary<string, string> parameters);
        IO2HtmlPage submitRequest_POST_Sync(string url, string targetFrame, string postString);
        IO2HtmlPage submitRequest_POST_Sync(string url, string targetFrame, byte[] postData);        
    }
}
