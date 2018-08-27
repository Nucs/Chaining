using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using nucs.Chaining.Extensions;

namespace nucs.Chaining.Tests.Extensions {
    [TestClass]
    public class ChainExtensionsTests {

        [TestMethod] 
        public void RunToCompletionTest() {
            var chain = Chain.Build(c => c.Complete());

            chain.PulseToCompletion();

            chain.Completed.Should().BeTrue();
            chain.Failed.Should().BeFalse();
            chain.State.Should().Be(ChainState.Completed);
        }

        [TestMethod] 
        public void RunToCompletionFailingTest() {
            var chain = Chain.Build(c => c.Fail());

            chain.PulseToCompletion();

            chain.Completed.Should().BeTrue();
            chain.Failed.Should().BeTrue();
            chain.State.Should().Be(ChainState.Failed);
        }

        [TestMethod] 
        public void RunToCompletionExceptionTest() {
            var chain = Chain.Build(c => throw new InvalidOperationException());
            Assert.ThrowsException<InvalidOperationException>(()=>chain.PulseToCompletion());

            chain.Completed.Should().BeFalse();
            chain.State.Should().Be(ChainState.Faulty);
            chain.Failed.Should().BeFalse();
        }
    }
}
