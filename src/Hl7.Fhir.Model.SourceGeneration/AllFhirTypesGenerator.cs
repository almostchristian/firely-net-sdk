#define CACHING // this changes the source generated code so that it uses Lazy<Type>[] so that the generated types are done only once.
//#define LAUNCH_DEBUGGER
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Hl7.Fhir.Model.SourceGeneration;

[Generator]
public class AllFhirTypesGenerator : ISourceGenerator
{
    private static readonly string AssemblyVersion = typeof(AllFhirTypesGenerator).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "1.0.0.0";

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
        var hl7Asms = receiver.MethodDeclarations
            .SelectMany(x => x.Assemblies)
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
                .Where(x => x.TryGetAttribute("Hl7.Fhir.Introspection.FhirModelAssemblyAttribute", out _))
                .ToArray();
        }

        var fhirTypes = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
        var classMappings = new List<(INamedTypeSymbol, AttributeData)>();
        var enumMappings = new List<(INamedTypeSymbol, AttributeData)>();

        if (context.CancellationToken.IsCancellationRequested)
        {
            return;
        }

        context.Compilation.GlobalNamespace.TraverseNamespace(fhirTypes, classMappings, enumMappings, context.CancellationToken);
        foreach (var asm in hl7Asms)
        {
            if (context.CancellationToken.IsCancellationRequested)
            {
                return;
            }

            asm.GlobalNamespace.TraverseNamespace(fhirTypes, classMappings, enumMappings, context.CancellationToken);
        }

        if (fhirTypes.Count > 0)
        {
#if CACHING
            var arrayTerminator = "]);";
            var arrayAccess = ".Value";
#else
            var arrayTerminator = "];";
            var arrayAccess = "()";
#endif
            StringBuilder code = new(
                $$"""
            namespace Hl7.Fhir.Model.SourceGeneration
            {
                using Hl7.Fhir.Introspection;
                using System.Linq;
                [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Hl7.Fhir.Model.SourceGeneration", "{{AssemblyVersion}}")]
                internal static class AllTypesContainer
                {
                    private static readonly Hl7.Fhir.Specification.FhirRelease FhirRelease = Hl7.Fhir.Utility.FhirReleaseParser.Parse(Hl7.Fhir.Model.ModelInfo.Version);
            
                    private static global::System.Collections.Generic.IEnumerable<global::System.ComponentModel.DataAnnotations.ValidationAttribute> GetValidationAttributes(global::System.Reflection.MemberInfo t, Hl7.Fhir.Specification.FhirRelease version)
                    {
                        return Hl7.Fhir.Utility.ReflectionHelper.GetAttributes<global::System.ComponentModel.DataAnnotations.ValidationAttribute>(t).Where(isRelevant);

                        bool isRelevant(global::System.Attribute a) => a is not Hl7.Fhir.Introspection.IFhirVersionDependent vd || a.AppliesToRelease(version);
                    }
                    
                    public static {{WriteMethodSignature("System.Type", "AllTypes")}}() =>
                    [

            """);

            foreach (var fhirType in fhirTypes)
            {
                code.AppendLine($"          typeof({fhirType.ToDisplayString()}),");
            }

            if (context.CancellationToken.IsCancellationRequested)
            {
                return;
            }

            code.Append(
                $$"""
                    {{arrayTerminator}}
                    public static {{WriteMethodSignature("Hl7.Fhir.Introspection.ClassMapping", "AllClassMappings")}}() =>
                    [

            """);

            foreach (var fhirType in classMappings)
            {
                if (context.CancellationToken.IsCancellationRequested)
                {
                    return;
                }

                if (fhirType.Item1.IsCqlType())
                {
                    WriteCqlType(code, fhirType);
                }
                else
                {
                    WriteFhirType(code, fhirType);
                }
            }

            code.Append(
                $$"""
                    {{arrayTerminator}}
                    public static {{WriteMethodSignature("Hl7.Fhir.Introspection.EnumMapping", "AllEnumMappings")}}() =>
                    [

            """);

            foreach (var fhirType in enumMappings)
            {
                if (context.CancellationToken.IsCancellationRequested)
                {
                    return;
                }

                WriteFhirEnumeration(code, fhirType);
            }

            code.Append(
                $$"""
                    {{arrayTerminator}}
                }
            }

            """);

            if (context.CancellationToken.IsCancellationRequested)
            {
                return;
            }

            foreach (var item in receiver.MethodDeclarations)
            {
                var methodSymbol = item.Method;
                var returnType = methodSymbol.ReturnType.ToDisplayString();
                var propertyToAccess = $"All{((IArrayTypeSymbol)methodSymbol.ReturnType).ElementType.Name}s";

                // generate code
                code.Append(
                    $$"""

                namespace {{item.Class.ContainingNamespace.ToDisplayString()}}
                {
                    {{item.Class.DeclaredAccessibility.ToCSharp()}}{{(item.Class.IsStatic ? " static" : string.Empty)}} partial class {{item.Class.Name}}
                    {
                        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Hl7.Fhir.Model.SourceGeneration", "{{AssemblyVersion}}")]
                        {{item.Method.DeclaredAccessibility.ToCSharp()}}{{(item.Method.IsStatic ? " static" : string.Empty)}} partial {{returnType}} {{item.Method.Name}}() => Hl7.Fhir.Model.SourceGeneration.AllTypesContainer.{{propertyToAccess}}{{arrayAccess}};
                    }
                }
                """);
            }

#if DEBUG
            var csharp = code.ToString();
            System.Diagnostics.Debug.WriteLine(csharp);
#endif
            context.AddSource("AllTypesContainer.g.cs", SourceText.From(code.ToString()!, Encoding.UTF8));
        }

        static string WriteMethodSignature(string returnType, string methodName)
        {
#if CACHING
            return $"readonly System.Lazy<{returnType}[]> {methodName} = new(";
#else
            return $"{returnType}[] {methodName}";
#endif
        }

        static void WriteCqlType(StringBuilder code, (INamedTypeSymbol, AttributeData) symbol)
        {
            var cqlType = symbol.Item1;
            code.AppendLine($"          new Hl7.Fhir.Introspection.ClassMapping(\"System.{cqlType.Name}\", typeof({cqlType.ToDisplayString()}), FhirRelease),");
        }

        static void WriteFhirType(StringBuilder code, (INamedTypeSymbol, AttributeData) symbol)
        {
            // for FhirTypeAttribute

            var fhirType = symbol.Item1;
            var data = symbol.Item2;
            var name = data.ConstructorArguments[0].Value?.ToString();
            var canonical = data.ConstructorArguments.ElementAtOrDefault(1).Value?.ToString();
            var isResource = data.NamedArguments.FirstOrDefault(x => x.Key == "IsResource").Value.Value?.ToString()?.ToLower();
            var isFhirPrimitive = fhirType.IsDerivedFrom("Hl7.Fhir.Model.PrimitiveType");
            var isCodeOfT = fhirType.IsCodeOfT();
            var hasValidationAttributes = fhirType.GetAttributes().Any(attrib => attrib.AttributeClass.IsDerivedFrom("System.ComponentModel.Validation.ValidationAttribute"));

            bool isBackbone = false;
            string? definitionPath = null;
            if (fhirType.ContainingType != null && fhirType.TryGetAttribute("Hl7.Fhir.Introspection.BackboneTypeAttribute", out var backboneAttribute))
            {
                isBackbone = true;
                definitionPath = isBackbone ? backboneAttribute!.ConstructorArguments[0].Value?.ToString() : null;
            }

            var isBindable = fhirType.TryGetAttribute("Hl7.Fhir.Introspection.BindableAttribute", out var bindableAttribute);
            code.Append($$"""
                      new Hl7.Fhir.Introspection.ClassMapping("{{name}}", typeof({{fhirType.ToDisplayString()}}), FhirRelease)
                      {
                          IsResource = {{(isResource ?? "false")}},
                          IsCodeOfT = {{isCodeOfT.ToString().ToLower()}},
                          IsFhirPrimitive = {{isFhirPrimitive.ToString().ToLower()}},
                          IsBackboneType = {{isBackbone.ToString().ToLower()}},
                          DefinitionPath = {{definitionPath.SurroundWithQuotesOrNull()}},
                          IsBindable = {{isBindable.ToString().ToLower()}},
                          Canonical = {{canonical.SurroundWithQuotesOrNull()}},
                          ValidationAttributes = {{(hasValidationAttributes ? $"GetValidationAttributes(typeof({fhirType.ToDisplayString()}), FhirRelease).ToArray(), // this can be optimized further" : "[],")}}
                      },
            """);
            code.AppendLine($"");
        }

        static void WriteFhirEnumeration(StringBuilder code, (INamedTypeSymbol, AttributeData) symbol)
        {
            // for FhirEnumerationAttribute
            // arg1 is name,
            // arg2 is the valueset,
            // arg3 is the system
            var enumType = symbol.Item1;
            var data = symbol.Item2;
            var name = data.ConstructorArguments[0].Value?.ToString();
            var valueset = data.ConstructorArguments[1].Value?.ToString();
            var system = data.ConstructorArguments.ElementAtOrDefault(2).Value?.ToString();
            code.AppendLine($"          new Hl7.Fhir.Introspection.EnumMapping(\"{name}\", \"{valueset}\", typeof({enumType.ToDisplayString()}), FhirRelease, \"{system}\"),");
        }
    }

}
