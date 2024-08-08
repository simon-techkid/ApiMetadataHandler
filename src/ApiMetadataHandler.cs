// ApiMetadataHandler by Simon Field

using Logging;
using Logging.Broadcasting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ApiMetadataHandler;

/// <summary>
/// A class for matching entries using their identifier (of type <typeparamref name="TIdentifier"/>) with their API metadata (of type <typeparamref name="TMetadata"/>) from the respective API service.
/// </summary>
/// <typeparam name="TImplementer">The type of object implementing <see cref="IApiMetadataRecordable{TIdentifier, TMetadata}"/>.</typeparam>
/// <typeparam name="TIdentifier">The type of the identifier of a <typeparamref name="TImplementer"/> object.</typeparam>
/// <typeparam name="TMetadata">The type of the metadata object of a <typeparamref name="TImplementer"/> object.</typeparam>
/// <typeparam name="TCast">The type of the object that is cast from <typeparamref name="TImplementer"/>.</typeparam>
public abstract class ApiMetadataHandler<TImplementer, TIdentifier, TMetadata, TCast>(IBroadcaster<string> bcaster) :
    IApiMetadataHandler<TCast>,
    ILogger<string>
    where TIdentifier : notnull, IEquatable<TIdentifier>
    where TImplementer : IApiMetadataRecordable<TIdentifier, TMetadata>
{
    private static IEnumerable<TImplementer> GetEntries(List<TCast> entries) => entries.OfType<TImplementer>();

    public IBroadcaster<string> BCaster { get; } = bcaster;

    /// <summary>
    /// Get a <see cref="Dictionary{TKey, TValue}"/> containing the metadata (of type <typeparamref name="TMetadata"/>) for the given identifiers (of type <typeparamref name="TIdentifier"/>).
    /// </summary>
    /// <param name="identifiers">An array of <typeparamref name="TIdentifier"/> objects to get metadata of type <typeparamref name="TMetadata"/> for.</param>
    /// <returns>A dictionary, where identifiers of type <typeparamref name="TIdentifier"/> are keys and metadata objects of type <typeparamref name="TMetadata"/> are values.</returns>
    protected abstract Dictionary<TIdentifier, TMetadata> GetMetadatas(List<TIdentifier> identifiers);

    /// <summary>
    /// Custom filter of returned identifiers of type <typeparamref name="TIdentifier"/>.
    /// </summary>
    protected virtual Func<TIdentifier, bool> CustomFilter => entry => true;

    /// <summary>
    /// Custom modifier of returned identifiers of type <typeparamref name="TIdentifier"/>, before they are processed.
    /// </summary>
    protected virtual Func<TIdentifier?, TIdentifier?> CustomModifier => entry => entry;

    /// <summary>
    /// The name of the entity being matched.
    /// </summary>
    protected virtual string NameOfEntity => "entity";

    /// <summary>
    /// The equality comparer for the identifiers of type <typeparamref name="TIdentifier"/>.
    /// Allows the identifiers to be compared for equality and filtered for uniqueness.
    /// </summary>
    protected virtual IEqualityComparer<TIdentifier> EqualityComparer => EqualityComparer<TIdentifier>.Default;

    /// <summary>
    /// The required converter from <typeparamref name="TImplementer"/> to <typeparamref name="TCast"/>.
    /// </summary>
    protected abstract Func<TImplementer, TCast> Converter { get; }

    public List<TCast> MatchEntries(List<TCast> allEntries)
    {
        IEnumerable<TImplementer> entries = GetEntries(allEntries);

        // Extract track IDs
        IEnumerable<TIdentifier> trackIds = entries
            .Select(validEntry => CustomModifier(validEntry.GetEntryCode()))
            .Where(identifier => identifier != null && CustomFilter(identifier))!;

        List<TIdentifier> distinctIds = trackIds.Distinct(EqualityComparer).ToList();
        BCaster.Broadcast($"Filtered {distinctIds.Count} unique {NameOfEntity} IDs from {trackIds.Count()} total {NameOfEntity} IDs");

        // Get metadata from Spotify API
        Dictionary<TIdentifier, TMetadata> metadatas = GetMetadatas(distinctIds)
            .ToDictionary(pair => pair.Key, pair => pair.Value);

        BCaster.Broadcast($"Retrieved {metadatas.Count}/{distinctIds.Count} {NameOfEntity} metadata entries from API.");

        List<TImplementer> entriesList = entries.ToList();

        for (int i = 0; i < entriesList.Count; i++)
        {
            TImplementer thisEntry = entriesList[i];

            if (thisEntry.TryGetEntryCode(out TIdentifier entryCode) == true)
            {
                if (metadatas.TryGetValue(entryCode, out TMetadata? metadata))
                {
                    thisEntry.Metadata = metadata;
                }
                else
                {
                    BCaster.BroadcastError(new Exception($"[API] No metadata found for {entriesList[i].ToString()} ({entryCode})"));
                }
            }

            entriesList[i] = thisEntry;
        }

        return entriesList.Select(entry => Converter(entry)).ToList();
    }

    /// <summary>
    /// Split an <see cref="IEnumerable{T}"/> into chunks of a given size.
    /// </summary>
    /// <typeparam name="T">The type of the chunks containing elements of type <typeparamref name="T"/>.</typeparam>
    /// <param name="source">The source <see cref="IEnumerable{T}"/> to split.</param>
    /// <param name="chunkSize">The size of each resulting chunk.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> of chunks of size <paramref name="chunkSize"/>.</returns>
    protected static IEnumerable<IEnumerable<T>> SplitIntoChunks<T>(IEnumerable<T> source, int chunkSize)
    {
        while (source.Any())
        {
            yield return source.Take(chunkSize);
            source = source.Skip(chunkSize);
        }
    }
}
