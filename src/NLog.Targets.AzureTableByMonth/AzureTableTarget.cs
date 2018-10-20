using System;
using System.Collections.Generic;
using System.Linq;
using NLog.Common;
using NLog.Config;

namespace NLog.Targets.AzureTableByMonth
{
    [Target("AzureTable")]
    public class AzureTableTarget : Target
    {
        [ThreadStatic]
        private static Random _random;
        private static Random Random => _random ?? (_random = new Random(Guid.NewGuid().GetHashCode()));

        private static readonly long MaxDateTimeTicks = DateTime.MaxValue.Ticks;


        [RequiredParameter]
        public string ConnectionStringName { get; set; }

        [RequiredParameter]
        public string TableName { get; set; }


        private readonly List<AzureLogTableProperty> _properties = new List<AzureLogTableProperty>();

        [ArrayParameter(typeof(AzureLogTableProperty), "property")]
        public IList<AzureLogTableProperty> Properties => _properties;

        private AzureLogTableStorageClient _client;


        protected override void InitializeTarget()
        {
            _client = new AzureLogTableStorageClient(ConnectionStringName, TableName);
            _client.CreateTableIfNotExists();

            base.InitializeTarget();
        }

        protected override void Write(LogEventInfo logEventInfo)
        {
            var azureLogEntity = CreateAzureLogEntity(logEventInfo);

            _client.Insert(azureLogEntity);
        }

        [Obsolete("Use override Write(IList<AsyncLogEventInfo> logEvents instead. Marked obsolete on NLog 4.5")]
        protected override void Write(AsyncLogEventInfo[] logEventInfos)
        {
            try
            {
                var entities = logEventInfos.Select(lei => CreateAzureLogEntity(lei.LogEvent));

                _client.BulkInsert(entities);

                foreach (var logEventInfo in logEventInfos)
                {
                    logEventInfo.Continuation(null);
                }
            }
            catch (Exception ex)
            {
                foreach (var logEventInfo in logEventInfos)
                {
                    logEventInfo.Continuation(ex);
                }
            }
        }

        protected override void Write(IList<AsyncLogEventInfo> logEvents)
        {
            try
            {
                var entities = logEvents.Select(lei => CreateAzureLogEntity(lei.LogEvent));

                _client.BulkInsert(entities);

                foreach (var logEventInfo in logEvents)
                {
                    logEventInfo.Continuation(null);
                }
            }
            catch (Exception ex)
            {
                foreach (var logEventInfo in logEvents)
                {
                    logEventInfo.Continuation(ex);
                }
            }
        }


        private AzureLogTableEntity CreateAzureLogEntity(LogEventInfo logEventInfo)
        {
            var utcNow = DateTime.UtcNow;
            var entity = new AzureLogTableEntity
            {
                PartitionKey = utcNow.ToString("yyyyMM"),
                RowKey = CreateRowKey(utcNow)
            };

            foreach (var property in _properties)
            {
                entity[property.Name] = property.Value.Render(logEventInfo);
            }

            return entity;
        }

        private string CreateRowKey(DateTime utcNow)
        {
            // Subtract current ticks from MaxValue.Ticks so that newest log entries show up first
            //  (remember that PartitionKey/RowKey are the clustered index, sorted ascending).
            // Then, to ensure uniqueness, append three meaningless random digits.
            // Need to support .NET 4.5
            //return $"{MaxDateTimeTicks - utcNow.Ticks}{Random.Next(0, 1000).ToString("000")}";
            var ticks = MaxDateTimeTicks - utcNow.Ticks;
            var randomSuffix = Random.Next(0, 1000);
            return string.Format("{0}{1}", ticks, randomSuffix.ToString("000"));
        }
    }
}
