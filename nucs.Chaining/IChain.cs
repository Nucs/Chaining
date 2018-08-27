using System;
using System.Threading;

namespace nucs.Chaining {
    public interface IChain {
        /// <summary>
        ///     When the script changes, equivalent to OnEnter in common state machines.
        /// </summary>
        event OnScriptChangedHandler ScriptChanged;

        /// <summary>
        ///     Fired right before execution of the current chain command.
        /// </summary>
        event OnBeforeCallingScriptHandler BeforeCallingScript;

        /// <summary>
        ///     Fired when the chain has been pulsed.
        /// </summary>
        event OnPulseHandler OnPulse;

        /// <summary>
        ///     Get cancellation token for this chain.
        /// </summary>
        /// <remarks>Returns a token using <see cref="CancellationTokenSource.Token"/></remarks>
        CancellationToken CancellationToken { get; }

        /// <summary>
        ///     A <see cref="TDelegate"/> that returns emptry results which signify to repeat current script.
        /// </summary>
        Delegate Empty { get; }

        /// <summary>
        ///     A <see cref="TDelegate"/> that returns emptry results which signify to repeat current script.
        /// </summary>
        Delegate Repeat { get; }

        /// <summary>
        ///     The size of the stack.
        /// </summary>
        /// <remarks>When calling <see cref="Backwards"/>, the stack must not go below 1.</remarks>
        int StackSize { get; }

        /// <summary>
        ///     The script that was passed via constructor.
        /// </summary>
        Delegate InitialScript { get; }

        /// <summary>
        ///     The state of the script.
        /// </summary>
        ChainState State { get; }

        /// <summary>
        ///     When the script has ended via <see cref="Complete"/>, should it auto reset via <see cref="Reset"/>?<br></br>
        ///     By default: false.
        /// </summary>
        bool AutoResetting { get; set; }

        /// <summary>
        ///     Has the script completed via <see cref="Complete"/>? note: Completed does not mean it was successful.
        /// </summary>
        bool Completed { get; }

        /// <summary>
        ///     Has the script marked failed via <see cref="Fail"/>? note: Completed does not mean it was successful.
        /// </summary>
        bool Failed { get; }

        /// <summary>
        ///     Is the script that is running now or will be run now is <see cref="InitialScript"/>.
        /// </summary>
        bool IsScriptCurrentlyInitialScript { get; }

        void BindOnScriptChanged(OnScriptChangedHandler script);
        void BindOnBeforeCallingScript(OnBeforeCallingScriptHandler script);
        void BindOnAfterCallingScript(OnAfterCallingScriptHandler script);
        void BindOnPulse(OnPulseHandler script);

        /// <summary>
        ///     Marks this chain as completed.
        /// </summary>
        /// <param name="ignoreResetting">If <see cref="BaseChain{TDelegate,Delegate}.AutoResetting"/> enabled, should it be ignored?</param>
        Delegate Complete(bool ignoreResetting = false);

        /// <summary>
        ///     Basicly does a reset that will be accepted on the next pulse.
        /// </summary>
        /// <returns></returns>
        Delegate Restart();

        /// <summary>
        ///     Basicly does a reset and immediatly continues to the chain that is set after the reset.
        /// </summary>
        /// <returns></returns>
        Delegate RestartContinue();

        /// if AutoResetting enabled, won't restart and <see cref="BaseChain{TDelegate,Delegate}.Failed"/> is marked true. (Note that <see cref="BaseChain{TDelegate,Delegate}.Complete"/> is also marked true).
        Delegate Fail();

        /// <summary>
        ///     Continue immediatly to <paramref name="toScript"/>. note: It is included into the history stack.
        /// </summary>
        /// <param name="toScript">The script to continue to without waiting to next pulse.</param>
        Delegate Continue(Delegate toScript);

        /// <summary>
        ///     Similar to continue in a loop, Reenters current script immediatly. note: Doesn't include into the history stack.
        /// </summary>
        Delegate Continue();

        /// <summary>
        ///     Falls back to previous script/script.
        /// </summary>
        /// <param name="times">How many stacks to fall.</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">When <paramref name="times"/> exceeds the stacks</exception>
        Delegate Backwards(int times = 1);

        /// <summary>
        ///     Goes backwards immediatly without waiting for the next tick; Works similarly to <see cref="BaseChain{TDelegate,Delegate}.Backwards"/>.
        /// </summary>
        /// <param name="times"></param>
        /// <returns></returns>
        Delegate ContinueBackwards(int times = 1);

        /// <summary>
        ///     Goes <see cref="BaseChain{TDelegate,Delegate}.Backwards"/> to the script that was named using <see cref="BaseChain{TDelegate,Delegate}.NameScript"/>.
        /// </summary>
        /// <exception cref="ArgumentException">The name is not registered or not in current script-stacktrace.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="scriptName"/> was passed null.</exception>
        Delegate BackwardsTo(string scriptName);

        /// <summary>
        ///     Goes <see cref="BaseChain{TDelegate,Delegate}.Backwards"/> to the script that was named using <see cref="BaseChain{TDelegate,Delegate}.NameScript"/> immediatly without waiting for the next pulse.
        /// </summary>
        /// <exception cref="ArgumentException">The name is not registered or not in current script-stacktrace.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="scriptName"/> was passed null.</exception>
        Delegate ContinueBackwardsTo(string scriptName);

        /// <summary>
        ///     Set or override the script corresponding to the passed script name.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="name"/> was passed null.</exception>
        IChain NameScript(string name);

        /// <summary>
        ///     Enter into the chain once.
        /// </summary>
        /// <remarks>Will enter the current chain command once.</remarks>
        void Pulse();

        /// <summary>
        ///     Performs a reset on this chain, for in-script, use <see cref="BaseChain{TDelegate,TReturnDelegate}.Restart"/>.
        /// </summary>
        void Reset();

        /// <summary>
        ///     Cancels this chain and prevents any <see cref="BaseChain{TDelegate,TReturnDelegate}.Pulse"/> after this has been called.
        /// </summary>
        void Cancel();

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        void Dispose();
    }
}