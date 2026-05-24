using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using TractorEcommerce.Api.Controllers;
using Xunit;

namespace TractorEcommerce.Modules.Catalog.Tests.Controllers
{
    public class AuthControllerTests
    {
        private readonly AuthController _controller;
        private readonly IConfiguration _configuration;

        public AuthControllerTests()
        {
            _configuration = Substitute.For<IConfiguration>();
            // Return a fixed 32-char secret so the controller doesn't fall back to default
            _configuration["Jwt:Secret"].Returns("TestSecret32CharXXXXXXXXXXXXXXX!");
            _controller = new AuthController(_configuration);
        }

        [Fact]
        public void Login_WithValidCredentials_ReturnsOkWithToken()
        {
            // Arrange
            var request = new AuthController.LoginRequest("adminuser", "password123");

            // Act
            var result = _controller.Login(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<AuthController.LoginResponse>(okResult.Value);
            Assert.Equal("adminuser", response.Username);
            Assert.False(string.IsNullOrWhiteSpace(response.Token));
        }

        [Fact]
        public void Login_WithEmptyUsername_ReturnsBadRequest()
        {
            // Arrange
            var request = new AuthController.LoginRequest("", "password123");

            // Act
            var result = _controller.Login(request);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public void Login_WithEmptyPassword_ReturnsBadRequest()
        {
            // Arrange
            var request = new AuthController.LoginRequest("adminuser", "");

            // Act
            var result = _controller.Login(request);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public void Login_TokenContainsJwtFormat_ValidStructure()
        {
            // Arrange
            var request = new AuthController.LoginRequest("tractor_admin", "pass");

            // Act
            var result = _controller.Login(request);
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<AuthController.LoginResponse>(okResult.Value);

            // Assert — JWT format: header.payload.signature (3 parts separated by dots)
            var parts = response.Token.Split('.');
            Assert.Equal(3, parts.Length);
        }

        [Fact]
        public void Login_DifferentUsers_ReturnDifferentTokens()
        {
            // Arrange & Act
            var result1 = _controller.Login(new AuthController.LoginRequest("alice", "pass1"));
            var result2 = _controller.Login(new AuthController.LoginRequest("bob", "pass2"));

            var token1 = ((AuthController.LoginResponse)((OkObjectResult)result1.Result!).Value!).Token;
            var token2 = ((AuthController.LoginResponse)((OkObjectResult)result2.Result!).Value!).Token;

            // Assert
            Assert.NotEqual(token1, token2);
        }
    }
}
