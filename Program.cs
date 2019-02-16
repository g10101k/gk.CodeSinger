/*
 *  "gk.CodeSigner", Utility to add license information to project files.
 *  Copyright (C) 2015-2019  Igor Tyulyakov aka g10101k, g101k. Contacts: <g101k@mail.ru>
 *  
 *  Licensed under the Apache License, Version 2.0 (the "License");
 *  you may not use this file except in compliance with the License.
 *  You may obtain a copy of the License at
 *
 *       http://www.apache.org/licenses/LICENSE-2.0
 *
 *   Unless required by applicable law or agreed to in writing, software
 *   distributed under the License is distributed on an "AS IS" BASIS,
 *   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *   See the License for the specific language governing permissions and
 *   limitations under the License.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using NDesk.Options;

namespace gk.CodeSinger
{
    class Program
    {
        private static string path = "";
        private static string mask = "";
        private static bool show_help;
        private static bool rec;
        static void Main(string[] args)
        {
            try
            {
                OptionSet p = new OptionSet() {
                { "p|path=", "Path to file or folder", v => path = v},
                { "m|mask=", "mask of file to sing", v => mask = v },
                { "r|rec", "recursive", v => rec = v != null },

                { "h|help", "Show this message and exit.", v => show_help = v != null },
            };
                p.Parse(args);
                if (show_help) { ShowHelp(p); return; }
                Run();
            }
            catch (OptionException e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                Console.WriteLine("Try 'gk.CodeSinger --help' for more information.");
                return;
            }

        }
        private static void ShowHelp(OptionSet p)
        {
            Console.WriteLine("Usage: gk.codesinger [OPTIONS]");
            Console.WriteLine("Sing your files");
            Console.WriteLine();
            Console.WriteLine("Options:");
            p.WriteOptionDescriptions(Console.Out);

        }
        private static void Run()
        {
            if (File.Exists(path))
            {
                try
                {
                    SingFile(path);
                }
                catch (Exception ex)
                { 
                    ErrorMessage("Run.File", ex);
                }
            }
            else
            {
                try
                {
                    DirectoryInfo dir = new DirectoryInfo(path);
                    DirectoryInfo[] dirs = new DirectoryInfo[0];
                    if (rec)
                        dirs = dir.GetDirectories("*", SearchOption.AllDirectories);

                    Array.Resize<DirectoryInfo>(ref dirs, dirs.Length + 1);
                    dirs[dirs.Length - 1] = dir;
                    foreach (DirectoryInfo item in dirs)
                    {
                        foreach (FileInfo f in item.GetFiles(mask.Replace("\"", "")))
                        {
                            SingFile(f.FullName);
                        }
                    }
                }
                catch (Exception ex)
                {
                    ErrorMessage("Run.Dir", ex);
                }
            }
        }



        public static void ErrorMessage(string fn, Exception ex)
        {
            Console.WriteLine(string.Format("{0}: ({1})\t{2}", DateTime.Now, fn, ex.Message));
            Console.WriteLine(string.Format("{0}: ({1})\t{2}", DateTime.Now, fn, ex.StackTrace));
        }

        public static void SingFile(string path)
        {
            try
            {
                string fileText = File.ReadAllText(path);

                //RegexOptions opt = RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant;
                string pattern = global::gk.CodeSinger.Properties.Settings.Default.SingPatternSearch;
                //Console.WriteLine((int)opt);
                RegexOptions opt = (RegexOptions)531;
                Match m = Regex.Match(fileText, pattern, opt);
                if (m.Success && m.Index == 0)
                {
                    fileText = fileText.Remove(m.Groups[0].Index, m.Groups[0].Length);
                }

                string singedCode = global::gk.CodeSinger.Properties.Settings.Default.SingTemplate + fileText;
                File.WriteAllText(path, singedCode);
            }
            catch (Exception ex)
            {
                ErrorMessage("SingFile", ex);
            }
        }
    }
}
