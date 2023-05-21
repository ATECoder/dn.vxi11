// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#pragma warning disable IDE0130 // Namespace does not match folder structure

namespace System.Diagnostics.CodeAnalysis;

/// <summary>Specifies that the method or property will ensure that the listed field and property members have not-null values.</summary>
/// <remarks> 
/// <see href="https://GitHub.com/dotnet/runtime/blob/main/src/libraries/System.Private.CoreLib/src/System/Diagnostics/CodeAnalysis/NullableAttributes.cs"/>
/// </remarks>
[AttributeUsage( AttributeTargets.Method | AttributeTargets.Property, Inherited = false, AllowMultiple = true )]
internal sealed class MemberNotNullAttribute : Attribute
{
    /// <summary>Initializes the attribute with a field or property member.</summary>
    /// <param name="member">
    /// The field or property member that is promised to be not-null.
    /// </param>
    public MemberNotNullAttribute( string member ) => this.Members = new[] { member };

    /// <summary>Initializes the attribute with the list of field and property members.</summary>
    /// <param name="members">
    /// The list of field and property members that are promised to be not-null.
    /// </param>
    public MemberNotNullAttribute( params string[] members ) => this.Members = members;

    /// <summary>Gets field or property member names.</summary>
    public string[] Members { get; }
}

/// <summary>Specifies that the method or property will ensure that the listed field and property members have not-null values when returning with the specified return value condition.</summary>
[AttributeUsage( AttributeTargets.Method | AttributeTargets.Property, Inherited = false, AllowMultiple = true )]
internal sealed class MemberNotNullWhenAttribute : Attribute
{
    /// <summary>Initializes the attribute with the specified return value condition and a field or property member.</summary>
    /// <param name="returnValue">
    /// The return value condition. If the method returns this value, the associated parameter will not be null.
    /// </param>
    /// <param name="member">
    /// The field or property member that is promised to be not-null.
    /// </param>
    public MemberNotNullWhenAttribute( bool returnValue, string member )
    {
        this.ReturnValue = returnValue;
        this.Members = new[] { member };
    }

    /// <summary>Initializes the attribute with the specified return value condition and list of field and property members.</summary>
    /// <param name="returnValue">
    /// The return value condition. If the method returns this value, the associated parameter will not be null.
    /// </param>
    /// <param name="members">
    /// The list of field and property members that are promised to be not-null.
    /// </param>
    public MemberNotNullWhenAttribute( bool returnValue, params string[] members )
    {
        this.ReturnValue = returnValue;
        this.Members = members;
    }

    /// <summary>Gets the return value condition.</summary>
    public bool ReturnValue { get; }

    /// <summary>Gets field or property member names.</summary>
    public string[] Members { get; }
}


