using Microsoft.AspNetCore.Mvc;

namespace ForraControl.API.Controllers;

[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    protected IActionResult Ok<T>(T data) => base.Ok(new { ok = true, data });

    protected IActionResult Created<T>(T data) => StatusCode(StatusCodes.Status201Created, new { ok = true, data });

    protected IActionResult Fail(string msg, int statusCode = StatusCodes.Status400BadRequest)
        => StatusCode(statusCode, new { ok = false, error = msg });
}
