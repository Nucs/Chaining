using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace nucs.Chaining.Tests {
    [TestClass]
    public class ChainTests {
        [TestMethod]
        [Timeout(5000)]
        public void Simple() {
            var chain = Chain.Build(c => {
                return c.Complete();
            });

            //run
            while (!chain.Completed) {
                chain.Pulse();
            }

            chain.Completed.Should().BeTrue();
            chain.Failed.Should().BeFalse();
            chain.State.Should().Be(ChainState.Completed);
        }

        [TestMethod]
        [Timeout(5000)]
        public void Cancellation() {
            var chain = Chain.Build(c => {
                return _ => c.Complete();
            });

            
            chain.Pulse();
            chain.Cancel();

            chain.Completed.Should().BeFalse();
            chain.Failed.Should().BeFalse();
            chain.State.Should().Be(ChainState.Cancelled);

            chain.Pulse(); 
            chain.Completed.Should().BeFalse("If cancel did not work, next pulse should set to complete.");
        }

        [TestMethod]
        [Timeout(5000)]
        public void SimpleDepth1() {
            int x = 0;
            var chain = Chain.Build(c => {
                return c1 => {
                    x++;
                    return c.Complete();
                };
            });

            //run
            while (!chain.Completed) {
                chain.Pulse();
            }

            x.Should().Be(1);
            chain.Completed.Should().BeTrue();
            chain.Failed.Should().BeFalse();
            chain.State.Should().Be(ChainState.Completed);
        }

        [TestMethod]
        [Timeout(5000)]
        public void Backwards() {
            int x = 0;
            var chain = Chain.Build(c => {
                if (x == 1) {
                    return c.Complete();
                }

                return c1 => {
                    x++;
                    return c1.Backwards();
                };
            });

            //run
            while (!chain.Completed) {
                chain.Pulse();
            }

            x.Should().Be(1);
            chain.Completed.Should().BeTrue();
            chain.Failed.Should().BeFalse();
            chain.State.Should().Be(ChainState.Completed);
        }

        [TestMethod]
        [Timeout(5000)]
        public void BackwardsDepth2() {
            int x = 0;
            var chain = Chain.Build(c => {
                if (x == 2) {
                    return c.Complete();
                }

                return c1 => {
                    x++;
                    return c3 => {
                        x++;
                        return c.Backwards(2);
                    };
                };
            });

            //run
            while (!chain.Completed) {
                chain.Pulse();
            }

            x.Should().Be(2);
            chain.Completed.Should().BeTrue();
            chain.Failed.Should().BeFalse();
            chain.State.Should().Be(ChainState.Completed);
        }

        [TestMethod]
        [Timeout(5000)]
        public void BackwardsContinue() {
            int x = 0;
            int changes = 0;
            int ticks = 0; //1 is first, (already 1 when calling first Pulse).
            var chain = Chain.Build(c => {
                if (x == 1) {
                    ticks.Should().Be(2);
                    return c.Complete();
                }

                return c1 => {
                    x++;
                    return c1.ContinueBackwards();
                };
            });
            chain.ScriptChanged += (previous, script) => {
                changes++;
            };
            //run
            while (!chain.Completed) {
                ticks++;
                chain.Pulse();
            }

            x.Should().Be(1);
            ticks.Should().Be(2);
            chain.Completed.Should().BeTrue();
            chain.Failed.Should().BeFalse();
            chain.State.Should().Be(ChainState.Completed);
        }

        [TestMethod]
        [Timeout(5000)]
        public void SimpleFail() {
            var chain = Chain.Build(c => {
                return c.Fail();
            });

            //run
            while (!chain.Completed) {
                chain.Pulse();
            }

            chain.Completed.Should().BeTrue();
            chain.Failed.Should().BeTrue();
            chain.State.Should().Be(ChainState.Failed);
        }

        [TestMethod]
        [Timeout(5000)]
        public void SimpleFailReset() {
            var chain = Chain.Build(c => {
                return c.Fail();
            });

            //run
            while (!chain.Completed) {
                chain.Pulse();
            }

            chain.Completed.Should().BeTrue();
            chain.Failed.Should().BeTrue();
            chain.State.Should().Be(ChainState.Failed);

            chain.Reset();
            //run
            while (!chain.Completed) {
                chain.Pulse();
            }

            chain.Completed.Should().BeTrue();
            chain.Failed.Should().BeTrue();
            chain.State.Should().Be(ChainState.Failed);
        }

        [TestMethod]
        [Timeout(5000)]
        public void AutoResetting() {
            int x = 0;
            var chain = Chain.Build(c => {
                if (x >= 1) {
                    return c.Complete(true);
                }

                return c1 => {
                    x++;
                    return c.Complete();
                };
            });

            //run
            while (!chain.Completed) {
                chain.Pulse();
            }

            x.Should().Be(1);
            chain.Completed.Should().BeTrue();
            chain.Failed.Should().BeFalse();
            chain.State.Should().Be(ChainState.Completed);
        }

        [TestMethod]
        [Timeout(5000)]
        public void Continue() {
            int ticks = 0;
            int x = 0;
            var chain = Chain.Build(c => {
                if (ticks == 1 && x == 3)
                    return c.Complete();
                x++;
                return c.Continue();
            });

            //run
            while (!chain.Completed) {
                ticks++;
                chain.Pulse();
            }

            ticks.Should().Be(1);
            x.Should().Be(3);
            chain.Completed.Should().BeTrue();
            chain.Failed.Should().BeFalse();
            chain.State.Should().Be(ChainState.Completed);
        }

        class stringHolder {
            public string str { get; set; }
        }

        [TestMethod]
        [Timeout(5000)]
        public void ConfigurationLayer() {
            var chain = Chain.Build(c => {
                //config
                stringHolder pointer = new stringHolder() {str = "1"};

                //------------------------------
                return c1 => {
                    if (pointer.str == "3") //check new value from inner.
                        return c.Complete();

                    if (pointer.str != "1")
                        throw new Exception("C# Broke");

                    //override inner property of pointer
                    pointer.str = "2";

                    //------------------------------
                    return _ => {
                        if (pointer.str != "2")
                            throw new Exception("C# Broke");

                        //override pointer.
                        pointer = new stringHolder() {str = "3"};
                        return c.Backwards(1); //to root.
                    };
                };
            });

            //run
            while (!chain.Completed) {
                chain.Pulse();
            }
        }

        [TestMethod]
        [Timeout(5000)]
        public void BackwardsTo() {
            int x = 0;
            var chain = Chain.Build(c => {
                //1
                return _ => {
                    //2
                    c.NameScript("2");
                    if (x > 0)
                        return c.Complete();

                    return __ => {
                        //3
                        return _1 => {
                            //4
                            return _2 => {
                                //5
                                return ___ => {
                                    //6
                                    x++;
                                    return c.BackwardsTo("2");
                                };
                            };
                        };
                    };
                };
            });

            //run
            while (!chain.Completed) {
                chain.Pulse();
            }
        }

        [TestMethod]
        [Timeout(5000)]
        public void BackwardsToContinue() {
            int x = 0;
            int ticks = 0;
            var chain = Chain.Build(c => {
                //1
                return _ => {
                    //2
                    c.NameScript("2");
                    if (x > 0)
                        return c.Complete();

                    return __ => {
                        //3
                        return _1 => {
                            //4
                            return _2 => {
                                //5
                                return ___ => {
                                    //6
                                    x++;
                                    return c.ContinueBackwardsTo("2");
                                };
                            };
                        };
                    };
                };
            });

            //run
            while (!chain.Completed) {
                ticks++;
                chain.Pulse();
            }

            ticks.Should().Be(6); //7 stages goes through, only 6 pulses.
        }

        [TestMethod]
        [Timeout(5000)]
        public void BackwardsToRoot() {
            int x = 0;
            var chain = Chain.Build(c => {
                c.NameScript("1");
                //1
                return _ => {
                    //2
                    if (x > 0)
                        return c.Complete();
                    return __ => {
                        //3
                        return _1 => {
                            //4
                            return _2 => {
                                //5
                                return ___ => {
                                    //6
                                    x++;
                                    return c.BackwardsTo("1");
                                };
                            };
                        };
                    };
                };
            });

            //run
            while (!chain.Completed) {
                chain.Pulse();
            }
        }

        [TestMethod]
        [Timeout(5000)]
        public void BackwardsToSameScript() {
            int x = 0;
            var chain = Chain.Build(c => {
                //1
                return _ => {
                    //2
                    c.NameScript("2");
                    x++;
                    if (x >= 3)
                        return c.Complete();
                    return c.BackwardsTo("2");
                };
            });

            //run
            while (!chain.Completed) {
                chain.Pulse();
            }

            chain.StackSize.Should().Be(2);
        }

        [TestMethod]
        [Timeout(5000)]
        public void To() {
            int x = 0;
            var chain = Chain.Build(c => {
                //1
                return _ => {
                    //2
                    c.NameScript("2");
                    x++;
                    if (x >= 3)
                        return c.Complete();
                    return c.To("2");
                };
            });

            //run
            while (!chain.Completed) {
                chain.Pulse();
            }

            chain.StackSize.Should().Be(4);
        }

        [TestMethod]
        [Timeout(5000)]
        public void StackSize() {
            int x = 0;
            var chain = Chain.Build(c => {
                //1
                return _ => {
                    //2
                    c.NameScript("2");
                    x++;
                    if (x >= 3)
                        return c.Complete();
                    return c.To("2");
                };
            });

            //run
            while (!chain.Completed) {
                chain.Pulse();
            }

            chain.StackSize.Should().Be(4);

            chain.Reset();
            chain.StackSize.Should().Be(1);
        }

        [TestMethod]
        [Timeout(5000)]
        public void ContinueTo() {
            int x = 0;
            var chain = Chain.Build(c => {
                //1
                return _ => {
                    //2
                    c.NameScript("2");
                    x++;
                    if (x >= 3)
                        return c.Complete();
                    return c.ContinueTo("2");
                };
            });

            //run
            while (!chain.Completed) {
                chain.Pulse();
            }

            chain.StackSize.Should().Be(4);
        }
    }
}