using System;
using System.Collections.Generic;

namespace CardMakerCustomizer.Logic
{
    public class ChunithmPrintStateInfo
    {
        public int OrderId { get; set; }
        public bool HasCompleted { get; set; }
        public string LimitDate { get; set; }
        public int PlaceId { get; set; }
        public int CardId { get; set; }
        public int GachaId { get; set; }
    }

    public static class ChunithmGachaHelper
    {
        public static ChunithmPrintStateInfo[] BuildFallbackPrintStates(int numCards, int gachaId, int[] cardIds, int placeId, DateTime localNow)
        {
            if (numCards <= 0)
            {
                return new ChunithmPrintStateInfo[0];
            }

            var states = new ChunithmPrintStateInfo[numCards];
            string limitDateStr = localNow.AddDays(30).ToString("yyyy-MM-dd HH:mm:ss");

            for (int i = 0; i < numCards; i++)
            {
                int cId = (cardIds != null && i < cardIds.Length) ? cardIds[i] : 0;
                states[i] = new ChunithmPrintStateInfo
                {
                    OrderId = i + 1,
                    HasCompleted = false,
                    LimitDate = limitDateStr,
                    PlaceId = placeId,
                    CardId = cId,
                    GachaId = gachaId
                };
            }

            return states;
        }

        public static void GeneratePrintSerial(long currentOrderId, string currentSerialId, DateTime utcNow, out long newOrderId, out string newSerialId)
        {
            if (currentOrderId == 0L)
            {
                newOrderId = utcNow.Ticks;
            }
            else
            {
                newOrderId = currentOrderId;
            }

            if (string.IsNullOrEmpty(currentSerialId))
            {
                newSerialId = "CHU-" + Guid.NewGuid().ToString("N").Substring(0, 16).ToUpper();
            }
            else
            {
                newSerialId = currentSerialId;
            }
        }

        public static bool ShouldRecoverGachaUpsert(bool isError)
        {
            return isError;
        }

        public static bool ShouldRecoverPrintState(bool hasPrintStates, int currentCount, int expectedCount)
        {
            return !hasPrintStates || currentCount != expectedCount;
        }

        public static int[] RollFallbackCardIds(int times, int[] pool)
        {
            if (times <= 0)
            {
                return new int[0];
            }

            if (pool == null || pool.Length == 0)
            {
                int[] defaults = new int[times];
                for (int i = 0; i < times; i++)
                {
                    defaults[i] = 1;
                }
                return defaults;
            }

            var result = new int[times];
            var rand = new Random();
            for (int i = 0; i < times; i++)
            {
                result[i] = pool[rand.Next(pool.Length)];
            }
            return result;
        }
    }
}
