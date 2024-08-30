using System;

namespace StallServer
{
    public class Util
    {
        private static string GenerateRandomText(int textLength)
        {
            Random random = new Random();
            string characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            string result = string.Empty;

            for (int i = 0; i < textLength; i++) {
                int character = random.Next(characters.Length);
                result += characters[character];
            }

            return result;
        }

        private static ConsoleColor GenerateRandomConsoleColor()
        {
            Random random = new Random();

            List<ConsoleColor> colors = new List<ConsoleColor>([ ConsoleColor.Blue, ConsoleColor.Green, ConsoleColor.Red, ConsoleColor.Yellow, ConsoleColor.Cyan, ConsoleColor.Magenta ]);
            return colors[random.Next(colors.Count)];
        }

        public static StallID CreateStallID(int textLength)
        {
            return new StallID(GenerateRandomText(textLength), GenerateRandomConsoleColor());
        }

        public static bool IsByteArrayEmpty(byte[] bytes)
        {
            bool isAllNull = true;

            foreach (byte b in bytes)
            {
                if (b != 0)
                {
                    isAllNull = false;
                    break;
                }
            }

            return isAllNull;
        }

        public static void Log(string origin, string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write($"{origin} ");

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(message);
        }

        public static void LogStall(StallID stallID, string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("[Stall ");

            Console.ForegroundColor = stallID.color;
            Console.Write(stallID.GetPreferredText());

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("] ");

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(message);
        }
    }
}
