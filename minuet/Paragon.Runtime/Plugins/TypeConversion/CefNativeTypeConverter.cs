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
using System.Reflection;
using Xilium.CefGlue;

namespace Paragon.Runtime.Plugins.TypeConversion
{
    public class CefNativeTypeConverter
    {
        private readonly Dictionary<string, INativeTypeMemberConverter> _members =
            new Dictionary<string, INativeTypeMemberConverter>();

        private readonly Type _nativeType;

        public CefNativeTypeConverter(Type nativeType)
        {
            _nativeType = nativeType;
            if (_nativeType.IsClass)
            {
                _members = GetJsonMembers(_nativeType.GetProperties)
                    .Select(metadata => new NativeTypePropertyConverter(metadata.Key, metadata.Value))
                    .OfType<INativeTypeMemberConverter>()
                    .ToDictionary(converter => converter.MemberName);
            }
            else
            {
                _members = GetJsonMembers(_nativeType.GetFields)
                    .Select(metadata => new NativeTypeFieldConverter(metadata.Key, metadata.Value))
                    .OfType<INativeTypeMemberConverter>()
                    .ToDictionary(converter => converter.MemberName);
            }
        }

        public object ToNative(CefV8Value cefObject)
        {
            if (cefObject == null)
            {
                throw new ArgumentNullException("cefObject");
            }
            if (!cefObject.IsObject)
            {
                throw new ArgumentOutOfRangeException("cefObject", "Must be an Object");
            }

            var cefValueNames = cefObject.GetKeys();
            var nativeObject = Activator.CreateInstance(_nativeType);

            foreach (var cefValueName in cefValueNames)
            {
                INativeTypeMemberConverter memberConverter;
                if (_members.TryGetValue(cefValueName, out memberConverter))
                {
                    memberConverter.SetNativeValue(nativeObject, cefObject);
                }
            }

            return nativeObject;
        }

        public CefV8Value ToCef(object nativeObject)
        {
            var cefObject = CefV8Value.CreateObject(null);

            foreach (var member in _members.Values)
            {
                member.SetCefValue(cefObject, nativeObject);
            }

            return cefObject;
        }

        private static IEnumerable<KeyValuePair<string, TMemberInfo>> GetJsonMembers<TMemberInfo>(
            Func<BindingFlags, TMemberInfo[]> getMembers) where TMemberInfo : MemberInfo
        {
            return getMembers(BindingFlags.Public | BindingFlags.Instance)
                .Select(member => new KeyValuePair<string, TMemberInfo>(
                    char.ToLower(member.Name[0]) + member.Name.Substring(1), member));
        }
    }
}