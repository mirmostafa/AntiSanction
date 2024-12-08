using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace AntiSanction;

public class DnsManager
{
    /// <summary>
    /// Changes DNS settings for the specified network interface.
    /// </summary>
    /// <param name="interfaceId">The unique ID of the network interface.</param>
    /// <param name="dnsAddresses">The new DNS addresses to apply.</param>
    public static void ChangeDnsByInterfaceId(string interfaceId, params string[] dnsAddresses)
    {
        var networkInterface = NetworkInterface.GetAllNetworkInterfaces()
            .FirstOrDefault(ni => ni.Id == interfaceId) ?? throw new ArgumentException("Network interface not found.");
        var processStartInfo = new ProcessStartInfo
        {
            FileName = "netsh",
            Arguments = $"interface ip set dns \"{networkInterface.Name}\" static {dnsAddresses[0]}",
            Verb = "runas",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        ExecuteProcess(processStartInfo);

        if (dnsAddresses.Length > 1)
        {
            for (int i = 1; i < dnsAddresses.Length; i++)
            {
                processStartInfo.Arguments = $"interface ip add dns \"{networkInterface.Name}\" {dnsAddresses[i]} index={i + 1}";
                ExecuteProcess(processStartInfo);
            }
        }

        // Flush the DNS cache
        var flushDnsProcess = new ProcessStartInfo
        {
            FileName = "ipconfig",
            Arguments = "/flushdns",
            Verb = "runas",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        ExecuteProcess(flushDnsProcess);
    }

    /// <summary>
    /// Gets the current DNS settings for the specified network interface.
    /// </summary>
    /// <param name="interfaceId">The unique ID of the network interface.</param>
    /// <returns>List of current DNS addresses.</returns>
    public static IEnumerable<string> GetCurrentDnsByInterfaceId(string interfaceId)
    {
        var networkInterface = NetworkInterface.GetAllNetworkInterfaces()
            .FirstOrDefault(ni => ni.Id == interfaceId) ?? throw new ArgumentException("Network interface not found.");
        var ipProperties = networkInterface.GetIPProperties();
        return ipProperties.DnsAddresses
            .Where(ip => ip.AddressFamily == AddressFamily.InterNetwork)
            .Select(ip => ip.ToString());
    }

    /// <summary>
    /// Retrieves a list of available network interfaces.
    /// </summary>
    /// <returns>List of network interfaces.</returns>
    public static IEnumerable<NetworkInterfaceInfo> GetInterfaces() => 
        NetworkInterface.GetAllNetworkInterfaces()
            .Select(ni => new NetworkInterfaceInfo
            {
                Id = ni.Id,
                Name = ni.Name,
                Description = ni.Description
            });

    /// <summary>
    /// Resets DNS settings for the specified network interface to automatic (DHCP).
    /// </summary>
    /// <param name="interfaceId">The unique ID of the network interface.</param>
    public static void ResetDns(string interfaceId)
    {
        var networkInterface = NetworkInterface.GetAllNetworkInterfaces()
            .FirstOrDefault(ni => ni.Id == interfaceId) ?? throw new ArgumentException("Network interface not found.");
        var processStartInfo = new ProcessStartInfo
        {
            FileName = "netsh",
            Arguments = $"interface ip set dns \"{networkInterface.Name}\" dhcp",
            Verb = "runas",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        ExecuteProcess(processStartInfo);
    }

    /// <summary>
    /// Executes a system command with the specified settings.
    /// </summary>
    /// <param name="processStartInfo">Process start information.</param>
    private static void ExecuteProcess(ProcessStartInfo processStartInfo)
    {
        using var process = Process.Start(processStartInfo) ?? throw new InvalidOperationException("Failed to start process.");
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            var error = process.StandardError.ReadToEnd();
            throw new InvalidOperationException($"Command failed: {error}");
        }
    }

    public class NetworkInterfaceInfo
    {
        public required string Description { get; set; }
        public required string Id { get; set; }
        public required string Name { get; set; }
    }
}