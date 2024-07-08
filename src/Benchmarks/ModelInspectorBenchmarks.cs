using BenchmarkDotNet.Attributes;
using Hl7.Fhir.Introspection;
using Hl7.Fhir.Model;
using Hl7.Fhir.Utility;
using System;
using System.Linq;

namespace Firely.Sdk.Benchmarks;

[MemoryDiagnoser]
public partial class ModelInspectorBenchmarks
{
    [GenerateModelInspector(ModelInspectorGenerationTypeInclusionMode.IncludeAllCoreFhirTypes, typeof(CapabilityStatement), typeof(Appointment), typeof(OperationDefinition))]
    public static partial Type[] Types4Resources();

    [GenerateModelInspector(ModelInspectorGenerationTypeInclusionMode.IncludeAllCoreFhirTypes, typeof(CapabilityStatement), typeof(Appointment), typeof(OperationDefinition))]
    public static partial ClassMapping[] ClassMappings4Resources();

    [GenerateModelInspector(ModelInspectorGenerationTypeInclusionMode.IncludeAllCoreFhirTypes, typeof(CapabilityStatement), typeof(Appointment), typeof(Annotation), typeof(VirtualServiceDetail), typeof(OperationDefinition))]
    public static partial EnumMapping[] EnumMappings4Resources();

    [GenerateModelInspector(ModelInspectorGenerationTypeInclusionMode.Default, typeof(FhirString), typeof(CapabilityStatement), typeof(Patient))]
    public static partial Type[] TypesAllResources();

    [GenerateModelInspector]
    public static partial EnumMapping[] EnumMappingsAllResources();

    [GenerateModelInspector]
    public static partial ClassMapping[] ClassMappingsAllResources();


    [GlobalSetup]
    public void BenchmarkSetup()
    {
        //  PropertyInfoExtensions.NoCodeGenSupport = true;

        var inspector = ScanAssemblies();
        Console.WriteLine($"ScanAssemblies:                Types: {inspector.ClassMappings.Count}, Enums: {inspector.EnumMappings.Count()}");

        inspector = ImportTypeAllResources();
        Console.WriteLine($"ImportTypeAllResources:        Types: {inspector.ClassMappings.Count}, Enums: {inspector.EnumMappings.Count()}");

        inspector = SourceGenMappingsAllResources();
        Console.WriteLine($"SourceGenMappingsAllResources: Types: {inspector.ClassMappings.Count}, Enums: {inspector.EnumMappings.Count()}");

        //inspector = NewWithTypesAllResources();
        //Console.WriteLine($"NewWithTypesAllResources:      Types: {inspector.ClassMappings.Count}, Enums: {inspector.EnumMappings.Count()}");

        inspector = ImportType4Resources();
        Console.WriteLine($"ImportType4Resources:          Types: {inspector.ClassMappings.Count}, Enums: {inspector.EnumMappings.Count()}");

        inspector = SourceGenMappings4Resources();
        Console.WriteLine($"SourceGenMappings4Resources:   Types: {inspector.ClassMappings.Count}, Enums: {inspector.EnumMappings.Count()}");

        //inspector = NewWithTypes4Resources();
        //Console.WriteLine($"NewWithTypes4Resources:        Types: {inspector.ClassMappings.Count}, Enums: {inspector.EnumMappings.Count()}");
    }

    internal static readonly Type[] PopularResources = new Type[]
    {
            typeof(Observation), typeof(Patient), typeof(Organization),
            typeof(Procedure), typeof(StructureDefinition), typeof(MedicationRequest),
            typeof(ValueSet), typeof(Questionnaire), typeof(Appointment),
            typeof(OperationOutcome)
    };


    internal static readonly Type[] TestResources =
    [
            typeof(CapabilityStatement), typeof(Appointment), typeof(OperationDefinition),
    ];

    //[IterationSetup]
    //[IterationCleanup]
    public void ResetCache()
    {
        // Make sure we work uncached initially on each run
        ModelInspector.Clear();
        ClassMapping.Clear();
        EnumMapping.Clear();
    }

    [Benchmark]
    public ModelInspector ScanAssemblies()
    {
        ResetCache();
        return ModelInspector.ForAssembly(typeof(ModelInfo).Assembly);
    }

    [Benchmark]
    public ModelInspector ImportTypeAllResources()
    {
        ResetCache();
        var inspector = new ModelInspector(Hl7.Fhir.Specification.FhirRelease.R5);
        foreach (var t in TypesAllResources())
        {
            inspector.ImportType(t);
        }

        return inspector;
    }

    [Benchmark]
    public ModelInspector SourceGenMappingsAllResources()
    {
        ResetCache();
        return ModelInspector.ForPredefinedMappings(ModelInfo.Version, ClassMappingsAllResources(), EnumMappingsAllResources());
    }

    //[Benchmark]
    //public ModelInspector NewWithTypesAllResources()
    //{
    //    FhirReleaseParser.Parse(ModelInfo.Version);
    //    ResetCache();
    //    return ModelInspector.ForTypes(ModelInfo.Version, TypesAllResources());
    //}

    [Benchmark]
    public ModelInspector ImportType4Resources()
    {
        ResetCache();
        var inspector = ModelInspector.ForAssembly(typeof(FhirString).Assembly);
        foreach (var t in TestResources)
        {
            inspector.ImportType(t);
        }

        return inspector;
    }

    [Benchmark]
    public ModelInspector SourceGenMappings4Resources()
    {
        ResetCache();
        return ModelInspector.ForPredefinedMappings(ModelInfo.Version, ClassMappings4Resources(), EnumMappings4Resources());
    }

    //[Benchmark]
    //public ModelInspector NewWithTypes4Resources()
    //{
    //    ResetCache();
    //    var inspector = ModelInspector.ForTypes(ModelInfo.Version, Types4Resources());
    //    return inspector;
    //}

    //[Benchmark]
    //public void GetPropertiesAll()
    //{
    //    // Make sure we work uncached initially on each run
    //    ModelInspector.Clear();
    //    ClassMapping.Clear();

    //    var inspector = ModelInspector.ForAssembly(typeof(ModelInfo).Assembly);
    //    foreach (var m in inspector.ClassMappings)
    //    {
    //        _ = m.PropertyMappings;
    //    }
    //}

}
