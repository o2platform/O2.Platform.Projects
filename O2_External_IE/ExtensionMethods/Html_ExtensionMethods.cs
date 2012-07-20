using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using O2.DotNetWrappers.ExtensionMethods;
using O2.DotNetWrappers.Network;
using O2.External.IE.Interfaces;
using O2.Kernel;


namespace O2.External.IE.ExtensionMethods
{
    public static class Html_ExtensionMethods
    {
        public static List<String> forms(this IO2HtmlPage htmlPage)
        {
            var forms = new List<String>();
            foreach (var form in htmlPage.Forms)
                forms.add(form.str());
            return forms;
        }
        
        public static IO2HtmlForm form(this IO2HtmlPage htmlPage, string nameOrId)
        {
            foreach (var form in htmlPage.Forms)
                if (form.Name == nameOrId || form.Id == nameOrId)
                    return form;
            return null;
        }
        
        public static string formData(this IO2HtmlForm form)
        {
            var formData = new StringBuilder();
            foreach (var field in form.FormFields)
                formData.Append(string.Format("{0}={1}&", field.Name, WebEncoding.urlEncode(field.Value)));
            formData.removeLastChar();

            return formData.ToString();
        }

        public static string formDetails(this IO2HtmlForm form)
        {
            return form.fieldNamesAndValues().str();
        }

        public static string name(this IO2HtmlForm form)
        {
            if (form.Name.valid())
                return form.Name;
            if (form.Id.valid())
                return form.Id;
            return "";
        }

        public static Dictionary<string, IO2HtmlFormField> fields(this IO2HtmlForm form)
        {
            var fields = new Dictionary<string, IO2HtmlFormField>();
            foreach (var field in form.FormFields)
                fields.Add(field.Name, field);
            return fields;
        }

        public static List<string> fieldNames(this IO2HtmlForm form)
        {
            var fieldNames = new List<string>();
            foreach (var field in form.FormFields)
                fieldNames.Add(field.Name);
            return fieldNames;
        }

        public static List<string> fieldNamesAndValues(this IO2HtmlForm form)
        {
            var fieldNames = new List<string>();
            foreach (var field in form.FormFields)
                fieldNames.Add("{0} = {1}".format(field.Name, field.Value));
            return fieldNames;
        }

        public static IO2HtmlForm set(this IO2HtmlForm form, string fieldName, string value)
        {
            try
            {
                var fields = form.fields();
                if (fields.ContainsKey(fieldName))
                    fields[fieldName].Value = value;
                else
                    O2.Kernel.PublicDI.log.error("the provided IO2HtmlForm.form did not contain the field: {0}", fieldName);
                return form;
            }
            catch (Exception ex)
            {
                PublicDI.log.ex(ex, "in IO2HtmlForm.set");
                return null;
            }            
        }

        public static string get(this IO2HtmlForm form, string fieldName)
        {
            var fields = form.fields();
            if (fields.ContainsKey(fieldName))
                return fields[fieldName].Value;

            O2.Kernel.PublicDI.log.error("the provided IO2HtmlForm.form did not contain the field: {0}", fieldName);
            return "";
        }

        public static IO2HtmlForm remove(this IO2HtmlForm form, string name)
        {
            var fields = form.fields();
            if (fields.ContainsKey(name))
                form.FormFields.Remove(fields[name]);
            else
                "in IO2HtmlForm form.remove(), could not find field: {0}".format(name).error();
            return form;
        }

        public static Uri uri(this IO2HtmlForm form)
        {
            return form.PageUri;
        }
     
        public static string page(this Uri uri)
        {
            return uri.Segments[uri.Segments.Count() - 1];
        }

        public static string action(this IO2HtmlForm form)
        {
            // this is just a first pass at trying to map this value
            // maybe see if I can get IE to resolve this for me
            if (form.Action.starts(form.uri().page()))
                return form.uri().OriginalString;


            return form.Action;
        }
                
        public static string html(this IO2HtmlPage htmlPage)
        {
            return htmlPage.PageSource;
        }

        public static IO2HtmlPage submit(this IO2Browser o2Browser, IO2HtmlForm form)
        {
            /*"submiting form:{0}".format(form.name()).info();
            "          to  :{0}".format(form.action()).debug();
            "		   with:{0}".format(form.formData()).debug();        	
            */
            return o2Browser.POST(form.action(), form.formData());
        }
    }
}
