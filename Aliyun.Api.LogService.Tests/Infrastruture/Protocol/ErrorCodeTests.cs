//
// ErrorCodeTests.cs
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
using Aliyun.Api.LogService.Infrastructure.Protocol;
using Xunit;

namespace Aliyun.Api.LogService.Tests.Infrastruture.Protocol
{
    public class ErrorCodeTests
    {
        [Fact]
        public void TestEqualsViaOperator()
        {
            // Same
            Assert.True(ErrorCode.SdkInternalError == ErrorCode.SdkInternalError);
            Assert.False(ErrorCode.SdkInternalError != ErrorCode.SdkInternalError);

            // Not same
            Assert.False(ErrorCode.SdkInternalError == ErrorCode.InternalServerError);
            Assert.True(ErrorCode.SdkInternalError != ErrorCode.InternalServerError);

            // Compare to string (value equals)
            Assert.True(ErrorCode.SdkInternalError == "SdkInternalError");
            Assert.False(ErrorCode.SdkInternalError != "SdkInternalError");

            // Compare to string (value not equals)
            Assert.False(ErrorCode.SdkInternalError == "Foo");
            Assert.True(ErrorCode.SdkInternalError != "Foo");

            // Be compared to string (value equals)
            Assert.True("SdkInternalError" == ErrorCode.SdkInternalError);
            Assert.False("SdkInternalError" != ErrorCode.SdkInternalError);

            // Be compared to string (value not equals)
            Assert.False("Foo" == ErrorCode.SdkInternalError);
            Assert.True("Foo" != ErrorCode.SdkInternalError);
        }

        [Fact]
        public void TestEqualsViaEqualsMethod()
        {
            // Same
            Assert.True(ErrorCode.SdkInternalError.Equals(ErrorCode.SdkInternalError));

            // Not same
            Assert.False(ErrorCode.SdkInternalError.Equals(ErrorCode.InternalServerError));

            // Compare to string (value equals)
            Assert.True(ErrorCode.SdkInternalError.Equals("SdkInternalError"));

            // Compare to string (value not equals)
            Assert.False(ErrorCode.SdkInternalError.Equals("Foo"));

            // Be compared to string (value equals)
            Assert.True("SdkInternalError".Equals(ErrorCode.SdkInternalError));

            // Be compared to string (value not equals)
            Assert.False("Foo".Equals(ErrorCode.SdkInternalError));
        }

        [Fact]
        public void TestEqualsViaObjectEqualsMethod()
        {
            // Same
            Assert.True(Equals(ErrorCode.SdkInternalError, ErrorCode.SdkInternalError));

            // Not same
            Assert.False(Equals(ErrorCode.SdkInternalError, ErrorCode.InternalServerError));

            // Compare to string (value equals)
            Assert.True(Equals(ErrorCode.SdkInternalError, "SdkInternalError"));

            // Compare to string (value not equals)
            Assert.False(Equals(ErrorCode.SdkInternalError, "Foo"));

            // Be compared to string (value equals)
            // NOTE: Cannot using string as leading argument.
            // Assert.True(Equals("SdkInternalError", ErrorCode.SdkInternalError));

            // Be compared to string (value not equals)
            // NOTE: Cannot using string as leading argument.
            // Assert.False(Equals("Foo", ErrorCode.SdkInternalError));
        }

        [Fact]
        public void TestCast()
        {
            // Implicit cast is ok.
            String stringValue = ErrorCode.SdkInternalError;

            // The implicit casted value is same to `Code` property.
            Assert.Same(ErrorCode.SdkInternalError.Code, stringValue);

            // Implicit cast is ok.
            ErrorCode errorCode = "SdkInternalError";

            // Cast String to ErrorCode will be load from cache.
            Assert.Same(ErrorCode.SdkInternalError, errorCode);
            Assert.True(errorCode.IsKnownErrorCode());
            
            // Implicit cast any string is ok.
            ErrorCode errorCodeNew = "Foo";
            ErrorCode errorCodeNew2 = "Foo";
            Assert.False(errorCodeNew.IsKnownErrorCode());
            Assert.False(errorCodeNew2.IsKnownErrorCode());
            // Everytime implicit cast from string will create a new ErrorCode instance.
            Assert.Equal(errorCodeNew, errorCodeNew2);
            Assert.NotSame(errorCodeNew, errorCodeNew2);
        }

        [Fact]
        public void TestUsingAsHashableKey()
        {
            var errorCodeKeyedDictionary = new Dictionary<ErrorCode, Object>();
            var stringKeyedDictionary = new Dictionary<String, Object>();
            var objectKeyedDictionary = new Dictionary<Object, Object>();

            errorCodeKeyedDictionary[ErrorCode.SdkInternalError] = "from code";
            errorCodeKeyedDictionary["SdkInternalError"] = "from string";

            stringKeyedDictionary[ErrorCode.SdkInternalError] = "from code";
            stringKeyedDictionary["SdkInternalError"] = "from string";

            objectKeyedDictionary[ErrorCode.SdkInternalError] = "from code";
            objectKeyedDictionary["SdkInternalError"] = "from string";

            Assert.Equal("from string", errorCodeKeyedDictionary.Single().Value);
            Assert.Equal("from string", stringKeyedDictionary.Single().Value);
            Assert.Equal("from string", objectKeyedDictionary.Single().Value);
        }
    }
}
