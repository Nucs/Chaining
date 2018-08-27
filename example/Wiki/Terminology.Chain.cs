using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
        }

        public void X1() {
            int i = 0;
            var c = Chain.Build(chain => {
                i++;
                return chain.Complete();
            });

            c.Pulse();
            c.Completed.Should().BeTrue();
            i.Should().Be(1);
        }
    }
}