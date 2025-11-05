using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Aliyun.Api.LogService.Infrastructure.Protocol.Http
{
public class CustomCamelCaseResolver : CamelCasePropertyNamesContractResolver
{
    protected override JsonDictionaryContract CreateDictionaryContract(Type objectType)
    {
        var contract = base.CreateDictionaryContract(objectType);

        // keep original case for Dictionary<string, object> and Dictionary<,>
        if (objectType == typeof(Dictionary<string, object>) || 
            objectType.IsGenericType && objectType.GetGenericTypeDefinition() == typeof(Dictionary<,>))
        {
            contract.DictionaryKeyResolver = propertyName => propertyName;
        }
        return contract;
    }
}
}