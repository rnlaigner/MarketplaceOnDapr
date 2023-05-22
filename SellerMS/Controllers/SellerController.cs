using Common.Events;
using Microsoft.AspNetCore.Mvc;
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
    public IActionResult GetDashboard(string sellerId)
    {
        // https://stackoverflow.com/questions/12636613/how-to-calculate-moving-average-without-keeping-the-count-and-data-total
        // total overall in sales, revenue, number of orders, average order value, average revenue per order,
        // top selling products and categories.
        // overview recent orders. order date, shipping status, customer name
        // inventory levels,
        // open shipments. calculate how late they are.
        // total number of shipments, avg shipment value per order, avg individual item shipment per order, average mean time to complete order

        // what is the dynamic? seller updates are always preceding this view?
        // instead of events, make sense to have aggregate all portions of the data here just as found in vldb

        // compression
        // https://stackoverflow.com/questions/11725078/restful-api-handling-large-amounts-of-data
        // chunk on header
        // https://medium.com/@michalbogacz/streaming-large-data-sets-f86a53e43472

        return Ok();
    }

    [HttpPost]
    [Route("/transaction-reports")]
    public IActionResult GetFinancialReport([FromBody] FinancialReportRequest request)
    {

        // submit event that is subscribed by all microservices. this service tracks the response of all services via events. bulk events can be used
        // reports are available for some period
        // a periodic task cleans up old reports
        // results may come inconsistent with each other. for instance, order that is delivered is not included because event has not yet received from payment, but payment is already considering the order completed

        // one may say it is simply get closed orders and then send to all other microservices. but this is incorrect because some orders may be finished indeed in the downstream services and will end up not being computed

        // works as a primitive query processor. otherwise how can we get data from all services?

        // avg per order
        // avg discount
        // top 10 most products sold in the period
        // most categories
        // avg delay to deliver


        return Ok();
    }
}

