// Techkid.ApiMetadata by Simon Field

using Techkid.Logging.Broadcasting;

namespace Techkid.ApiMetadata;

/// <summary>
/// Factory for ApiMetadataHandler classes.
/// </summary>
/// <typeparam name="TCast">The type of entries to match.</typeparam>
public abstract partial class ApiHandlerFactory<TCast>
{
    public delegate IApiMetadataHandler<TCast> ApiHandlerCreator(IBroadcaster<string> bcaster);
}
