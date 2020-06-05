using Alan;
using AlanEncoder;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Alan___Terminal.commands {
    abstract class Command {
        public abstract void Execute(string[] Args);

        public static void CreateCommands() {
            Program.commands["help"]            = new help();
            Program.commands["ip"]              = new ip();
            Program.commands["http"]            = new http();
            Program.commands["encoder"]         = new encoder();
            Program.commands["desktop-connect"] = new desktopconnect();
            Program.commands["app"]             = new app();
            Program.commands["ping"]            = new ping();
            Program.commands["clear"]           = new clear();
            Program.commands["exit"]            = new exit();
        }

    }

    class help : Command {
        public override void Execute(string[] Args) {
            Program.Print("help\t\tLista svih komandi");
            Program.Print("ip\t\tProvjeri svoje IP adrese");
            Program.Print("app\t\tPreuzmi i instaliraj aplikacije");
            Program.Print("encoder\t\tEnkriptuj ili dekriptuj string");
            Program.Print("desktop-connect\tPovezi sa racunarom koristeci MAC adresu i sifru");
            Program.Print("http\t\tHTTP odgovor sa stranice");
            Program.Print("ping\t\tProvjeri dostupnost stranice i vrijeme odgovora");
            Program.Print("clear\t\tVrati terminal u pocetno stanje");
            Program.Print("exit\t\tIzlaz iz terminala");
        }
    }
    class ip : Command {
        public override void Execute(string[] Args) {
            Program.Print($"PUBLIC IP: \t{Program.HtmlResponse("https://ifconfig.me/ip")}");
            Program.Print($"IPv4:\t\t{Program.IPv4()}");
        }
    }
    class http : Command {
        public override void Execute(string[] Args) {
            Program.Print($"Cekam na odgovor sa stranice...");
            string url = Program.ArgValue(Args, "url"), type = Program.ArgValue(Args, "type", "get");
            if (type == "post") Program.Print(Program.HtmlResponse(url, Program.ArgValue(Args, "params")));
            else Program.Print(Program.HtmlResponse(url));
        }
    }
    class encoder : Command {
        public override void Execute(string[] Args) {
            string key = Program.ArgValue(Args, "key", Encoder.Generate()), s = Program.ArgValue(Args, "string");
            switch (Args[1]) {
                case "encode":
                    Program.Print("Enkodirana vrijednost: " + new Encoder(key).Encode(s));
                    break;
                case "decode":
                    Program.Print("Dekodirana vrijednost: " + new Encoder(key).Decode(s));
                    break;
            }
        }
    }
    class desktopconnect : Command {
        public override void Execute(string[] Args) {
            Program.Print("Saljem podatke na stranicu...");

            string mac = Program.ArgValue(Args, "mac"), pass = Program.ArgValue(Args, "pass");
            Program.PUBLIC_IP = Program.HtmlResponse("https://ifconfig.me/ip");
            JSONElement json = JSON.Parse(Program.HtmlResponse("http://alantr7.uwebweb.com/alan/request/device.php", $"device={mac}&password={pass}"));

            if (json.c["status"].ToString() == "success") {
                string ip = json.c["ip"].ToString();

                Program.Print("IP preuzet. Povezivanje...");
                Remote.Connect(ip, mac).Wait();

            }
            else Program.Print($"Greska: " + json.c["error"].ToString());
        }
    }
    class app : Command {
        public override void Execute(string[] Args) {
            
            if (Args.Length == 1) {
                Program.Print("list");
                Program.Print("list-update");
                Program.Print("install");
                Program.Print("installed");  
                Program.Print("scan");  
                Program.Print("start");
                Program.Print("delete");
                return;
            }

            switch (Args[1]) {
                case "list-update":
                    Program.Print("Preuzimam linkove...");
                    using (WebClient wc = new WebClient()) {

                        wc.DownloadFile("http://alantr7.uwebweb.com/alan/terminal/app-list.txt?_=" + DateTimeOffset.Now.ToUnixTimeMilliseconds(), Program.DIRECTORY + "app-list.txt");

                    }
                    Program.Print("Linkovi uspjesno preuzeti.");
                    return;
                case "list": {
                        string[] lines = File.ReadAllLines(Program.DIRECTORY + "app-list.txt");
                        List<string> installed = new List<string>();

                        foreach (string l in File.ReadAllLines(Program.DIRECTORY + "app-installed.txt")) {
                            JSONElement json = JSON.Parse(l);
                            installed.Add(json.c["keyword"].ToString());
                        }

                        Program.Print("\t§8NAZIV\t\t\tNAZIV PROGRAMA\t\tBIN\n", false);
                        foreach (string line in lines) {
                            JSONElement e = JSON.Parse(line);
                            string output = $"\t{e.c["keyword"].ToString()}";

                            string tabs;
                            if (e.c["keyword"].ToString().Length < 8)
                                tabs = "\t\t\t";
                            else tabs = "\t\t";

                            output += tabs + e.c["name"].ToString();

                            if (e.c["name"].ToString().Length < 8)
                                tabs = "\t\t\t";
                            else tabs = "\t\t";

                            output += tabs + e.c["bin"];

                            if (e.c["bin"].ToString().Length < 8)
                                tabs = "\t\t\t";
                            else if (e.c["bin"].ToString().Length < 16)
                                tabs = "\t\t";
                            else tabs = "\t";

                            output += tabs;

                            Console.Write($"{output}");
                            if (installed.Contains(e.c["keyword"].ToString()))
                                Program.Print("§aInstalirano§f", false, false);
                            Console.WriteLine();
                        }
                        return;
                    }
                case "install":
                    using (WebClient wc = new WebClient()) {

                        string[] lines = File.ReadAllLines(Program.DIRECTORY + "app-list.txt");
                        foreach (string line in lines) {
                            JSONElement e = JSON.Parse(line);
                            if (e.c["keyword"].ToString().Equals(Args[2].ToLower())) {

                                Stream stream = wc.OpenRead(e.c["url"].ToString());
                                int filesize = Convert.ToInt32(wc.ResponseHeaders["Content-Length"]);
                                stream.Dispose();

                                wc.DownloadFileAsync(new Uri(e.c["url"].ToString()), Program.DIRECTORY + "downloads\\" + e.c["bin"].ToString());
                                bool done = false;

                                Program.Print($"Ukupna velicina: {filesize / (1024 * 1024)}.{("" + filesize % (1024 * 1024)).Substring(0, 2)}MB");
                                int[] cp = Program.GetCursorPosition();

                                wc.DownloadFileCompleted += (o, s) => done = true;

                                while (!done) {
                                    Program.SetCursorPosition(cp);
                                    Program.Print($"Preuzeto: {(int)(new FileInfo(Program.DIRECTORY + "downloads\\" + e.c["bin"].ToString()).Length / (double)filesize * 100)}%");
                                    Thread.Sleep(500);
                                };

                                Console.WriteLine();
                                Program.Print("Cekam da se instalacija zavrsi...");

                                Process.Start(new ProcessStartInfo() {
                                    FileName = Program.DIRECTORY + "downloads\\" + e.c["bin"].ToString()
                                }).WaitForExit();

                                Program.Print("Brisem setup");
                                Thread.Sleep(2000);
                                File.Delete(Program.DIRECTORY + "downloads\\" + e.c["bin"].ToString());

                                Program.Print("Instalacija zavrsena");

                                break;
                            }
                        }
                        
                    }
                    return;
                case "scan": {

                        int searchlevel = Int32.Parse(Program.ArgValue(Args, "level", "3"));

                        List<string> directories = new List<string>();
                        foreach (DriveInfo di in DriveInfo.GetDrives()) {
                            if (di.IsReady) directories.Add(di.Name);
                        }
                        directories.Add(Environment.GetEnvironmentVariable("APPDATA"));
                        directories.Add(Environment.GetEnvironmentVariable("LOCALAPPDATA"));
                        directories.Add("C:\\Program Files (x86)\\Google\\Chrome\\Application");

                        Dictionary<string, string> apps = new Dictionary<string, string>();
                        foreach (string line in File.ReadAllLines(Program.DIRECTORY + "app-list.txt")) {
                            JSONElement json = JSON.Parse(line);
                            apps.Add(json.c["keyword"].ToString(), json.c["bin"].ToString().ToLower());
                        }

                        int[] cp = Program.GetCursorPosition();
                        List<JSONElement> Found = new List<JSONElement>();

                        void SearchDirectory(string dir, int level) {
                            if (level > searchlevel) return;

                            if (level == 1) {
                                Program.EmptyLine(cp);
                                Program.SetCursorPosition(cp);
                                Program.Print($"§7Provjeravam direktorij §8{dir}");
                            }

                            try {
                                foreach (string s in Directory.GetFiles(dir)) {

                                    if (Directory.Exists(s)) {
                                        SearchDirectory(s, level);
                                    }

                                    string[] split = s.Split('\\');
                                    string name = split[split.Length - 1].ToLower();

                                    foreach (string app in apps.Keys)
                                        if (name.Equals(apps[app])) {
                                            JSONElement appf = new JSONElement();
                                            
                                            JSONElement appf_n = new JSONElement();
                                            appf_n.v = app;
                                            JSONElement appf_p = new JSONElement();
                                            appf_p.v = s;
                                            JSONElement appf_b = new JSONElement();
                                            appf_b.v = apps[app];

                                            appf.c.Add("keyword", appf_n);
                                            appf.c.Add("path", appf_p);
                                            appf.c.Add("bin", appf_b);
                                            
                                            Found.Add(appf);
                                            break;
                                        }
                                }
                                int newlevel = level + 1;
                                foreach (string s in Directory.GetDirectories(dir)) {
                                    SearchDirectory(s, newlevel);
                                }
                            }
                            catch { }
                        }
                        foreach (string dir in directories) {
                            SearchDirectory(dir, 1);
                        }

                        Program.EmptyLine(cp);
                        Program.SetCursorPosition(cp);
                        Program.Print($"Pronadjeno §a{Found.Count} §finstaliranih aplikacija");

                        File.Delete(Program.DIRECTORY + "app-installed.txt");
                        string fline = "";
                        foreach (JSONElement el in Found) {
                            fline += $"{JSON.Stringify(el)}\n";
                        }
                        if (fline.EndsWith(",")) fline = fline.Substring(0, fline.Length - 1);

                        File.WriteAllText(Program.DIRECTORY + "app-installed.txt", fline);

                        return;
                    }
                case "start": {

                        string name = Args[2];
                        

                        foreach (string l in File.ReadAllLines(Program.DIRECTORY + "app-installed.txt")) {
                            JSONElement json = JSON.Parse(l);
                            if (json.c["keyword"].ToString().ToLower().Equals(name.ToLower())) {
                                Program.Print($"Aplikacija §7{name} §fpokrenuta");
                                Process.Start(new ProcessStartInfo() {
                                    FileName = json.c["path"].ToString()
                                });
                                return;
                            }
                        }

                        Program.Print($"Aplikacija §c{name} §fnije pronadjena");

                        break;
                    }
            }

        }
    }
    class ping : Command {
        public override void Execute(string[] Args) {

            string ip = Program.ArgValue(Args, "ip"); int port = Int32.Parse(Program.ArgValue(Args, "port", "80")), times = Int32.Parse(Program.ArgValue(Args, "times", "1"));

            for (int i = 0; i < times; i++) {
                Socket sock = new Socket(SocketType.Stream, ProtocolType.Tcp);
                string numip = Dns.GetHostEntry(ip).AddressList[0].ToString();

                long s = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                sock.Connect(IPAddress.Parse(numip), port);

                long elapsed = DateTimeOffset.Now.ToUnixTimeMilliseconds() - s;

                Program.Print($"§c{numip} §7-> §c{elapsed}ms");

                sock.Dispose();
            }
        }
    }
    class clear : Command {
        public override void Execute(string[] Args) {
            Console.Clear();
            Console.WriteLine("$ help\n");
            Program.commands["help"].Execute(new string[] { });
        }
    }
    class exit : Command {
        public override void Execute(string[] Args) {
            Environment.Exit(0);
        }
    }

}
