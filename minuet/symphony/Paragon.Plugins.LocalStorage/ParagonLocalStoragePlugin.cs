using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using Newtonsoft.Json.Linq;
using Paragon.Plugins.LocalStorage.Annotations;

namespace Paragon.Plugins.LocalStorage
{
    [JavaScriptPlugin(Name = "paragon.storage.local"), UsedImplicitly]
    public class ParagonLocalStoragePlugin : ParagonPlugin
    {
        private const string StorageFileName = "paragon.storage.local";

        private string FileName
        {
            get { return string.Format("{0}.{1}", StorageFileName, Application.Metadata.Id); }
        }

        [JavaScriptPluginMember, UsedImplicitly]
        public object Get(object keys)
        {
            JArray items = null;
            JObject obj = null;

            if (keys is string)
            {
                items = new JArray();
                items.Add(new JValue(keys));
            }
            else
            {
                items = keys as JArray;
            }

            if (items == null)
            {
                throw new ArgumentException("keys must be string or an array of strings");
            }

            if (items.Count > 0)
            {
                var store = Load();

                if (store.Count != 0)
                {
                    var props = store.Properties();
                    foreach (var item in items)
                    {
                        try
                        {
                            var val = props.First(p => p.Name == item.ToString());
                            if (val != null)
                            {
                                if (obj == null)
                                {
                                    obj = new JObject();
                                }
                                obj.Add(val);
                            }
                        }
                        catch
                        {
                        }
                    }
                }
            }

            return obj;
        }

        [JavaScriptPluginMember, UsedImplicitly]
        public void Set(object value)
        {
            var values = value as JObject;
            if (values == null)
            {
                throw new Exception("value should be an object");
            }

            var store = Load();
            if (store != null)
            {
                foreach (var item in values.Properties())
                {
                    var val = store.GetValue(item.Name);
                    if (val != null)
                    {
                        val.Replace(item.Value);
                    }
                    else
                    {
                        store.Add(item.Name, item.Value);
                    }
                }
                Save(store);
            }
        }

        [JavaScriptPluginMember, UsedImplicitly]
        public void Remove(object keys)
        {
            JArray items = null;

            if (keys is string)
            {
                items = new JArray();
                items.Add(new JValue(keys));
            }
            else
            {
                items = keys as JArray;
            }

            if (items == null)
            {
                throw new ArgumentException("keys must be string or an array of strings");
            }
            if (items.Count > 0)
            {
                var store = Load();
                if (store != null)
                {
                    foreach (var item in items)
                    {
                        store.Remove(item.ToString());
                    }
                    Save(store);
                }
            }
        }

        [JavaScriptPluginMember, UsedImplicitly]
        public void Clear()
        {
            var isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null);
            isoStore.DeleteFile(FileName);
        }

        [JavaScriptPluginMember, UsedImplicitly]
        public double GetBytesInUse(string[] keys)
        {
            throw new NotImplementedException("this API is not implemented yet");
        }

        private JObject Load()
        {
            Application.Logger.Debug(string.Format("Loading from {0}", FileName));
            var isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null);

            if (isoStore.GetFileNames(FileName).Length > 0)
            {
                using (var reader = new StreamReader(new IsolatedStorageFileStream(FileName, FileMode.OpenOrCreate, isoStore)))
                {
                    var str = reader.ReadToEnd();

                    if (!string.IsNullOrEmpty(str))
                    {
                        return JObject.Parse(str);
                    }

                    return JObject.Parse("{}");
                }
            }

            return JObject.Parse("{}");
        }

        private void Save(JObject store)
        {
            Application.Logger.Debug(string.Format("Saving to {0}", FileName));
            var isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null);
            using (var writer = new StreamWriter(new IsolatedStorageFileStream(FileName, FileMode.Create, FileAccess.Write, isoStore)))
            {
                writer.Write(store.ToString());
                writer.Flush();
            }
        }
    }
}