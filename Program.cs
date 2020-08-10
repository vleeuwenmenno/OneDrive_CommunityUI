using WebWindows.Blazor;
using System;
using System.Drawing;
using System.Diagnostics;

namespace OneDrive_CommunityUI
{
    public class Program
    {        
        static string unix(string exec, string parameter, bool killSoon = false, string input = "")
        {
            Process proc = new Process {
                StartInfo = new ProcessStartInfo {
                    FileName = exec,
                    Arguments = $"{parameter}",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardInput = input != "",
                    CreateNoWindow = true
                }
            };

            proc.Start();
            string output = "";

            while (!proc.StandardOutput.EndOfStream) 
            {
                string line = proc.StandardOutput.ReadLine();
                output += line+Environment.NewLine;

                if (input != "")
                {
                    proc.StandardInput.WriteLine(input);
                    input = "";
                }

                if (killSoon)
                    proc.Kill();
            }
            return output;
        }

        static void Main(string[] args)
        {
            if (unix("onedrive", "--version").StartsWith("onedrive v2."))
                Console.WriteLine("OneDrive is installed!");
            else
                Console.WriteLine("ERROR: OneDrive cli is not installed or a incompatible version is installed, expecting v2.*.\n\tInstall with: sudo apt install onedrive");

            if (unix("kdocker", "-a").StartsWith("KDocker 5."))
                Console.WriteLine("KDocker is installed!");
            else
                Console.WriteLine("ERROR: KDocker is not installed or a incompatible version is installed.\n\tInstall with: sudo apt install kdocker");
                
            string s = unix("onedrive", "", true);
            if (s.StartsWith("Authorize this app visiting:"))
            {
                unix("xdg-open", s.Replace("Authorize this app visiting:\n\n", "").Replace("\n\nEnter the response uri: \n", ""));
                
                string res = unix("zenity", "--forms --title=\"Login to your Microsoft Account\" --text=\"Please login to your Microsoft account to continue.\\nOnce logged in you will be redirected to a blank page, copy the address url and paste it down below.\" --add-entry=\"Authentication URL\"");
                Console.WriteLine(unix("onedrive", "", false, res.Replace("\n", "")));
            }

            //ComponentsDesktop.Run<Startup>("OneDrive - Community UI", "wwwroot/index.html", new Size(400, 650));
        }
    }
}
