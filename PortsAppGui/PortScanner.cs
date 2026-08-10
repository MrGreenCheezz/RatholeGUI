using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;

namespace PortsAppGui
{
    /// <summary>A port some local application is listening on, together with the owning process.</summary>
    public sealed class ListeningEndpoint
    {
        public required string Protocol { get; init; }
        public required IPAddress Address { get; init; }
        public required int Port { get; init; }
        public required int Pid { get; init; }
        public required string ProcessName { get; init; }
        public string Description { get; init; } = "";
        public string ExecutablePath { get; init; } = "";

        /// <summary>True when the socket is bound to every interface (0.0.0.0 / ::).</summary>
        public bool BindsAnyAddress => Address.Equals(IPAddress.Any) || Address.Equals(IPAddress.IPv6Any);

        public string AddressDisplay => BindsAnyAddress ? "0.0.0.0 (any)" : Address.ToString();

        /// <summary>Address to put into a rathole client service for this endpoint.</summary>
        public string SuggestedLocalAddress => BindsAnyAddress || IPAddress.IsLoopback(Address)
            ? "127.0.0.1"
            : Address.ToString();

        public string Hint => WellKnownPorts.Describe(Port, Protocol);

        public string Label => $"{ProcessName} — {SuggestedLocalAddress}:{Port}/{Protocol}";
    }

    public static class PortScanner
    {
        /// <summary>
        /// Lists every listening TCP socket and bound UDP socket on this machine with its owning process.
        /// Rows for the same process/protocol/port are merged so the picker stays readable.
        /// </summary>
        public static List<ListeningEndpoint> Scan(bool includeUdp = true)
        {
            var rows = new List<(string Protocol, IPAddress Address, int Port, int Pid)>();
            rows.AddRange(GetTcpRows(AfInet));
            rows.AddRange(GetTcpRows(AfInet6));

            if (includeUdp)
            {
                rows.AddRange(GetUdpRows(AfInet));
                rows.AddRange(GetUdpRows(AfInet6));
            }

            var processes = BuildProcessInfo(rows.Select(row => row.Pid).Distinct());

            return rows
                .GroupBy(row => (row.Pid, row.Protocol, row.Port))
                .Select(group =>
                {
                    var info = processes.TryGetValue(group.Key.Pid, out var found)
                        ? found
                        : new ProcessInfo($"pid {group.Key.Pid}", "", "");

                    return new ListeningEndpoint
                    {
                        Protocol = group.Key.Protocol,
                        Port = group.Key.Port,
                        Pid = group.Key.Pid,
                        Address = PickAddress(group.Select(row => row.Address)),
                        ProcessName = info.Name,
                        Description = info.Description,
                        ExecutablePath = info.Path
                    };
                })
                .OrderBy(endpoint => endpoint.ProcessName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(endpoint => endpoint.Port)
                .ToList();
        }

        private static IPAddress PickAddress(IEnumerable<IPAddress> addresses)
        {
            var list = addresses.ToList();
            if (list.Any(address => address.Equals(IPAddress.Any) || address.Equals(IPAddress.IPv6Any)))
                return IPAddress.Any;

            return list.FirstOrDefault(IPAddress.IsLoopback)
                   ?? list.FirstOrDefault(address => address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                   ?? list[0];
        }

        private sealed record ProcessInfo(string Name, string Description, string Path);

        /// <summary>
        /// Resolves PIDs to names as unintrusively as possible: one snapshot of the process list for
        /// the names, QueryFullProcessImageName for the paths and version info read from the file on
        /// disk. Deliberately avoids Process.MainModule, which enumerates the modules loaded inside
        /// another process — slow, blocked for protected processes, and something other software on
        /// the machine can react badly to.
        /// </summary>
        private static Dictionary<int, ProcessInfo> BuildProcessInfo(IEnumerable<int> pids)
        {
            var names = new Dictionary<int, string>();
            foreach (var process in Process.GetProcesses())
            {
                try
                {
                    names[process.Id] = process.ProcessName;
                }
                catch
                {
                    // The process died between the snapshot and this read.
                }
                finally
                {
                    process.Dispose();
                }
            }

            var descriptions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var result = new Dictionary<int, ProcessInfo>();

            foreach (var pid in pids)
            {
                if (pid <= 4)
                {
                    result[pid] = new ProcessInfo(pid == 0 ? "System Idle" : "System", "Windows kernel", "");
                    continue;
                }

                var name = names.TryGetValue(pid, out var found) ? found : $"pid {pid}";
                var path = TryGetExecutablePath(pid);

                if (!descriptions.TryGetValue(path, out var description))
                {
                    description = TryGetFileDescription(path);
                    descriptions[path] = description;
                }

                result[pid] = new ProcessInfo(name, description, path);
            }

            return result;
        }

        private static string TryGetExecutablePath(int pid)
        {
            var handle = OpenProcess(ProcessQueryLimitedInformation, false, pid);
            if (handle == IntPtr.Zero)
                return "";

            try
            {
                var buffer = new StringBuilder(1024);
                var size = buffer.Capacity;
                return QueryFullProcessImageName(handle, 0, buffer, ref size) ? buffer.ToString() : "";
            }
            catch
            {
                return "";
            }
            finally
            {
                CloseHandle(handle);
            }
        }

        private static string TryGetFileDescription(string path)
        {
            if (string.IsNullOrEmpty(path))
                return "";

            try
            {
                return FileVersionInfo.GetVersionInfo(path).FileDescription ?? "";
            }
            catch
            {
                return "";
            }
        }

        #region kernel32

        private const int ProcessQueryLimitedInformation = 0x1000;

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr OpenProcess(int desiredAccess, [MarshalAs(UnmanagedType.Bool)] bool inheritHandle,
            int processId);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr handle);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool QueryFullProcessImageName(IntPtr process, int flags, StringBuilder exeName,
            ref int size);

        #endregion

        #region iphlpapi

        private const int AfInet = 2;
        private const int AfInet6 = 23;
        private const int TcpTableOwnerPidListener = 3;
        private const int UdpTableOwnerPid = 1;
        private const uint ErrorInsufficientBuffer = 122;

        [DllImport("iphlpapi.dll", SetLastError = true)]
        private static extern uint GetExtendedTcpTable(IntPtr table, ref int size, bool order, int addressFamily,
            int tableClass, int reserved);

        [DllImport("iphlpapi.dll", SetLastError = true)]
        private static extern uint GetExtendedUdpTable(IntPtr table, ref int size, bool order, int addressFamily,
            int tableClass, int reserved);

        [StructLayout(LayoutKind.Sequential)]
        private struct MibTcpRowOwnerPid
        {
            public uint State;
            public uint LocalAddress;
            public uint LocalPort;
            public uint RemoteAddress;
            public uint RemotePort;
            public uint OwningPid;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MibTcp6RowOwnerPid
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public byte[] LocalAddress;
            public uint LocalScopeId;
            public uint LocalPort;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public byte[] RemoteAddress;
            public uint RemoteScopeId;
            public uint RemotePort;
            public uint State;
            public uint OwningPid;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MibUdpRowOwnerPid
        {
            public uint LocalAddress;
            public uint LocalPort;
            public uint OwningPid;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MibUdp6RowOwnerPid
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public byte[] LocalAddress;
            public uint LocalScopeId;
            public uint LocalPort;
            public uint OwningPid;
        }

        private static List<(string Protocol, IPAddress Address, int Port, int Pid)> GetTcpRows(int addressFamily)
        {
            return ReadTable(addressFamily, TcpTableOwnerPidListener, isTcp: true);
        }

        private static List<(string Protocol, IPAddress Address, int Port, int Pid)> GetUdpRows(int addressFamily)
        {
            return ReadTable(addressFamily, UdpTableOwnerPid, isTcp: false);
        }

        private static List<(string Protocol, IPAddress Address, int Port, int Pid)> ReadTable(int addressFamily,
            int tableClass, bool isTcp)
        {
            var rows = new List<(string, IPAddress, int, int)>();
            var protocol = isTcp ? "TCP" : "UDP";
            var size = 0;

            var status = isTcp
                ? GetExtendedTcpTable(IntPtr.Zero, ref size, false, addressFamily, tableClass, 0)
                : GetExtendedUdpTable(IntPtr.Zero, ref size, false, addressFamily, tableClass, 0);

            if (status != ErrorInsufficientBuffer && status != 0)
                return rows;

            if (size <= 0)
                return rows;

            var buffer = Marshal.AllocHGlobal(size);
            try
            {
                status = isTcp
                    ? GetExtendedTcpTable(buffer, ref size, false, addressFamily, tableClass, 0)
                    : GetExtendedUdpTable(buffer, ref size, false, addressFamily, tableClass, 0);

                if (status != 0)
                    return rows;

                var count = Marshal.ReadInt32(buffer);
                var cursor = buffer + 4;

                for (var i = 0; i < count; i++)
                {
                    if (isTcp && addressFamily == AfInet)
                    {
                        var row = Marshal.PtrToStructure<MibTcpRowOwnerPid>(cursor);
                        rows.Add((protocol, new IPAddress(row.LocalAddress), ToPort(row.LocalPort), (int)row.OwningPid));
                        cursor += Marshal.SizeOf<MibTcpRowOwnerPid>();
                    }
                    else if (isTcp)
                    {
                        var row = Marshal.PtrToStructure<MibTcp6RowOwnerPid>(cursor);
                        rows.Add((protocol, new IPAddress(row.LocalAddress), ToPort(row.LocalPort), (int)row.OwningPid));
                        cursor += Marshal.SizeOf<MibTcp6RowOwnerPid>();
                    }
                    else if (addressFamily == AfInet)
                    {
                        var row = Marshal.PtrToStructure<MibUdpRowOwnerPid>(cursor);
                        rows.Add((protocol, new IPAddress(row.LocalAddress), ToPort(row.LocalPort), (int)row.OwningPid));
                        cursor += Marshal.SizeOf<MibUdpRowOwnerPid>();
                    }
                    else
                    {
                        var row = Marshal.PtrToStructure<MibUdp6RowOwnerPid>(cursor);
                        rows.Add((protocol, new IPAddress(row.LocalAddress), ToPort(row.LocalPort), (int)row.OwningPid));
                        cursor += Marshal.SizeOf<MibUdp6RowOwnerPid>();
                    }
                }
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }

            return rows.Where(row => row.Item3 > 0).ToList();
        }

        /// <summary>Ports come back in network byte order packed into a DWORD.</summary>
        private static int ToPort(uint value)
        {
            var bytes = BitConverter.GetBytes(value);
            return (bytes[0] << 8) | bytes[1];
        }

        #endregion
    }

    internal static class WellKnownPorts
    {
        private static readonly Dictionary<int, string> Names = new()
        {
            [20] = "FTP data", [21] = "FTP", [22] = "SSH", [23] = "Telnet", [25] = "SMTP",
            [53] = "DNS", [80] = "HTTP", [110] = "POP3", [123] = "NTP", [143] = "IMAP",
            [443] = "HTTPS", [445] = "SMB", [465] = "SMTPS", [587] = "SMTP submission",
            [993] = "IMAPS", [995] = "POP3S", [1194] = "OpenVPN", [1433] = "MSSQL",
            [1521] = "Oracle DB", [1723] = "PPTP", [1883] = "MQTT", [2049] = "NFS",
            [2333] = "rathole control", [2375] = "Docker", [2376] = "Docker TLS",
            [3000] = "dev server / Grafana", [3128] = "Squid proxy", [3306] = "MySQL",
            [3389] = "RDP", [4000] = "dev server", [4200] = "Angular dev", [5000] = "dev server",
            [5173] = "Vite dev", [5432] = "PostgreSQL", [5900] = "VNC", [5938] = "TeamViewer",
            [6379] = "Redis", [6881] = "BitTorrent", [7777] = "game server", [8000] = "HTTP alt",
            [8080] = "HTTP alt", [8081] = "HTTP alt", [8443] = "HTTPS alt", [8888] = "HTTP alt",
            [9000] = "HTTP alt / PHP-FPM", [9090] = "Prometheus", [9200] = "Elasticsearch",
            [11211] = "Memcached", [19132] = "Minecraft Bedrock", [25565] = "Minecraft Java",
            [27015] = "Source engine", [27017] = "MongoDB", [28015] = "Rust game",
            [3479] = "PlayStation", [5060] = "SIP", [64738] = "Mumble"
        };

        public static string Describe(int port, string protocol)
        {
            if (Names.TryGetValue(port, out var name))
                return name;

            if (port >= 49152)
                return "ephemeral";

            return protocol == "UDP" && port == 137 ? "NetBIOS" : "";
        }
    }
}
