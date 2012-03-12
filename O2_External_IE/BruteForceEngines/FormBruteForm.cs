using System.Collections.Generic;
using O2.Kernel.ExtensionMethods;
using O2.DotNetWrappers.ExtensionMethods;
using O2.External.IE.Wrapper;
using O2.External.IE.Interfaces;
using O2.External.IE.ExtensionMethods;

namespace O2.External.IE.BruteForceEngines
{
    public class FormBruteForce
    {
        public bool DebugMode { get; set; }
        public bool ReturnOnFirstMatch { get; set; }
        public O2BrowserIE WebBrowser { get; set; }
        public IO2HtmlForm HtmlForm { get; set; }
        public string StartPage { get; set;}
        public string Field_1_Name { get; set; }
        public List<string> Field_1_Payloads { get; set; }
		
        public string Field_2_Name { get; set; }
        public List<string> Field_2_Payloads { get; set; }
		
        //public string TargetField { get; set; }		
        public List<string> FailureCriteria { get; set; }
        public List<string> SuccessCriteria { get; set; }
        public int Attempts { get; set; }		
        public Dictionary<string,string> Results { get; set; }
		
        public Dictionary<string,string>  start()
        {
            Attempts = 0;
            WebBrowser.openSync(StartPage);
            Results = new Dictionary<string,string>();
            foreach(var payload_1 in Field_1_Payloads)
                foreach(var payload_2 in Field_2_Payloads)
                {
                    Attempts++;
                    if (DebugMode)					
                        "trying values: {0}={1} , {2}={3}".format(Field_1_Name, payload_1, Field_2_Name, payload_2).debug();
                    HtmlForm.set(Field_1_Name,payload_1);
                    HtmlForm.set(Field_2_Name,payload_2);
                    WebBrowser.openSync("about:blank");
                    WebBrowser.submit(HtmlForm);
					
                    if (WebBrowser.contains(SuccessCriteria))
                    {
                        "MATCH!! on value: {0}={1}".format(payload_1,payload_2).info();
                        if (false==Results.ContainsKey(payload_1))
                            Results.Add(payload_1,payload_2);
                        else
                            Results.Add(payload_1.appendGuid() ,payload_2);
                        if (ReturnOnFirstMatch)
                            return Results;
                        else
                        {
                            WebBrowser.openSync(StartPage);
                            break;
                        }
                    }
                    else
                        if (false == WebBrowser.contains(FailureCriteria))
                        {
                            "Aborting Neither the SuccessCriteria or FailureCriteria was found in this page".error();
                            return Results;
                        }
                }
            return Results;
        }
		
        public void showResultsDetails()
        {
            if (Results.size()>0)
            {
                "--------------".info();
                "Matches in {0} attempts".format(Attempts).debug();
                foreach(var result in Results)
                    " {0} : {1}".format(result.Key, result.Value).info();
            }
            else
                "NO Match in {0} attempts".format(Attempts).error();
        }
    }
}