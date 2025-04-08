using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NewsApi.AppHandler.Wrapper.Interface;
using NewsAPI.Domain.DTOS.UserAccountDto;
using NewsAPI.Domain.IdentityEntity;
using NewsApi.AppHandler.Wrapper.WorkWrapper;
using NewsApi.AppHandler.ErrorHandler;

namespace NewsAPIApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserAccountController : ControllerBase
    {
        private readonly UserManager<Account> _userManager;
        private readonly SignInManager<Account> _signInManager;

        public UserAccountController(UserManager<Account> userManager, SignInManager<Account> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpPost("Register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDTO model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    // Extract the error messages from ModelState
                    var errors = ModelState.Values.SelectMany(v => v.Errors)
                                                   .Select(e => e.ErrorMessage)
                                                   .ToList();
                    // Convert the list of errors into a single string or pass the list itself if your Fail method supports it
                    var errorMessage = string.Join("; ", errors);
                    return BadRequest(ResultResponse<UserDto>.Fail(errorMessage));
                }
                var guestExists = await CheckIfUserExist(model.Email);
                if (guestExists.Value)
                {
                    return BadRequest(new ApiResponse(400, "This Email Is Already Exist"));
                }
                var user = new Account
                {
                    Email = model.Email,
                    UserName = model.UserName
                };
                var Result = await _userManager.CreateAsync(user, model.Password);
                if (!Result.Succeeded) return BadRequest(new ApiResponse(400));

                var returnData = new UserDto { Email = model.Email, UserName = model.UserName };
                return Ok(ResultResponse<UserDto>.Success(returnData, "register Success"));
            }
            catch (Exception ex)
            {
                return Ok(ResultResponse<UserDto>.Fail(ex.Message));
            }
        }

        [HttpPost("AccountLogin")]
        public async Task<ActionResult<UserDto>> AccountLogin([FromBody] LoginDto model)
        {
            try
            {
                // Step 1: Validate the incoming model state
                if (!ModelState.IsValid)
                {
                    // Extract the error messages from ModelState
                    var errors = ModelState.Values.SelectMany(v => v.Errors)
                                                   .Select(e => e.ErrorMessage)
                                                   .ToList();
                    // Convert the list of errors into a single string or pass the list itself if your Fail method supports it
                    var errorMessage = string.Join("; ", errors);
                    return BadRequest(ResultResponse<LoginDto>.Fail(errorMessage));
                }

                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    // If the user is not found, return an Unauthorized response
                    return Unauthorized(new ApiResponse(401, "User Not Found"));
                }

                // Step 5: Check the user's password
                var resultcode = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);

                if (!resultcode.Succeeded)
                {
                    // If the password check fails, return appropriate error messages
                    if (resultcode.IsLockedOut)
                        return BadRequest(new ApiResponse(400, "User is locked out"));
                    if (resultcode.IsNotAllowed)
                        return BadRequest(new ApiResponse(400, "User is not allowed to sign in"));
                    if (resultcode.RequiresTwoFactor)
                        return BadRequest(new ApiResponse(400, "Two-factor authentication required"));

                    // If none of the above, return a generic invalid login attempt message
                    return BadRequest(new ApiResponse(400, "Password or email is wrong"));
                }

                // Step 6: Create a response object with the user's email and a token
                var returnedUser = new UserDto
                {
                    Email = model.Email,
                    UserName = user.UserName
                };

                // Step 7: Return a success response with the created token
                return Ok(ResultResponse<UserDto>.Success(returnedUser, "Login successful"));
            }
            catch (Exception ex)
            {
                // Step 8: Handle any exceptions that occur during the process
                return StatusCode(500, ResultResponse<UserDto>.Fail(ex.Message));
            }
        }

        [HttpGet("IsUserExist")]
        public async Task<ActionResult<bool>> CheckIfUserExist(string Email)
        {
            return await _userManager.FindByEmailAsync(Email) is not null;
        }
    }
}