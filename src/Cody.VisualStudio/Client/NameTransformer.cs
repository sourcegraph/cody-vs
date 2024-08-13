﻿using Cody.Core.Agent;
using StreamJsonRpc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Cody.VisualStudio.Client
{
    public class NameTransformer
    {
        public static Func<string, string> CreateTransformer(Type type)
        {
            var dic = type
                .GetMethods()
                .ToDictionary(k => k.Name, v => v.GetCustomAttribute<AgentMethodAttribute>()?.Name ?? v.Name);

            Func<string, string> func = (x) => dic[x];

            return func;
        }

        public static Func<string, string> CreateTransformer<T>() where T : class => CreateTransformer(typeof(T));

        public static IReadOnlyDictionary<MethodInfo, JsonRpcMethodAttribute> GetNotificationMethods(Type type)
        {
            var dic = type
                .GetMethods()
                .Where(x => x.GetCustomAttribute<AgentNotificationAttribute>() != null)
                .ToDictionary(k => k, v =>
                {
                    var att = v.GetCustomAttribute<AgentNotificationAttribute>();
                    return new JsonRpcMethodAttribute(att.Name)
                    {
                        UseSingleObjectParameterDeserialization = att.DeserializeToSingleObject
                    };
                });

            return dic;
        }
    }
}