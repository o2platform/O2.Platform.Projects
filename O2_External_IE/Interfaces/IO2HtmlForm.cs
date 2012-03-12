using System;
using System.Collections.Generic;

namespace O2.External.IE.Interfaces
{
    public interface IO2HtmlForm
    {
        Uri PageUri { get; set; }
        string OuterHtml { get; set; }
        string Action { get; set; }
        string Dir { get; set; }
        string Encoding { get; set; }
        string Id { get; set; }
        int Length { get; set; }
        string Method { get; set; }
        string Name { get; set; }
        string Target { get; set; }
        string AcceptCharset { get; set; }
        string OnSubmit { get; set; }
        List<IO2HtmlFormField> FormFields { get; set; }
    }
}