// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#pragma warning disable IDE0130 // Namespace does not match folder structure

namespace System.Diagnostics.CodeAnalysis;

/// <summary>
/// Specifies that null is allowed as an input even if the corresponding type disallows it.
/// </summary>
[AttributeUsage( AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Property, Inherited = false )]
internal sealed class AllowNullAttribute : Attribute { }

/// <summary>
/// Specifies that null is disallowed as an input even if the corresponding type allows it.
/// </summary>
[AttributeUsage( AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Property, Inherited = false )]
internal sealed class DisallowNullAttribute : Attribute { }

/// <summary>
/// Specifies that an output may be null even if the corresponding type disallows it.
/// </summary>
[AttributeUsage( AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.ReturnValue, Inherited = false )]
internal sealed class MaybeNullAttribute : Attribute { }

/// <summary>
/// Specifies that an output will not be null even if the corresponding type allows it. Specifies
/// that an input argument was not null when the call returns.
/// </summary>
[AttributeUsage( AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.ReturnValue, Inherited = false )]
internal sealed class NotNullAttribute : Attribute { }

/// <summary>
/// Specifies that when a method returns <see cref="ReturnValue"/>, the parameter may be null
/// even if the corresponding type disallows it.
/// </summary>
/// <remarks>   Initializes the attribute with the specified return value condition. </remarks>
/// <param name="returnValue">  The return value condition. If the method returns this value, the
///                             associated parameter may be null. </param>
[AttributeUsage( AttributeTargets.Parameter, Inherited = false )]
internal sealed class MaybeNullWhenAttribute( bool returnValue ) : Attribute
{

    /// <summary>   Gets the return value condition. </summary>
    /// <value> True if return value, false if not. </value>
    public bool ReturnValue { get; } = returnValue;
}

/// <summary>
/// Specifies that when a method returns <see cref="ReturnValue"/>, the parameter will not be
/// null even if the corresponding type allows it.
/// </summary>
/// <remarks>   Initializes the attribute with the specified return value condition. </remarks>
/// <param name="returnValue">  The return value condition. If the method returns this value, the
///                             associated parameter will not be null. </param>
[AttributeUsage( AttributeTargets.Parameter, Inherited = false )]
internal sealed class NotNullWhenAttribute( bool returnValue ) : Attribute
{

    /// <summary>   Gets the return value condition. </summary>
    /// <value> True if return value, false if not. </value>
    public bool ReturnValue { get; } = returnValue;
}

/// <summary>
/// Specifies that the output will be non-null if the named parameter is non-null.
/// </summary>
/// <remarks>   Initializes the attribute with the associated parameter name. </remarks>
/// <param name="parameterName">    The associated parameter name.  The output will be non-null
///                                 if the argument to the parameter specified is non-null. </param>
[AttributeUsage( AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.ReturnValue, AllowMultiple = true, Inherited = false )]
internal sealed class NotNullIfNotNullAttribute( string parameterName ) : Attribute
{

    /// <summary>   Gets the associated parameter name. </summary>
    /// <value> The name of the parameter. </value>
    public string ParameterName { get; } = parameterName;
}

/// <summary>
/// Specifies that the method will not return if the associated Boolean parameter is passed the
/// specified value.
/// </summary>
/// <remarks>   Initializes the attribute with the specified parameter value. </remarks>
/// <param name="parameterValue">   The condition parameter value. Code after the method will be
///                                 considered unreachable by diagnostics if the argument to the
///                                 associated parameter matches this value. </param>
[AttributeUsage( AttributeTargets.Parameter, Inherited = false )]
internal sealed class DoesNotReturnIfAttribute( bool parameterValue ) : Attribute
{

    /// <summary>   Gets the condition parameter value. </summary>
    /// <value> True if parameter value, false if not. </value>
    public bool ParameterValue { get; } = parameterValue;
}

