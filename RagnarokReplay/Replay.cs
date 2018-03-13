using System;
using System.Collections.Generic;
using System.IO;

namespace RagnarokReplay
{
    public class Replay
    {
        #region Properties
        public byte[] Header { get; set; }
        public byte Version { get; set; }
        public byte[] Sig { get; set; }
        public DateTime Date { get; set; }
        public Region Region { get; set; }
        public long Size { get; set; }
        public List<ChunkContainer> ChunkContainers { get; set; }
        #endregion

        #region Public methods
        public void LoadFile(string filename)
        {
            if (!File.Exists(filename))
            {
                throw new FileNotFoundException();
            }

            using (var fs = new FileStream(filename, FileMode.Open))
            {
                using (var br = new BinaryReader(fs))
                {
                    ParseHeader(br);
                    switch (Version)
                    {
                        case 5:
                            LoadReplayV5(br, br.BaseStream.Length);
                            break;
                    }
                }
            }
        }
        #endregion

        #region Private methods
        private void ParseHeader(BinaryReader br)
        {
            Header = br.ReadBytes(100);
            Version = br.ReadByte();
            Sig = br.ReadBytes(3);
            var year = br.ReadInt16();
            var month = br.ReadByte();
            var day = br.ReadByte();
            var unused = br.ReadByte();
            var hour = br.ReadByte();
            var minute = br.ReadByte();
            var second = br.ReadByte();
            Date = new DateTime(year, month, day, hour, minute, second);
        }

        private void LoadReplayV5(BinaryReader br, long filesize)
        {
            Size = filesize;
            ChunkContainers = new List<ChunkContainer>();

            for (var i = 0; i < 24; i++)
            {
                var chunk = new ChunkContainer();
                chunk.Data = new List<Chunk>();
                chunk.ContainerType = (ContainerType)br.ReadUInt16();
                chunk.Length = br.ReadInt32();
                chunk.Offset = br.ReadInt32();

                if (chunk.ContainerType >= ContainerType.LastContainerType)
                    continue;

                if (chunk.Offset == 0 && chunk.ContainerType == ContainerType.None)
                    continue;

                ChunkContainers.Add(chunk);

                if (chunk.Length == 0)
                {
                    chunk.Length = (int)filesize - chunk.Offset;
                }

                var lastOffset = br.BaseStream.Position;
                br.BaseStream.Seek(chunk.Offset, SeekOrigin.Begin);
                var content = br.ReadBytes(chunk.Length);

                if (chunk.ContainerType == ContainerType.PacketStream)
                {
                    var ptr = 0;
                    using (var mschunk = new MemoryStream(content))
                    {
                        using (var brchunk = new BinaryReader(mschunk))
                        {
                            while (ptr < chunk.Length)
                            {
                                var packet = new Chunk();
                                packet.Id = brchunk.ReadInt32();
                                packet.Time = brchunk.ReadInt32();
                                packet.Length = brchunk.ReadUInt16();
                                packet.Data = brchunk.ReadBytes(packet.Length);
                                packet.Data = Crypt(packet.Length, packet.Data);
                                packet.Header = (ushort)((packet.Data[1] << 8) | packet.Data[0]);
                                chunk.Data.Add(packet);
                                ptr += packet.Length + 10;
                            }
                        }
                    }
                }
                else
                {
                    content = Crypt(chunk.Length, content);
                    var ptr = 0;
                    using (var mschunk = new MemoryStream(content))
                    {
                        using (var brchunk = new BinaryReader(mschunk))
                        {
                            while (ptr < chunk.Length)
                            {
                                var entry = new Chunk();
                                entry.Id = brchunk.ReadInt16();
                                entry.Length = brchunk.ReadInt32();
                                entry.Data = brchunk.ReadBytes(entry.Length);
                                chunk.Data.Add(entry);
                                ptr += entry.Length + 6;
                            }
                        }
                    }
                }

                br.BaseStream.Seek(lastOffset, SeekOrigin.Begin);
            }
        }

        private byte[] Crypt(int size, byte[] buffer)
        {
            var offset = 0;

            if (buffer == null)
                return new byte[] { };

            var realKey1 = GetKey1(Date) >> 5;
            var realKey2 = GetKey2(Date) >> 3;
            var ret = new byte[size];
            using (var ms = new MemoryStream(buffer))
            {
                using (var msw = new MemoryStream(ret))
                {
                    using (var br = new BinaryReader(ms))
                    {
                        using (var bw = new BinaryWriter(msw))
                        {
                            for (var cursor = 0; cursor < size / 4; cursor++)
                            {
                                var tempOld = br.ReadInt32();
                                var temp = tempOld ^ (realKey1 + (cursor + 1)) * realKey2;
                                bw.Write(temp);
                                offset += 4;
                            }
                            //Debug.Print("{0}", size - offset);
                            bw.Write(br.ReadBytes(size - offset));
                        }
                    }
                }
            }

            return ret;
        }

        private int GetKey1(DateTime date)
        {
            var b = new byte[4];
            using (var ms = new MemoryStream())
            {
                using (var bw = new BinaryWriter(ms))
                {
                    bw.Write((short)date.Year);
                    bw.Write((byte)date.Month);
                    bw.Write((byte)date.Day);
                }

                b = ms.ToArray();
            }

            using (var ms = new MemoryStream(b))
            {
                using (var br = new BinaryReader(ms))
                {
                    return br.ReadInt32();
                }
            }
        }

        private int GetKey2(DateTime date)
        {
            var b = new byte[4];
            using (var ms = new MemoryStream())
            {
                using (var bw = new BinaryWriter(ms))
                {
                    bw.Write((byte)0);
                    bw.Write((byte)date.Hour);
                    bw.Write((byte)date.Minute);
                    bw.Write((byte)date.Second);
                }

                b = ms.ToArray();
            }

            using (var ms = new MemoryStream(b))
            {
                using (var br = new BinaryReader(ms))
                {
                    return br.ReadInt32();
                }
            }
        }
        #endregion
    }
}
