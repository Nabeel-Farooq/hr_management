using Business.Communication;
using Business.Data;
using Business.Domain.Services;
using Business.Resources;
using Business.Resources.Timesheet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Serilog;

namespace API.Controllers;

[ApiController]
[Route("api/v1/timesheet")]
public sealed class TimesheetController : ControllerBase
{
    private const string AuthorizedRoles =
        $"{Role.Admin}, {Role.EditorQTNS}, {Role.EditorKT}";

    private readonly ITimesheetService _timesheetService;
    protected readonly ResponseMessage ResponseMessage;

    public TimesheetController(
        ITimesheetService timesheetService,
        IOptionsMonitor<ResponseMessage> responseMessage)
    {
        _timesheetService = timesheetService;
        ResponseMessage = responseMessage.CurrentValue;
    }

    [HttpPost("import")]
    //[Authorize(Roles = $"{Role.Admin}, {Role.EditorQTNS}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ImportAsync(IFormFile file)
    {
        var validation = ValidateTimesheet(file);

        if (!validation.IsSuccess)
            return BadRequest(validation.Result);

        var filePath = Path.GetTempFileName();

        try
        {
            await using (var stream = new FileStream(
                filePath,
                FileMode.Create,
                FileAccess.ReadWrite,
                FileShare.None,
                81920,
                useAsync: true))
            {
                await file.CopyToAsync(stream);
                stream.Position = 0;

                var result = await _timesheetService.ImportAsync(stream);

                return result.Success
                    ? Ok(result)
                    : BadRequest(result);
            }
        }
        finally
        {
            if (System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath);
        }
    }

    [HttpGet]
    [Authorize(Roles = AuthorizedRoles)]
    [ProducesResponseType(typeof(BaseResponse<TimesheetResource>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<TimesheetResource>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetTimesheetByPersonIdAsync(
        int personId,
        DateTime date)
    {
        Log.Information(
            "{User} requested timesheet for PersonId {PersonId}",
            User.Identity?.Name,
            personId);

        var result = await _timesheetService.GetTimesheetByPersonIdAsync(
            personId,
            date);

        return result.Success
            ? Ok(result)
            : BadRequest(result);
    }

    private (bool IsSuccess, BaseResponse<object> Result) ValidateTimesheet(
        IFormFile file)
    {
        if (file is null || file.Length == 0)
            return (
                false,
                new BaseResponse<object>(
                    ResponseMessage.Values["File_Empty"]));

        if (!string.Equals(
                Path.GetExtension(file.FileName),
                ".xlsx",
                StringComparison.OrdinalIgnoreCase))
        {
            return (
                false,
                new BaseResponse<object>(
                    ResponseMessage.Values["Not_Support_File"]));
        }

        return (true, new BaseResponse<object>(true));
    }
}
