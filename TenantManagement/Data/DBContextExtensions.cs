using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;

namespace TenantManager.Data
{
    public class FilterOperators
    {
        public const string Equal = "eq";
        public const string NotEqual = "neq";
        public const string GreaterThan = "gt";
        public const string GreaterThanOrEqual = "gte";
        public const string LessThan = "lt";
        public const string LessThanOrEqual = "lte";
        public const string Like = "like";
        public const string IsDate = "isdate";
    }

    public static class DBContextExtensions
    {
        public static IQueryable<T> AddInclude<T>(this IQueryable<T> query, string include) where T : class
        {
            if (string.IsNullOrWhiteSpace(include))
            {
                return query;
            }

            string[] includedList = include.Split(",");

            foreach (var item in includedList)
            {
                string[] parts = item.Split(".");
                var prop = typeof(T).GetProperties().Where(p => p.Name.ToLower() == parts[0].ToLower().Trim()).FirstOrDefault();

                if (prop != null)
                {
                    parts[0] = prop.Name;
                    query = query.Include(string.Join('.', parts));
                }
            }

            return query;
        }

        public static bool HasDuplicates(this List<int> items)
        {
            return items.GroupBy(i => i).Any(g => g.Count() > 1);
        }

        public static IQueryable<T> Sort<T>(this IQueryable<T> query, List<string> sortParam)
        {
            if (sortParam == null || sortParam.Count() <= 0)
            {
                return query;
            }

            IOrderedQueryable<T> orderedQuery = null;
            sortParam.ForEach(sp =>
            {
                if (!string.IsNullOrEmpty(sp))
                {
                    var ascending = sp.First() == '^';
                    if (ascending)
                    {
                        sp = sp.Substring(1);
                    }

                    var keyProp = typeof(T).GetProperties().Where(p => p.Name.ToLower() == sp.Trim().ToLower()).FirstOrDefault();
                    if (keyProp != null)
                    {
                        var param = Expression.Parameter(typeof(T));
                        var memberAccess = Expression.PropertyOrField(param, keyProp.Name);
                        var convert = Expression.Convert(memberAccess, typeof(object));
                        var keySelector = Expression.Lambda<Func<T, object>>(convert, param);

                        if (!ascending)
                        {
                            if (orderedQuery == null)
                            {
                                orderedQuery = query.OrderBy(keySelector);
                            }
                            else
                            {
                                orderedQuery = orderedQuery.ThenBy(keySelector);
                            }
                        }
                        else
                        {
                            if (orderedQuery == null)
                            {
                                orderedQuery = query.OrderByDescending(keySelector);
                            }
                            else
                            {
                                orderedQuery = orderedQuery.ThenByDescending(keySelector);
                            }
                        }
                    }
                }
            });

            return orderedQuery == null ? query : orderedQuery;
        }

        public static IQueryable<T> Filter<T>(this IQueryable<T> query, string filter)
        {
            if (!string.IsNullOrEmpty(filter))
            {
                var ftType = Activator.CreateInstance<T>().GetType();
                var expressions = new List<string>(filter.Split(";"));
                var param = Expression.Parameter(typeof(T));

                foreach (var item in expressions)
                {
                    var orExpressions = new List<string>(item.Split("||"));

                    Expression exp = null;
                    foreach (var part in orExpressions)
                    {
                        Regex regex = new Regex(@"^\s*([^\s]+)\s+([^\s]+)\s+(.+)$");
                        var mc = regex.Match(part);

                        if (mc.Success)
                        {
                            var lh = mc.Groups[1].Value;
                            var op = mc.Groups[2].Value.ToLower();
                            var rh = string.IsNullOrEmpty(mc.Groups[3].Value) ? null : mc.Groups[3].Value;
                            var keyProp = ftType.GetProperties().Where(p => p.Name.ToLower() == lh.Trim().ToLower()).FirstOrDefault();
                            if (keyProp != null)
                            {
                                try
                                {
                                    ConstantExpression val = NormalizeValue<T>(keyProp, rh, op);
                                    if (exp == null)
                                    {
                                        exp = FilterExpression(keyProp, val, op, param);
                                    }
                                    else
                                    {
                                        exp = Expression.Or(exp, FilterExpression(keyProp, val, op, param));
                                    }
                                }
                                catch (Exception ex)
                                {
                                    //ignore filter express error
                                }
                            }
                        }
                    }

                    if (exp != null)
                    {
                        query = query.Where(Expression.Lambda<Func<T, bool>>(exp, param));
                    }
                }
            }

            return query;
        }

        private static ConstantExpression NormalizeValue<TItem>(PropertyInfo property, string value, string op)
        {
            Type type = property.PropertyType;

            // We need to check whether the property is NULLABLE
            if (property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                // If it is NULLABLE, then get the underlying type. eg if "Nullable<int>" then this will return just "int"
                type = property.PropertyType.GetGenericArguments()[0];
            }

            if (value == "NULL")
            {
                value = null;
            }

            var ceVal = Expression.Constant(value);
            if (type != typeof(string))
            {
                var nullVal = Activator.CreateInstance(type);
                ceVal = nullVal switch
                {
                    int => Expression.Constant(value == null ? null : int.Parse(value)),
                    long => Expression.Constant(value == null ? null : long.Parse(value)),
                    decimal => Expression.Constant(value == null ? null : decimal.Parse(value)),
                    double => Expression.Constant(value == null ? null : double.Parse(value)),
                    float => Expression.Constant(value == null ? null : float.Parse(value)),
                    bool => Expression.Constant(value == null ? null : bool.Parse(value)),
                    Enum => Expression.Constant(value == null ? null : Enum.Parse(type, value, true)),
                    DateTime => op == FilterOperators.IsDate ? Expression.Constant(value == null ? null : DateTime.Parse(value).Date) : Expression.Constant(value == null ? null : DateTime.Parse(value)),
                    _ => Expression.Constant(value == null ? null : value.ToString())
                };
            }

            return ceVal;
        }

        private static Expression FilterExpression(PropertyInfo property, ConstantExpression value, string op, ParameterExpression param)
        {
            MethodInfo containsfn = typeof(string).GetMethod(nameof(op.Contains), new[] { typeof(string) });

            Expression prop = Expression.Property(param, property);
            if (property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>) && value.Value != null)
            {
                // If it is NULLABLE, then get the underlying type. eg if "Nullable<int>" then this will return just "int"
                var type = property.PropertyType.GetGenericArguments()[0];
                prop = Expression.Convert(prop, type);
            }

            Expression body = op switch
            {
                FilterOperators.Equal => Expression.Equal(prop, value),
                FilterOperators.NotEqual => Expression.NotEqual(prop, value),
                FilterOperators.GreaterThan => Expression.GreaterThan(prop, value),
                FilterOperators.GreaterThanOrEqual => Expression.GreaterThanOrEqual(prop, value),
                FilterOperators.LessThan => Expression.LessThan(prop, value),
                FilterOperators.LessThanOrEqual => Expression.LessThanOrEqual(prop, value),
                FilterOperators.Like => Expression.Call(prop, containsfn, value),
                FilterOperators.IsDate => Expression.Equal(Expression.Property(prop, nameof(DateTime.Now.Date)), value),
                _ => throw new InvalidFilterCriteriaException($"Invalid Operator: {op}"),
            };

            return body;
        }
    }
}