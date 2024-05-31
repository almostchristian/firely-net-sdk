// -------------------------------------------------------------------------------------------------
// Copyright (c) Integrated Health Information Systems Pte Ltd. All rights reserved.
// -------------------------------------------------------------------------------------------------

//#define LAUNCH_DEBUGGER

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Hl7.Fhir.Model.SourceGeneration;

internal sealed class SyntaxContextReceiver : ISyntaxContextReceiver
{
    public const string GeneratedAllTypesAttributeName = "Hl7.Fhir.Model.GenerateAllFhirTypesAttribute";
    public const string GenerateModelInspectorAttributeName = "Hl7.Fhir.Model.GenerateModelInspectorAttribute";

    public HashSet<(ITypeSymbol Class, IMethodSymbol Method, INamedTypeSymbol[] Types, bool ScanAll)> MethodDeclarations { get; } = new();

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

    private static (ITypeSymbol Class, IMethodSymbol Method, INamedTypeSymbol[] Types, bool ScanAll)? GetSemanticTargetForGeneration(IMethodSymbol methodSymbol)
    {
        if (!methodSymbol.TryGetAttribute(GeneratedAllTypesAttributeName, out var attributeSyntax) &&
            !methodSymbol.TryGetAttribute(GenerateModelInspectorAttributeName, out attributeSyntax))
        {
            return null;
        }

        var typeSymbol = methodSymbol.ContainingType;

#if LAUNCH_DEBUGGER
        System.Diagnostics.Debugger.Launch();
#endif

        var arg = attributeSyntax!.ConstructorArguments;

        bool scanAllTypes = true;
        if (arg.Length > 0 && arg[0].Kind != TypedConstantKind.Type)
        {
            bool.TryParse(arg[0].Value!.ToString(), out scanAllTypes);
        }

        var types = arg
            .Select(x => x.Value)
            .OfType<INamedTypeSymbol>()
            .Distinct(SymbolEqualityComparer.Default)
            .OfType<INamedTypeSymbol>()
            .ToArray();
        return (typeSymbol, methodSymbol, types, scanAllTypes);
    }
}
