// Common/Primitives/Error.cs
namespace DoubleStar.SharedKernel.Common.Primitives;

public sealed record Error(string Code, string Message, ErrorType Type);