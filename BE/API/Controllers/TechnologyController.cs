using AutoMapper;
using Business.Communication;
using Business.Data;
using Business.Domain.Models;
using Business.Domain.Repositories;
using Business.Domain.Services;
using Business.Extensions;
using Business.Resources;
using Business.Resources.Technology;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Serilog;

namespace API.Controllers;

[Route("api/v1/technology")]
public sealed class TechnologyController
    : DongNguyenController<
        TechnologyResource,
        CreateTechnologyResource,
        UpdateTechnologyResource,
        Technology>
{
    private const string EditorRoles =
        $"{Role.Admin}, {Role.EditorQTNS}, {Role.EditorQTDA}";

    private readonly ITechnologyRepository _technologyRepository;

    public TechnologyController(
        ITechnologyService technologyService,
        ITechnologyRepository technologyRepository,
        IMapper mapper,
        IOptionsMonitor<ResponseMessage> responseMessage)
        : base(technologyService, mapper, responseMessage)
    {
        _technologyRepository = technologyRepository;
    }

    [HttpGet("search")]
    [Authorize(Roles = EditorRoles)]
    [ProducesResponseType(typeof(BaseResponse<IEnumerable<TechnologyResource>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<IEnumerable<TechnologyResource>>), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(BaseResponse<IEnumerable<TechnologyResource>>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> FindAsync([FromQuery] string filterName)
    {
        var keyword = filterName.RemoveSpaceCharacter();

        Log.Information(
            "{User} searched technologies with keyword {Keyword}",
            User.Identity?.Name,
            keyword);

        var result = await _technologyRepository.FindByNameAsync(keyword);

        if (result is null || !result.Any())
            return NoContent();

        var resources = Mapper.Map<IEnumerable<TechnologyResource>>(result);

        return Ok(new BaseResponse<IEnumerable<TechnologyResource>>(resources));
    }

    [HttpPost]
    [Authorize(Roles = EditorRoles)]
    [ProducesResponseType(typeof(BaseResponse<TechnologyResource>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(BaseResponse<TechnologyResource>), StatusCodes.Status400BadRequest)]
    public new Task<IActionResult> CreateAsync(
        [FromBody] CreateTechnologyResource resource)
    {
        Log.Information(
            "{User} created a technology.",
            User.Identity?.Name);

        return base.CreateAsync(resource);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = EditorRoles)]
    [ProducesResponseType(typeof(BaseResponse<TechnologyResource>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<TechnologyResource>), StatusCodes.Status400BadRequest)]
    public new Task<IActionResult> UpdateAsync(
        int id,
        [FromBody] UpdateTechnologyResource resource)
    {
        Log.Information(
            "{User} updated technology {Id}.",
            User.Identity?.Name,
            id);

        return base.UpdateAsync(id, resource);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = EditorRoles)]
    [ProducesResponseType(typeof(BaseResponse<TechnologyResource>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<TechnologyResource>), StatusCodes.Status400BadRequest)]
    public new Task<IActionResult> DeleteAsync(int id)
    {
        Log.Information(
            "{User} deleted technology {Id}.",
            User.Identity?.Name,
            id);

        return base.DeleteAsync(id);
    }
}
