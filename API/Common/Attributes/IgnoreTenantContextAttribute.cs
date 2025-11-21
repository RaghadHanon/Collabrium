namespace API.Common.Attributes;

/// <summary>
/// Specifies that an endpoint does not require a tenant ID and should be
/// excluded from the tenant check middleware.
/// </summary>
[AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
public class IgnoreTenantContextAttribute : Attribute
{
}