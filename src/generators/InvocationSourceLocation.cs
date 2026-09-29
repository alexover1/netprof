namespace Netprof.Generators;

internal readonly struct InvocationSourceLocation
{
    public InvocationSourceLocation(string interceptsLocationAttribute, string callSite, string location, bool isAsync)
    {
        InterceptsLocationAttribute = interceptsLocationAttribute;
        CallSite = callSite;
        Location = location;
        IsAsync = isAsync;
    }

    public string InterceptsLocationAttribute { get; }
    public string CallSite { get; }
    public string Location { get; }
    public bool IsAsync { get; }
}
