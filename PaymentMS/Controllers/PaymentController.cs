using Microsoft.AspNetCore.Mvc;

namespace PaymentMS.Controllers;

[ApiController]
public class PaymentController : ControllerBase
{

    private readonly ILogger<PaymentController> _logger;

    public PaymentController(ILogger<PaymentController> logger)
    {
        this._logger = logger;
    }

}

