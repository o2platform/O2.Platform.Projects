using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.IO;
using System.Reflection;
using Microsoft.Win32;
using System.Windows.Forms;

namespace O2.Platform
{
	public class O2_Setup
	{
		public string O2_Execution_Folder	{ get; set; }
		public string Dll_Download_Location { get; set; }
		

		public O2_Setup()
		{
			loadValuesFromConfigFile();
		}

		public void loadValuesFromConfigFile()
		{
			var virtualPath = ConfigurationSettings.AppSettings["Local_O2_Dlls_Folder"];
			this.O2_Execution_Folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,virtualPath);
			if (Directory.Exists(this.O2_Execution_Folder) == false)
				Directory.CreateDirectory(this.O2_Execution_Folder);

			this.Dll_Download_Location = ConfigurationSettings.AppSettings["GitHub_O2_Dlls"];
		}

		public Assembly load_O2_Assembly(string assemblyName)
		{
			var fullPath = Path.Combine(this.O2_Execution_Folder, assemblyName);

			return Assembly.LoadFrom(fullPath);			

		}
		
		public void loadDependencies()
		{
			load_O2_Assembly("O2_FluentSharp_CoreLib.dll");
			load_O2_Assembly("O2_FluentSharp_BCL.dll");
		}

		public void complileO2StartupScriptAndExecuteIt(string[] args)
		{
			/*var o2Bcl = load_O2_Assembly("O2_FluentSharp_BCL.dll");
			var startO2_Type = o2Bcl.GetType("O2.Platform.BCL.Start_O2");

			var startO2 = Activator.CreateInstance(startO2_Type);

			var compileScript = startO2.GetType().GetMethod("compileScript");
			var assembly = (Assembly)compileScript.Invoke(startO2, new object[] { "ascx_Execute_Scripts.cs" });
			 */
			
			
			
			var h2Script = O2.DotNetWrappers.ExtensionMethods.CompileEngine_ExtensionMethods.local(
				//"Util - SourceCodeViewer.h2");// fails
				"Simple O2 Gui.h2"); //fails
				//"Util - O2 Available scripts.h2");
				//"Util - LogViewer.h2"); // loads ok
				//("Dinis Cruz (Custom O2 version).h2");
			var assembly2 =O2.External.SharpDevelop.ExtensionMethods.FastCompiler_ExtensionMethods.compile_H2Script(h2Script);
			
			if (assembly2 == null)
				System.Windows.Forms.MessageBox.Show("NUll");
			else
			{
				//System.Windows.Forms.MessageBox.Show("Good");			
			 	O2.External.SharpDevelop.ExtensionMethods.FastCompiler_ExtensionMethods.executeFirstMethod(assembly2);
			}
			return;
			//var assembly = new O2.Platform.BCL.Start_O2().compileScript("Dinis Cruz (Custom O2 version).h2");
			
			
			var assembly = new O2.Platform.BCL.Start_O2().compileScript("ascx_Execute_Scripts.cs" );
			
			if (assembly == null)
			{
				MessageBox.Show("There was a problem compiling the ascx_Execute_Scripts.cs script file","O2 Start error");
				return;
			}

//			var types = assembly.GetTypes();
			var ascx_Execute_Scripts = assembly.GetType("O2.XRules.Database.ascx_Execute_Scripts");

			var startControl_No_Args = ascx_Execute_Scripts.GetMethod("startControl_With_Args");
						
			startControl_No_Args.Invoke(null, new object[] { args});
		}

		public void startO2(string[] args)
		{			
			loadDependencies();
			
			complileO2StartupScriptAndExecuteIt(args);
			
		}


		
	}
}
 