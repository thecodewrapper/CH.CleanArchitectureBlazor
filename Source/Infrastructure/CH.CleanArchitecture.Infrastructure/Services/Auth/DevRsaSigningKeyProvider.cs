using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using CH.CleanArchitecture.Core.Application;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace CH.CleanArchitecture.Infrastructure.Services
{
    /// <summary>
    /// Development-only RSA signing key provider that reads the private key from a PEM file.
    /// If the PEM does not exist, it is generated and written to disk.
    /// </summary>
    public sealed class DevRsaSigningKeyProvider : ISigningKeyProvider
    {
        private readonly string _pemPath;
        private RSA? _rsa;
        private RsaSecurityKey? _key;

        // simple in-proc lock; file lock handles cross-proc
        private static readonly object Sync = new();

        public DevRsaSigningKeyProvider(IConfiguration cfg) {
            _pemPath = cfg["IdentityProvider:DevPrivateKeyPemPath"] ?? throw new InvalidOperationException("IdentityProvider:DevPrivateKeyPemPath missing.");
        }

        public Task<SigningCredentials> GetSigningCredentialsAsync(CancellationToken ct) {
            EnsureLoaded();
            return Task.FromResult(new SigningCredentials(_key!, SecurityAlgorithms.RsaSha256));
        }

        public string GetKeyId() {
            EnsureLoaded();
            return _key!.KeyId!;
        }

        public SecurityKey GetValidationKey() {
            EnsureLoaded();
            return _key!;
        }

        private void EnsureLoaded() {
            if (_key != null) return;

            lock (Sync) {
                if (_key != null) return;

                EnsurePemExists();

                var pem = File.ReadAllText(_pemPath);

                _rsa = RSA.Create();
                _rsa.ImportFromPem(pem);

                _key = new RsaSecurityKey(_rsa)
                {
                    KeyId = ComputeKid(_rsa)
                };
            }
        }

        private void EnsurePemExists() {
            // Create directory if needed
            var dir = Path.GetDirectoryName(_pemPath);
            if (!string.IsNullOrWhiteSpace(dir))
                Directory.CreateDirectory(dir);

            if (File.Exists(_pemPath))
                return;

            // Cross-process safety: lock file while generating/writing
            var lockPath = _pemPath + ".lock";

            using var lockStream = new FileStream(
                lockPath,
                FileMode.OpenOrCreate,
                FileAccess.ReadWrite,
                FileShare.None);

            // Another process might have created it while we waited for the lock
            if (File.Exists(_pemPath))
                return;

            using var rsa = RSA.Create(2048);

            // Export PKCS#8 private key PEM: "BEGIN PRIVATE KEY"
            var privateKeyBytes = rsa.ExportPkcs8PrivateKey();
            var privatePem = PemEncoding.Write("PRIVATE KEY", privateKeyBytes);

            // Atomic write
            var tmp = _pemPath + "." + Guid.NewGuid().ToString("N") + ".tmp";
            File.WriteAllText(tmp, privatePem);

#if NET6_0_OR_GREATER
            File.Move(tmp, _pemPath, overwrite: false);
#else
            File.Move(tmp, _pemPath);
#endif
        }

        private static string ComputeKid(RSA rsa) {
            var pub = rsa.ExportSubjectPublicKeyInfo();
            var hash = SHA256.HashData(pub);
            return Base64UrlEncoder.Encode(hash);
        }
    }
}