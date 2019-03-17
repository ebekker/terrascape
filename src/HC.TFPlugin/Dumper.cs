using System;
using System.IO;

namespace HC.TFPlugin
{
    public class Dumper
    {
        static Dumper()
        {
            var sw = new StreamWriter(new FileStream("dump.txt", FileMode.Append));
            sw.AutoFlush = true;
            Out = sw;
            Out.WriteLine();
            Out.WriteLine($"----- START: [{DateTime.Now.ToString("yyyy-MM-dd_HH:mm:ss")}]"
                + " ---------------------------------------------------");
        }

        public static TextWriter Out { get; }
    }
}