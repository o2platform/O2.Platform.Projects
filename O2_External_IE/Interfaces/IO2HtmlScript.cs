namespace O2.External.IE.Interfaces
{
    public interface IO2HtmlScript
    {
        string CharSet { get; set; }
        string Event { get; set; }
        bool Defer { get; set; }
        string HtmlFor { get; set; }
        string Src { get; set; }
        string Text { get; set; }
        string OuterHtml { get; set; }
    }
}