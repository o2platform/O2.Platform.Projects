using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.TextEditor;
using O2.Kernel.ExtensionMethods;
using O2.DotNetWrappers.DotNet;
using O2.DotNetWrappers.ExtensionMethods;
using O2.External.SharpDevelop.Ascx;
using O2.External.SharpDevelop.AST;
using O2.Kernel;
using ICSharpCode.TextEditor.Document;

namespace O2.External.SharpDevelop.ExtensionMethods
{
    public static class TextEditor_ExtensionMethods
    {
        
        public static TextArea          textArea        (this TextEditorControl textEditorControl)
        {
            return textEditorControl.ActiveTextAreaControl.TextArea;
        }
        public static string            get_Text        (this TextArea textArea)
        {
            return (string)textArea.invokeOnThread(() => { return textArea.Document.TextContent; });
        }        
        public static string            get_Text        (this TextEditorControl textEditorControl)
        {            
            return (string)textEditorControl.textArea().invokeOnThread(
                () =>
                {
                    var text = textEditorControl.Text; ;//textEditorControl.textArea().Text
                    return text;
                });
        }
        public static string            get_Text        (this TextEditorControl textEditorControl, int offset, int lenght)
        {
            return (string)textEditorControl.textArea().invokeOnThread(
                () =>
                {
                    var text = textEditorControl.textArea().Document.GetText(offset, lenght);
                    return text;
                });
        }
        public static int               currentOffset   (this TextEditorControl textEditorControl)
        {
            return (int)textEditorControl.textArea().invokeOnThread(
                () =>
                {
                    return textEditorControl.textArea().Caret.Offset;                    
                });
        }

        public static TextEditorControl     open            (this TextEditorControl textEditorControl, string sourceCodeFile)
        {
            return (TextEditorControl)textEditorControl.invokeOnThread(
                () =>
                {
                    if (sourceCodeFile.fileExists())
                        textEditorControl.LoadFile(sourceCodeFile);
                    else
                    {
                        textEditorControl.SetHighlighting("C#");
                        textEditorControl.Document.TextContent = sourceCodeFile;
                    }
                    return textEditorControl;
                });
        }     
        public static TextEditorControl     insertTextAtCurrentCaretLocation(this TextEditorControl textEditorControl, string textToInsert)
        {
            return (TextEditorControl)textEditorControl.textArea().invokeOnThread(
                () =>
                {
                    var caret = textEditorControl.textArea().Caret;
                    textEditorControl.ActiveTextAreaControl.Document.Insert(caret.Offset, textToInsert);
                    //caret.Offset += textToInsert.Length;
                    return textEditorControl;
                });
        }
        public static TextEditorControl     showLineNumbers (this TextEditorControl textEditorControl, bool value)
        {
            return (TextEditorControl)textEditorControl.textArea().invokeOnThread(
                () =>
                {
                    textEditorControl.ShowLineNumbers = value;
                    return textEditorControl;
                });
        }
        public static TextEditorControl     showTabs        (this TextEditorControl textEditorControl, bool value)
        {
            return (TextEditorControl)textEditorControl.textArea().invokeOnThread(
                () =>
                {
                    textEditorControl.ShowTabs = value;
                    return textEditorControl;
                });
        }
        public static TextEditorControl     showSpaces      (this TextEditorControl textEditorControl, bool value)
        {
            return (TextEditorControl)textEditorControl.textArea().invokeOnThread(
                () =>
                {
                    textEditorControl.ShowSpaces = value;
                    return textEditorControl;
                });
        }
        public static TextEditorControl     showInvalidLines(this TextEditorControl textEditorControl, bool value)
        {
            return (TextEditorControl)textEditorControl.textArea().invokeOnThread(
                () =>
                {
                    textEditorControl.ShowInvalidLines = value;
                    return textEditorControl;
                });
        }
        public static TextEditorControl     showEOLMarkers  (this TextEditorControl textEditorControl, bool value)
        {
            return (TextEditorControl)textEditorControl.textArea().invokeOnThread(
                () =>
                {
                    textEditorControl.ShowEOLMarkers = value;
                    return textEditorControl;
                });
        }
        public static TextEditorControl     showHRuler      (this TextEditorControl textEditorControl, bool value)
        {
            return (TextEditorControl)textEditorControl.textArea().invokeOnThread(
                () =>
                {
                    textEditorControl.ShowHRuler = value;
                    return textEditorControl;
                });
        }
        public static TextEditorControl     showVRuler      (this TextEditorControl textEditorControl, bool value)
        {
            return (TextEditorControl)textEditorControl.textArea().invokeOnThread(
                () =>
                {
                    textEditorControl.ShowVRuler = value;
                    return textEditorControl;
                });
        }

        public static TextEditorControl     textEditor(this ascx_SourceCodeEditor sourceCodeEditor)
        {
            return sourceCodeEditor.textEditorControl();
        }
        public static TextEditorControl     textEditor(this ascx_SourceCodeViewer sourceCodeViewer)
        {
            return sourceCodeViewer.textEditorControl();
        }
        public static TextEditorControl     textEditorControl(this ascx_SourceCodeEditor sourceCodeEditor)
        {
            return sourceCodeEditor.getObject_TextEditorControl();
        }
        public static TextEditorControl     textEditorControl(this ascx_SourceCodeViewer sourceCodeViewer)
        {
            return sourceCodeViewer.editor().getObject_TextEditorControl();
        }
        public static TextEditorControl     showAstValueInSourceCode(this TextEditorControl textEditorControl, AstValue<object> astValue)
        {
            return (TextEditorControl)textEditorControl.invokeOnThread(() =>
                {
                    PublicDI.log.error("{0} {1} - {2}", astValue.Text, astValue.StartLocation, astValue.EndLocation);

                    var start = new TextLocation(astValue.StartLocation.X - 1,
                                                                        astValue.StartLocation.Y - 1);
                    var end = new TextLocation(astValue.EndLocation.X - 1, astValue.EndLocation.Y - 1);
                    var selection = new DefaultSelection(textEditorControl.Document, start, end);
                    textEditorControl.ActiveTextAreaControl.SelectionManager.SetSelection(selection);
                    setCaretToCurrentSelection(textEditorControl);
                    return textEditorControl;
                });
        }
        public static TextEditorControl     setCaretToCurrentSelection(this TextEditorControl textEditorControl)
        {
            return (TextEditorControl)textEditorControl.invokeOnThread(() =>
                {
                    var finalCaretPosition = textEditorControl.ActiveTextAreaControl.TextArea.SelectionManager.SelectionCollection[0].StartPosition;
                    var tempCaretPosition = new TextLocation
                    {
                        X = finalCaretPosition.X,
                        Y = finalCaretPosition.Y + 10
                    };
                    textEditorControl.ActiveTextAreaControl.Caret.Position = tempCaretPosition;
                    textEditorControl.ActiveTextAreaControl.TextArea.ScrollToCaret();
                    textEditorControl.ActiveTextAreaControl.Caret.Position = finalCaretPosition;
                    textEditorControl.ActiveTextAreaControl.TextArea.ScrollToCaret();
                    return textEditorControl;
                });
        }        
        public static TextEditorControl     _textEditor(this ascx_SourceCodeViewer sourceCodeViewer)
		{
			"here".info();			
			return sourceCodeViewer._textEditorControl();
		}
		public static TextEditorControl     _textEditorControl(this ascx_SourceCodeViewer sourceCodeViewer)
		{
			"here 22".info();
			return null;
			//return sourceCodeViewer._editor().getObject_TextEditorControl();
		}

    }
}
