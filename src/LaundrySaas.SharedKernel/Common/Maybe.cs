using System;
using System.Collections.Generic;

namespace LaundrySaas.SharedKernel.Common;

public class Maybe<TValue> : IEquatable<Maybe<TValue>>
{
    private readonly TValue _value;

    private Maybe(TValue value)
    {
        _value = value;
    }

    public bool HasValue => _value is not null;
    public bool HasNoValue => !HasValue;

    public TValue Value => HasValue
        ? _value
        : throw new InvalidOperationException("The value of a failure result cannot be accessed.");

    public static Maybe<TValue> From(TValue value) => new(value);
    public static Maybe<TValue> None => new(default!);

    public static implicit operator Maybe<TValue>(TValue value) => From(value);
    public static implicit operator TValue(Maybe<TValue> maybe) => maybe.Value;

    public bool Equals(Maybe<TValue>? other)
    {
        if (other is null) return false;
        if (HasNoValue && other.HasNoValue) return true;
        if (HasNoValue || other.HasNoValue) return false;
        return EqualityComparer<TValue>.Default.Equals(_value, other._value);
    }

    public override bool Equals(object? obj) =>
        obj is Maybe<TValue> other && Equals(other);

    public override int GetHashCode() =>
        HasValue ? EqualityComparer<TValue>.Default.GetHashCode(_value!) : 0;

    public override string ToString() =>
        HasValue ? _value!.ToString()! : "None";
}
