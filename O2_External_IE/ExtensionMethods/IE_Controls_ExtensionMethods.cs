using System;
using System.Collections.Generic;
using System.Windows.Forms;
using mshtml;
using O2.DotNetWrappers.ExtensionMethods;
using O2.External.IE.Interfaces;
using O2.External.IE.WebObjects;
using O2.External.IE.Wrapper;


namespace O2.External.IE.ExtensionMethods
{
    public static class IE_Controls_ExtensionMethods
    {
        public static O2BrowserIE add_Browser(this Control control)
        {
            //var browserType = "O2_External_IE.dll".type("O2BrowserIE");
            //return hostControl.add_Control(browserType);
            return (O2BrowserIE)control.add_WebBrowser();
        }

        public static IO2Browser add_WebBrowser(this Control control)
        {            
            return (IO2Browser) control.invokeOnThread(
                                    () =>
                                        {
                                            var o2BrowserIE = new O2BrowserIE {Dock = DockStyle.Fill};
                                            control.Controls.Add(o2BrowserIE);                        
                                            return o2BrowserIE;
                                        });
        }
        
        public static IO2Browser add_WebBrowserWithLocationBar(this Control control)
        {
            return control.add_WebBrowserWithLocationBar("");
        }

        public static IO2Browser add_WebBrowserWithLocationBar(this Control control, string startUrl)
        {
            return control.add_WebBrowserWithLocationBar(startUrl,(webBrowser, url) => webBrowser.open(url));
        }

        public static IO2Browser add_WebBrowserWithLocationBar(this Control control, string startUrl, Action<IO2Browser,string> onEnter)
        {
            return control.add_WebBrowserWithLocationBar(startUrl,
                                                         (keys, webBrowser, url) =>
                                                             {
                                                                 if (keys == Keys.Enter)
                                                                     onEnter(webBrowser, url);
                                                             });
        }

        public static IO2Browser add_WebBrowserWithLocationBar(this Control control, string startUrl, Action<Keys, IO2Browser, string> onKeyUp)
        {
            return (IO2Browser)control.invokeOnThread(
                                   () =>
                                       {
                                           var splitControl = control.add_SplitContainer(
                                               false, 		//setOrientationToVertical
                                               true,		// setDockStyleoFill
                                               false);		// setBorderStyleTo3D                        
                                           splitControl.FixedPanel = FixedPanel.Panel1;
                                           splitControl.Panel1MinSize = 20;
                                           splitControl.SplitterDistance = 20;
                                           control.Controls.Add(splitControl);
                                           var textBox = splitControl.Panel1.add_TextBox();
                                           //textBox.Multiline = false;
                                           textBox.Dock = DockStyle.Fill;
                                           var webBrowser = splitControl.Panel2.add_WebBrowser();
                                           
                                           webBrowser.onDocumentCompleted +=
                                               htmlPage => textBox.set_Text(htmlPage.PageUri.ToString());
                                           //textBox.TextChanged += (sender, e) => webBrowser.open(textBox.Text);
                                           textBox.KeyUp += (sender, e) => onKeyUp(e.KeyCode, webBrowser, textBox.Text);
                                           textBox.Text = startUrl;
                                           if (startUrl != "")
                                               webBrowser.open(startUrl);
                                           return webBrowser;
                                       });
        }

        public static IO2HtmlFormField formField(this IO2HtmlForm form, object data)
        {
            /*"form field type: {0}".format(data.comTypeName()).error();
            foreach(var prop in data.type().properties())
                "  p: {0}".format(prop.Name).debug();\*/
            try
            {
                object name = null;
                object type = null;
                object value = null;
                object disabled = null;
                if (data is HTMLSelectElementClass)
                {
                    name = data.prop("name");
                    type = data.prop("type");
                    value = data.prop("value");
                    disabled = data.prop("disabled");
                }
                else if (data is DispHTMLInputElement)
                {
                    name = ((DispHTMLInputElement)data).name;
                    type = ((DispHTMLInputElement)data).type;
                    value = ((DispHTMLInputElement)data).value;
                    disabled = !((DispHTMLInputElement)data).disabled;
                }
                else
                {
                    name = data.prop("name");
                    type = data.prop("type");
                    value = data.prop("value");
                    disabled = data.prop("disabled");
                }
                return new IE_Form_Field
                {
                    Form = form,
                    Name = (name != null) ? name.ToString() : "",
                    Type = (type != null) ? type.ToString() : "",
                    Value = (value != null) ? value.ToString() : "",
                    Enabled = (disabled != null) ? ! bool.Parse(disabled.ToString()) : false
                };
            }
            catch (Exception ex)
            {
                ex.log("in formField");
                return null;
            }
        }

        public static IO2HtmlFormField formField(this IO2HtmlForm form, string name, string type, string value, bool enabled)
        {
            return new IE_Form_Field
                       {
                           Form = form,
                           Name = name,
                           Type = type,
                           Value = value,
                           Enabled = enabled
                       };
        }
       
        public static O2BrowserIE onTextChange(this O2BrowserIE o2BrowserIE, Action<string> callback)
        {
            return o2BrowserIE.onEditedHtmlChange(callback);
        }

        public static O2BrowserIE onEditedHtmlChange(this O2BrowserIE o2BrowserIE, Action<string> onHtmlChange)
        {
            return (O2BrowserIE)o2BrowserIE.invokeOnThread(() =>
                    {
                        if (o2BrowserIE.Document != null)
                        {
                            var markupContainer2 = (mshtml.IMarkupContainer2)o2BrowserIE.Document.DomDocument;

                            uint pdwCookie;
                            markupContainer2.RegisterForDirtyRange(
                                new IEChangeSink(() => onHtmlChange(o2BrowserIE.html())), out pdwCookie);
                        }
                        return o2BrowserIE;
                    });
        }

        public static HtmlDocument document(this O2BrowserIE o2BrowserIE)
        {
            return (HtmlDocument)o2BrowserIE.invokeOnThread(() => o2BrowserIE.Document);
        }

        public static string html(this O2BrowserIE o2BrowserIE)
        {
            return o2BrowserIE.text();
        }

        public static string text(this O2BrowserIE o2BrowserIE)
        {
            return (string)o2BrowserIE.invokeOnThread(() => o2BrowserIE.DocumentText);
        }

        public static bool contains(this O2BrowserIE o2BrowserIE, string stringToFind)
        {
            return o2BrowserIE.text().contains(stringToFind);
        }

        public static bool contains(this O2BrowserIE o2BrowserIE, List<string> stringsToFind)
        {
            foreach (var stringToFind in stringsToFind)
                if (o2BrowserIE.text().contains(stringToFind))
                    return true;
            return false;
        }

        public static O2BrowserIE silent(this O2BrowserIE o2BrowserIE, bool value)
        {            
            return (O2BrowserIE)o2BrowserIE.invokeOnThread(
                () =>
                {
                    o2BrowserIE.ActiveXInstance.prop("Silent", value);
                    return o2BrowserIE;
                });
        }

        public static ExtendedWebBrowser.IWebBrowser2 activeX(this O2BrowserIE o2BrowserIE)
        {
            return (ExtendedWebBrowser.IWebBrowser2)o2BrowserIE.ActiveXInstance;
        }

        public static O2BrowserIE openBlank(this O2BrowserIE o2BrowserIE)
        {
            o2BrowserIE.openSync("about:blank");
            return o2BrowserIE;
        }

        public static string cookie(this O2BrowserIE o2BrowserIE)
        {
            return (string)o2BrowserIE.invokeOnThread(() => o2BrowserIE.document().Cookie);
        }

        public static O2BrowserIE cookie(this O2BrowserIE o2BrowserIE, string value)
        {
            return (O2BrowserIE)o2BrowserIE.invokeOnThread(
                () =>
                {
                    var document = o2BrowserIE.document();
                    if (document != null)
                        document.Cookie = value;
                    return o2BrowserIE;
                });
        }

        public static O2BrowserIE clearCookie(this O2BrowserIE o2BrowserIE)
        {
            return o2BrowserIE.cookie("");
        }

        public static void logBeforeNavigate(this O2BrowserIE o2BrowserIE)
        {            
            o2BrowserIE.BeforeNavigate +=
                (URL, flags, postData, headers)
                    =>
                    {
                        "on before Navigate for {0}".format(URL).debug();
                        if (postData != null)
                        {
                            "post flags: {0}".format(flags).info();
                            "post headers: {0}".format(headers).info();
                            "post url: {0}".format(URL).info();
                            "post data: {0}".format(((byte[])postData).ascii()).info();
                        }
                    };
        }

        public static IO2HtmlPage open(this Uri uri)
        {
            var browser = (O2BrowserIE)O2.Kernel.open.webBrowser();
            return browser.openSync(uri);
        }

        public static O2BrowserIE set_Text(this O2BrowserIE o2BrowserIE, string text)
        {
            return (O2BrowserIE)o2BrowserIE.invokeOnThread(
                () =>
                {
                    o2BrowserIE.DocumentText = text;
                    return o2BrowserIE;
                });

        }

        public static O2BrowserIE editMode(this O2BrowserIE o2BrowserIE)
        {
            return (O2BrowserIE)o2BrowserIE.invokeOnThread(
                () =>
                {
                    o2BrowserIE.HtmlEditMode = true;
                    return o2BrowserIE;
                });
        }

    }
}