using AutoMapper;
using Business.Communication;
using Business.Data;
using Business.Domain.Models;
using Business.Domain.Services;
using Business.Resources;
using Business.Resources.WorkHistory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Serilog;

namespace API.Controllers;

[Route("api/v1/work-history")]
public sealed class WorkHistoryController
    : DongNguyenController<WorkHistoryResource, CreateWorkHistoryResource, UpdateWorkHistoryResource, WorkHistory>
{
    private const string EditorRoles =
        $"{Role.Admin}, {Role.EditorQTNS}, {Role.EditorQTDA}";

    public WorkHistoryController(
        IWorkHistoryService workHistoryService,
        IMapper mapper,
        IOptionsMonitor<ResponseMessage> responseMessage)
        : base(workHistoryService, mapper, responseMessage)
    {
    }

    [HttpPost]
    [Authorize(Roles = EditorRoles)]
    [ProducesResponseType(typeof(BaseResponse<WorkHistoryResource>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(BaseResponse<WorkHistoryResource>), StatusCodes.Status400BadRequest)]
    public new Task<IActionResult> CreateAsync([FromBody] CreateWorkHistoryResource resource)
    {
        Log.Information("{User} created a work-history.", User.Identity?.Name);

        return base.CreateAsync(resource);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = EditorRoles)]
    [ProducesResponseType(typeof(BaseResponse<WorkHistoryResource>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<WorkHistoryResource>), StatusCodes.Status400BadRequest)]
    public new Task<IActionResult> UpdateAsync(
        int id,
        [FromBody] UpdateWorkHistoryResource resource)
    {
        Log.Information(
            "{User} updated work-history with Id {Id}.",
            User.Identity?.Name,
            id);

        return base.UpdateAsync(id, resource);
    }

    [HttpPut]
    [Authorize(Roles = EditorRoles)]
    [ProducesResponseType(typeof(BaseResponse<WorkHistoryResource>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<WorkHistoryResource>), StatusCodes.Status400BadRequest)]
    public new Task<IActionResult> ChangeOrderIndexAsync([FromBody] List<int> ids)
    {
        Log.Information(
            "{User} changed work-history order indexes.",
            User.Identity?.Name);

        return base.ChangeOrderIndexAsync(ids);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = EditorRoles)]
    [ProducesResponseType(typeof(BaseResponse<WorkHistoryResource>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<WorkHistoryResource>), StatusCodes.Status400BadRequest)]
    public new Task<IActionResult> DeleteAsync(int id)
    {
        Log.Information(
            "{User} deleted work-history with Id {Id}.",
            User.Identity?.Name,
            id);

        return base.DeleteAsync(id);
    }
}
