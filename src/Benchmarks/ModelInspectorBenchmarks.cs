using BenchmarkDotNet.Attributes;
using Hl7.Fhir.Introspection;
using Hl7.Fhir.Model;
using Hl7.Fhir.Utility;
using System;
using System.Linq;

namespace Firely.Sdk.Benchmarks;

[MemoryDiagnoser]
public class ModelInspectorBenchmarks
{
    [GlobalSetup]
    public void BenchmarkSetup()
    {
        //  PropertyInfoExtensions.NoCodeGenSupport = true;

        var inspector = ScanAssemblies();
        Console.WriteLine($"ScanAssemblies: Types: {inspector.ClassMappings.Count}, Enums: {inspector.EnumMappings.Count()}");

        inspector = ImportTypeAllResources();
        Console.WriteLine($"ImportTypeAllResources: Types: {inspector.ClassMappings.Count}, Enums: {inspector.EnumMappings.Count()}");

        inspector = NewWithSourceGenMappings();
        Console.WriteLine($"NewWithSourceGenMappings: Types: {inspector.ClassMappings.Count}, Enums: {inspector.EnumMappings.Count()}");

        inspector = NewWithTypesAllResources();
        Console.WriteLine($"NewWithTypesAllResources: Types: {inspector.ClassMappings.Count}, Enums: {inspector.EnumMappings.Count()}");

        inspector = ImportType4Resources();
        Console.WriteLine($"ImportType4Resources: Types: {inspector.ClassMappings.Count}, Enums: {inspector.EnumMappings.Count()}");

        inspector = NewWithTypes4Resources();
        Console.WriteLine($"NewWithTypes4Resources: Types: {inspector.ClassMappings.Count}, Enums: {inspector.EnumMappings.Count()}");
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
        foreach (var t in ModelInfo.GenerateAllFhirTypes())
        {
            inspector.ImportType(t);
        }

        return inspector;
    }

    [Benchmark]
    public ModelInspector ImportType4Resources()
    {
        ResetCache();
        var inspector = new ModelInspector(Hl7.Fhir.Specification.FhirRelease.R5); //ModelInspector.ForAssembly(typeof(ModelInfo).Assembly);
        foreach (var t in TestResources)
        {
            inspector.ImportType(t);
        }

        return inspector;
    }

    [Benchmark]
    public ModelInspector NewWithSourceGenMappings()
    {
        FhirReleaseParser.Parse(ModelInfo.Version);
        ResetCache();
        return new ModelInspector(ModelInfo.GenerateAllClassMappings(), ModelInfo.GenerateAllEnumMappings()) { FhirRelease = Hl7.Fhir.Specification.FhirRelease.R5 };
    }

    [Benchmark]
    public ModelInspector NewWithTypesAllResources()
    {
        FhirReleaseParser.Parse(ModelInfo.Version);
        ResetCache();
        return ModelInspector.ForTypes(ModelInfo.Version, ModelInfo.GenerateAllFhirTypes());
    }

    [Benchmark]
    public ModelInspector NewWithTypes4Resources()
    {
        ResetCache();
        var inspector = ModelInspector.ForTypes(ModelInfo.Version, [
            typeof(CapabilityStatement),
            typeof(CapabilityStatement.ConditionalDeleteStatus),
            typeof(CapabilityStatement.ConditionalReadStatus),
            typeof(CapabilityStatement.DocumentMode),
            typeof(CapabilityStatement.EventCapabilityMode),
            typeof(CapabilityStatement.ReferenceHandlingPolicy),
            typeof(CapabilityStatement.ResourceVersionPolicy),
            typeof(CapabilityStatement.RestfulCapabilityMode),
            typeof(CapabilityStatement.SystemRestfulInteraction),
            typeof(CapabilityStatement.TypeRestfulInteraction),
            typeof(CapabilityStatement.DocumentComponent),
            typeof(CapabilityStatement.EndpointComponent),
            typeof(CapabilityStatement.ImplementationComponent),
            typeof(CapabilityStatement.MessagingComponent),
            typeof(CapabilityStatement.OperationComponent),
            typeof(CapabilityStatement.ResourceComponent),
            typeof(CapabilityStatement.ResourceInteractionComponent),
            typeof(CapabilityStatement.RestComponent),
            typeof(CapabilityStatement.SearchParamComponent),
            typeof(CapabilityStatement.SecurityComponent),
            typeof(CapabilityStatement.SoftwareComponent),
            typeof(CapabilityStatement.SupportedMessageComponent),
            typeof(CapabilityStatement.SystemInteractionComponent),
            typeof(Appointment),
            typeof(Appointment.AppointmentStatus),
            typeof(Appointment.IANATimezones),
            typeof(Appointment.ParticipationStatus),
            typeof(Appointment.WeekOfMonth),
            typeof(Appointment.MonthlyTemplateComponent),
            typeof(Appointment.ParticipantComponent),
            typeof(Appointment.RecurrenceTemplateComponent),
            typeof(Appointment.WeeklyTemplateComponent),
            typeof(Appointment.YearlyTemplateComponent),
            typeof(OperationDefinition),
            typeof(OperationDefinition.BindingComponent),
            typeof(OperationDefinition.OperationKind),
            typeof(OperationDefinition.OperationParameterScope),
            typeof(OperationDefinition.OverloadComponent),
            typeof(OperationDefinition.ParameterComponent),
            typeof(OperationDefinition.ReferencedFromComponent),
        ]);
        return inspector;
    }

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
