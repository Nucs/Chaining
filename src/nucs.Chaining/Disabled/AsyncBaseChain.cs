//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Linq;
//using System.Threading;
//using System.Threading.Tasks;
//using nucs.Collections;

//namespace Ebby {
//    /// <summary>
//    /// 
//    /// </summary>
//    /// <typeparam name="TDelegate">A delegate or object that can be invoked.</typeparam>
//    [DebuggerDisplay("{" + nameof(_debuggerDisplay) + ",nq}")]
//    public abstract class AsyncBaseChain<TDelegate> where TDelegate : class {
//        /// holds the names of delegates called via <see cref="NameScript"/>
//        private readonly SafeDictionary<string, TDelegate> _nameDelegates = new SafeDictionary<string, TDelegate>();

//        /// The current running script.
//        private TDelegate _currentScript => _stagesStack.Peek();

//        /// Flags to continue immediatly to <see cref="_currentScript"/>
//        private bool _immediateContinueRequested = false;

//        private bool _backwardsRequested;

//        /// The previous script to <see cref="_currentScript"/>, if no previous script, it'll be equal to <see cref="_currentScript"/>
//        private Stack<TDelegate> _stagesStack { get; } = new Stack<TDelegate>();

//        /// cancellation for dropping out when calling <see cref="Cancel"/>
//        private readonly CancellationTokenSource _source;

//        private TDelegate _initialScript;


//        #region Events

//        public delegate void OnScriptChangedHandler(TDelegate previous, TDelegate script);

//        /// <summary>
//        ///     When the script changes, equivalent to OnEnter in common state machines.
//        /// </summary>
//        public event OnScriptChangedHandler ScriptChanged;

//        public delegate void OnBeforeCallingScriptHandler(TDelegate script);

//        /// <summary>
//        ///     Fired right before execution of the current chain command.
//        /// </summary>
//        public event OnBeforeCallingScriptHandler BeforeCallingScript;

//        public delegate void OnPulseHandler(ChainState state);

//        /// <summary>
//        ///     Fired when the chain has been pulsed.
//        /// </summary>
//        public event OnPulseHandler OnPulse;

//        public void BindOnScriptChanged(OnScriptChangedHandler script) { ScriptChanged += script; }
//        public void BindOnBeforeCallingScript(OnBeforeCallingScriptHandler script) { BeforeCallingScript += script; }
//        public void BindOnPulse(OnPulseHandler script) { OnPulse += script; }

//        #endregion

//        #region Properties

//        /// <summary>
//        ///     Get cancellation token for this chain.
//        /// </summary>
//        /// <remarks>Returns a token using <see cref="CancellationTokenSource.Token"/></remarks>
//        public CancellationToken CancellationToken {
//            get {
//                lock (_source) {
//                    return _source.Token;
//                }
//            }
//        }

//        /// <summary>
//        ///     A <see cref="TDelegate"/> that returns emptry results which signify to repeat current script.
//        /// </summary>
//        public Task<TDelegate> Empty => NullReturningDelegate;

//        /// <summary>
//        ///     A <see cref="TDelegate"/> that returns emptry results which signify to repeat current script.
//        /// </summary>
//        public Task<TDelegate> Repeat => NullReturningDelegate;

//        /// <summary>
//        ///     The size of the stack.
//        /// </summary>
//        /// <remarks>When calling <see cref="Backwards"/>, the stack must not go below 1.</remarks>
//        public int StackSize => _stagesStack.Count;

//        /// <summary>
//        ///     The script that was passed via constructor.
//        /// </summary>
//        public TDelegate InitialScript {
//            get => _initialScript;
//            protected set {
//                _initialScript = value;
//                if (_stagesStack.Count != 0)
//                    _stagesStack.Clear();
//                else
//                    _stagesStack.Push(value);
//            }
//        }

//        /// <summary>
//        ///     The state of the script.
//        /// </summary>
//        public ChainState State { get; protected set; } = ChainState.Unstarted;

//        /// <summary>
//        ///     When the script has ended via <see cref="Complete"/>, should it auto reset via <see cref="Reset"/>?<br></br>
//        ///     By default: false.
//        /// </summary>
//        public bool AutoResetting { get; set; }

//        /// <summary>
//        ///     Has the script completed via <see cref="Complete"/>? note: Completed does not mean it was successful.
//        /// </summary>
//        public bool Completed { get; private set; }

//        /// <summary>
//        ///     Has the script marked failed via <see cref="Fail"/>? note: Completed does not mean it was successful.
//        /// </summary>
//        public bool Failed { get; private set; }

//        /// <summary>
//        ///     Is the script that is running now or will be run now is <see cref="InitialScript"/>.
//        /// </summary>
//        public bool IsScriptCurrentlyInitialScript => _currentScript == InitialScript;

//        #endregion

//        protected AsyncBaseChain(TDelegate script) : this() { InitialScript = script; }


//        protected AsyncBaseChain() { _source = new CancellationTokenSource(); }

//        #region Methods

//        #region Abstract

//        /// <summary>
//        ///     How should be <typeparamref name="TDelegate"/> be invoked?
//        /// </summary>
//        protected abstract TDelegate InvokeDelegate(Task<TDelegate> @delegate);

//        /// <summary>
//        ///     An equivalent to null return for type <typeparamref name="TDelegate"/>.
//        /// </summary>
//        protected abstract Task<TDelegate> NullReturningDelegate { get; }

//        #endregion

//        /// <summary>
//        ///     Marks this chain as completed.
//        /// </summary>
//        /// <param name="ignoreResetting">If <see cref="AutoResetting"/> enabled, should it be ignored?</param>
//        public Task<TDelegate> Complete(bool ignoreResetting = false) {
//            Completed = true;
//            if (AutoResetting && !ignoreResetting) {
//                Reset();
//                return null;
//            }

//            State = ChainState.Completed;
//            return NullReturningDelegate;
//        }

//        /// <summary>
//        ///     Basicly does a reset that will be accepted on the next pulse.
//        /// </summary>
//        /// <returns></returns>
//        public Task<TDelegate> Restart() {
//            Reset();
//            return null;
//        }

//        /// <summary>
//        ///     Basicly does a reset and immediatly continues to the chain that is set after the reset.
//        /// </summary>
//        /// <returns></returns>
//        public Task<TDelegate> RestartContinue() {
//            Reset();
//            return Continue();
//        }

//        /// if AutoResetting enabled, won't restart and <see cref="Failed"/> is marked true. (Note that <see cref="Complete"/> is also marked true).
//        public Task<TDelegate> Fail() {
//            Completed = true;
//            Failed = true;
//            State = ChainState.Failed;
//            return NullReturningDelegate;
//        }

//        /// <summary>
//        ///     Continue immediatly to <paramref name="toScript"/>. note: It is included into the history stack.
//        /// </summary>
//        /// <param name="toScript">The script to continue to without waiting to next pulse.</param>
//        public Task<TDelegate> Continue(Task<TDelegate> toScript) {
//            _immediateContinueRequested = true;
//            return toScript;
//        }

//        /// <summary>
//        ///     Similar to continue in a loop, Reenters current script immediatly. note: Doesn't include into the history stack.
//        /// </summary>
//        public Task<TDelegate> Continue() { return Continue(Repeat); }

//        /// <summary>
//        ///     Falls back to previous script/script.
//        /// </summary>
//        /// <param name="times">How many stacks to fall.</param>
//        /// <returns></returns>
//        /// <exception cref="InvalidOperationException">When <paramref name="times"/> exceeds the stacks</exception>
//        public TDelegate Backwards(int times = 1) {
//            if (_stagesStack.Count >= times) {
//                do {
//                    _stagesStack.Pop();
//                } while ((--times) > 0);

//                _backwardsRequested = true;
//                return null;
//            }

//            throw new InvalidOperationException($"Could not go backwards {times} times because it appears to be at or past root.");
//        }

//        /// <summary>
//        ///     Goes backwards immediatly without waiting for the next tick; Works similarly to <see cref="Backwards"/>.
//        /// </summary>
//        /// <param name="times"></param>
//        /// <returns></returns>
//        public TDelegate BackwardsContinue(int times = 1) {
//            if (_stagesStack.Count >= times) {
//                do {
//                    _stagesStack.Pop();
//                } while ((--times) > 0);

//                _immediateContinueRequested = true;
//                _backwardsRequested = true;
//                return null;
//            }

//            throw new InvalidOperationException($"Could not go backwards {times} times because it appears to be at or past root.");
//        }

//        /// <summary>
//        ///     Goes <see cref="Backwards"/> to the script that was named using <see cref="NameScript"/>.
//        /// </summary>
//        /// <exception cref="ArgumentException">The name is not registered or not in current script-stacktrace.</exception>
//        /// <exception cref="ArgumentNullException"><paramref name="scriptName"/> was passed null.</exception>
//        public TDelegate BackwardsTo(string scriptName) {
//            if (scriptName == null)
//                throw new ArgumentNullException(nameof(scriptName));
//            TDelegate @delegate = _nameDelegates[scriptName];
//            if (@delegate == null)
//                throw new ArgumentException($"Could not find script corresponding to name '{scriptName}'.");

//            var match = _stagesStack.ToArray().Select((item, index) => new {Item = item, Index = index}).FirstOrDefault(t => t.Item == @delegate);
//            if (match == null || match.Item == null)
//                throw new ArgumentException($"Could not find script corresponding to name '{scriptName}' in the script-stacktrace.");

//            //is it the head of the scripts right now.
//            if (match.Index == 0)
//                return Repeat;

//            return Backwards(match.Index);
//        }

//        /// <summary>
//        ///     Goes <see cref="Backwards"/> to the script that was named using <see cref="NameScript"/> immediatly without waiting for the next pulse.
//        /// </summary>
//        /// <exception cref="ArgumentException">The name is not registered or not in current script-stacktrace.</exception>
//        /// <exception cref="ArgumentNullException"><paramref name="scriptName"/> was passed null.</exception>
//        public TDelegate BackwardsToContinue(string scriptName) {
//            _immediateContinueRequested = true;
//            return BackwardsTo(scriptName);
//        }

//        /// <summary>
//        ///     Set or override the script corresponding to the passed script name.
//        /// </summary>
//        /// <exception cref="ArgumentNullException"><paramref name="name"/> was passed null.</exception>
//        public BaseChain<TDelegate> NameScript(string name) {
//            if (name == null)
//                throw new ArgumentNullException(nameof(name));
//            _nameDelegates[name] = _currentScript;
//            return this;
//        }

//        /// <summary>
//        ///     Enter into the chain once.
//        /// </summary>
//        /// <remarks>Will enter the current chain command once.</remarks>
//        public virtual void Pulse() {
//            lock (_source)
//                if (_source.IsCancellationRequested) {
//                    State = ChainState.Cancelled;
//                    OnPulse?.Invoke(State);
//                    return;
//                }

//            OnPulse?.Invoke(State);
//            _reenter:
//            switch (State) {
//                case ChainState.Unstarted:
//                    State = ChainState.Running;
//                    goto _reenter;
//                case ChainState.Running:
//                    try {
//                        var prev = _currentScript;

//                        BeforeCallingScript?.Invoke(_currentScript);
//                        var next = InvokeDelegate(_currentScript);

//                        lock (_source)
//                            if (_source.IsCancellationRequested) {
//                                State = ChainState.Cancelled;
//                                break;
//                            }


//                        if (State == ChainState.Unstarted) //has been resetted.
//                            break;

//                        if (_immediateContinueRequested) {
//                            if (next != Repeat) {
//                                //it is continue into a different script with backward support.
//                                if (!_backwardsRequested)
//                                    _stagesStack.Push(next);
//                                ScriptChanged?.Invoke(prev, _currentScript);
//                                goto _reenter;
//                            } else { //next == Repeat
//                                //it is loop-like immediate continue.
//                                goto _reenter;
//                            }
//                        }

//                        if (next == null || Completed || next == Repeat)
//                            return;

//                        //new next, push to stack.
//                        if (!_backwardsRequested)
//                            _stagesStack.Push(next);
//                        ScriptChanged?.Invoke(prev, _currentScript);
//                        break;
//                    } finally {
//                        _immediateContinueRequested = false;
//                        _backwardsRequested = false;
//                    }
//                case ChainState.Completed:
//                case ChainState.Failed:
//                case ChainState.Cancelled:
//                    break;

//                default:
//                    throw new ArgumentOutOfRangeException();
//            }
//        }

//        /// <summary>
//        ///     Performs a reset on this chain, for in-script, use <see cref="Restart"/>.
//        /// </summary>
//        public virtual void Reset() {
//            //reset to the original func.
//            _stagesStack.Clear();
//            _stagesStack.Push(InitialScript);

//            ScriptChanged = null;
//            Failed = false;
//            Completed = false;

//            _nameDelegates.Clear();

//            State = ChainState.Unstarted;
//        }

//        /// <summary>
//        ///     Cancels this chain and prevents any <see cref="Pulse"/> after this has been called.
//        /// </summary>
//        public void Cancel() {
//            lock (_source)
//                if (!_source.IsCancellationRequested)
//                    _source.Cancel();

//            State = ChainState.Cancelled;
//        }

//        #endregion

//        #region IDisposable

//        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
//        public void Dispose() {
//            lock (_source) { //equivalent to Cancel()
//                if (!_source.IsCancellationRequested)
//                    _source.Cancel();
//                _source.Dispose();
//            }
//        }

//        #endregion

//        private string _debuggerDisplay => $"{this.GetType().Name}: {State} (StackSize {StackSize}) : {(_nameDelegates.FindKeyByValue(_currentScript) ?? _currentScript?.ToString() ?? this.GetType().FullName)}";
//    }
//}