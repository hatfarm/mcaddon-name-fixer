using Microsoft.Extensions.Caching.Memory;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace MinecraftAddonNameFixer.WebApp.Cache;

public class CleanFileCache
{
    private readonly static CleanFileCache instance = new();

    private readonly HashSet<string> cacheKeys = new();

    private readonly IMemoryCache memoryCache = new MemoryCache(
        new MemoryCacheOptions
        {
            SizeLimit = 100,
            ExpirationScanFrequency = TimeSpan.FromSeconds(60)
        });

    private CleanFileCache()
    {
    }

    /// <summary>
    /// The singleton instance of this cache.
    /// </summary>
    public static CleanFileCache Instance => instance;

    /// <summary>
    /// A set of keys that exist in the cache.
    /// </summary>
    /// <remarks>This is only necessary because IMemoryCache doesn't support looking up each key.</remarks>
    public IReadOnlySet<string> CacheKeys
    {
        get 
        {
            return this.cacheKeys.Where(key => this.memoryCache.TryGetValue(key, out _)).ToImmutableHashSet();
        } 
    }

    /// <summary>
    /// Clean a file, and cache it locally
    /// </summary>
    /// <param name="fileName">The name of the file coming</param>
    /// <param name="inputStream">A stream with the contents of the file</param>
    /// <returns>The cached name, so we can look it up later</returns>
    /// <exception cref="InvalidOperationException">If we run into an issue where we've tried the same file 10k times, should be rare.</exception>
    public string CleanFile([NotNull] string fileName, Stream inputStream)
    {
        string cleanedFileName = $"cleaned_{fileName}";
        int counter = 0;
        while (this.memoryCache.TryGetValue(cleanedFileName, out _))
        {
            if (counter > 9999)
            {
                throw new InvalidOperationException("Too many files with the exact same name");
            }
            cleanedFileName = $"cleaned_{(counter++):D4}_{fileName}";
        }
        this.memoryCache.Set(cleanedFileName, MinecraftFilenameFixer.CleanFile(inputStream), TimeSpan.FromHours(8));
        this.cacheKeys.Add(cleanedFileName);

        return cleanedFileName;
    }

    /// <summary>
    /// See if we can get a file by name
    /// </summary>
    /// <param name="fileName">The name of the file we're looking up</param>
    /// <param name="cleanedFileStream">The stream of the cleaned file, if it exists</param>
    /// <returns>True if the file is found, false otherwise.</returns>
    public bool TryGetCleanedFile([NotNull] string fileName, [NotNullWhen(true)] out Stream? cleanedFileStream)
    {
        cleanedFileStream = null;
        return this.memoryCache.TryGetValue(fileName, out cleanedFileStream);
    }
}
