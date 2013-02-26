using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using O2.Kernel;
using O2.DotNetWrappers.DotNet;
using O2.DotNetWrappers.ExtensionMethods;
using O2.Kernel.InterfacesBaseImpl;

namespace O2.FluentSharp.VisualStudio
{	
	public class O2Platform_VisualStudio
	{
		public void loadO2PlatformVSEnvironment()
		{
			try
			{				
				VisualStudio_O2_Utils.waitForDTEObject();
				VisualStudio_O2_Utils.waitForOutputWindow();
				
				//VisualStudio_2010.O2_OutputWindow.
				"[O2_Platform_VisualStudio_2010Package] Package: {0}, DTE: {1}".info(VisualStudio_2010.Package, VisualStudio_2010.DTE2);

				CompileEngine.LocalFoldersToSearchForCodeFiles.Add(this.type().assemblyLocation().parentFolder());		// so that "{file}".local() is able to find files included with this
				//CompileEngine.clearCompilationCache();
				VisualStudio_O2_Utils.compileAndExecuteScript(@"VS_Scripts\O2_Platform_Gui.cs", "O2_Platform_Gui", "buildGui");
			}
			catch (Exception ex)
			{
				ex.log("in O2_Platform_VisualStudio_2010Package Initialize");
			}
		}
	}
}
