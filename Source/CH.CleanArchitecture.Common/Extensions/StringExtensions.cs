using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Components;

namespace CH.CleanArchitecture.Common
{
    public static class StringExtensions
    {
        public static T ToEnum<T>(this string value) where T : struct {
            if (!Enum.TryParse<T>(value, out var enumeration)) {
                return default;
            }
            return enumeration;
        }

        public static MarkupString ToMarkupString(this string value) {
            return new MarkupString(value);
        }

        public static string ToInitials(this string value) {
            if (string.IsNullOrWhiteSpace(value)) {
                return string.Empty;
            }

            var builder = new StringBuilder();

            var words = value.Split(" ");
            foreach (var word in words) {
                builder.Append(word.Substring(0, 1));
            }

            return builder.ToString().ToUpper();
        }

        public static string TrimStart(this string target, string trimString) {
            if (string.IsNullOrEmpty(trimString)) return target;

            string result = target;
            while (result.StartsWith(trimString)) {
                result = result.Substring(trimString.Length);
            }

            return result;
        }

        public static string Base64Encode(this string value) {
            var bytes = Encoding.UTF8.GetBytes(value);
            var s = Convert.ToBase64String(bytes); // Regular base64 encoder
            return s;
        }

        public static string Base64Decode(this string value) {
            var bytes = Convert.FromBase64String(value); // Standard base64 decoder
            return Encoding.UTF8.GetString(bytes);
        }

        public static string Base64UrlEncode(this string value) {
            var s = Base64Encode(value);
            s = s.Split('=')[0]; // Remove any trailing '='s
            s = s.Replace('+', '-'); // 62nd char of encoding
            s = s.Replace('/', '_'); // 63rd char of encoding
            return s;
        }

        public static string Base64UrlDecode(this string value) {
            var s = value;
            s = s.Replace('-', '+'); // 62nd char of encoding
            s = s.Replace('_', '/'); // 63rd char of encoding
            switch (s.Length % 4) // Pad with trailing '='s
            {
                case 0:
                    break; // No pad chars in this case
                case 2:
                    s += "==";
                    break; // Two pad chars
                case 3:
                    s += "=";
                    break; // One pad char
                default:
                    throw new Exception("Illegal base64 url string!");
            }

            return Base64Decode(s);
        }

        public static string FirstCharToUpper(this string input) =>
        input switch
        {
            null => input,
            "" => input,
            _ => input.First().ToString().ToUpper() + input.Substring(1)
        };

        /// <summary>
        /// Trims any leading and trailing whitespace characters from the <paramref name="source"/> and 
        /// returns the trimmed <see cref="string"/> or <c>null</c> when the <paramref name="source"/> is only whitespace.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        public static string TrimToNull(this string source) {
            source = source?.Trim();
            return string.IsNullOrWhiteSpace(source) ? null : source;
        }

        /// <summary>
        /// Chops the string upto the specified length
        /// If the string is smaller than the specified length, it returns the entire string back as it was provided
        /// </summary>
        /// <param name="str"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string Chop(this string str, int length) {
            return str.Length <= length ? str : $"{str.Substring(0, length)}...";
        }

        /// <summary>
        /// Get MD5 hash
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ComputeMD5(this string str) {
            StringBuilder sb = new StringBuilder();

            // Initialize a MD5 hash object
            using (MD5 md5 = MD5.Create()) {
                // Compute the hash of the given string
                byte[] hashValue = md5.ComputeHash(Encoding.UTF8.GetBytes(str));

                // Convert the byte array to string format
                foreach (byte b in hashValue) {
                    sb.Append($"{b:X2}");
                }
            }

            return sb.ToString();
        }

        public static T ToFlagsEnum<T>(this IEnumerable<string> valueList) where T : struct, Enum {
            T result = (T)Enum.ToObject(typeof(T), 0); // Start with default value (0)

            foreach (var value in valueList) {
                if (Enum.TryParse<T>(value, out T parsedValue)) {
                    result = (T)Enum.ToObject(typeof(T), Convert.ToInt64(result) | Convert.ToInt64(parsedValue));
                }
                else {
                    Console.WriteLine($"Warning: Unable to parse '{value}' into {typeof(T)}.");
                }
            }

            return result;
        }

        public static bool TryGetFileExtension(this string filename, out string extension) {
            extension = string.Empty; // Initialize the output parameter with an empty string

            // Check if the filename is null or empty
            if (string.IsNullOrEmpty(filename)) {
                return false;
            }

            // Use the Path.GetExtension method to attempt to extract the extension
            extension = Path.GetExtension(filename);

            // Check if an extension was successfully retrieved
            if (!string.IsNullOrEmpty(extension)) {
                return true; // An extension was found
            }

            return false; // No extension was found
        }

        public static string RemoveFileExtension(this string filename) {
            // Check if the filename is null or empty
            if (string.IsNullOrEmpty(filename)) {
                return filename; // Return the original filename if it's null or empty
            }

            // Use the Path.GetFileNameWithoutExtension method to remove the extension
            string filenameWithoutExtension = Path.GetFileNameWithoutExtension(filename);

            return filenameWithoutExtension;
        }
    }
}
