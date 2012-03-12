using System;
using System.Collections.Generic;

namespace O2.External.IE.Interfaces
{
    public interface IO2HtmlPage
    {
        Uri PageUri { get; set; }
        string PageSource { get; set; }
        List<IO2HtmlAnchor> Anchors { get; set; }     
        List<IO2HtmlForm> Forms { get; set; }
        List<IO2HtmlImg> Images { get; set; }
        List<IO2HtmlLink> Links { get; set; }
        List<IO2HtmlScript> Scripts { get; set; }

    }
}