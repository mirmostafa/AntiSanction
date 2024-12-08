using static AntiSanction.DnsManager;

try
{
    var ethernet = GetInterfaces().First(x => x.Name == "Ethernet");
    if (args.Length == 0)
    {
        Console.WriteLine("Re/set DNS Server to Shekan.");
        Console.WriteLine();
        Console.WriteLine("  (1): Show Current DNS.");
        Console.WriteLine("  (2): Set to shekan.");
        Console.WriteLine("  (3): Reset to default DNS (192.168.1.1).");
        Console.WriteLine("  (?): Show help.");
        Console.WriteLine("  Other keys: Exit.");
        Console.Write("Choose an item: ");
        var resp = Console.ReadKey();
        Console.WriteLine();
        if (resp.Key is ConsoleKey.D1 or ConsoleKey.NumPad1)
        {
            showCurrentDns(ethernet);
        }
        if (resp.Key is ConsoleKey.D2 or ConsoleKey.NumPad2)
        {
            setToShekan(ethernet);
        }
        if (resp.Key is ConsoleKey.D3 or ConsoleKey.NumPad3)
        {
            resetDns(ethernet);
        }
        if (resp.Key is ConsoleKey.Oem2 or ConsoleKey.H)
        {
            showHelp();
        }

        return;
    }
    if (args.Length == 1)
    {
        if (args[0] is "--info" or "-i" or "i")
        {
            showCurrentDns(ethernet);
            return;
        }
        if (args[0] is "--reset" or "-r" or "r")
        {
            resetDns(ethernet);
            return;
        }
        if (args[0] is "--set" or "-s" or "s")
        {
            setToShekan(ethernet);
            return;
        }
        if (args[0] is "--help" or "-h" or "h")
        {
            showHelp();
            return;
        }
    }
    showHelp();
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}

static void showCurrentDns(NetworkInterfaceInfo ethernet)
{
    var currentDnses = GetCurrentDnsByInterfaceId(ethernet.Id);
    foreach (var curr in currentDnses)
    {
        Console.WriteLine(curr);
    }
}

static void resetDns(NetworkInterfaceInfo ethernet)
{
    ChangeDnsByInterfaceId(ethernet.Id, "192.168.1.1");
    Console.WriteLine("Reset.");
}

static void setToShekan(NetworkInterfaceInfo ethernet)
{
    ChangeDnsByInterfaceId(ethernet.Id, "178.22.122.100", "185.51.200.2");
    Console.WriteLine("Changed to Shekan.");
}

static void showHelp()
{
    Console.WriteLine("--info\t-i i : Show current DNS.");
    Console.WriteLine("--set\t-s s : Set DNS to Shekan.");
    Console.WriteLine("--reset\t-r r : Reset DNS to default.");
}