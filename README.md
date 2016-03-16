# NLog.Targets.AzureTableByMonth

This is an NLog Target based off of chaowlert's [NLog.Targets.Azure](https://github.com/chaowlert/NLog.Targets.Azure). chaowlert's version is a more general solution with much more flexibility. I suggest you start there before trying my customized version.

My version has diverged in the following ways:

- It partitions everything by month. The `PartitionKey` has the format `YYYYMM`.

- The `RowKey` is `(DateTime.MaxValue.Ticks - DateTime.UtcNow.Ticks).ToString()` + three random digits. In Azure Table, this has the effect of making the newest entries for a month show up at the top of the grid in Cloud Explorer. It also makes querying for each month's latest alerts very efficient. Lastly, the encoding is minimal, so the `RowKey`'s value is easy to work with if needed.

