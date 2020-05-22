using Alan;
using Alan___Terminal.commands;
using forestpeas.WebSocketClient;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Alan___Terminal {
    class Program {

        public static string PUBLIC_IP;

        public static Dictionary<string, Command> commands = new Dictionary<string, Command>();

        static void Main(string[] args) {

            Command.CreateCommands();

            if (args.Length == 0) {
                while (true) {
                    Console.Write("$ ");

                    string Input = "";
                    int[] Cursor = new int[] { Console.CursorLeft, Console.CursorTop };
                    while (Input.Length < 1) {
                        Console.SetCursorPosition(Cursor[0], Cursor[1]);
                        Input = Console.ReadLine();
                    }
                    Print("");

                    Answer(LineToArgs(Input));
                    Print("");
                }
            }
        }

        public static string HtmlResponse(string Url, string post = null) {
            using (WebClient wc = new WebClient()) {
                if (post != null) {
                    wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                    return wc.UploadString(Url, post);
                }
                return wc.DownloadString(Url);
            }
        }

        public static string IPv4() {
            foreach (var ip in Dns.GetHostEntry(Dns.GetHostName()).AddressList) {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork) {
                    return ip.ToString();
                }
            }
            return "";
        }

        public static string[] LineToArgs(string Line) {
            List<string> InputArgs = new List<string>();

            string arg = "";
            bool Quoted = false;
            foreach (char c in Line) {
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

            return InputArgs.ToArray();
        }

        public static string ArgValue(string[] Args, string Name, string Default = "") {
            foreach (string a in Args) {
                try {
                    if (!a.Contains(':')) continue;
                    string n = a.Split(':')[0];
                    string v = a.Substring(n.Length + 1);

                    if (n == Name) return v;
                } catch { }
            }
            return Default;
        }

        static void Answer(string[] Args) {
            try {
                string Command = Args[0];
                foreach (string k in commands.Keys) {
                    if (k == Command.ToLower()) {
                        commands[k].Execute(Args);
                        return;
                    }
                }
                Print("Nepoznata komanda > help");
            } catch {
                Print("Doslo je do greske prilikom izvrsavanja komande");
            }
        }

        public static void Print(string Line = "") {
            if (Line.Length == 0) {
                Console.WriteLine();
                return;
            }
            Console.WriteLine("  [ " + DateTime.Now.ToString("hh:mm:ss") + " ] " + Line);
        }

    }
}
