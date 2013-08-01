﻿// MainClass-JS.cs
//
// Copyright 2010 Microsoft Corporation
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;

namespace Microsoft.Ajax.Utilities
{
    using Configuration;

    public partial class MainClass
    {
        #region file processing

        private int ProcessJSFile(IList<InputGroup> inputGroups, SwitchParser switchParser, StringBuilder outputBuilder)
        {
            var returnCode = 0;
            var settings = switchParser.JSSettings;

            // blank line before
            WriteProgress();

            GlobalScope sharedGlobalScope = null;
            var originalTermSetting = settings.TermSemicolons;

            // output visitor requires a text writer, so make one from the string builder
            using (var writer = new StringWriter(outputBuilder, CultureInfo.InvariantCulture))
            {
                var outputIndex = 0;
                for (var inputGroupIndex = 0; inputGroupIndex < inputGroups.Count; ++inputGroupIndex)
                {
                    var inputGroup = inputGroups[inputGroupIndex];

                    // create the a parser object for our chunk of code
                    JSParser parser = new JSParser(inputGroup.Source);
                    parser.GlobalScope = sharedGlobalScope;

                    // hook the engine events
                    parser.UndefinedReference += OnUndefinedReference;
                    parser.CompilerError += (sender, ea) =>
                    {
                        var error = ea.Error;
                        if (inputGroup.Origin == SourceOrigin.Project || error.Severity == 0)
                        {
                            // ignore severity values greater than our severity level
                            // also ignore errors that are in our ignore list (if any)
                            if (error.Severity <= switchParser.WarningLevel)
                            {
                                // we found an error
                                m_errorsFound = true;

                                // write the error out
                                WriteError(error.ToString());
                            }
                        }
                    };

                    // for all but the last item, we want the term-semicolons setting to be true.
                    // but for the last entry, set it back to its original value
                    settings.TermSemicolons = inputGroupIndex < inputGroups.Count - 1 ? true : originalTermSetting;

                    // if this is preprocess-only or echo-input, then set up the writer as the echo writer for the parser
                    if (settings.PreprocessOnly || m_echoInput)
                    {
                        parser.EchoWriter = writer;
                        if (inputGroupIndex > 0)
                        {
                            // separate subsequent input groups with an appropriate line terminator
                            writer.Write(settings.LineTerminator);
                            writer.Write(';');
                            writer.Write(settings.LineTerminator);
                        }
                    }

                    // start the timer and parse the input code
                    var scriptBlock = parser.Parse(settings);

                    if (m_outputTimer)
                    {
                        OutputTimingPoints(parser, inputGroupIndex, inputGroups.Count);
                    }

                    if (!settings.PreprocessOnly && !m_echoInput)
                    {
                        if (scriptBlock != null)
                        {
                            if (outputIndex++ > 0)
                            {
                                // separate subsequent input groups with an appropriate line terminator
                                writer.Write(settings.LineTerminator);
                            }

                            // crunch the output and write it to debug stream, but make sure
                            // the settings we use to output THIS chunk are correct
                            if (settings.Format == JavaScriptFormat.JSON)
                            {
                                if (!JSONOutputVisitor.Apply(writer, scriptBlock))
                                {
                                    returnCode = 1;
                                }
                            }
                            else
                            {
                                OutputVisitor.Apply(writer, scriptBlock, settings);
                            }
                        }
                        else
                        {
                            // no code?
                            WriteProgress(AjaxMin.NoParsedCode);
                        }
                    }

                    // save the global scope for later
                    sharedGlobalScope = parser.GlobalScope;
                }

                // give the symbols map a chance to write something at the bottom of the source file
                // (and if this isn't preprocess-only or echo)
                if (settings.SymbolsMap != null && !settings.PreprocessOnly && !m_echoInput)
                {
                    settings.SymbolsMap.EndFile(writer, settings.LineTerminator);
                }
            }

            if (switchParser.AnalyzeMode)
            {
                // blank line before
                WriteProgress();

                // output our report
                CreateReport(sharedGlobalScope, switchParser);
            }

            return returnCode;
        }

        private void OutputTimingPoints(JSParser parser, int groupIndex, int groupCount)
        {
            // frequency is ticks per second, so if we divide by 1000.0, then we will have a
            // double-precision value indicating the ticks per millisecond. Divide this into the
            // number of ticks we measure, and we'll get the milliseconds in double-precision.
            var frequency = Stopwatch.Frequency / 1000.0;

            // step names
            var stepNames = new[] { AjaxMin.StepParse, AjaxMin.StepResolve, AjaxMin.StepReorder, 
                                                AjaxMin.StepAnalyzeNode, AjaxMin.StepAnalyzeScope, AjaxMin.StepAutoRename, 
                                                AjaxMin.StepEvaluateLiterals, AjaxMin.StepFinalPass, AjaxMin.StepValidateNames };

            // and output other steps to debug
            var stepCount = parser.TimingPoints.Count;
            var latestTimingPoint = 0L;
            var previousTimingPoint = 0L;
            var sb = new StringBuilder();
            for (var ndx = stepCount - 1; ndx >= 0; --ndx)
            {
                if (parser.TimingPoints[ndx] != 0)
                {
                    // 1-based step index
                    var stepIndex = stepCount - ndx;
                    latestTimingPoint = parser.TimingPoints[ndx];
                    var deltaMS = (latestTimingPoint - previousTimingPoint) / frequency;
                    previousTimingPoint = latestTimingPoint;

                    sb.AppendFormat(AjaxMin.Culture, AjaxMin.TimerStepFormat, stepIndex, deltaMS, stepNames[stepIndex - 1]);
                    sb.AppendLine();
                }
            }

            var timerFormat = groupCount > 1 ? AjaxMin.TimerMultiFormat : AjaxMin.TimerFormat;
            var timerMessage = string.Format(CultureInfo.CurrentUICulture, timerFormat, groupIndex + 1, latestTimingPoint / frequency);
            Debug.WriteLine(timerMessage);
            Debug.Write(sb.ToString());
            WriteProgress(timerMessage);
            WriteProgress(sb.ToString());
        }

        #endregion

        #region CreateJSFromResourceStrings method

        private static string CreateJSFromResourceStrings(ResourceStrings resourceStrings)
        {
            StringBuilder sb = new StringBuilder();
            // start the var statement using the requested name and open the initializer object literal
            sb.Append("var ");
            sb.Append(resourceStrings.Name);
            sb.Append("={");

            // we're going to need to insert commas between each pair, so we'll use a boolean
            // flag to indicate that we're on the first pair. When we output the first pair, we'll
            // set the flag to false. When the flag is false, we're about to insert another pair, so
            // we'll add the comma just before.
            bool firstItem = true;

            // loop through all items in the collection
            foreach(var keyPair in resourceStrings.NameValuePairs)
            {
                // if this isn't the first item, we need to add a comma separator
                if (!firstItem)
                {
                    sb.Append(',');
                }
                else
                {
                    // next loop is no longer the first item
                    firstItem = false;
                }

                // append the key as the name, a colon to separate the name and value,
                // and then the value
                // must quote if not valid JS identifier format, or if it is, but it's a keyword
                // (use strict mode just to be safe)
                string propertyName = keyPair.Key;
                if (!JSScanner.IsValidIdentifier(propertyName) || JSScanner.IsKeyword(propertyName, true))
                {
                    sb.Append("\"");
                    // because we are using quotes for the delimiters, replace any instances
                    // of a quote character (") with an escaped quote character (\")
                    sb.Append(propertyName.Replace("\"", "\\\""));
                    sb.Append("\"");
                }
                else
                {
                    sb.Append(propertyName);
                }
                sb.Append(':');

                // make sure the Value is properly escaped, quoted, and whatever we
                // need to do to make sure it's a proper JS string.
                // pass false for whether this string is an argument to a RegExp constructor.
                // pass false for whether to use W3Strict formatting for character escapes (use maximum browser compatibility)
                // pass true for ecma strict mode
                string stringValue = ConstantWrapper.EscapeString(
                    keyPair.Value,
                    false,
                    false,
                    true
                    );
                sb.Append(stringValue);
            }

            // close the object literal and return the string
            sb.AppendLine("};");
            return sb.ToString();
        }

        #endregion

        #region Variable Renaming method

        private void ProcessRenamingFile(string filePath)
        {
            var fileReader = new StreamReader(filePath);
            try
            {
                using (var reader = XmlReader.Create(fileReader))
                {
                    fileReader = null;

                    // let the manifest factory do all the heavy lifting of parsing the XML
                    // into config objects
                    var config = ManifestFactory.Create(reader);
                    if (config != null)
                    {
                        // add any rename pairs
                        foreach (var pair in config.RenameIdentifiers)
                        {
                            m_switchParser.JSSettings.AddRenamePair(pair.Key, pair.Value);
                        }

                        // add any no-rename identifiers
                        m_switchParser.JSSettings.SetNoAutoRenames(config.NoRenameIdentifiers);
                    }
                }
            }
            catch (XmlException e)
            {
                // throw an error indicating the XML error
                System.Diagnostics.Debug.WriteLine(e.ToString());
                throw new NotSupportedException(AjaxMin.InputXmlError.FormatInvariant(e.Message));
            }
            finally
            {
                if (fileReader != null)
                {
                    fileReader.Close();
                    fileReader = null;
                }
            }
        }

        #endregion
        
        #region reporting methods

        private void CreateReport(GlobalScope globalScope, SwitchParser switchParser)
        {
            string reportText;
            using (var writer = new StringWriter(CultureInfo.InvariantCulture))
            {
                using (IScopeReport scopeReport = CreateScopeReport(switchParser))
                {
                    scopeReport.CreateReport(writer, globalScope, switchParser.JSSettings.MinifyCode);
                }
                reportText = writer.ToString();
            }

            if (!string.IsNullOrEmpty(reportText))
            {
                if (string.IsNullOrEmpty(switchParser.ReportPath))
                {
                    // no report path specified; send to console
                    WriteProgress(reportText);
                    WriteProgress();
                }
                else
                {
                    // report path specified -- write to the file.
                    // don't append; use UTF-8 as the output format.
                    // let any exceptions bubble up.
                    using (var writer = new StreamWriter(switchParser.ReportPath, false, new UTF8Encoding(false)))
                    {
                        writer.Write(reportText);
                    }
                }
            }
        }

        private static IScopeReport CreateScopeReport(SwitchParser switchParser)
        {
            // check the switch parser for a report format.
            // At this time we only have two: XML or DEFAULT. If it's XML, use
            // the XML report; all other values use the default report.
            // No error checking at this time. 
            if (string.CompareOrdinal(switchParser.ReportFormat, "XML") == 0)
            {
                return new XmlScopeReport();
            }

            return new DefaultScopeReport();
        }

        #endregion

        #region Error-handling Members

        private void OnUndefinedReference(object sender, UndefinedReferenceEventArgs e)
        {
            var parser = sender as JSParser;
            if (parser != null)
            {
                parser.GlobalScope.AddUndefinedReference(e.Exception);
            }
        }

        #endregion
    }
}
