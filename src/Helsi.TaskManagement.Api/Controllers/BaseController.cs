using Helsi.TaskManagement.Api.Infrastructure.Attributes;
using Helsi.TaskManagement.Api.Infrastructure.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace Helsi.TaskManagement.Api.Controllers;

[RequireUserIdHeader]
public abstract class BaseController : ControllerBase
{
    protected string? CurrentUserId => HttpContext.GetUserId();
}