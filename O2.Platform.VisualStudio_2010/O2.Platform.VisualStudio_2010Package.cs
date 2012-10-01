using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using Microsoft.Win32;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using O2.FluentSharp.VisualStudio;
using O2.Kernel;
using O2.DotNetWrappers.DotNet;
using O2.DotNetWrappers.ExtensionMethods;
using System.Windows.Forms;

namespace O2.Platform.VisualStudio_2010_Extension
{
    static class GuidList
    {
        public const string guidO2_Platform_VisualStudio_2010PkgString = "F886416F-3DBF-4DEE-9578-E7692FC59871";
        //     public const string guidO2_Platform_VisualStudio_2010CmdSetString = "dcf44788-1870-4627-9dbb-910bee34c55c";

        //     public static readonly Guid guidO2_Platform_VisualStudio_2010CmdSet = new Guid(guidO2_Platform_VisualStudio_2010CmdSetString);
    }
    [PackageRegistration(UseManagedResourcesOnly = true)]    
    [Guid(GuidList.guidO2_Platform_VisualStudio_2010PkgString)]
    [ProvideAutoLoad(UIContextGuids80.NoSolution)]               // ensures this gets called on VisualStudio start
    public sealed class O2_Platform_VisualStudio_2010Package : NoSolution_Package
    {
     
        public O2_Platform_VisualStudio_2010Package()
        {
			O2ConfigSettings.O2Version = "O2_VS2010_4.4.5";
			PublicDI.config = new O2.Kernel.InterfacesBaseImpl.KO2Config();
        }


         
        /////////////////////////////////////////////////////////////////////////////
        // Overriden Package Implementation
        #region Package Members
      
        protected override void Initialize()
        {
			base.Initialize();
			if (Control.ModifierKeys == Keys.Shift)
				open.scriptEditor();
			try
			{
				VisualStudio_O2_Utils.waitForDTEObject();
				"[O2_Platform_VisualStudio_2010Package] Package: {0}, DTE: {1}".info(VisualStudio_2010.Package, VisualStudio_2010.DTE2);
				
				CompileEngine.LocalFoldersToSearchForCodeFiles.Add(this.type().assemblyLocation().parentFolder());		// so that "{file}".local() is able to find files included with this

				VisualStudio_O2_Utils.compileAndExecuteScript(@"VS_Scripts\O2_Platform_Gui.cs", "O2_Platform_Gui", "buildGui");
			}
			catch (Exception ex)
			{
				ex.log("in O2_Platform_VisualStudio_2010Package Initialize");
			}
            
            

        }
        #endregion

    }
}
