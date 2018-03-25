using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Processor
{
    public class DiffParser
    {

        private static SrcML srcml = SrcML.Initialize();
        private XNamespace position = "http://www.srcML.org/srcML/position", rootNs = "http://www.srcML.org/srcML/src", cpp = "http://www.srcML.org/srcML/cpp";

        /// <summary>
        /// Regex for parsing hunk block headers.
        /// Line change values can be accessed using the named groups.
        /// </summary>
        private const string hunkBlocks = @"^@@\s
            (?:
                (?:
                    \-(?<hunk_old_start>\d +)   # Start line no, original
                    ,
                    (?<hunk_old_count>\d +)     # Line count, original
                )
                |
                \-(?<file_deletion>\d+)             # Delete file
            )
            \s
            (?:
                (?:
                    \+(?<hunk_new_start>\d+)        # Start line no, new
                    ,
                    (?<hunk_new_count>\d+)          # Line count, new
                )
                |
                \+(?<file_creation>\d+)             # Create file
            )
        \s@@";

        /// <summary>
        ///  Split on line terminators (CRLF, CR, and LF) to convert a string into an array.
        /// </summary>
        /// <param name="splitThis">The string to be split.</param>
        /// <returns>A string array split by lines.</returns>
        public static string[] StringToLineArray(string splitThis)
        {
            return splitThis.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        }


        /// <summary>
        ///  Correlate the additions in a unified diff with a fully patched file.
        ///  Returns the terms for new line elements that are declarative statements.
        /// </summary>
        /// <param name="filename">The filename of fullFile.</param>
        /// <param name="fullFile">The fullFile read as a string.</param>
        /// <param name="unifiedDiff">The unifiedDiff read as a string.</param>
        /// <returns>A list of added terms.</returns>
        public List<string> FindTerms(string filename, string fullFile, string unifiedDiff)
        {
            // @TODO: Correlate object expressions with variable type (e.g. s1.hasToken() => StringTokenizer)

            string tempFile = Path.GetTempFileName();
            File.WriteAllText(tempFile, fullFile);
            XDocument parsedFile = srcml.GenerateSrcML(filename, tempFile);

            string[] diffLineArray = StringToLineArray(unifiedDiff);
            List<int> changedLineNumbers = ParseUnifiedDiff(diffLineArray);
            List<string> allTerms = new List<string>();

            IEnumerable<XElement> classNodes = parsedFile.Descendants(rootNs + "class").Elements(rootNs + "name");
            IEnumerable<XElement> declarativeNodes = parsedFile.Descendants(rootNs + "decl_stmt").Descendants(rootNs + "type").Descendants();
            IEnumerable<XElement> expressionNodes = parsedFile.Descendants(rootNs + "expr").Descendants(rootNs + "call").Elements(rootNs + "name");
            IEnumerable<XElement> importNodes = parsedFile.Descendants(rootNs + "import").Union(parsedFile.Descendants(rootNs + "using"));
            IEnumerable<XElement> cImportNodes = parsedFile.Descendants(cpp + "file");

            foreach (int line in changedLineNumbers)
            {
                IEnumerable<string> classTerms = classNodes
                                                .Where(element => element.Name.Equals(rootNs + "name") && element.Attribute(position + "line") != null && (int)element.Attribute(position + "line") == line)
                                                .Select(element => element.Value);

                IEnumerable<string> declarativeTerms = declarativeNodes
                                                .Where(element => element.Name.Equals(rootNs + "name") && element.Attribute(position + "line") != null && (int)element.Attribute(position + "line") == line)
                                                .Select(element => element.Value);

                IEnumerable<string> expressionTerms = expressionNodes
                                               .Where(element => element.Attribute(position + "line") != null && (int)element.Attribute(position + "line") == line)
                                               .Select(element => element.Value);

                IEnumerable<string> importTerms = importNodes
                                               .Where(element => element.Attribute(position + "line") != null && (int)element.Attribute(position + "line") == line && element.Element(rootNs + "name") != null)
                                               .Select(element => element.Element(rootNs + "name").Value);

                IEnumerable<string> cImportTerms = cImportNodes
                                               .Where(element => element.Attribute(position + "line") != null && (int)element.Attribute(position + "line") == line)
                                               .Select(element => element.Value);

                allTerms.AddRange(classTerms);
                allTerms.AddRange(declarativeTerms);
                allTerms.AddRange(expressionTerms);
                allTerms.AddRange(importTerms);
                allTerms.AddRange(cImportTerms);
            }

            File.Delete(tempFile);
            return allTerms;
        }

        /// <summary>
        ///  Parse a unified diff to find all line additions by number across all hunks.
        /// </summary>
        /// <param name="diffLineArray">The unified diff parsed by lines into an array.</param>
        /// <returns>A list of line additions for the unified diff.</returns>
        public List<int> ParseUnifiedDiff(string[] diffLineArray)
        {
            List<int> changedLines = new List<int>();
            int lineCount = diffLineArray.Length;

            // @TODO: Optimize to skip processed hunk lines
            for (int lineNumber = 0; lineNumber < lineCount; lineNumber++)
            {
                Match match = Regex.Match(diffLineArray[lineNumber], hunkBlocks, RegexOptions.IgnorePatternWhitespace);
                if (match.Success)
                {
                    List<int> changed = ProcessHunk(diffLineArray, Int32.Parse(match.Groups["hunk_old_count"].Value), Int32.Parse(match.Groups["hunk_new_count"].Value), Int32.Parse(match.Groups["hunk_new_start"].Value), lineNumber);
                    changedLines.AddRange(changed);
                }
            }

            return changedLines;
        }

        /// <summary>
        ///  Process a hunk in a unified diff to find line additions by number.
        ///  Reads the leading control character to determine how the line should be processed.
        /// </summary>
        /// <param name="diffLineArray">The unified diff parsed by lines into an array.</param>
        /// <param name="hunkOldCount">The old line count of the hunk.</param>
        /// <param name="hunkNewCount">The new line count of the hunk.</param>
        /// <param name="fileLine">The current file line of the associated hunk.</param>
        /// <param name="diffLine">The current unified diff line of the associated hunk.</param>
        /// <returns>A list of line additions for the hunk.</returns>
        private List<int> ProcessHunk(string[] diffLineArray, int hunkOldCount, int hunkNewCount, int fileLine, int diffLine)
        {
            List<int> newLines = new List<int>();
            int diffLineCount = diffLineArray.Length;
            int added = 0, removed = 0, unchanged = 0;
            diffLine++; // Skip over the hunk header

            for (int lineNumber = diffLine; lineNumber < diffLineCount; lineNumber++)
            {
                string controlCharacter = diffLineArray[lineNumber].Substring(0, 1);
                if (controlCharacter.Equals("-"))
                {
                    removed++;
                }
                if (controlCharacter.Equals("+"))
                {
                    newLines.Add(fileLine);
                    added++;
                }
                else
                {
                    unchanged++;
                }

                fileLine++;

                // Stop processing when we've seen the entire hunk
                if ((removed + unchanged == hunkOldCount) && (added + unchanged == hunkNewCount))
                {
                    break;
                }
            }

            return newLines;
        }
    }
}
