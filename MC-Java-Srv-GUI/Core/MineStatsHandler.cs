using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MineStatLib;

namespace MC_Java_Srv_GUI.Core
{
    public static class MineStatsHandler
    {
        //  Guide of List<string>:
        //      Int 0: Address:Port
        //      Int 1: Sever Status w/ Ping & Protocol
        //      ===== Proceed if online =====
        //      Int 2: Server Version
        //      Int 3: CurrentPlayers/MaximumPlayers
        //      Int 4: Motd

        public static bool Working = false;

        public static List<string> GetStats(string address = "localhost", Int32 port = 25565)
        {
            List<string> stats = new List<string>();
            MineStat ms = new MineStat(address, (ushort)port, (int)SlpProtocol.Json);
            stats.Add($"Address: {ms.Address}:{ms.Port}");
            if (ms.ServerUp)
            {
                stats.Add($"Status: Online | {ms.Latency}ms | {ms.Protocol}");
                stats.Add($"Version: {ms.Version}");
                stats.Add($"Players: {ms.CurrentPlayers}/{ms.MaximumPlayers}");
                if (ms.Gamemode != null)
                    stats.Add($"Game mode: {ms.Gamemode}");
                stats.Add($"MOTD: {Environment.NewLine}{ms.Stripped_Motd}");
            }
            else
                stats.Add("Status: Offline");
            // Return stats
            return stats;
        }
    }
}
