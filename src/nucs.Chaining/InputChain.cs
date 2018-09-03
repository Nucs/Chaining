using System;
using System.Diagnostics;
using Ebby;

namespace nucs.Chaining {
    public delegate bool EqualityComparison<in T>(T left, T right);

    /// Consuming E without reference.
    public delegate CE<TInput> CE<TInput>(InputChain<TInput> c);

    /// Refering Input E.
    public delegate CE<TInput> CRE<TInput>(InputChain<TInput> c, Reference<TInput> o);

    [DebuggerStepThrough]
    public class InputChain<TInput> : BaseChain<CRE<TInput>, CE<TInput>> {
        private readonly Reference<TInput> _reference = new Reference<TInput>(default); //has to be field to allow passing as 'ref'.

        /// <summary>
        ///     A comparer used to determine if the object inputted has changed and then updated <see cref="Reference{TValue}.HasChanged"/> using <see cref="object.Equals(object)"/>
        /// </summary>
        public static EqualityComparison<TInput> DefaultEqualityComparer { get; } = (left, right) => object.Equals(left, right);

        /// <summary>
        ///     A comparer used to determine if the object inputted has changed and then updated <see cref="Reference{TValue}.HasChanged"/>
        /// </summary>
        public EqualityComparison<TInput> EqualityComparer { get; set; } = DefaultEqualityComparer;

        /// <summary>
        ///     The current/last object that was consumed/is consumed.
        /// </summary>
        public TInput Current {
            get => _reference;
            set => _reference.Value = value;
        }

        /// <summary>
        ///     The reference currently being used by the chain.
        /// </summary>
        public Reference<TInput> Reference => _reference;

        /// <summary>
        ///     An equivalent to null return for type <typeparamref name="TDelegate"/>.
        /// </summary>
        protected override CE<TInput> NullReturningDelegate => chain => null;

        public static InputChain<TInput> Build(CRE<TInput> func) { return new InputChain<TInput>(func); }

        public static InputChain<TInput> Build(TInput initialInput, CRE<TInput> func) {
            var ret = new InputChain<TInput>(func);
            ret._reference.Value = initialInput;
            ret._reference.HasChanged = true;
            return ret;
        }

        public InputChain(CRE<TInput> script) : base(script) { }

        protected InputChain() { }

        /// <summary>
        ///     How should be <typeparamref name="TDelegate"/> be invoked?
        /// </summary>
        protected override CE<TInput> InvokeDelegate(CRE<TInput> @delegate) { return @delegate?.Invoke(this, _reference); }

        /// <summary>
        ///     How should be <typeparamref name="TReturnDelegate"/> be invoked?
        /// </summary>
        protected override CE<TInput> InvokeReturnedDelegate(CE<TInput> @delegate) { return @delegate?.Invoke(this); }

        /// <summary>
        ///     Enter into the chain once with the previously pulsed object.
        /// </summary>
        /// <remarks>Will enter the current chain command once.</remarks>
        public override void Pulse() {
            if (State != ChainState.Unstarted)
                _reference.HasChanged = false;
            base.Pulse();
        }

        /// <summary>
        ///     Enter into the chain once with the previous object.
        /// </summary>
        /// <remarks>Will enter the current chain command once.</remarks>
        public virtual void Pulse(TInput input) {
            if (State != ChainState.Unstarted) 
                _reference.HasChanged = EqualityComparer(_reference.Value, input) == false;
            _reference.Value = input;
            base.Pulse();
            _reference.HasChanged = false;
        }

        /// <summary>
        ///     Performs a reset on this chain, for in-script, use <see cref="BaseChain{TDelegate,TReturnedDelegate}.Restart"/>.
        /// </summary>
        public override void Reset() {
            base.Reset();
            _reference.Value = default;
            _reference.HasChanged = false;
        }

        /// Used to reference a new input via <see cref="InputChain{TInput}._holder"/>, when no new input - <see cref="InputChain{TInput}._holder"/> is null.
        public class Holder {
            public TInput Input { get; set; }

            public Holder(TInput input) { Input = input; }
        }
    }
}