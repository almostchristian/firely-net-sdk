// <auto-generated/>
// Contents of: hl7.fhir.r5.core version: 5.0.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Hl7.Fhir.Introspection;
using Hl7.Fhir.Serialization;
using Hl7.Fhir.Specification;
using Hl7.Fhir.Utility;
using Hl7.Fhir.Validation;

/*
  Copyright (c) 2011+, HL7, Inc.
  All rights reserved.
  
  Redistribution and use in source and binary forms, with or without modification, 
  are permitted provided that the following conditions are met:
  
   * Redistributions of source code must retain the above copyright notice, this 
     list of conditions and the following disclaimer.
   * Redistributions in binary form must reproduce the above copyright notice, 
     this list of conditions and the following disclaimer in the documentation 
     and/or other materials provided with the distribution.
   * Neither the name of HL7 nor the names of its contributors may be used to 
     endorse or promote products derived from this software without specific 
     prior written permission.
  
  THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND 
  ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED 
  WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. 
  IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, 
  INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT 
  NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR 
  PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, 
  WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) 
  ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE 
  POSSIBILITY OF SUCH DAMAGE.
  
*/

namespace Hl7.Fhir.Model
{
  /// <summary>
  /// A grouping of people or organizations with a common purpose
  /// </summary>
  [Serializable]
  [DataContract]
  [FhirType("Organization","http://hl7.org/fhir/StructureDefinition/Organization", IsResource=true)]
  public partial class Organization : Hl7.Fhir.Model.DomainResource, IIdentifiable<List<Identifier>>
  {
    /// <summary>
    /// FHIR Type Name
    /// </summary>
    public override string TypeName { get { return "Organization"; } }

    /// <summary>
    /// Qualifications, certifications, accreditations, licenses, training, etc. pertaining to the provision of care
    /// </summary>
    [Serializable]
    [DataContract]
    [FhirType("Organization#Qualification", IsNestedType=true)]
    [BackboneType("Organization.qualification")]
    public partial class QualificationComponent : Hl7.Fhir.Model.BackboneElement
    {
      /// <summary>
      /// FHIR Type Name
      /// </summary>
      public override string TypeName { get { return "Organization#Qualification"; } }

      /// <summary>
      /// An identifier for this qualification for the organization
      /// </summary>
      [FhirElement("identifier", Order=40)]
      [Cardinality(Min=0,Max=-1)]
      [DataMember]
      public List<Hl7.Fhir.Model.Identifier> Identifier
      {
        get { if(_Identifier==null) _Identifier = new List<Hl7.Fhir.Model.Identifier>(); return _Identifier; }
        set { _Identifier = value; OnPropertyChanged("Identifier"); }
      }

      private List<Hl7.Fhir.Model.Identifier> _Identifier;

      /// <summary>
      /// Coded representation of the qualification
      /// </summary>
      [FhirElement("code", Order=50)]
      [Binding("Qualification")]
      [Cardinality(Min=1,Max=1)]
      [DataMember]
      public Hl7.Fhir.Model.CodeableConcept Code
      {
        get { return _Code; }
        set { _Code = value; OnPropertyChanged("Code"); }
      }

      private Hl7.Fhir.Model.CodeableConcept _Code;

      /// <summary>
      /// Period during which the qualification is valid
      /// </summary>
      [FhirElement("period", Order=60)]
      [DataMember]
      public Hl7.Fhir.Model.Period Period
      {
        get { return _Period; }
        set { _Period = value; OnPropertyChanged("Period"); }
      }

      private Hl7.Fhir.Model.Period _Period;

      /// <summary>
      /// Organization that regulates and issues the qualification
      /// </summary>
      [FhirElement("issuer", Order=70)]
      [CLSCompliant(false)]
      [References("Organization")]
      [DataMember]
      public Hl7.Fhir.Model.ResourceReference Issuer
      {
        get { return _Issuer; }
        set { _Issuer = value; OnPropertyChanged("Issuer"); }
      }

      private Hl7.Fhir.Model.ResourceReference _Issuer;

      public override IDeepCopyable CopyTo(IDeepCopyable other)
      {
        var dest = other as QualificationComponent;

        if (dest == null)
        {
          throw new ArgumentException("Can only copy to an object of the same type", "other");
        }

        base.CopyTo(dest);
        if(Identifier != null) dest.Identifier = new List<Hl7.Fhir.Model.Identifier>(Identifier.DeepCopy());
        if(Code != null) dest.Code = (Hl7.Fhir.Model.CodeableConcept)Code.DeepCopy();
        if(Period != null) dest.Period = (Hl7.Fhir.Model.Period)Period.DeepCopy();
        if(Issuer != null) dest.Issuer = (Hl7.Fhir.Model.ResourceReference)Issuer.DeepCopy();
        return dest;
      }

      public override IDeepCopyable DeepCopy()
      {
        return CopyTo(new QualificationComponent());
      }

      ///<inheritdoc />
      public override bool Matches(IDeepComparable other)
      {
        var otherT = other as QualificationComponent;
        if(otherT == null) return false;

        if(!base.Matches(otherT)) return false;
        if( !DeepComparable.Matches(Identifier, otherT.Identifier)) return false;
        if( !DeepComparable.Matches(Code, otherT.Code)) return false;
        if( !DeepComparable.Matches(Period, otherT.Period)) return false;
        if( !DeepComparable.Matches(Issuer, otherT.Issuer)) return false;

        return true;
      }

      public override bool IsExactly(IDeepComparable other)
      {
        var otherT = other as QualificationComponent;
        if(otherT == null) return false;

        if(!base.IsExactly(otherT)) return false;
        if( !DeepComparable.IsExactly(Identifier, otherT.Identifier)) return false;
        if( !DeepComparable.IsExactly(Code, otherT.Code)) return false;
        if( !DeepComparable.IsExactly(Period, otherT.Period)) return false;
        if( !DeepComparable.IsExactly(Issuer, otherT.Issuer)) return false;

        return true;
      }

      [IgnoreDataMember]
      public override IEnumerable<Base> Children
      {
        get
        {
          foreach (var item in base.Children) yield return item;
          foreach (var elem in Identifier) { if (elem != null) yield return elem; }
          if (Code != null) yield return Code;
          if (Period != null) yield return Period;
          if (Issuer != null) yield return Issuer;
        }
      }

      [IgnoreDataMember]
      public override IEnumerable<ElementValue> NamedChildren
      {
        get
        {
          foreach (var item in base.NamedChildren) yield return item;
          foreach (var elem in Identifier) { if (elem != null) yield return new ElementValue("identifier", elem); }
          if (Code != null) yield return new ElementValue("code", Code);
          if (Period != null) yield return new ElementValue("period", Period);
          if (Issuer != null) yield return new ElementValue("issuer", Issuer);
        }
      }

      protected override bool TryGetValue(string key, out object value)
      {
        switch (key)
        {
          case "identifier":
            value = Identifier;
            return Identifier?.Any() == true;
          case "code":
            value = Code;
            return Code is not null;
          case "period":
            value = Period;
            return Period is not null;
          case "issuer":
            value = Issuer;
            return Issuer is not null;
          default:
            return base.TryGetValue(key, out value);
        }

      }

      protected override IEnumerable<KeyValuePair<string, object>> GetElementPairs()
      {
        foreach (var kvp in base.GetElementPairs()) yield return kvp;
        if (Identifier?.Any() == true) yield return new KeyValuePair<string,object>("identifier",Identifier);
        if (Code is not null) yield return new KeyValuePair<string,object>("code",Code);
        if (Period is not null) yield return new KeyValuePair<string,object>("period",Period);
        if (Issuer is not null) yield return new KeyValuePair<string,object>("issuer",Issuer);
      }

    }

    /// <summary>
    /// Identifies this organization  across multiple systems
    /// </summary>
    [FhirElement("identifier", InSummary=true, Order=90, FiveWs="FiveWs.identifier")]
    [Cardinality(Min=0,Max=-1)]
    [DataMember]
    public List<Hl7.Fhir.Model.Identifier> Identifier
    {
      get { if(_Identifier==null) _Identifier = new List<Hl7.Fhir.Model.Identifier>(); return _Identifier; }
      set { _Identifier = value; OnPropertyChanged("Identifier"); }
    }

    private List<Hl7.Fhir.Model.Identifier> _Identifier;

    /// <summary>
    /// Whether the organization's record is still in active use
    /// </summary>
    [FhirElement("active", InSummary=true, IsModifier=true, Order=100, FiveWs="FiveWs.status")]
    [DataMember]
    public Hl7.Fhir.Model.FhirBoolean ActiveElement
    {
      get { return _ActiveElement; }
      set { _ActiveElement = value; OnPropertyChanged("ActiveElement"); }
    }

    private Hl7.Fhir.Model.FhirBoolean _ActiveElement;

    /// <summary>
    /// Whether the organization's record is still in active use
    /// </summary>
    /// <remarks>This uses the native .NET datatype, rather than the FHIR equivalent</remarks>
    [IgnoreDataMember]
    public bool? Active
    {
      get { return ActiveElement != null ? ActiveElement.Value : null; }
      set
      {
        if (value == null)
          ActiveElement = null;
        else
          ActiveElement = new Hl7.Fhir.Model.FhirBoolean(value);
        OnPropertyChanged("Active");
      }
    }

    /// <summary>
    /// Kind of organization
    /// </summary>
    [FhirElement("type", InSummary=true, Order=110, FiveWs="FiveWs.class")]
    [Binding("OrganizationType")]
    [Cardinality(Min=0,Max=-1)]
    [DataMember]
    public List<Hl7.Fhir.Model.CodeableConcept> Type
    {
      get { if(_Type==null) _Type = new List<Hl7.Fhir.Model.CodeableConcept>(); return _Type; }
      set { _Type = value; OnPropertyChanged("Type"); }
    }

    private List<Hl7.Fhir.Model.CodeableConcept> _Type;

    /// <summary>
    /// Name used for the organization
    /// </summary>
    [FhirElement("name", InSummary=true, Order=120)]
    [DataMember]
    public Hl7.Fhir.Model.FhirString NameElement
    {
      get { return _NameElement; }
      set { _NameElement = value; OnPropertyChanged("NameElement"); }
    }

    private Hl7.Fhir.Model.FhirString _NameElement;

    /// <summary>
    /// Name used for the organization
    /// </summary>
    /// <remarks>This uses the native .NET datatype, rather than the FHIR equivalent</remarks>
    [IgnoreDataMember]
    public string Name
    {
      get { return NameElement != null ? NameElement.Value : null; }
      set
      {
        if (value == null)
          NameElement = null;
        else
          NameElement = new Hl7.Fhir.Model.FhirString(value);
        OnPropertyChanged("Name");
      }
    }

    /// <summary>
    /// A list of alternate names that the organization is known as, or was known as in the past
    /// </summary>
    [FhirElement("alias", Order=130)]
    [Cardinality(Min=0,Max=-1)]
    [DataMember]
    public List<Hl7.Fhir.Model.FhirString> AliasElement
    {
      get { if(_AliasElement==null) _AliasElement = new List<Hl7.Fhir.Model.FhirString>(); return _AliasElement; }
      set { _AliasElement = value; OnPropertyChanged("AliasElement"); }
    }

    private List<Hl7.Fhir.Model.FhirString> _AliasElement;

    /// <summary>
    /// A list of alternate names that the organization is known as, or was known as in the past
    /// </summary>
    /// <remarks>This uses the native .NET datatype, rather than the FHIR equivalent</remarks>
    [IgnoreDataMember]
    public IEnumerable<string> Alias
    {
      get { return AliasElement != null ? AliasElement.Select(elem => elem.Value) : null; }
      set
      {
        if (value == null)
          AliasElement = null;
        else
          AliasElement = new List<Hl7.Fhir.Model.FhirString>(value.Select(elem=>new Hl7.Fhir.Model.FhirString(elem)));
        OnPropertyChanged("Alias");
      }
    }

    /// <summary>
    /// Additional details about the Organization that could be displayed as further information to identify the Organization beyond its name
    /// </summary>
    [FhirElement("description", InSummary=true, Order=140)]
    [DataMember]
    public Hl7.Fhir.Model.Markdown DescriptionElement
    {
      get { return _DescriptionElement; }
      set { _DescriptionElement = value; OnPropertyChanged("DescriptionElement"); }
    }

    private Hl7.Fhir.Model.Markdown _DescriptionElement;

    /// <summary>
    /// Additional details about the Organization that could be displayed as further information to identify the Organization beyond its name
    /// </summary>
    /// <remarks>This uses the native .NET datatype, rather than the FHIR equivalent</remarks>
    [IgnoreDataMember]
    public string Description
    {
      get { return DescriptionElement != null ? DescriptionElement.Value : null; }
      set
      {
        if (value == null)
          DescriptionElement = null;
        else
          DescriptionElement = new Hl7.Fhir.Model.Markdown(value);
        OnPropertyChanged("Description");
      }
    }

    /// <summary>
    /// Official contact details for the Organization
    /// </summary>
    [FhirElement("contact", Order=150)]
    [Cardinality(Min=0,Max=-1)]
    [DataMember]
    public List<Hl7.Fhir.Model.ExtendedContactDetail> Contact
    {
      get { if(_Contact==null) _Contact = new List<Hl7.Fhir.Model.ExtendedContactDetail>(); return _Contact; }
      set { _Contact = value; OnPropertyChanged("Contact"); }
    }

    private List<Hl7.Fhir.Model.ExtendedContactDetail> _Contact;

    /// <summary>
    /// The organization of which this organization forms a part
    /// </summary>
    [FhirElement("partOf", InSummary=true, Order=160)]
    [CLSCompliant(false)]
    [References("Organization")]
    [DataMember]
    public Hl7.Fhir.Model.ResourceReference PartOf
    {
      get { return _PartOf; }
      set { _PartOf = value; OnPropertyChanged("PartOf"); }
    }

    private Hl7.Fhir.Model.ResourceReference _PartOf;

    /// <summary>
    /// Technical endpoints providing access to services operated for the organization
    /// </summary>
    [FhirElement("endpoint", Order=170)]
    [CLSCompliant(false)]
    [References("Endpoint")]
    [Cardinality(Min=0,Max=-1)]
    [DataMember]
    public List<Hl7.Fhir.Model.ResourceReference> Endpoint
    {
      get { if(_Endpoint==null) _Endpoint = new List<Hl7.Fhir.Model.ResourceReference>(); return _Endpoint; }
      set { _Endpoint = value; OnPropertyChanged("Endpoint"); }
    }

    private List<Hl7.Fhir.Model.ResourceReference> _Endpoint;

    /// <summary>
    /// Qualifications, certifications, accreditations, licenses, training, etc. pertaining to the provision of care
    /// </summary>
    [FhirElement("qualification", Order=180)]
    [Cardinality(Min=0,Max=-1)]
    [DataMember]
    public List<Hl7.Fhir.Model.Organization.QualificationComponent> Qualification
    {
      get { if(_Qualification==null) _Qualification = new List<Hl7.Fhir.Model.Organization.QualificationComponent>(); return _Qualification; }
      set { _Qualification = value; OnPropertyChanged("Qualification"); }
    }

    private List<Hl7.Fhir.Model.Organization.QualificationComponent> _Qualification;

    List<Identifier> IIdentifiable<List<Identifier>>.Identifier { get => Identifier; set => Identifier = value; }

    public override IDeepCopyable CopyTo(IDeepCopyable other)
    {
      var dest = other as Organization;

      if (dest == null)
      {
        throw new ArgumentException("Can only copy to an object of the same type", "other");
      }

      base.CopyTo(dest);
      if(Identifier != null) dest.Identifier = new List<Hl7.Fhir.Model.Identifier>(Identifier.DeepCopy());
      if(ActiveElement != null) dest.ActiveElement = (Hl7.Fhir.Model.FhirBoolean)ActiveElement.DeepCopy();
      if(Type != null) dest.Type = new List<Hl7.Fhir.Model.CodeableConcept>(Type.DeepCopy());
      if(NameElement != null) dest.NameElement = (Hl7.Fhir.Model.FhirString)NameElement.DeepCopy();
      if(AliasElement != null) dest.AliasElement = new List<Hl7.Fhir.Model.FhirString>(AliasElement.DeepCopy());
      if(DescriptionElement != null) dest.DescriptionElement = (Hl7.Fhir.Model.Markdown)DescriptionElement.DeepCopy();
      if(Contact != null) dest.Contact = new List<Hl7.Fhir.Model.ExtendedContactDetail>(Contact.DeepCopy());
      if(PartOf != null) dest.PartOf = (Hl7.Fhir.Model.ResourceReference)PartOf.DeepCopy();
      if(Endpoint != null) dest.Endpoint = new List<Hl7.Fhir.Model.ResourceReference>(Endpoint.DeepCopy());
      if(Qualification != null) dest.Qualification = new List<Hl7.Fhir.Model.Organization.QualificationComponent>(Qualification.DeepCopy());
      return dest;
    }

    public override IDeepCopyable DeepCopy()
    {
      return CopyTo(new Organization());
    }

    ///<inheritdoc />
    public override bool Matches(IDeepComparable other)
    {
      var otherT = other as Organization;
      if(otherT == null) return false;

      if(!base.Matches(otherT)) return false;
      if( !DeepComparable.Matches(Identifier, otherT.Identifier)) return false;
      if( !DeepComparable.Matches(ActiveElement, otherT.ActiveElement)) return false;
      if( !DeepComparable.Matches(Type, otherT.Type)) return false;
      if( !DeepComparable.Matches(NameElement, otherT.NameElement)) return false;
      if( !DeepComparable.Matches(AliasElement, otherT.AliasElement)) return false;
      if( !DeepComparable.Matches(DescriptionElement, otherT.DescriptionElement)) return false;
      if( !DeepComparable.Matches(Contact, otherT.Contact)) return false;
      if( !DeepComparable.Matches(PartOf, otherT.PartOf)) return false;
      if( !DeepComparable.Matches(Endpoint, otherT.Endpoint)) return false;
      if( !DeepComparable.Matches(Qualification, otherT.Qualification)) return false;

      return true;
    }

    public override bool IsExactly(IDeepComparable other)
    {
      var otherT = other as Organization;
      if(otherT == null) return false;

      if(!base.IsExactly(otherT)) return false;
      if( !DeepComparable.IsExactly(Identifier, otherT.Identifier)) return false;
      if( !DeepComparable.IsExactly(ActiveElement, otherT.ActiveElement)) return false;
      if( !DeepComparable.IsExactly(Type, otherT.Type)) return false;
      if( !DeepComparable.IsExactly(NameElement, otherT.NameElement)) return false;
      if( !DeepComparable.IsExactly(AliasElement, otherT.AliasElement)) return false;
      if( !DeepComparable.IsExactly(DescriptionElement, otherT.DescriptionElement)) return false;
      if( !DeepComparable.IsExactly(Contact, otherT.Contact)) return false;
      if( !DeepComparable.IsExactly(PartOf, otherT.PartOf)) return false;
      if( !DeepComparable.IsExactly(Endpoint, otherT.Endpoint)) return false;
      if( !DeepComparable.IsExactly(Qualification, otherT.Qualification)) return false;

      return true;
    }

    [IgnoreDataMember]
    public override IEnumerable<Base> Children
    {
      get
      {
        foreach (var item in base.Children) yield return item;
        foreach (var elem in Identifier) { if (elem != null) yield return elem; }
        if (ActiveElement != null) yield return ActiveElement;
        foreach (var elem in Type) { if (elem != null) yield return elem; }
        if (NameElement != null) yield return NameElement;
        foreach (var elem in AliasElement) { if (elem != null) yield return elem; }
        if (DescriptionElement != null) yield return DescriptionElement;
        foreach (var elem in Contact) { if (elem != null) yield return elem; }
        if (PartOf != null) yield return PartOf;
        foreach (var elem in Endpoint) { if (elem != null) yield return elem; }
        foreach (var elem in Qualification) { if (elem != null) yield return elem; }
      }
    }

    [IgnoreDataMember]
    public override IEnumerable<ElementValue> NamedChildren
    {
      get
      {
        foreach (var item in base.NamedChildren) yield return item;
        foreach (var elem in Identifier) { if (elem != null) yield return new ElementValue("identifier", elem); }
        if (ActiveElement != null) yield return new ElementValue("active", ActiveElement);
        foreach (var elem in Type) { if (elem != null) yield return new ElementValue("type", elem); }
        if (NameElement != null) yield return new ElementValue("name", NameElement);
        foreach (var elem in AliasElement) { if (elem != null) yield return new ElementValue("alias", elem); }
        if (DescriptionElement != null) yield return new ElementValue("description", DescriptionElement);
        foreach (var elem in Contact) { if (elem != null) yield return new ElementValue("contact", elem); }
        if (PartOf != null) yield return new ElementValue("partOf", PartOf);
        foreach (var elem in Endpoint) { if (elem != null) yield return new ElementValue("endpoint", elem); }
        foreach (var elem in Qualification) { if (elem != null) yield return new ElementValue("qualification", elem); }
      }
    }

    protected override bool TryGetValue(string key, out object value)
    {
      switch (key)
      {
        case "identifier":
          value = Identifier;
          return Identifier?.Any() == true;
        case "active":
          value = ActiveElement;
          return ActiveElement is not null;
        case "type":
          value = Type;
          return Type?.Any() == true;
        case "name":
          value = NameElement;
          return NameElement is not null;
        case "alias":
          value = AliasElement;
          return AliasElement?.Any() == true;
        case "description":
          value = DescriptionElement;
          return DescriptionElement is not null;
        case "contact":
          value = Contact;
          return Contact?.Any() == true;
        case "partOf":
          value = PartOf;
          return PartOf is not null;
        case "endpoint":
          value = Endpoint;
          return Endpoint?.Any() == true;
        case "qualification":
          value = Qualification;
          return Qualification?.Any() == true;
        default:
          return base.TryGetValue(key, out value);
      }

    }

    protected override IEnumerable<KeyValuePair<string, object>> GetElementPairs()
    {
      foreach (var kvp in base.GetElementPairs()) yield return kvp;
      if (Identifier?.Any() == true) yield return new KeyValuePair<string,object>("identifier",Identifier);
      if (ActiveElement is not null) yield return new KeyValuePair<string,object>("active",ActiveElement);
      if (Type?.Any() == true) yield return new KeyValuePair<string,object>("type",Type);
      if (NameElement is not null) yield return new KeyValuePair<string,object>("name",NameElement);
      if (AliasElement?.Any() == true) yield return new KeyValuePair<string,object>("alias",AliasElement);
      if (DescriptionElement is not null) yield return new KeyValuePair<string,object>("description",DescriptionElement);
      if (Contact?.Any() == true) yield return new KeyValuePair<string,object>("contact",Contact);
      if (PartOf is not null) yield return new KeyValuePair<string,object>("partOf",PartOf);
      if (Endpoint?.Any() == true) yield return new KeyValuePair<string,object>("endpoint",Endpoint);
      if (Qualification?.Any() == true) yield return new KeyValuePair<string,object>("qualification",Qualification);
    }

  }

}

// end of file
