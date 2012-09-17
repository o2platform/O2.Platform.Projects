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
using O2.FluentSharp;

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
            
        }


         
        /////////////////////////////////////////////////////////////////////////////
        // Overriden Package Implementation
        #region Package Members
      
        protected override void Initialize()
        {            
            //VisualStudio_O2_Utils.open_LogViewer();
            //VisualStudio_O2_Utils.open_ScriptEditor();
            //new NoSolution_Package().Initialize();          // 
            VisualStudio_O2_Utils.compileAndExecuteScript(@"VS_Scripts\O2_Platform_Gui.cs", "O2_Platform_Gui", "buildGui"); 
            
            base.Initialize();

        }
        #endregion

    }
}
