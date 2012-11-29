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
	public static class VisualStudio_2010_ExtensionMethods_WPF_Application
	{
		public static T invokeOnThread<T>(this VisualStudio_2010 visualStudio, Func<T> func)
		{
			return System.Windows.Application.Current.wpfInvoke(() => func());
		}
		public static VisualStudio_2010 invokeOnThread(this VisualStudio_2010 visualStudio, Action action)
		{
			return (VisualStudio_2010)System.Windows.Application.Current.wpfInvoke(() =>
			{
				action();
				return visualStudio;
			});
		}
		public static Application application(this VisualStudio_2010 visualStudio)
		{
			return visualStudio.invokeOnThread(() => System.Windows.Application.Current);
		}
		public static MainWindow mainWindow(this VisualStudio_2010 visualStudio)
		{
			return (MainWindow)visualStudio.invokeOnThread(() => visualStudio.application().MainWindow);
		}
	}

	public static class VisualStudio_2010_ExtensionMethods_WinFormsIntegration
	{
		public static WindowsFormsHost windowsFormHost(this System.Windows.Forms.Control control)
		{
			try
			{
				var containerControl = control.parent<System.Windows.Forms.ContainerControl>();
				if (containerControl.isNull())
					return null;
				if (containerControl.typeName() == "WinFormsAdapter")
					return (WindowsFormsHost)containerControl.field("_host");
				return containerControl.windowsFormHost();
			}
			catch (Exception ex)
			{
				ex.log("[in windowsFormHost(this Control control]");
			}
			return null;
		}
		public static UserControl userControl(this WindowsFormsHost windowsFormsHost)
		{
			return windowsFormsHost.userControl<UserControl>();
		}
		public static T userControl<T>(this WindowsFormsHost windowsFormsHost) where T : UserControl
		{
			try
			{
				return (T)windowsFormsHost.Parent;
			}
			catch (Exception ex)
			{
				ex.log();
				return null;
			}

		}
		public static ToolWindowPane toolWindowPane(this UserControl userControl)
		{
			if (VisualStudio_2010.ToolWindowPanes.hasKey(userControl))
				return VisualStudio_2010.ToolWindowPanes[userControl];
			return null;
		}
		public static ToolWindowPane toolWindowPane(this System.Windows.Forms.Control control)
		{
			return control.windowsFormHost()
						  .userControl()
						  .toolWindowPane();
		}
	}

}