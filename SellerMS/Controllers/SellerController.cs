using Common.Entities;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using SellerMS.DTO;
using SellerMS.Services;
using SellerMS.Repositories;

namespace SellerMS.Controllers;

[ApiController]
public class SellerController : ControllerBase
{
    private readonly ISellerRepository sellerRepository;
    private readonly ISellerService sellerService;
    private readonly ILogger<SellerController> logger;

    public SellerController(ISellerRepository sellerRepository, ISellerService sellerService, ILogger<SellerController> logger)
    {
        this.sellerRepository = sellerRepository;
        this.sellerService = sellerService;
        this.logger = logger;
    }

    [HttpPost]
    [Route("/")]
    [ProducesResponseType((int)HttpStatusCode.Created)]
    public ActionResult AddSeller([FromBody] Seller seller)
    {
        this.logger.LogInformation("[AddSeller] received for seller {0}", seller.id);
        this.sellerRepository.Insert(new()
        {
            id = seller.id,
            name = seller.name,
            company_name = seller.company_name,
            email = seller.email,
            phone = seller.phone,
            mobile_phone = seller.mobile_phone,
            cpf = seller.cpf,
            cnpj = seller.cnpj,
            address = seller.address,
            complement = seller.complement,
            city = seller.city,
            state = seller.state,
            zip_code = seller.zip_code,
        });
        this.logger.LogInformation("[AddSeller] completed for seller {0}.", seller.id);
        return StatusCode((int)HttpStatusCode.Created);
    }

    [HttpGet]
    [Route("{sellerId}")]
    [ProducesResponseType(typeof(Seller),(int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public ActionResult<Seller> GetSeller(int sellerId)
    {
        this.logger.LogInformation("[GetSeller] received for seller {0}", sellerId);
        var seller = this.sellerRepository.Get(sellerId);
        this.logger.LogInformation("[GetSeller] completed for seller {0}.", sellerId);
        if(seller is not null) return Ok(new Seller()
        {
            id = seller.id,
            name = seller.name,
            company_name = seller.company_name,
            email = seller.email,
            phone = seller.phone,
            mobile_phone = seller.mobile_phone,
            cpf = seller.cpf,
            cnpj = seller.cnpj,
            address = seller.address,
            complement = seller.complement,
            city = seller.city,
            state = seller.state,
            zip_code = seller.zip_code,
        });
        return NotFound();
    }

    [HttpGet]
    [Route("/dashboard/{sellerId}")]
    [ProducesResponseType(typeof(SellerDashboard),(int)HttpStatusCode.OK)]
    public ActionResult<SellerDashboard> GetDashboard(int sellerId)
    {
        var dash = this.sellerService.QueryDashboard(sellerId);
        return Ok(dash);
    }

    [Route("/reset")]
    [HttpPatch]
    [ProducesResponseType((int)HttpStatusCode.Accepted)]
    public ActionResult Reset()
    {
        logger.LogWarning("Reset requested at {0}", DateTime.UtcNow);
        this.sellerService.Reset();
        return Ok();
    }

    [Route("/cleanup")]
    [HttpPatch]
    [ProducesResponseType((int)HttpStatusCode.Accepted)]
    public ActionResult Cleanup()
    {
        logger.LogWarning("Cleanup requested at {0}", DateTime.UtcNow);
        this.sellerService.Cleanup();
        return Ok();
    }

}