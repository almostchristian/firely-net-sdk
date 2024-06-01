using System;

namespace Hl7.Fhir.Model
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class GenerateModelInspectorAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GenerateModelInspectorAttribute"/> class.
        /// </summary>
        public GenerateModelInspectorAttribute()
            : this(ModelInspectorGenerationTypeInclusionMode.Default, [])
        {
        }

        public GenerateModelInspectorAttribute(Type type)
            : this(ModelInspectorGenerationTypeInclusionMode.Default, [type])
        {
        }

        public GenerateModelInspectorAttribute(Type type1, Type type2)
            : this(ModelInspectorGenerationTypeInclusionMode.Default, [type1, type2])
        {
        }

        public GenerateModelInspectorAttribute(Type type1, Type type2, Type type3)
            : this(ModelInspectorGenerationTypeInclusionMode.Default, [type1, type2, type3])
        {
        }

        public GenerateModelInspectorAttribute(params Type[] assembliesContainingType)
            : this(ModelInspectorGenerationTypeInclusionMode.IncludeAllTypesInAssembly, assembliesContainingType)
        {
        }

        public GenerateModelInspectorAttribute(ModelInspectorGenerationTypeInclusionMode typeInclusionMode, Type type)
            : this(typeInclusionMode, [type])
        {
        }

        public GenerateModelInspectorAttribute(ModelInspectorGenerationTypeInclusionMode typeInclusionMode, Type type1, Type type2)
            : this(typeInclusionMode, [type1, type2])
        {
        }

        public GenerateModelInspectorAttribute(ModelInspectorGenerationTypeInclusionMode typeInclusionMode, Type type1, Type type2, Type type3)
            : this(typeInclusionMode, [type1, type2, type3])
        {
        }

        public GenerateModelInspectorAttribute(ModelInspectorGenerationTypeInclusionMode typeInclusionMode, params Type[] types)
        {
            TypeInclusionMode = typeInclusionMode;
            Types = types;
        }

        public ModelInspectorGenerationTypeInclusionMode TypeInclusionMode { get; }

        public Type[] Types { get; }
    }

    public enum ModelInspectorGenerationTypeInclusionMode
    {
        Default = IncludeAllTypesInAssembly,
        IncludeAllTypesInAssembly = 0,
        IncludeOnlyListedTypes,
        IncludeAllCoreFhirTypes,
    }
}