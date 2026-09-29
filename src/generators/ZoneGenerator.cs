using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Netprof.Generators;

[Generator(LanguageNames.CSharp)]
public sealed class ZoneGenerator : IIncrementalGenerator
{
    private const string EnterZone = nameof(EnterZone);
    private const string EnterAsyncZone = nameof(EnterAsyncZone);

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
                return memberAccess.Name.Identifier.ValueText == EnterZone || memberAccess.Name.Identifier.ValueText == EnterAsyncZone;

            case IdentifierNameSyntax identifier:
                return identifier.Identifier.ValueText == EnterZone || identifier.Identifier.ValueText == EnterAsyncZone;

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
            (method.Name != EnterZone && method.Name != EnterAsyncZone) ||
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
        var enclosingSymbol = GetContainingNonLambdaSymbol(context.SemanticModel, invocation.SpanStart, cancellationToken);
        var callSite = enclosingSymbol?.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat) ?? "<unknown>";
        var isAsync = method.Name == EnterAsyncZone;

        return new InvocationSourceLocation(interceptsLocationAttribute, callSite, location.GetDisplayLocation(), isAsync);
    }

    // NOTE(alex): This method was added to avoid zones inside lambdas always returning "lambda expression" as their callsite.
    private static ISymbol? GetContainingNonLambdaSymbol(SemanticModel semanticModel, int position, CancellationToken cancellationToken)
    {
        var symbol = semanticModel.GetEnclosingSymbol(position, cancellationToken);

        while (symbol is IMethodSymbol { MethodKind: MethodKind.AnonymousFunction })
        {
            symbol = symbol.ContainingSymbol;
        }

        return symbol;
    }
}
