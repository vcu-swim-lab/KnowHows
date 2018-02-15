using System.Linq;
using System.IO;
using System.Diagnostics;
using System.Xml.Linq;
using System.Collections.ObjectModel;
using System;
using System.ComponentModel;

namespace Processor
{
    public class SrcML
    {

        private static string srcMLExecutable = "srcml";

        public SrcML()
        {
        }

        public SrcML(string customSrcMLPath)
        {
            srcMLExecutable = customSrcMLPath;
        }

        /*
         * Initialize the Tokenizer class.
         * Will look for SrcML (srcml) on the environment PATH.
         */
        public static SrcML Initialize()
        {
            if (!ExecutableExistsOnPath(srcMLExecutable)) return null;
            return new SrcML();
        }

        /*
         * Initialize the Tokenizer class.
         * Specify a custom path for the SrcML executable.
         */
        public static SrcML Initialize(string customSrcMLPath)
        {
            if (!ExecutableExistsOnPath(customSrcMLPath)) return null;
            return new SrcML(customSrcMLPath);
        }

        /*
         * Check if the current execution platform is Unix.
         */
        private static bool IsUnix
        {
            get
            {
                int p = (int)Environment.OSVersion.Platform;
                return (p == 4) || (p == 6) || (p == 128);
            }
        }

        /*
         * Checks for the specified executable on the PATH using where/whereis.
         */
        private static bool ExecutableExistsOnPath(string filename)
        {
            using (Process process = new Process())
            {
                try
                {
                    process.StartInfo.FileName = IsUnix ? "whereis" : "where";
                    process.StartInfo.Arguments = filename;
                    process.StartInfo.CreateNoWindow = true;
                    process.StartInfo.UseShellExecute = false;

                    process.Start();
                    process.WaitForExit();
                    return process.ExitCode == 0;
                }

                catch (Win32Exception e)
                {
                    throw new Exception(String.Format("Failed to check for executable on PATH: {0}", e.ToString()));
                }
            }
        }

        /*
         * Build the argument string for the command line.
         */
        private string BuildArgumentString(Collection<string> arguments)
        {
            return String.Join(" ", arguments.ToArray());
        }

        /*
         * Calls srcML from the PATH with the specified arguments.
         * Returns an XDocument with the received string.
         */
        private XDocument SrcMLRunner(string arguments)
        {
            string output;
            using (Process process = new Process())
            {
                try
                {
                    process.StartInfo.FileName = srcMLExecutable;
                    process.StartInfo.Arguments = arguments;
                    process.StartInfo.CreateNoWindow = true;
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;

                    process.Start();
                    output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();
                }

                catch (Exception e)
                {
                    throw new Exception(String.Format("Failed to process the file/directoryt with SrcML: {0}", e.ToString()));
                }
            }

            return XDocument.Parse(output);
        }

        /*
         * Determine the language from the file extension.
         * Taken from https://github.com/github/linguist/blob/master/lib/linguist/languages.yml.
         * Returns null on languages that cannot be parsed by srcML.
         */
        public static string GetLanguage(string filename)
        {
            var extension = Path.GetExtension(filename);
            switch (extension)
            {
                case ".c":
                case ".cats":
                case ".idc":
                    return "C";
                case ".cpp":
                case ".c++":
                case ".cc":
                case ".cp":
                case ".cxx":
                case ".h":
                case ".h++":
                case ".hh":
                case ".hpp":
                case ".hxx":
                case ".inc":
                case ".inl":
                case ".ino":
                case ".ipp":
                case ".re":
                case ".tcc":
                case ".tpp":
                    return "C++";
                case ".java":
                    return "Java";
                case ".aj":
                    return "AspectJ";
                case ".cs":
                case ".cake":
                case ".cshtml":
                case ".csx":
                    return "C#";
                default:
                    return "";
            }
        }

        /*
         * Generate srcML document for raw input. Requires filename to determine language.
         * Works on snippets, e.g. diffs or full files.
         * Returns null when SrcML does not run.
         */
        public XDocument GenerateSrcML(string filename, string input)
        {
            var arguments = new Collection<string>();
            arguments.Add(string.Format("--text \"{0}\"", input));

            var language = GetLanguage(filename);
            if (string.IsNullOrEmpty(language)) return null;
            arguments.Add(string.Format("--language \"{0}\"", language));

            arguments.Add("--position");

            return SrcMLRunner(BuildArgumentString(arguments));
        }

        /*
         * Generate srcML document from directory path containing files.
         * For use parsing full repos.
         * Returns null when SrcML does not run.
         */
        public XDocument GenerateSrcML(string directory)
        {
            var arguments = new Collection<string>();
            arguments.Add(directory);

            return SrcMLRunner(BuildArgumentString(arguments));
        }
    }
}
