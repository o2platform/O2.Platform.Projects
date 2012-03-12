namespace O2.External.IE.Interfaces
{
    public interface IO2HtmlLink
    {
        string Href { get; set; }
        string InnerText { get; set; }
        string InnerHtml { get; set; }  
        string OuterHtml { get; set; }
        string Target { get; set; }
    }
}