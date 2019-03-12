using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace uFR_AES_tester_console
{

    class Program
    {
        static void Main(string[] args)
        {
            Functions.reader_open();
            Functions.usage();

            while (true)
            {
                char c = Console.ReadKey(true).KeyChar;

                if(c != '\x1b' && (byte)c != 0)
                {
                    Functions.menu(c);
                }else if((byte)c == 0)
                {

                }
                else if(c == '\x1b')
                {
                    break;
                }
            }

        }
    }
}
