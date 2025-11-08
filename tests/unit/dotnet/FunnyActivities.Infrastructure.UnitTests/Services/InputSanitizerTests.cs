using System;
using FluentAssertions;
using Xunit;
using FunnyActivities.Infrastructure.Services;

namespace FunnyActivities.Infrastructure.UnitTests.Services
{
    public class InputSanitizerTests
    {
        private readonly InputSanitizer _sanitizer;

        public InputSanitizerTests()
        {
            _sanitizer = new InputSanitizer();
        }

        [Fact]
        public void SanitizeString_ShouldRemoveHtmlTags()
        {
            // Arrange
            var input = "<script>alert('xss')</script>Hello <b>World</b>";

            // Act
            var result = _sanitizer.SanitizeString(input);

            // Assert
            result.Should().Be("Hello World");
        }

        [Fact]
        public void SanitizeString_ShouldRemoveScriptTags()
        {
            // Arrange
            var input = "Hello <script type='text/javascript'>alert('xss');</script>World";

            // Act
            var result = _sanitizer.SanitizeString(input);

            // Assert
            result.Should().Be("Hello World");
        }

        [Fact]
        public void SanitizeString_ShouldRemoveJavascriptProtocol()
        {
            // Arrange
            var input = "<a href='javascript:alert(\"xss\")'>Click me</a>";

            // Act
            var result = _sanitizer.SanitizeString(input);

            // Assert
            result.Should().Be("Click me");
        }

        [Fact]
        public void SanitizeString_ShouldRemoveVbscriptProtocol()
        {
            // Arrange
            var input = "<a href='vbscript:msgbox(\"xss\")'>Click me</a>";

            // Act
            var result = _sanitizer.SanitizeString(input);

            // Assert
            result.Should().Be("Click me");
        }

        [Fact]
        public void SanitizeString_ShouldRemoveDataUrls()
        {
            // Arrange
            var input = "<img src='data:text/html,<script>alert(\"xss\")</script>'>";

            // Act
            var result = _sanitizer.SanitizeString(input);

            // Assert
            result.Should().Be("");
        }

        [Fact]
        public void SanitizeString_ShouldRemoveEventHandlers()
        {
            // Arrange
            var input = "<button onclick='alert(\"xss\")'>Click me</button>";

            // Act
            var result = _sanitizer.SanitizeString(input);

            // Assert
            result.Should().Be("Click me");
        }

        [Fact]
        public void SanitizeString_ShouldTrimWhitespace()
        {
            // Arrange
            var input = "  Hello World  ";

            // Act
            var result = _sanitizer.SanitizeString(input);

            // Assert
            result.Should().Be("Hello World");
        }

        [Fact]
        public void SanitizeString_ShouldHandleNullInput()
        {
            // Arrange
            string input = null;

            // Act
            var result = _sanitizer.SanitizeString(input);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void SanitizeString_ShouldHandleEmptyInput()
        {
            // Arrange
            var input = "";

            // Act
            var result = _sanitizer.SanitizeString(input);

            // Assert
            result.Should().Be("");
        }

        [Fact]
        public void SanitizeString_WithMaxLength_ShouldTruncateLongInput()
        {
            // Arrange
            var input = "This is a very long string that should be truncated";
            var maxLength = 20;

            // Act
            var result = _sanitizer.SanitizeString(input, maxLength);

            // Assert
            result.Should().Be("This is a very long ");
            result.Length.Should().BeLessOrEqualTo(maxLength);
        }

        [Fact]
        public void SanitizeString_WithMaxLength_ShouldHandleShortInput()
        {
            // Arrange
            var input = "Short";
            var maxLength = 20;

            // Act
            var result = _sanitizer.SanitizeString(input, maxLength);

            // Assert
            result.Should().Be("Short");
        }

        [Fact]
        public void SanitizeString_WithMaxLength_ShouldSanitizeAndTruncate()
        {
            // Arrange
            var input = "<b>This is a <script>alert('xss')</script>very long string</b>";
            var maxLength = 15;

            // Act
            var result = _sanitizer.SanitizeString(input, maxLength);

            // Assert
            result.Should().Be("This is a very");
            result.Length.Should().BeLessOrEqualTo(maxLength);
        }

        [Fact]
        public void SanitizeString_WithMaxLength_ShouldHandleNullInput()
        {
            // Arrange
            string input = null;
            var maxLength = 20;

            // Act
            var result = _sanitizer.SanitizeString(input, maxLength);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void SanitizeString_ShouldRemoveNestedScriptTags()
        {
            // Arrange
            var input = "<div><script><script>alert('nested')</script></script>Content</div>";

            // Act
            var result = _sanitizer.SanitizeString(input);

            // Assert
            result.Should().Be("Content");
        }

        [Fact]
        public void SanitizeString_ShouldRemoveMultipleScriptTags()
        {
            // Arrange
            var input = "<script>alert(1)</script>Hello<script>alert(2)</script>World<script>alert(3)</script>";

            // Act
            var result = _sanitizer.SanitizeString(input);

            // Assert
            result.Should().Be("HelloWorld");
        }

        [Fact]
        public void SanitizeString_ShouldHandleComplexHtml()
        {
            // Arrange
            var input = "<div class='test'><img src='x' onerror='alert(1)'/><a href='javascript:void(0)' onclick='evil()'>Link</a><script>bad()</script>Good content</div>";

            // Act
            var result = _sanitizer.SanitizeString(input);

            // Assert
            result.Should().Be("LinkGood content");
        }
    }
}