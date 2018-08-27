namespace nucs.Chaining {
    /// <summary>
    ///     A state of a chain machine.
    /// </summary>
    public enum ChainState {
        /// When <see cref="BaseChain{TDelegate,TReturnDelegate}.Complete"/> is called after the called pulse.
        Completed,
        /// When <see cref="BaseChain{TDelegate,TReturnDelegate}.Fail"/> is called after the called pulse.
        Failed,
        /// When an exception is thrown during a pulse.
        Faulty,
        /// When the chain has been pulsed and is currently running.
        Running,
        /// When the chain has been cancelled via <see cref="BaseChain{TDelegate,TReturnDelegate}.Dispose"/> or <see cref="BaseChain{TDelegate,TReturnDelegate}.Cancel"/>.
        Cancelled,
        /// When the chain has never been pulsed.
        Unstarted
    }
}
