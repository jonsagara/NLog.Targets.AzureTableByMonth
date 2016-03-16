using System.Collections.Generic;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace NLog.Targets.AzureTableByMonth
{
    public class AzureLogTableEntity : TableEntity
    {
        private readonly Dictionary<string, string> _properties = new Dictionary<string, string>();

        public string this[string name]
        {
            get
            {
                string value;
                if (_properties.TryGetValue(name, out value))
                {
                    return value;
                }

                return null;
            }
            set
            {
                _properties[name] = value;
            }
        }

        public override IDictionary<string, EntityProperty> WriteEntity(OperationContext operationContext)
        {
            var dict = new Dictionary<string, EntityProperty>();

            foreach (var property in _properties)
            {
                dict.Add(property.Key, new EntityProperty(property.Value));
            }

            return dict;
        }
    }
}
