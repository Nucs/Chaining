using System;
using System.Runtime.InteropServices.WindowsRuntime;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using nucs.Chaining.Extensions;

namespace nucs.Chaining.Example.Wiki {
    [TestClass]
    public class Terminology_Condition_Script {
        [TestMethod]
        public void X() {
            var chain = Chain.Build(c => {
                var rand = new Random();
                return c.Continue(_ => { 
                    //condition script
                    if (rand.Next(0, 100) != 50)
                        return null; //or c.Repeat will cause this script to be rescheduled again on next pulse.
                    return __ => {
                        //work
                        return c.Complete();
                    };
                });
            });

            chain.PulseToCompletion();
            chain.Completed.Should().BeTrue();
        }
    }
}