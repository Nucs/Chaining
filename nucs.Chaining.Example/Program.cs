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
                    c.NameScript("Route");

                    if (a == int.MaxValue) {
                        return c.Continue(_1 => {
                            Console.Write("Enter first number:");
                            var line = Console.ReadLine();
                            if (int.TryParse(line, out var n)) {
                                a = n;
                                return c.ContinueBackwardsTo("Route"); //jump to the script that was named Route.
                            }
                            //parsing failed, reset;
                            Console.WriteLine("Invalid Input;");
                            return c.Continue(); //wait for next pulse to rerun this script.
                        });
                    }

                    if (b == int.MaxValue) {
                        return c.Continue(_1 => {
                            Console.Write("Enter second number:");
                            var line = Console.ReadLine();
                            if (int.TryParse(line, out var n)) {
                                b = n;
                                return c.ContinueBackwardsTo("Route"); //jump to the script that was named Route.
                            }
                            //parsing failed, reset;
                            Console.WriteLine("Invalid Input;");
                            return c.Continue(); //wait for next pulse to rerun this script.
                        });
                    }

                    return c.ContinueTo("End");
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