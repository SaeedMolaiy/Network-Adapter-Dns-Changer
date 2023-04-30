using System.Net.NetworkInformation;
using System.Management;

namespace DnsChanger
{
    internal static class NetworkManagement
    {
        public static bool SetDnsServers(string preferredDnsServer, string alternateDnsServer)
        {
            try
            {
                var activeInterface =
                    NetworkInterface.GetAllNetworkInterfaces()
                        .FirstOrDefault(n => !(n.OperationalStatus != OperationalStatus.Up ||
                                               n.NetworkInterfaceType == NetworkInterfaceType.Loopback));

                if (activeInterface == null)
                {
                    Console.WriteLine("No active network interface found.");
                    return false;
                }

                ManagementObjectSearcher searcher =
                    new($"SELECT * FROM Win32_NetworkAdapterConfiguration WHERE InterfaceIndex=" +
                        $"{activeInterface.GetIPProperties().GetIPv4Properties().Index}");

                var adapterConfig = searcher.Get().OfType<ManagementObject>().FirstOrDefault();

                if (adapterConfig == null)
                {
                    Console.WriteLine("Failed to get network adapter configuration.");
                    return false;
                }

                adapterConfig.InvokeMethod("SetDNSServerSearchOrder",
                    new object[] { new[] { preferredDnsServer, alternateDnsServer } });

                Console.WriteLine($"DNS servers set to {preferredDnsServer} (preferred) and {alternateDnsServer} (alternate).");

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to set DNS servers: {ex.Message}");
                return false;
            }
        }
    }
}