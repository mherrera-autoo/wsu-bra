using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ERP.Modules.Identity.Application.Services;

namespace ERP.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EmailTestController : ControllerBase
{
    private readonly EmailSmokeTestService _emailTestService;

    public EmailTestController(EmailSmokeTestService emailTestService)
    {
        _emailTestService = emailTestService;
    }

    [HttpPost("send-test")]
    public async Task<IActionResult> SendTestEmail([FromBody] SendTestEmailRequest request, CancellationToken ct)
    {
        try
        {
            await _emailTestService.SendTestEmailAsync(request.ToEmail, ct);
            return Ok(new { message = "Test email sent successfully", to = request.ToEmail });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to send test email", details = ex.Message });
        }
    }
}

public class SendTestEmailRequest
{
    public string ToEmail { get; set; } = "";
}