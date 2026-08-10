using System.Security.Cryptography;

namespace PortsAppGui
{
    public static class TokenGenerator
    {
        private const string Alphabet = "abcdefghijkmnopqrstuvwxyzABCDEFGHJKLMNPQRSTUVWXYZ23456789";

        /// <summary>Creates a shared secret for a rathole service.</summary>
        public static string Create(int length = 24)
        {
            var characters = new char[length];
            for (var i = 0; i < length; i++)
                characters[i] = Alphabet[RandomNumberGenerator.GetInt32(Alphabet.Length)];

            return new string(characters);
        }
    }
}
