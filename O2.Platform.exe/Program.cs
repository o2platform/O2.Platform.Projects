// This file is part of the OWASP O2 Platform (http://www.owasp.org/index.php/OWASP_O2_Platform) and is released under the Apache 2.0 License (http://www.apache.org/licenses/LICENSE-2.0)
using System;
using FluentSharp.O2Platform.Utils;

namespace O2.Platform
{
    public class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>

        [STAThread]
        public static void Main(string[] args)
        {                                    			
            //O2Launch.o2Gui(args);            
            //args = new [] {@"Util - LogViewer.h2"};
            new O2_Start().OpenStartGui(args);
        }
    }
}