using System.Diagnostics;
using nucs.Chaining;

namespace Ebby {
    [DebuggerStepThrough]
    public static class ChainExtensions {
        /// <summary>
        ///     Sets this chain to auto reset when it ends with <see cref="ChainState.Completed"/>.
        /// </summary>
        /// <param name="value">true for autoreset, false for nono.</param>
        /// <returns></returns>
        public static Chain AutoResetting(this Chain c, bool value = true) {
            c.AutoResetting = value;
            return c;
        }

        /// <summary>
        ///     Resets this chain via <see cref="E.Reset()"/> and returns self.
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static Chain Reset(this Chain c) {
            c.Reset();
            return c;
        }

        /// <summary>
        ///     Wraps <paramref name="script"/> so that <paramref name="onExit"/> will be called after it.
        /// </summary>
        /// <param name="script">The script to run first</param>
        /// <param name="onExit">The script to run after <paramref name="script"/> finishes.</param>
        /// <returns>onExit() ?? script()</returns>
        public static E OnExit(this E script, E onExit) { return AndThen(script, onExit); }

        /// <summary>
        ///     Wraps <paramref name="first"/> so that <paramref name="then"/> will be called after it.
        /// </summary>
        /// <param name="first">The script to run first</param>
        /// <param name="then">The script to run after <paramref name="first"/> finishes.</param>
        /// <returns>first() ?? then()</returns>
        public static E AndThen(this E first, E then) {
            return chain => {
                var a = first?.Invoke(chain);
                var b = then?.Invoke(chain);
                return a ?? b;
            };
        }
    }
}