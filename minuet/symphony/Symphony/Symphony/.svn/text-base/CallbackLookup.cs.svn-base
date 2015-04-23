using System.Collections.Generic;

namespace Symphony
{
    public class CallbackLookup
    {
        private readonly Dictionary<string, string> callbacksByKey = new Dictionary<string, string>();

        public string Find(string key)
        {
            string methodName;

            if (this.callbacksByKey.TryGetValue(key, out methodName))
            {
                return methodName;
            }

            return null;
        }

        public void Register(string key, string methodName)
        {
            this.callbacksByKey[key] = methodName;
        }
    }
}
