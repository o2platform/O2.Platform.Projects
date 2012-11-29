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
	public static class VisualStudio_2010_ExtensionMethods_ToolWindowsPane
	{
		public static int lastWindowId = 0;

		public static Grid				create_WPF_Window(this string title)
		{
			ToolWindowPane toolWindow = null;
			return title.create_WPF_Window(ref toolWindow);
		}
		public static Grid				create_WPF_Window(this string title, ref ToolWindowPane toolWindow)
		{
			var visualStudio = new VisualStudio_2010();
			ToolWindowPane window = null;
			var grid = visualStudio.invokeOnThread(
			() =>
			{
				var type = typeof(O2.FluentSharp.VisualStudio.WindowPane_WPF);
				window = (ToolWindowPane)visualStudio.package().invoke("CreateToolWindow", type, ++lastWindowId);
				window.Caption = title;
				IVsWindowFrame windowFrame = (IVsWindowFrame)window.Frame;
				windowFrame.Show();
				var content = (Control_WPF)window.Content;

				return (Grid)content.Content;
			});
			toolWindow = window;
			return grid;
		}
		public static WinForms.Panel	create_WinForms_Window(this string title)
		{
			return title.create_WinForms_Window(VSFRAMEMODE.VSFM_Dock);
		}
		public static WinForms.Panel	create_WinForms_Window(this string title, VSFRAMEMODE frameMode)
		{
			var visualStudio = new VisualStudio_2010();
			var _panel = visualStudio.invokeOnThread(
			() =>
			{
				var type = typeof(O2.FluentSharp.VisualStudio.WindowPane_WinForms);
				var window = (ToolWindowPane)visualStudio.package().invoke("CreateToolWindow", type, 64000.random());

				window.Caption = title;
				IVsWindowFrame windowFrame = (IVsWindowFrame)window.Frame;
				//if(floating)
				//    windowFrame.SetProperty((int)__VSFPROPID.VSFPROPID_FrameMode, VSFRAMEMODE.VSFM_Float);
				windowFrame.SetProperty((int)__VSFPROPID.VSFPROPID_FrameMode, frameMode);
				windowFrame.Show();
				var content = (Control_WinForms)window.Content;
				var windowsFormHost = (System.Windows.Forms.Integration.WindowsFormsHost)content.Content;
				var panel = new WinForms.Panel();
				panel.backColor("Control");
				windowsFormHost.Child = panel;
				return panel;
			});
			return _panel;
		}
		public static WinForms.Panel	create_WinForms_Window_Float(this string title, int width, int height)
		{
			var panel = title.create_WinForms_Window_Float();
			panel.dte_Window().width(width).height(height);
			return panel;
		}
		public static WinForms.Panel	create_WinForms_Window_Float(this string title)
		{
			return title.create_WinForms_Window(VSFRAMEMODE.VSFM_Float);
		}
		public static WinForms.Panel	create_WinForms_Window_MdiChild(this string title)
		{
			return title.create_WinForms_Window(VSFRAMEMODE.VSFM_MdiChild);
		}
		public static WinForms.Panel	create_WinForms_Window(this VisualStudio_2010 visualStudio, string title)
		{
			return title.create_WinForms_Window();
		}
		public static WinForms.Panel	create_WinForms_Window_Float(this VisualStudio_2010 visualStudio, string title)
		{
			return title.create_WinForms_Window_Float();
		}
		public static string			caption<T>(this T toolWindowPane) where T : ToolWindowPane
		{
			return new VisualStudio_2010().invokeOnThread(() => toolWindowPane.Caption);
		}
		public static T					caption<T>(this T toolWindowPane, string title) where T : ToolWindowPane
		{
			return new VisualStudio_2010().invokeOnThread(() => { toolWindowPane.Caption = title; return toolWindowPane; });
		}
		public static ToolWindowPane	as_MdiChild(this ToolWindowPane toolWindow)
		{
			if (toolWindow.notNull())
			{
				IVsWindowFrame windowFrame = (IVsWindowFrame)toolWindow.Frame;
				windowFrame.SetProperty((int)__VSFPROPID.VSFPROPID_FrameMode, VSFRAMEMODE.VSFM_MdiChild);
			}
			return toolWindow;
		}
		public static ToolWindowPane	as_Float(this ToolWindowPane toolWindow)
		{
			if (toolWindow.notNull())
			{
				IVsWindowFrame windowFrame = (IVsWindowFrame)toolWindow.Frame;
				windowFrame.SetProperty((int)__VSFPROPID.VSFPROPID_FrameMode, VSFRAMEMODE.VSFM_Float);
			}
			return toolWindow;
		}
		public static ToolWindowPane	as_Dock(this ToolWindowPane toolWindow)
		{
			if (toolWindow.notNull())
			{
				IVsWindowFrame windowFrame = (IVsWindowFrame)toolWindow.Frame;
				windowFrame.SetProperty((int)__VSFPROPID.VSFPROPID_FrameMode, VSFRAMEMODE.VSFM_Dock);
			}
			return toolWindow;
		}
	}
	public static class VisualStudio_2010_ExtensionMethods_WindowBase
	{
		public static WindowBase		windowBase	(this WinForms.Control  control						 )
		{
			try
			{
				return (WindowBase)control.dte_Window();
			}
			catch (Exception ex)
			{
				ex.log("[in control.windowBase]");
				return null;
			}
		}
		public static List<WindowBase>	windows		(this VisualStudio_2010 visualStudio				 )
		{
			var windows = new List<WindowBase>();
			foreach (WindowBase window in visualStudio.dte().Windows)
				windows.Add(window);
			return windows;
		}
		public static WindowBase		window		(this VisualStudio_2010 visualStudio, string caption )
		{
			return visualStudio.windows().Where((window) => window.Caption == caption).first();
		}
		public static WindowBase		get_Window	(this VisualStudio_2010 visualStudio, string caption )
		{
			return visualStudio.window(caption);
		}
		public static List<string>		names		(this List<WindowBase>  windows						 )
		{
			return windows.captions();
		}
		public static List<string>		titles		(this List<WindowBase>  windows						 )
		{
			return windows.captions();
		}
		public static List<string>		captions	(this List<WindowBase>  windows						 )
		{
			return windows.Select((window) => window.Caption).toList();
		}
		public static WindowBase		title		(this WindowBase window	, string value				 )
		{
			try
			{
				window.Caption = value;
			}
			catch (Exception ex)
			{
				ex.log("[window.title]");
			}
			return window;
		}
		public static WindowBase		floating	(this WindowBase window, bool value					 )
		{
			try
			{
				window.IsFloating = value;
			}
			catch (Exception ex)
			{
				ex.log("[window.floating]");
			}
			return window;
		}
		public static WindowBase		linkable	(this WindowBase window, bool value					 )
		{
			try
			{
				window.Linkable = value;
			}
			catch (Exception ex)
			{
				ex.log("[window.linkable]");
			}
			return window;
		}
		public static WindowBase		autoHide	(this WindowBase window, bool value					 )
		{
			try
			{
				window.AutoHides = value;
			}
			catch (Exception ex)
			{
				ex.log("[window.autoHide]");
			}
			return window;
		}
		public static WindowBase		visible		(this WindowBase window, bool value					 )
		{
			try
			{
				window.Visible = value;
			}
			catch (Exception ex)
			{
				ex.log("[window.visible]");
			}
			return window;
		}		
		public static WindowBase		left		(this WindowBase window, int value					 )
		{
			try
			{
				window.Left = value;
			}
			catch (Exception ex)
			{
				ex.log("[window.left]");
			}
			return window;
		}
		public static WindowBase		top			(this WindowBase window, int value					 )
		{
			try
			{
				window.Top = value;
			}
			catch (Exception ex)
			{
				ex.log("[window.top]");
			}
			return window;
		}
		public static WindowBase		width		(this WindowBase window, int value					 )
		{
			try
			{
				window.Width = value;
			}
			catch (Exception ex)
			{
				ex.log("[window.width]");
			}
			return window;
		}
		public static WindowBase		height		(this WindowBase window, int value					 )
		{
			try
			{
				window.Height = value;
			}
			catch (Exception ex)
			{
				ex.log("[window.height]");
			}
			return window;
		}
		public static WindowBase		focus		(this WindowBase window								 )
		{
			return window.show();
		}
		public static WindowBase		show		(this WindowBase window								 )
		{
			try
			{
				window.visible(true);
				window.Activate();
			}
			catch (Exception ex)
			{
				ex.log("[window.show]");
			}
			return window;
		}
		public static WindowBase		hide		(this WindowBase window								 )
		{
			window.visible(false);
			return window;
		}
	}

	public static class VisualStudio_2010_ExtensionMethods_DTE_Window
	{
		public static EnvDTE.Window dte_Window(this System.Windows.Forms.Control control)
		{
			return control.toolWindowPane().dte_Window();
		}
		public static EnvDTE.Window dte_Window(this ToolWindowPane toolWindowPane)
		{
			return new VisualStudio_2010().invokeOnThread(
				() =>
				{
					IVsWindowFrame windowFrame = (IVsWindowFrame)toolWindowPane.Frame;
					return VsShellUtilities.GetWindowObject(windowFrame);
				});
		}
		public static string title(this EnvDTE.Window window)
		{
			return new VisualStudio_2010().invokeOnThread(
				() => window.Caption);
		}
		public static EnvDTE.Window title(this EnvDTE.Window window, string value)
		{
			return new VisualStudio_2010().invokeOnThread(() =>
			{
				window.Caption = value;
				return window;
			});
		}
		public static EnvDTE.Window width(this EnvDTE.Window window, int value)
		{
			return new VisualStudio_2010().invokeOnThread(() =>
			{
				window.Width = value;
				return window;
			});
		}
		public static EnvDTE.Window height(this EnvDTE.Window window, int value)
		{
			return new VisualStudio_2010().invokeOnThread(() =>
			{
				window.Height = value;
				return window;
			});
		}
		public static bool close(this EnvDTE.Window window)
		{
			try
			{
				window.Close(); //will throw exeption if window has been closed
				return true;
			}
			catch (Exception ex)
			{
				ex.log("[in EnvDTE.window.close]");
				return false;
			}
		}
		public static bool visible(this EnvDTE.Window window)
		{
			try
			{
				return window.Visible; //will throw exeption if window has been closed
			}
			catch (Exception ex)
			{
				ex.log("[in EnvDTE.window.visible]");
				return false;
			}
		}
		public static EnvDTE.Window visible(this EnvDTE.Window window, bool value)
		{
			try
			{
				window.Visible = value; //will throw exeption if window has been closed                
				return window;
			}
			catch (Exception ex)
			{
				ex.log("[in EnvDTE.window.visible]");
				return null;
			}
		}
		public static T close_in_NSeconds<T>(this T window, int seconds) where T : EnvDTE.Window
		{
			O2Thread.mtaThread(() => window.wait(seconds * 1000).close());
			return window;
		}
	}
}