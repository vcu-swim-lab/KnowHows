using System.Linq;
using System.IO;
using System.Diagnostics;
using System.Xml.Linq;
using System.Collections.ObjectModel;
using System;
using System.ComponentModel;
using System.Collections.Generic;

namespace Processor
{
    public class SrcML
    {
        private static string srcMlExecutable = "srcml";

        public static Dictionary<string, string> supportedExtensions = new Dictionary<string, string>
        {
            { ".c", "C" },
            { ".cs", "C#" },
            { ".cpp", "C++" },
            { ".java", "Java" }
        };

        private SrcML()
        {
        }

        private SrcML(string customSrcMLPath)
        {
            srcMlExecutable = customSrcMLPath;
        }

        /// <summary>
        ///  Initialize the SrcML class with the default srcML executable name.
        /// </summary>
        public static SrcML Initialize()
        {
            if (!ExecutableExistsOnPath(srcMlExecutable)) return null;
            return new SrcML();
        }

        /// <summary>
        ///  Initialize the SrcML class with a custom path for the srcML executable.
        /// </summary>
        public static SrcML Initialize(string customSrcMlPath)
        {
            if (!ExecutableExistsOnPath(customSrcMlPath)) return SrcML.Initialize();
            return new SrcML(customSrcMlPath);
        }

        /// <summary>
        ///  Check if the current execution platform is Unix.
        /// </summary>
        /// <returns>True if the platform is Unix-based.</returns>
        private static bool IsUnix
        {
            get
            {
                int p = (int)Environment.OSVersion.Platform;
                return (p == 4) || (p == 6) || (p == 128);
            }
        }

        /// <summary>
        ///  Check for the specified executable on the PATH using where/whereis.
        /// </summary>
        /// <param name="filename">The executable name to be checked.</params>
        /// <returns>True if the executable is on the PATH.</returns>
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

        /// <summary>
        ///  Build the argument string for the command line.
        /// </summary>
        /// <param name="arguments">The command line arguments.</param>
        /// <returns>A string separated by whitespace.</returns>
        private string BuildArgumentString(Collection<string> arguments)
        {
            return String.Join(" ", arguments.ToArray());
        }

        /// <summary>
        ///  Calls srcML from the PATH with the specified arguments.
        /// </summary>
        /// <param name="arguments">The command line arguments.</param>
        /// <returns>The srcML parsed document.</returns>
        private XDocument SrcMLRunner(string arguments)
        {
            string output;
            using (Process process = new Process())
            {
                try
                {
                    process.StartInfo.FileName = srcMlExecutable;
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
                    throw new Exception(String.Format("Failed to process the file/directoryt with srcML: {0}", e.ToString()));
                }
            }

            if (String.IsNullOrEmpty(output)) return new XDocument();
            return XDocument.Parse(output);
        }

        /// <summary>
        ///  Generate a srcML document for a file.
        ///  Returns an empty document when SrcML does not run.
        /// </summary>
        /// <param name="filename">The original source filename.</param>
        /// <param name="file">>The full path of the file to be parsed.</param>
        /// <returns>The srcML parsed document.</returns>
        public XDocument GenerateSrcML(string filename, string file)
        {
            var arguments = new Collection<string>();

            string fileExtension = Path.GetExtension(filename);
            if (!supportedExtensions.ContainsKey(fileExtension)) return new XDocument();
            string language = supportedExtensions[fileExtension];
            arguments.Add(string.Format("--language \"{0}\"", language));

            arguments.Add("--position");
            arguments.Add(file);

            return SrcMLRunner(BuildArgumentString(arguments));
        }

        /// <summary>
        ///  Generate a srcML document from a directory path containing files.
        ///  Returns null when SrcML does not run.
        /// </summary>
        /// <param name="directory">The full path to the directory.</param>
        /// <returns>The srcML parsed document.</returns>
        public XDocument GenerateSrcML(string directory)
        {
            var arguments = new Collection<string>();
            arguments.Add(directory);

            return SrcMLRunner(BuildArgumentString(arguments));
        }
    }
}
