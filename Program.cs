using Alan;
using Alan___Terminal.commands;
using AlanEncoder;
using forestpeas.WebSocketClient;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Alan___Terminal {
    class Program {

        public static string PUBLIC_IP, DIRECTORY = Environment.GetEnvironmentVariable("APPDATA") + "\\Alan\\terminal\\";

        public static Dictionary<string, Command> commands = new Dictionary<string, Command>();

        static void Main(string[] args) {

            Command.CreateCommands();

            if (args.Length == 0) {
                Console.WriteLine("$ help\n");
                Answer(new string[] { "help" });
                Console.WriteLine();
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

        public static int[] GetCursorPosition() {
            return new int[] { Console.CursorLeft, Console.CursorTop };
        }
        public static void SetCursorPosition(int[] pos) {
            Console.SetCursorPosition(pos[0], pos[1]);
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
            for (int i = 0; i < Line.Length; i++) {
                char c = Line[i];
                if ((c == '"' && i == 0) || (c == '"' && i > 0 && Line[i - 1] != '\\')) {
                    Quoted = !Quoted;
                    continue;
                }

                if (c == ' ' && !Quoted) {
                    InputArgs.Add(arg);
                    arg = "";

                    continue;
                }

                if (c == '\\' && i > 0 && Line[i - 1] == '\\') {
                    arg += c;
                    Console.WriteLine("YEAH! c = " + c + " and " + Line[i - 1] + " = \\");
                }
                else if (c != '\\') arg += c;
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
            } catch (Exception e) {
                Print("Doslo je do greske prilikom izvrsavanja komande");
                Print(e.Message);
            }
        }

        public static void Print(string Line = "", bool printtime = true, bool newline = true) {
            if (Line.Length == 0) {
                Console.WriteLine();
                return;
            }
            if (printtime)
                Line = $"  §8[ {DateTime.Now.ToString("hh:mm:ss")} ] §f{Line}";

            for (int i = 0; i < Line.Length; i++) {
                if (Line[i] == '§') {
                    if (i + 1 >= Line.Length) break;
                    char colorChar = Line[i + 1];

                    if (Char.IsDigit(colorChar))
                        Console.ForegroundColor = (ConsoleColor)Int32.Parse(colorChar + "");
                    else Console.ForegroundColor = (ConsoleColor)(byte)colorChar - 87;
                    i += 1;
                }
                else Console.Write(Line[i]);
            };
            if (newline) Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.White;
        }
        public static void EmptyLine(int[] cp) {
            SetCursorPosition(cp);
            string line = "";
            for (int i = 0; i < 200; i++) line += " ";
            Console.WriteLine(line);
        }
    }
}
