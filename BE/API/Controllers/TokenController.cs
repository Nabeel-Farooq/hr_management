using Business.Communication;
using Business.Domain.Services;
using Business.Resources;
using Business.Resources.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Serilog;

namespace API.Controllers;

[ApiController]
[Route("api/v1/token")]
public sealed class TokenController : ControllerBase
{
    private readonly ITokenManagementService _tokenManagementService;
    protected readonly ResponseMessage ResponseMessage;

    public TokenController(
        ITokenManagementService tokenManagementService,
        IOptionsMonitor<ResponseMessage> responseMessage)
    {
        _tokenManagementService = tokenManagementService;
        ResponseMessage = responseMessage.CurrentValue;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(typeof(BaseResponse<AccessTokenResource>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<AccessTokenResource>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> LoginAsync([FromBody] LoginResource resource)
    {
        var userAgent = Request.Headers.UserAgent.ToString();

        var result = await _tokenManagementService.GenerateTokensAsync(
            resource,
            DateTime.UtcNow,
            userAgent);

        if (!result.Success)
            return Unauthorized(result);

        Log.Information(
            "{UserName} logged in successfully.",
            result.Resource?.UserName);

        return Ok(result);
    }

    [AllowAnonymous]
    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(BaseResponse<TokenResource>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<TokenResource>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GenerateNewTokensAsync(
        [FromBody] RefreshTokenResource resource)
    {
        resource.UserAgent = Request.Headers.UserAgent.ToString();

        var result = await _tokenManagementService.GenerateNewTokensAsync(
            resource,
            DateTime.UtcNow);

        Log.Information(
            "Account {AccountId} used refresh token {RefreshTokenId}. Success: {Success}",
            resource.AccountId,
            resource.Id,
            result.Success);

        return result.Success
            ? Ok(result)
            : Unauthorized(result);
    }

    [AllowAnonymous]
    [HttpPost("logout")]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> LogoutAsync([FromBody] LogoutResource resource)
    {
        Log.Information(
            "Refresh token {RefreshTokenId} logged out.",
            resource.Id);

        var result = await _tokenManagementService.LogoutAsync(resource);

        return result.Success
            ? Ok(result)
            : BadRequest(result);
    }
}
