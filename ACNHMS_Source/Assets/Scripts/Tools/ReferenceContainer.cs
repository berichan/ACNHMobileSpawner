using System;

public class ReferenceContainer<T>
{
    public T Value;

    public ReferenceContainer(T value)
    {
        Value = value;
    }

    public T UpdateValue(T newValue)
    {
        Value = newValue;
        return Value;
    }
}
