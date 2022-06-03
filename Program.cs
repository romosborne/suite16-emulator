// See https://aka.ms/new-console-template for more information
using Microsoft.Extensions.Logging;

class Program {
    static void Main(string[] args) {
        using var loggerFactory = LoggerFactory.Create(b => {
            b.AddFilter("Microsoft", LogLevel.Warning)
            .AddFilter("System", LogLevel.Warning)
            .AddFilter("Program", LogLevel.Debug)
            .AddConsole();
        });

        var logger = loggerFactory.CreateLogger<Program>();
        logger.LogInformation("Hello world!");
        logger.LogInformation("Press q to quit");

        var port = args.Length > 0 ? args[0] : "COM1";

        using var s16 = new Suite16(port, loggerFactory.CreateLogger<Suite16>());
        while (true) {
            var key = Console.ReadKey();
            if (key.KeyChar == 'q') break;
        }

        logger.LogInformation("Closing");
    }
}