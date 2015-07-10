using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Globalization;
using MyUtils;

namespace MyVideos
{
    public class Ffmpeg
    {
        
        static Process process = null;

        public static List<string> VIDEO_EXTENSIONS = new List<string>()
        {
            ".3g2",
            ".3gp",
            ".avi",
            ".dat",
            ".divx",
            ".flv",
            //".jpg",
            ".m4v",
            ".mkv",
            ".mov",
            ".mp2",
            ".m4a",
            ".mp4",
            ".mpe",
            ".mpeg",
            ".mpg",
            //".png",
            ".vob",
            ".webm",
            ".wma",
            ".wmv"
        };

        static void process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            //Console.WriteLine("Input line: {0} ({1:m:s:fff})", lineCount++, DateTime.Now);
            //Console.WriteLine(e.Data);
            //Console.WriteLine();
        }

        static void process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            //Console.WriteLine("Output Data Received.");
            //Console.WriteLine("odr => ");
            Console.WriteLine(e.Data);
            //Console.WriteLine();

        }

        static void process_Exited(object sender, EventArgs e)
        {
            process.Dispose();
            Console.WriteLine("Bye bye!");
        }

        public static string ExecuteProcess(string exe, string option,  int WaitTime=3000, bool createWindows = false)
        {
            try
            {
                process = new Process();
                string ffpath = "";
                if (exe.StartsWith("ff"))
                {
                    if (Environment.Is64BitOperatingSystem)
                    {
                        ffpath = "ffmpeg/64/";
                    }
                    else
                    {
                        ffpath = "ffmpeg/32/";
                    }
                }

                //ProcessStartInfo info = new ProcessStartInfo("ffmpeg/ffprobe.exe", "-v error -of flat=s=_ -select_streams v:0 -show_entries stream=height,width zzz.mp4");
                ProcessStartInfo info = new ProcessStartInfo(ffpath + exe + ".exe", option);

                if (! createWindows)
                {
                    info.CreateNoWindow = true;
                }
                info.UseShellExecute = false;
                //info.WorkingDirectory = ffpath;
                info.RedirectStandardError = true;
                info.RedirectStandardOutput = true;

                process.StartInfo = info;
                /*
                process.EnableRaisingEvents = true;
                process.ErrorDataReceived +=
                    new DataReceivedEventHandler(process_ErrorDataReceived);
                process.OutputDataReceived +=
                    new DataReceivedEventHandler(process_OutputDataReceived);
                process.Exited += new EventHandler(process_Exited);
                */

                DateTime debTime = DateTime.Now;

                process.Start();

                if (WaitTime > 0)
                {
                    // Wait for maw time WaitTime then force end process
                    process.WaitForExit(WaitTime);
                    DateTime endTime = DateTime.Now;
                    System.TimeSpan diff = endTime - debTime;
                    if (diff.TotalSeconds >= (WaitTime / 1000))
                    {
                        process.Dispose();
                        process.Kill();
                        //Console.WriteLine(ffpath + exe + ".exe " + option);
                    }
                }
                else
                {
                    //process.WaitForExit();
                }
                /*
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                */
                return process.StandardOutput.ReadToEnd();
            }
            catch
            {
                if (process != null) process.Dispose();
                return "";
            }
        }

        /// <summary>
        /// Test si un fichier est un vidéo
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool IsVideoFile(string path)
        {
            FileInfo f = new FileInfo(path);

            if (VIDEO_EXTENSIONS.Contains(f.Extension.ToString().ToLower()))
            {
                return true;
            }
            return false;
        }


        public static Hashtable getVideoInformation(string path)
        {
            Hashtable res = new Hashtable();
            if (File.Exists(path) && IsVideoFile(path))
            {
                path = "\"" + path + "\"";
                string detail = ExecuteProcess("ffprobe", "-v error -show_format -of flat=s=_ -show_entries stream=height,width " + path + "");
                List<string> aCmd = detail.RegSplitNoEmpty("\n");
                List<string> aLine;
                string key;
                foreach (string line in aCmd)
                {
                    aLine = line.RegSplitNoEmpty("=");
                    if (aLine.Count == 2)
                    {
                        key = aLine[0].Trim();
                        switch (key)
                        {
                            case "streams_stream_0_width":
                            case "streams_stream_1_width":
                            case "streams_stream_2_width":
                            case "streams_stream_3_width":
                            case "streams_stream_4_width":
                                key = "width";
                                break;
                            case "streams_stream_0_height":
                            case "streams_stream_1_height":
                            case "streams_stream_2_height":
                            case "streams_stream_3_height":
                            case "streams_stream_4_height":
                                key = "height";
                                break;
                            default:
                                break;
                        }
                        aLine[1] = aLine[1].RegReplace("\"", "").Trim().Replace("\r\n", "");
                        if (aLine[1].IsNumeric())
                        {
                            aLine[1] = aLine[1].Replace(".", ",");

                            int valI;
                            float valF;
                            if (int.TryParse(aLine[1], out valI))
                            {
                                res[key] = valI;
                            }
                            else if (float.TryParse(aLine[1], out valF))
                            {
                                res[key] = valF;
                            }
                            else
                            {
                                res[key] = aLine[1];
                            }
                        }else {
                            res[key] = aLine[1];
                        }
                    }
                }
            }
            return res;
        }

        public static Process MergeVideoFiles(List<string> files, string output)
        {
            List<string> sFile = new List<string>();
            foreach (string f in files)
            {
                sFile.Add("file '" + f + "'");
            }
            File.WriteAllText("merge", string.Join("\n", sFile));
            
            //string cmd = "/C ffmpeg\\64\\ffmpeg.exe -f concat -i merge -c copy output.mp4";
            //System.Diagnostics.Process.Start("cmd.exe", cmd);

            Process process = new Process();
            string ffpath;
            if (Environment.Is64BitOperatingSystem)
            {
                ffpath = "ffmpeg\\64\\ffmpeg.exe";
            }
            else
            {
                ffpath = "ffmpeg\\32\\ffmpeg.exe";
            }

            ProcessStartInfo info = new ProcessStartInfo();

            info.FileName = "cmd.exe";
            info.Arguments = "/C " + ffpath + " -f concat -i merge -c copy \"" + output + "\"";
            info.CreateNoWindow = false;
                
            //info.UseShellExecute = false;
            //info.WorkingDirectory = ffpath;
            //info.RedirectStandardError = true;
            //info.RedirectStandardOutput = true;

            process.StartInfo = info;

            process.EnableRaisingEvents = true;
            //process.Exited += new EventHandler(handler);
            //Process.Start(info);
            /*
            process.EnableRaisingEvents = true;
            process.ErrorDataReceived +=
                new DataReceivedEventHandler(process_ErrorDataReceived);
            process.OutputDataReceived +=
                new DataReceivedEventHandler(process_OutputDataReceived);
            process.Exited += new EventHandler(process_Exited);
            */

            process.Start();
            //process.WaitForExit();
            return process;
        }

        public static Process RotateVideoFile(string path, string output, int direction)
        {
            ProcessStartInfo info = new ProcessStartInfo();
            string ffpath;
            if (Environment.Is64BitOperatingSystem)
            {
                ffpath = "ffmpeg\\64\\ffmpeg.exe";
            }
            else
            {
                ffpath = "ffmpeg\\32\\ffmpeg.exe";
            }

            info.FileName = "cmd.exe";
            info.Arguments = "/C " + ffpath + " -i \"" + path + "\" -vf \"transpose=" + direction + "\" -qscale 0 -acodec copy \"" + output + "\"";
            info.CreateNoWindow = false;

            //info.UseShellExecute = false;
            //info.WorkingDirectory = ffpath;
            //info.RedirectStandardError = true;
            //info.RedirectStandardOutput = true;

            process.StartInfo = info;

            process.EnableRaisingEvents = true;
            process.Start();
            return process;
        }

        // Handle Exited event and display process information. 
        /*
        {

            Console.WriteLine("Exit time:    {0}\r\n");
        }
         * */
    }
}
