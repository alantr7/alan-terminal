using System;
using System.Collections.Generic;
using System.Net;

namespace Alan___Terminal {
    class Program {
        static void Main(string[] args) {
            if (args.Length == 0) {
                while (true) {
                    Console.Write("$ ");

                    string Input = "";
                    int[] Cursor = new int[] { Console.CursorLeft, Console.CursorTop };
                    while (Input.Length < 1) {
                        Console.SetCursorPosition(Cursor[0], Cursor[1]);
                        Input = Console.ReadLine();
                    }
                    Console.WriteLine("");

                    List<string> InputArgs = new List<string>();

                    string arg = "";
                    bool Quoted = false;
                    foreach (char c in Input) {
                        if (c == '"') {
                            Quoted = !Quoted;
                            continue;
                        }

                        if (c == ' ' && !Quoted) {
                            InputArgs.Add(arg);
                            arg = "";

                            continue;
                        }

                        arg += c;
                    }
                    if (arg.Length > 0) InputArgs.Add(arg);

                    Answer(InputArgs.ToArray());

                    Console.WriteLine("");
                }
            }
        }

        static string HtmlResponse(string Url, string post = null) {
            using (WebClient wc = new WebClient()) {
                if (post != null) {
                    wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                    return wc.UploadString(Url, post);
                }
                return wc.DownloadString(Url);
            }
            return "";
        }

        static string IPv4() {
            foreach (var ip in Dns.GetHostEntry(Dns.GetHostName()).AddressList) {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork) {
                    return ip.ToString();
                }
            }
            return "";
        }

        static string ArgValue(string[] Args, string Name) {
            foreach (string a in Args) {
                try {
                    if (!a.Contains(':')) continue;
                    string n = a.Split(':')[0];
                    string v = a.Split(':')[1];

                    if (n == Name) return v;
                } catch { }
            }
            return "";
        }

        static void Answer(string[] Args) {
            string Command = Args[0];
            switch (Command) {

                case "ip":
                    Console.WriteLine($"  PUBLIC IP: \t{HtmlResponse("https://ifconfig.me/ip")}\n  IPv4:\t\t{IPv4()}");
                    break;
                case "desktop-connect":
                    string mac = ArgValue(Args, "mac"), pass = ArgValue(Args, "pass");
                    Console.WriteLine($"  IP: " + HtmlResponse("http://alantr7.uwebweb.com/alan/request/device.php", $"device={mac}&password={pass}"));
                    break;
                default:
                    Console.WriteLine("  Nepoznata komanda. /help");
                    break;

            }
        }

    }
}
