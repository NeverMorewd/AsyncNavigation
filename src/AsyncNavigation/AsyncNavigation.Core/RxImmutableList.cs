/*

The MIT License (MIT)

Copyright (c) .NET Foundation and Contributors

All rights reserved.

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

*/
namespace AsyncNavigation.Core;

/// <summary>
/// https://github.com/dotnet/reactive/blob/main/Rx.NET/Source/src/System.Reactive/Internal/ImmutableList.cs
/// </summary>
/// <typeparam name="T"></typeparam>
internal sealed class RxImmutableList<T>
{
    public static readonly RxImmutableList<T> Empty = new();

    private readonly T[] _data;

    private RxImmutableList() => _data = [];

    public RxImmutableList(T[] data) => _data = data;

    public T[] Data => _data;

    public RxImmutableList<T> Add(T value)
    {
        var newData = new T[_data.Length + 1];

        Array.Copy(_data, newData, _data.Length);
        newData[_data.Length] = value;

        return new RxImmutableList<T>(newData);
    }

    public RxImmutableList<T> Remove(T value)
    {
        var i = Array.IndexOf(_data, value);
        if (i < 0)
        {
            return this;
        }

        var length = _data.Length;
        if (length == 1)
        {
            return Empty;
        }

        var newData = new T[length - 1];

        Array.Copy(_data, 0, newData, 0, i);
        Array.Copy(_data, i + 1, newData, i, length - i - 1);

        return new RxImmutableList<T>(newData);
    }
}
