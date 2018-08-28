using System;
using System.Runtime.InteropServices.WindowsRuntime;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using nucs.Chaining.Extensions;

namespace nucs.Chaining.Example.Wiki {
    [TestClass]
    public class Terminology_Script_Naming {
        [TestMethod]
        public void X0() {
            var chain = Chain.Build(c => {
                int rolls = 100; //total amount of rolls allowed.
                var rand = new Random();
                return c.Continue(_ => {
                    c.NameScript("Reroll Dice");
                    //condition script
                    //check rolls limit
                    if (rolls <= 0) {
                        Console.WriteLine("Out of rolls...");
                        return c.Complete();
                    }

                    //roll dice
                    int n = rand.Next(0, 6);
                    rolls--;
                    Console.WriteLine($"Rolling.... Dice surface shows: {n}");

                    //check results
                    if (n != 3) {
                        Console.WriteLine("Rerolling...");
                        return null; //or c.Repeat will cause this script to be rescheduled again on next pulse.
                    } 

                    //winner winner, chicken dinner
                    return __ => {
                        //work
                        Console.WriteLine("3! win win win!");
                        Console.WriteLine("Rerolling...");
                        return c.BackwardsTo("Reroll Dice"); //will fallback/crawl to the last time "Reroll Dice" has been executed.
                        //If you wish to push into the stack, use c.To(string). 
                    };
                });
            });

            chain.PulseToCompletion();
            chain.Completed.Should().BeTrue();
        }

        [TestMethod]
        public void X1() {
            var chain = Chain.Build(c => {
                var rand = new Random();
                return c.Continue(_ => {
                    c.NameScript("Reroll Dice");

                    //roll dice
                    int n = rand.Next(0, 6);
                    Console.WriteLine($"Rolling.... Dice surface shows: {n}");

                    //check results
                    if (n != 3) {
                        Console.WriteLine("Rerolling...");
                        return null; //or c.Repeat will cause this script to be rescheduled again on next pulse.
                    } 

                    //winner winner, chicken dinner
                    return __ => {
                        try {
                            return c.To("Outside Script");
                        } catch (ArgumentException) {
                            return null; //repeat till script is found.
                        }
                    };
                });
            });

            chain.NameScript("Outside Script", c => {
                Console.WriteLine("Im 'injected' from the outside of the main workflow script.");
                return c.Complete();
            });

            chain.PulseToCompletion();
            chain.Completed.Should().BeTrue();
        }
    }
}