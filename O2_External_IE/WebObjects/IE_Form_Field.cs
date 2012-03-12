using O2.External.IE.Interfaces;

namespace O2.External.IE.WebObjects
{
    public class IE_Form_Field : IO2HtmlFormField
    {
        public IO2HtmlForm Form { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }
        public bool Enabled { get; set; }        
    }
}