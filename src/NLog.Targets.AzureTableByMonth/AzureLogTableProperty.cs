using NLog.Config;
using NLog.Layouts;

namespace NLog.Targets.AzureTableByMonth
{
    [NLogConfigurationItem]
    public class AzureLogTableProperty
    {
        [RequiredParameter]
        public string Name { get; set; }

        [RequiredParameter]
        public Layout Value { get; set; }
    }
}
