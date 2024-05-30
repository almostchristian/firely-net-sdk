// -------------------------------------------------------------------------------------------------
// Copyright (c) Integrated Health Information Systems Pte Ltd. All rights reserved.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Hl7.Fhir.Model.SourceGeneration;

internal sealed class SyntaxContextReceiver : ISyntaxContextReceiver
{
    public const string GeneratedAllTypesAttributeName = "Hl7.Fhir.Model.GenerateAllFhirTypesAttribute";

    public HashSet<(ITypeSymbol Class, IMethodSymbol Method, IAssemblySymbol[] Assemblies)> MethodDeclarations { get; } = new();

    internal static SyntaxContextReceiver Create()
    {
        return new SyntaxContextReceiver();
    }

    public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
    {
        if (IsSyntaxTargetForGeneration(context, out var methodSymbol))
        {
            var syntax = GetSemanticTargetForGeneration(methodSymbol);
            if (syntax is not null)
            {
                MethodDeclarations.Add(syntax.Value);
            }
        }
    }

    private static bool IsSyntaxTargetForGeneration(GeneratorSyntaxContext context, out IMethodSymbol? methodSymbol)
    {
        if (context.Node is MethodDeclarationSyntax methodDeclarationSyntax)
        {
            var sm = context.SemanticModel;
            methodSymbol = sm.GetDeclaredSymbol(methodDeclarationSyntax)!;
            if (methodSymbol.IsPartialDefinition)
            {
                return true;
            }
        }

        methodSymbol = null;
        return false;
    }

    private static (ITypeSymbol Class, IMethodSymbol Method, IAssemblySymbol[] Assemblies)? GetSemanticTargetForGeneration(IMethodSymbol methodSymbol)
    {
        if (!methodSymbol.TryGetAttribute(GeneratedAllTypesAttributeName, out var attributeSyntax))
        {
            return null;
        }

        var typeSymbol = methodSymbol.ContainingType;

        var argval = attributeSyntax.NamedArguments.FirstOrDefault(x => x.Key == "AssembliesContainingTypes").Value;
        var arg = !argval.IsNull ? argval.Values : [];
        var assemblies = arg
            .Select(x => x.Value)
            .OfType<INamedTypeSymbol>()
            .Select(e => e.ContainingAssembly)
            .Distinct(SymbolEqualityComparer.Default)
            .OfType<IAssemblySymbol>()
            .ToArray();
        return (typeSymbol, methodSymbol, assemblies);
    }
}
