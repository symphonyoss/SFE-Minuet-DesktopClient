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
