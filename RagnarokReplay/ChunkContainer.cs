using System.Collections.Generic;

namespace RagnarokReplay
{
    public enum ContainerType : short
    {
        None = 0,
        PacketStream = 1,
        ReplayData = 2,
        Session = 3,
        Status = 4,
        Quests = 6,
        GroupAndFriends = 7,
        Items = 8,
        UnknownContainingPet = 9,
        Unknown_10 = 10,
        Unknown_12 = 12,
        Unknown_13 = 13,
        InitialPackets = 14,
        Unknown_15 = 15,
        Unknown_16 = 16,
        Efst = 17,
        Unknown_18 = 18,
        Unknown_19 = 19,
        Unknown_20 = 20,
        Unknown_21 = 21,
        Unknown_22 = 22,
        Unknown_23 = 23,
        Unknown_24 = 24,
        LastContainerType
    }

    public class ChunkContainer
    {
        public ContainerType ContainerType { get; set; }
        public int Length { get; set; }
        public int Offset { get; set; }
        public List<Chunk> Data { get; set; }
    }
}
