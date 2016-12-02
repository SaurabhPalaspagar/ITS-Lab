using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AccessMatlab;

namespace VehicleArrivalModel
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(Matlab.callFunction(2));
            Console.ReadLine();
            
        }
    }
}
