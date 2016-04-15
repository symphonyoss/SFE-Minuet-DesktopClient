//Licensed to the Apache Software Foundation(ASF) under one
//or more contributor license agreements.See the NOTICE file
//distributed with this work for additional information
//regarding copyright ownership.The ASF licenses this file
//to you under the Apache License, Version 2.0 (the
//"License"); you may not use this file except in compliance
//with the License.  You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
//Unless required by applicable law or agreed to in writing,
//software distributed under the License is distributed on an
//"AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
//KIND, either express or implied.  See the License for the
//specific language governing permissions and limitations
//under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Paragon.Plugins;
using Paragon.Runtime.Kernel.Plugins;
using Paragon.Runtime.Kernel.Windowing;
using Paragon.Properties;
using Paragon.Runtime;
using Paragon.Runtime.WPF;
using Paragon.Plugins.MessageBus;

namespace Paragon
{
    internal class Bootstrapper
    {
        private readonly IocContainer _container = new IocContainer();
        private readonly ILogger _logger = ParagonLogManager.GetLogger();

        public Bootstrapper()
        {
            RegisterPlugins();

            _container
                .RegisterType<ApplicationFactory>(true)
                .RegisterType<ICefWebBrowser, CefWebBrowser>()
                .RegisterType<IApplicationWindowEx, ApplicationWindow>()
                .RegisterType<IApplicationWindowManagerEx, ApplicationWindowManager>(true)
                .RegisterType<IParagonPlugin, ParagonAppRuntimePlugin>()
                .RegisterType<IParagonPlugin, ParagonAppWindowPlugin>()
                .RegisterType<IParagonPlugin, ParagonContextMenuPlugin>()
                .RegisterType<IParagonPlugin, ParagonSystemIdlePlugin>()
                .RegisterType<IParagonPlugin, ParagonSystemPlugin>()
                .RegisterType<IParagonPlugin, ParagonWindowOverridesPlugin>()
                .RegisterType<IParagonPlugin, LoggerPlugin>()
                .RegisterType<IParagonPlugin, MessageBusPlugin>();
        }

        public T Resolve<T>() where T : class
        {
            return _container.Resolve<T>();
        }

        private void RegisterPlugins()
        {
            foreach (var pluginTypeName in Settings.Default.Plugins)
            {
                Type pluginType;
                try
                {
                    pluginType = Type.GetType(pluginTypeName);
                }
                catch (Exception e)
                {
                    var typeName = pluginTypeName;
                    _logger.Error("Error resolving plugin type: {0} - {1}", typeName, e.Message);
                    continue;
                }

                if (pluginType == null)
                {
                    _logger.Error("Plugin type not found: " + pluginTypeName);
                    continue;
                }

                if (!typeof(IParagonPlugin).IsAssignableFrom(pluginType))
                {
                    _logger.Error("Specified plugin type is not an IParagonPlugin: " + pluginTypeName);
                    continue;
                }

                _container.RegisterType<IParagonPlugin>(pluginType);
                _logger.Info("Registered plugin type: " + pluginTypeName);
            }
        }

        private class IocContainer
        {
            private readonly Dictionary<Type, List<TypeRegistration>> _dependencyMap =
                new Dictionary<Type, List<TypeRegistration>>();

            public T Resolve<T>()
            {
                return (T)Resolve(typeof(T));
            }

            public IocContainer RegisterType<T>(bool isSingleton = false)
            {
                // Add a type registration lookup if one doesn't exist.
                if (!_dependencyMap.ContainsKey(typeof(T)))
                {
                    _dependencyMap.Add(typeof(T), new List<TypeRegistration>());
                }

                // Add the registration.
                _dependencyMap[typeof(T)].Add(new TypeRegistration(typeof(T), isSingleton));
                return this;
            }

            public IocContainer RegisterType<TFrom, TTo>(bool isSingleton = false)
            {
                // Add a type registration lookup if one doesn't exist.
                if (!_dependencyMap.ContainsKey(typeof(TFrom)))
                {
                    _dependencyMap.Add(typeof(TFrom), new List<TypeRegistration>());
                }

                // Add the registration.
                _dependencyMap[typeof(TFrom)].Add(new TypeRegistration(typeof(TTo), isSingleton));
                return this;
            }

            public IocContainer RegisterType<TFrom>(Type toType, bool isSingleton = false)
            {
                
                // Add a type registration lookup if one doesn't exist.
                if (!_dependencyMap.ContainsKey(typeof(TFrom)))
                {
                    _dependencyMap.Add(typeof(TFrom), new List<TypeRegistration>());
                }

                // Add the registration.
                _dependencyMap[typeof(TFrom)].Add(new TypeRegistration(toType, isSingleton));
                return this;
            }

            private object Resolve(Type type)
            {
                // Special handling for array types.
                if (type.IsArray)
                {
                    return ResolveArrayType(type);
                }

                // Special handling for Func<> types.
                if (type.IsGenericType
                    && type.GetGenericTypeDefinition() == typeof(Func<>))
                {
                    return ResolveFactoryType(type);
                }

                // Confirm a registration exists for the specified type.
                if (!_dependencyMap.ContainsKey(type)
                    || _dependencyMap[type].Count != 1)
                {
                    throw new InvalidOperationException("Unable to resolve dependency type: " + type.FullName);
                }

                // Get the registration.
                var registration = _dependencyMap[type].First();
                if (registration.IsSingleton)
                {
                    // Singleton behavior has been specified, return the single instance or create one if needed.
                    return registration.SingletonInstance ?? (registration.SingletonInstance = CreateInstance(registration));
                }

                // Return a new instance (non-singleton).
                return CreateInstance(registration);
            }

            private object ResolveFactoryType(Type type)
            {
                // For Func<> types, return a Func<> that resolves the type(s) when invoked.
                var args = type.GetGenericArguments().ToList();
                if (args.Count != 1)
                {
                    throw new InvalidOperationException("Unable to resolve dependency type: " + type.FullName);
                }

                var method = GetType().GetMethod("Resolve", BindingFlags.NonPublic | BindingFlags.Instance);

                var body = Expression.Convert(Expression.Call(Expression.Constant(this),
                    method, Expression.Constant(args[0], typeof(Type))), args[0]);

                var lambda = Expression.Lambda(type, body);
                return lambda.Compile();
            }

            private object[] ResolveArrayType(Type type)
            {
                // For array types, resolve all instances of the specified type and return as an array.
                var fromType = type.GetElementType();
                if (!_dependencyMap.ContainsKey(fromType))
                {
                    throw new InvalidOperationException("Unable to resolve dependency type: " + fromType.FullName);
                }

                var registrations = _dependencyMap[fromType];
                var instances = Array.CreateInstance(fromType, registrations.Count);

                for (var i = 0; i < registrations.Count; ++i)
                {
                    var reg = registrations[i];
                    if (reg.IsSingleton)
                    {
                        if (reg.SingletonInstance == null)
                        {
                            reg.SingletonInstance = CreateInstance(reg);
                        }

                        instances.SetValue(reg.SingletonInstance, i);
                    }
                    else
                    {
                        instances.SetValue(CreateInstance(reg), i);
                    }
                }

                return (object[])instances;
            }

            private object CreateInstance(TypeRegistration registration)
            {
                var constructor = registration.Type.GetConstructors().First();
                var parameters = constructor.GetParameters();

                return !parameters.Any() ? Activator.CreateInstance(registration.Type)
                    : constructor.Invoke(ResolveParameters(parameters).ToArray());
            }

            private IEnumerable<object> ResolveParameters(IEnumerable<ParameterInfo> parameters)
            {
                return parameters.Select(p => Resolve(p.ParameterType)).ToList();
            }

            private class TypeRegistration
            {
                public TypeRegistration(Type type, bool isSingleton)
                {
                    Type = type;
                    IsSingleton = isSingleton;
                }

                public Type Type { get; private set; }
                public bool IsSingleton { get; private set; }
                public object SingletonInstance { get; set; }
            }
        }
    }
}