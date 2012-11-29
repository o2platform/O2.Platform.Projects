using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.CommandBars;
using EnvDTE80;
using O2.FluentSharp.VisualStudio;
using O2.DotNetWrappers.DotNet;
using O2.DotNetWrappers.ExtensionMethods;
using EnvDTE;



namespace O2.FluentSharp.VisualStudio.ExtensionMethods
{
	public static class VisualStudio_2010_ExtensionMethods_EnvDTE_Document
	{
		public static string				activeFile(this VisualStudio_2010 visualStudio)
		{
			return visualStudio.activeDocument_FullName();
		}
		public static string				activeDocument_FullName(this VisualStudio_2010 visualStudio)
		{
			var activeDocument = visualStudio.activeDocument();
			if (activeDocument.notNull())
				return activeDocument.FullName;
			return null;
		}
		public static Document				activeDocument(this VisualStudio_2010 visualStudio)
		{
			return VisualStudio_2010.DTE2.ActiveDocument;
		}
		public static List<EnvDTE.Document> documents(this VisualStudio_2010 visualStudio)
		{
			return (from window in visualStudio.windows()
					where window.Document.notNull()
					select window.Document).toList();
		}
		public static EnvDTE.Document		document(this VisualStudio_2010 visualStudio, string path)
		{
			//first search by fullpath
			var match = (EnvDTE.Document)(from document in visualStudio.documents()
										  where document.FullName == path
										  select document).first();
			if (match.notNull())
				return match;

			//then by filename
			return (EnvDTE.Document)(from document in visualStudio.documents()
									 where document.FullName.fileName() == path
									 select document).first();

		}
	}
	

	public static class VisualStudio_2010_ExtensionMethods_IVsWindowFrame
	{
		public static IVsWindowFrame		open_Document(this string file)
		{
			try
			{
				if (file.fileExists().isFalse())
					"[open_Document] provided file doesn't exist: {0}".info(file);
				else
				{
					var package = VisualStudio_2010.Package;
					var openDoc = package.getService<IVsUIShellOpenDocument>();
					IVsWindowFrame frame;
					Microsoft.VisualStudio.OLE.Interop.IServiceProvider serviceProvider;
					IVsUIHierarchy hierarchy;
					uint itemId;
					Guid logicalView = VSConstants.LOGVIEWID_Code;
					openDoc.OpenDocumentViaProject(file, ref logicalView, out serviceProvider, out hierarchy, out itemId, out frame);
					if (frame.notNull())
					{
						frame.Show();
						return frame;
					}
					"[open_Document] could not get IVsWindowFrame for file: {0}".info(file);
				}
			}
			catch (Exception ex)
			{
				ex.log("[in file.open_Document]");
			}
			return null;
		}
	}
}