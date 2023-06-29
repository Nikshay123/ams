using AutoMapper;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TenantManagement.Models
{
    public static class MapperExtensions
    {
        public static TDest MapIgnoreCycles<TSrc, TDest>(this IMapper mapper, TSrc entity)
        {
            var jsonOptions = new JsonSerializerOptions
            {
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
            };

            /*if (typeof(TSrc).IsGenericType && typeof(TSrc).GetGenericTypeDefinition() == typeof(List<>))
            {
                var listType = typeof(TSrc).GetGenericArguments()[0];
                dynamic result = (TSrc)Activator.CreateInstance(typeof(TSrc));
                foreach (var item in (entity as IEnumerable))
                {
                    var jsonstr = JsonSerializer.Serialize(item, jsonOptions);
                    dynamic de = JsonSerializer.Deserialize(jsonstr, listType);
                    result.Add(Convert.ChangeType(de, listType));
                }

                return mapper.Map<TSrc, TDest>(result);
            }*/

            var json = JsonSerializer.Serialize(entity, jsonOptions);
            var deserializedEntity = (TSrc)JsonSerializer.Deserialize(json, typeof(TSrc));
            return mapper.Map<TSrc, TDest>(deserializedEntity);
        }
    }
}