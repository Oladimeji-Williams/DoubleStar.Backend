// Common/Primitives/ErrorType.cs
namespace DoubleStar.SharedKernel.Common.Primitives;

public enum ErrorType
{
    Validation,
    NotFound,
    Conflict,
    Unauthorized,
    Forbidden,
    Failure
}