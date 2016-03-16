# NLog.Targets.AzureTableByMonth

This is an NLog Target based off of chaowlert's [NLog.Targets.Azure](https://github.com/chaowlert/NLog.Targets.Azure). chaowlert's version is a more general solution with much more flexibility. I suggest you start there before trying my customized version.

My version has diverged in the following ways:

- It partitions everything by month. The `PartitionKey` has the format `YYYYMM`.

- The `RowKey` is `(DateTime.MaxValue.Ticks - DateTime.UtcNow.Ticks).ToString()` + three random digits. In Azure Table, this has the effect of making the newest entries for a month show up at the top of the grid in Cloud Explorer. It also makes querying for each month's latest alerts very efficient. Lastly, the encoding is minimal, so the `RowKey`'s value is easy to work with if needed.

## Sample NLog.config:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<nlog
  xmlns="http://www.nlog-project.org/schemas/NLog.xsd"
  xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
  autoReload="true"
  throwExceptions="false"
  internalLogLevel="Off"
  internalLogFile="d:\home\site\wwwroot\app_data\Logs\nlog-internal.log">

  <extensions>
    <add assembly="NLog.Targets.AzureTableByMonth"/>
  </extensions>

  <targets async="true">
    <target name="AzureTableStorage" xsi:type="AzureTable" connectionStringName="AzureLogs" tableName="MyApplicationLogs">
      <property name="LogLevel" value="${level}" />
      <property name="MachineName" value="${machinename}" />
      <property name="Message" value="${message}" />
      <property name="LoggerName" value="${logger}" />
      <property name="ExceptionMessage" value="${exception:format=ToString}" />
      <property name="ExceptionType" value="${exception:format=Type}" />
    </target>
  </targets>

  <rules>
    <logger name="*" minlevel="Trace" writeTo="AzureTableStorage" />
  </rules>
</nlog>
```