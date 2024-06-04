using BenchmarkDotNet.Attributes;
using Hl7.Fhir.Model;
using Hl7.Fhir.Utility;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Firely.Sdk.Benchmarks
{
    [MemoryDiagnoser]
    public class PropertyGetterSetterBenchmarks
    {
        MyObject objectInstance = new();
        object valueToSet;
        Action<object, object> setter;
        Func<object, object> getter;

        [ParamsAllValues]
        public bool Struct { get; set; }

        [ParamsAllValues]
        public PropertyAccessMode Mode { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            if (Mode == PropertyAccessMode.Reflection)
            {
                var type = typeof(MyObject);
                PropertyInfo prop = Struct ? type.GetProperty(nameof(MyObject.StructValue)) : type.GetProperty(nameof(MyObject.RefValue));
                getter = prop.GetValueGetter();
                setter = prop.GetValueSetter();
            }
            else if (Mode == PropertyAccessMode.Cast)
            {
                if (Struct)
                {
                    getter = o => ((MyObject)o).StructValue;
                    setter = (o, v) => ((MyObject)o).StructValue = (DateTimeOffset?)v;
                }
                else
                {
                    getter = o => ((MyObject)o).RefValue;
                    setter = (o, v) => ((MyObject)o).RefValue = (FhirString?)v;
                }
            }
            else if (Mode == PropertyAccessMode.UnsafeAs)
            {
                if (Struct)
                {
                    getter = o => System.Runtime.CompilerServices.Unsafe.As<MyObject>(o).StructValue;
                    setter = (o, v) => System.Runtime.CompilerServices.Unsafe.As<MyObject>(o).StructValue = System.Runtime.CompilerServices.Unsafe.Unbox<DateTimeOffset>(v);
                }
                else
                {
                    getter = o => System.Runtime.CompilerServices.Unsafe.As<MyObject>(o).RefValue;
                    setter = (o, v) => System.Runtime.CompilerServices.Unsafe.As<MyObject>(o).RefValue = System.Runtime.CompilerServices.Unsafe.As<FhirString>(v);
                }
            }

            valueToSet = Struct ? DateTimeOffset.UtcNow : new FhirString(Guid.NewGuid().ToString());
        }

        [Benchmark]
        public void SetValue()
        {
            setter(objectInstance, valueToSet);
        }

        [Benchmark]
        public object GetValue()
        {
            return getter(objectInstance);
        }

        public sealed class MyObject
        {
            public DateTimeOffset? StructValue { get; set; }

            public FhirString RefValue { get; set; }
        }

        public enum PropertyAccessMode
        {
            Reflection,
            Cast,
            UnsafeAs
        }
    }
}
