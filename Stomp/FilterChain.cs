using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Stomp.Filters;

namespace Stomp
{
    public class FilterChain
    {
        public static int Indent = -2;
        public List<IFilter> Filters = new List<IFilter>();

        public FilterChain()
        {
        }

        public FilterChain(params IFilter[] filters)
        {
            Append(filters);
        }

        public void Apply(FastBitmap bmp)
        {
            Indent += 2;
            foreach (var filter in Filters)
            {
                filter.OnMessage += (message, sender) => 
                {
                    PrintMessage('+', message);
                };

                if (filter.IsContext)
                {
                    PrintMessage('=', "Entering context {0}", filter.ScriptName);
                    Stopwatch sw = Stopwatch.StartNew();
                    Indent += 2;
                    filter.Apply(bmp);
                    Indent -= 2;
                    sw.Stop();
                    PrintMessage('=', "Exiting context {0}", filter.ScriptName);
                }
                else
                {
                    Stopwatch sw = Stopwatch.StartNew();
                    filter.Apply(bmp);
                    sw.Stop();

                    PrintMessage('+', "Executed filter {0} in {1:0.00} seconds.", filter.ScriptName, sw.ElapsedMilliseconds / 1000d);
                }
            }
            Indent -= 2;
        }

        public void PrintMessage(char c, string msg, params object[] format)
        {
            Console.Write(new String(' ', Indent));
            Console.Write("[");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(c);
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("] ");
            Console.WriteLine(msg, format);
        }

        public void Append(params IFilter[] filters)
        {
            AppendRange(filters);
        }

        public void AppendRange(IEnumerable<IFilter> filters)
        {
            foreach (var filter in filters)
                Filters.Add(filter);
        }
    }
}

