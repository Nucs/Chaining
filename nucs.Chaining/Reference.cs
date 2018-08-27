using System;
using System.Collections.Generic;

namespace nucs.Chaining {
    /// <summary>
    ///     Simple class that holds a <typeparamref name="TValue"/>.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    public class Reference<TValue> : IEquatable<Reference<TValue>> {
        protected TValue _value;

        public virtual TValue Value {
            get => _value;
            set {
                _value = value;
                HasChanged = true;
            }
        }

        /// <summary>
        ///     Has value been changed?
        /// </summary>
        public bool HasChanged { get; set; }
        public Reference(TValue value) { _value = value; }

        public static implicit operator TValue(Reference<TValue> @ref) {
            return @ref == null ? default : @ref.Value;
        }

        /// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// <see langword="true" /> if the current object is equal to the <paramref name="other" /> parameter; otherwise, <see langword="false" />.</returns>
        public bool Equals(Reference<TValue> other) {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return EqualityComparer<TValue>.Default.Equals(Value, other.Value);
        }

        /// <summary>Determines whether the specified object is equal to the current object.</summary>
        /// <param name="obj">The object to compare with the current object. </param>
        /// <returns>
        /// <see langword="true" /> if the specified object  is equal to the current object; otherwise, <see langword="false" />.</returns>
        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != this.GetType())
                return false;
            return Equals((Reference<TValue>) obj);
        }

        /// <summary>Serves as the default hash function. </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode() {
            return EqualityComparer<TValue>.Default.GetHashCode(Value);
        }

        /// <summary>Returns a value that indicates whether the values of two <see cref="T:nucs.Chaining.Reference`1" /> objects are equal.</summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>true if the <paramref name="left" /> and <paramref name="right" /> parameters have the same value; otherwise, false.</returns>
        public static bool operator ==(Reference<TValue> left, Reference<TValue> right) { return Equals(left, right); }

        /// <summary>Returns a value that indicates whether two <see cref="T:nucs.Chaining.Reference`1" /> objects have different values.</summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>true if <paramref name="left" /> and <paramref name="right" /> are not equal; otherwise, false.</returns>
        public static bool operator !=(Reference<TValue> left, Reference<TValue> right) { return !Equals(left, right); }
    }
}