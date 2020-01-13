using System;

namespace HC.TFPlugin
{
    public static class Computed
    {
        public static Computed<T> Create<T>(T value) => new Computed<T>(value);
    }

    public interface IComputed
    {
        bool IsKnown { get; }

        object GetValue();
    }

    public struct Computed<T> : IComputed
    {
        public static readonly Computed<T> Unknown = new Computed<T>();

        private T _Value;
        private bool _IsKnown;

        internal Computed(T value)
        {
            _Value = value;
            _IsKnown = true;
        }

        public T Value => _IsKnown ? _Value : throw new Exception("value is unknown");

        public T ValueOrDefault() => _IsKnown ? _Value : default(T);

        public T ValueOrDefault(T @default) => _IsKnown ? _Value : @default;

        public bool IsKnown => _IsKnown;

        object IComputed.GetValue() => Value;

        public static implicit operator Computed<T>(T value)
        {
            return new Computed<T>(value);
        }

        public static implicit operator T(Computed<T> computed)
        {
            return computed._IsKnown ? computed._Value : default(T);
        }
    }
}