using System;
using System.Diagnostics;
using System.Threading;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using nucs.Chaining.Extensions;

namespace nucs.Chaining.Example.Wiki {
    [TestClass]
    public class Terminology_Initialization_Script {
        [TestMethod]
        public void X() {
            var chain = Chain.Build(c => {
                //initialization script
                var stopwatch = new Stopwatch();
                var rand = new Random();
                stopwatch.Start();
                return c.Continue(_ => { //working script
                    var i = rand.Next(1, 100);
                    if (i == 5) {
                        return __ => {
                            stopwatch.Stop();
                            Console.WriteLine(stopwatch.Elapsed);
                            return c.Complete();
                        };
                    } else {
                        return __ => {
                            Thread.Sleep(1); //some work
                            return c.Backwards(1);
                        };
                    }
                });
            });

            chain.PulseToCompletion();

            chain.Completed.Should().BeTrue();
            chain.StackSize.Should().Be(3);
        }
    }
}