// This file is part of the OWASP O2 Platform (http://www.owasp.org/index.php/OWASP_O2_Platform) and is released under the Apache 2.0 License (http://www.apache.org/licenses/LICENSE-2.0)
using System;
using System.Reflection;
using System.IO;
using O2.Kernel;

namespace O2.Platform
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>

        [STAThread]
        static void Main(string[] args)
        {            
			//new O2_Setup().startO2(args);
            launch.o2Gui(args);
            //launch_O2Gui_Via_Emebeded_Assembly(args);
        }

        /*public static void launch_O2Gui_Via_Emebeded_Assembly(string[] args)
        {
            var resourceName = "O2.Platform._Dlls_Embeded.O2_FluentSharp_CoreLib.dll";
            var assemblyStream = Assembly.GetEntryAssembly().GetManifestResourceStream(resourceName);
            byte[] data = new BinaryReader(assemblyStream).ReadBytes((int)assemblyStream.Length);
            Assembly assembly = Assembly.Load(data);
            var type = assembly.GetType("O2.Kernel.launch");
            var method = type.GetMethod("o2Gui");
            method.Invoke(null, new object[] { args });
        } */    


    }
}