using mshtml;
using O2.External.IE.Interfaces;
using O2.DotNetWrappers.ExtensionMethods;

namespace O2.External.IE.WebObjects
{
    public class IE_Anchor : IO2HtmlAnchor
    {
        public string OuterHtml { get; set; }

         public IE_Anchor(object _object)
        {
        	if (_object is DispHTMLAnchorElement)
				loadData((DispHTMLAnchorElement)_object);
			else
				"In IE_Anchor, not supported type: {0}".format(_object.comTypeName()).error();
		}
		
        public IE_Anchor(DispHTMLAnchorElement anchor)
        {
            loadData(anchor);
        }
        
        public void loadData(DispHTMLAnchorElement anchor)       
        {
            OuterHtml = anchor.outerHTML;
        }
    }
}