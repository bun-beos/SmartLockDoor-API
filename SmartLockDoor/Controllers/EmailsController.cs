using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace SmartLockDoor.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class EmailsController : ControllerBase
    {
        private readonly IEmailService _emailService;
        public EmailsController(IEmailService emailService)
        {
            _emailService = emailService;
        }

        [HttpPost]
        public IActionResult SendEmail(EmailDto emailDto)
        {
            try
            {

                _emailService.SendEmail(emailDto);
                return Ok();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
