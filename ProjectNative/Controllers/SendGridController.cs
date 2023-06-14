using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace ProjectNative.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SendGridController : ControllerBase
    {
        private readonly SendGridClient _sendGridClient;

        public SendGridController(SendGridClient sendGridClient)
        {
            _sendGridClient = sendGridClient;
        }

        [HttpPost("SendMessageToEmail")]
        public async Task<IActionResult> SendEmail(string recipientEmail, string subject, string message)
        {
            var from = new EmailAddress("64123250113@kru.ac.th", "Golf");
            var to = new EmailAddress(recipientEmail);
            var emailMessage = MailHelper.CreateSingleEmail(from, to, subject, message, message);
            var response = await _sendGridClient.SendEmailAsync(emailMessage);

            if (response.StatusCode == System.Net.HttpStatusCode.Accepted)
                return Ok("อีเมลถูกส่งแล้ว");
            return BadRequest("เกิดข้อผิดพลาดในการส่งอีเมล");
        }

        


    }
}
