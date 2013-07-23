// This file is part of the OWASP O2 Platform (http://www.owasp.org/index.php/OWASP_O2_Platform) and is released under the Apache 2.0 License (http://www.apache.org/licenses/LICENSE-2.0)
using System;
using FluentSharp.REPL.Utils;

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
			//new O2_Setup().startO2(args);
            O2Launch.o2Gui(args);
            //launch_O2Gui_Via_Emebeded_Assembly(args);
        }
    }
}