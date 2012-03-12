// This file is part of the OWASP O2 Platform (http://www.owasp.org/index.php/OWASP_O2_Platform) and is released under the Apache 2.0 License (http://www.apache.org/licenses/LICENSE-2.0)


using mshtml;

namespace O2.External.IE.WebObjects
{
    public class O2Form
    {

        /*public Dictionary<String, O2FormInputField> dFormInputFields = new Dictionary<String, O2FormInputField>();
        private HTMLFormElementClass hfeForm;*/

        public HTMLFormElementClass htmlForm { get; set; }

        public O2Form(HTMLFormElementClass _htmlForm)
        {
            htmlForm = _htmlForm;
            //populateFieldsFromHtmlElementData(heForm);
        }

        /*   public String sId
        {
            get { return hfeForm.id; }            
        }

        public String sName
        {
            get { return hfeForm.name; }            
        }

        public String sTarget
        {
            get { return hfeForm.target; }            
        }

        public String sAction
        {
            get { return hfeForm.action; }            
        }

        public String sOuterHtml
        {
            get { return hfeForm.outerHTML; }            
        }

        public void populateFieldsFromHtmlElementData(HtmlElement heForm)
        {
            if (heForm.DomElement.GetType().FullName == "mshtml.HTMLFormElementClass")
            {
                hfeForm = (HTMLFormElementClass) heForm.DomElement;
                foreach (HtmlElement heInputField in heForm.GetElementsByTagName("input"))
                {
                    var fifFormInputField = new O2FormInputField(heInputField);
                    if (fifFormInputField.sName != null &&
                        false == dFormInputFields.ContainsKey(fifFormInputField.sName))
                        dFormInputFields.Add(fifFormInputField.sName, fifFormInputField);
                    else
                    {
                        if (fifFormInputField.sId != null &&
                            false == dFormInputFields.ContainsKey(fifFormInputField.sId))
                            dFormInputFields.Add(fifFormInputField.sId, fifFormInputField);
                        else
                            DI.log.error(
                                "in populateFieldsFromHtmlElementData(), could not item field with Id (duplicated name and id)");
                    }
                }
            }
            else
            {
                DI.log.error("in O2Form(), provided object was not a O2Form element: {0}",
                                heForm.DomElement.GetType().FullName);
            }
        }

        public bool submitFormWithValues(Dictionary<String, String> dValues)
        {
            try
            {
                // populate WebBrowser O2Form with the provided values
                foreach (var name in dValues.Keys)
                    if (dFormInputFields.ContainsKey(name))
                        dFormInputFields[sName].sValue = dValues[name];
                    else
                        DI.log.error("in submitFormWithValues, input field not found:{0}", name);
                // then submit the O2Form
                hfeForm.submit();
                return true;
            }
            catch (Exception ex)
            {
                DI.log.error("in submitFormWithValues: {0}", ex.Message);
                return false;
            }
        }*/
    }
}