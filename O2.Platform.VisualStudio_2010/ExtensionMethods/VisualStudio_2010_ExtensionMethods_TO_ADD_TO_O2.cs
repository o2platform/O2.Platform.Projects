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
	//Extra WPF Extension Methods
	public static class WPF_ExtensionMethods_Window
	{
		public static string title<T>(this T window) where T : System.Windows.Window
		{
			return window.wpfInvoke(() => window.Title);
		}
		public static T title<T>(this T window, string title) where T : System.Windows.Window
		{
			return window.wpfInvoke(() => { window.Title = title; return window; });
		}
	}
}