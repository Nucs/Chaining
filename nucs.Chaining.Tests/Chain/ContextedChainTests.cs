//using System;
//using System.Diagnostics;
//using Ebby.Algorithm.Indicators.Analytical.Modules;
//using FluentAssertions;
//using Microsoft.VisualStudio.TestTools.UnitTesting;

//namespace Ebby.Algorithm.Tests {
//    [TestClass]
//    public class ContextedChainTests {
//        [TestMethod]
//        [Timeout(5000)]
//        public void Simple() {
//            var chain = Chain<string>.Build("a",(c, ctx) => {
//                return ("yo", c.Complete());
//            });

//            //run
//            while (!chain.Completed) {
//                chain.Pulse();
//            }

//            chain.Completed.Should().BeTrue();
//            chain.Failed.Should().BeFalse();
//            chain.State.Should().Be(ChainState.Completed);
//        }

//        [TestMethod]
//        [Timeout(5000)]
//        public void SimpleDepth1() {
//            int x = 0;
//            var chain = Chain<string>.Build("a",(c, ctx) => {
//                return (ctx, (c1, ctx2) => {
//                    x++;
//                    return (null, c.Complete());
//                });
//            });

//            //run
//            while (!chain.Completed) {
//                chain.Pulse();
//            }

//            x.Should().Be(1);
//            chain.Completed.Should().BeTrue();
//            chain.Failed.Should().BeFalse();
//            chain.State.Should().Be(ChainState.Completed);
//        }

//        [TestMethod]
//        [Timeout(5000)]
//        public void Backwards() {
//            int x = 0;
//            var chain = Chain<string>.Build("a",(c, ctx) => {
//                if (x == 1) {
//                    return c.Complete();
//                }

//                return c1 => {
//                    x++;
//                    return c1.Backwards();
//                };
//            });

//            //run
//            while (!chain.Completed) {
//                chain.Pulse();
//            }

//            x.Should().Be(1);
//            chain.Completed.Should().BeTrue();
//            chain.Failed.Should().BeFalse();
//            chain.State.Should().Be(ChainState.Completed);
//        }

//        [TestMethod]
//        [Timeout(5000)]
//        public void BackwardsDepth2() {
//            int x = 0;
//            var chain = Chain<string>.Build("a",(c, ctx) => {
//                if (x == 2) {
//                    return c.Complete();
//                }

//                return c1 => {
//                    x++;
//                    return c3 => {
//                        x++;
//                        return c.Backwards(2);
//                    };
//                };
//            });

//            //run
//            while (!chain.Completed) {
//                chain.Pulse();
//            }

//            x.Should().Be(2);
//            chain.Completed.Should().BeTrue();
//            chain.Failed.Should().BeFalse();
//            chain.State.Should().Be(ChainState.Completed);
//        }

//        [TestMethod]
//        [Timeout(5000)]
//        public void BackwardsContinue() {
//            int x = 0;
//            int changes = 0;
//            int ticks = 0; //1 is first, (already 1 when calling first Pulse).
//            var chain = Chain<string>.Build("a",(c, ctx) => {
//                if (x == 1) {
//                    ticks.Should().Be(2);
//                    return c.Complete();
//                }

//                return c1 => {
//                    x++;
//                    return c1.BackwardsContinue();
//                };
//            });
//            chain.ScriptChanged += (previous, script) => {
//                changes++;
//            };
//            //run
//            while (!chain.Completed) {
//                ticks++;
//                chain.Pulse();
//            }

//            x.Should().Be(1);
//            ticks.Should().Be(2);
//            chain.Completed.Should().BeTrue();
//            chain.Failed.Should().BeFalse();
//            chain.State.Should().Be(ChainState.Completed);
//        }

//        [TestMethod]
//        [Timeout(5000)]
//        public void SimpleFail() {
//            var chain = Chain<string>.Build("a",(c, ctx) => {
//                return c.Fail();
//            });

//            //run
//            while (!chain.Completed) {
//                chain.Pulse();
//            }

//            chain.Completed.Should().BeTrue();
//            chain.Failed.Should().BeTrue();
//            chain.State.Should().Be(ChainState.Failed);
//        }

//        [TestMethod]
//        [Timeout(5000)]
//        public void SimpleFailReset() {
//            var chain = Chain<string>.Build("a",(c, ctx) => {
//                return c.Fail();
//            });

//            //run
//            while (!chain.Completed) {
//                chain.Pulse();
//            }

//            chain.Completed.Should().BeTrue();
//            chain.Failed.Should().BeTrue();
//            chain.State.Should().Be(ChainState.Failed);

//            chain.Reset();
//            //run
//            while (!chain.Completed) {
//                chain.Pulse();
//            }

//            chain.Completed.Should().BeTrue();
//            chain.Failed.Should().BeTrue();
//            chain.State.Should().Be(ChainState.Failed);
//        }

//        [TestMethod]
//        [Timeout(5000)]
//        public void AutoResetting() {
//            int x = 0;
//            var chain = Chain<string>.Build("a",(c, ctx) => {
//                if (x >= 1) {
//                    return c.Complete(true);
//                }

//                return c1 => {
//                    x++;
//                    return c.Complete();
//                };
//            });

//            //run
//            while (!chain.Completed) {
//                chain.Pulse();
//            }

//            x.Should().Be(1);
//            chain.Completed.Should().BeTrue();
//            chain.Failed.Should().BeFalse();
//            chain.State.Should().Be(ChainState.Completed);
//        }

//        [TestMethod]
//        [Timeout(5000)]
//        public void Continue() {
//            int ticks = 0;
//            int x = 0;
//            var chain = Chain<string>.Build("a",(c, ctx) => {
//                if (ticks == 1 && x == 3) 
//                    return c.Complete();
//                x++;
//                return c.Continue();
//            });

//            //run
//            while (!chain.Completed) {
//                ticks++;
//                chain.Pulse();
//            }

//            ticks.Should().Be(1);
//            x.Should().Be(3);
//            chain.Completed.Should().BeTrue();
//            chain.Failed.Should().BeFalse();
//            chain.State.Should().Be(ChainState.Completed);
//        }

//        class stringHolder {
//            public string str { get; set; }
//        }
//        [TestMethod]
//        [Timeout(5000)]
//        public void ConfigurationLayer() {
//            var chain = Chain<string>.Build("a",(c, ctx) => {
//                //config
//                stringHolder pointer = new stringHolder(){str = "1"};

//                //------------------------------
//                return c1 => {

//                    if (pointer.str == "3") //check new value from inner.
//                        return c.Complete();

//                    if (pointer.str != "1")
//                        throw new Exception("C# Broke");

//                    //override inner property of pointer
//                    pointer.str = "2"; 

//                    //------------------------------
//                    return _ => {
//                        if (pointer.str != "2")
//                            throw new Exception("C# Broke");

//                        //override pointer.
//                        pointer = new stringHolder(){str = "3"};
//                        return c.Backwards(1); //to root.
//                    };
//                };
//            });

//            //run
//            while (!chain.Completed) {
//                chain.Pulse();
//            }

//        }
//    }
//}