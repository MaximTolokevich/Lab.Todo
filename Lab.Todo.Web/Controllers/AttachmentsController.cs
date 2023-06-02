using AutoMapper;
using Lab.Todo.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Lab.Todo.BLL.Services.AttachmentManagers;
using Lab.Todo.BLL.Services.AttachmentManagers.Models;

namespace Lab.Todo.Web.Controllers
{
    public class AttachmentsController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IAttachmentManager _attachmentManager;

        public AttachmentsController(IMapper mapper, IAttachmentManager attachmentManager)
        {
            _mapper = mapper;
            _attachmentManager = attachmentManager;
        }

        [HttpGet]
        public IActionResult Upload() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(AttachmentCreateModel attachment)
        {
            if (!ModelState.IsValid)
            {
                return View(attachment);
            }

            var attachmentCreateInfo = _mapper.Map<AttachmentCreateData>(attachment);

            await _attachmentManager.UploadAttachmentAsync(attachmentCreateInfo);

            return View();
        }
    }
}