# Aliyun LogServie SDK for .NET Core

## 1.0.0

### 变更内容

- 支持阿里云 [LogService REST API](https://help.aliyun.com/document_detail/29007.html) ；
- 所有接口均支持异步请求；
- 支持基于 .NetStandard 2.0 的平台；
- 支持 Zlib 压缩/解压缩；
- 支持 Lz4 压缩/解压缩；
- 支持[HMAC-SHA1签名](https://help.aliyun.com/document_detail/29012.html)；
- 支持[RAM访问方式](https://help.aliyun.com/document_detail/29049.html)；
- 支持[STS访问方式](https://help.aliyun.com/document_detail/47277.html)；
- 此版本支持接口：
    + CreateLogStore
    + DeleteLogStore
    + UpdateLogStore
    + GetLogStore
    + ListLogStore
    + ListShards
    + SplitShard
    + MergeShards
    + GetCursor
    + GetShipperStatus
    + RetryShipperTask
    + Create
    + PostLogStoreLogs
    + PullLogs
    + GetLogs
    + GetProjectLogs
    + GetHistograms
    + CreateMachineGroup
    + DeleteMachineGroup
    + UpdateMachineGroup
    + ListMachineGroup
    + GetMachineGroup
    + ApplyConfigToMachineGroup
    + RemoveConfigFromMachineGroup
    + ListMachines
    + GetAppliedConfigs
    + CreateConfig
    + ListConfig
    + GetAppliedMachineGroups
    + GetConfig
    + DeleteConfig
    + UpdateConfig
    + CreateProject
    + ListProject
    + GetProject
    + DeleteProject
