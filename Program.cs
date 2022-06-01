// See https://aka.ms/new-console-template for more information
class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");

        var port = args.Length > 0 ? args[0] : "COM1";

        using var s16 = new Suite16(port);
        while (true)
        {
            var key = Console.ReadKey();
            if (key.KeyChar == 'q') break;
        }

        Console.WriteLine("Closing");
    }
}