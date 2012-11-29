using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.PlatformUI; 
using Microsoft.VisualStudio.Platform.WindowManagement; 
using Microsoft.VisualStudio.Platform.WindowManagement.DTE;
using System.Windows.Forms.Integration;
using WinForms = System.Windows.Forms;
using EnvDTE;
using EnvDTE80;
using O2.DotNetWrappers.DotNet;
using O2.DotNetWrappers.ExtensionMethods;
using O2.FluentSharp.VisualStudio;


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
