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
    private const string arrayTerminator = "]);";
    private const string terminator = ");";
    private const string arrayAccess = ".Value";
#else
    private const string arrayTerminator = "];";
    private const string terminator = ";";
    private const string arrayAccess = "()";
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

#if LAUNCH_DEBUGGER
        System.Diagnostics.Debugger.Launch();
#endif
        var doScan = receiver.MethodDeclarations.Any(static x => x.ScanAll);

        var definedTypes = receiver.MethodDeclarations.SelectMany(static x => x.Types).ToList();

        HashSet<INamedTypeSymbol> allFhirTypes = FindAllTypes(context, doScan, definedTypes);
        var allClassMappings = new List<KeyValuePair<INamedTypeSymbol, AttributeData>>();
        var allEnumMappings = new List<KeyValuePair<INamedTypeSymbol, AttributeData>>();

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
                    private static readonly Hl7.Fhir.Specification.FhirRelease FhirRelease = Hl7.Fhir.Utility.FhirReleaseParser.Parse(Hl7.Fhir.Model.ModelInfo.Version);
            
                    private static global::System.Collections.Generic.IEnumerable<global::System.ComponentModel.DataAnnotations.ValidationAttribute> GetValidationAttributes(global::System.Reflection.MemberInfo t, Hl7.Fhir.Specification.FhirRelease version)
                    {
                        return Hl7.Fhir.Utility.ReflectionHelper.GetAttributes<global::System.ComponentModel.DataAnnotations.ValidationAttribute>(t).Where(isRelevant);

                        bool isRelevant(global::System.Attribute a) => a is not Hl7.Fhir.Introspection.IFhirVersionDependent vd || a.AppliesToRelease(version);
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

            WriteMethod(code, "Hl7.Fhir.Introspection.ClassMapping[]", "AllClassMappings", code =>
            {
                foreach (var fhirType in allClassMappings)
                {
                    if (context.CancellationToken.IsCancellationRequested)
                    {
                        return;
                    }

                    if (fhirType.Key.IsCqlType())
                    {
                        WriteCqlType(code, fhirType.Key, fhirType.Value);
                    }
                    else
                    {
                        WriteFhirType(code, fhirType.Key, fhirType.Value);
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

                    WriteFhirEnumeration(code, fhirType.Key, fhirType.Value);
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
                    var fhirTypes = FindAllTypes(context, methodDoScan, methodDefinedTypes.ToList());
                    if (allFhirTypes.SetEquals(fhirTypes))
                    {
                        WriteNoDiff();
                    }
                    else
                    {
                        WriteMethod(code, "Hl7.Fhir.Introspection.ModelInspector", $"Get{methodSymbol.Name}ModelInspector", code =>
                        {
                            code.AppendLine($"          new Hl7.Fhir.Introspection.ModelInspector(Hl7.Fhir.Model.ModelInfo.Version, {methodSymbol.Name}ClassMappings{arrayAccess}, {methodSymbol.Name}EnumMappings{arrayAccess})");
                        });

                        WriteMethod(code, "System.Type[]", $"{methodSymbol.Name}Types", code =>
                        {
                            foreach (var fhirType in fhirTypes)
                            {
                                code.AppendLine($"          typeof({fhirType.ToDisplayString()}),");
                            }
                        });

                        WriteMethod(code, "Hl7.Fhir.Introspection.ClassMapping[]", $"{methodSymbol.Name}ClassMappings", code =>
                        {
                            foreach (var fhirType in fhirTypes.Where(x => !x.IsEnumMapping(out _)))
                            {
                                code.AppendLine($"          AllClassMappings{arrayAccess}[{allClassMappings.FindIndex(kvp => kvp.Key.Equals(fhirType, SymbolEqualityComparer.Default))}],");
                            }
                        });

                        WriteMethod(code, "Hl7.Fhir.Introspection.EnumMapping[]", $"{methodSymbol.Name}EnumMappings", code =>
                        {
                            foreach (var fhirType in fhirTypes.Where(x => x.IsEnumMapping(out _)))
                            {
                                code.AppendLine($"          AllEnumMappings{arrayAccess}[{allEnumMappings.FindIndex(kvp => kvp.Key.Equals(fhirType, SymbolEqualityComparer.Default))}],");
                            }
                        });
                    }
                }

                void WriteNoDiff()
                {
#if CACHING
                    code.AppendLine($"        internal static System.Lazy<Hl7.Fhir.Introspection.ModelInspector> Get{methodSymbol.Name}ModelInspector = GetModelInspector;");
                    code.AppendLine($"        internal static System.Lazy<global::System.Type[]> {methodSymbol.Name}Types = AllTypes;");
                    code.AppendLine($"        internal static System.Lazy<Hl7.Fhir.Introspection.ClassMapping[]> {methodSymbol.Name}ClassMappings = AllClassMappings;");
                    code.AppendLine($"        internal static System.Lazy<Hl7.Fhir.Introspection.EnumMapping[]> {methodSymbol.Name}EnumMappings = AllEnumMappings;");
#else
                    code.AppendLine($"        internal static Hl7.Fhir.Introspection.ModelInspector Get{methodSymbol.Name}ModelInspector = GetModelInspector;");
                    code.AppendLine($"        internal static global::System.Type[] {methodSymbol.Name}Types = AllTypes;");
                    code.AppendLine($"        internal static Hl7.Fhir.Introspection.ClassMapping[] {methodSymbol.Name}ClassMappings = AllClassMappings;");
                    code.AppendLine($"        internal static Hl7.Fhir.Introspection.EnumMapping[] {methodSymbol.Name}EnumMappings = AllEnumMappings;");
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

        static HashSet<INamedTypeSymbol> FindAllTypes(GeneratorExecutionContext context, bool doScan, List<INamedTypeSymbol> definedTypes)
        {
            HashSet<INamedTypeSymbol> fhirTypes;
            if (doScan || definedTypes.Count == 0)
            {
                fhirTypes = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
                var hl7Asms = definedTypes
                    .Select(static x => x.ContainingAssembly)
                    .Distinct(SymbolEqualityComparer.Default)
                    .OfType<IAssemblySymbol>()
                    .ToArray();

                // if the AssembliesContainingTypes property is empty, we scan for referenced
                // assemblies with the FhirModelAssemblyAttribute
                if (hl7Asms.Length == 0)
                {
                    hl7Asms = context.Compilation.References
                        .Select(context.Compilation.GetAssemblyOrModuleSymbol)
                        .OfType<IAssemblySymbol>()
                        .Where(static x => x.TryGetAttribute(FhirModelAssemblyAttributeName, out _))
                        .ToArray();
                }

                context.Compilation.GlobalNamespace.TraverseNamespace(fhirTypes, context.CancellationToken);
                foreach (var asm in hl7Asms)
                {
                    asm.GlobalNamespace.TraverseNamespace(fhirTypes, context.CancellationToken);
                }
            }
            else
            {
                fhirTypes = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);

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

            return fhirTypes;
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

        static void WriteCqlType(StringBuilder code, INamedTypeSymbol cqlType, AttributeData data)
        {
            code.AppendLine($"          Hl7.Fhir.Introspection.ClassMapping.Build(\"System.{cqlType.Name}\", typeof({cqlType.ToDisplayString()}), FhirRelease),");
        }

        static void WriteFhirType(StringBuilder code, INamedTypeSymbol fhirType, AttributeData data)
        {
            // for FhirTypeAttribute
            var name = data.ConstructorArguments[0].Value?.ToString();
            var canonical = data.ConstructorArguments.ElementAtOrDefault(1).Value?.ToString();
            var isResource = data.NamedArguments.FirstOrDefault(static x => x.Key == "IsResource").Value.Value?.ToString()?.ToLower();
            var isFhirPrimitive = fhirType.IsDerivedFrom("Hl7.Fhir.Model.PrimitiveType");
            var isCodeOfT = fhirType.IsCodeOfT();
            var hasValidationAttributes = fhirType.GetAttributes().Any(static attrib => attrib.AttributeClass.IsDerivedFrom("System.ComponentModel.Validation.ValidationAttribute"));

            bool isBackbone = false;
            string? definitionPath = null;
            if (fhirType.ContainingType != null && fhirType.TryGetAttribute("Hl7.Fhir.Introspection.BackboneTypeAttribute", out var backboneAttribute))
            {
                isBackbone = true;
                definitionPath = isBackbone ? backboneAttribute!.ConstructorArguments[0].Value?.ToString() : null;
            }

            var isBindable = fhirType.TryGetAttribute("Hl7.Fhir.Introspection.BindableAttribute", out var bindableAttribute);
            code.Append(
                $$"""
                          Hl7.Fhir.Introspection.ClassMapping.Build(
                              {{name.SurroundWithQuotesOrNull()}},
                              typeof({{fhirType.ToDisplayString()}}),
                              FhirRelease,
                              isResource: {{(isResource ?? "false")}},
                              isCodeOfT: {{isCodeOfT.ToString().ToLower()}},
                              isFhirPrimitive: {{isFhirPrimitive.ToString().ToLower()}},
                              isBackboneType: {{isBackbone.ToString().ToLower()}},
                              definitionPath: {{definitionPath.SurroundWithQuotesOrNull()}},
                              isBindable: {{isBindable.ToString().ToLower()}},
                              canonical: {{canonical.SurroundWithQuotesOrNull()}},
                              validationAttributes: {{(hasValidationAttributes ? $"GetValidationAttributes(typeof({fhirType.ToDisplayString()}), FhirRelease).ToArray(), // this can be optimized further" : "[], // it appears this is always empty")}}
                              propertyMapFactory: null // todo
                          ),
                """);
            code.AppendLine($"");
        }

        static void WriteFhirEnumeration(StringBuilder code, INamedTypeSymbol enumType, AttributeData data)
        {
            // for FhirEnumerationAttribute
            // arg1 is name,
            // arg2 is the valueset,
            // arg3 is the system
            var name = data.ConstructorArguments[0].Value?.ToString();
            var valueset = data.ConstructorArguments[1].Value?.ToString();
            var system = data.ConstructorArguments.ElementAtOrDefault(2).Value?.ToString();
            code.AppendLine($"          Hl7.Fhir.Introspection.EnumMapping.Build({name.SurroundWithQuotesOrNull()}, {valueset.SurroundWithQuotesOrNull()}, typeof({enumType.ToDisplayString()}), FhirRelease, {system.SurroundWithQuotesOrNull()}),");
        }
    }

}
