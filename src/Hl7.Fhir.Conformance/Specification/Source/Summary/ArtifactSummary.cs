﻿/* 
 * Copyright (c) 2017, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://github.com/FirelyTeam/firely-net-sdk/blob/master/LICENSE
 */

#nullable enable

using Hl7.Fhir.Introspection;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Hl7.Fhir.Specification.Summary;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Errors = Hl7.Fhir.Utility.Error;

namespace Hl7.Fhir.Specification.Source
{
    // Note:
    // 1. ArtifactSummaryGenerator creates new (mutable) ArtifactSummaryPropertyBag
    // 2. ArtifactSummaryGenerator calls the available ArtifactSummaryHarvester delegates
    //    to harvest artifact summary information and add it to the property bag
    // 3. ArtifactSummaryGenerator creates a new (read-only) ArtifactSummary instance
    //    from the initialized property bag and returns it to the caller.
    //    This way, harvested property values are protected against modification by callers.

    /// <summary>Represents summary information harvested from a FHIR artifact.</summary>
    /// <remarks>
    /// Created by the <see cref="ArtifactSummaryGenerator"/> class from
    /// a property bag containing the harvested summary information.
    /// <para>
    /// Implements the <see cref="IArtifactSummaryPropertyBag"/> interface
    /// to support extension methods for accessing specific harvested properties.
    /// </para>
    /// </remarks>
    [DebuggerDisplay(@"\{{DebuggerDisplay,nq}}")]
    public class ArtifactSummary : IArtifactSummaryPropertyBag
    {
        /// <summary>Returns an empty <see cref="ArtifactSummary"/> instance.</summary>
        public static ArtifactSummary Empty => new ArtifactSummary(ArtifactSummaryPropertyBag.Empty, ModelInspector.ForAssembly(typeof(IModelInfo).Assembly));

        // Note: omit leading underscore to be CLS compliant
        protected readonly IArtifactSummaryPropertyBag properties;
        private readonly IModelInfo _modelInfo;

        #region Factory

        /// <summary>Create a new <see cref="ArtifactSummary"/> instance from the specified exception.</summary>
        /// <param name="error">An exception that occured while harvesting artifact summary information.</param>
        /// <param name="modelInfo">type information</param>
        public static ArtifactSummary FromException(Exception error, IModelInfo modelInfo) => FromException(error, modelInfo, null);

        /// <summary>Create a new <see cref="ArtifactSummary"/> instance from the specified exception.</summary>
        /// <param name="error">An exception that occured while harvesting artifact summary information.</param>
        /// <param name="origin">The original location of the target artifact.</param>
        /// <param name="modelInfo">type information</param>
        public static ArtifactSummary FromException(Exception error, IModelInfo modelInfo, string? origin)
        {
            if (error is null) { throw Errors.ArgumentNull(nameof(error)); }
            // Create a new (default) ArtifactSummaryPropertyBag to store the artifact origin
            var properties = new ArtifactSummaryPropertyBag();
            if (!string.IsNullOrEmpty(origin))
            {
                properties[ArtifactSummaryProperties.OriginKey] = origin;
            }
            return new ArtifactSummary(properties, modelInfo, error);
        }

        #endregion

        #region ctor

        /// <summary>Create a new <see cref="ArtifactSummary"/> instance from a set of harvested artifact summary properties.</summary>
        /// <param name="properties">A property bag with harvested artifact summary information.</param>
        /// <param name="modelInfo">type information</param>
        public ArtifactSummary(IArtifactSummaryPropertyBag properties, IModelInfo modelInfo) : this(properties, modelInfo, null) { }

        /// <summary>
        /// Create a new <see cref="ArtifactSummary"/> instance from a set of harvested artifact summary properties
        /// and a runtime exception that occured during harvesting.
        /// </summary>
        public ArtifactSummary(IArtifactSummaryPropertyBag properties, IModelInfo modelInfo, Exception? error)
        {
            this.properties = properties ?? throw Errors.ArgumentNull(nameof(properties));
            this.Error = error;
            _modelInfo = modelInfo;
        }

        #endregion

        #region Properties

        /// <summary>Returns information about errors that occured while generating the artifact summary.</summary>
        public Exception? Error { get; }

        /// <summary>Indicates if any errors occured while generating the artifact summary.</summary>
        /// <remarks>If <c>true</c>, then the <see cref="Error"/> property returns detailed error information.</remarks>
        public bool IsFaulted => Error != null; // cf. Task

        /// <summary>Indicates if the summary describes a valid FHIR resource.</summary>
        /// <value>Returns <c>true</c> if <see cref="ResourceTypeName"/> is known (not <c>null</c>), or <c>false</c> otherwise.</value>
        public bool IsFhirResource => ResourceTypeName is not null && _modelInfo.IsCoreModelType(ResourceTypeName);

        /// <summary>Gets the original location of the associated artifact.</summary>
        public string? Origin => properties.GetOrigin();

        /// <summary>Gets the size of the original artifact file.</summary>
        public long? FileSize => properties.GetFileSize();

        /// <summary>Gets the last modified date of the original artifact file.</summary>
        public DateTimeOffset? LastModified => properties.GetLastModified();

        /// <summary>
        /// Get a string value that represents the artifact serialization format,
        /// as defined by the <see cref="FhirSerializationFormats"/> class, if available.
        /// </summary>
        public string? SerializationFormat => properties.GetSerializationFormat();

        /// <summary>
        /// Gets an opaque value that represents the position of the artifact within the container.
        /// Allows the <see cref="CommonDirectorySource"/> to retrieve and deserialize the associated artifact.
        /// </summary>
        public string? Position => properties.GetPosition();

        /// <summary>Gets the type name of the resource.</summary>
        public string? ResourceTypeName => properties.GetTypeName();

        /// <summary>Gets the resource uri.</summary>
        /// <remarks>The <see cref="CommonDirectorySource"/> generates virtual uri values for resources that are not bundle entries.</remarks>
        public string? ResourceUri => properties.GetResourceUri();

        /// <summary>Returns <c>true</c> if the summary describes a Bundle entry resource.</summary>
        public bool IsBundleEntry => properties.IsBundleEntry();

        #endregion

        #region IEnumerable

        /// <summary>Returns an enumerator that iterates through the summary properties.</summary>
        public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => properties.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => properties.GetEnumerator();

        #endregion

        #region IReadOnlyCollection

        /// <summary>Gets the number of harvested property values.</summary>
        public int Count => properties.Count;

        #endregion

        #region IReadOnlyDictionary

        /// <summary>Gets the property value associated with the specified property key.</summary>
        public object this[string key] => properties[key];

        /// <summary>Gets a collection of property keys.</summary>
        public IEnumerable<string> Keys => properties.Keys;

        /// <summary>Gets a collection of property values.</summary>
        public IEnumerable<object> Values => properties.Values;

        /// <summary>Determines whether the summary contains a property value for the specified property key.</summary>
        public bool ContainsKey(string key) => properties.ContainsKey(key);

        /// <summary>Gets the property value associated with the specified property key.</summary>
#pragma warning disable CS8767
        public bool TryGetValue(string key, [NotNullWhen(true)] out object? value) => properties.TryGetValue(key, out value);
#pragma warning restore CS8767

        #endregion

        // Allow derived classes to override
        // http://blogs.msdn.com/b/jaredpar/archive/2011/03/18/debuggerdisplay-attribute-best-practices.aspx
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        protected virtual string DebuggerDisplay
            => $"{GetType().Name} for {ResourceTypeName} | Origin: {Origin}"
             + (IsFaulted ? " | Error: " + Error?.Message : string.Empty);
    }
}
#nullable restore