using System;

namespace nucs.Chaining {
    public interface IReference<TValue> : IEquatable<IReference<TValue>> {
        /// <summary>
        ///     The value of this reference.
        /// </summary>
        TValue Value { get; set; }

        /// <summary>
        ///     Has value been changed?
        /// </summary>
        bool HasChanged { get; set; }
    }
}