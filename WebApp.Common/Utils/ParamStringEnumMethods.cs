using System;
using System.Collections.Generic;
using System.Linq;

namespace WebApp.Common.Utils
{
    public static class _
    {
        public static List<T> GetEnumListFromString<T>(this string paramString) where T : Enum
        {
            List<T> enumList = new();

            if (paramString == null)
            {
                return enumList;
            }

            List<string> paramList = paramString.Trim().Split(',').ToList();

            paramList.ForEach(param =>
            {
                object userInputEnum;
                Enum.TryParse(typeof(T), param, true, out userInputEnum);
                if (userInputEnum != null)
                {
                    enumList.Add((T)userInputEnum);
                }
            });

            return enumList;
        }

        public static bool ContainsEnumListEntry<T>(this string paramString, Enum enumListEntry) where T : Enum
        {
            List<T> enumList = paramString.GetEnumListFromString<T>();

            foreach (Enum item in enumList)
            {
                if (item.Equals(enumListEntry))
                {
                    return true;
                }
            }

            return false;
        }

        public static string ExtractTokenFromCommaSeparatedList(this string tokens, string token, string separator = ",", bool caseInsensitive = true)
        {
            if (!string.IsNullOrEmpty(tokens))
            {
                var tokenlist = tokens.Split(",").ToList();
                var newList = tokenlist.FindAll(t =>
                {
                    if (caseInsensitive)
                    {
                        return t.ToLower() != token.ToLower();
                    }

                    return t != token;
                }).ToList();

                if (tokenlist.Count() != newList.Count())
                {
                    return string.Join(separator, newList);
                }
            }

            return null;
        }
    }
}