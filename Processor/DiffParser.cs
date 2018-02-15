using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Processor
{
    public class DiffParser
    {

        private static SrcML srcml = SrcML.Initialize();
        private XNamespace position = "http://www.srcML.org/srcML/position";

        /*
         * Regex for parsing hunk block headers.
         * Line change values can be accessed using the named groups.
         */
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

        /*
         * Split on line terminators to convert a string into an array.
         */
        private static string[] StringToLineArray(string diffBlock)
        {
            return diffBlock.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        }

        /*
         * Correlate the additions in a unified diff with a fully patched file.
         * Returns the values for all new line elements as a List<string> (duplicates included).
         * TODO: Filter to only include relevant nodes.
         */
        public List<string> FindTerms(string filename, string fullFile, string unifiedDiff)
        {
            XDocument parsedFile = srcml.GenerateSrcML(filename, fullFile);
            string[] diffLineArray = StringToLineArray(unifiedDiff);
            List<int> changedLineNumbers = ParseUnifiedDiff(diffLineArray);
            List<string> allTerms = new List<string>();

            foreach (int line in changedLineNumbers)
            {
                IEnumerable<string> lineTerms = parsedFile.Descendants()
                                            .Where(element => element.Attribute(position + "line") != null && (int)element.Attribute(position + "line") == line)
                                            .Select(element => element.Value);

                allTerms.AddRange(lineTerms);
            }

            return allTerms;
        }

        /*
         * Parse a unified diff to find all line additions by number across all hunks.
         */
        public List<int> ParseUnifiedDiff(string[] diffLineArray)
        {
            List<int> changedLines = new List<int>();
            int lineCount = diffLineArray.Length;

            // TODO: Optimize to skip processed hunk lines
            for (int lineNumber = 0; lineNumber < lineCount; lineNumber++)
            {
                Match match = Regex.Match(diffLineArray[lineNumber], hunkBlocks, RegexOptions.IgnorePatternWhitespace);
                if (match.Success)
                {
                    List<int> changed = ProcessHunk(diffLineArray, Int32.Parse(match.Groups["hunk_old_count"].Value), Int32.Parse(match.Groups["hunk_new_count"].Value), lineNumber);
                    changedLines.AddRange(changed);
                }
            }

            return changedLines;
        }

        /*
         * Process a hunk in a unified diff to find line additions by number.
         * Reads the leading control character to determine how the line should be processed.
         */
        private List<int> ProcessHunk(string[] diffLineArray, int hunkOldCount, int hunkNewCount, int currentLine)
        {
            List<int> newLines = new List<int>();
            int diffLineCount = diffLineArray.Length;
            int added = 0, removed = 0, unchanged = 0;
            currentLine++; // Skip over the hunk header

            for (int lineNumber = currentLine; lineNumber < diffLineCount; lineNumber++)
            {
                string controlCharacter = diffLineArray[lineNumber].Substring(0, 1);
                if (controlCharacter.Equals("-"))
                {
                    removed++;
                }
                if (controlCharacter.Equals("+"))
                {
                    newLines.Add(lineNumber);
                    added++;
                }
                else
                {
                    unchanged++;
                }

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
