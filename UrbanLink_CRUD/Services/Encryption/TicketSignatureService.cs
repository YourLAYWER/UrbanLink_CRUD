using System.Security.Cryptography;
using System.Text;

namespace UrbanLink_CRUD.Services.Encryption
{
    public class TicketSignatureService
    {
        private readonly string _base64PrivateKey;

        public TicketSignatureService(IConfiguration config)
        {
            // We load the Private Key from your appsettings.json.
            // NEVER hardcode this key or send it to the Android app.
            _base64PrivateKey = config["Cryptography:EcdsaPrivateKey"]
                ?? throw new ArgumentNullException("Private Key is missing!");
        }

        public string GenerateSecureQrCode(string ticketId, long expiryUnixTimestamp, int routeId)
        {
            // 1. COMBINE THE DATA: We use a pipe (|) symbol to separate the data fields.
            // Example output: "550e8400-e29b-41d4-a716-446655440000|1735689600|1"
            string payload = $"{ticketId}|{expiryUnixTimestamp}|{routeId}";
            byte[] payloadBytes = Encoding.UTF8.GetBytes(payload);

            // 2. LOAD THE PRIVATE KEY: We create the Elliptic Curve cryptography object
            // and feed it your secret Private Key.
            using var ecdsa = ECDsa.Create();
            byte[] privateKeyBytes = Convert.FromBase64String(_base64PrivateKey);
            ecdsa.ImportPkcs8PrivateKey(privateKeyBytes, out _);

            // 3. SIGN THE DATA: We hash the payload using SHA256, then sign it.
            // CRITICAL COMMENT: You MUST use DSASignatureFormat.Rfc3279DerSequence. 
            // By default, C# uses a different math format than Android. This line forces 
            // C# to format the signature so Android's Java engine can read it perfectly.
            byte[] signatureBytes = ecdsa.SignData(
                payloadBytes,
                HashAlgorithmName.SHA256,
                DSASignatureFormat.Rfc3279DerSequence
            );

            // 4. CONVERT TO TEXT: Turn the mathematical signature bytes into a readable string
            string base64Signature = Convert.ToBase64String(signatureBytes);

            // 5. THE FINAL QR CODE: We glue the raw data and the signature together with a dot (.)
            // The Android app will split this string at the dot.
            return $"{payload}.{base64Signature}";
        }
    }
}
