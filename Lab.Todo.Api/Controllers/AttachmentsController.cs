using System.Diagnostics.CodeAnalysis;
using AutoMapper;
using Lab.Todo.Api.DTOs.Requests;
using Lab.Todo.Api.DTOs.Responses;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Lab.Todo.BLL.Services.AttachmentManagers;
using Lab.Todo.BLL.Services.AttachmentManagers.Models;

namespace Lab.Todo.Api.Controllers
{
    [SuppressMessage("ReSharper", "UnusedParameter.Global")]
    [Route("api/[controller]")]
    [ApiController]
    public class AttachmentsController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IAttachmentManager _attachmentManager;

        public AttachmentsController(IMapper mapper, IAttachmentManager attachmentManager)
        {
            _mapper = mapper;
            _attachmentManager = attachmentManager;
        }

        [HttpGet("{id}")]
        public ActionResult GetAttachment(int id)
        {
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> CreateAttachment(AttachmentCreateRequest attachmentCreateRequest)
        {
            var attachmentCreateInfo = _mapper.Map<AttachmentCreateData>(attachmentCreateRequest);

            var attachment = await _attachmentManager.UploadAttachmentAsync(attachmentCreateInfo);

            var attachmentResponse = _mapper.Map<AttachmentCreateResponse>(attachment);

            return CreatedAtAction(nameof(GetAttachment), new { id = attachmentResponse.Id }, attachmentResponse);
        }
    }
}