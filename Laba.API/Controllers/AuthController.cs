using System.ComponentModel.DataAnnotations;
using AutoMapper;
using Laba.API.Abstract.Interfaces.RepositoryInterfaces;
using Laba.API.Abstract.Interfaces.ServiceInterfaces;
using Laba.API.Filters;
using Laba.Shared.Domain.Dtos;
using Laba.Shared.Domain.Entities;
using Laba.Shared.Domain.Models;
using Laba.Shared.Domain.ValueObjects;
using Laba.Shared.Extensions;
using Laba.Shared.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Laba.API.Controllers;

[ApiController]
[Route("/api/auth")]
public class AuthController(
    ILogger<AuthController> logger,
    IAttemptServer attemptServer,
    IUserRepository userRepository,
    IHash hasher,
    IMapper mapper,
    IHashVerify hashVerify,
    IJwtService jwtService,
    ITokenRepository tokenRepository)
    : ControllerBase
{
    [HttpGet("get-info"), Authorize]
    public async Task<IActionResult> GetUserInfo()
    {
        User.TryGetUserId(out var id);

        var userEntity = await userRepository.Get(id);
        var userDto = mapper.Map<UserDtoToView>(userEntity);

        return Ok(userDto);
    }

    [HttpPost("register"), AnonymousOnlyFilter]
    public async Task<IActionResult> Register([Required, FromForm] RegisterRequest request)
    {
        var email = EmailValueObject.Create(request.Email);

        if (email is null)
            return BadRequest("The Email field is invalid");

        var isUsernameTaken = await userRepository.Exists(username: request.Username);
        var isEmailTaken = await userRepository.Exists(email: (EmailValueObject)email);

        if (isUsernameTaken)
            return Conflict(new ErrorOfProperty
                { PropertyName = "username", ErrorMessage = "The Username is already taken" });
        if (isEmailTaken)
            return Conflict(new ErrorOfProperty
                { PropertyName = "email", ErrorMessage = "The Email is already taken" });

        var hashedPassword = new HashedPasswordValueObject { Password = hasher.Hash(request.Password) };
        var user = UserEntity.Create(request.Username, (EmailValueObject)email, hashedPassword);

        if (user == null)
            throw new ApplicationException("Why api lets create a invalid user?");

        await userRepository.CreateUser(user);
        var tokens = await jwtService.GenerateNewTokens(user.Id, user.Username);
        return Ok(tokens);
    }

    [HttpPost("login"), AnonymousOnlyFilter]
    public async Task<IActionResult> Login([Required, FromForm] LoginRequest request)
    {
        var userIp = HttpContext.Connection.RemoteIpAddress?.ToString();

        if (userIp is null)
            return BadRequest("The remote IP address is invalid");

        var keyInCache = $"{userIp}={request.Email}";
        if (attemptServer.IsBlocked(keyInCache))
        {
            return StatusCode(StatusCodes.Status429TooManyRequests,
                "Too many failed login attempts. Try later");
        }

        var email = EmailValueObject.Create(request.Email);

        if (email is null)
            return BadRequest(new ErrorOfProperty
                { PropertyName = "email", ErrorMessage = "The Email field is invalid" });

        var user = await userRepository.Get(email: (EmailValueObject)email);

        if (user == null)
            return NotFound("The user doesn't exist");

        var passwordIsRight = hashVerify.AreBothSame(str: request.Password, hashedString: user.HashedPassword.Password);

        if (!passwordIsRight)
        {
            await attemptServer.Decrement(keyInCache);

            return BadRequest(new ErrorOfProperty
                { PropertyName = "password", ErrorMessage = "The password isn't right" });
        }

        var tokens = await jwtService.GenerateNewTokens(user.Id, user.Username);
        return Ok(tokens);
    }


    [HttpPut("tokens-update/{oldRefreshToken}"), AnonymousOnlyFilter]
    public async Task<IActionResult> TokensUpdate([Required] string oldRefreshToken)
    {
        var oldToken = await tokenRepository.Get(oldRefreshToken);

        if (oldToken is null)
            return NotFound("You don't have a refresh token in database");

        var user = await userRepository.Get(oldToken.UserId);

        if (user is null)
            throw new ApplicationException("The user should exists if there's the refresh token in database");

        var tokens = await jwtService.GenerateNewTokens(user.Id, user.Username);
        return Ok(tokens);
    }
}