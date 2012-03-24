using System;
using Extensibility;
using EnvDTE;
using EnvDTE80;
using System.Windows.Forms;
using System.Reflection;
using System.Diagnostics;
using System.IO;

namespace O2.VisualStudio
{
    public class Connect_Helpers
    {
        static Connect_Helpers()
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        }

        public static string path_to_O2Assemblies = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        public static void showMessage(string message)
        { 
             MessageBox.Show(message, "O2 VisualStudio Addin");
        }

        static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            if(args.Name.Contains("O2"))
            {
                string simpleName = new AssemblyName (args.Name).Name;
                return loadO2Assembly(simpleName,false);
            }
            return null;
        }

        
        public static Assembly loadO2Assembly(string assemblyName, bool loadAsBinaryStream)
        {
            try
            {
                if(Path.GetExtension(assemblyName) == "")
                    assemblyName += ".dll";
                var assemblyPath = Path.Combine(path_to_O2Assemblies,assemblyName);
                if (File.Exists(assemblyPath) == false)
                {
                    if (assemblyName.Contains("XmlSerializers") == false && assemblyName.Contains(".resources") == false)
                        showMessage("[O2.VisualStudio.Connect_Helpers] Error: Could not find file: " + assemblyPath);
                }
                else
                {
                    if (loadAsBinaryStream)
                    {
                        using (var fileStream = new FileStream(assemblyPath, FileMode.Open))
                        {

                            byte[] data = new BinaryReader(fileStream).ReadBytes((int)fileStream.Length);
                            var assembly = Assembly.Load(data);
                            if (assembly == null)
                                showMessage("[O2.VisualStudio.Connect_Helpers] Error: Assembly failed to load: " + assemblyPath);
                            else
                                return assembly;
                        }
                    }
                    else
                        return Assembly.LoadFrom(assemblyPath);

                }
            }
            catch (Exception ex)
            { 
                if (ex.Message.Contains("is being used by another process") == false) 
                    showMessage("[O2.VisualStudio.Connect_Helpers] Error: " + ex.Message);                
            }
            return null;
        }

    }

	public class Connect : IDTExtensibility2, IDTCommandTarget
	{
        public bool AskQuestion = true;
        public Type type;
        public Object connect;
        public MethodInfo onConnection;
        public MethodInfo onDisconnection;
        public MethodInfo queryStatus;
        public MethodInfo exec;

        public Connect()
        { 
            try
            {
                var fluentSharp_CoreLib      = Connect_Helpers.loadO2Assembly("O2_FluentSharp_CoreLib.dll"  , false);
                var fluentSharp_Bcl          = Connect_Helpers.loadO2Assembly("O2_FluentSharp_BCL.dll"      , false);
                //var fluentSharp_VisualStudio = Connect_Helpers.loadO2Assembly(@"..\..\O2.FluentSharp\binaries\O2_FluentSharp_VisualStudio.dll", true);   
                //var fluentSharp_VisualStudio = Connect_Helpers.loadO2Assembly(@"O2_FluentSharp_VisualStudio.dll", false);   


                //Compile VisualStudio_Connect.cs
			
			    var startO2_Type = fluentSharp_Bcl.GetType("O2.Platform.BCL.Start_O2");
			    var startO2 = Activator.CreateInstance(startO2_Type);
			    var compileScript = startO2.GetType().GetMethod("compileScript");

                var startScript = "VisualStudio_Connect.cs";
			    var VisualStudio_Connect = (Assembly)compileScript.Invoke(startO2, new object[] { startScript });

                if (VisualStudio_Connect == null)
                { 
                    Connect_Helpers.showMessage("[O2.VisualStudio.Connect] failed to compile script: " + startScript);
                }
                type            = VisualStudio_Connect.GetType("O2.FluentSharp.VisualStudio.Connect");
                connect         = Activator.CreateInstance(type);
                onConnection    = connect.GetType().GetMethod("OnConnection");
                onDisconnection = connect.GetType().GetMethod("OnDisconnection");
                queryStatus     = connect.GetType().GetMethod("QueryStatus");
                exec            = connect.GetType().GetMethod("Exec");
            }
            catch (Exception ex)
            { 
                Connect_Helpers.showMessage("[O2.VisualStudio.Connect] " + ex.Message);
            }
        }

		public void OnConnection(object application, ext_ConnectMode connectMode, object addInInst, ref Array custom)
		{	
		    if (onConnection == null)
                return;

            /*if(AskQuestion)
            {
                /*var result = MessageBox.Show("Do you want to load up the O2 VisualStudio AddIn", "O2 Platform",MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                { 
                    MessageBox.Show("Loading up O2...");
                }
             */
                try
                {                    
                    onConnection.Invoke(connect, new object[] { application, connectMode, addInInst , custom });                    
                }
                catch (Exception ex)
                { 
                    Connect_Helpers.showMessage("[O2.VisualStudio.Connect] OnConnection: " + ex.Message);
                }

            //}
		}

		#region not_implemented_methods
		/// <summary>Implements the OnDisconnection method of the IDTExtensibility2 interface. Receives notification that the Add-in is being unloaded.</summary>
		/// <param term='disconnectMode'>Describes how the Add-in is being unloaded.</param>
		/// <param term='custom'>Array of parameters that are host application specific.</param>
		/// <seealso class='IDTExtensibility2' />
		public void OnDisconnection(ext_DisconnectMode disconnectMode, ref Array custom)
		{
            try
            {
                onDisconnection.Invoke(connect, new object[] { disconnectMode , custom });                    
            }
            catch (Exception ex)
            { 
                Connect_Helpers.showMessage("[O2.VisualStudio.Connect] onDisconnection: " + ex.Message);
            }
		}

		/// <summary>Implements the OnAddInsUpdate method of the IDTExtensibility2 interface. Receives notification when the collection of Add-ins has changed.</summary>
		/// <param term='custom'>Array of parameters that are host application specific.</param>
		/// <seealso class='IDTExtensibility2' />		
		public void OnAddInsUpdate(ref Array custom)
		{
		}

		/// <summary>Implements the OnStartupComplete method of the IDTExtensibility2 interface. Receives notification that the host application has completed loading.</summary>
		/// <param term='custom'>Array of parameters that are host application specific.</param>
		/// <seealso class='IDTExtensibility2' />
		public void OnStartupComplete(ref Array custom)
		{
		}

		/// <summary>Implements the OnBeginShutdown method of the IDTExtensibility2 interface. Receives notification that the host application is being unloaded.</summary>
		/// <param term='custom'>Array of parameters that are host application specific.</param>
		/// <seealso class='IDTExtensibility2' />
		public void OnBeginShutdown(ref Array custom)
		{
		}
		#endregion 

		/// <summary>Implements the QueryStatus method of the IDTCommandTarget interface. This is called when the command's availability is updated</summary>
		/// <param term='commandName'>The name of the command to determine state for.</param>
		/// <param term='neededText'>Text that is needed for the command.</param>
		/// <param term='status'>The state of the command in the user interface.</param>
		/// <param term='commandText'>Text requested by the neededText parameter.</param>
		/// <seealso class='Exec' />
        /// 
         
		public void QueryStatus(string commandName, vsCommandStatusTextWanted neededText, ref vsCommandStatus status, ref object commandText)
		{	
		    if (queryStatus == null)
                return;
			try
            {                
                object[] args  = new object[] { commandName , neededText, status, commandText };
                queryStatus.Invoke(connect, args);
                status = (vsCommandStatus)args[2];
                commandText = (object)args[3];                
            }
            catch (Exception ex)
            { 
                Connect_Helpers.showMessage("[O2.VisualStudio.Connect] queryStatus: " + ex.Message);
            }
		}

		/// <summary>Implements the Exec method of the IDTCommandTarget interface. This is called when the command is invoked.</summary>
		/// <param term='commandName'>The name of the command to execute.</param>
		/// <param term='executeOption'>Describes how the command should be run.</param>
		/// <param term='varIn'>Parameters passed from the caller to the command handler.</param>
		/// <param term='varOut'>Parameters passed from the command handler to the caller.</param>
		/// <param term='handled'>Informs the caller if the command was handled or not.</param>
		/// <seealso class='Exec' />
		public void Exec(string commandName, vsCommandExecOption executeOption, ref object varIn, ref object varOut, ref bool handled)
		{	
            if (exec == null)
                return;
			try
            {
                var args = new object[] { commandName , executeOption, varIn, varOut , handled };
                exec.Invoke(connect, args); 
                varIn   = args[2];
                varOut  = args[3];
                handled = (bool)args[4];
            }
            catch (Exception ex)
            { 
                Connect_Helpers.showMessage("[O2.VisualStudio.Connect] exec: " + ex.Message);
            }		
		}
		
	}
}