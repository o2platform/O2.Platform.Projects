using System.Windows.Forms;
using mshtml;

namespace O2.External.IE.ExtensionMethods
{
    public class IEChangeSink :IHTMLChangeSink
    {
        public MethodInvoker onChange;

        public IEChangeSink(MethodInvoker _onChange)
    	{
    		onChange = _onChange;
    	}
    	
        public void Notify()
        {
        	onChange();        	            
            //PublicDI.log.debug("in changeSink.Notify()");
        }
    }
}
