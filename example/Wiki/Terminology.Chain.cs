using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using nucs.Chaining.Extensions;

namespace nucs.Chaining.Example.Wiki {
    [TestClass]
    public class Teminology_Chain {
        [TestMethod]
        public void X0() {
            var chain = Chain.Build(
                c => { //first method
                    return _ => { //second method
                        return c.Complete(); //third method returned.
                    };
                });

            chain.PulseToCompletion();
            chain.StackSize.Should().Be(2);
            chain.Completed.Should().BeTrue();
        }

        [TestMethod]
        public void X1() {
            int i = 0;
            var chain = Chain.Build(c => {
                i++;
                return c.Complete();
            });

            chain.Pulse();
            chain.Completed.Should().BeTrue();
            i.Should().Be(1);
        }
    }
}