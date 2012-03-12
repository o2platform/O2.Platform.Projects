// This file is part of the OWASP O2 Platform (http://www.owasp.org/index.php/OWASP_O2_Platform) and is released under the Apache 2.0 License (http://www.apache.org/licenses/LICENSE-2.0)
//using mshtml;

using System;
using System.Windows.Forms;
using mshtml;

namespace O2.External.IE.WebObjects
{
    public class O2FormInputField
    {
        public HTMLInputElementClass hieForm_InputField;

        public O2FormInputField(HtmlElement heForm_InputField)
        {
            //String asd = heForm_InputField.DomElement.GetType().FullName;
            if (heForm_InputField.DomElement.GetType().FullName == "mshtml.HTMLInputElementClass")
            {
                hieForm_InputField = (HTMLInputElementClass) heForm_InputField.DomElement;
            }
            else
            {
                DI.log.error("in O2FormInputField(), provided object was not a Input Form element: {0}",
                                heForm_InputField.DomElement.GetType().FullName);
            }
        }

        public String sId
        {
            get { return hieForm_InputField.id; }
            set { hieForm_InputField.id = value; }
        }

        public String sName
        {
            get { return hieForm_InputField.name; }
            set { hieForm_InputField.name = value; }
        }

        public String sValue
        {
            get { return hieForm_InputField.value; }
            set { hieForm_InputField.value = value; }
        }

        public String sOuterHtml
        {
            get { return hieForm_InputField.outerHTML; }
            set { hieForm_InputField.outerHTML = value; }
        }
    }
}