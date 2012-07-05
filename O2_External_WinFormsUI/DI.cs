// This file is part of the OWASP O2 Platform (http://www.owasp.org/index.php/OWASP_O2_Platform) and is released under the Apache 2.0 License (http://www.apache.org/licenses/LICENSE-2.0)
using System;
using System.Collections.Generic;
using O2.External.WinFormsUI.Forms;
using O2.External.WinFormsUI.O2Environment;
using O2.Interfaces.Messages;
using O2.Interfaces.O2Core;
using O2.Kernel;
using O2.Kernel.InterfacesBaseImpl;

namespace O2.External.WinFormsUI
{
    internal class DI
    {
        static DI()
        {
            log = PublicDI.log; // _note that when the O2GuiWithDockPanel is create it will overide the PublicDI log with WinFormsUILog() object            
            reflection = PublicDI.reflection;
            config = PublicDI.config;
            o2MessageQueue = KO2MessageQueue.getO2KernelQueue();
            
            new O2MessagesHandler(); // set up O2Message hook

            autoAddLogViewerToGui = true;
        }
        
        // DI objects
        public static KO2Config config { get; set;}        
        public static IO2MessageQueue o2MessageQueue { get; set; }
        public static IO2Log log;
        public static IReflection reflection;

        // local global vars
        public static Dictionary<String, O2DockContent> dO2LoadedO2DockContent  = new Dictionary<String, O2DockContent>();
        public static O2GuiWithDockPanel o2GuiWithDockPanel;
        public static bool o2GuiStandAloneFormMode;
        
        public static bool autoAddLogViewerToGui { get; set; }
     
    }
}
