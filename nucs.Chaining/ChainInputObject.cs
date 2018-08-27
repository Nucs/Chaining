namespace nucs.Chaining {
    /// <summary>
    ///     A reference object with capability to bind to a <see cref="IChain"/> to reset <see cref="Reference{T}.HasChanged"/> after executing a script.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ChainInputObject<T> : Reference<T> {
        /// <summary>
        ///     Binds to <see cref="chain"/> to reset when executing 
        /// </summary>
        /// <param name="chain"></param>
        /// <returns></returns>
        public ChainInputObject<T> Of(IChain chain) {
            chain.BindOnAfterCallingScript((executed, next) => HasChanged = false);
            return this;
        }

        /// <summary>
        ///     Updates the value, equivalent to directly changing <see cref="Reference{T}.Value"/>.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public ChainInputObject<T> Update(T value) {
            Value = value;
            return this;
        }

        public ChainInputObject(T value) : base(value) { }
        public ChainInputObject() : this(default) { }
    }
}