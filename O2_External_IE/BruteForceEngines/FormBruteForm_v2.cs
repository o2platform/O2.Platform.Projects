using System;
using System.Collections.Generic;

using O2.DotNetWrappers.ExtensionMethods;
using O2.External.IE.Interfaces;

namespace O2.External.IE.BruteForceEngines
{
    public class FormBruteForce_v2
    {
        public bool DebugMode { get; set; }
        public bool Abort { get; set; }
        public bool ReturnOnFirstMatch { get; set; }
        public IO2Browser O2Browser { get; set; }
        public Func<IO2Browser, string , string,string> SubmitFunction { get ;set ;}		
        public List<string> Field_1_Payloads { get; set; }	
        public List<string> Field_2_Payloads { get; set; }
        public List<string> FailureCriteria { get; set; }
        public List<string> SuccessCriteria { get; set; }
        public int Attempts { get; set; }		
        public Dictionary<string,string> Results { get; set; }
		
        public FormBruteForce_v2()
        {			
            Field_1_Payloads = new List<string>();
            Field_2_Payloads = new List<string>();
            FailureCriteria = new List<string>();
            SuccessCriteria = new List<string>();
        }				
		
        public string htmlFromFirstRequest()
        {
            return SubmitFunction(O2Browser, Field_1_Payloads[0],Field_2_Payloads[0]);
        }
		
        public Dictionary<string,string>  start()
        {
            Attempts = 0;
            Abort = false;
            Results = new Dictionary<string,string>();
            foreach(var payload_1 in Field_1_Payloads)
                foreach(var payload_2 in Field_2_Payloads)				
                {					
                    if (execute(payload_1, payload_2))
                        break;
                    if (Abort)
                        return Results;										
                }
            return Results;
        }
		
        public Dictionary<string,string>  start_Mode_BothSame()
        {
            Attempts = 0;
            Abort = false;
            Results = new Dictionary<string,string>();
            foreach(var payload_1 in Field_1_Payloads)				
            {					
                execute(payload_1, payload_1);					
                if (Abort)
                    return Results;										
            }
            return Results;
        }
					
        public bool execute(string payload_1, string payload_2)
        {
            Attempts++;
            if (DebugMode)					
                "trying values: {0} : {1} ".format(payload_1, payload_2).debug();
				
            var html = SubmitFunction(O2Browser, payload_1,payload_2);					
			
            if (html.contains(SuccessCriteria))
            {
                "MATCH!! on value: {0}={1}".format(payload_1,payload_2).info();
                if (false==Results.ContainsKey(payload_1))
                    Results.Add(payload_1,payload_2);
                else
                    Results.Add(payload_1.appendGuid() ,payload_2);				
                if (ReturnOnFirstMatch)
                    Abort = true;
                return true;					
            }            
            if (false == html.contains(FailureCriteria))
            {
                "Aborting Neither the SuccessCriteria or FailureCriteria was found in this page".error();
                Abort = true;					
            }
            return false;
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
		
        public void addPayloads1(params string[] payloads)
        {
            Field_1_Payloads.AddRange(payloads);			
        }
		
        public void addPayloads2(params string[] payloads)
        {
            Field_2_Payloads.AddRange(payloads);			
        }
		
        public void addBothPayloads(params string[] payloads)
        {
            Field_1_Payloads.AddRange(payloads);
            Field_2_Payloads.AddRange(payloads);			
        }
	
        public void addSuccess(params string[] sucessCriteria)
        {
            SuccessCriteria.AddRange(sucessCriteria);
        }		
		
        public void addFailure(params string[] failureCriteria)
        {
            FailureCriteria.AddRange(failureCriteria);
        }	
    }
}