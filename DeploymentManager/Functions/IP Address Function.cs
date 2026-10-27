// Copyright © - Unpublished - Toby Hunter
using System.Net;

namespace DeploymentManager.Functions
{
    public static class IPAddressFunction
    {
        private static readonly string[] CloudflareIPv4Ranges =
        {
            "173.245.48.0/20", "103.21.244.0/22", "103.22.200.0/22",
            "103.31.4.0/22", "141.101.64.0/18", "108.162.192.0/18",
            "190.93.240.0/20", "188.114.96.0/20", "197.234.240.0/22",
            "198.41.128.0/17", "162.158.0.0/15", "104.16.0.0/13",
            "104.24.0.0/14", "172.64.0.0/13", "131.0.72.0/22"
        };

        /// <summary>
        /// Returns the IP address for logging.
        /// </summary>
        public static string FetchIpAddress(IHttpContextAccessor contextAccessor)
        {
            string ipAddress = string.Empty;
            string directIp = contextAccessor?.HttpContext?.Connection?.RemoteIpAddress?.ToString();

            if (!string.IsNullOrWhiteSpace(directIp) && IsCloudflareIP(directIp) && contextAccessor?.HttpContext?.Request.Headers.ContainsKey("CF-Connecting-IP") == true)
            {
                ipAddress = contextAccessor.HttpContext.Request.Headers["CF-Connecting-IP"].ToString();
            }

            else if (contextAccessor?.HttpContext?.Request.Headers.ContainsKey("X-Forwarded-For") == true)
            {
                ipAddress = contextAccessor.HttpContext.Request.Headers["X-Forwarded-For"].ToString()
                    .Split(',')[0]
                    .Trim();
            }

            else if (!string.IsNullOrWhiteSpace(directIp))
            {
                ipAddress = directIp;
            }

            return ipAddress;
        }

        /// <summary>
        /// Returns whether the given IP address belongs to a Cloudflare range.
        /// </summary>
        private static bool IsCloudflareIP(string ipAddress)
        {
            if (!IPAddress.TryParse(
                ipAddress,
                out IPAddress ip))
            {
                return false;
            }

            foreach (string range in CloudflareIPv4Ranges)
            {
                if (IsInRange(
                    ip,
                    range))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Returns whether the given IP address falls within the specified CIDR range.
        /// </summary>
        private static bool IsInRange(
            IPAddress address,
            string cidr)
        {
            string[] parts = cidr.Split('/');
            IPAddress networkAddress = IPAddress.Parse(parts[0]);
            int prefixLength = int.Parse(parts[1]);

            byte[] addressBytes = address.GetAddressBytes();
            byte[] networkBytes = networkAddress.GetAddressBytes();

            if (addressBytes.Length != networkBytes.Length)
            {
                return false;
            }

            int fullBytes = prefixLength / 8;
            int remainingBits = prefixLength % 8;

            for (int i = 0; i < fullBytes; i++)
            {
                if (addressBytes[i] != networkBytes[i])
                {
                    return false;
                }
            }

            if (remainingBits > 0)
            {
                byte mask = (byte)(0xFF << (8 - remainingBits));

                if ((addressBytes[fullBytes] & mask) != (networkBytes[fullBytes] & mask))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
