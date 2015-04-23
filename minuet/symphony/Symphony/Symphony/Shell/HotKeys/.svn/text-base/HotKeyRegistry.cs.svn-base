using System.Collections.Generic;

namespace Symphony.Shell.HotKeys
{
    public class HotKeyRegistry
    {
        private readonly Dictionary<string, IHotKeyCommand> registryByName;
        private readonly Dictionary<int, string> indexByHashCode;

        public HotKeyRegistry()
        {
            this.registryByName = new Dictionary<string, IHotKeyCommand>();
            this.indexByHashCode = new Dictionary<int, string>();
        }

        public bool Contains(string name)
        {
            return this.registryByName.ContainsKey(name);
        }

        public void Register(string name, IHotKeyCommand command)
        {
            this.registryByName.Add(name, command);
            this.indexByHashCode.Add(command.GetHashCode(), name);
        }

        public bool TryGetCommand(string name, out IHotKeyCommand command)
        {
            return this.registryByName.TryGetValue(name, out command);
        }

        public bool TryGetCommand(int hashCode, out IHotKeyCommand command)
        {
            string name;

            if (this.TryGetName(hashCode, out name))
            {
                return this.registryByName.TryGetValue(name, out command);
            }

            command = null;
            return false;
        }

        public bool TryGetName(int hashCode, out string name)
        {
            return this.indexByHashCode.TryGetValue(hashCode, out name);
        }

        public bool TryRemove(string name, out IHotKeyCommand command)
        {
            if (this.TryGetCommand(name, out command))
            {
                this.registryByName.Remove(name);
                this.indexByHashCode.Remove(command.GetHashCode());

                return true;
            }

            return false;
        }
    }
}
