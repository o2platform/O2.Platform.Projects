namespace O2.External.IE.Interfaces
{
    public interface IO2HtmlFormField
    {
        IO2HtmlForm Form { get; set; }
        string Name { get; set; }
        string Type { get; set; }
        string Value { get; set; }
        bool Enabled { get; set; }
    }
}