using Hl7.Fhir.Introspection;
using Hl7.Fhir.Specification;
using Hl7.Fhir.Utility;
using System;

namespace Hl7.Fhir.Model
{
    /// <summary>
    /// Contains partial stubs for generating source gen mappings.
    /// </summary>
    public partial class ModelInfo
    {
        private static readonly FhirRelease release = FhirReleaseParser.Parse(Version);
        private static readonly Lazy<ModelInspector> cachedModelInspector = new(() => new ModelInspector(GenerateAllClassMappings(), GenerateAllEnumMappings()) { FhirRelease = release, FhirVersion = Version });

        public static ModelInspector CachedModelInspector => cachedModelInspector.Value;

        /// <summary>
        /// Only used for benchmark verification.
        /// </summary>
        /// <returns></returns>
        [GenerateAllFhirTypes]
        public static partial Type[] GenerateAllFhirTypes();

        /// <summary>
        /// Only used for benchmark verification.
        /// </summary>
        /// <returns></returns>
        [GenerateAllFhirTypes]
        public static partial EnumMapping[] GenerateAllEnumMappings();

        /// <summary>
        /// Only used for benchmark verification.
        /// </summary>
        /// <returns></returns>
        [GenerateAllFhirTypes]
        public static partial ClassMapping[] GenerateAllClassMappings();
    }

    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    internal sealed class GenerateAllFhirTypesAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GenerateAllFhirTypesAttribute"/> class.
        /// </summary>
        public GenerateAllFhirTypesAttribute()
        {
        }

        public Type[] AssembliesContainingTypes { get; set; } = [];
    }
}