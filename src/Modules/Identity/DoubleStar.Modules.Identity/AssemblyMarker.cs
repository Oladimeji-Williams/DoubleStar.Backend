// AssemblyMarker.cs
namespace DoubleStar.Modules.Identity;

/// <summary>
/// Non-functional marker used to obtain this module's assembly for MediatR,
/// FluentValidation and ApplicationPart registration, without any one
/// internal type having to stay public just to be a reflection target.
/// </summary>
public static class AssemblyMarker;