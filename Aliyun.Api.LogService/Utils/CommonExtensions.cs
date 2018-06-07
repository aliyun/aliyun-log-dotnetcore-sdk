//
// CommonExtensions.cs
//
// Author:
//       MiNG <developer@ming.gz.cn>
//
// Copyright (c) 2018 Alibaba Cloud
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace Aliyun.Api.LogService.Utils
{
    internal static class CommonExtensions
    {
        internal static Boolean IsEmpty(this String source)
        {
            return String.IsNullOrEmpty(source);
        }

        internal static Boolean IsNotEmpty(this String source)
        {
            return !String.IsNullOrEmpty(source);
        }

        internal static Boolean IsEmpty<T>(this T[] source)
        {
            return source == null || source.Length == 0;
        }

        internal static Boolean IsNotEmpty<T>(this T[] source)
        {
            return source != null && source.Length != 0;
        }

        internal static Boolean IsEmpty<T>(this IEnumerable<T> source)
        {
            return source == null || !source.Any();
        }

        internal static Boolean IsNotEmpty<T>(this IEnumerable<T> source)
        {
            return source != null && source.Any();
        }

        internal static IEnumerable<KeyValuePair<String, String>> ToEnumerable(this NameValueCollection source)
        {
            return source.AllKeys
                .SelectMany(key => source.GetValues(key), (key, value) => new KeyValuePair<String, String>(key, value));
        }

        /// <summary>
        /// Freeze the IEnumerable to avoid re-evaluating on <paramref name="source"/>.
        /// </summary>
        /// <param name="source">The source to freeze.</param>
        /// <typeparam name="T">The item type in source.</typeparam>
        /// <returns>If source has already frozen, return itself; otherwize <see cref="Enumerable.ToArray{TSource}"/> will be applied.</returns>
        internal static IEnumerable<T> Freeze<T>(this IEnumerable<T> source)
        {
            switch (source)
            {
                case null:
                case Array _:
                case ICollection<T> _:
                case IReadOnlyCollection<T> _:
                {
                    return source;
                }

                default:
                {
                    return source.ToArray();
                }
            }
        }

        internal static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> source, TKey key)
        {
            if (source == null)
            {
                return default;
            }

            return source.TryGetValue(key, out var value) ? value : default;
        }
    }
}
