using System;
using System.Collections.Generic;
using System.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace NLog.Targets.AzureTableByMonth
{
    public class AzureLogTableStorageClient
    {
        private readonly CloudStorageAccount _storageAccount;
        private readonly CloudTableClient _tableClient;
        private readonly string _tableName;

        public AzureLogTableStorageClient(string connectionStringName, string tableName)
        {
            _storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings[connectionStringName].ConnectionString);
            _tableClient = _storageAccount.CreateCloudTableClient();
            _tableName = tableName;
        }

        public void Insert(ITableEntity entity)
        {
            var table = _tableClient.GetTableReference(_tableName);

            var insertOp = TableOperation.Insert(entity);
            table.Execute(insertOp);
        }

        public void BulkInsert<T>(IEnumerable<T> entities)
            where T : ITableEntity
        {
            if (entities == null)
            {
                throw new ArgumentNullException(nameof(entities));
            }

            var table = _tableClient.GetTableReference(_tableName);
            var batchInsertOp = new TableBatchOperation();

            foreach (var entity in entities)
            {
                batchInsertOp.Insert(entity);

                if (batchInsertOp.Count < 100)
                {
                    // batch up to 100 at a time.
                    continue;
                }

                // We've hit 100 items. Submit the batch, then clear it.
                table.ExecuteBatch(batchInsertOp);
                batchInsertOp.Clear();
            }

            // If there are any that weren't submitted as a batch of 100, submit them now.
            if (batchInsertOp.Count > 0)
            {
                table.ExecuteBatch(batchInsertOp);
            }
        }


        public void CreateTableIfNotExists()
        {
            var table = _tableClient.GetTableReference(_tableName);
            table.CreateIfNotExists();
        }
    }
}
