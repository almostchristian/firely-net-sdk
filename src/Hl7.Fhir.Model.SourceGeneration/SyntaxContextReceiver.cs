//#define LAUNCH_DEBUGGER

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Hl7.Fhir.Model.SourceGeneration;

internal sealed class SyntaxContextReceiver : ISyntaxContextReceiver
{
    public const string GenerateModelInspectorAttributeName = "Hl7.Fhir.Model.GenerateModelInspectorAttribute";

    public HashSet<(ITypeSymbol Class, IMethodSymbol Method, INamedTypeSymbol[] Types, ModelInspectorGenerationTypeInclusionMode Mode)> MethodDeclarations { get; } = new();

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

    private static (ITypeSymbol Class, IMethodSymbol Method, INamedTypeSymbol[] Types, ModelInspectorGenerationTypeInclusionMode Mode)? GetSemanticTargetForGeneration(IMethodSymbol methodSymbol)
    {
        if (!methodSymbol.TryGetAttribute(GenerateModelInspectorAttributeName, out var attributeSyntax))
        {
            return null;
        }

        var typeSymbol = methodSymbol.ContainingType;

#if LAUNCH_DEBUGGER
        System.Diagnostics.Debugger.Launch();
#endif

        var arg = attributeSyntax!.ConstructorArguments;

        bool hasMode = false;
        ModelInspectorGenerationTypeInclusionMode mode = ModelInspectorGenerationTypeInclusionMode.Default;
        if (arg.Length > 0 && arg[0].Kind != TypedConstantKind.Type)
        {
            Enum.TryParse(arg[0].Value!.ToString(), out mode);
            hasMode = true;
        }

        INamedTypeSymbol[] types;
        if (hasMode && arg.Length == 2 && arg[1].Kind == TypedConstantKind.Array)
        {
            types = arg[1].Values
                .Select(x => x.Value)
                .OfType<INamedTypeSymbol>()
                .Distinct(SymbolEqualityComparer.Default)
                .OfType<INamedTypeSymbol>()
                .ToArray();
        }
        else
        {
            types = arg
                .Skip(hasMode ? 1 : 0)
                .Select(x => x.Value)
                .OfType<INamedTypeSymbol>()
                .Distinct(SymbolEqualityComparer.Default)
                .OfType<INamedTypeSymbol>()
                .ToArray();
        }

        
        return (typeSymbol, methodSymbol, types, mode);
    }
}
