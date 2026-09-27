using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Netprof.Generators;

[Generator(LanguageNames.CSharp)]
public sealed class ZoneGenerator : IIncrementalGenerator
{
    private const string EnterZone = nameof(EnterZone);

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var syntaxProvider = context.SyntaxProvider
            .CreateSyntaxProvider(static (node, _) => IsEnterZoneInvocation(node), GetInvocationSourceLocation)
            .Where(static location => location.HasValue)
            .Select(static (location, _) => location!.Value);

        var assemblyNameProvider = context.CompilationProvider
            .Select(static (compilation, _) => compilation.AssemblyName ?? "UnknownAssembly");

        var combinedProvider = assemblyNameProvider.Combine(syntaxProvider.Collect());

        context.RegisterSourceOutput(combinedProvider, static (productionContext, source) =>
        {
            using var profileZonesWriter = new ProfileZonesWriter();
            profileZonesWriter.Write(source.Left, source.Right.Sort(static (x, y) => x.CallSite.CompareTo(y.CallSite)));
            productionContext.AddSource("ProfileZones.g.cs", profileZonesWriter.ToString());
        });
    }

    private static bool IsEnterZoneInvocation(SyntaxNode node)
    {
        // NOTE(alex): First, check if the node is a method call with no arguments.
        if (node is not InvocationExpressionSyntax invocation ||
            invocation.ArgumentList.Arguments.Count != 0)
        {
            return false;
        }

        // NOTE(alex): Then, check to see if it is the correct method (by name for now, by global symbol later):
        switch (invocation.Expression)
        {
            case MemberAccessExpressionSyntax memberAccess:
                return memberAccess.Name.Identifier.ValueText == EnterZone;

            case IdentifierNameSyntax identifier:
                return identifier.Identifier.ValueText == EnterZone;

            default:
                return false;
        }
    }

    private static InvocationSourceLocation? GetInvocationSourceLocation(GeneratorSyntaxContext context, CancellationToken cancellationToken)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;
        var symbolInfo = context.SemanticModel.GetSymbolInfo(invocation, cancellationToken);

        // NOTE(alex): Now that we have the actual semantic model of the syntax node, ensure we are calling the correct method.
        if (symbolInfo.Symbol is not IMethodSymbol method ||
            method.Name != EnterZone ||
            method.Parameters.Length != 0 ||
            method.ContainingType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) != "global::Netprof.Profiler")
        {
            return null;
        }

        var location = context.SemanticModel.GetInterceptableLocation(invocation, cancellationToken);

        if (location is null)
        {
            return null;
        }

        var interceptsLocationAttribute = location.GetInterceptsLocationAttributeSyntax();
        var enclosingSymbol = context.SemanticModel.GetEnclosingSymbol(invocation.SpanStart, cancellationToken);
        var callSite = enclosingSymbol?.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat) ?? "<unknown>";

        return new InvocationSourceLocation(interceptsLocationAttribute, callSite, location.GetDisplayLocation());
    }
}
