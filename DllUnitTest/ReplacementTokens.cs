﻿using System;
using System.Text;
using System.Collections.Generic;
using System.IO;

using Microsoft.Ajax.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DllUnitTest
{
    /// <summary>
    /// Summary description for ReplacementTokens
    /// </summary>
    [TestClass]
    public class ReplacementTokens
    {
        #region private fields

        private static string s_inputFolder;

        private static string s_outputFolder;

        private static string s_expectedFolder;

        #endregion

        #region Additional test attributes

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        // Use ClassInitialize to run code before running the first test in the class
        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext)
        {
            var testClassName = testContext.FullyQualifiedTestClassName.Substring(
                testContext.FullyQualifiedTestClassName.LastIndexOf('.') + 1);
            s_inputFolder = Path.Combine(testContext.DeploymentDirectory, "Dll", "Input", testClassName);
            s_outputFolder = Path.Combine(testContext.DeploymentDirectory, "Dll", "Output", testClassName);
            s_expectedFolder = Path.Combine(testContext.DeploymentDirectory, "Dll", "Expected", testClassName);

            // make sure the output folder exists
            Directory.CreateDirectory(s_outputFolder);
        }

        #endregion

        [TestMethod]
        public void ReplacementStringsJS()
        {
            // reuse the same parser object
            var parser = new JSParser();

            // default should leave tokens intact
            var settings = new CodeSettings();
            var source = "var a = 'He said, %MyToken:foo%'";
            var actual = Parse(parser, settings, source);
            Assert.AreEqual("var a=\"He said, %MyToken:foo%\"", actual);

            settings.ReplacementTokensApplyDefaults(new Dictionary<string, string> { 
                    { "mytoken", "\"Now he's done it!\"" },
                    { "numtoken", "123" },
                    { "myjson", "{\"a\": 1, \"b\": 2, \"c\": [ 1, 2, 3 ] }" },
                });
            settings.ReplacementFallbacks.Add("zero", "0");

            actual = Parse(parser, settings, source);
            Assert.AreEqual("var a='He said, \"Now he\\'s done it!\"'", actual);

            actual = Parse(parser, settings, "var b = '%NumToken%';");
            Assert.AreEqual("var b=\"123\"", actual);

            actual = Parse(parser, settings, "var c = '%MyJSON%';");
            Assert.AreEqual("var c='{\"a\": 1, \"b\": 2, \"c\": [ 1, 2, 3 ] }'", actual);
        }

        [TestMethod]
        public void ReplacementNodesJS()
        {
            // reuse the same parser object
            var parser = new JSParser();

            // default should leave tokens intact
            var settings = new CodeSettings();
            var source = "var a = %MyToken:foo%;";
            var actual = Parse(parser, settings, source);
            Assert.AreEqual("var a=%MyToken:foo%", actual);

            settings.ReplacementTokensApplyDefaults(new Dictionary<string, string> { 
                    { "mytoken", "\"Now he's done it!\"" },
                    { "numtoken", "123" },
                    { "myjson", "{\"a\": 1, \"b\": 2, \"c\": [ 1, 2, 3 ] }" },
                });
            settings.ReplacementFallbacks.Add("zero", "0");

            actual = Parse(parser, settings, source);
            Assert.AreEqual("var a=\"Now he's done it!\"", actual);

            actual = Parse(parser, settings, "var b = %NumToken%;");
            Assert.AreEqual("var b=123", actual);

            actual = Parse(parser, settings, "var c = %MyJSON%;");
            Assert.AreEqual("var c={\"a\":1,\"b\":2,\"c\":[1,2,3]}", actual);

            actual = Parse(parser, settings, "var d = '*%MissingToken:zero%*';");
            Assert.AreEqual("var d=\"*0*\"", actual);

            actual = Parse(parser, settings, "var e = '*%MissingToken:ack%*';");
            Assert.AreEqual("var e=\"**\"", actual);

            actual = Parse(parser, settings, "var f = '*%MissingToken:%*';");
            Assert.AreEqual("var f=\"**\"", actual);
        }

        [TestMethod]
        public void ReplacementFallbacksJS()
        {
            // reuse the same parser object
            var parser = new JSParser();

            // default should leave tokens intact
            var settings = new CodeSettings();

            settings.ReplacementTokensApplyDefaults(new Dictionary<string, string> { 
                    { "mytoken", "\"Now he's done it!\"" },
                    { "numtoken", "123" },
                    { "myjson", "{\"a\": 1, \"b\": 2, \"c\": [ 1, 2, 3 ] }" },
                });
            settings.ReplacementFallbacks.Add("zero", "0");

            var actual = Parse(parser, settings, "var a = %MissingToken:zero%;");
            Assert.AreEqual("var a=0", actual);

            actual = Parse(parser, settings, "var b = %MissingToken:ack% + 0;");
            Assert.AreEqual("var b=+0", actual);

            actual = Parse(parser, settings, "var c = %MissingToken:% + 0;");
            Assert.AreEqual("var c=+0", actual);

            actual = Parse(parser, settings, "var d = %MissingToken:%;debugger;throw 'why?';");
            Assert.AreEqual("var d=;throw\"why?\";", actual);
        }

        [TestMethod]
        public void ReplacementTokensCSS()
        {
            var source = ReadFile(s_inputFolder, "replacements.css");

            var settings = new CssSettings();
            settings.ReplacementTokensApplyDefaults(new Dictionary<string, string> { 
                    { "MediaQueries.SnapMax", "600px" },
                    { "bing-orange", "#930" },
                    { "MetroSdk.Resolution", "24x" },
                    { "Global.Right", "right" },
                    { "dim-gray", "#cccccc" },
                    { "theme_name", "green" },
                    { "Module-ID", "weather" },
                });
            settings.ReplacementFallbacks.Add("full", "100%");
            settings.ReplacementFallbacks.Add("1x", "1x");
            settings.ReplacementFallbacks.Add("color", "#ff0000");

            var minifier = new Minifier();
            var actual = minifier.MinifyStyleSheet(source, settings);

            var expected = ReadFile(s_expectedFolder, "replacements.css");
            Assert.AreEqual(expected, actual);
        }

        private string ReadFile(string folder, string fileName)
        {
            var inputPath = Path.Combine(folder, fileName);
            using (var reader = new StreamReader(inputPath))
            {
                return reader.ReadToEnd();
            }
        }

        private string Parse(JSParser parser, CodeSettings settings, string source)
        {
            var block = parser.Parse(source, settings);
            return OutputVisitor.Apply(block, settings);
        }
    }
}
