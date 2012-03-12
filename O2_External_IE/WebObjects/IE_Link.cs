// This file is part of the OWASP O2 Platform (http://www.owasp.org/index.php/OWASP_O2_Platform) and is released under the Apache 2.0 License (http://www.apache.org/licenses/LICENSE-2.0)
using mshtml;
using O2.External.IE.Interfaces;
using O2.Kernel.ExtensionMethods;

namespace O2.External.IE.WebObjects
{
    public class IE_Link : IO2HtmlLink
    {
        public string Href { get; set; }
        public string InnerText { get; set; }
        public string InnerHtml { get; set; }        
        public string OuterHtml { get; set; }
        public string Target { get; set; }

        //public HtmlLinkIE(HTMLLinkElementClass linkElement)
        //public IE_Link(HTMLAnchorElementClass linkElement)
        
        public IE_Link(object _object)
        {
        	if (_object is DispHTMLAnchorElement)
				loadData((DispHTMLAnchorElement)_object);
			else
				"In IE_Link, not supported type: {0}".format(_object.comTypeName()).error();
		}
		
        public IE_Link(DispHTMLAnchorElement linkElement)         
        {
        	loadData(linkElement);
        }
        
        public void loadData(DispHTMLAnchorElement linkElement)       
        {
            try
            {
                Href = linkElement.href;
                OuterHtml = linkElement.outerHTML;
                InnerHtml = linkElement.innerHTML;
                InnerText = linkElement.innerText;
                if (false == linkElement is DispHTMLAreaElement)
                    Target = linkElement.target;
            }
            catch (System.Exception ex)
            {
                "in IE_Link.loadData: {0}".format(ex.Message).error();
            }
        }    
    }
}