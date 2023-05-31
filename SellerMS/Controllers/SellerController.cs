using Common.Events;
using Microsoft.AspNetCore.Mvc;
using SellerMS.DTO;
using SellerMS.Services;

namespace SellerMS.Controllers;

[ApiController]
[Route("[controller]")]
public class SellerController : ControllerBase
{
    private readonly ISellerService sellerService;
    private readonly ILogger<SellerController> logger;

    public SellerController(ISellerService sellerService, ILogger<SellerController> logger)
    {
        this.sellerService = sellerService;
        this.logger = logger;
    }

    [HttpGet]
    [Route("/dashboard/{sellerId}")]
    public ActionResult<SellerDashboard> GetDashboard(long sellerId)
    {
        // https://stackoverflow.com/questions/12636613/how-to-calculate-moving-average-without-keeping-the-count-and-data-total
        // total overall in sales, revenue, number of orders, average order value, average revenue per order,
        // top selling products (total number and revenue) and categories.
        // overview recent orders. order date, payment info, shipping status, customer name
        // inventory levels,
        // open shipments. calculate how late they are.
        // total number of shipments, avg shipment value per order, avg individual item shipment per order, average mean time to complete order

        // what is the dynamic? seller updates are always preceding this view?
        // instead of events, make sense to have aggregate all portions of the data here just as found in vldb

        // compression
        // https://stackoverflow.com/questions/11725078/restful-api-handling-large-amounts-of-data
        // chunk on header
        // https://medium.com/@michalbogacz/streaming-large-data-sets-f86a53e43472
        var dash = this.sellerService.QueryDashboard(sellerId);
        return Ok(dash);
    }

}