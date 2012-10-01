using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Linq;
using System.Text;
using WeifenLuo.WinFormsUI.Docking;
using O2.Kernel;
using O2.DotNetWrappers.ExtensionMethods;
using O2.External.SharpDevelop.ExtensionMethods;
using O2.FluentSharp.REPL;
using O2.FluentSharp.VisualStudio;


namespace O2.Platform.VisualStudio_2010_Extension
{    
    public class O2_Platform_Gui
    {
        public static VisualStudio_2010     visualStudio;

        public O2_Platform_Gui()
        {
            visualStudio = new VisualStudio_2010();
        }

        public O2_Platform_Gui buildGui()
        {            
            createTopLevelMenu();            
			return this;
        }

        public O2_Platform_Gui installO2Scripts()
        {
            VisualStudio_O2_Utils.installO2Scripts_IfDoesntExist();
            return this;
        }
        public O2_Platform_Gui createTopLevelMenu()
        {
            @"VS_O2_Plugins\Create O2 Platform Menu.h2".local().executeH2Script();            
            return this;
        }        

        public DockPanel openScriptsViewer()
        {                        
            var scriptsFolder = @"VS_O2_PlugIns\Create O2 Platform Menu.h2".local().parentFolder();
            return scriptsFolder.open_Script_Viewer_GUI();            
        }                    
    }
    
}
