using FluentAssertions;
using FunnyActivities.Application.Behaviors;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Security.Claims;
using Xunit;

namespace FunnyActivities.Application.UnitTests.Behaviors
{
    /// <summary>
    /// Unit tests for AuthorizationBehavior.
    /// Tests authorization checks, policy evaluation, and access control scenarios.
    /// </summary>
    public class AuthorizationBehaviorTests
    {
        private readonly Mock<IAuthorizationService> _authorizationServiceMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly AuthorizationBehavior<TestRequest, TestResponse> _behavior;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizationBehaviorTests"/> class.
        /// </summary>
        public AuthorizationBehaviorTests()
        {
            _authorizationServiceMock = new Mock<IAuthorizationService>();
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            _behavior = new AuthorizationBehavior<TestRequest, TestResponse>(
                _authorizationServiceMock.Object,
                _httpContextAccessorMock.Object);
        }

        #region Authorized Requests

        [Fact]
        public async Task Handle_AuthorizedRequest_ShouldContinuePipeline()
        {
            // Arrange
            var request = new TestRequest();
            var response = new TestResponse();
            var nextCalled = false;
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "testuser") }));
            var httpContext = new DefaultHttpContext { User = user };

            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);
            _authorizationServiceMock.Setup(x => x.AuthorizeAsync(user, "CanCreateMaterial"))
                .ReturnsAsync(AuthorizationResult.Success());

            RequestHandlerDelegate<TestResponse> next = (RequestHandlerDelegate<TestResponse>)(Delegate)(() =>
            {
                nextCalled = true;
                return Task.FromResult(response);
            });

            // Act
            var result = await _behavior.Handle(request, next, CancellationToken.None);

            // Assert
            result.Should().Be(response);
            nextCalled.Should().BeTrue();
            _authorizationServiceMock.Verify(x => x.AuthorizeAsync(user, "CanCreateMaterial"), Times.Once);
        }

        [Fact]
        public async Task Handle_RequestWithNoRequiredPolicy_ShouldContinuePipeline()
        {
            // Arrange
            var request = new TestRequest { RequestType = "UnknownRequest" };
            var response = new TestResponse();
            var nextCalled = false;
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "testuser") }));
            var httpContext = new DefaultHttpContext { User = user };

            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

            RequestHandlerDelegate<TestResponse> next = (RequestHandlerDelegate<TestResponse>)(Delegate)(() =>
            {
                nextCalled = true;
                return Task.FromResult(response);
            });

            // Act
            var result = await _behavior.Handle(request, next, CancellationToken.None);

            // Assert
            result.Should().Be(response);
            nextCalled.Should().BeTrue();
            _authorizationServiceMock.Verify(x => x.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<string>()), Times.Never);
        }

        #endregion

        #region Unauthorized Requests

        [Fact]
        public async Task Handle_UnauthorizedRequest_ShouldThrowUnauthorizedAccessException()
        {
            // Arrange
            var request = new TestRequest();
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "testuser") }));
            var httpContext = new DefaultHttpContext { User = user };

            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);
            _authorizationServiceMock.Setup(x => x.AuthorizeAsync(user, "CanCreateMaterial"))
                .ReturnsAsync(AuthorizationResult.Failed());

            RequestHandlerDelegate<TestResponse> next = (RequestHandlerDelegate<TestResponse>)(Delegate)(() => Task.FromResult(new TestResponse()));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _behavior.Handle(request, next, CancellationToken.None));

            exception.Message.Should().Contain("Access denied");
            exception.Message.Should().Contain("CanCreateMaterial");
            _authorizationServiceMock.Verify(x => x.AuthorizeAsync(user, "CanCreateMaterial"), Times.Once);
        }

        [Fact]
        public async Task Handle_UnauthorizedRequest_ShouldNotContinuePipeline()
        {
            // Arrange
            var request = new TestRequest();
            var nextCalled = false;
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "testuser") }));
            var httpContext = new DefaultHttpContext { User = user };

            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);
            _authorizationServiceMock.Setup(x => x.AuthorizeAsync(user, "CanCreateMaterial"))
                .ReturnsAsync(AuthorizationResult.Failed());

            RequestHandlerDelegate<TestResponse> next = (RequestHandlerDelegate<TestResponse>)(Delegate)(() =>
            {
                nextCalled = true;
                return Task.FromResult(new TestResponse());
            });

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _behavior.Handle(request, next, CancellationToken.None));

            nextCalled.Should().BeFalse();
        }

        #endregion

        #region User Context Issues

        [Fact]
        public async Task Handle_NoHttpContext_ShouldThrowUnauthorizedAccessException()
        {
            // Arrange
            var request = new TestRequest();

            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext)null);

            RequestHandlerDelegate<TestResponse> next = (RequestHandlerDelegate<TestResponse>)(Delegate)(() => Task.FromResult(new TestResponse()));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _behavior.Handle(request, next, CancellationToken.None));

            exception.Message.Should().Contain("User context is not available");
            _authorizationServiceMock.Verify(x => x.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Handle_NoUserInContext_ShouldThrowUnauthorizedAccessException()
        {
            // Arrange
            var request = new TestRequest();
            var httpContext = new DefaultHttpContext { User = null };

            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

            RequestHandlerDelegate<TestResponse> next = (RequestHandlerDelegate<TestResponse>)(Delegate)(() => Task.FromResult(new TestResponse()));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _behavior.Handle(request, next, CancellationToken.None));

            exception.Message.Should().Contain("User context is not available");
            _authorizationServiceMock.Verify(x => x.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<string>()), Times.Never);
        }

        #endregion

        #region Policy Mapping Tests

        [Theory]
        [InlineData("BulkCreateMaterialsCommand", "CanCreateMaterial")]
        [InlineData("UpdateMaterialCommand", "CanUpdateMaterial")]
        [InlineData("BulkUpdateMaterialsCommand", "CanUpdateMaterial")]
        [InlineData("DeleteMaterialCommand", "CanDeleteMaterial")]
        [InlineData("UploadMaterialPhotosCommand", "CanManagePhotos")]
        [InlineData("DeleteMaterialPhotoCommand", "CanManagePhotos")]
        [InlineData("GetMaterialQuery", "CanViewMaterial")]
        [InlineData("GetMaterialsQuery", "CanViewMaterial")]
        [InlineData("GetMaterialPhotosQuery", "CanViewMaterial")]
        [InlineData("DownloadMaterialPhotoQuery", "CanViewMaterial")]
        public async Task Handle_CommandType_ShouldMapToCorrectPolicy(string requestType, string expectedPolicy)
        {
            // Arrange
            var request = new TestRequest { RequestType = requestType };
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "testuser") }));
            var httpContext = new DefaultHttpContext { User = user };

            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);
            _authorizationServiceMock.Setup(x => x.AuthorizeAsync(user, expectedPolicy))
                .ReturnsAsync(AuthorizationResult.Success());

            RequestHandlerDelegate<TestResponse> next = (RequestHandlerDelegate<TestResponse>)(Delegate)(() => Task.FromResult(new TestResponse()));

            // Act
            await _behavior.Handle(request, next, CancellationToken.None);

            // Assert
            _authorizationServiceMock.Verify(x => x.AuthorizeAsync(user, expectedPolicy), Times.Once);
        }

        #endregion

        #region Edge Cases

        [Fact]
        public async Task Handle_AuthorizationServiceThrowsException_ShouldPropagateException()
        {
            // Arrange
            var request = new TestRequest();
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "testuser") }));
            var httpContext = new DefaultHttpContext { User = user };

            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);
            _authorizationServiceMock.Setup(x => x.AuthorizeAsync(user, "CanCreateMaterial"))
                .ThrowsAsync(new Exception("Authorization service error"));

            RequestHandlerDelegate<TestResponse> next = (RequestHandlerDelegate<TestResponse>)(Delegate)(() => Task.FromResult(new TestResponse()));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _behavior.Handle(request, next, CancellationToken.None));
        }

        #endregion
    }

    // Test classes
    public class TestRequest : IRequest<TestResponse>
    {
        public string RequestType { get; set; } = "BulkCreateMaterialsCommand";
    }

    public class TestResponse { }
}