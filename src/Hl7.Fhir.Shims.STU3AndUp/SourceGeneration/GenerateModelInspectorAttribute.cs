using System;

namespace Hl7.Fhir.Model
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class GenerateModelInspectorAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GenerateAllFhirTypesAttribute"/> class.
        /// </summary>
        public GenerateModelInspectorAttribute()
            : this(true, [])
        {
        }

        public GenerateModelInspectorAttribute(Type type)
            : this(true, [type])
        {
        }

        public GenerateModelInspectorAttribute(Type type1, Type type2)
            : this(true, [type1, type2])
        {
        }

        public GenerateModelInspectorAttribute(Type type1, Type type2, Type type3)
            : this(true, [type1, type2, type3])
        {
        }

        public GenerateModelInspectorAttribute(params Type[] assembliesContainingType)
            : this(true, assembliesContainingType)
        {
        }

        public GenerateModelInspectorAttribute(bool scanAllTypes, Type type)
            : this(scanAllTypes, [type])
        {
        }

        public GenerateModelInspectorAttribute(bool scanAllTypes, Type type1, Type type2)
            : this(scanAllTypes, [type1, type2])
        {
        }

        public GenerateModelInspectorAttribute(bool scanAllTypes, Type type1, Type type2, Type type3)
            : this(scanAllTypes, [type1, type2, type3])
        {
        }

        public GenerateModelInspectorAttribute(bool scanAllTypes, params Type[] types)
        {
            ScanAllTypes = scanAllTypes;
            Types = types;
        }

        public bool ScanAllTypes { get; }

        public Type[] Types { get; }
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

        public GenerateAllFhirTypesAttribute(params Type[] assembliesContainingType)
        {
            AssembliesContainingTypes = assembliesContainingType;
        }

        public Type[] AssembliesContainingTypes { get; set; } = [];
    }
}