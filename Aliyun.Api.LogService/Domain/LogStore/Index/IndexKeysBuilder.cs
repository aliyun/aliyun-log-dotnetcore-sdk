//
// IndexKeysBuilder.cs
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
using System.Linq;
using Aliyun.Api.LogService.Utils;

namespace Aliyun.Api.LogService.Domain.LogStore.Index
{
    public class IndexKeysBuilder
    {
        private readonly Boolean allowJson;

        private readonly IDictionary<String, IndexKeyInfo> keys = new Dictionary<String, IndexKeyInfo>();

        public IndexKeysBuilder() : this(true)
        {
            // Empty constructor.
        }

        private IndexKeysBuilder(Boolean allowJson)
        {
            this.allowJson = allowJson;
        }

        public IndexKeysBuilder AddText(String key, params Char[] token)
        {
            return this.AddText(key, token.AsEnumerable());
        }

        public IndexKeysBuilder AddText(String key, IEnumerable<Char> token, Boolean? caseSensitive = null, Boolean? chn = null)
        {
            var frozenToken = token?.ToArray(); // Avoid recalculate the non-reentranceable IEnumerable.
            Ensure.NotEmpty(frozenToken, nameof(token));

            var textKeyInfo = new IndexTextKeyInfo(frozenToken)
            {
                CaseSensitive = caseSensitive,
                Chn = chn
            };

            this.keys.Add(key, textKeyInfo);

            return this;
        }

        public IndexKeysBuilder AddLong(String key, Boolean? docValue = null, String alias = null)
        {
            var longKeyInfo = new IndexLongKeyInfo
            {
                DocValue = docValue,
                Alias = alias
            };

            this.keys.Add(key, longKeyInfo);

            return this;
        }

        public IndexKeysBuilder AddDouble(String key, Boolean? docValue = null, String alias = null)
        {
            var doubleKeyInfo = new IndexDoubleKeyInfo
            {
                DocValue = docValue,
                Alias = alias
            };

            this.keys.Add(key, doubleKeyInfo);

            return this;
        }

        public IndexKeysBuilder AddJson(String key, Int32 maxDepth, params Char[] token)
        {
            return this.AddJson(key, token, maxDepth);
        }

        public IndexKeysBuilder AddJson(String key, IEnumerable<Char> token, Int32 maxDepth,
            Boolean? chn = null, Boolean? indexAll = null, Action<IndexKeysBuilder> jsonKeys = null)
        {
            if (!this.allowJson)
            {
                throw new InvalidOperationException("json index info is not support in current state.");
            }

            var frozenToken = token?.ToArray(); // Avoid recalculate the non-reentranceable IEnumerable.
            Ensure.NotEmpty(frozenToken, nameof(token));

            IDictionary<String, IndexKeyInfo> subKeys;
            if (jsonKeys != null)
            {
                var subBuilder = new IndexKeysBuilder(false);
                jsonKeys(subBuilder);
                subKeys = subBuilder.Build();
            } else
            {
                subKeys = null;
            }

            var jsonKeyInfo = new IndexJsonKeyInfo(frozenToken, maxDepth)
            {
                Chn = chn,
                IndexAll = indexAll,
                JsonKeys = subKeys
            };

            this.keys.Add(key, jsonKeyInfo);

            return this;
        }

        public IDictionary<String, IndexKeyInfo> Build()
        {
            return new Dictionary<String, IndexKeyInfo>(this.keys);
        }
    }
}
