# Easy Logger

This project was created to provide an easy to use and configurable logging library. If the default configuration is sufficient for your needs the library can be used out of the box without further setup. However, if you have specific logging needs you can alter the library configuration settings and also provide custom logging methods.

The default implementation is capable of:

- Recording to text log files
- Recording to JSON log files
- Recording to SQL databases
- Adding dated folders to text-based logs (ex. /logs/2020/05/01/log.log)
- Adding dated filenames to text-based logs (ex. /logs/2020-05-01.log)
- Adding custom column names to SQL based logs
- Being used as an ILogger implementation for ASP.NET and other API type applications

This can also be extended to record logs via custom logging endpoints.

## Getting Started

These instuctions can be used to acquire and implement the library.

### Installation

To use this library either clone a copy of the repository or check out the [NuGet package](https://www.nuget.org/packages/Easy.Log.Writer/)

### Usage

**Basic Example**

The following example provides a complete use case.

```
Console.WriteLine("Enter a message to log:");
var message = Console.ReadLine();

var logger = new EasyLoggerService(new LoggingConfiguration());

logger.SaveToLog(input);

```

**Using custom configuration**

In the previous example, the call to ```new LoggingConfiguration()``` was done inline in the service setup. This example makes use of the most basic configuration. However, it can be prepared beforehand and the logger will use different settings or you can create your own using ```ILoggingConfiguration```.

```
var configuration = new LoggingConfiguration()
{
    UseTextLogger = false,
    UseJsonLogger = true,
    UseDatedSubdirectory = false, 
    LogFilename = "[Date:yyyy]-myapplog-[Date:MM-dd_HH]"
};

var logger = new EasyLoggerService(configuration);
```

**Adding A Custom Logging Endpoint**

The system also supports adding your own logging endpoints that will run with the built-in ones. This is done by using ```ILoggerEndpoint```.

The sample tester class.

```
public class ConsoleLogger : ILoggerEndpoint
{
    public ConsoleLogger(ILoggingConfiguration loggingConfiguration)
    {
        Settings = loggingConfiguration;
    }

    public ILoggingConfiguration Settings { get; set; }

    public bool SaveToLog(ILoggerEntry loggerEntry)
    {
        var message = $"Received new log message: {loggerEntry.Message}" + Environment.NewLine +
            $"  at {loggerEntry.Timestamp}" + Environment.NewLine +
            $"  with tag {loggerEntry.Tag}" + Environment.NewLine +
            $"  and severity {loggerEntry.Severity}" + Environment.NewLine;

        Console.WriteLine(message);

        return true;
    }
}
```

Adding to logging service.

```
var configuration = new LoggingConfiguration();
var logger = new EasyLoggerService(configuration);

logger.AddLogger(new ConsoleLogger(configuration));
logger.SaveToLog(input);
```

**Adding A Custom Log Entry Class**

The system also supports adding your own class to contain log entry data. This provides the benefits of allowing your custom endpoints to consume custom data as well as allowing use of custom formats for the built in text logging endpoint.

In the built in text logging endpoint the system checks for an override of ```ToString()``` and when found will use that to construct the message that is saved to the log file.

The sample entry class.

```
public class MyCustomLoggerEntry : ILoggerEntry
{
    public DateTime Timestamp { get; set; }
    public string Tag { get; set; }
    public string Message { get; set; }
    public LogLevel Severity { get; set; }

    public string MyExtraData { get; set; }
    public double MyExtraNumber { get; set; }

    public override string ToString()
    {
        return $"{Timestamp},{Message},{Severity},{MyExtraData},{MyExtraNumber}" + Environment.NewLine;
    }
}
```

Adding to logging service.

```
var logger = new EasyLoggerService(new LoggingConfiguration());

var entry = new MyCustomLoggerEntry()
{
    Timestamp = DateTime.Now,
    Message = input,
    Severity = LogLevel.Warning,
    MyExtraData = "Cool extra data",
    MyExtraNumber = 7.5
};

logger.SaveToLog(entry);
```

**Using Custom SQL Column Mappings**

The system supports mapping the SQL logs to custom columns that have different names or even data types than the built in values.

To do so, build a custom log entry class as specified above and annotate the properties with the ```ColumnAttribute``` as follows.

```
public class MyCustomLoggerEntry : ILoggerEntry
{
    [Column("MyCustomTimestampColumn")]
    public DateTime Timestamp { get; set; }

    [Column("MyCustomTagColumn")]
    public string Tag { get; set; }

    [Column("MyCustomMessageColumn")]
    public string Message { get; set; }

    [Column("MyCustomSeverityColumn")]
    public LogLevel Severity { get; set; }
}
```

**Using in ASP.NET or a Web API**

There are two additional classes that integrate the Easy Logger Service with ASP.NET based projects.

To use with such a project simply use the ```EasyWebLogProvider``` class in your ```ConfigureServices()``` method as follows.

```
public void ConfigureServices(IServiceCollection services) {
    // Your other startup stuff

    var loggingConfiguration = new LoggingConfiguration() {
        LogDirectory = "C:\\MyLogDirectory"
    };

    var logLevels = new LogLevel[] { LogLevel.Warning, LogLevel.Error, LogLevel.Critical };

    services.AddLogging(logging => 
    {
        logging.ClearProviders();
        logging.AddProvider(new EasyWebLogProvider(loggingConfiguration, logLevels));
    });
}
```

## Authors

* **Nathanael Frey**

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details

## Acknowledgments

Thank you to:
* [Kmg Design](https://www.iconfinder.com/kmgdesignid) for the project icon