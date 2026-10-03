// .../RequestEmailSignInCodeCommandValidator.cs
namespace DoubleStar.Modules.Identity.Application.Commands.RequestEmailSignInCodeCommand;

public sealed class RequestEmailSignInCodeCommandValidator : AbstractValidator<RequestEmailSignInCodeCommand>
{
    public RequestEmailSignInCodeCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
    }
}