using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace Hl7.Fhir.Model.SourceGeneration
{
    internal static class Helpers
    {
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

        public static bool IsCodeOfT(this INamedTypeSymbol type)
        {
            return type.Arity == 1 && type.ToDisplayString().StartsWith("Hl7.Fhir.Mode.Code<");
        }

        public static bool IsDerivedFrom(this INamedTypeSymbol type, string typeName)
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

        public static bool IsCqlType(this INamedTypeSymbol type)
            => type.ToDisplayString() == "Hl7.Fhir.ElementModel.Types.Any" ||
            type.BaseType?.ToDisplayString() == "Hl7.Fhir.ElementModel.Types.Any";

        public static bool IsClassMapping(this INamedTypeSymbol type, out AttributeData? data)
            => type.TryGetAttribute("Hl7.Fhir.Introspection.FhirTypeAttribute", out data) || IsCqlType(type);

        public static bool IsEnumMapping(this INamedTypeSymbol type, out AttributeData? data)
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

        internal static void TraverseNamespace(this INamespaceSymbol ns, HashSet<INamedTypeSymbol> allTypes, List<(INamedTypeSymbol, AttributeData)> classMappings, List<(INamedTypeSymbol, AttributeData)> enumMappings)
        {
            foreach (var type in ns.GetTypeMembers())
            {
                if (!type.IsGenericType &&
                    type.CanBeReferencedByName &&
                    (type.IsClassMapping(out _) || type.IsEnumMapping(out _)))
                {
                    if (allTypes.Add(type))
                    {
                        if (type.IsClassMapping(out var typeData))
                        {
                            classMappings.Add((type, typeData));
                        }
                        else if (type.IsEnumMapping(out var enumData))
                        {
                            enumMappings.Add((type, enumData));
                        }

                        foreach (var nestedType in type.GetTypeMembers().Where(x => x.IsClassMapping(out _) || x.IsEnumMapping(out _)))
                        {
                            allTypes.Add(nestedType);
                            if (nestedType.IsClassMapping(out var typeData2))
                            {
                                classMappings.Add((nestedType, typeData2));
                            }
                            else if (nestedType.IsEnumMapping(out var enumData2))
                            {
                                enumMappings.Add((nestedType, enumData2));
                            }
                        }
                    }
                }
            }

            foreach (var childNamespace in ns.GetNamespaceMembers())
            {
                TraverseNamespace(childNamespace, allTypes, classMappings, enumMappings);
            }
        }
    }
}
