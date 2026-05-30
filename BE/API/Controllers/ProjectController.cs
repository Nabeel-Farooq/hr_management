using AutoMapper;
using Business.Communication;
using Business.Data;
using Business.Domain.Models;
using Business.Domain.Services;
using Business.Resources;
using Business.Resources.Project;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Serilog;

namespace API.Controllers;

[Route("api/v1/project")]
public sealed class ProjectController
    : DongNguyenController<
        ProjectResource,
        CreateProjectResource,
        UpdateProjectResource,
        Project>
{
    private const string EditorRoles =
        $"{Role.Admin}, {Role.EditorQTNS}, {Role.EditorQTDA}";

    public ProjectController(
        IProjectService projectService,
        IMapper mapper,
        IOptionsMonitor<ResponseMessage> responseMessage)
        : base(projectService, mapper, responseMessage)
    {
    }

    [HttpPost]
    [Authorize(Roles = EditorRoles)]
    [ProducesResponseType(typeof(BaseResponse<ProjectResource>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(BaseResponse<ProjectResource>), StatusCodes.Status400BadRequest)]
    public new Task<IActionResult> CreateAsync(
        [FromBody] CreateProjectResource resource)
    {
        Log.Information(
            "{User} created a project.",
            User.Identity?.Name);

        return base.CreateAsync(resource);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = EditorRoles)]
    [ProducesResponseType(typeof(BaseResponse<ProjectResource>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<ProjectResource>), StatusCodes.Status400BadRequest)]
    public new Task<IActionResult> UpdateAsync(
        int id,
        [FromBody] UpdateProjectResource resource)
    {
        Log.Information(
            "{User} updated project {ProjectId}.",
            User.Identity?.Name,
            id);

        return base.UpdateAsync(id, resource);
    }

    [HttpPut]
    [Authorize(Roles = EditorRoles)]
    [ProducesResponseType(typeof(BaseResponse<ProjectResource>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<ProjectResource>), StatusCodes.Status400BadRequest)]
    public new Task<IActionResult> ChangeOrderIndexAsync(
        [FromBody] List<int> ids)
    {
        Log.Information(
            "{User} changed project order indexes.",
            User.Identity?.Name);

        return base.ChangeOrderIndexAsync(ids);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = EditorRoles)]
    [ProducesResponseType(typeof(BaseResponse<ProjectResource>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<ProjectResource>), StatusCodes.Status400BadRequest)]
    public new Task<IActionResult> DeleteAsync(int id)
    {
        Log.Information(
            "{User} deleted project {ProjectId}.",
            User.Identity?.Name,
            id);

        return base.DeleteAsync(id);
    }
}
