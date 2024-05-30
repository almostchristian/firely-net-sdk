/* 
 * Copyright (c) 2020, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/firely-net-sdk/master/LICENSE
 */

#nullable enable

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Hl7.Fhir.Introspection
{
    internal class ClassMappingCollection
    {
        public ClassMappingCollection()
        {
            _byName = new(StringComparer.OrdinalIgnoreCase);
            _byType = new();
            _byCanonical = new();
        }

        public ClassMappingCollection(IEnumerable<ClassMapping> mappings)
        {
            _byName = new(mappings.Select(static x => new KeyValuePair<string, ClassMapping>(x.Name, x)), StringComparer.OrdinalIgnoreCase);
            _byType = new(mappings.Select(static x => new KeyValuePair<Type, ClassMapping>(x.NativeType, x)));
            _byCanonical = new(mappings.Where(static x => x.Canonical is not null).Select(static x => new KeyValuePair<string, ClassMapping>(x.Canonical!, x)));
        }

        /// <summary>
        /// Adds the mapped type to the collection, updating the indexed
        /// collections. Note: a newer mapping for the same canonical/name will overwrite
        /// the old one. This way, it is possible to substitute mappings if necessary.
        /// </summary>
        public void Add(ClassMapping mapping)
        {
            var propKey = mapping.Name;
            _byName[propKey] = mapping;

            _byType[mapping.NativeType] = mapping;

            var canonical = mapping.Canonical;
            if (canonical is not null)
                _byCanonical[canonical] = mapping;
        }

        public void AddRange(IEnumerable<ClassMapping> mappings)
        {
            foreach (var mapping in mappings)
                Add(mapping);
        }

        /// <summary>
        /// List of the class mappings, keyed by name.
        /// </summary>
        public IReadOnlyDictionary<string, ClassMapping> ByName => _byName;
        private readonly ConcurrentDictionary<string, ClassMapping> _byName;

        /// <summary>
        /// List of the class mappings, keyed by canonical.
        /// </summary>
        public IReadOnlyDictionary<string, ClassMapping> ByCanonical => _byCanonical;
        private readonly ConcurrentDictionary<string, ClassMapping> _byCanonical;

        /// <summary>
        /// List of the class mappings, keyed by canonical.
        /// </summary>
        public IReadOnlyDictionary<Type, ClassMapping> ByType => _byType;
        public readonly ConcurrentDictionary<Type, ClassMapping> _byType;
    }
}

#nullable restore