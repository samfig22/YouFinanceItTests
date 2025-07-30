using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using YouFinanceIt.Controllers;
using YouFinanceIt.Data;
using YouFinanceIt.Models;
using YouFinanceIt.Models.ViewModels;
using IdentitySignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace YouFinanceIt.Tests.Controllers
{
    public class AccountControllerTests
    {
        private Mock<UserManager<ApplicationUser>> GetMockUserManager()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            var mgr = new Mock<UserManager<ApplicationUser>>(store.Object, null, null, null, null, null, null, null, null);
            return mgr;
        }

        private Mock<SignInManager<ApplicationUser>> GetMockSignInManager(Mock<UserManager<ApplicationUser>> userManager)
        {
            var contextAccessor = new Mock<Microsoft.AspNetCore.Http.IHttpContextAccessor>();
            var claimsFactory = new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();
            return new Mock<SignInManager<ApplicationUser>>(userManager.Object, contextAccessor.Object, claimsFactory.Object, null, null, null, null);
        }

        private ApplicationDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("TestDb_" + System.Guid.NewGuid())
                .Options;
            return new ApplicationDbContext(options);
        }

        [Fact]
        public void Register_Get_ReturnsViewResult()
        {
            // Arrange
            var userManager = GetMockUserManager();
            var signInManager = GetMockSignInManager(userManager);
            var context = GetInMemoryDbContext();
            var logger = new Mock<ILogger<AccountController>>();

            var controller = new AccountController(userManager.Object, signInManager.Object, context, logger.Object);

            // Act
            var result = controller.Register();

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task Register_Post_ReturnsRedirectToLogin_OnSuccess()
        {
            // Arrange
            var userManager = GetMockUserManager();
            var signInManager = GetMockSignInManager(userManager);
            var context = GetInMemoryDbContext();
            var logger = new Mock<ILogger<AccountController>>();

            var user = new ApplicationUser { Id = "user123", UserName = "test@example.com", Email = "test@example.com" };

            userManager.Setup(u => u.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success)
                .Callback<ApplicationUser, string>((usr, pwd) => user.Id = "user123");

            var controller = new AccountController(userManager.Object, signInManager.Object, context, logger.Object);

            // Setup TempData to avoid null reference
            var tempData = new Microsoft.AspNetCore.Mvc.ViewFeatures.TempDataDictionary(
                new DefaultHttpContext(),
                Mock.Of<Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataProvider>());
            controller.TempData = tempData;

            var model = new RegisterViewModel
            {
                Email = "test@example.com",
                Password = "Password123!",
                ConfirmPassword = "Password123!"
            };

            // Act
            var result = await controller.Register(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectResult.ActionName);
            Assert.Equal("Account", redirectResult.ControllerName);
        }


        [Fact]
        public async Task Register_Post_ReturnsViewResult_WhenModelStateInvalid()
        {
            // Arrange
            var userManager = GetMockUserManager();
            var signInManager = GetMockSignInManager(userManager);
            var context = GetInMemoryDbContext();
            var logger = new Mock<ILogger<AccountController>>();

            var controller = new AccountController(userManager.Object, signInManager.Object, context, logger.Object);
            controller.ModelState.AddModelError("Email", "Required");

            var model = new RegisterViewModel();

            // Act
            var result = await controller.Register(model);

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void Login_Get_ReturnsViewResult()
        {
            // Arrange
            var userManager = GetMockUserManager();
            var signInManager = GetMockSignInManager(userManager);
            var context = GetInMemoryDbContext();
            var logger = new Mock<ILogger<AccountController>>();

            var controller = new AccountController(userManager.Object, signInManager.Object, context, logger.Object);

            // Act
            var result = controller.Login();

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task Login_Post_ReturnsRedirectToDashboard_OnSuccess()
        {
            // Arrange
            var userManager = GetMockUserManager();
            var signInManager = GetMockSignInManager(userManager);
            var context = GetInMemoryDbContext();
            var logger = new Mock<ILogger<AccountController>>();

            signInManager.Setup(s => s.PasswordSignInAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(IdentitySignInResult.Success);

            var controller = new AccountController(userManager.Object, signInManager.Object, context, logger.Object);

            var model = new LoginViewModel
            {
                Email = "test@example.com",
                Password = "Password123!",
                RememberMe = false
            };

            // Act
            var result = await controller.Login(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("Dashboard", redirectResult.ControllerName);
        }

        [Fact]
        public async Task Login_Post_ReturnsViewResult_WhenModelStateInvalid()
        {
            // Arrange
            var userManager = GetMockUserManager();
            var signInManager = GetMockSignInManager(userManager);
            var context = GetInMemoryDbContext();
            var logger = new Mock<ILogger<AccountController>>();

            var controller = new AccountController(userManager.Object, signInManager.Object, context, logger.Object);
            controller.ModelState.AddModelError("Email", "Required");

            var model = new LoginViewModel();

            // Act
            var result = await controller.Login(model);

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task Logout_Post_RedirectsToLogin()
        {
            // Arrange
            var userManager = GetMockUserManager();
            var signInManager = GetMockSignInManager(userManager);
            var context = GetInMemoryDbContext();
            var logger = new Mock<ILogger<AccountController>>();

            signInManager.Setup(s => s.SignOutAsync()).Returns(Task.CompletedTask);

            var controller = new AccountController(userManager.Object, signInManager.Object, context, logger.Object);

            // Act
            var result = await controller.Logout();

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectResult.ActionName);
            Assert.Equal("Account", redirectResult.ControllerName);
        }

        [Fact]
        public void AccessDenied_Get_ReturnsViewResult()
        {
            // Arrange
            var userManager = GetMockUserManager();
            var signInManager = GetMockSignInManager(userManager);
            var context = GetInMemoryDbContext();
            var logger = new Mock<ILogger<AccountController>>();

            var controller = new AccountController(userManager.Object, signInManager.Object, context, logger.Object);

            // Act
            var result = controller.AccessDenied();

            // Assert
            Assert.IsType<ViewResult>(result);
        }
    }
}
