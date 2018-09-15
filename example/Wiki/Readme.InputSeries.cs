using System;

namespace nucs.Chaining.Example.Wiki {
    public class Readme_InputSeries {
        public void X() {
            bool isOdd(int @in) => @in % 2 != 0;
            int[] input = {0, 1, 1, 2, 3, 5, 8, 13, 21, 34, 55, 89, 144, 233, 377, 610, 987, 1597, 2584, 4181, 6765, 10946, 17711, 28657, 46368, 75025, 121393, 196418, 317811};

            //if two inputs one after an other are odd, decrement both of them by 1 to make them even.

            var chain = InputChain<(int Index, int Value)>.Build(
                (c, r) => { //First stage
                    if (!isOdd(r.Value.Value)) //is not odd
                        return c.Repeat; //same as returning null.

                    return c.Continue(_ => { //Enters Second stage without waiting for next pulse.
                        c.NameScript("Set Previous");
                        (int Index, int Value) previous = r.Value;

                        return __ => { //third stage on next pulse.
                            (int Index, int Value) current = r.Value;
                            if (!isOdd(current.Value)) //is not odd
                                return c.ContinueBackwardsTo("Set Previous"); //fallback without waiting for pulse and set current as previous.

                            //decrement
                            input[previous.Index] = previous.Value - 1;
                            input[current.Index] = current.Value - 1;
                            return c.Restart(); //goes back to first stage on next pulse.
                        };
                    });
                    //is odd
                });

            Console.WriteLine("Before: " + string.Join(", ", input));

            //pulse all inputs, this can be through an event or any other source.
            for (int i = 0; i < input.Length; i++)
                chain.Pulse((i, input[i]));

            Console.WriteLine("After: " + string.Join(", ", input));
        }
    }
}