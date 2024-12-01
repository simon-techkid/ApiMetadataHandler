// Techkid.ApiMetadata by Simon Field

using System.Collections.Generic;

namespace Techkid.ApiMetadata;

/// <summary>
/// An interface for matching entries (of type <typeparamref name="TIdentifier"/>) with their API metadata.
/// </summary>
/// <typeparam name="TCast">The type of entries to match.</typeparam>
public interface IApiMetadataHandler<TCast>
{
    /// <summary>
    /// Match data entries of type <typeparamref name="TCast"/> to API metadata using the underlying handler.
    /// </summary>
    /// <param name="allEntries">A list of entries to match their API data with.</param>
    /// <returns>A list of <typeparamref name="TCast"/> objects that match the entries; containing corresponding API metadata.</returns>
    public List<TCast> MatchEntries(List<TCast> allEntries);
}
