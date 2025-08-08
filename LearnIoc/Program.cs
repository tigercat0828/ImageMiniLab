using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.CompilerServices;


var builder = new ServiceCollection();
builder.AddSingleton<ILogger, Logger>();
var services = builder.BuildServiceProvider();

var logger1 = services.GetService<ILogger>();
logger1!.Log("Halo");
logger1.ShowID();
var logger2 = services.GetService<ILogger>();
logger2!.Log("World");
logger2.ShowID();


Console.WriteLine(logger1 == logger2);

interface ILogger {
    void Log(string message);
    int ShowID();
}

public class Logger : ILogger {

    public int id { get; }
    public static int count;
    public Logger() {
        id = count++;
    }
    public void Log(string message) {
        Console.WriteLine($"Log: {message}");
    }
    public int ShowID() {
        Console.WriteLine(id);
        return id;
    }
}
