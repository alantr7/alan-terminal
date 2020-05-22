using Alan;
using System;
using System.Collections.Generic;
using System.Text;

namespace Alan___Terminal.commands {
    abstract class Command {
        public abstract void Execute(string[] Args);

        public static void CreateCommands() {
            Program.commands["help"]            = new help();
            Program.commands["ip"]              = new ip();
            Program.commands["http"]            = new http();
            Program.commands["desktop-connect"] = new desktopconnect();
            Program.commands["exit"]            = new exit();
        }

    }

    class help : Command {
        public override void Execute(string[] Args) {
            Program.Print("help\t\tLista svih komandi");
            Program.Print("ip\t\tProvjeri svoje IP adrese");
            Program.Print("desktop-connect\tPovezi sa racunarom koristeci MAC adresu i sifru");
            Program.Print("http\t\tHTTP odgovor sa stranice");
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
    class exit : Command {
        public override void Execute(string[] Args) {
            Environment.Exit(0);
        }
    }

}
