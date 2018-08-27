using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace nucs.Chaining.Example.Wiki
{
    [TestClass]
    public class Teminology_Pulse {
        [TestMethod]
        public void X() {
            var chain = Chain.Build(c => { //method one
                return _c => { //method two
                    return c.Complete();
                };
            });
            chain.Pulse(); 
            chain.Pulse();
            chain.Completed.Should().BeTrue();
        }
    }
}