using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper.Configuration;
using Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Piranha.AspNetCore.Identity;
using Piranha.AspNetCore.Identity.Data;
using Piranha.AspNetCore.Identity.Models;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{

	// private readonly DataContext _context;
	// private readonly IConfiguration _configuration;
	
	private readonly Piranha.ISecurity _service;
	private readonly IDb _db;
	private readonly UserManager<User> _userManager;

	private readonly TokenDb _tokenDb;


	public UserController(IDb db,  UserManager<User> userManager, Piranha.ISecurity service, TokenDb tokenDb)
	{
		_db = db;
		_userManager = userManager;
		_service = service;
		_tokenDb = tokenDb;
	}


	[HttpPost]
	[Route("/api/user/login")]
	public async Task<ActionResult<AccountDTO>> Login([FromForm] AccountDTO item)
	{
		var signedIn = await _service.SignIn(HttpContext, item.Email, item.Password);

		if (signedIn) {
			var user = _db.Users.Where(x => x.Email == item.Email).FirstOrDefault();

			_tokenDb.Tokens.RemoveRange(_tokenDb.Tokens.Where(x => x.User == user.Id));
			await _tokenDb.SaveChangesAsync();

			var token = new Token() {
				User = user.Id,
				APIToken = Guid.NewGuid(),
				Created = DateTime.Now
			};

			_tokenDb.Attach(token);
			await _tokenDb.SaveChangesAsync();

			return CreatedAtAction(nameof(Token), new { id = token.Id }, token);
		}
		
		return BadRequest();
	}


	[HttpPost]
	[Route("/api/user/save")]
	// [Authorize(Policy = Permissions.UsersSave)]
	public async Task<IActionResult> Save([FromForm] AccountDTO inputModel) {

		var model = new UserEditModel() {
			User = new User() {
				UserName = inputModel.Email,
				Email = inputModel.Email
			},
			Password = inputModel.Password,
			PasswordConfirm = inputModel.PasswordConfirm
		};

		try
		{
			if (string.IsNullOrWhiteSpace(model.User.UserName))
			{
				return BadRequest("Username is mandatory.");
			}

			if (string.IsNullOrWhiteSpace(model.User.Email))
			{
				return BadRequest("Email address is mandatory.");
			}

			if (!string.IsNullOrWhiteSpace(model.Password) && model.Password != model.PasswordConfirm)
			{
				return BadRequest("The new passwords does not match.");
			}

			if (model.User.Id == Guid.Empty && string.IsNullOrWhiteSpace(model.Password))
			{
				return BadRequest("Password is mandatory when creating a new user.");
			}

			if (!string.IsNullOrWhiteSpace(model.Password) && _userManager.PasswordValidators.Count > 0)
			{
				var errors = new List<string>();
				foreach (var validator in _userManager.PasswordValidators)
				{
					var errorResult = await validator.ValidateAsync(_userManager, model.User, model.Password);
					if (!errorResult.Succeeded)
						errors.AddRange(errorResult.Errors.Select(msg => msg.Description));
					if (errors.Count > 0)
					{
						return BadRequest(string.Join("<br />", errors));
					}
				}
			}

			//check username
			if (await _db.Users.CountAsync(u => u.UserName.ToLower().Trim() == model.User.UserName.ToLower().Trim()) > 0)
			{
				return BadRequest("Username is used by another user.");
			}

			//check email
			if (await _db.Users.CountAsync(u => u.Email.ToLower().Trim() == model.User.Email.ToLower().Trim()) > 0)
			{
				return BadRequest("Email address is used by another user.");
			}

			if ((await model.Save(_userManager)).Succeeded)
			{
				return Ok(model.User.Id);
			}

			var errorMessages = new List<string>();
			errorMessages.AddRange((await model.Save(_userManager)).Errors.Select(msg => msg.Description));

			return BadRequest("The user could not be saved." + "<br/><br/>" + string.Join("<br />", errorMessages));
		}
		catch (Exception ex)
		{
			return BadRequest(ex.Message);
		}
	}
}