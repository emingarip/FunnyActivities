using FluentAssertions;
using FunnyActivities.Application.Behaviors;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Reflection;
using System.Security.Claims;
using Xunit;

namespace FunnyActivities.Application.UnitTests.Behaviors
{
    public class AuthorizationBehaviorTests
    {
        [Fact]
        public async Task Handle_AuthorizedRequest_ShouldContinuePipeline()
        {
            var authorizationServiceMock = new Mock<IAuthorizationService>();
            var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            var behavior = new AuthorizationBehavior<UpdateMaterialCommandRequest, TestResponse>(
                authorizationServiceMock.Object,
                httpContextAccessorMock.Object);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "testuser") }));
            httpContextAccessorMock.Setup(x => x.HttpContext).Returns(new DefaultHttpContext { User = user });

            authorizationServiceMock
                .Setup(x => x.AuthorizeAsync(
                    It.IsAny<ClaimsPrincipal>(),
                    It.IsAny<object?>(),
                    It.IsAny<string>()))
                .ReturnsAsync(AuthorizationResult.Success());

            var nextCalled = false;
            RequestHandlerDelegate<TestResponse> next = _ =>
            {
                nextCalled = true;
                return Task.FromResult(new TestResponse());
            };

            var result = await behavior.Handle(new UpdateMaterialCommandRequest(), next, CancellationToken.None);

            result.Should().NotBeNull();
            nextCalled.Should().BeTrue();
            authorizationServiceMock.Verify(x => x.AuthorizeAsync(
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<object?>(),
                It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task Handle_UnauthorizedRequest_ShouldThrowUnauthorizedAccessException()
        {
            var authorizationServiceMock = new Mock<IAuthorizationService>();
            var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            var behavior = new AuthorizationBehavior<UpdateMaterialCommandRequest, TestResponse>(
                authorizationServiceMock.Object,
                httpContextAccessorMock.Object);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "testuser") }));
            httpContextAccessorMock.Setup(x => x.HttpContext).Returns(new DefaultHttpContext { User = user });

            authorizationServiceMock
                .Setup(x => x.AuthorizeAsync(
                    It.IsAny<ClaimsPrincipal>(),
                    It.IsAny<object?>(),
                    It.IsAny<string>()))
                .ReturnsAsync(AuthorizationResult.Failed());

            var nextCalled = false;
            RequestHandlerDelegate<TestResponse> next = _ =>
            {
                nextCalled = true;
                return Task.FromResult(new TestResponse());
            };

            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                behavior.Handle(new UpdateMaterialCommandRequest(), next, CancellationToken.None));

            exception.Message.Should().Contain("Access denied");
            exception.Message.Should().Contain("CanUpdateMaterial");
            nextCalled.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_RequestWithNoRequiredPolicy_ShouldSkipAuthorizationAndContinuePipeline()
        {
            var authorizationServiceMock = new Mock<IAuthorizationService>();
            var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            var behavior = new AuthorizationBehavior<UnknownRequest, TestResponse>(
                authorizationServiceMock.Object,
                httpContextAccessorMock.Object);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "testuser") }));
            httpContextAccessorMock.Setup(x => x.HttpContext).Returns(new DefaultHttpContext { User = user });

            var nextCalled = false;
            RequestHandlerDelegate<TestResponse> next = _ =>
            {
                nextCalled = true;
                return Task.FromResult(new TestResponse());
            };

            var result = await behavior.Handle(new UnknownRequest(), next, CancellationToken.None);

            result.Should().NotBeNull();
            nextCalled.Should().BeTrue();
            authorizationServiceMock.Verify(x => x.AuthorizeAsync(
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<object?>(),
                It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Handle_NoHttpContext_ShouldThrowUnauthorizedAccessException()
        {
            var authorizationServiceMock = new Mock<IAuthorizationService>();
            var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            var behavior = new AuthorizationBehavior<UpdateMaterialCommandRequest, TestResponse>(
                authorizationServiceMock.Object,
                httpContextAccessorMock.Object);

            httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext?)null);
            RequestHandlerDelegate<TestResponse> next = _ => Task.FromResult(new TestResponse());

            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                behavior.Handle(new UpdateMaterialCommandRequest(), next, CancellationToken.None));

            exception.Message.Should().Contain("User context is not available");
        }

        [Fact]
        public async Task Handle_AuthorizationServiceThrowsException_ShouldPropagateException()
        {
            var authorizationServiceMock = new Mock<IAuthorizationService>();
            var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            var behavior = new AuthorizationBehavior<UpdateMaterialCommandRequest, TestResponse>(
                authorizationServiceMock.Object,
                httpContextAccessorMock.Object);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "testuser") }));
            httpContextAccessorMock.Setup(x => x.HttpContext).Returns(new DefaultHttpContext { User = user });

            authorizationServiceMock
                .Setup(x => x.AuthorizeAsync(
                    It.IsAny<ClaimsPrincipal>(),
                    It.IsAny<object?>(),
                    It.IsAny<string>()))
                .ThrowsAsync(new Exception("Authorization service error"));

            RequestHandlerDelegate<TestResponse> next = _ => Task.FromResult(new TestResponse());

            await Assert.ThrowsAsync<Exception>(() =>
                behavior.Handle(new UpdateMaterialCommandRequest(), next, CancellationToken.None));
        }

        [Theory]
        [InlineData(typeof(UpdateMaterialCommandRequest), "CanUpdateMaterial")]
        [InlineData(typeof(DeleteMaterialCommandRequest), "CanDeleteMaterial")]
        [InlineData(typeof(UploadMaterialPhotosCommandRequest), "CanManagePhotos")]
        [InlineData(typeof(DeleteMaterialPhotoCommandRequest), "CanManagePhotos")]
        [InlineData(typeof(GetMaterialQueryRequest), "CanViewMaterial")]
        [InlineData(typeof(GetMaterialsQueryRequest), "CanViewMaterial")]
        [InlineData(typeof(GetMaterialPhotosQueryRequest), "CanViewMaterial")]
        [InlineData(typeof(DownloadMaterialPhotoQueryRequest), "CanViewMaterial")]
        [InlineData(typeof(UnknownRequest), null)]
        public void GetPolicyForRequest_ShouldReturnExpectedPolicy(Type requestType, string? expectedPolicy)
        {
            var method = typeof(AuthorizationBehavior<object, TestResponse>)
                .GetMethod("GetPolicyForRequest", BindingFlags.NonPublic | BindingFlags.Static);

            method.Should().NotBeNull();
            var request = Activator.CreateInstance(requestType)!;
            var result = method!.Invoke(null, new[] { request }) as string;

            result.Should().Be(expectedPolicy);
        }

        public class TestResponse { }

        public class UpdateMaterialCommandRequest : IRequest<TestResponse> { }
        public class DeleteMaterialCommandRequest : IRequest<TestResponse> { }
        public class UploadMaterialPhotosCommandRequest : IRequest<TestResponse> { }
        public class DeleteMaterialPhotoCommandRequest : IRequest<TestResponse> { }
        public class GetMaterialQueryRequest : IRequest<TestResponse> { }
        public class GetMaterialsQueryRequest : IRequest<TestResponse> { }
        public class GetMaterialPhotosQueryRequest : IRequest<TestResponse> { }
        public class DownloadMaterialPhotoQueryRequest : IRequest<TestResponse> { }
        public class UnknownRequest : IRequest<TestResponse> { }
    }
}
