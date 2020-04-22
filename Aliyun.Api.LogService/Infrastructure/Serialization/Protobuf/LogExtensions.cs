//
// LogExtensions.cs
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
using Aliyun.Api.LogService.Domain.Log;

namespace Aliyun.Api.LogService.Infrastructure.Serialization.Protobuf
{
    public static class LogExtensions
    {
        public static LogInfo ToDomainModel(this Log proto)
            => proto == null
                ? null
                : new LogInfo
                {
                    Time = DateTimeOffset.FromUnixTimeSeconds(proto.Time),
                    Contents = proto.Contents?
                        .ToDictionary(x => x.Key, x => x.Value) // NOTE: potential `ArgumentException` when key duplicated.
                };

        public static LogGroupInfo ToDomainModel(this LogGroup proto)
            => proto == null
                ? null
                : new LogGroupInfo
                {
                    Topic = proto.Topic,
                    Source = proto.Source,
                    LogTags = proto.LogTags?
                        .ToDictionary(x => x.Key, x => x.Value), // NOTE: potential `ArgumentException` when key duplicated.
                    Logs = proto.Logs?
                        .Select(x => x.ToDomainModel())
                        .ToList()
                };

        public static IList<LogGroupInfo> ToDomainModel(this LogGroupList proto)
            => proto?.LogGroupList_?
                .Select(x => x.ToDomainModel())
                .ToList();


        public static Log ToProtoModel(this LogInfo domain)
            => domain == null
                ? null
                : new Log
                {
                    Time = (UInt32) domain.Time.ToUnixTimeSeconds(),
                    Contents =
                    {
                        domain.Contents?
                            .Select(kv => new Log.Types.Content
                            {
                                Key = kv.Key,
                                Value = kv.Value ?? String.Empty // Empty is allowed, but not null.
                            })
                    }
                };

        public static LogGroup ToProtoModel(this LogGroupInfo domain)
            => domain == null
                ? null
                : new LogGroup
                {
                    // https://github.com/aliyun/aliyun-log-dotnetcore-sdk/issues/14
                    Topic = domain.Topic ?? String.Empty, // Empty is allowed, but not null.
                    Source = domain.Source ?? String.Empty, // Empty is allowed, but not null.
                    LogTags =
                    {
                        domain.LogTags?
                            .Select(x => new LogTag
                            {
                                Key = x.Key,
                                Value = x.Value ?? String.Empty // Empty is allowed, but not null.
                            })
                        ?? Enumerable.Empty<LogTag>()
                    },
                    Logs =
                    {
                        domain.Logs?
                            .Select(x => x.ToProtoModel())
                        ?? Enumerable.Empty<Log>()
                    }
                };
    }
}
