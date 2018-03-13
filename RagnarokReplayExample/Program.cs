using RagnarokReplay;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RagnarokReplayExample
{
    class Program
    {
        static void Main(string[] args)
        {
            var replay = new Replay();
            replay.LoadFile(Path.Combine("Replay", "woe1103-1.rrf"));

            foreach (var chunk in replay.ChunkContainers)
            {
                switch (chunk.ContainerType)
                {
                    case ContainerType.PacketStream: // packets
                        // chunk.Data contains the packet data for each packet
                        foreach (var packet in chunk.Data)
                        {
                            switch (packet.Header)
                            {
                                default:
                                    if (!Enum.IsDefined(typeof(HEADER), packet.Header))
                                    {
                                        Console.WriteLine($"[+{ConvertMsToTime(packet.Time)}] Unknown packet {packet.Header}");
                                    }
                                    else
                                    {
                                        //Console.WriteLine($"[+{ConvertMsToTime(packet.Time)}] packet {(HEADER)packet.Header}");
                                    }
                                    break;
                            }
                        }
                        break;

                    case ContainerType.InitialPackets: // packets
                        foreach (var packet in chunk.Data)
                        {
                            switch (packet.Id)
                            {
                                default:
                                    if (!Enum.IsDefined(typeof(ReplayOpCodes), (short)packet.Id))
                                    {
                                        Console.WriteLine($"Unknown initial packet: {packet.Id}");
                                    }
                                    else
                                    {
                                        Console.WriteLine($"Unparsed initial packet: {(ReplayOpCodes)packet.Id}");
                                    }
                                    break;
                            }
                        }
                        break;

                    // variables
                    case ContainerType.ReplayData:
                    case ContainerType.Session:
                    case ContainerType.Status:
                    case ContainerType.Quests:
                    case ContainerType.GroupAndFriends:
                    case ContainerType.Items:
                    case ContainerType.UnknownContainingPet:
                    case ContainerType.Efst:
                        Console.WriteLine($"ContainerType {chunk.ContainerType}");
                        foreach (var entry in chunk.Data)
                        {
                            switch (entry.Id)
                            {
                                default:
                                    if (!Enum.IsDefined(typeof(ReplayOpCodes), (short)entry.Id))
                                    {
                                        Console.WriteLine($"Unknown opcode {entry.Id}");
                                        Console.WriteLine(entry.Data.Hexdump());
                                    }
                                    else
                                    {
                                        Console.WriteLine($"[Chunk {chunk.ContainerType}] Unparsed opcode {(ReplayOpCodes)entry.Id}, Length={entry.Length}");
                                    }
                                    break;
                            }
                        }

                        Console.WriteLine();
                        break;

                    default: // variables - duplicated to check what containerType this could be
                        Console.WriteLine($"Unhandled container type {chunk.ContainerType}");
                        foreach (var entry in chunk.Data)
                        {
                            switch (entry.Id)
                            {
                                default:
                                    if (!Enum.IsDefined(typeof(ReplayOpCodes), (short)entry.Id))
                                    {
                                        Console.WriteLine($"Unknown opcode {entry.Id}");
                                        Console.WriteLine(entry.Data.Hexdump());
                                    }
                                    else
                                    {
                                        Console.WriteLine($"[Chunk {chunk.ContainerType}] Unparsed opcode {(ReplayOpCodes)entry.Id}, Length={entry.Length}");
                                    }
                                    break;
                            }
                        }

                        Console.WriteLine();
                        break;
                }
            }

            //Console.WriteLine("Done");
            //Console.ReadKey();
        }

        private static string ConvertMsToTime(int ms)
        {
            var uSec = ms % 1000;
            ms = ms / 1000;

            var seconds = ms % 60;
            ms = ms / 60;

            var minutes = ms % 60;
            ms = ms / 60;

            var hours = ms % 60;
            return string.Format("{0:00}:{1:00}:{2:00}:{3:000}", hours, minutes, seconds, uSec);
        }
    }
}
