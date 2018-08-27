namespace nucs.Chaining.Extensions {
    public static class ChainExtensions {
        public static TChain PulseToCompletion<TChain>(this TChain chain) where TChain : Chain {
            if (chain == null)
                return null;
            while (!chain.Completed)
                chain.Pulse();

            return chain;
        }
    }
}