using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using Lab.Todo.Api.Controllers;
using Lab.Todo.Api.DTOs.Requests;
using Lab.Todo.Api.DTOs.Responses;
using Lab.Todo.Api.Tests.Helpers;
using Lab.Todo.BLL.Services.AttachmentManagers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;

namespace Lab.Todo.Api.Tests.ControllerTests
{
    public class AttachmentsControllerTests
    {
        private readonly Mock<IMapper> _mapperMock = new();
        private readonly Mock<IAttachmentManager> _managerMock = new();
        private readonly AttachmentsController _controller;

        public AttachmentsControllerTests()
        {
            _controller = new AttachmentsController(_mapperMock.Object, _managerMock.Object);
        }

        [Theory]
        [MemberData(nameof(GetAttachmentCreateRequests))]
        public void AttachmentCreateRequest_ModelStateValidationTheory(AttachmentCreateRequest item, bool expectedResult)
        {
            // Arrange         
            var validationContext = new ValidationContext(item);

            // Act
            var isValid = Validator.TryValidateObject(item, validationContext, null, true);

            // Assert
            Assert.Equal(expectedResult, isValid);
        }

        public static IEnumerable<object[]> GetAttachmentCreateRequests()
        {
            yield return new object[] { new AttachmentCreateRequest { FileName = null, Content = new byte[] { 1, 2, 3 } }, false };
            yield return new object[] { new AttachmentCreateRequest { FileName = "text.txt", Content = null }, false };
            yield return new object[] { new AttachmentCreateRequest { FileName = $"{ApiTestsHelper.GenerateString(200)}.txt", Content = new byte[] { 1, 2, 3 } }, false };
            yield return new object[] { new AttachmentCreateRequest { FileName = "text.txt", Content = Array.Empty<byte>() }, false };
            yield return new object[] { new AttachmentCreateRequest { FileName = "text.txt", Content = new byte[6_000_000] }, false };
            yield return new object[] { new AttachmentCreateRequest { FileName = "text", Content = new byte[] { 1, 2, 3 } }, false };
            yield return new object[] { new AttachmentCreateRequest { FileName = "text.super-long-extension", Content = new byte[] { 1, 2, 3 } }, false };
            yield return new object[] { new AttachmentCreateRequest { FileName = "text.txt", Content = new byte[] { 1, 2, 3 } }, true };
        }

        [Fact]
        public async Task CreateAttachment_ReturnsCreatedAtAction()
        {
            // Arrange
            var response = new AttachmentCreateResponse { Id = 1, FileName = "text.txt" };
            _mapperMock.Setup(x => x.Map<AttachmentCreateResponse>(null))
                .Returns<AttachmentCreateResponse>(_ => response);

            // Act
            var result = await _controller.CreateAttachment(null);

            // Assert
            result.Should().BeOfType<CreatedAtActionResult>();
            ((CreatedAtActionResult)result).Value.Should().BeSameAs(response);
            ((CreatedAtActionResult)result).RouteValues["id"].Should().BeEquivalentTo(response.Id);
        }
    }
}
