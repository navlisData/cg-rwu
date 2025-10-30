using System.Text;
using System.Text.RegularExpressions;

namespace engine.TextureProcessing
{
    public readonly record struct AssetRef<T>(ulong Id)
    {
        /// <summary>
        ///     Creates an asset reference by hashing a canonicalized path.
        ///     Signature: <c>static AssetRef&lt;T&gt; FromPath(string path)</c>
        /// </summary>
        public static AssetRef<T> FromPath(string path)
            => new(Hash64.FromString(AssetPath.Canonicalize(path)));
    }

    internal static class AssetPath
    {
        // Allows only lowercase a–z, digits, and single slashes between non-empty segments.
        private static readonly Regex SegmentPathRegex =
            new("^[a-z0-9]+(?:/[a-z0-9]+)*$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        /// <summary>
        ///     Canonicalizes and validates an asset path.
        ///     Allowed characters after canonicalization: [a-z], [0-9], '/' (between non-empty segments).
        ///     Signature: <c>static string Canonicalize(string raw)</c>
        /// </summary>
        /// <exception cref="ArgumentNullException">When <paramref name="raw"/> is null.</exception>
        /// <exception cref="ArgumentException">When the path is empty or violates the allowed pattern.</exception>
        public static string Canonicalize(string raw)
        {
            if (raw is null) throw new ArgumentNullException(nameof(raw));

            var trimmedPath = raw.Trim().ToLowerInvariant();

            if (trimmedPath.Length == 0)
                throw new ArgumentException("Asset path must not be empty.", nameof(raw));

            if (!SegmentPathRegex.IsMatch(trimmedPath))
                throw new ArgumentException(
                    "Path may only contain lowercase [a–z], digits [0–9], and single '/' separators (no leading/trailing/double slashes).",
                    nameof(raw));

            return trimmedPath;
        }
    }

    internal static class Hash64
    {
        /// <summary>
        ///     Computes a 64-bit FNV-1a hash for the given string using UTF-8 bytes.
        /// </summary>
        /// <param name="s">Input string to hash (must not be null).</param>
        /// <returns>A non-zero 64-bit hash value (0 is remapped to 1 as a sentinel).</returns>
        /// <remarks>
        /// Implementation details:
        ///     - UTF-8 encoding ensures platform-stable byte sequences.
        ///     - The multiplication uses <c>unchecked</c> to allow intentional 64-bit wraparound per FNV-1a.
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="s"/> is null.</exception>
        public static ulong FromString(string s)
        {
            if (s is null) throw new ArgumentNullException(nameof(s));

            const ulong offset = 14695981039346656037UL; // FNV-1a offset basis
            const ulong prime  = 1099511628211UL;        // FNV-1a prime

            ulong hash = offset;

            // Convert to UTF-8 bytes once; simple and fast enough for most scenarios.
            foreach (byte b in Encoding.UTF8.GetBytes(s))
            {
                hash ^= b;
                hash = unchecked(hash * prime); // wrap on overflow, as intended by FNV-1a
            }

            // If 0 is a reserved sentinel, remap to 1 to keep "non-zero" invariant.
            return hash == 0 ? 1UL : hash;
        }
    }
}