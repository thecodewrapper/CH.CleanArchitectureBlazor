// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System.ComponentModel.DataAnnotations;
using CH.CleanArchitecture.Core.Application.Commands;
using CH.CleanArchitecture.Presentation.WebApp.Extensions;
using CH.CleanArchitecture.Presentation.WebApp.Models;
using CH.CleanArchitecture.Presentation.WebApp.Services;
using CH.Messaging.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CH.CleanArchitecture.Presentation.WebApp.Pages.Account
{
    public class RegisterUserModel : PageModel
    {
        private readonly ILogger<RegisterUserModel> _logger;
        private readonly IServiceBus _serviceBus;
        private readonly UserAccountService _userAccountService;
        private readonly DocumentsService _documentUploadService;

        public RegisterUserModel(ILogger<RegisterUserModel> logger, IServiceBus serviceBus, UserAccountService userAccountService, DocumentsService documentUploadService) {
            _logger = logger;
            _serviceBus = serviceBus;
            _userAccountService = userAccountService;
            _documentUploadService = documentUploadService;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public class InputModel
        {
            [Required]
            [Display(Name = "Name")]
            public string Name { get; set; }

            [Required]
            [Display(Name = "Surname")]
            public string Surname { get; set; }

            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [Phone]
            [Display(Name = "Phone")]
            public string Phone { get; set; }

            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [Display(Name = "Description")]
            public string Description { get; set; }

            [StringLength(10, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 1)]
            [Display(Name = "Street number")]
            public string AddressStreetNumber { get; set; }

            [StringLength(50, ErrorMessage = "The {0} must not exceed {1} characters.")]
            [Display(Name = "Flat number")]
            public string AddressFlatNumber { get; set; }

            [StringLength(50, ErrorMessage = "The {0} must not exceed {1} characters.")]
            [Display(Name = "Street 1")]
            public string AddressStreet1 { get; set; }

            [StringLength(50, ErrorMessage = "The {0} must not exceed {1} characters.")]
            [Display(Name = "Street 2")]
            public string AddressStreet2 { get; set; }

            [StringLength(50, ErrorMessage = "The {0} must not exceed {1} characters.")]
            [Display(Name = "Area")]
            public string AddressArea { get; set; }

            [StringLength(50, ErrorMessage = "The {0} must not exceed {1} characters.")]
            [Display(Name = "City")]
            public string AddressCity { get; set; }

            [StringLength(50, ErrorMessage = "The {0} must not exceed {1} characters.")]
            [Display(Name = "Country")]
            public string AddressCountry { get; set; }

            [Required]
            [StringLength(64, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 12)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

            [Display(Name = "Profile picture")]
            public FileViewModel ProfilePicture { get; set; }
        }


        public async Task OnGetAsync(string returnUrl = null) {
            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null) {
            returnUrl ??= Url.Content("~/");
            if (ModelState.IsValid) {
                var createUserResult = await _serviceBus.SendAsync(new CreateUserCommand(Input.Email, Input.Name, Input.Surname, Input.Email, Input.Password, new List<string>() { "User" }));

                if (createUserResult.IsSuccessful) {
                    _logger.LogInformation("User created a new account with password.");

                    //await SetProfilePicture(createUserResult.Unwrap());

                    await _userAccountService.SendConfirmationEmailAsync(Input.Email, HttpContext.GetBaseUri());

                    return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl });
                }
                else {
                    foreach (var error in createUserResult.Errors) {
                        ModelState.AddModelError(string.Empty, error.Error);
                    }
                }
            }

            return Page();
        }

        //private async Task SetProfilePicture(Guid userId) {
        //    if (Input.ProfilePicture != null) {
        //        string profilePictureResourceId = Guid.NewGuid().ToString();
        //        var uploadResult = await _documentUploadService.UploadProfilePictureAsync(Input.ProfilePicture?.FormFile, profilePictureResourceId); //if this fails, we continue as if nothing happened.
        //        if (uploadResult.IsSuccessful) {
        //            string profilePictureUri = _documentUploadService.GetURIForProfilePicture(profilePictureResourceId);
        //            await _serviceBus.SendAsync(new ChangeUserProfilePictureCommand(userId, profilePictureUri));
        //        }
        //    }
        //}
    }
}
