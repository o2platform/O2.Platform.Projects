//Copyright (c) Microsoft Corporation.  All rights reserved.

using System;
using System.Collections;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System.IO;
using System.Collections.Specialized;

namespace Xml.Schema.Linq.VS
{
    public class LinqToXsdTask : ToolTask
    {
        ITaskItem[] _sources;
        string _filename;
        string _configurationFile;
        string _linqToXsdDir = String.Empty;

        [Output]
        public string Filename
        {
            get { return _filename; }
            set { _filename = value; }
        }

        public string LinqToXsdDir
        {
            get { return _linqToXsdDir; }
            set { _linqToXsdDir = value; }
        }

        public string ConfigurationFile
        {
            get { return _configurationFile; }
            set { _configurationFile = value; }
        }

        public ITaskItem[] Sources
        {
            get { return _sources; }
            set { _sources = value; }
        }

        #region ToolTask Members
        protected override string ToolName
        {
            get { return "LinqToXsd.exe"; }
        }

        /// <summary>
        /// Use ToolLocationHelper to find ILASM.EXE in the Framework directory
        /// </summary>
        /// <returns></returns>
        protected override string GenerateFullPathToTool()
        {
            return LinqToXsdDir + "\\LinqToXsd.exe";
        }

        protected override MessageImportance StandardErrorLoggingImportance { get { return MessageImportance.High;} }


        #endregion

        #region LinqToXsd Members
        /// <summary>
        /// Construct the command line from the task properties by using the CommandLineBuilder
        /// </summary>
        /// <returns></returns>
        protected override string GenerateCommandLineCommands()
        {
            CommandLineBuilder builder = new CommandLineBuilder();

            foreach(ITaskItem iti in this._sources)
            {
                builder.AppendFileNameIfNotNull(iti);
            }

            if (this._configurationFile != null)
            {
                builder.AppendSwitchIfNotNull("/config:", new string[] {this._configurationFile}, ":");
            }

            if (this._filename != null)
            {
                builder.AppendSwitchIfNotNull("/fileName:", new string[] {this._filename}, ":");
            }

            // Log a High importance message stating the file that we are assembling
            Log.LogMessage(MessageImportance.High, "Assembling {0}", this._sources);

            // We have all of our switches added, return the commandline as a string
            return builder.ToString();
        }
        #endregion
    }
}
