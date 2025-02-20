using System.Text;
using System.Text.Json;
using System.Security.Cryptography;

namespace ArmaLogBackend.Licensing;

/// <summary>
/// Validates a license JSON with usage constraints + remote kill. 
/// If invalid or usage exceeded or remote-killed, returns false.
/// </summary>
public static class LicensingService
{
    private static readonly string RsaPublicKeyPem = @"
-----BEGIN PUBLIC KEY-----
MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAt...
-----END PUBLIC KEY-----
";

    private static readonly string RemoteKillUrl = "https://your-licensing-server.com/api/checkKill";

    public static bool ValidateAndConsumeLicense(string licenseJson)
    {
        try
        {
            var doc = JsonDocument.Parse(licenseJson);
            var root = doc.RootElement;
            var licenseData = root.GetProperty("LicenseData").GetString() ?? "";
            var signatureBase64 = root.GetProperty("Signature").GetString() ?? "";
            var signature = Convert.FromBase64String(signatureBase64);

            // 1) Check RSA signature
            using var rsa = RSA.Create();
            rsa.ImportFromPem(RsaPublicKeyPem);

            var dataBytes = Encoding.UTF8.GetBytes(licenseData);
            bool validSig = rsa.VerifyData(dataBytes, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            if (!validSig) return false;

            // 2) Parse license data => type, max usage, used count, expires
            var dataDoc = JsonDocument.Parse(licenseData);
            var type = dataDoc.RootElement.GetProperty("Type").GetString() ?? "Unknown";
            var maxUsage = dataDoc.RootElement.GetProperty("MaxUsage").GetInt32();
            var usedCount = dataDoc.RootElement.GetProperty("UsedCount").GetInt32();
            var expires = dataDoc.RootElement.GetProperty("Expires").GetDateTime();

            // 3) Check expiry
            if (expires < DateTime.UtcNow) return false;

            // 4) Generate licenseId from signature
            var licenseId = Convert.ToBase64String(SHA256.HashData(signature));

            // 5) Remote kill check
            if (IsLicenseRemotelyKilled(licenseId))
            {
                return false;
            }

            // 6) Usage store => load usage, check constraints
            var usage = LicenseUsageStore.GetOrCreateUsage(licenseId, type, maxUsage, expires);

            if (usage.Expires < DateTime.UtcNow) return false;
            if (usage.UsedCount >= usage.MaxUsage) return false;

            // increment usage
            usage.UsedCount += 1;
            LicenseUsageStore.SaveUsage(licenseId, usage);

            return true;
        }
        catch
        {
            return false;
        }
    }

    private static bool IsLicenseRemotelyKilled(string licenseId)
    {
        try
        {
            using var client = new HttpClient();
            var response = client.GetStringAsync($"{RemoteKillUrl}?licenseId={Uri.EscapeDataString(licenseId)}").Result;
            return response.Trim().ToLower() == "true";
        }
        catch
        {
            // If can't reach the server, assume not killed or fail => your choice
            return false;
        }
    }
}
