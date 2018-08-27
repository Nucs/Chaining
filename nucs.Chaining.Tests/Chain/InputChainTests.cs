using System;
using System.Collections;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace nucs.Chaining.Tests {
    [TestClass]
    public class InputChainTests {
        [TestMethod]
        [Timeout(5000)]
        public void Simple() {
            var chain = InputChain<int>.Build((c, r) => {
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
            var chain = InputChain<int>.Build((c, r) => {
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
            var chain = InputChain<int>.Build((c, r) => {
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
            var chain = InputChain<int>.Build((c, r) => {
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
            var chain = InputChain<int>.Build((c, r) => {
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
            var chain = InputChain<int>.Build((c, r) => {
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
            var chain = InputChain<int>.Build((c, r) => {
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
            var chain = InputChain<int>.Build((c, r) => {
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
            var chain = InputChain<int>.Build((c, r) => {
                if (r.Value >= 1) {
                    return c.Complete(true);
                }

                return c1 => {
                    r.Value++;
                    return c.Complete();
                };
            });

            //run
            while (!chain.Completed) {
                chain.Pulse();
            }

            chain.Current.Should().Be(1);
            chain.Completed.Should().BeTrue();
            chain.Failed.Should().BeFalse();
            chain.State.Should().Be(ChainState.Completed);
        }

        [TestMethod]
        [Timeout(5000)]
        public void Continue() {
            int ticks = 0;
            int x = 0;
            var chain = InputChain<int>.Build((c, r) => {
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
            var chain = InputChain<int>.Build((c, r) => {
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
            var chain = InputChain<int>.Build((c, r) => {
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
            var chain = InputChain<int>.Build((c, r) => {
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
            var chain = InputChain<int>.Build((c, r) => {
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
            var chain = InputChain<int>.Build((c, r) => {
                //1
                return _ => {
                    //2
                    c.NameScript("2");
                    r.Value++;
                    if (r.Value >= 3)
                        return c.Complete();
                    return c.BackwardsTo("2");
                };
            });

            //run
            while (!chain.Completed) {
                chain.Pulse();
            }
        }


        [TestMethod]
        [Timeout(5000)]
        public void Dequeue() {
            var queue = new Queue<int>();
            queue.Enqueue(1);
            queue.Enqueue(2);
            queue.Enqueue(3);
            queue.Enqueue(4);
            queue.Enqueue(5);
            var chain = InputChain<int>.Build((c, r) => {
                r.Value.Should().Be(1);
                return _ => {
                    r.Value.Should().Be(2);
                    return __ => {
                        r.Value.Should().Be(3);
                        return ___ => {
                            r.Value.Should().Be(4);
                            return ____ => {
                                r.Value.Should().Be(5);
                                return c.Complete();
                            };
                        };
                    };
                };
            });

            //run
            while (!chain.Completed && queue.Count > 0) {
                chain.Pulse(queue.Dequeue());
            }
        }

        [TestMethod]
        [Timeout(5000)]
        public void ReferenceHasChanged() {
            var queue = new Queue<int>();
            queue.Enqueue(1);
            queue.Enqueue(2);
            queue.Enqueue(3);
            queue.Enqueue(4);
            queue.Enqueue(5);

            var chain = InputChain<int>.Build((c, r) => {
                r.Value.Should().Be(1);
                r.HasChanged.Should().BeTrue();
                return _ => {
                    r.Value.Should().Be(2);
                    r.HasChanged.Should().BeTrue();
                    return __ => {
                        r.Value.Should().Be(2);
                        r.HasChanged.Should().BeFalse();
                        return __3 => {
                            r.Value.Should().Be(2);
                            r.HasChanged.Should().BeFalse();
                            return __1 => {
                                r.Value.Should().Be(3);
                                r.HasChanged.Should().BeTrue();
                                return ___ => {
                                    r.Value.Should().Be(4);
                                    r.HasChanged.Should().BeTrue();
                                    return ____ => {
                                        r.Value.Should().Be(5);
                                        r.HasChanged.Should().BeTrue();
                                        return c.Complete();
                                    };
                                };
                            };
                        };
                    };
                };
            });

            //run
            chain.Pulse(queue.Dequeue()); //1
            chain.Pulse(queue.Dequeue()); //2
            chain.Pulse(); //2
            chain.Pulse(); //2
            chain.Pulse(queue.Dequeue()); //3
            chain.Pulse(queue.Dequeue()); //4
            chain.Pulse(queue.Dequeue()); //5
            chain.Completed.Should().BeTrue();
        }
    }
}