/* 
 * Copyright (c) 2023, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/firely-net-sdk/master/LICENSE
 */

#nullable enable

using Hl7.Fhir.Specification;
using Hl7.Fhir.Utility;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Hl7.Fhir.Introspection
{
    /// <summary>
    /// A container for the metadata of a FHIR code from a valueset as present on the .NET Enum member.
    /// </summary>
    public class EnumMemberMapping
    {
        /// <summary>
        /// The code that is represented by this member.
        /// </summary>
        public required string Code { get; init; }

        /// <summary>
        /// The coding system that is associated with the code.
        /// </summary>
        public string? System { get; init; }

        /// <summary>
        /// A description of the concept.
        /// </summary>
        public string? Description { get; init; }

        /// <summary>
        /// The .NET enum value for this enum member.
        /// </summary>
        public required object Value { get; init; }

        /// <summary>
        /// The original <see cref="FieldInfo"/> the metadata was extracted from.
        /// </summary>
        public FieldInfo NativeField { get; init; } = default!;

        /// <summary>
        /// Inspects the given enum member, extracting metadata from its attributes and creating a new <see cref="EnumMemberMapping"/>.
        /// </summary>
        public static bool TryCreate(FieldInfo member, [NotNullWhen(true)] out EnumMemberMapping? result, FhirRelease release = (FhirRelease)int.MaxValue, string? defaultSystem = null)
        {
            result = null;
            if (ClassMapping.GetAttribute<EnumLiteralAttribute>(member, release) is not { } ela) return false;

            var code = ela.Literal ?? member.Name;
            var value = (Enum)member.GetValue(null)!;
            var desc = ClassMapping.GetAttribute<DescriptionAttribute>(member, release)?.Description;

            result = new EnumMemberMapping
            {
                Code = code,
                System = ela.System ?? defaultSystem,
                Value = value,
                Description = desc,
                NativeField = member,
            };
            return true;
        }
    }
}

#nullable restore