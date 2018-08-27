using System.Diagnostics;
using Ebby;

namespace nucs.Chaining {

    public delegate E E(Chain c);

    [DebuggerStepThrough]
    public class Chain : BaseChain<E, E> {
        public static Chain Build(E func) { return new Chain(func); }

        public Chain(E script) : base(script) { }
        protected Chain() { }

        #region Overrides of BaseChain<E>

        protected override E InvokeDelegate(E @delegate) { return @delegate?.Invoke(this); }

        /// <summary>
        ///     How should be <typeparamref name="TDelegate"/> be invoked?
        /// </summary>
        protected override E InvokeReturnedDelegate(E @delegate) {return @delegate?.Invoke(this);}

        protected override E NullReturningDelegate { get; } = c => null;

        #endregion
    }
}