using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mogre;
using Samples.CCS;

namespace Samples
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("-----------------------------------------");
            Console.WriteLine("Demo Initializing...");
            Dictionary<string, Action> apps = new Dictionary<string, Action>();
            apps.Add("Mogre Camera Control System Demo", () => { new CCSDemo().Go(); });
            
            string input = "";
            while (true) {
                Console.WriteLine("Demos available are shown below, start demo by input the number.");
                Console.WriteLine("Press q for quit.");
                Console.WriteLine("-----------------------------------------");

                int i = 0;
                foreach (var pair in apps) {
                    i++;
                    Console.WriteLine("{0}. {1}", i, pair.Key);
                }
                Console.WriteLine("please input your choice:");
                input = Console.ReadLine();
                if (input.ToLower() == "q") {
                    break;
                }
                int index = -1;
                if (int.TryParse(input, out index)) {
                    if (index >= 1 && index <= apps.Count) {
                        try {
                            Console.WriteLine("Starting {0}", apps.Keys.Skip(index - 1).First());
                            apps.Values.Skip(index - 1).First()();
                        } catch (Exception ex) {
                            Console.WriteLine(ex.Message);
                        }
                        continue;
                    }
                }
                Console.WriteLine("Wrong numder, try again...");

            }

        }
    }
}
