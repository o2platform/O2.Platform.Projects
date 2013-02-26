using Microsoft.VisualStudio.Shell;
using WinForms = System.Windows.Forms;


namespace O2.FluentSharp.VisualStudio.ExtensionMethods
{
    public static class VisualStudio_2010_ExtensionMethods_Packages
    {
    	public static Package package(this VisualStudio_2010 visualStudio)
    	{
    		return VisualStudio_2010.Package;
    	}
    	public static T getService<T>(this VisualStudio_2010 visualStudio)
		{
			return VisualStudio_2010.Package.getService<T>();
		}
    }
 
}
