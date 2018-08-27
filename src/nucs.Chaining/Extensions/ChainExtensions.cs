namespace nucs.Chaining.Extensions {
    public static class ChainExtensions {
        /// <summary>
        ///     Will pulse the chain till <see cref="nucs.Chaining.BaseChain{E,E}.Completed"/> is true.
        /// </summary>
        /// <typeparam name="TChain">The type of the chain.</typeparam>
        /// <param name="chain">The chain to pulse to completion.</param>
        /// <returns>The chain passed.</returns>
        public static TChain PulseToCompletion<TChain>(this TChain chain) where TChain : BaseChain<E, E> {
            if (chain == null)
                return null;

            while (!chain.Completed)
                chain.Pulse();

            return chain;
        }
    }
}