# Aliyun LogServie SDK for .NET Core 开发指南

## 依赖

项目基于 .Net Standard 2.0 构建。

其中 DotNetZip 无直接支持 .Net Standard 2.0 的依赖包，暂用基于 .Net Framework 4.6.1 的包，经过验证在 MacOS X 环境下可用。

## 项目结构

    .
    ├── Aliyun.Api.LogService/.............SDK源码主项目
    │   ├── Domain/...........................主要领域模型
    │   ├── Infrastructure/...................SDK基础设施
    │   │   ├── Authentication/...................验证相关设施
    │   │   ├── Protocol/.........................底层协议相关设施
    │   │   │   └── Http/.............................HTTP协议实现
    │   │   └── Serialization/....................序列化相关设施
    │   │       └── Protobuf/.........................Protobuf序列化实现
    │   ├── Properties/.......................程序集自身相关信息
    │   │   └── AssemblyInfo.cs
    │   └── Utils/.............................内部工具类
    ├── Aliyun.Api.LogService.Benchmark/...SDK性能指标项目
    ├── Aliyun.Api.LogService.Examples/....SDK示例项目
    ├── Aliyun.Api.LogService.Tests/.......SDK单元测试项目
    └── docs/..............................文档存放处

## 开发

### Protobuf

- `Log.proto`编译

  ```bash
  $ cd Aliyun.Api.LogService/Infrastructure/Serialization/Protobuf
  $ protoc --csharp_out=. --csharp_opt=file_extension=.Generated.cs Log.proto
  ```
  
- proto2兼容性

  在阿里云服务中使用的是proto2，由于在proto2官方没有对应的C# Stub生成支持，只能使用proto3协议生成stub。

  在proto2中，`string`类型字段在为空（非`null`）时回写入字段的标头；但在proto3中会直接跳过整个字段，从而导致服务器无法解释报文。
  在处理可空的字符串类型时需要使用修改生成的stub文件，并手动在代码中过滤`null`值，如：
  
  ```csharp
  // Log.Generated.cs

  public static partial class Types {
    public sealed partial class Content : pb::IMessage<Content> {
      public void WriteTo(pb::CodedOutputStream output) {
        ...
        // if (Value.Length != 0) {
        if (value_ != null) {
          output.WriteRawTag(18);
          output.WriteString(Value);
        }
        ...
      }

      public int CalculateSize() {
        ...
        // if (Value.Length != 0) {
        if (value_ != null) {
          size += 1 + pb::CodedOutputStream.ComputeStringSize(Value);
        }
        ...
      }

      public void MergeFrom(Content other) {
        ...
        // if (other.Value.Length != 0) {
        if (other.value_ != null) {
          Value = other.Value;
        }
        ...
      }
  ```
  
  ```csharp
  // proto.Value = value;
  proto.Value = value ?? String.Empty;
  ```

### 类型公开原则

项目中对外API相关的类型中禁止出现依赖库相关的外部类型在类型**声明**的层面出现，实现层面不限制。如：

```csharp
class Foo
{
    // 错误，返回值为外部类型
    JObject get(String key);
    
    // 正确，隐藏外部类型
    IDictionary<String, Object> get(String key);
    
    // 推荐，提供访问便利性同时隐藏外部类型
    dynamic get(String key)
    {
        // 实现的代码中直接依赖外部类型是可以的
        return JObject.Parse("{......}");
    }
}
```

对于部分外部类型（如：[`HttpResponseMessage`](Aliyun.Api.LogService/Infrastructure/Protocol/Http/HttpResponseExtensions.cs#L52)）可以通过在**扩展方法**中声明对应的外部类型，但依旧不能直接在原始的接口或类中声明。

```csharp
public interface IFoo
{
    // 错误，不要在基础接口中声明外部类型
    HttpResponseMessage Response { get; }
}

internal class HttpFoo : IFoo
{
    // 实现层面可依赖外部类型
    internal HttpResponseMessage Response { get; }
}

public static class FooExtensions
{
    // 正确，通过扩展方法返回外部类型
    public static HttpResponseMessage GetResponse(this IFoo source)
    {
        return ((HttpFoo)souce).Response;
    }
}
```

### Dynamic支持

在`Aliyun.Api.LogService`项目中，出于便利性对外提供了`dynamic`返回的方法（如：[`LogHeaderExtensions.GetQueryInfoAsDynamic()`](Aliyun.Api.LogService/Infrastructure/Protocol/Http/LogHeaderExtensions.cs#L178)）。

但`Aliyun.Api.LogService`项目中并不支持**使用**dynamic对象，如需使用请按下面步骤添加依赖：

- 添加依赖[`Microsoft.CSharp`](https://www.nuget.org/packages/Microsoft.CSharp)
- 可执行的项目中添加[`System.Dynamic.Runtime`](https://www.nuget.org/packages/System.Dynamic.Runtime)，如果已是.NetFramework或.NetCoreApp项目可以忽略。

## 测试

### 服务凭据配置

单元测试使用生产环境，配置文件：[Aliyun.Api.LogService.Tests/TestUtils/TestContextFixture.cs](Aliyun.Api.LogService.Tests/TestUtils/TestContextFixture.cs)，其中构建 Client 时候的 `Credential` 配置已被隐藏，运行单元测试前务必配置有效的服务凭据。

### Shipper测试配置

如需要需要测试投递功能，由于对外 API 不支持动态创建 Shipper，需要先创建 LogStore 后按照[投递流程](https://help.aliyun.com/document_detail/29002.html)的指引创建独立的 Shipper 再把 Shipper 的名称配置到 `TestContextFixture`.`ShipperName` 才可测试。 

### 测试开关

`TestContextFixture` 中有如下开关：

- ShouldInitProject

    是否执行 `CreateProject`

- ShouldCleanProject

    是否执行 `DeleteProject`

- ShouldInit

    是否执行 `CreateLogStore`、`CreateConfig`、`CreateIndex`、`CreateMachineGroup`

- ShouldClean

    是否执行 `DeleteLogStore`、`DeleteConfig`、`DeleteMachineGroup`

- ShouldTestShipper

    是否执行 `GetShipperStatus`、`RetryShipperTask`
