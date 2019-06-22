using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;

namespace SSM
{
    public static class Reflection
    {
        private const BindingFlags fieldFlag = BindingFlags.GetField | BindingFlags.IgnoreCase |
                                                BindingFlags.Instance | BindingFlags.Public;

        private const BindingFlags propertyFlag = BindingFlags.GetField | BindingFlags.IgnoreCase |
                                                  BindingFlags.Instance | BindingFlags.Public;

        public static void InitializeFields<T>(IList<FieldInfo> fields, T instance, bool skipIfNotNull = true)
        {
            for (int i = 0; i < fields.Count; i++)
            {
                if (skipIfNotNull && fields[i].GetValue(instance) != null) { continue; }
                object obj = Activator.CreateInstance(fields[i].FieldType);
                fields[i].SetValue(instance, obj);
            }
        }

        public static void SubscribeToFields<T, K>(IList<FieldInfo> fields, T instance, 
                                                    string eventFieldName, string handlerMethod)
        {
            for (int i = 0; i < fields.Count; i++)
            {
                var obj = fields[i].GetValue(instance);
                var eInfo = typeof(K).GetEvent(eventFieldName);
                var eType = eInfo.EventHandlerType;
                var eventHandler = typeof(T).GetMethod(handlerMethod, BindingFlags.Instance | BindingFlags.NonPublic);
                var handler = Delegate.CreateDelegate(eType, instance, eventHandler);
                eInfo.AddEventHandler(obj, handler);
            }
        }

        public static IList<FieldInfo> GetFieldsImplementingInterface(Type type, Type interfaceType)
        {
            var selectedFields = new List<FieldInfo>();
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
            foreach (FieldInfo field in fields)
            {
                var interfaces = field.FieldType.GetInterfaces();

                for (int i = 0; i < interfaces.Length; i++)
                {
                    if (interfaces[i] == interfaceType)
                    {
                        selectedFields.Add(field);
                    }
                }
            }

            return selectedFields;
        }

        public static MemberTypes GetMemberType<T>(this Expression<Func<T>> accessor, Type type)
        {
            var s = nameof(accessor);
            var member = GetMember(accessor, type);
            if (member == null)
            {
                var message = "Could not find member named " + s + " in type " + type;
                throw new NullReferenceException(message);
            }
            else if (member.Length <= 0)
            {
                var message = "Could not find member named " + s + " in type " + type;
                throw new InvalidOperationException(message);
            }
            else
            {
                return member[0].MemberType;
            }
        }

        /*public static FieldInfo GetField<T, TT>(this Expression<Func<T, TT>> accessor, Type type)
        {
            var s = nameof(() => accessor);
            return type.GetField(s);
        }*/

        public static FieldInfo GetField<T>(this Expression<Func<T>> accessor, Type type)
        {
            //var s = nameof(accessor);
            var s = nameof(() => accessor);
            return type.GetField(s, fieldFlag);
        }

        public static PropertyInfo GetProperty<T>(this Expression<Func<T>> accessor, Type type)
        {
            var s = nameof(accessor);
            return type.GetProperty(s, propertyFlag);
        }

        public static MemberInfo[] GetMember<T>(this Expression<Func<T>> accessor, Type type)
        {
            var s = nameof(accessor);
            return type.GetMember(s);
        }

        /*public static String nameof<T, TT>(this Expression<Func<T, TT>> accessor)
        {
            return nameof(accessor.Body);
        }*/

        public static String nameof<T>(this Expression<Func<T>> accessor)
        {
            return nameof(accessor.Body);
        }

        public static String nameof<T, TT>(this T obj, Expression<Func<T, TT>> propertyAccessor)
        {
            return nameof(propertyAccessor.Body);
        }

        private static String nameof(Expression expression)
        {
            if (expression.NodeType == ExpressionType.MemberAccess)
            {
                var memberExpression = expression as MemberExpression;
                if (memberExpression == null)
                    return null;
                return memberExpression.Member.Name;
            }
            return null;
        }
    }
}