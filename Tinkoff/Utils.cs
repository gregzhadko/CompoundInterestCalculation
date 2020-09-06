using System;

namespace Tinkoff
{
    public class Utils
    {
        public static void WriteLine(string line)
        {
            Console.WriteLine($"{DateTime.Now.TimeOfDay}: {line}");
        }
    }
}