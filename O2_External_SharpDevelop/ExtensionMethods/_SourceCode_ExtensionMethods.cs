using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.TextEditor;
using O2.External.SharpDevelop.Ascx;
using O2.Kernel.ExtensionMethods;

namespace O2.External.SharpDevelop.ExtensionMethods
{
	public static class _SourceCode_ExtensionMethods
	{
		public static string hello(this string _string)
		{
			return _string + " world";
		}

		public static ascx_SourceCodeEditor _editor(this ascx_SourceCodeViewer sourceCodeViewer)
		{
			return sourceCodeViewer.getSourceCodeEditor();
		}

		public static TextEditorControl _textEditor(this ascx_SourceCodeViewer sourceCodeViewer)
		{
			"here".info();			
			return sourceCodeViewer._textEditorControl();
		}

		public static TextEditorControl _textEditorControl(this ascx_SourceCodeViewer sourceCodeViewer)
		{
			"here 22".info();
			return null;
			//return sourceCodeViewer._editor().getObject_TextEditorControl();
		}
	}
}
