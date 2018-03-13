using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RagnarokReplayExample
{
    public static class ByteArrayExtension
    {
        public static string Hexdump(this byte[] buffer)
        {
            var b = new StringBuilder();
            b.AppendLine("[0x" + buffer.Length.ToString("X8") + " (" + buffer.Length + ")] {");

            int length = Math.Min(buffer.Length, 0xFFFF);
            for (int i = 0; i < length; i += 16)
            {
                b.AppendFormat("  {0:X4}  ", i);
                for (int n = 0; n < 8; n++)
                {
                    int o = i + n;
                    if (o < length)
                        b.AppendFormat("{0:X2} ", buffer[o]);
                    else
                        b.Append("   ");
                }
                b.Append(" ");
                for (int n = 0; n < 8; n++)
                {
                    int o = i + n + 8;
                    if (o < length)
                        b.AppendFormat("{0:X2} ", buffer[o]);
                    else
                        b.Append("   ");
                }
                b.Append(" ");

                for (int n = 0; n < 8; n++)
                {
                    int o = i + n;
                    if (o < length)
                        b.AppendFormat("{0}", ToHexChar(buffer[o]));
                    else
                        b.Append(" ");
                }
                b.Append(" ");
                for (int n = 0; n < 8; n++)
                {
                    int o = i + n + 8;
                    if (o < length)
                        b.AppendFormat("{0}", ToHexChar(buffer[o]));
                    else
                        b.Append(" ");
                }
                b.AppendLine();
            }

            b.AppendLine("}");
            return b.ToString();
        }

        private static char ToHexChar(byte b)
        {
            return (b >= 0x20 && b < 0x80) ? (char)b : '.';
        }
    }
}
