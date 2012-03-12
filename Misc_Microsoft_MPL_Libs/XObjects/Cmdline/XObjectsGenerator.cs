//Copyright (c) Microsoft Corporation.  All rights reserved.

using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Linq;
using System.CodeDom;
using System.CodeDom.Compiler;
using Xml.Schema.Linq;
using Xml.Schema.Linq.CodeGen;
using System.IO;
using System.Globalization;
using Microsoft.CSharp;
using System.Reflection;

namespace XObjectsGenerator
{
    class XObjectsGenerator {

        private static Assembly ThisAssembly;

        public static int Main(string[] args) {

            ThisAssembly  = Assembly.GetExecutingAssembly();
            XmlSchemaSet set = new XmlSchemaSet();
            ValidationEventHandler veh = new ValidationEventHandler(ValidationCallback);
            set.ValidationEventHandler += veh;
            string csFileName = string.Empty;
            string configFileName = null;
            string assemblyName = string.Empty;
            bool fSourceNameProvided = false;
            bool xmlSerializable = false;
            bool nameMangler2 = false;

            if (args.Length == 0) {
                PrintHelp();
                return 0;
            }
            for (int i = 0; i < args.Length; i++) {
                string arg = args[i];
                string value = string.Empty;
                bool argument = false;
                
                if (arg.StartsWith("/") || arg.StartsWith("-")) {
                    argument = true;
                    int colonPos = arg.IndexOf(":");
                    if (colonPos != -1) {
                        value = arg.Substring(colonPos + 1);
                        arg = arg.Substring(0, colonPos);
                    }
                }
                arg = arg.ToLower(CultureInfo.InvariantCulture);
                if (!argument) {
                    try{
                        set.Add(null, CreateReader(arg));
                    }
                    catch(Exception e){
                        PrintErrorMessage(e.ToString());
                        return 1;
                    }
                    if (csFileName == string.Empty) {
                        csFileName = Path.ChangeExtension(arg, "cs");
                    }
                }
                else if (ArgumentMatch(arg, "?") || ArgumentMatch(arg, "help")) {
                    PrintHelp();
                    return 0;
                }
                else if (ArgumentMatch(arg, "config")) {
                    configFileName = value;
                }
                else if (ArgumentMatch(arg, "filename")) {
                    csFileName = value;
                    fSourceNameProvided = true;
                }
                else if (ArgumentMatch(arg, "enableservicereference")) {
                    xmlSerializable = true; 
                }
                else if (ArgumentMatch(arg, "lib"))
                {
                    assemblyName = value;
                }
                else if (ArgumentMatch(arg, "namemangler2"))
                {
                    nameMangler2 = true;
                }
            }
            if(assemblyName != string.Empty && !fSourceNameProvided)
            { //only generate assembly
                csFileName = string.Empty;
            }
            set.Compile();
            set.ValidationEventHandler  -= veh;
            if (set.Count > 0 && set.IsCompiled) {
                try {
                    GenerateXObjects(
                        set, csFileName, configFileName, assemblyName, xmlSerializable, nameMangler2);
                }
                catch(Exception e) {
                    PrintErrorMessage(e.ToString());
                    return 1;
                }
            }
            return 0;
        }

        static void GenerateXObjects(
            XmlSchemaSet set, string csFileName, string configFileName, string assemblyName, bool xmlSerializable, bool nameMangler2) 
        {
            LinqToXsdSettings configSettings = new LinqToXsdSettings(nameMangler2);
            if (configFileName != null) {
                configSettings.Load(configFileName);
            }
            configSettings.EnableServiceReference = xmlSerializable;
            XsdToTypesConverter xsdConverter = new XsdToTypesConverter(configSettings);
            ClrMappingInfo mapping = xsdConverter.GenerateMapping(set);

            CodeDomTypesGenerator codeGenerator = new CodeDomTypesGenerator(configSettings);
            CodeCompileUnit ccu = new CodeCompileUnit();
         //   if (mapping != null)                            //DC
                foreach(CodeNamespace codeNs in codeGenerator.GenerateTypes(mapping)) {
                    ccu.Namespaces.Add(codeNs);
            }
            //Write to file
            CSharpCodeProvider provider = new CSharpCodeProvider();
            if (csFileName != string.Empty)
            {
                /*
                StreamWriter sw = new StreamWriter(csFileName);
                provider.GenerateCodeFromCompileUnit(ccu, sw, new CodeGeneratorOptions());
                sw.Flush(); 
                sw.Close();
                 * */
                using (var update = 
                    new Update(csFileName, System.Text.Encoding.UTF8))
                {
                    provider.GenerateCodeFromCompileUnit(
                        ccu, update.Writer, new CodeGeneratorOptions());
                }                
                PrintMessage(csFileName);
            }
            if(assemblyName != string.Empty)
            {
                CompilerParameters options = new CompilerParameters();
                options.OutputAssembly = assemblyName;
                options.IncludeDebugInformation = true;
                options.TreatWarningsAsErrors = true;
                options.ReferencedAssemblies.Add("System.dll");
                options.ReferencedAssemblies.Add("System.Core.dll");
                options.ReferencedAssemblies.Add("System.Xml.dll");
                options.ReferencedAssemblies.Add("System.Xml.Linq.dll");
                options.ReferencedAssemblies.Add("O2_Misc_Microsoft_MPL_Libs.dll");
                CompilerResults results = provider.CompileAssemblyFromDom(options, ccu);
                if (results.Errors.Count > 0)
                {
                    PrintErrorMessage("compilation error(s): ");
                    for (int i = 0; i < results.Errors.Count; i++)
                        PrintErrorMessage(results.Errors[i].ToString());
                }
                else
                {
                    PrintMessage("Generated Assembly: " + results.CompiledAssembly.ToString());
                }
            };
        }

        private static XmlReader CreateReader(string xsdFile) {
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.ProhibitDtd = false;
            return XmlReader.Create(xsdFile, settings);
        }
        
        private static void PrintMessage(string csFileName) {
            PrintHeader();
            //Console.WriteLine("Generated " + csFileName + "...");
        }

        private static void PrintErrorMessage(String e)
        {
            Console.Error.WriteLine("LinqToXsd: error TX0001: {0}", e);
        }

        private static void PrintErrorMessage(ValidationEventArgs args)
        {

            Console.Error.WriteLine("{0}({1},{2}): {3} TX0001: {4}", 
                                    args.Exception.SourceUri.Replace("file:///", "").Replace('/', '\\'), 
                                    args.Exception.LineNumber,
                                    args.Exception.LinePosition, 
                                    args.Severity == XmlSeverityType.Warning ? "warning" : "error",
                                    args.Message);
        }

        private static void PrintHeader() {
            //Console.WriteLine(String.Format(CultureInfo.CurrentCulture, "[Microsoft (R) .NET Framework, Version {0}]", ThisAssembly.ImageRuntimeVersion));
        }
        private static void PrintHelp() {
            PrintHeader();
            string name = ThisAssembly.GetName().Name;
            Console.WriteLine();
            Console.WriteLine(name + " - " + "Utility to generate typed wrapper classes from a XML Schema");
            Console.WriteLine("Usage: " + name + " <schemaFile> [one or more schema files] [/fileName:<csFileName>.cs] [/lib:<assemblyName>] [/config:<configFileName>.xml] [/enableServiceReference] [/nameMangler2]");
        }

        private static void ValidationCallback(object sender, ValidationEventArgs args) {
            PrintErrorMessage(args);
        }

        private static bool ArgumentMatch(string arg, string toMatch) {
            if (arg[0] != '/' && arg[0] != '-') {
                return false;
            }
            arg = arg.Substring(1);
            return arg == toMatch;
        }
    }
}
