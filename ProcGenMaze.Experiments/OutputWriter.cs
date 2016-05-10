using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcGenMaze.Experiments
{
    public class OutputWriter : IOutputWriter
    {
        public void Print(string val)
        {
            Console.Write(val);
        }

        public void PrintLn(string val)
        {
            Console.WriteLine(val);
        }
    }
}
