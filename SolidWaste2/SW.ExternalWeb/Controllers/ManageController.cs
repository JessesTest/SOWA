using Common.Extensions;
using Common.Services.AddressValidation;
using Common.Services.Email;
using Common.Services.Sms;
using Identity.BL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PE.BL.Services;
using PE.DM;
using SW.BLL.Services;
using SW.ExternalWeb.Extensions;
using SW.ExternalWeb.Identity;
using SW.ExternalWeb.Models.Manage;
using System.Text;

namespace SW.ExternalWeb.Controllers
{
    [Authorize]
    public class ManageController : Controller
    {
        private readonly IAddressService _addressService;
        private readonly IAddressValidationService _addressValidationService;
        private readonly ICodeService _codeService;
        private readonly ICustomerService _customerService;
        private readonly IEmailService _emailService;
        private readonly IPersonEntityService _personEntityService;
        private readonly IPhoneService _phoneService;
        private readonly ISendGridService _sendGridService;
        private readonly ITwilioService _twilioservice;
        private readonly IUserNotificationService _userNotificationService;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public ManageController(
            IAddressService addressService,
            IAddressValidationService addressValidationService,
            ICodeService codeService,
            ICustomerService customerService,
            IEmailService emailService,
            IPersonEntityService personEntityService,
            IPhoneService phoneService,
            ISendGridService sendGridService,
            ITwilioService twilioService,
            IUserNotificationService userNotificationService,
            SignInManager<ApplicationUser> signInManager, 
            UserManager<ApplicationUser> userManager)
        {
            _addressService = addressService;
            _addressValidationService = addressValidationService;
            _codeService = codeService;
            _customerService = customerService;
            _emailService = emailService;
            _personEntityService = personEntityService;
            _phoneService = phoneService;
            _sendGridService = sendGridService;
            _twilioservice = twilioService;
            _userNotificationService = userNotificationService;
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<ActionResult> Index()
        {
            var user = await _userManager.FindByIdAsync(User.GetUserId());
            var vm = new IndexViewModel();

            var person = await _personEntityService.GetById(user.UserId);
            var email = (await _emailService.GetByPerson(user.UserId, false)).SingleOrDefault(e => e.IsDefault);
            var phone = (await _phoneService.GetByPerson(user.UserId, false)).SingleOrDefault(e => e.IsDefault);
            var address = (await _addressService.GetByPerson(user.UserId, false)).SingleOrDefault(e => e.IsDefault && e.Code.Description == "Billing");

            vm.Address1 = address == null ? string.Empty : string.Format("{0} {1} {2} {3} {4}", address.Number?.ToString(), address.Direction, address.StreetName, address.Suffix, address.Apt).Trim().ToUpper();
            vm.Address2 = address == null ? string.Empty : string.Format("{0}, {1} {2}", address.City, address.State, address.Zip).Trim().ToUpper();
            vm.EmailAddress = email == null ? string.Empty : email.Email1;
            vm.PhoneType = phone == null ? string.Empty : phone.Code.Description;
            vm.PhoneNumber = phone == null ? string.Empty : phone.PhoneNumber;
            vm.TwoFactor = user.TwoFactorEnabled ? "ENABLED" : "DISABLED";
            vm.Name = person.FullName;
            vm.WhenCreated = person.WhenCreated;
            vm.Delivery = !person.PaperLess.HasValue ? "Email and Paper Billing" : person.PaperLess.Value ? "Email Billing Only" : "Paper Billing Only";

            return View(vm);
        }

        #region Password

        [HttpGet]
        public async Task<ActionResult> Password()
        {
            var vm = new PasswordViewModel();

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Password(PasswordViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            try
            {
                var user = await _userManager.FindByIdAsync(User.GetUserId());
                var result = await _userManager.ChangePasswordAsync(user, vm.OldPassword, vm.NewPassword);
                if (result.Errors.Any())
                    throw new Exception(result.Errors.First().Description.ToString());

                if (result.Succeeded)
                {
                    if (user != null)
                        await SignInAsync(user, false);

                    ModelState.Clear();
                    ModelState.AddModelError("success", "Password successfully changed");
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    if (user != null)
                        await SignInAsync(user, false);

                    ModelState.Clear();
                    ModelState.AddModelError("", "Current password is incorrect or new passwords do not match.");
                    return RedirectToAction(nameof(Password));
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(vm);
            }
        }

        #endregion

        #region Two Factor

        [HttpGet]
        public async Task<ActionResult> TwoFactor()
        {
            var user = await _userManager.FindByIdAsync(User.GetUserId());
            var vm = new TwoFactorViewModel();

            vm.EmailsDropDown = await GenerateEmailsSelectList(user.UserId);
            vm.PhonesDropDown = await GeneratePhonesSelectList(user.UserId);
            vm.TwoFactor = await _userManager.GetTwoFactorEnabledAsync(user);
            vm.BrowserRemembered = await _signInManager.IsTwoFactorClientRememberedAsync(user);

            var email = (await _emailService.GetByPerson(user.UserId, false)).SingleOrDefault(e => e.Email1 == user.Email);
            var phone = (await _phoneService.GetByPerson(user.UserId, false)).SingleOrDefault(p => p.PhoneNumber == user.PhoneNumber);

            vm.TwoFactorEmailID = email == null ? 0 : email.Id;
            vm.TwoFactorPhoneID = phone == null ? 0 : phone.Id;

            return View(vm);
        }

        [HttpGet]
        public async Task<ActionResult> EnableTFA()
        {
            try
            {
                var user = await _userManager.FindByIdAsync(User.GetUserId());
                await _userManager.SetTwoFactorEnabledAsync(user, true);

                ModelState.Clear();
                ModelState.AddModelError("success", "Two factor authentication enabled");
                return RedirectToAction(nameof(TwoFactor));
            }
            catch(Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return RedirectToAction(nameof(TwoFactor));
            }
        }

        [HttpGet]
        public async Task<ActionResult> DisableTFA()
        {
            try
            {
                var user = await _userManager.FindByIdAsync(User.GetUserId());
                await _userManager.SetTwoFactorEnabledAsync(user, false);

                ModelState.Clear();
                ModelState.AddModelError("success", "Two factor authentication disabled");
                return RedirectToAction(nameof(TwoFactor));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return RedirectToAction(nameof(TwoFactor));
            }
        }

        [HttpGet]
        public async Task<ActionResult> RememberBrowser()
        {
            try
            {
                var user = await _userManager.FindByIdAsync(User.GetUserId());
                await _signInManager.RememberTwoFactorClientAsync(user);

                ModelState.Clear();
                ModelState.AddModelError("success", "Two factor authentication set to remember browser");
                return RedirectToAction(nameof(TwoFactor));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return RedirectToAction(nameof(TwoFactor));
            }
        }

        [HttpGet]
        public async Task<ActionResult> ForgetBrowser()
        {
            try
            {
                await _signInManager.ForgetTwoFactorClientAsync();

                ModelState.Clear();
                ModelState.AddModelError("success", "Two factor authentication set to not remember browser");
                return RedirectToAction(nameof(TwoFactor));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return RedirectToAction(nameof(TwoFactor));
            }
        }

        [HttpGet]
        public async Task<ActionResult> ClearTwoFactorPhone()
        {
            try
            {
                var user = await _userManager.FindByIdAsync(User.GetUserId());
                await _userManager.SetPhoneNumberAsync(user, null);

                ModelState.Clear();
                ModelState.AddModelError("success", "Two factor phone settings cleared");
                return RedirectToAction(nameof(TwoFactor));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return RedirectToAction(nameof(TwoFactor));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SetTwoFactorPhone(TwoFactorViewModel vm)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(User.GetUserId());
                var phone = await _phoneService.GetById(vm.TwoFactorPhoneID);

                // Make sure the user selected a valid phone from the list
                if (vm.TwoFactorPhoneID == 0)
                    return RedirectToAction(nameof(ClearTwoFactorPhone));

                // Make sure the phone belongs to this user
                if (phone.PersonEntityID != user.UserId)
                    throw new Exception("Phone number does not belong to user");

                var code = await _userManager.GenerateChangePhoneNumberTokenAsync(user, phone.PhoneNumber);
                var messageBody = "Your security code is: " + code;
                _ = await _twilioservice.SendSmsAsync(phone.PhoneNumber, messageBody);

                var verifyModel = new VerifyPhoneViewModel
                {
                    PhoneType = phone.Type,
                    Number = phone.PhoneNumber,
                    Ext = phone.Ext
                };
                TempData["VerifyPhone"] = verifyModel;

                return RedirectToAction(nameof(VerifyPhoneNumber));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return RedirectToAction(nameof(TwoFactor));
            }
        }

        // SOWA-38 pathing to this view is commented out and related actions don't exist so this was commented out on conversion, refactor will be needed if this is re-enabled
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> SetTwoFactorEmail(TwoFactorViewModel vm)
        //{
        //    try
        //    {
        //        var user = await _userManager.FindByIdAsync(User.GetUserId());
        //        var email = await _emailService.GetById(vm.TwoFactorEmailID);

        //        // Make sure the user selected a valid email from the list
        //        if (vm.TwoFactorEmailID == 0)
        //            return RedirectToAction(nameof(ClearTwoFactorEmail));

        //        // Make sure the email belongs to this user
        //        if (email.PersonEntityID != user.UserId)
        //            throw new Exception("Email address does not belong to user");

        //        await _userManager.SetEmailAsync(user, email.Email1);
        //        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);

        //        var confirmationEmail = new SendEmailDto
        //        {
        //            TextContent = "Your security code is: " + code,
        //            Subject = "Set Two Factor Email",
        //        }
        //        .SetFrom("no-reply.scsw@sncoapps.us")
        //        .AddTo(email.Email1);
        //        _ = _sendGridService.SendSingleEmail(confirmationEmail);

        //        var verifyModel = new VerifyEmailViewModel
        //        {
        //            Email = email.Email1,
        //            EmailType = email.Type
        //        };
        //        TempData["VerifyEmail"] = verifyModel;

        //        return RedirectToAction(nameof(VerifyEmailAddress));
        //    }
        //    catch (Exception ex)
        //    {
        //        ModelState.AddModelError("", ex.Message);
        //        return RedirectToAction(nameof(TwoFactor));
        //    }
        //}

        [HttpGet]
        public async Task<ActionResult> VerifyPhoneNumber()
        {
            var vm = TempData["VerifyPhone"] as VerifyPhoneViewModel;
            return (vm == null || vm.Number == null) ? View("Error") : View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyPhoneNumber(VerifyPhoneViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            try
            {
                var user = await _userManager.FindByIdAsync(User.GetUserId());
                var phone = new PE.DM.Phone
                {
                    Type = vm.PhoneType,
                    PhoneNumber = vm.Number,
                    Ext = vm.Ext,
                    PersonEntityID = user.UserId
                };

                var result = await _userManager.ChangePhoneNumberAsync(user, vm.Number, vm.Code);
                if (result.Succeeded)
                {
                    if (user != null)
                        await SignInAsync(user, false);

                    ModelState.Clear();
                    ModelState.AddModelError("success", "Phone number successfully added");
                    return RedirectToAction(nameof(Index));
                }
                throw new Exception(result.Errors.ToString());
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(vm);
            }
        }

        #endregion

        #region Phone

        [HttpGet]
        public async Task<ActionResult> Phones()
        {
            var user = await _userManager.FindByIdAsync(User.GetUserId());
            var phonesDM = await _phoneService.GetByPerson(user.UserId, false);

            var vm = new PhonesViewModel();
            vm.Phones = phonesDM.Select(p => new PhoneListViewModel
            {
                PhoneID = p.Id,
                TypeDescription = p.Code.Description,
                Number = string.Format("{0}{1}", p.PhoneNumber, string.IsNullOrWhiteSpace(p.Ext) ? "" : " x" + p.Ext),
                IsDefault = p.IsDefault
            }).ToList();

            return View(vm);
        }

        [HttpGet]
        public async Task<ActionResult> AddPhoneNumber()
        {
            var user = await _userManager.FindByIdAsync(User.GetUserId());
            var phonesDM = await _phoneService.GetByPerson(user.UserId, false);

            var vm = new AddPhoneViewModel();
            vm.PhoneTypesDropDown = await GeneratePhoneTypesSelectList();
            vm.Phones = phonesDM.Select(p => new PhoneListViewModel
            {
                PhoneID = p.Id,
                TypeDescription = p.Code.Description,
                Number = string.Format("{0}{1}", p.PhoneNumber, string.IsNullOrWhiteSpace(p.Ext) ? "" : " x" + p.Ext),
                IsDefault = p.IsDefault
            }).ToList();

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddPhoneNumber(AddPhoneViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.PhoneTypesDropDown = await GeneratePhoneTypesSelectList();
                return View(vm);
            }

            try
            {
                // Make sure the phone type is valid
                if (!await PhoneTypeIsValid(vm.PhoneType))
                    throw new Exception("Phone type is invalid");

                // Make sure the user doesn't already have the same phone number elsewhere
                if (await PhoneExists(vm.Number, vm.Ext))
                    throw new Exception("Phone number already exists");

                var user = await _userManager.FindByIdAsync(User.GetUserId());
                var phones = await _phoneService.GetByPerson(user.UserId, false);
                var phone = new PE.DM.Phone
                {
                    Type = vm.PhoneType,
                    PhoneNumber = vm.Number,
                    Ext = vm.Ext,
                    PersonEntityID = user.UserId
                };

                // If user's first phone, set it to default
                if (!phones.Any())
                    phone.IsDefault = true;

                phone.Status = true;

                await _phoneService.Add(phone);

                ModelState.Clear();
                ModelState.AddModelError("success", "Phone number successfully added");
                return RedirectToAction(nameof(Phones));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                vm.PhoneTypesDropDown = await GeneratePhoneTypesSelectList();
                return View(vm);
            }
        }

        [HttpGet]
        public async Task<ActionResult> ChangePhoneNumber(int? id)
        {
            if (id == null)
                return RedirectToAction(nameof(Phones));

            try
            {
                var user = await _userManager.FindByIdAsync(User.GetUserId());
                var phonesDM = await _phoneService.GetByPerson(user.UserId, false);
                var phone = phonesDM.SingleOrDefault(p => p.Id == id.Value);

                // Make sure the phone number exists and belongs to this user
                if (phone == null)
                    throw new Exception("Phone not found or does not belong to this user");

                var vm = new ChangePhoneViewModel 
                { 
                    PhoneID = phone.Id,
                    PhoneType = phone.Type,
                    Number = phone.PhoneNumber,
                    Ext = phone.Ext,
                    PhoneTypesDropDown = await GeneratePhoneTypesSelectList(),
                    Phones = phonesDM.Select(p => new PhoneListViewModel
                    {
                        PhoneID = p.Id,
                        TypeDescription = p.Code.Description,
                        Number = string.Format("{0}{1}", p.PhoneNumber, string.IsNullOrWhiteSpace(p.Ext) ? "" : " x" + p.Ext),
                        IsDefault = p.IsDefault
                    }).ToList()
                };

                return View(vm);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return RedirectToAction(nameof(Phones));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePhoneNumber(ChangePhoneViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.PhoneTypesDropDown = await GeneratePhoneTypesSelectList();
                return View(vm);
            }

            try
            {
                // Make sure the user doesn't already have the same phone number
                if (await PhoneExists(vm.Number, vm.Ext, vm.PhoneID))
                    throw new Exception("Phone number already exists");

                var user = await _userManager.FindByIdAsync(User.GetUserId());
                var phone = await _phoneService.GetById(vm.PhoneID);

                // Make sure the phone number being changed belongs to this user
                if (phone.PersonEntityID != user.UserId)
                    throw new Exception("Phone number does not belong to user");

                // Remove phone from two factor if it's the same
                if (user.PhoneNumber == phone.PhoneNumber)
                    await _userManager.SetPhoneNumberAsync(user, null);

                phone.Id = vm.PhoneID;
                phone.Type = vm.PhoneType;
                phone.PhoneNumber = vm.Number;
                phone.Ext = vm.Ext;

                phone.Code = null;
                await _phoneService.Update(phone);

                ModelState.Clear();
                ModelState.AddModelError("success", "Phone number successfully changed");
                return RedirectToAction(nameof(Phones));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                vm.PhoneTypesDropDown = await GeneratePhoneTypesSelectList();
                return View(vm);
            }
        }

        [HttpGet]
        public async Task<ActionResult> SetDefaultPhone(int? id)
        {
            if (id == null)
                return RedirectToAction(nameof(Phones));

            try
            {
                var user = await _userManager.FindByIdAsync(User.GetUserId());
                var phone = await _phoneService.GetById(id.Value);

                // Make sure the phone belongs to this user
                if (phone.PersonEntityID != user.UserId)
                    throw new Exception("Phone does not belong to user");

                await _phoneService.SetDefault(user.UserId, id.Value);

                ModelState.Clear();
                ModelState.AddModelError("success", "Primary contact phone number changed");
                return RedirectToAction(nameof(Phones));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return RedirectToAction(nameof(Phones));
            }
        }

        [HttpGet]
        public async Task<ActionResult> RemovePhoneNumber(int? id)
        {
            if (id == null)
                return RedirectToAction(nameof(Phones));

            try
            {
                var user = await _userManager.FindByIdAsync(User.GetUserId());
                var phones = await _phoneService.GetByPerson(user.UserId, false);
                var phone = phones.SingleOrDefault(p => p.Id == id.Value);

                // Make sure the phone number exists and belongs to this user
                if (phone == null)
                    throw new Exception("Phone not found or does not belong to this user");

                // Make sure the phone is not primary
                if (phone.IsDefault)
                    throw new Exception("Cannot remove a phone that is set to primary");

                // Remove phone from two factor if it's the same
                if (user.PhoneNumber == phone.PhoneNumber)
                    await _userManager.SetPhoneNumberAsync(user, null);

                await _phoneService.Remove(phone);

                ModelState.Clear();
                ModelState.AddModelError("success", "Successfully removed phone number");
                return RedirectToAction(nameof(Phones));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return RedirectToAction(nameof(Phones));
            }
        }

        #endregion

        #region Emails

        [HttpGet]
        public async Task<ActionResult> Emails()
        {
            var user = await _userManager.FindByIdAsync(User.GetUserId());
            var emailsDM = await _emailService.GetByPerson(user.UserId, false);

            var vm = new EmailsViewModel
            {
                Emails = emailsDM.Select(e => new EmailListViewModel
                {
                    EmailID = e.Id,
                    TypeDescription = e.Code.Description,
                    EmailAddress = e.Email1,
                    IsDefault = e.IsDefault
                }).ToList()
            };

            return View(vm);
        }

        [HttpGet]
        public async Task<ActionResult> AddEmailAddress()
        {
            var user = await _userManager.FindByIdAsync(User.GetUserId());
            var emailsDM = await _emailService.GetByPerson(user.UserId, false);

            AddEmailViewModel vm = new AddEmailViewModel 
            {
                EmailTypesDropDown = await GenerateEmailTypesSelectList(),
                Emails = emailsDM.Select(e => new EmailListViewModel
                {
                    EmailID = e.Id,
                    TypeDescription = e.Code.Description,
                    EmailAddress = e.Email1,
                    IsDefault = e.IsDefault
                }).ToList()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddEmailAddress(AddEmailViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.EmailTypesDropDown = await GenerateEmailTypesSelectList();
                return View(vm);
            }

            try
            {
                // Make sure the email type is valid
                if (!await EmailTypeIsValid(vm.EmailType))
                    throw new Exception("Email type is invalid");

                // Make sure the user doesn't already have the same email address elsewhere
                if (await EmailExists(vm.Email))
                    throw new Exception("Email address already exists");

                var user = await _userManager.FindByIdAsync(User.GetUserId());
                var emails = await _emailService.GetByPerson(user.UserId, false);
                
                var email = new Email 
                { 
                    Type = vm.EmailType,
                    Email1 = vm.Email.ToLower(),
                    PersonEntityID = user.UserId,
                    Status = true
                };

                // If user's first email, set it to default
                if (emails.Count == 0)
                    email.IsDefault = true;

                await _emailService.Add(email);

                ModelState.Clear();
                ModelState.AddModelError("success", "Email address successfully added");
                return RedirectToAction(nameof(Emails));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                vm.EmailTypesDropDown = await GenerateEmailTypesSelectList();
                return View(vm);
            }
        }

        [HttpGet]
        public async Task<ActionResult> ChangeEmailAddress(int? id)
        {
            if (id == null)
                return RedirectToAction(nameof(Emails));

            try
            {
                var user = await _userManager.FindByIdAsync(User.GetUserId());
                var emailsDM = await _emailService.GetByPerson(user.UserId, false);
                var email = emailsDM.SingleOrDefault(e => e.Id == id.Value);

                // Make sure the email address exists and belongs to this user
                if (email == null)
                    throw new Exception("Email not found or does not belong to this user");

                var vm = new ChangeEmailViewModel
                {
                    EmailTypesDropDown = await GenerateEmailTypesSelectList(),
                    Emails = emailsDM.Select(e => new EmailListViewModel
                    {
                        EmailID = e.Id,
                        TypeDescription = e.Code.Description,
                        EmailAddress = e.Email1,
                        IsDefault = e.IsDefault
                    }).ToList(),
                    EmailID = email.Id,
                    EmailType = email.Type,
                    Email = email.Email1
                };

                return View(vm);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return RedirectToAction(nameof(Emails));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangeEmailAddress(ChangeEmailViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.EmailTypesDropDown = await GenerateEmailTypesSelectList();
                return View(vm);
            }

            try
            {
                // Make sure the user doesn't already have the same email address
                if (await EmailExists(vm.Email, vm.EmailID))
                    throw new Exception("Email address already exists");

                var user = await _userManager.FindByIdAsync(User.GetUserId());
                var email = await _emailService.GetById(vm.EmailID);

                // Make sure the email address being changed belongs to this user
                if (email.PersonEntityID != user.UserId)
                    throw new Exception("Email address does not belong to user");

                // Update email from two factor if it's the same
                if (user.Email == email.Email1 && vm.Email != email.Email1)
                    await _userManager.SetEmailAsync(user, vm.Email);

                email.Id = vm.EmailID;
                email.Type = vm.EmailType;
                email.Email1 = vm.Email.ToLower();

                email.Code = null;
                await _emailService.Update(email);

                var uri = new Uri(Request.Host.Value);
                var scheme = uri.Scheme;
                var callbackUrl = Url.Action("ConfirmEmail", "Account", null, scheme);
                _ = _userNotificationService.SendConfirmationEmailByUserId(user.UserId, callbackUrl);

                ModelState.Clear();
                ModelState.AddModelError("success", "Email address successfully changed");
                return RedirectToAction(nameof(Emails));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                vm.EmailTypesDropDown = await GenerateEmailTypesSelectList();
                return View(vm);
            }
        }

        [HttpGet]
        public async Task<ActionResult> SetDefaultEmail(int? id)
        {
            if (id == null)
                return RedirectToAction(nameof(Emails));

            try
            {
                var user = await _userManager.FindByIdAsync(User.GetUserId());
                var email = await _emailService.GetById(id.Value);

                // Make sure the email belongs to this user
                if (email.PersonEntityID != user.UserId)
                    throw new Exception("Email does not belong to user");

                await _emailService.SetDefault(user.UserId, id.Value);
                await _userManager.SetEmailAsync(user, email.Email1);

                var uri = new Uri(Request.Host.Value);
                var scheme = uri.Scheme;
                var callbackUrl = Url.Action("ConfirmEmail", "Account", null, scheme);
                _ = _userNotificationService.SendConfirmationEmailByUserId(user.UserId, callbackUrl);

                ModelState.Clear();
                ModelState.AddModelError("success", "Primary contact email address changed");
                return RedirectToAction(nameof(Emails));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return RedirectToAction(nameof(Emails));
            }
        }

        [HttpGet]
        public async Task<ActionResult> RemoveEmailAddress(int? id)
        {
            if (id == null)
                return RedirectToAction(nameof(Emails));

            try
            {
                var user = await _userManager.FindByIdAsync(User.GetUserId());
                var emails = await _emailService.GetByPerson(user.UserId, false);
                var email = emails.SingleOrDefault(p => p.Id == id.Value);

                // Make sure the email exists and belongs to this user
                if (email == null)
                    throw new Exception("Email not found or does not belong to this user");

                // Make sure the email is not primary
                if (email.IsDefault)
                    throw new Exception("Cannot remove an email that is set to primary");

                // Do not allow removal of email that is set for two factor
                if (user.Email == email.Email1)
                    throw new Exception("Cannot remove an email that is set for two factor authentication");

                await _emailService.Remove(email);

                ModelState.Clear();
                ModelState.AddModelError("success", "Successfully removed email address");
                return RedirectToAction(nameof(Emails));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return RedirectToAction(nameof(Emails));
            }
        }

        #endregion

        #region Addresses

        [HttpGet]
        public async Task<ActionResult> Addresses()
        {
            var user = await _userManager.FindByIdAsync(User.GetUserId());
            var addressesDM = (await _addressService.GetByPerson(user.UserId, false)).Where(a => a.Code.Description == "Billing");

            var vm = new AddressesViewModel
            {
                Addresses = addressesDM.Select(a => new AddressListViewModel
                {
                    AddressID = a.Id,
                    IsDefault = a.IsDefault,
                    Line1 = a.ToLine1String(),
                    Line2 = a.ToLine2String()
                }).ToList()
            };

            return View(vm);
        }

        [HttpGet]
        public async Task<ActionResult> AddAddress()
        {
            var user = await _userManager.FindByIdAsync(User.GetUserId());
            var addressesDM = (await _addressService.GetByPerson(user.UserId, false)).Where(a => a.Code.Description == "Billing");

            var vm = new AddAddressViewModel 
            {
                Addresses = addressesDM.Select(a => new AddressListViewModel
                {
                    AddressID = a.Id,
                    IsDefault = a.IsDefault,
                    Line1 = a.ToLine1String(),
                    Line2 = a.ToLine2String()
                }).ToList()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddAddress(AddAddressViewModel vm)
        {
            if (!vm.ValidationMode && !ModelState.IsValid)
                return View(vm);

            try
            {
                var user = await _userManager.FindByIdAsync(User.GetUserId());
                var addresses = await _addressService.GetByPerson(user.UserId, false);

                if (vm.ValidAddressSelect.HasValue)
                {
                    var address = vm.ValidAddresses.ElementAt(vm.ValidAddressSelect.Value);

                    // If user's first address, set it to default
                    if (addresses.Count == 0)
                        address.IsDefault = true;

                    address.Type = 9;
                    address.PersonEntityID = user.UserId;
                    await _addressService.Add(address);

                    ModelState.Clear();
                    ModelState.AddModelError("success", "Address successfully added");
                    return RedirectToAction(nameof(Addresses));
                }
                else
                {
                    vm.ValidationMode = true;
                    var validAddressList = await _addressValidationService.GetCandidates(vm.StreetAddress, vm.City, vm.Zip, 3);
                    var validAddresses = validAddressList?.Select(val => new Address
                    {
                        StreetName = val.Address,
                        City = val.City,
                        State = val.State,
                        Zip = val.Zip
                    }).ToList();

                    if (validAddresses == null || validAddresses.Count == 0)
                    {
                        vm.ValidationMode = false;

                        ModelState.Clear();
                        ModelState.AddModelError("", "No matching addresses could be found. Please correct the address or contact Solid Waste at 785-233-4774 if you are sure this is correct.");
                        return View(vm);
                    }
                    else if (validAddresses.Count > 1)
                    {
                        vm.ValidAddresses = validAddresses;

                        ModelState.Clear();
                        ModelState.AddModelError("", "Please select the correct address from the list.");
                        return View(vm);
                    }
                    else
                    {
                        var address = validAddresses.First();

                        // If user's first address, set it to default
                        if (addresses.Count == 0)
                            address.IsDefault = true;

                        address.Type = 9;
                        address.PersonEntityID = user.UserId;
                        await _addressService.Add(address);

                        ModelState.Clear();
                        ModelState.AddModelError("success", "Address successfully added");
                        return RedirectToAction(nameof(Addresses));
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(vm);
            }
        }

        [HttpGet]
        public async Task<ActionResult> ChangeAddress(int? id)
        {
            if (id == null)
                return RedirectToAction(nameof(Addresses));

            try
            {
                var user = await _userManager.FindByIdAsync(User.GetUserId());
                var addressesDM = (await _addressService.GetByPerson(user.UserId, false)).Where(a => a.Code.Description == "Billing");
                var address = addressesDM.SingleOrDefault(p => p.Id == id.Value);

                // Make sure the address exists and belongs to this user
                if (address == null)
                    throw new Exception("Address not found or does not belong to this user");

                var vm = new ChangeAddressViewModel
                {
                    Addresses = addressesDM.Select(a => new AddressListViewModel
                    {
                        AddressID = a.Id,
                        IsDefault = a.IsDefault,
                        Line1 = a.ToLine1String(),
                        Line2 = a.ToLine2String()
                    }).ToList(),
                    AddressId = address.Id,
                    StreetAddress = address.ToLine1String(),
                    City = address.City,
                    State = address.State,
                    Zip = address.Zip
                };

                return View(vm);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return RedirectToAction(nameof(Addresses));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangeAddress(ChangeAddressViewModel vm)
        {
            if (!vm.ValidationMode && !ModelState.IsValid)
                return View(vm);

            try
            {
                var user = await _userManager.FindByIdAsync(User.GetUserId());
                var userAddress = await _addressService.GetById(vm.AddressId);

                // Make sure the address being changed belongs to this user
                if (userAddress.PersonEntityID != user.UserId)
                    throw new Exception("Address does not belong to user");

                if (vm.ValidAddressSelect.HasValue)
                {
                    var address = vm.ValidAddresses.ElementAt(vm.ValidAddressSelect.Value);

                    address.AddDateTime = userAddress.AddDateTime;
                    address.AddToi = userAddress.AddToi;
                    address.Type = userAddress.Type;
                    address.PersonEntityID = userAddress.PersonEntityID;
                    address.IsDefault = userAddress.IsDefault;
                    address.Id = userAddress.Id;
                    await _addressService.Update(address);

                    ModelState.Clear();
                    ModelState.AddModelError("success", "Address successfully changed");
                    return RedirectToAction(nameof(Addresses));
                }
                else
                {
                    vm.ValidationMode = true;
                    var validAddressList = await _addressValidationService.GetCandidates(vm.StreetAddress, vm.City, vm.Zip, 3);
                    var validAddresses = validAddressList?.Select(val => new Address
                    {
                        StreetName = val.Address,
                        City = val.City,
                        State = val.State,
                        Zip = val.Zip
                    }).ToList();

                    if (validAddresses == null || validAddresses.Count == 0)
                    {
                        vm.ValidationMode = false;

                        ModelState.Clear();
                        ModelState.AddModelError("", "No matching addresses could be found. Please correct the address or contact Solid Waste at 785-233-4774 if you are sure this is correct.");
                        return View(vm);
                    }
                    else if (validAddresses.Count > 1)
                    {
                        vm.ValidAddresses = validAddresses;

                        ModelState.Clear();
                        ModelState.AddModelError("", "Please select the correct address from the list.");
                        return View(vm);
                    }
                    else
                    {
                        var address = validAddresses.First();

                        address.AddDateTime = userAddress.AddDateTime;
                        address.AddToi = userAddress.AddToi;
                        address.Type = userAddress.Type;
                        address.PersonEntityID = userAddress.PersonEntityID;
                        address.IsDefault = userAddress.IsDefault;
                        address.Id = userAddress.Id;
                        await _addressService.Update(address);

                        var pe = await _personEntityService.GetById(user.UserId);

                        if (pe.Pab == true)
                        {
                            var customer = await _customerService.GetByPE(user.UserId);

                            var requestNumber = address.Number.ToString();
                            var requestDirection = address.Direction;
                            var requestStreetName = address.StreetName;
                            var requestStreetSuffix = address.Suffix;
                            var requestApt = address.Apt;
                            var requestCity = address.City;
                            var requestState = address.State;
                            var requestZip = address.Zip.ToString();
                            var body = GenerateBadAddressChangeRequest(customer.CustomerId, requestNumber, requestDirection, requestStreetName, requestStreetSuffix, requestApt, requestCity, requestState, requestZip);

                            var email = new SendEmailDto
                            {
                                HtmlContent = body,
                                Subject = "User requesting address change that had previous bad address flag",
                            }
                            .SetFrom("no-reply.scsw@sncoapps.us")
                            .AddTo("andrew.moomau@snco.us");

                            _ = _sendGridService.SendSingleEmail(email);
                        }
                    }
                }

                ModelState.Clear();
                ModelState.AddModelError("success", "Address successfully changed");
                return RedirectToAction(nameof(Addresses));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(vm);
            }
        }

        [HttpGet]
        public async Task<ActionResult> SetDefaultAddress(int? id)
        {
            if (id == null)
                return RedirectToAction(nameof(Addresses));

            try
            {
                var user = await _userManager.FindByIdAsync(User.GetUserId());
                var address = await _addressService.GetById(id.Value);
                var pe = await _personEntityService.GetById(user.UserId);

                // Make sure the address belongs to this user
                if (address.PersonEntityID != user.UserId)
                    throw new Exception("Address does not belong to user");

                await _addressService.SetDefault(user.UserId, id.Value);

                if (pe.Pab == true)
                {
                    var customer = await _customerService.GetByPE(user.UserId);

                    var requestNumber = address.Number.ToString();
                    var requestDirection = address.Direction;
                    var requestStreetName = address.StreetName;
                    var requestStreetSuffix = address.Suffix;
                    var requestApt = address.Apt;
                    var requestCity = address.City;
                    var requestState = address.State;
                    var requestZip = address.Zip.ToString();
                    var body = GenerateBadAddressChangeRequest(customer.CustomerId, requestNumber, requestDirection, requestStreetName, requestStreetSuffix, requestApt, requestCity, requestState, requestZip);

                    var email = new SendEmailDto
                    {
                        HtmlContent = body,
                        Subject = "User requesting address change that had previous bad address flag",
                    }
                    .SetFrom("no-reply.scsw@sncoapps.us")
                    .AddTo("andrew.moomau@snco.us");

                    _ = _sendGridService.SendSingleEmail(email);
                }

                ModelState.Clear();
                ModelState.AddModelError("success", "Primary mailing address changed");
                return RedirectToAction(nameof(Addresses));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return RedirectToAction(nameof(Addresses));
            }
        }

        [HttpGet]
        public async Task<ActionResult> RemoveAddress(int? id)
        {
            if (id == null)
                return RedirectToAction(nameof(Addresses));

            try
            {
                var user = await _userManager.FindByIdAsync(User.GetUserId());
                var addresses = await _addressService.GetByPerson(user.UserId, false);
                var address = addresses.SingleOrDefault(p => p.Id == id.Value);

                // Make sure the address exists and belongs to this user
                if (address == null)
                    throw new Exception("Address not found or does not belong to this user");

                // Make sure the address is not primary
                if (address.IsDefault)
                    throw new Exception("Cannot remove an address that is set to primary");

                await _addressService.Remove(address);

                ModelState.Clear();
                ModelState.AddModelError("success", "Successfully removed address");
                return RedirectToAction(nameof(Addresses));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return RedirectToAction(nameof(Addresses));
            }
        }

        #endregion

        #region Delivery

        [HttpGet]
        public async Task<ActionResult> Delivery()
        {
            try
            {
                var user = await _userManager.FindByIdAsync(User.GetUserId());
                var person = await _personEntityService.GetById(user.UserId);

                var vm = new DeliveryViewModel 
                { 
                    DeliveryOption = !person.PaperLess.HasValue ? "3" : person.PaperLess.Value ? "1" : "2"
                };

                return View(vm);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return RedirectToAction(nameof(Delivery));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delivery(DeliveryViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            try
            {
                var user = await _userManager.FindByIdAsync(User.GetUserId());
                var person = await _personEntityService.GetById(user.UserId);
                var defaultEmail = (await _emailService.GetByPerson(user.UserId, false)).SingleOrDefault(e => e.IsDefault);

                person.PaperLess = vm.DeliveryOption == "1" ? true : vm.DeliveryOption == "2" ? false : null;

                await _personEntityService.Update(person);

                var deliveryOption = vm.DeliveryOption == "1"
                    ? "Email Billing Only"
                    : vm.DeliveryOption == "2" ? "Paper Billing Only" : "Email and Paper Billing";
                var body = GenerateDeliveryOptionBody(deliveryOption);

                var email = new SendEmailDto
                {
                    HtmlContent = body,
                    Subject = "Bill Delivery Options Changed",
                }
                .SetFrom("no-reply.scsw@sncoapps.us")
                .AddTo(defaultEmail.Email1);

                _ = _sendGridService.SendSingleEmail(email);

                ModelState.Clear();
                ModelState.AddModelError("success", "Delivery options successfully changed");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("danger", ex.Message);
                return View(vm);
            }
        }

        #endregion

        #region Profile

        [HttpGet]
        public async Task<ActionResult> Profile()
        {
            var user = await _userManager.FindByIdAsync(User.GetUserId());
            var person = await _personEntityService.GetById(user.UserId);

            if (person.NameTypeFlag)
            {
                var vm = new BusinessProfileViewModel 
                { 
                    Name = person.FullName,
                    Url = person.WebUrl,
                    WhenCreated = person.WhenCreated
                };
                return View("BusinessProfile", vm);
            }
            else
            {
                var vm = new PersonalProfileViewModel
                {
                    FirstName = person.FirstName,
                    MiddleName = person.MiddleName,
                    LastName = person.LastName,
                    Sex = person.Sex,
                    DateOfBirth = person.DateOfBirth,
                    Url = person.WebUrl,
                    WhenCreated = person.WhenCreated
                };
                return View("PersonalProfile", vm);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> BusinessProfile(BusinessProfileViewModel vm)
        {
            if (!ModelState.IsValid)
                return View("BusinessProfile", vm);

            try
            {
                var user = await _userManager.FindByIdAsync(User.GetUserId());
                var person = await _personEntityService.GetById(user.UserId);

                person.FullName = vm.Name;
                person.WebUrl = vm.Url;

                await _personEntityService.Update(person);

                ModelState.Clear();
                ModelState.AddModelError("success", "Profile successfully updated");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("BusinessProfile", vm);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> PersonalProfile(PersonalProfileViewModel vm)
        {
            if (!ModelState.IsValid)
                return View("PersonalProfile", vm);

            try
            {
                var user = await _userManager.FindByIdAsync(User.GetUserId());
                var person = await _personEntityService.GetById(user.UserId);

                person.FirstName = vm.FirstName;
                person.MiddleName = vm.MiddleName;
                person.LastName = vm.LastName;
                person.Sex = vm.Sex;
                person.DateOfBirth = vm.DateOfBirth;
                person.WebUrl = vm.Url;

                await _personEntityService.Update(person);

                ModelState.Clear();
                ModelState.AddModelError("success", "Profile successfully updated");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("PersonalProfile", vm);
            }
        }

        #endregion

        #region Helpers

        private async Task SignInAsync(ApplicationUser user, bool isPersistent)
        {
            await _signInManager.SignOutAsync();
            await _signInManager.SignInAsync(user, isPersistent);
        }

        [NonAction]
        private async Task<bool> PhoneExists(string phoneNumber, string phoneExt)
        {
            var user = await _userManager.FindByIdAsync(User.GetUserId());
            var phones = await _phoneService.GetByPerson(user.UserId, false);

            return phones.Any(p => p.PhoneNumber == phoneNumber && p.Ext == phoneExt);
        }

        [NonAction]
        private async Task<bool> PhoneExists(string phoneNumber, string phoneExt, int phoneID)
        {
            var user = await _userManager.FindByIdAsync(User.GetUserId());
            var phones = await _phoneService.GetByPerson(user.UserId, false);

            return phones.Any(p => p.PhoneNumber == phoneNumber && p.Ext == phoneExt && p.Id != phoneID);
        }

        [NonAction]
        private async Task<bool> EmailExists(string emailAddress)
        {
            var user = await _userManager.FindByIdAsync(User.GetUserId());
            var emails = await _emailService.GetByPerson(user.UserId, false);

            return emails.Any(e => e.Email1.ToLower() == emailAddress.ToLower());
        }

        [NonAction]
        private async Task<bool> EmailExists(string emailAddress, int emailID)
        {
            var user = await _userManager.FindByIdAsync(User.GetUserId());
            var emails = await _emailService.GetByPerson(user.UserId, false);

            return emails.Any(e => e.Email1.ToLower() == emailAddress.ToLower() && e.Id != emailID);
        }

        [NonAction]
        private async Task<bool> PhoneTypeIsValid(int phoneType)
        {
            var codes = await _codeService.GetByType("Phone", false);

            return codes.Any(c => c.Id == phoneType);
        }

        [NonAction]
        private async Task<bool> EmailTypeIsValid(int emailType)
        {
            var codes = await _codeService.GetByType("Email", false);

            return codes.Any(c => c.Id == emailType);
        }

        [NonAction]
        private async Task<IEnumerable<SelectListItem>> GeneratePhoneTypesSelectList()
        {
            var codes = await _codeService.GetByType("Phone", false);

            var phoneTypes = codes
                .Where(c => c.Description != "Fax")
                .Select(c => new SelectListItem { Text = c.Description, Value = c.Id.ToString() })
                .ToList();
            phoneTypes.Insert(0, new SelectListItem { Text = "Choose phone type", Value = "0" });

            return phoneTypes;
        }

        [NonAction]
        private async Task<IEnumerable<SelectListItem>> GenerateEmailTypesSelectList()
        {
            var codes = await _codeService.GetByType("Email", false);

            var emailTypes = codes.Select(c => new SelectListItem { Text = c.Description, Value = c.Id.ToString() }).ToList();
            emailTypes.Insert(0, new SelectListItem { Text = "Choose email type", Value = "0" });

            return emailTypes;
        }

        [NonAction]
        private async Task<IEnumerable<SelectListItem>> GenerateEmailsSelectList(int peID)
        {
            var emails = await _emailService.GetByPerson(peID, false);

            return emails.Select(e => new SelectListItem { Text = e.Email1, Value = e.Id.ToString() });
        }

        [NonAction]
        private async Task<IEnumerable<SelectListItem>> GeneratePhonesSelectList(int peID)
        {
            var phones = await _phoneService.GetByPerson(peID, false);

            var selectList = phones.Select(p => new SelectListItem
            {
                Text = string.Format("{0}{1}", p.PhoneNumber, p.Ext == null ? string.Empty : " x" + p.Ext),
                Value = p.Id.ToString()
            }).ToList();
            selectList.Insert(0, new SelectListItem { Text = "None", Value = "0" });

            return selectList;
        }

        [NonAction]
        private static string GenerateBadAddressChangeRequest(int CustomerID, string requestNumber, string requestDirection, string requestStreetName, string requestStreetSuffix, string requestApt, string requestCity, string requestState, string requestZip)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<div style=\"background-color:#909EB8\">");
            sb.AppendLine("     <div align=\"center\">");
            sb.AppendLine("         <div style=\"background-color:#344479; min-height:100px; padding-left: 20px; padding-right: 20px; padding-top: 28px; height: 100px; min-width: 100%; display:table; overflow:hidden;\">");
            sb.AppendLine("             <div style=\"display: table-cell; vertical-align: middle; font-size: 40px; color:white; font-family: Verdana;\">");
            sb.AppendLine("                 <div align=\"center\">");
            sb.AppendLine("                     Shawnee County Solid Waste");
            sb.AppendLine("                 </div>");
            sb.AppendLine("             </div>");
            sb.AppendLine("         </div>");
            sb.AppendLine("     </div>");
            sb.AppendLine("     <div align=\"center\">");
            sb.AppendLine("        <div style=\"padding: 20px 20px 20px 20px; background-color: #D4D4D4; width: 50%;\">");
            sb.AppendLine("            <div style=\"padding: 20px 20px 20px 20px; background-color: #FFFFFF;\">");
            sb.AppendLine("                <div style=\"display: block; margin-bottom: 20px;\">");
            sb.AppendLine($"                        <text style=\"padding: 8px 15px 8px 15px; text-align:center; background-color: #6EBF4A; color: #FFFFFF; font-weight: bold; text-decoration: none;\">User {CustomerID} is requesting a change to a previous bad address.</text>");
            sb.AppendLine("                </div>");
            sb.AppendLine("                <div style=\"display: block; margin-bottom: 20px; text-align: center;\">");
            sb.AppendLine("                     <div align=\"center\">");
            sb.AppendLine($"                        <text style=\"padding: 8px 15px 8px 15px; text-align:center; background-color: #6EBF4A; color: #FFFFFF; font-weight: bold; text-decoration: none;\">New Address Requested: </text>");
            sb.AppendLine("                    </div>");
            sb.AppendLine("                     <div align=\"center\">");
            sb.AppendLine($"                        <text style=\"padding: 8px 15px 8px 15px; text-align:center; background-color: #6EBF4A; color: #FFFFFF; font-weight: bold; text-decoration: none;\">{requestNumber} {requestDirection} {requestStreetName} {requestStreetSuffix} {requestApt} </text>");
            sb.AppendLine("                    </div>");
            sb.AppendLine("                     <div align=\"center\">");
            sb.AppendLine($"                        <text style=\"padding: 8px 15px 8px 15px; text-align:center; background-color: #6EBF4A; color: #FFFFFF; font-weight: bold; text-decoration: none;\">{requestCity} {requestState} {requestZip} </text>");
            sb.AppendLine("                    </div>");
            sb.AppendLine("                 </div>");
            sb.AppendLine("            </div>");
            sb.AppendLine("<hr>");
            sb.AppendLine("</div>");
            sb.AppendLine("</div>");
            return sb.ToString();
        }

        [NonAction]
        private static string GenerateDeliveryOptionBody(string deliveryOption)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<div style=\"background-color:#909EB8\">");
            sb.AppendLine("     <div align=\"center\">");
            sb.AppendLine("         <div style=\"background-color:#344479; min-height:100px; padding-left: 20px; padding-right: 20px; padding-top: 28px; height: 100px; min-width: 100%; display:table; overflow:hidden;\">");
            sb.AppendLine("             <div style=\"display: table-cell; vertical-align: middle; font-size: 40px; color:white; font-family: Verdana;\">");
            sb.AppendLine("                 <div align=\"center\">");
            sb.AppendLine("                     Shawnee County Solid Waste");
            sb.AppendLine("                 </div>");
            sb.AppendLine("             </div>");
            sb.AppendLine("         </div>");
            sb.AppendLine("     </div>");
            sb.AppendLine("     <div align=\"center\">");
            sb.AppendLine("        <div style=\"padding: 20px 20px 20px 20px; background-color: #D4D4D4; width: 50%;\">");
            sb.AppendLine("            <div style=\"padding: 20px 20px 20px 20px; background-color: #FFFFFF;\">");
            sb.AppendLine("                <div style=\"display: block; margin-bottom: 20px;\">");
            sb.AppendLine("                    This email is to notify you that your account's delivery options have been changed to:");
            sb.AppendLine("                </div>");
            sb.AppendLine("                <div style=\"display: block; margin-bottom: 20px; text-align: center;\">");
            sb.AppendLine("                     <div align=\"center\">");
            sb.AppendLine($"                        <text style=\"padding: 8px 15px 8px 15px; text-align:center; background-color: #6EBF4A; color: #FFFFFF; font-weight: bold; text-decoration: none;\">{deliveryOption}</text>");
            sb.AppendLine("                    </div>");
            sb.AppendLine("                 </div>");
            sb.AppendLine("            </div>");
            sb.AppendLine("         <div align=\"center\" style=\"padding-top:20px\">");
            sb.AppendLine("             <div style=\"font-size:12px;\">");
            sb.AppendLine("                 DO NOT REPLY TO THIS EMAIL. If you've received this email in error, please notify us by telephone at 785.233.4774 or by email at solidwaste@snco.us.");
            sb.AppendLine("         </div>");
            sb.AppendLine("     </div>");
            sb.AppendLine("<hr>");
            sb.AppendLine("     <div width=\"50%\">");
            sb.AppendLine("         Visit the Shawnee County Website at http://www.snco.us");
            sb.AppendLine("     </div>");
            sb.AppendLine("     <div width=\"50%\">");
            sb.AppendLine("         Questions? Contact Solid Waste at 785.233.4774 (voice) 785.291.4928(fax)");
            sb.AppendLine("     </div>");
            sb.AppendLine("</div>");
            sb.AppendLine("</div>");
            return sb.ToString();
        }

        #endregion
    }
}
