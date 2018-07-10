# Aliyun LogService SDK for .NET Core

[![NuGet](https://img.shields.io/nuget/v/Aliyun.Api.LogService.svg)](https://www.nuget.org/packages/Aliyun.Api.LogService)

## 简介

阿里云 LogService Rest API 的 .NET Core SDK。

基于 [Microsoft.AspNet.WebApi.Client](https://www.nuget.org/packages/Microsoft.AspNet.WebApi.Client) on [.NetStandard 2.0](https://github.com/dotnet/standard/blob/master/docs/versions/netstandard2.0.md) 构建。

不支持 **.NetStandard 2.0** 以下的平台！

### 平台支持

关于 .NetStandard 的实现支持请参考 https://docs.microsoft.com/en-us/dotnet/standard/net-standard ，当前支持：

- .NET Core 2.0
- .NET Framework (with .NET Core 1.x SDK) 4.6.2
- .NET Framework (with .NET Core 2.0 SDK) 4.6.1
- Mono 5.4
- Xamarin.iOS 10.14
- Xamarin.Mac 3.8
- Xamarin.Android 8.0
- Universal Windows Platform 10.0.16299

### 功能依赖

| 功能 | 依赖 |
| :-- | :-- |
| Http | [Microsoft.AspNet.WebApi.Client &bull; 5.2.4](https://www.nuget.org/packages/Microsoft.AspNet.WebApi.Client/5.2.4) |
| Json | [Json.Net &bull; 11.0.2](https://www.nuget.org/packages/Newtonsoft.Json/11.0.2) |
| Protobuf | [Google.Protobuf &bull; 3.5.1](https://www.nuget.org/packages/Google.Protobuf/3.5.1) |
| Zlib | [Iconic.Zlib.NetStandard &bull; 1.0.0](https://www.nuget.org/packages/Iconic.Zlib.Netstandard/1.0.0) |
| Lz4 | [lz4net &bull; 1.0.15.93](https://www.nuget.org/packages/lz4net/1.0.15.93) |

## 快速开始

1. 构建`ILogServiceClient`，此实例所有方法均为线程安全，支持通过**单例（singleton）模式**使用：

    ```csharp
    LogServiceClientBuilders.HttpBuilder
        .Endpoint("<endpoint>", "<projectName>")
        .Credential("<accessKeyId>", "<accessKey>")
        .Build();
    ```

2. 使用client访问异步访问接口（请注意`await`），有两种方法：

    - 直接使用接口方法，需要传入 `XxxRequest` 的请求对象，此方式的好处是**有利于二次封装时参数传递**：

        ```csharp
        // 调用方法时需要传入对应的 Request 对象。
        var getLogsResponse = await client.GetLogsAsync(
            // 「必填参数」会在 Request 构造器中列出，并且不可set；
            new GetLogsRequest("example-logstore", from, to)
            {
                // 「可选参数」不会在 Request 构造器中列出，可通过setter设置。
                Offset = 1,
                Line = 100,
            });
        ```
    
    - 使用扩展方法调用，所有（简单类型的）请求参数都会被平铺到方法入参上，非必传参数使用可选参数表示，此方式的好处是**代码可读性高，调用简单**：

        ```csharp
        using Aliyun.Api.LogService; // 使用扩展方法时如ide无提示请注意引入命名空间。

        var getLogsResponse = await client.GetLogsAsync
        (
            // 「必填参数」作为方法的普通必须参数
            "example-logstore",
            from,
            to,
            
            // 「可选参数」作为方法的可选参数，可通过命名参数方式指定
            offset: 1,
            line: 10
        );
        ```

    > 注意：在 Asp.NET 及 UI 环境中请勿使用同步方式直接访问 `Task.Result` 属性否则会造成循环等待。

3. 处理响应报文

    获取到响应对象后，必须先判断 `IsSuccess` 或调用 `EnsureSuccess()` 方法后才能访问 `Result` 属性，否则，可能会在访问 `Result` 时抛出 `NullReferenceException`。

    - 处理业务异常

        在部分接口中可能存在需要用于手动处理的业务错误，此时可以通过 `Error` 属性获取错误信息。

        ```csharp
        using Aliyun.Api.LogService;
        using Aliyun.Api.LogService.Infrastructure.Protocol;
    
        var response = await client.GetLogsAsync(...);

        if (!response.IsSuccess)
        {
            var errorCode = response.Error.ErrorCode;
            var errorMessage = response.Error.ErrorMessage;

            if (errorCode == ErrorCode.SignatureNotMatch /* SDK中预定义的错误码 */)
            {
                // 在这里处理业务可处理的错误。。。。。。
                Logger.Error("Signature not match, {0}.", errorMessage);
            } else if (errorCode == "ParameterInvalid" /* 业务相关特殊的SDK中未定义的错误码 */)
            {
                // 在这里处理业务可处理的错误。。。。。。
                Logger.Error("Parameter invalid, {0}.", errorMessage);
            } else
            {
                // 处理不到的异常请务必重新抛出错误！
            }

            throw new YourBizException("这里可以是系统的业务异常。" + response.Error /* 最好带上服务返回的错误信息以便调试 */);
        }
        
        // 此处获取Result是安全的。
        var result = response.Result;
        ```

    - 直接抛出业务异常

        大多数情况下，服务返回的错误码并没有业务含义，业务中无法处理，让其直接抛出即可。此时可调用 `EnsureSuccess()` 方法，在 `IsSuccess` 为 `false` 的情况下会抛出包含错误信息的 `LogServiceException`，**在二次封装场景下尤其有用**。

        ```csharp
        using Aliyun.Api.LogService;
        using Aliyun.Api.LogService.Infrastructure.Protocol;

        public async Task Caller()
        {
            try
            {
                return await Wrapper();
            } catch (LogServiceException e)
            {
                // 捕获 `LogServiceException` 后可获取如下信息： 
                Console.WriteLine($@"
                    RequestId (请求ID): {e.RequestId}
                    ErrorCode (错误码): {e.ErrorCode}
                    ErrorMessage (错误消息): {e.ErrorMessage}");
                throw;
            }
        }

        private async Task<IResponse> Wrapper()
        {
            var response = await client.GetLogsAsync(...);
    
            return response
                // 此处如果请求返回结果不成功会抛出 `LogServiceException`。
                .EnsureSuccess()
                .Result;
        }
        ```

## 更多

更多信息，请参考[wiki](https://github.com/aliyun/aliyun-log-dotnetcore-sdk/wiki)。
