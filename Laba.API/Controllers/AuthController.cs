using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Security.Cryptography;
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
using Laba.Shared.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace Laba.API.Controllers;

[ApiController]
[Route("/api/auth")]
public class AuthController(
    ILogger<AuthController> logger,
    IAttemptServer attemptService,
    IUserRepository userRepository,
    IHash hasher,
    IMapper mapper,
    IHashVerify hashVerify,
    IJwtService jwtService,
    ITokenRepository tokenRepository,
    IChallengeService challengeService)
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

        var hashedPassword = new HashedPasswordValueObject { Password = hasher.HashPassword(request.Password) };
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
        var userIp = HttpContext.Connection.RemoteIpAddress ?? HttpContext.Connection.LocalIpAddress;

        if (userIp is null)
            return BadRequest("The remote IP address is invalid");

        var keyInCache = $"{userIp.ToString()}={request.Email}";
        bool isBlocked = await attemptService.IsBlocked(keyInCache);
        if (isBlocked)
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

        var passwordIsRight = hashVerify.IsPasswordRight(password: request.Password,
            hashedPassword: user.HashedPassword);

        if (!passwordIsRight)
        {
            await attemptService.Decrement(keyInCache);

            return BadRequest(new ErrorOfProperty
                { PropertyName = "password", ErrorMessage = "The password isn't right" });
        }

        var tokens = await jwtService.GenerateNewTokens(user.Id, user.Username);
        return Ok(tokens);
    }

    [HttpPost("handshake/start/{email}"), AnonymousOnlyFilter]
    public async Task<IActionResult> StartHandshake([Required] string email)
    {
        var emailValueObject = EmailValueObject.Create(email);

        if (emailValueObject is null)
            return BadRequest(new ErrorOfProperty
                { PropertyName = "email", ErrorMessage = "The Email field is invalid" });

        var user = await userRepository.Get(email: (EmailValueObject)emailValueObject);

        if (user == null)
            return NotFound("The user doesn't exist");

        var exists = await challengeService.Exists(user.Email);
        if (exists)
            await challengeService.RemoveNonce(user.Email);

        // Ниже генерируем Challenge и возвращаем
        var challenge = await challengeService.CreateNonce(user.Email, user.HashedPassword);
        return Ok(challenge);
    }

    [HttpPost("handshake/end"), AnonymousOnlyFilter]
    public async Task<IActionResult> EndHandshake([Required, FromForm] EndHandshakeRequest request)
    {
        var userIp = HttpContext.Connection.RemoteIpAddress ?? HttpContext.Connection.LocalIpAddress;

        if (userIp is null)
            return BadRequest("The remote IP address is invalid");

        var keyInCache = $"{userIp.ToString()}={request.Email}";
        bool isBlocked = await attemptService.IsBlocked(keyInCache);
        if (isBlocked)
        {
            return StatusCode(StatusCodes.Status429TooManyRequests,
                "Too many failed login attempts. Try later");
        }

        var emailValueObject = EmailValueObject.Create(request.Email);

        if (emailValueObject is null)
            return BadRequest(new ErrorOfProperty
                { PropertyName = "email", ErrorMessage = "The Email field is invalid" });

        var user = await userRepository.Get(email: (EmailValueObject)emailValueObject);

        if (user == null)
            return NotFound("The user doesn't exist");

        var nonce = await challengeService.GetNonce(user.Email);

        // Если вызова (challenge) нету в кеше значит либо законичлось макс. время его жизни либо он был удален после успешного входа юзера  
        if (string.IsNullOrEmpty(nonce))
            return BadRequest(new ErrorOfProperty
            {
                ErrorMessage = "Handshake expired or not started",
                PropertyName = "nonce"
            });

        var parts = user.HashedPassword.Password.Split('-');
        var hash = Convert.FromHexString(parts[0]);
        var response = Convert.FromHexString(request.HashedResponse);

        if (hashVerify.AreHashesSame(hash, response) == false)
        {
            await attemptService.Decrement(keyInCache);
            return BadRequest(new ErrorOfProperty
            {
                ErrorMessage = "Invalid response",
                PropertyName = "password"
            });
        }

        await challengeService.RemoveNonce(user.Email);
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