using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using MelonLoader;
using CardMaker.Common;
using CardMakerCustomizer.Config;

namespace CardMakerCustomizer.Logic
{
    /// <summary>
    /// Provides rich diagnostic logging, error analysis, and actionable hints
    /// for Sega CardMaker network packets across Ongeki, Chunithm, Maimai, and Common titles.
    /// </summary>
    public static class NetworkDiagnosticHelper
    {
        private static readonly Dictionary<string, DateTime> _lastErrorLogTimes = new Dictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);
        private static readonly object _lock = new object();

        /// <summary>
        /// Resets the error deduplication and rate-limiting tracking state.
        /// </summary>
        public static void ResetLoggingState()
        {
            Monitor.Enter(_lock);
            try
            {
                _lastErrorLogTimes.Clear();
            }
            finally
            {
                Monitor.Exit(_lock);
            }
        }

        /// <summary>
        /// Checks whether an error with the given key should be logged based on cooldown (default 5.0 seconds).
        /// Returns true if enough time has passed since the last log for this key.
        /// </summary>
        public static bool ShouldLogError(string key, double cooldownSeconds = 5.0)
        {
            if (string.IsNullOrEmpty(key)) return true;

            Monitor.Enter(_lock);
            try
            {
                DateTime now = DateTime.UtcNow;
                if (_lastErrorLogTimes.TryGetValue(key, out var lastTime))
                {
                    if ((now - lastTime).TotalSeconds < cooldownSeconds)
                    {
                        return false;
                    }
                }
                _lastErrorLogTimes[key] = now;
                return true;
            }
            finally
            {
                Monitor.Exit(_lock);
            }
        }

        public static void SafeLog(string msg)
        {
            try
            {
                MelonLogger.Msg(msg);
            }
            catch
            {
                Console.WriteLine(msg);
            }
        }

        public static void SafeWarn(string msg)
        {
            try
            {
                MelonLogger.Warning(msg);
            }
            catch
            {
                Console.WriteLine("[WARN] " + msg);
            }
        }

        public static void SafeError(string msg)
        {
            try
            {
                MelonLogger.Error(msg);
            }
            catch
            {
                Console.WriteLine("[ERROR] " + msg);
            }
        }

        /// <summary>
        /// Translates integer status values to descriptive human-readable text.
        /// </summary>
        public static string GetStatusDescription(int status)
        {
            switch (status)
            {
                case 0:
                    return "Success (OK)";
                case -1:
                    return "Socket Creation Error (Cannot establish network socket - server offline or address unreachable)";
                case -2:
                    return "Request Encoding Error (Failed to encode or serialize request message payload)";
                case -3:
                    return "Response Decoding Error (Server response body could not be decoded or was invalid JSON)";
                case -4:
                    return "Timeout (Server did not respond within configured timeout window)";
                case -5:
                    return "Connection Error (TCP connection dropped or actively refused)";
                case -6:
                    return "Internal System Error";
                case -7:
                    return "HTTP Status Error (Server returned non-200 HTTP error status)";
                default:
                    return StringCompat.Format("Unknown Status ({0})", status);
            }
        }

        /// <summary>
        /// Translates internal PacketBase.Status enum values to descriptive human-readable text.
        /// </summary>
        public static string GetStatusDescription(PacketBase.Status status)
        {
            return GetStatusDescription((int)status);
        }

        /// <summary>
        /// Provides actionable troubleshooting hints based on status, HTTP status, and error code.
        /// </summary>
        public static string GetDiagnosticHint(int status, int httpStatus, int errorCode)
        {
            switch (status)
            {
                case -1: // Error_Create
                    return "Server is unreachable. Check if your private server (e.g., Aqua, Artemis) is running, or verify server IP in segatools.ini / ALL.Net.";
                case -4: // Error_Timeout
                    return "Request timed out. Check network latency, server performance, or firewall settings.";
                case -7: // Error_HttpStatus
                    if (httpStatus == 404)
                    {
                        return "HTTP 404 Not Found. The server does not implement this API endpoint route. Verify server version and routes.";
                    }
                    if (httpStatus == 500)
                    {
                        return "HTTP 500 Internal Server Error. Check your server console/logs for stack trace or unhandled exception.";
                    }
                    if (httpStatus == 403)
                    {
                        return "HTTP 403 Forbidden. The server rejected the client request or credentials.";
                    }
                    return StringCompat.Format("HTTP error status {0}. Inspect server logs for details.", httpStatus);
                case -3: // Error_DecodeResponse
                    return "Failed to decode server response. Check if server returned HTML error page instead of JSON, or if payload encryption is mismatched.";
                case -5: // Error_Connection
                    return "Connection closed or refused by peer. Verify TLS/encryption bypass matches your server configuration.";
                case -2: // Error_EcodeRequest
                    return "Client failed to serialize request payload. Check game user state and packet query parameters.";
                default:
                    return null;
            }
        }

        /// <summary>
        /// Provides actionable troubleshooting hints based on PacketBase.Status enum.
        /// </summary>
        public static string GetDiagnosticHint(PacketBase.Status status, int httpStatus, int errorCode)
        {
            return GetDiagnosticHint((int)status, httpStatus, errorCode);
        }

        /// <summary>
        /// Safely truncates a long string for single-line log output.
        /// </summary>
        public static string TruncatePayload(string payload, int maxLen = 200)
        {
            if (string.IsNullOrEmpty(payload)) return "(empty)";
            string clean = payload.Replace("\r", " ").Replace("\n", " ").Trim();
            if (clean.Length <= maxLen) return clean;
            return clean.Substring(0, maxLen) + "...";
        }

        /// <summary>
        /// Safely extracts the packet class name.
        /// </summary>
        public static string GetPacketName(PacketBase packet)
        {
            if (object.ReferenceEquals(packet, null)) return "UnknownPacket";
            return packet.GetType().Name;
        }

        /// <summary>
        /// Safely extracts the target server name (e.g. Ongeki, Chunithm, Maimai, CardMaker).
        /// </summary>
        public static string GetServerName(PacketBase packet)
        {
            if (object.ReferenceEquals(packet, null)) return "UnknownServer";
            try
            {
                var traverse = HarmonyLib.Traverse.Create(packet);
                var s = traverse.Field("server_").GetValue<CommonStatic.Server>();
                return s.ToString();
            }
            catch
            {
                return "UnknownServer";
            }
        }

        /// <summary>
        /// Safely extracts the INetQuery from a packet instance.
        /// </summary>
        public static INetQuery GetQuery(PacketBase packet)
        {
            if (object.ReferenceEquals(packet, null)) return null;
            try
            {
                return HarmonyLib.Traverse.Create(packet).Field("query_").GetValue<INetQuery>();
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Safely extracts the HTTP status from a packet.
        /// </summary>
        public static int GetHttpStatus(PacketBase packet)
        {
            if (object.ReferenceEquals(packet, null)) return -1;
            try
            {
                return packet.httpStatus;
            }
            catch
            {
                return -1;
            }
        }

        /// <summary>
        /// Safely extracts the errorCode_ field from a packet.
        /// </summary>
        public static int GetErrorCode(PacketBase packet)
        {
            if (object.ReferenceEquals(packet, null)) return 0;
            try
            {
                return HarmonyLib.Traverse.Create(packet).Field("errorCode_").GetValue<int>();
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Safely extracts the retryCount_ field from a packet.
        /// </summary>
        public static int GetRetryCount(PacketBase packet)
        {
            if (object.ReferenceEquals(packet, null)) return 0;
            try
            {
                return HarmonyLib.Traverse.Create(packet).Field("retryCount_").GetValue<int>();
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Logs when a packet instance is initialized and created.
        /// </summary>
        public static void LogPacketCreated(PacketBase packet, INetQuery query, string host, ushort port, string basePath, CommonStatic.Server server, bool success)
        {
            if (ModConfig.EnableNetworkLogging != null && !ModConfig.EnableNetworkLogging.Value)
            {
                return;
            }

            string packetName = GetPacketName(packet);
            string serverName = server.ToString();
            string endpoint = (query != null && !string.IsNullOrEmpty(query.URL)) ? query.URL : "(none)";

            if (!success)
            {
                SafeWarn(StringCompat.Format(
                    "[CardMakerCustomizer] [Network] [WARN] [{0}] Failed to create client for {1} (Host: '{2}:{3}', BasePath: '{4}', Endpoint: '{5}')",
                    serverName, packetName, host, port, basePath, endpoint));
                return;
            }

            if (ModConfig.VerboseNetworkLogging != null && ModConfig.VerboseNetworkLogging.Value)
            {
                SafeLog(StringCompat.Format(
                    "[CardMakerCustomizer] [Network] [INIT] [{0}] Initialized {1} -> Host: {2}:{3}, BasePath: '{4}', Endpoint: '{5}'",
                    serverName, packetName, host, port, basePath, endpoint));
            }
        }

        /// <summary>
        /// Handles state transitions during PacketBase.procImpl().
        /// Prevents duplicate/spam logging on consecutive identical frames.
        /// </summary>
        public static void HandlePacketStateTransition(PacketBase packet, PacketBase.State oldState, PacketBase.State newState)
        {
            if (ModConfig.EnableNetworkLogging != null && !ModConfig.EnableNetworkLogging.Value)
            {
                return;
            }

            if (object.ReferenceEquals(packet, null)) return;
            if (oldState == newState) return; // Ignore unchanging frames

            string packetName = GetPacketName(packet);
            string serverName = GetServerName(packet);
            INetQuery query = GetQuery(packet);
            string url = query != null ? query.URL : "UnknownAPI";

            // 1. Packet request was just dispatched
            if (oldState == PacketBase.State.Ready && newState == PacketBase.State.Process)
            {
                long userId = query != null ? query.UserID : 0;
                bool compress = query != null && query.Compress;
                SafeLog(StringCompat.Format(
                    "[CardMakerCustomizer] [Network] [SEND] [{0}] {1} -> {2} (UserID: {3}, Compress: {4})",
                    serverName, packetName, url, userId, compress));

                if (ModConfig.VerboseNetworkLogging != null && ModConfig.VerboseNetworkLogging.Value && query != null)
                {
                    try
                    {
                        string reqBody = query.getRequest();
                        SafeLog(StringCompat.Format(
                            "[CardMakerCustomizer] [Network]   -> Request Payload: {0}",
                            TruncatePayload(reqBody, 300)));
                    }
                    catch { }
                }
                return;
            }

            // 2. Packet request succeeded
            if (newState == PacketBase.State.Done)
            {
                int httpStatus = GetHttpStatus(packet);
                SafeLog(StringCompat.Format(
                    "[CardMakerCustomizer] [Network] [RECV] [{0}] {1} -> Success (HTTP {2})",
                    serverName, packetName, httpStatus));
                return;
            }

            // 3. Packet request encountered an error
            if (newState == PacketBase.State.Error)
            {
                PacketBase.Status status = packet.status;
                int httpStatus = GetHttpStatus(packet);
                int errorCode = GetErrorCode(packet);

                string dedupKey = StringCompat.Format("{0}:{1}:{2}:{3}", packetName, serverName, (int)status, httpStatus);
                if (!ShouldLogError(dedupKey, 5.0))
                {
                    return; // Rate limit duplicate errors
                }

                string statusDesc = GetStatusDescription(status);
                string hint = GetDiagnosticHint(status, httpStatus, errorCode);

                SafeError(StringCompat.Format(
                    "[CardMakerCustomizer] [Network] [ERROR] [{0}] {1} ({2}) failed: {3} (HTTP: {4}, ErrorCode: {5})",
                    serverName, packetName, url, statusDesc, httpStatus, errorCode));

                if (!string.IsNullOrEmpty(hint))
                {
                    SafeWarn(StringCompat.Format(
                        "[CardMakerCustomizer] [Network]   -> Diagnostic: {0}", hint));
                }
                return;
            }

            // 4. Packet entered retry wait
            if (newState == PacketBase.State.RetryWait)
            {
                int retryCount = GetRetryCount(packet);
                SafeWarn(StringCompat.Format(
                    "[CardMakerCustomizer] [Network] [RETRY] [{0}] {1} -> Will retry (Attempt #{2})...",
                    serverName, packetName, retryCount));
                return;
            }
        }
    }
}
