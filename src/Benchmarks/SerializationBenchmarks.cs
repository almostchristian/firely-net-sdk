﻿using BenchmarkDotNet.Attributes;
using Hl7.Fhir.ElementModel;
using Hl7.Fhir.Introspection;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Hl7.Fhir.Utility;
using System.IO;
using System.Text.Json;

namespace Firely.Sdk.Benchmarks
{
    [MemoryDiagnoser]
    public class SerializationBenchmarks
    {
        internal Patient Patient;
        JsonSerializerOptions Options;
        BaseFhirXmlPocoSerializer XmlSerializer;


        [Params(false, true)]
        public bool UseSourceGen { get; set; }

        [GlobalSetup]
        public void BenchmarkSetup()
        {
            ModelInspector modelInspector;

            if (UseSourceGen)
            {
                modelInspector = ModelInspector.ForPredefinedMappings(ModelInfo.Version, ModelInspectorBenchmarks.ClassMappingsAllResources(), ModelInspectorBenchmarks.EnumMappingsAllResources());
            }
            else
            {
                modelInspector = ModelInspector.ForAssembly(typeof(ModelInfo).Assembly);
            }

            var filename = Path.Combine("TestData", "fp-test-patient.json");
            var data = File.ReadAllText(filename);
            // For now, deserialize with the existing deserializer, until we have completed
            // the dynamicserializer too.
            Patient = FhirJsonNode.Parse(data).ToPoco<Patient>();
            Options = new JsonSerializerOptions().ForFhir(modelInspector);
            XmlSerializer = new FhirXmlPocoSerializer();
        }

        [Benchmark]
        public string JsonDictionarySerializer()
        {
            return JsonSerializer.Serialize(Patient, Options);
        }

        //[Benchmark]
        public string XmlDictionarySerializer()
        {
            return SerializationUtil.WriteXmlToString(Patient, (o, w) => XmlSerializer.Serialize(o, w));
        }

        //[Benchmark]
        public string TypedElementSerializerJson()
        {
            return Patient.ToTypedElement().ToJson();
        }

        [Benchmark]
        public string TypedElementSerializerXml()
        {
            return Patient.ToTypedElement().ToXml();
        }
    }
}