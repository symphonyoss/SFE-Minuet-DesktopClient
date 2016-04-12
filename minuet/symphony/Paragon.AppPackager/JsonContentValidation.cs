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
using System.IO;
using Newtonsoft.Json;
using Paragon.Runtime.PackagedApplication;
namespace Paragon.AppPackager
{
    class JsonContentValidation
    {
        public static bool ValidateJsonFileContent(string jsonFilePath)
        {
            try
            {
                var reader = new StreamReader(jsonFilePath);
                var json = reader.ReadToEnd();
                var manifest = JsonConvert.DeserializeObject<ApplicationManifest>(json);
                reader.Close();
                return true;
            }
            catch (JsonSerializationException e)
            {
                string message = e.Message + "\nNOTE: The following fields are mandatory in the manifest.json file: ID, NAME, VERSION, DESCRIPTION and APP";
                Console.WriteLine(message);
                return false;
            }
        }   
    }
}
