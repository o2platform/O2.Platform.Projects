using System;
using System.Collections.Generic;
using mshtml;
using O2.External.IE.ExtensionMethods;
using O2.External.IE.Interfaces;
using O2.Kernel;
using O2.DotNetWrappers.ExtensionMethods;

namespace O2.External.IE.WebObjects
{
    public class IE_Form : IO2HtmlForm
    {
        public Uri PageUri { get; set; }
        public string OuterHtml { get; set; }
        public string Action { get; set; }
        public string Dir { get; set; }
        public string Encoding { get; set; }
        public string Id { get; set; }
        public int Length { get; set; }
        public string Method { get; set; }
        public string Name { get; set; }
        public string Target { get; set; }
        public string AcceptCharset { get; set; }
        public string OnSubmit { get; set; }
        public List<IO2HtmlFormField> FormFields { get; set; }

        // use this to get all details from elements
        //public List<HTMLInputElementClass> Elements { get; set; }
        //public List<HTMLSelectElementClass> Elements { get; set; }

        public IE_Form()
        {
            FormFields = new List<IO2HtmlFormField>();
        }

        public IE_Form(object _object)
            : this()
        {
            if (_object is DispHTMLFormElement)
                loadData((DispHTMLFormElement)_object);
            else
                "In IE_Form, not supported type: {0}".format(_object.comTypeName()).error();
        }

        public IE_Form(DispHTMLFormElement form) : this()
        {
            loadData(form);
            /*Elements = new List<HTMLInputElementClass>();
            foreach (var element in ((IHTMLFormElement)form))
            {                
                Elements.Add((HTMLInputElementClass) element);             
            }
             */
            //PublicDI.log.debug(" --- there are {0} elements loaded", Elements.Count);        
        }
        private void loadData(DispHTMLFormElement form)
        {            
            Action = ((IHTMLFormElement)form).action;
            Dir = ((IHTMLFormElement)form).dir;
            Encoding = ((IHTMLFormElement)form).encoding;
            Id = ((IHTMLElement)form).id;
            Length = ((IHTMLFormElement)form).length;
            Method = ((IHTMLFormElement)form).method;
            Name = ((IHTMLFormElement)form).name;
            Target = ((IHTMLFormElement)form).target;
            AcceptCharset = ((IHTMLFormElement2)form).acceptCharset;
            OuterHtml = form.outerHTML;
            if (((IHTMLFormElement)form).onsubmit != null)
                OnSubmit = ((IHTMLFormElement)form).onsubmit.ToString();

            foreach (var element in ((IHTMLFormElement)form))
            {
                switch (element.comTypeName())
                {
                    case "HTMLInputElementClass":
                    case "HTMLTextAreaElementClass":
                    case "HTMLSelectElementClass":
                    case "DispHTMLInputElement": 
                        //case "HTMLFieldSetElementClass":  //todo: need to solve this issue that shows up in news.bbc.co.uk                        
                        FormFields.Add(this.formField(element));                        
                        break;                                                    
                    default:
                        PublicDI.log.error("In IE_Form. loadData, unhandled Form type :{0}", element.comTypeName());
                        break;
                }
                //PublicDI.log.debug(element.type().Name);
            }
            
        }
        public override string ToString()
        {
            if (Name.valid())
                return Name;
            return Id;
        }
    }
}