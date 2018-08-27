using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using nucs.Chaining.Extensions;

namespace nucs.Chaining.Example {
    class Program {
        static void Main(string[] args) {

            bool hascalled = false;

            var statemachine = Chain.Build(c => {
                //initialize machine variables
                c.NameScript("Root"); //names current script root.
                hascalled = false;
                Console.WriteLine("Machine initialized;");

                return _1 => {
                    c.NameScript("Get number"); //names current script root.
                    Console.Write("Enter number to call: ");
                    var number = Console.ReadLine();
                    if (number==null || !number.All(Char.IsDigit)) { //if input is not all digits, repeat script on next pulse.
                        Console.WriteLine($"{number} is invalid phone number.");
                        return null; //returning null singals to repeat this script on next pulse, equivalent to c.Repeat.
                    }

                    hascalled = true;

                    //continue immediatly to next state without waiting for an other pulse:
                    return c.Continue(_2 => {
                        c.NameScript("Wait for number");

                        var sw = new Stopwatch();
                        Console.WriteLine($"Calling {number}...");
                        sw.Start();
                        //continue immediatly to current script.
                        return c.Continue(__ => {
                            //do actual call here
                            Thread.Sleep(1500);
                            sw.Stop();
                            Console.WriteLine($"Call finished after {sw.Elapsed.TotalSeconds:0.##}s.");
                            return c.BackwardsTo("Wait for number"); //walks backwards in callstack to the script named "Wait for number"
                        });
                    });
                };
            });

            statemachine.Pulse(); //first pulse to initialize machine.

            while(!hascalled)
                statemachine.Pulse();

            Console.WriteLine();
            Console.WriteLine("Exited State Machine.");
            Console.WriteLine("Press Enter to exit...");
            Console.ReadLine();
        }
    }
}