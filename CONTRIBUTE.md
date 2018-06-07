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
    ├── Aliyun.Api.LogService.Examples/....SDK示例项目
    ├── Aliyun.Api.LogService.Tests/.......SDK单元测试项目
    └── docs/..............................文档存放处


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
