using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoMapper.Configuration;
using Data;
using KnApp.Services;
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
	private readonly Piranha.ISecurity _service;
	private readonly IDb _db;
	private readonly UserManager<User> _userManager;
	private readonly IEmailService _emailService;

	private readonly TokenDb _tokenDb;

	public UserController(IDb db,  UserManager<User> userManager, Piranha.ISecurity service, TokenDb tokenDb, IEmailService emailService)
	{
		_db = db;
		_userManager = userManager;
		_service = service;
		_tokenDb = tokenDb;
		_emailService = emailService;
	}
	


	[HttpPost]
	[Route("/api/user/checktoken")]
	public async Task<ActionResult<AccountDTO>> Checktoken([FromForm] Token item)
	{
		// var signedIn = await _service.SignIn(HttpContext, item.Email, item.Password);

		var exists = _tokenDb.Tokens.Any(x => x.APIToken == item.APIToken);

		if (exists) {
			return Ok();
		}
		return Unauthorized();
	}

	[HttpPost]
	[Route("/api/user/login")]
	public async Task<ActionResult<AccountDTO>> Token([FromForm] AccountDTO item)
	{

		Console.WriteLine(item.Email);
		Console.WriteLine(item.Password);
		
		var signedIn = await _service.SignIn(HttpContext, item.Email, item.Password);

		if (signedIn) {
			var user = _db.Users.Where(x => x.Email == item.Email).FirstOrDefault();

			if (!user.EmailConfirmed) {
				return BadRequest("Du måste verifiera ditt konto, kolla mailen!");
			}

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
		
		return BadRequest("Felaktiga inloggningsuppgifter, var god och prova igen =)");
	}

	[HttpPost]
	[Route("/api/user/register")]
	// [Authorize(Policy = Permissions.UsersSave)]
	public async Task<IActionResult> Register([FromForm] AccountDTO inputModel) {

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
				var host = $"{Request.Scheme}://{Request.Host.Value}";
				var url = this.Url.Action("Confirm", "User", new { id = model.User.Id });
				var fullurl = host + url;

				var html = $"Hej! <br><br>Klicka på länken nedan för att bekräfta ditt konto: <a href=\"{fullurl}\">{fullurl}</a><br><br>Mvh KnAppen";
				await _emailService.SendAsync(model.User.Email, "Verifiera konto för KnApp", "", html);
				
				return Ok("Hurra! Verifiera ditt konto genom att klicka på länken i mailet du får snart.");
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

	[HttpGet]
	[Route("/api/user/confirm")]
	public async Task<ActionResult<AccountDTO>> Confirm(Guid id)
	{
		var user = _db.Users.Where(x => x.Id == id).FirstOrDefault();

		if (user != null) {

			if (user.EmailConfirmed) {
				return Ok("Already confirmed");
			}

			user.EmailConfirmed = true;
			await _db.SaveChangesAsync();

			return Ok("ok");
		}

		return Ok("fel");
	}
}