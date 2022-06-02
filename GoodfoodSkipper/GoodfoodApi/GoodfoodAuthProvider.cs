using GoodfoodSkipper.GoogleIdentity;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace GoodfoodSkipper.GoodfoodApi
{
    internal class GoodfoodAuthProvider
    {
        private GoodfoodAuthSession? _cachedSession;
        private SemaphoreSlim _sessionSemaphore = new SemaphoreSlim(1, 1);
        private readonly GoogleIdentityToolkitClient _identityClient;
        private readonly string _username;
        private readonly string _password;

        public GoodfoodAuthProvider(GoogleIdentityToolkitClient identityClient, IOptions<GoodfoodAuthOptions> options)
        {
            _identityClient = identityClient;
            _username = options.Value.Username;
            _password = options.Value.Password;
        }

        public async Task<GoodfoodAuthSession> GetAuthSession()
        {
            await _sessionSemaphore.WaitAsync();

            if (_cachedSession != null && _cachedSession.ExpiresOn > DateTime.UtcNow.AddMinutes(5))
            {
                _sessionSemaphore.Release();
                return _cachedSession;
            }

            try
            {
                var verifyResponse = await _identityClient.VerifyPassword(new VerifyPasswordRequest
                {
                    Email = _username,
                    Password = _password,
                    ReturnSecureToken = true
                });

                var userId = ExtractUserIdFromIdToken(verifyResponse.IdToken);

                _cachedSession = new GoodfoodAuthSession
                {
                    IdToken = verifyResponse.IdToken,
                    UserId = userId,
                    ExpiresOn = DateTime.UtcNow.AddSeconds(int.Parse(verifyResponse.ExpiresIn))
                };
                return _cachedSession;
            }
            finally
            {
                _sessionSemaphore.Release();
            }
        }

        private static string ExtractUserIdFromIdToken(ReadOnlySpan<char> token)
        {
            var delimeterPos = token.IndexOf('.');
            var remaining = token.Slice(delimeterPos + 1);
            var finalDelimiterPos = remaining.IndexOf('.');
            var encodedClaims = remaining.Slice(0, finalDelimiterPos);

            Span<byte> claimBytes = new byte[encodedClaims.Length * 6 / 8];
            if (!Convert.TryFromBase64Chars(encodedClaims, claimBytes, out var _))
                throw new Exception("Error parsing UserId from IdToken");

            var claims = JsonSerializer.Deserialize<GoodfoodAuthTokenCliams>(claimBytes);

            if (claims == null)
                throw new Exception("Error deserializing claims from IdToken");

            var userId = claims.gf_user_id;
            if (string.IsNullOrEmpty(userId))
                throw new Exception("Error parsing UserId from Idtoken");

            return userId;
        }

    }

    internal class GoodfoodAuthSession
    {
        public string IdToken { get; set; }
        public DateTime ExpiresOn { get; set; }
        public string UserId { get; set; }
    }

    internal class GoodfoodAuthTokenCliams
    {
        public string gf_user_id { get; set; }
    }
}
