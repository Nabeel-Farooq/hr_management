using AutoMapper;
using Business.Communication;
using Business.Data;
using Business.Domain.Models;
using Business.Domain.Services;
using Business.Resources;
using Business.Resources.Position;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Serilog;

namespace API.Controllers;

[Route("api/v1/position")]
public sealed class PositionController
    : DongNguyenController<
        PositionResource,
        CreatePositionResource,
        UpdatePositionResource,
        Position>
{
    private const string EditorRoles =
        $"{Role.Admin}, {Role.EditorQTNS}, {Role.EditorQTDA}";

    public PositionController(
        IPositionService positionService,
        IMapper mapper,
        IOptionsMonitor<ResponseMessage> responseMessage)
        : base(positionService, mapper, responseMessage)
    {
    }

    [HttpPost]
    [Authorize(Roles = EditorRoles)]
    [ProducesResponseType(typeof(BaseResponse<PositionResource>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(BaseResponse<PositionResource>), StatusCodes.Status400BadRequest)]
    public new Task<IActionResult> CreateAsync(
        [FromBody] CreatePositionResource resource)
    {
        Log.Information(
            "{User} created a position.",
            User.Identity?.Name);

        return base.CreateAsync(resource);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = EditorRoles)]
    [ProducesResponseType(typeof(BaseResponse<PositionResource>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<PositionResource>), StatusCodes.Status400BadRequest)]
    public new Task<IActionResult> UpdateAsync(
        int id,
        [FromBody] UpdatePositionResource resource)
    {
        Log.Information(
            "{User} updated position {PositionId}.",
            User.Identity?.Name,
            id);

        return base.UpdateAsync(id, resource);
    }

    [HttpPut]
    [Authorize(Roles = EditorRoles)]
    [ProducesResponseType(typeof(BaseResponse<PositionResource>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<PositionResource>), StatusCodes.Status400BadRequest)]
    public new Task<IActionResult> ChangeOrderIndexAsync(
        [FromBody] List<int> ids)
    {
        Log.Information(
            "{User} changed position order indexes.",
            User.Identity?.Name);

        return base.ChangeOrderIndexAsync(ids);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = EditorRoles)]
    [ProducesResponseType(typeof(BaseResponse<PositionResource>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<PositionResource>), StatusCodes.Status400BadRequest)]
    public new Task<IActionResult> DeleteAsync(int id)
    {
        Log.Information(
            "{User} deleted position {PositionId}.",
            User.Identity?.Name,
            id);

        return base.DeleteAsync(id);
    }
}
