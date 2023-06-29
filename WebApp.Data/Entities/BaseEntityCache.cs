using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Reflection;
using TenantManagement.Data.Entities;

namespace WebApp.Data.Entities
{
    public class IgnorePropertiesResolver : DefaultContractResolver
    {
        private readonly HashSet<string> ignoreProps;

        public IgnorePropertiesResolver(IEnumerable<string> propNamesToIgnore)
        {
            ignoreProps = new HashSet<string>(propNamesToIgnore);
        }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);
            if (ignoreProps.Contains(property.PropertyName))
            {
                property.ShouldSerialize = _ => false;
            }
            return property;
        }
    }

    public static class BaseEntityCache
    {
        public static readonly string CachePropertyName = "CachePropertyName";
        public static readonly string CacheExpirationPropertyName = "CachePropertyExpirationHours";
        public static readonly int DefaultExpirationInHours = 15;
        public static readonly int DefaultNoExpiration = 0;
        public static readonly BindingFlags bindings = BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        public static void SetCache(this BaseEntity entity, object cacheData, IEnumerable<string> ignoreProps = null)
        {
            var property = entity.GetType().GetField(CachePropertyName, bindings | BindingFlags.Static)?.GetValue(entity) as string;
            if (property == null)
            {
                property = entity.GetType().GetProperty(CachePropertyName, bindings | BindingFlags.Static)?.GetValue(entity) as string;
            }

            entity.SetCache(property, cacheData, ignoreProps);
        }

        public static void SetCache(this BaseEntity entity, string property, object cacheData, IEnumerable<string> ignoreProps = null)
        {
            _ = SetCacheProperty(entity, property, cacheData, ignoreProps) || SetCacheField(entity, property, cacheData, ignoreProps);
        }

        public static T? GetCache<T>(this BaseEntity entity)
        {
            return entity.GetCache<T>(GetPropertyName(entity));
        }

        public static T? GetCache<T>(this BaseEntity entity, string property, bool force = false)
        {
            var expiration = GetExpirationInHours(entity);
            if (force || entity.ModifiedDatetime == null || expiration == DefaultNoExpiration || DateTime.Now <= entity.ModifiedDatetime.Value.AddHours(expiration))
            {
                var val = entity.GetType().GetProperty(property, bindings)?.GetValue(entity) as string;
                if (val == null)
                {
                    val = entity.GetType().GetField(property, bindings)?.GetValue(entity) as string;
                }

                if (val != null)
                {
                    return JsonConvert.DeserializeObject<T>(val);
                }
            }

            return default;
        }

        private static bool SetCacheProperty(BaseEntity entity, string property, object cacheData, IEnumerable<string> ignoreProps = null)
        {
            var prop = entity.GetType().GetProperty(property, bindings);
            if (prop != null)
            {
                if (cacheData == null)
                {
                    prop.SetValue(entity, cacheData);
                }
                else
                {
                    var settings = new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };

                    if (ignoreProps != null)
                    {
                        settings.ContractResolver = new IgnorePropertiesResolver(ignoreProps);
                    }

                    prop.SetValue(entity, JsonConvert.SerializeObject(cacheData, Formatting.None, settings));
                }

                return true;
            }

            return false;
        }

        private static bool SetCacheField(BaseEntity entity, string property, object cacheData, IEnumerable<string> ignoreProps = null)
        {
            var prop = entity.GetType().GetField(property, bindings);

            if (prop != null)
            {
                if (cacheData == null)
                {
                    prop.SetValue(entity, cacheData);
                }
                else
                {
                    JsonSerializerSettings settings = null;

                    if (ignoreProps != null)
                    {
                        new JsonSerializerSettings() { ContractResolver = new IgnorePropertiesResolver(ignoreProps) };
                    }

                    prop.SetValue(entity, JsonConvert.SerializeObject(cacheData, settings));
                }

                return true;
            }

            return false;
        }

        private static int GetExpirationInHours(BaseEntity entity)
        {
            var expiration = entity.GetType().GetField(CacheExpirationPropertyName, bindings | BindingFlags.Static)?.GetValue(entity) as int?;
            if (expiration == null)
            {
                expiration = entity.GetType().GetProperty(CacheExpirationPropertyName, bindings | BindingFlags.Static)?.GetValue(entity) as int?;
                if (expiration == null)
                {
                    expiration = DefaultExpirationInHours;
                }
            }

            return (int)expiration;
        }

        private static string GetPropertyName(BaseEntity entity)
        {
            var property = entity.GetType().GetField(CachePropertyName, bindings | BindingFlags.Static)?.GetValue(entity) as string;
            if (property == null)
            {
                property = entity.GetType().GetProperty(CachePropertyName, bindings | BindingFlags.Static)?.GetValue(entity) as string;
            }

            if (property == null)
            {
                throw new MissingFieldException("Cache Property Not Found");
            }

            return property;
        }
    }
}