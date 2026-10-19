using System;

namespace CardMakerCustomizer.Logic
{
    public class QRPayloadInfo
    {
        public int PromotionCode { get; set; }
        public int CardId { get; set; }
        public int Serial { get; set; }
        public uint GameId { get; set; }
    }

    public static class QRHelper
    {
        public static readonly byte[] SegaKey = new byte[]
        {
            38, 34, 177, 150, 54, 114, 151, 245, 80, 162, 229, 42, 75, 224, 55, 156
        };

        public static readonly byte[] OngekiKey = SegaKey;

        public static readonly byte[] MaimaiKey = new byte[]
        {
            144, 95, 66, 167, 195, 243, 253, 226, 84, 194, 239, 80, 177, 205, 41, 78
        };

        public const uint OngekiGameId = 5522500U;
        public const uint MaimaiGameId = 5915972U;

        public static int GetQRDimension(int version)
        {
            return 21 + (version - 1) * 4;
        }

        public static byte[] CreateBinaryPayload(int promotionCode, int cardID, int serial, uint gameID)
        {
            byte[] bytes = new byte[14];
            bytes[0] = (byte)(promotionCode >> 0 & 255);
            bytes[1] = (byte)(promotionCode >> 8 & 255);
            bytes[2] = (byte)(promotionCode >> 16 & 255);
            bytes[3] = (byte)(promotionCode >> 24 & 255);
            bytes[4] = (byte)(cardID >> 0 & 255);
            bytes[5] = (byte)(cardID >> 8 & 255);
            bytes[6] = (byte)(cardID >> 16 & 255);
            bytes[7] = (byte)(serial >> 0 & 255);
            bytes[8] = (byte)(serial >> 8 & 255);
            bytes[9] = (byte)(serial >> 16 & 255);
            bytes[10] = (byte)(serial >> 24 & 255);
            bytes[11] = (byte)(gameID >> 0 & 255U);
            bytes[12] = (byte)(gameID >> 8 & 255U);
            bytes[13] = (byte)(gameID >> 16 & 255U);
            return bytes;
        }

        public static QRPayloadInfo ParsePayload(byte[] bytes)
        {
            if (bytes == null || bytes.Length < 14)
            {
                throw new ArgumentException("Payload must be at least 14 bytes");
            }

            int promo = bytes[0] | (bytes[1] << 8) | (bytes[2] << 16) | (bytes[3] << 24);
            int cardId = bytes[4] | (bytes[5] << 8) | (bytes[6] << 16);
            int serial = bytes[7] | (bytes[8] << 8) | (bytes[9] << 16) | (bytes[10] << 24);
            uint gameId = (uint)(bytes[11] | (bytes[12] << 8) | (bytes[13] << 16));

            return new QRPayloadInfo
            {
                PromotionCode = promo,
                CardId = cardId,
                Serial = serial,
                GameId = gameId
            };
        }

        private static void Swap(ref byte x0, ref byte x1)
        {
            byte b = x1;
            x1 = x0;
            x0 = b;
        }

        public static void EncryptARCFOUR(byte[] data, byte[] key = null)
        {
            byte[] actualKey = key ?? SegaKey;
            byte[] array = new byte[256];
            for (int k = 0; k < 256; k++)
            {
                array[k] = (byte)k;
            }

            int num = 0;
            for (int k = 0; k < 256; k++)
            {
                num = (num + (int)array[k] + (int)actualKey[k % actualKey.Length] & 255);
                Swap(ref array[k], ref array[num]);
            }

            int i = 0;
            num = 0;
            for (int j = 0; j < data.Length; j++)
            {
                i = (i + 1 & 255);
                num = (num + (int)array[i] & 255);
                Swap(ref array[i], ref array[num]);
                data[j] ^= array[(int)(array[i] + array[num] & byte.MaxValue)];
            }
        }

        public static void DecryptARCFOUR(byte[] cipher, byte[] key = null)
        {
            EncryptARCFOUR(cipher, key);
        }

        public static string ResolveCustomQRText(bool enabled, string customText)
        {
            if (enabled && !StringCompat.IsNullOrWhiteSpace(customText))
            {
                return customText.Trim();
            }

            return null;
        }
    }
}
