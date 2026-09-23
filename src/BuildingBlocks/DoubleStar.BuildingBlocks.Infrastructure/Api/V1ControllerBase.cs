// Api/V1ControllerBase.cs
using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DoubleStar.SharedKernel.Common.Primitives;
using DoubleStar.BuildingBlocks.Infrastructure.Api.Contracts;

namespace DoubleStar.BuildingBlocks.Infrastructure.Api;

[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/[controller]")]
public abstract class V1ControllerBase : ApiControllerBase
{
    protected IActionResult Success<T>(T data) => Ok(new ApiResponse<T>(true, data, null));

    protected IActionResult Created<T>(string location, T data) =>
        new CreatedResult(location, new ApiResponse<T>(true, data, null));

    protected IActionResult Failure<T>(Result<T> result) => Failure(result.Errors);
    protected IActionResult Failure(Result result) => Failure(result.Errors);

    private IActionResult Failure(IReadOnlyList<Error> errors)
    {
        var response = new ApiResponse<object>(
            false, null, errors.Select(e => new ApiError(e.Code, e.Message, e.Type.ToString())).ToList());

        var type = errors.Select(e => e.Type).FirstOrDefault();

        return type switch
        {
            ErrorType.Validation => BadRequest(response),
            ErrorType.NotFound => NotFound(response),
            ErrorType.Conflict => Conflict(response),
            ErrorType.Unauthorized => Unauthorized(response),
            ErrorType.Forbidden => new ObjectResult(response) { StatusCode = StatusCodes.Status403Forbidden },
            _ => new ObjectResult(response) { StatusCode = StatusCodes.Status500InternalServerError },
        };
    }
}