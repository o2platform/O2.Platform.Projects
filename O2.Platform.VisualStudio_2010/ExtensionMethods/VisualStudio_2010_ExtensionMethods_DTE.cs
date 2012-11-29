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
	public static class VisualStudio_2010_ExtensionMethods_DTE
	{
		public static DTE2 dte(this VisualStudio_2010 visualStudio)
		{
			return VisualStudio_2010.DTE2;
		}

		public static class VisualStudio_2010_ExtensionMethods_DTE_StatusBar
		{
			public static string statusBar(this VisualStudio_2010 visualStudio)
			{
				return visualStudio.dte().StatusBar.Text;
			}
			public static VisualStudio_2010 statusBar(this VisualStudio_2010 visualStudio, string text)
			{
				visualStudio.dte().StatusBar.Text = text;
				return visualStudio;
			}
		}

		public static class VisualStudio_2010_ExtensionMethods_DTE_OutputWindow
		{
			public static EnvDTE.OutputWindowPane outputWindow(this VisualStudio_2010 visualStudio)
			{
				return visualStudio.dte().ToolWindows.OutputWindow.ActivePane;
			}
			public static EnvDTE.OutputWindowPane outputWindow(this VisualStudio_2010 visualStudio, string name)
			{
				try
				{
					return visualStudio.dte().ToolWindows.OutputWindow.OutputWindowPanes.Item(name);
				}
				catch
				{
					"could not find output Window with name: {0}".error(name);
					return null;
				}
			}
			public static EnvDTE.OutputWindowPane outputWindow_Create(this DTE2 dte, string name, bool logError)
			{
				try
				{
					return dte.ToolWindows.OutputWindow.OutputWindowPanes.Add(name);
				}
				catch (Exception ex)
				{
					if (logError)
						ex.log("[in create_OutputWindow]");
					return null;
				}

			}
			public static EnvDTE.OutputWindowPane outputWindow_Create(this VisualStudio_2010 visualStudio, string name)
			{
				var outputWindow = visualStudio.outputWindow(name);
				if (outputWindow.notNull())
				{
					"[create_OutputWindow] there was already an output window called '{0}' so returning the existing one".debug(name);
					return outputWindow;
				}
				return visualStudio.dte().outputWindow_Create(name, true);
			}
			public static EnvDTE.OutputWindowPane writeLine(this EnvDTE.OutputWindowPane outputWindow, string text)
			{
				outputWindow.OutputString(text.line());
				return outputWindow;
			}
		}

		public static class VisualStudio_2010_ExtensionMethods_DTE_CommandWindow
		{
			public static EnvDTE.CommandWindow commandWindow(this VisualStudio_2010 visualStudio)
			{
				return visualStudio.dte().ToolWindows.CommandWindow;
			}
			public static EnvDTE.CommandWindow writeLine(this EnvDTE.CommandWindow commandWindow, string text)
			{
				commandWindow.OutputString(text.line());
				return commandWindow;
			}
			public static EnvDTE.CommandWindow sendInput_and_Execute(this EnvDTE.CommandWindow commandWindow, string input)
			{
				commandWindow.SendInput(input, true);
				return commandWindow;
			}
			public static EnvDTE.CommandWindow execute(this EnvDTE.CommandWindow commandWindow, string input)
			{
				commandWindow.sendInput_and_Execute(input);
				return commandWindow;
			}
		}
	}
}