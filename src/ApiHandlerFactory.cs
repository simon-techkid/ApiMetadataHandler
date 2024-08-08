// ApiMetadataHandler by Simon Field

using Logging.Broadcasting;

namespace ApiMetadataHandler;

/// <summary>
/// Factory for ApiMetadataHandler classes.
/// </summary>
/// <typeparam name="TCast">The type of entries to match.</typeparam>
public abstract partial class ApiHandlerFactory<TCast>
{
    public delegate IApiMetadataHandler<TCast> ApiHandlerCreator(IBroadcaster<string> bcaster);
}
