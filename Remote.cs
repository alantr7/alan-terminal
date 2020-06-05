using forestpeas.WebSocketClient;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Alan___Terminal {
    class Remote {

        static string ip;
        public static async Task Connect(string _ip, string _mac) {
            ip = _ip;
            Program.Print("IP: " + _ip);
            using (var client = await WsClient.ConnectAsync(new Uri("ws://" + ip))) {
                Program.Print("Uspjesno povezan.\n");
                _ = Listen(client);
                while (true) {
                    Program.Print($"§a{Program.PUBLIC_IP}§8@§c{_mac} §f$ ", true, false);
                    string Input = Console.ReadLine();

                    string[] Arg = Program.LineToArgs(Input);
                    if (Arg[0] == "exit")
                        return;
                    if (Arg[0] == "alan-desktop") {
                        await client.SendStringAsync(Input.Substring(Arg[0].Length + 1));
                        return;
                    }
                    await client.SendStringAsync($"{{\"action\":\"terminal\",\"command\":\"{Input.Replace("\"", "\\\"")}\"}}");
                }
            }
        }

        private static async Task Listen(WsClient ws) {
            while (true) {
                string r = await ws.ReceiveStringAsync();
                //Program.Print(r);
            }
        }

    }
}
