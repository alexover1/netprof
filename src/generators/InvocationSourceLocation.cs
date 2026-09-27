namespace Netprof.Generators;

internal readonly struct InvocationSourceLocation
{
    public InvocationSourceLocation(string interceptsLocationAttribute, string callSite, string location)
    {
        InterceptsLocationAttribute = interceptsLocationAttribute;
        CallSite = callSite;
        Location = location;
    }

    public string InterceptsLocationAttribute { get; }
    public string CallSite { get; }
    public string Location { get; }
}
