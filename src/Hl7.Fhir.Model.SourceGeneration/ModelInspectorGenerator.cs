#define CACHING // this changes the source generated code so that it uses Lazy<Type>[] so that the generated types are done only once.
//#define LAUNCH_DEBUGGER
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Hl7.Fhir.Model.SourceGeneration;

[Generator]
public class ModelInspectorGenerator : ISourceGenerator
{
#if CACHING
    internal const string arrayTerminator = "]);";
    internal const string terminator = ");";
    internal const string arrayAccess = ".Value";
#else
    internal const string arrayTerminator = "];";
    internal const string terminator = ";";
    internal const string arrayAccess = "()";
#endif
    private const string FhirModelAssemblyAttributeName = "Hl7.Fhir.Introspection.FhirModelAssemblyAttribute";
    private static readonly string AssemblyVersion = typeof(ModelInspectorGenerator).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "1.0.0.0";

    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(SyntaxContextReceiver.Create);
    }

    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxContextReceiver is not SyntaxContextReceiver receiver || receiver.MethodDeclarations.Count == 0)
        {
            // nothing to do yet
            return;
        }

#if LAUNCH_DEBUGGER && DEBUG
        System.Diagnostics.Debugger.Launch();
#endif
        var typeInclusionMode = receiver.MethodDeclarations.Min(static x => x.Mode);

        var definedTypes = receiver.MethodDeclarations.SelectMany(static x => x.Types).ToList();

        FhirRelease fhirRelease;

        HashSet<ITypeSymbol> allFhirTypes = FindAllTypes(context, typeInclusionMode, definedTypes, out fhirRelease);
        var allClassMappings = new Dictionary<ITypeSymbol, AttributeData>(SymbolEqualityComparer.Default);
        var allEnumMappings = new Dictionary<ITypeSymbol, AttributeData>(SymbolEqualityComparer.Default);

        Helpers.PopulateMappings(allFhirTypes, allClassMappings, allEnumMappings);

        if (context.CancellationToken.IsCancellationRequested)
        {
            return;
        }

        if (allFhirTypes.Count > 0)
        {
            StringBuilder code = new(
                $$"""
                using Hl7.Fhir.Introspection;
                using System.Linq;
                [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Hl7.Fhir.Model.SourceGeneration", "{{AssemblyVersion}}")]
                file static class GeneratedModelInspectorContainer
                {
                    private static readonly Hl7.Fhir.Specification.FhirRelease FhirRelease = Hl7.Fhir.Specification.FhirRelease.{{fhirRelease}};

                    private static Hl7.Fhir.Introspection.PropertyMapping CreateProp(Hl7.Fhir.Introspection.ClassMapping declaringClass, System.Reflection.PropertyInfo prop)
                    {
                        if (Hl7.Fhir.Introspection.PropertyMapping.TryCreate(prop, out var result, declaringClass, FhirRelease))
                        {
                            return result;
                        }

                        throw new System.InvalidOperationException($"Cannot create PropertyMapping for [{prop}] for type [{declaringClass.Name}].");
                    }

                """);

            WriteMethod(code, "Hl7.Fhir.Introspection.ModelInspector", "GetModelInspector", code =>
            {
                code.AppendLine($"          new Hl7.Fhir.Introspection.ModelInspector(Hl7.Fhir.Model.ModelInfo.Version, AllClassMappings{arrayAccess}, AllEnumMappings{arrayAccess})");
            });

            WriteMethod(code, "System.Type[]", "AllTypes", code =>
            {
                foreach (var fhirType in allFhirTypes)
                {
                    code.AppendLine($"          typeof({fhirType.ToDisplayString()}),");
                }
            });

            var classIndex = allClassMappings
                .Select(static (x, i) => (x.Key, i))
                .Where(static x => !x.Key.IsCqlType())
                .ToDictionary(static x => x.Key, static x => x.i, SymbolEqualityComparer.Default);
            WriteMethod(code, "Hl7.Fhir.Introspection.ClassMapping[]", "AllClassMappings", code =>
            {
                foreach (var fhirType in allClassMappings)
                {
                    if (context.CancellationToken.IsCancellationRequested)
                    {
                        return;
                    }

                    if (fhirType.Key.IsSupportedNetType())
                    {
                        fhirType.Key.WriteNetType(code);
                    }
                    else if (fhirType.Key.IsCqlType())
                    {
                        fhirType.Key.WriteCqlType(code, fhirType.Value);
                    }
                    else
                    {
                        fhirType.Key.WriteFhirType(code, fhirType.Value, classIndex, fhirRelease);
                    }
                }
            });

            WriteMethod(code, "Hl7.Fhir.Introspection.EnumMapping[]", "AllEnumMappings", code =>
            {
                foreach (var fhirType in allEnumMappings)
                {
                    if (context.CancellationToken.IsCancellationRequested)
                    {
                        return;
                    }

                    fhirType.Key.WriteFhirEnumeration(code, fhirType.Value);
                }
            });

            foreach (var (_, methodSymbol, methodDefinedTypes, methodDoScan) in receiver.MethodDeclarations)
            {
                if (receiver.MethodDeclarations.Count == 1)
                {
                    WriteNoDiff();
                }
                else
                {
                    var fhirTypes = FindAllTypes(context, methodDoScan, methodDefinedTypes.ToList(), out var methodRelease);
                    if (allFhirTypes.SetEquals(fhirTypes))
                    {
                        WriteNoDiff();
                    }
                    else
                    {
                        WriteMethod(code, "Hl7.Fhir.Introspection.ModelInspector", $"Get{methodSymbol.Name}ModelInspector", code =>
                        {
                            code.AppendLine($"         new Hl7.Fhir.Introspection.ModelInspector(Hl7.Fhir.Model.ModelInfo.Version, {methodSymbol.Name}ClassMappings{arrayAccess}, {methodSymbol.Name}EnumMappings{arrayAccess})");
                        });

                        WriteMethod(code, "System.Type[]", $"{methodSymbol.Name}Types", code =>
                        {
                            foreach (var fhirType in fhirTypes)
                            {
                                code.AppendLine($"         typeof({fhirType.ToDisplayString()}),");
                            }
                        });

                        WriteMethod(code, "Hl7.Fhir.Introspection.ClassMapping[]", $"{methodSymbol.Name}ClassMappings", code =>
                        {
                            foreach (var fhirType in fhirTypes)
                            {
                                if (fhirType.IsClassMapping(out var mapping))
                                {
                                    if (fhirType.IsSupportedNetType())
                                    {
                                        fhirType.WriteNetType(code);
                                    }
                                    else if (fhirType.IsCqlType())
                                    {
                                        fhirType.WriteCqlType(code, mapping);
                                    }
                                    else
                                    {
                                        fhirType.WriteFhirType(code, mapping, classIndex, methodRelease);
                                    }
                                }
                            }
                        });

                        WriteMethod(code, "Hl7.Fhir.Introspection.EnumMapping[]", $"{methodSymbol.Name}EnumMappings", code =>
                        {
                            foreach (var fhirType in fhirTypes.Where(x => x.IsEnumMapping(out _)))
                            {
                                if (fhirType.IsEnumMapping(out var mapping))
                                {
                                    fhirType.WriteFhirEnumeration(code, mapping);
                                }
                            }
                        });
                    }
                }

                void WriteNoDiff()
                {
#if CACHING
                    code.AppendLine($"    internal static System.Lazy<Hl7.Fhir.Introspection.ModelInspector> Get{methodSymbol.Name}ModelInspector = GetModelInspector;");
                    code.AppendLine($"    internal static System.Lazy<global::System.Type[]> {methodSymbol.Name}Types = AllTypes;");
                    code.AppendLine($"    internal static System.Lazy<Hl7.Fhir.Introspection.ClassMapping[]> {methodSymbol.Name}ClassMappings = AllClassMappings;");
                    code.AppendLine($"    internal static System.Lazy<Hl7.Fhir.Introspection.EnumMapping[]> {methodSymbol.Name}EnumMappings = AllEnumMappings;");
#else
                    code.AppendLine($"    internal static Hl7.Fhir.Introspection.ModelInspector Get{methodSymbol.Name}ModelInspector() => GetModelInspector();");
                    code.AppendLine($"    internal static global::System.Type[] {methodSymbol.Name}Types() => AllTypes();");
                    code.AppendLine($"    internal static Hl7.Fhir.Introspection.ClassMapping[] {methodSymbol.Name}ClassMappings() => AllClassMappings();");
                    code.AppendLine($"    internal static Hl7.Fhir.Introspection.EnumMapping[] {methodSymbol.Name}EnumMappings() => AllEnumMappings();");
#endif
                }
            }

            code.Append("}");

            if (context.CancellationToken.IsCancellationRequested)
            {
                return;
            }

            foreach (var (classSymbol, methodSymbol, _, _) in receiver.MethodDeclarations)
            {
                var returnType = methodSymbol.ReturnType.ToDisplayString();
                string propertyToAccess = methodSymbol.ReturnType is IArrayTypeSymbol array ? $"{methodSymbol.Name}{array.ElementType.Name}s" : $"Get{methodSymbol.Name}{methodSymbol.ReturnType.Name}";

                // generate code
                code.Append(
                    $$"""

                namespace {{classSymbol.ContainingNamespace.ToDisplayString()}}
                {
                    {{classSymbol.DeclaredAccessibility.ToCSharp()}}{{(classSymbol.IsStatic ? " static" : string.Empty)}} partial class {{classSymbol.Name}}
                    {
                        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Hl7.Fhir.Model.SourceGeneration", "{{AssemblyVersion}}")]
                        {{methodSymbol.DeclaredAccessibility.ToCSharp()}}{{(methodSymbol.IsStatic ? " static" : string.Empty)}} partial {{returnType}} {{methodSymbol.Name}}() => GeneratedModelInspectorContainer.{{propertyToAccess}}{{arrayAccess}};
                    }
                }
                """);
            }

#if DEBUG
            var csharp = code.ToString();
            System.Diagnostics.Debug.WriteLine(csharp);
#endif
            context.AddSource("GeneratedModelInspectorContainer.g.cs", SourceText.From(code.ToString()!, Encoding.UTF8));
        }

        static HashSet<ITypeSymbol> FindAllTypes(GeneratorExecutionContext context, ModelInspectorGenerationTypeInclusionMode typeInclusionMode, List<INamedTypeSymbol> definedTypes, out FhirRelease release)
        {
            HashSet<ITypeSymbol> fhirTypes;
            FhirRelease? scannedRelease = null;
            if (typeInclusionMode == ModelInspectorGenerationTypeInclusionMode.Default || definedTypes.Count == 0)
            {
                AttributeData? modelMetadata = null;
                fhirTypes = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);
                var hl7Asms = definedTypes
                    .Select(static x => x.ContainingAssembly)
                    .Where(x => x.TryGetAttribute(FhirModelAssemblyAttributeName, out modelMetadata))
                    .Distinct(SymbolEqualityComparer.Default)
                    .OfType<IAssemblySymbol>()
                    .ToList();

                // if the AssembliesContainingTypes property is empty, we scan for referenced
                // assemblies with the FhirModelAssemblyAttribute
                if (hl7Asms.Count == 0)
                {
                    hl7Asms = context.Compilation.References
                        .Select(context.Compilation.GetAssemblyOrModuleSymbol)
                        .OfType<IAssemblySymbol>()
                        .Where(x => x.TryGetAttribute(FhirModelAssemblyAttributeName, out modelMetadata))
                        .ToList();
                }

                if (!hl7Asms.Any(x => x.Name == "Hl7.Fhir.Base") &&
                    GetCoreAssembly() is IAssemblySymbol coreAssembly)
                {
                    hl7Asms.Add(coreAssembly);
                }

                context.Compilation.GlobalNamespace.TraverseNamespace(fhirTypes, context.CancellationToken);
                foreach (var asm in hl7Asms)
                {
                    asm.GlobalNamespace.TraverseNamespace(fhirTypes, context.CancellationToken);
                }

                if (modelMetadata.ReadSinceProperty() is FhirRelease r &&
                    (!scannedRelease.HasValue || scannedRelease.Value < r))
                {
                    scannedRelease = r;
                }
            }
            else
            {
                fhirTypes = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);

                if (typeInclusionMode == ModelInspectorGenerationTypeInclusionMode.IncludeAllCoreFhirTypes)
                {
                    var coreAssembly = GetCoreAssembly();

                    if (coreAssembly != null)
                    {
                        coreAssembly.GlobalNamespace.TraverseNamespace(fhirTypes, context.CancellationToken);
                    }
                }

                foreach (var type in definedTypes)
                {
                    if (!type.IsGenericType &&
                        type.CanBeReferencedByName &&
                        (type.IsClassMapping(out _) || type.IsEnumMapping(out _)))
                    {
                        if (fhirTypes.Add(type))
                        {
                            foreach (var nestedType in type.GetTypeMembers().Where(static x => x.IsClassMapping(out _) || x.IsEnumMapping(out _)))
                            {
                                fhirTypes.Add(nestedType);
                            }
                        }
                    }
                }
            }

            foreach (var dotnetType in Helpers.SupportedDotNetPrimitiveTypeNames.ToList())
            {
                var searchType = dotnetType;
                bool isArray = false;
                if (dotnetType.EndsWith("[]"))
                {
                    isArray = true;
                    searchType = dotnetType.Substring(0, dotnetType.Length - 2);
                }

                if (context.Compilation.GetTypeByMetadataName(searchType) is ITypeSymbol type)
                {
                    if (isArray)
                    {
                        type = context.Compilation.CreateArrayTypeSymbol(type);
                    }

                    Helpers.SupportedDotNetPrimitiveTypeNames.Add(type.ToDisplayString());
                    fhirTypes.Add(type);
                }
            }

            release = scannedRelease ?? FhirRelease.STU3;
            return fhirTypes;

            IAssemblySymbol? GetCoreAssembly()
            {
                return context.Compilation.References
                        .Select(context.Compilation.GetAssemblyOrModuleSymbol)
                        .OfType<IAssemblySymbol>()
                        .Where(static x => x.TryGetAttribute(FhirModelAssemblyAttributeName, out _) && x.Name == "Hl7.Fhir.Base")
                        .SingleOrDefault();
            }
        }

        static void WriteMethod(StringBuilder code, string returnType, string name, Action<StringBuilder> writeContent)
        {
            bool isArray = returnType.EndsWith("[]");
            code.Append(
                $$"""
                    public static {{WriteMethodSignature(returnType, name)}}() =>
                    {{(isArray ? "[" : string.Empty)}}

                """);

            writeContent(code);

            code.Append(
                $$"""
                    {{(isArray ? arrayTerminator : terminator)}}

                """);
        }

        static string WriteMethodSignature(string returnType, string methodName)
        {
#if CACHING
            return $"readonly System.Lazy<{returnType}> {methodName} = new(";
#else
            return $"{returnType} {methodName}";
#endif
        }
    }

}
