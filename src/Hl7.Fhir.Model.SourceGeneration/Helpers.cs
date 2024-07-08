using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;

namespace Hl7.Fhir.Model.SourceGeneration;

internal static class Helpers
{
    internal static readonly Type[] SupportedDotNetPrimitiveTypes =
        [
            typeof(int), typeof(uint), typeof(long), typeof(ulong),
            typeof(float), typeof(double), typeof(decimal),
            typeof(string),
            typeof(bool),
            typeof(DateTimeOffset),
            typeof(byte[]),
            typeof(Enum)
        ];
    internal static readonly HashSet<string> SupportedDotNetPrimitiveTypeNames = new HashSet<string>(SupportedDotNetPrimitiveTypes.Select(x => x.FullName));

    public static string SurroundWithQuotesOrNull(this string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return "null";
        }
        else
        {
            return $"\"{value}\"";
        }
    }

    public static string ToCSharp(this Accessibility accessibility)
        => accessibility switch
        {
            Accessibility.Internal => "internal",
            Accessibility.Private => "private",
            Accessibility.ProtectedOrInternal or Accessibility.ProtectedAndInternal => "protected internal",
            Accessibility.Protected => "protected",
            Accessibility.Public => "public",
            _ => "internal",
        };

    public static bool IsCodeOfT(this ITypeSymbol type)
    {
        return type is INamedTypeSymbol nt && nt.Arity == 1 && type.ToDisplayString().StartsWith("Hl7.Fhir.Mode.Code<");
    }

    public static bool IsDerivedFrom(this ITypeSymbol type, string typeName)
    {
        if (type.ToDisplayString() == typeName)
        {
            return true;
        }
        else if (type.BaseType is not null)
        {
            return IsDerivedFrom(type.BaseType, typeName);
        }

        return false;
    }

    public static bool IsCqlType(this ITypeSymbol type)
        => type.ToDisplayString() == "Hl7.Fhir.ElementModel.Types.Any" ||
        type.BaseType?.ToDisplayString() == "Hl7.Fhir.ElementModel.Types.Any";

    public static bool IsSupportedNetType(this ITypeSymbol type)
        => SupportedDotNetPrimitiveTypeNames.Contains(type.ToDisplayString());

    public static bool IsClassMapping(this ITypeSymbol type, out AttributeData? data)
        => type.TryGetAttribute("Hl7.Fhir.Introspection.FhirTypeAttribute", out data) || type.IsCqlType() || type.IsSupportedNetType();

    public static bool IsEnumMapping(this ITypeSymbol type, out AttributeData? data)
        => type.TryGetAttribute("Hl7.Fhir.Utility.FhirEnumerationAttribute", out data);

    public static bool TryGetAttribute(this ISymbol symbol, string attributeTypeName, out AttributeData? attributeData)
    {
        foreach (var attributeListSyntax in symbol.GetAttributes())
        {
            if (attributeListSyntax.AttributeClass?.ToDisplayString() == attributeTypeName)
            {
                attributeData = attributeListSyntax;
                return true;
            }
        }

        attributeData = null;
        return false;
    }

    public static bool TryGetVersionedAttribute(this ISymbol symbol, string attributeTypeName, FhirRelease fhirRelease, out AttributeData? attributeData)
    {
        var matches = symbol.GetAttributes()
            .Where(x => x.AttributeClass?.ToDisplayString() == attributeTypeName)
            .Select(attrib => (attrib, since: attrib.ReadSinceProperty()))
            .OrderByDescending(x => x.since)
            .ToList();

        foreach (var match in matches)
        {
            var notMappedSince = match.since;
            if (notMappedSince == null || notMappedSince <= fhirRelease)
            {
                attributeData = match.attrib;
                return true;
            }
        }

        attributeData = null;
        return false;
    }

    internal static void TraverseNamespace(this INamespaceSymbol ns, HashSet<ITypeSymbol> allTypes, CancellationToken cancellationToken)
    {
        foreach (var type in ns.GetTypeMembers())
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            if (!type.IsGenericType &&
                type.CanBeReferencedByName &&
                (type.IsClassMapping(out _) || type.IsEnumMapping(out _)))
            {
                allTypes.Add(type);
            }

            foreach (var nestedType in type.GetTypeMembers().Where(static x => x.IsClassMapping(out _) || x.IsEnumMapping(out _)))
            {
                allTypes.Add(nestedType);
            }
        }

        foreach (var childNamespace in ns.GetNamespaceMembers())
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            TraverseNamespace(childNamespace, allTypes, cancellationToken);
        }
    }

    internal static void PopulateMappings(IEnumerable<ITypeSymbol> allTypes, Dictionary<ITypeSymbol, AttributeData> classMappings, Dictionary<ITypeSymbol, AttributeData> enumMappings)
    {
        foreach (var type in allTypes)
        {
            if (type.IsSupportedNetType())
            {
                classMappings[type] = new EmptyAttributeData();
            }
            else
            {
                if (type.IsClassMapping(out var typeData))
                {
                    classMappings[type] = typeData;
                }
                else if (type.IsEnumMapping(out var enumData))
                {
                    enumMappings[type] = enumData;
                }

                foreach (var nestedType in type.GetTypeMembers())
                {
                    if (nestedType.IsClassMapping(out var typeData2))
                    {
                        classMappings[nestedType] = typeData2;
                    }
                    else if (nestedType.IsEnumMapping(out var enumData2))
                    {
                        enumMappings[nestedType] = enumData2;
                    }
                }
            }
        }
    }


    public static void WriteNetType(this ITypeSymbol netType, StringBuilder code)
    {
        if (netType is IArrayTypeSymbol arrayType)
        {
            code.AppendLine(
                $$"""
                         new Hl7.Fhir.Introspection.ClassMapping
                         {
                            Name = "Net.{{arrayType.ElementType.Name}}[]",
                            NativeType = typeof({{netType.ToDisplayString()}}),
                            Release = FhirRelease,
                            IsPrimitive = true,
                         },
                """);
        }
        else
        {
            code.AppendLine(
                $$"""
                         new Hl7.Fhir.Introspection.ClassMapping
                         {
                            Name = "Net.{{netType.Name}}",
                            NativeType = typeof({{netType.ToDisplayString()}}),
                            Release = FhirRelease,
                            IsPrimitive = true,
                         },
                """);
        }
    }

    public static void WriteCqlType(this ITypeSymbol cqlType, StringBuilder code, AttributeData data)
    {
        code.AppendLine(
            $$"""
                     new Hl7.Fhir.Introspection.ClassMapping
                     {
                        Name = "System.{{cqlType.Name}}",
                        NativeType = typeof({{cqlType.ToDisplayString()}}),
                        Release = FhirRelease,
                     },
            """);
    }

    public static void WriteFhirType(this ITypeSymbol fhirType, StringBuilder code, AttributeData data, IReadOnlyDictionary<ISymbol?, int> classIndex, FhirRelease fhirRelease)
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
                  new Hl7.Fhir.Introspection.ClassMapping(cm =>
                  [

            """);

        var properties = GetProps(fhirType).ToList();

        static IEnumerable<IPropertySymbol> GetProps(ITypeSymbol fhirType) => fhirType
            .GetMembers()
            .Where(x => x.Kind == SymbolKind.Property && x.CanBeReferencedByName)
            .Union(fhirType.BaseType != null ? GetProps(fhirType.BaseType) : [], SymbolEqualityComparer.Default)
            .OfType<IPropertySymbol>();

        var codeType = classIndex.Keys.FirstOrDefault(x => x.ToDisplayString() == "Hl7.Fhir.Model.Code") as ITypeSymbol;
        var enumType = classIndex.Keys.FirstOrDefault(x => x.ToDisplayString() == "System.Enum") as ITypeSymbol;
        foreach (var property in properties)
        {
            if (property.TryGetVersionedAttribute("Hl7.Fhir.Introspection.FhirElementAttribute", fhirRelease, out var elementData))
            {
                if (elementData.ReadSinceProperty() is FhirRelease since)
                {
                    if (since > fhirRelease)
                    {
                        continue;
                    }
                }

                if (property.TryGetVersionedAttribute("Hl7.Fhir.Introspection.NotMappedAttribute", fhirRelease, out var notMappedData))
                {
                    continue;
                }

                // fhirElement values:
                var propName = elementData!.ConstructorArguments[0].Value?.ToString();
                var choice = elementData.NamedArguments.FirstOrDefault(x => x.Key == "Choice").Value.ToCSharpStringOrDefault("Hl7.Fhir.Introspection.ChoiceType.None");
                var order = elementData.NamedArguments.FirstOrDefault(x => x.Key == "Order").Value.Value?.ToString() ?? "0";
                var xmlRep = elementData.NamedArguments.FirstOrDefault(x => x.Key == "XmlSerialization").Value.ToCSharpStringOrDefault("Hl7.Fhir.Specification.XmlRepresentation.None");
                var inSummary = elementData.NamedArguments.FirstOrDefault(x => x.Key == "InSummary").Value.Value?.ToString().ToLower() ?? "false";
                var isModifier = elementData.NamedArguments.FirstOrDefault(x => x.Key == "IsModifier").Value.Value?.ToString().ToLower() ?? "false";
                var fiveWs = elementData.NamedArguments.FirstOrDefault(x => x.Key == "FiveWs").Value.Value?.ToString() ?? string.Empty;

                var propertyType = property.Type;
                bool isCollection = false;
                if (propertyType.ToDisplayString().StartsWith("System.Collections.Generic.List<") && propertyType is INamedTypeSymbol nt && nt.Arity == 1)
                {
                    isCollection = true;
                    propertyType = nt.TypeArguments[0];
                }

                if (propertyType.ToDisplayString().EndsWith("?") && propertyType is INamedTypeSymbol nullable && nullable.Arity == 1)
                {
                    propertyType = nullable.TypeArguments[0];
                }

                var implementingType = propertyType;
                
                if (propertyType.ToDisplayString().StartsWith("Hl7.Fhir.Model.Code<"))
                {
                    propertyType = codeType!;
                }

                if (propertyType.IsEnumMapping(out _))
                {
                    propertyType = enumType;
                }

                List<string> validationAttribs = [$"new Hl7.Fhir.Introspection.FhirElementAttribute({propName.SurroundWithQuotesOrNull()})"];
                foreach (var valAttrib in property.GetAttributes().Where(x => x.ConstructorArguments.Length == 0 && x.NamedArguments.Length == 0 && x.AttributeClass?.BaseType?.ToDisplayString() == "System.ComponentModel.DataAnnotations.ValidationAttribute"))
                {
                    // todo: use singleton instance?
                    validationAttribs.Add($"new {valAttrib.AttributeClass.ToDisplayString()}()");
                }

                string? bindingName = null;
                if (property.TryGetVersionedAttribute("Hl7.Fhir.Introspection.BindingAttribute", fhirRelease, out var bindingData))
                {
                    bindingName = bindingData!.ConstructorArguments[0].Value?.ToString();
                }

                var isMandatory = false;
                if (property.TryGetAttribute("Hl7.Fhir.Validation.CardinalityAttribute", out var cardinalityData) &&
                    int.TryParse(cardinalityData!.NamedArguments.FirstOrDefault(x => x.Key == "Min").Value.Value?.ToString(), out var min))
                {
                    isMandatory = min > 0;
                    validationAttribs.Add($"new Hl7.Fhir.Validation.CardinalityAttribute {{ {string.Join(", ", cardinalityData.NamedArguments.Select(x => $"{x.Key} = {x.Value.Value.ToString()}"))} }}");
                }

                var fhirTypes = $"typeof({propertyType.ToDisplayString(NullableFlowState.None)})";
                
                if (property.TryGetVersionedAttribute("Hl7.Fhir.Introspection.DeclaredTypeAttribute", fhirRelease, out var declaredTypeData))
                {
                    var t = (INamedTypeSymbol)declaredTypeData!.NamedArguments.Single(x => x.Key == "Type").Value.Value!;
                    fhirTypes = $"typeof({t.ToDisplayString(NullableFlowState.None)})";
                    propertyType = t;
                }

                int propertyClassMappingIdx = -1;
                if (classIndex.TryGetValue(propertyType, out var idx))
                {
                    propertyClassMappingIdx = idx;
                }
                else if (classIndex.TryGetValue(implementingType, out idx))
                {
                    propertyClassMappingIdx = idx;
                }

                if (choice != "Hl7.Fhir.Introspection.ChoiceType.None" && property.TryGetAttribute("Hl7.Fhir.Validation.AllowedTypesAttribute", out var typesData))
                {
                    var typeArgs = typesData!.ConstructorArguments[0].Values.OfType<TypedConstant>().Select(x => x.Value);
                    fhirTypes = string.Join(", ", typeArgs.Select(t => $"typeof({t!.ToString()})"));

                    validationAttribs.Add($"new Hl7.Fhir.Validation.AllowedTypesAttribute({fhirTypes})");
                }

                var isPrimitive = property.Type.IsAllowedNativeTypeForDataTypeValue();

                code.AppendLine($"                //CreateProp(cm, cm.NativeType.GetProperty({property.Name.SurroundWithQuotesOrNull()})),");
                code.AppendLine(
                    $$"""
                                    new Hl7.Fhir.Introspection.PropertyMapping(
                                       getter: static i => System.Runtime.CompilerServices.Unsafe.As<{{fhirType.ToDisplayString()}}>(i).{{property.Name}},
                                       setter: static (i, v) => System.Runtime.CompilerServices.Unsafe.As<{{fhirType.ToDisplayString()}}>(i).{{property.Name}} = {{GetCastedSetterValue()}})
                                    {
                                       Name = {{propName.SurroundWithQuotesOrNull()}},
                                       DeclaringClass = cm, // ClassMapping for T,
                                       ImplementingType = typeof({{implementingType.ToDisplayString(NullableFlowState.None)}}),
                                       PropertyTypeMapping = GeneratedModelInspectorContainer.AllClassMappings{{ModelInspectorGenerator.arrayAccess}}[{{propertyClassMappingIdx}}], // ClassMapping for {{propertyType.ToDisplayString()}}
                                       FhirType = [{{fhirTypes}}],
                                       Release = FhirRelease,
                                       InSummary = {{inSummary}},
                                       IsModifier = {{isModifier}},
                                       Choice = {{choice}},
                                       SerializationHint = {{xmlRep}},
                                       Order = {{order}},
                                       IsCollection = {{isCollection.ToString().ToLower()}},
                                       IsMandatoryElement = {{isMandatory.ToString().ToLower()}},
                                       IsPrimitive = {{isPrimitive.ToString().ToLower()}},
                                       RepresentsValueElement = {{(isPrimitive && elementData.IsPrimitiveValueElement()).ToString().ToLower()}},
                                       ValidationAttributes = [{{string.Join(", ", validationAttribs)}}],
                                       FiveWs = {{fiveWs.SurroundWithQuotesOrNull()}},
                                       BindingName = {{bindingName.SurroundWithQuotesOrNull()}},
                                    },
                    """);

                string GetCastedSetterValue() => property.Type.IsValueType ?
                    $"System.Runtime.CompilerServices.Unsafe.Unbox<{property.Type.GetNonNullableSymbol().ToDisplayString()}>(v)" :
                    $"System.Runtime.CompilerServices.Unsafe.As<{property.Type.ToDisplayString(NullableFlowState.None)}>(v)";
            }
        }

        code.Append(
            $$"""
                  ])
                  {
                     Name = {{name.SurroundWithQuotesOrNull()}},
                     NativeType = typeof({{fhirType.ToDisplayString()}}),
                     Release = FhirRelease,
                     IsResource = {{(isResource ?? "false")}},
                     IsCodeOfT = {{isCodeOfT.ToString().ToLower()}},
                     IsFhirPrimitive = {{isFhirPrimitive.ToString().ToLower()}},
                     IsBackboneType = {{isBackbone.ToString().ToLower()}},
                     DefinitionPath = {{definitionPath.SurroundWithQuotesOrNull()}},
                     IsBindable = {{isBindable.ToString().ToLower()}},
                     Canonical = {{canonical.SurroundWithQuotesOrNull()}},
                     ValidationAttributes = {{(hasValidationAttributes ? $"GetValidationAttributes(typeof({fhirType.ToDisplayString()}), FhirRelease).ToArray(), // this can be optimized further" : "[], // it appears this is always empty")}}
                  },
            """);
        code.AppendLine($"");
    }

    private static ITypeSymbol GetNonNullableSymbol(this ITypeSymbol type)
    {
        if (type is INamedTypeSymbol nt && nt.Arity == 1 && nt.ToDisplayString().EndsWith("?"))
        {
            return nt.TypeArguments[0];
        }

        return type;
    }

    private static string ToCSharpStringOrDefault(this TypedConstant value, string defaultValue)
    {
        return value.IsNull ? defaultValue : value.ToCSharpString();
    }

    private static bool IsAllowedNativeTypeForDataTypeValue(this ITypeSymbol type)
    {
        // Special case, allow Nullable<enum>
        if (type is INamedTypeSymbol nt && nt.Arity == 1 && type.ToDisplayString().EndsWith("?"))
            type = nt.TypeArguments[0];

        return type.IsEnumMapping(out _) || Helpers.SupportedDotNetPrimitiveTypeNames.Contains(type.ToDisplayString(NullableFlowState.None));
    }

    private static bool IsPrimitiveValueElement(this AttributeData valueElementAttr)
    {
        return valueElementAttr.NamedArguments.Any(x => x.Key == "IsPrimitiveValue" && bool.Parse(x.Value.Value?.ToString() ?? "true"));
    }

    public static void WriteFhirEnumeration(this ITypeSymbol enumType, StringBuilder code, AttributeData data)
    {
        // for FhirEnumerationAttribute
        // arg1 is name,
        // arg2 is the valueset,
        // arg3 is the system
        var name = data.ConstructorArguments[0].Value?.ToString();
        var valueset = data.ConstructorArguments[1].Value?.ToString();
        var system = data.ConstructorArguments.ElementAtOrDefault(2).Value?.ToString();
        code.AppendLine(
            $$"""
                    new Hl7.Fhir.Introspection.EnumMapping({{system.SurroundWithQuotesOrNull()}})
                    {
                       Name = {{name.SurroundWithQuotesOrNull()}},
                       Canonical = {{valueset.SurroundWithQuotesOrNull()}},
                       NativeType = typeof({{enumType.ToDisplayString()}}),
                       Release = FhirRelease,
                    },
             """);
    }

    public static FhirRelease? ReadSinceProperty(this AttributeData? data)
    {
        if (data != null &&
            int.TryParse(
                (data.NamedArguments.FirstOrDefault(x => x.Key == "Since").Value.Value ?? data.ConstructorArguments.FirstOrDefault().Value)?.ToString(), out var r))
        {
            return (FhirRelease)r;
        }

        return null;
    }

    private sealed class EmptyAttributeData : AttributeData
    {
        protected override INamedTypeSymbol? CommonAttributeClass => throw new NotImplementedException();

        protected override IMethodSymbol? CommonAttributeConstructor => throw new NotImplementedException();

        protected override SyntaxReference? CommonApplicationSyntaxReference => throw new NotImplementedException();

        protected override ImmutableArray<TypedConstant> CommonConstructorArguments => throw new NotImplementedException();

        protected override ImmutableArray<KeyValuePair<string, TypedConstant>> CommonNamedArguments => throw new NotImplementedException();
    }
}
