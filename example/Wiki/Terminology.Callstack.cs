using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace nucs.Chaining.Example.Wiki {
    [TestClass]
    public class Teminology_Callstack {
        [TestMethod]
        public void X() {
            var chain = Chain.Build(c => {
                //c.StackSize == 1
                //script 1 - initialization
                int i = 1;
                return c.Continue(_ => {
                    //c.StackSize == 2
                    //script 2 - condition
                    if (i == 2)
                        return c.Complete();
                    return _c => {
                        //c.StackSize == 3
                        //script 3 - action
                        i++;
                        return c.Backwards(1); // goes back to script 2.
                    };
                });
            });

            chain.Pulse();
            chain.Pulse();
            chain.Pulse();
            chain.Completed.Should().BeTrue();
        }
    }
}