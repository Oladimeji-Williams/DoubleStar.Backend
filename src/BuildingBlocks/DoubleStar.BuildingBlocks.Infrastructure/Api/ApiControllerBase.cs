// Api/ApiControllerBase.cs
using Microsoft.AspNetCore.Mvc;

namespace DoubleStar.BuildingBlocks.Infrastructure.Api;

[ApiController]
[Produces("application/json")]
public abstract class ApiControllerBase : ControllerBase;