using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using nucs.Chaining.Extensions;

namespace nucs.Chaining.Example {
    class Program {
        static void Main(string[] args) {
            var statemachine = Chain.Build(c => {
                //initialize the states
                Console.WriteLine("Welcome;");
                
                int a = int.MaxValue;
                int b = int.MaxValue;

                //add independent script that can be called at any time:
                c.NameScript("End", _ => {
                    Console.WriteLine($"Finished, {a}*{b}={a*b}");
                    return c.Complete();
                });

                //continue immediatly to next state without waiting for an other pulse:
                return c.Continue(_ => {
                    
                });
            });

            statemachine.Pulse();

            Console.WriteLine();
            Console.WriteLine("Exited State Machine.");
            Console.WriteLine("Press Enter to exit...");
            Console.ReadLine();
        }
    }
}