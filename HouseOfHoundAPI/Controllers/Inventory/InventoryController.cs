using HouseOfHound.Api.Models;
using HouseOfHound.Api.Repositories;
using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Http;

namespace HouseOfHound.Api.Controllers
{
    [RoutePrefix("api/inventory")]
    public class InventoryController : ApiController
    {
        private readonly InventoryRepository _inventoryRepository;

        public InventoryController()
        {
            _inventoryRepository = new InventoryRepository();
        }

        // GET api/inventory/stock
        // GET api/inventory/stock?search=therapy
        [HttpGet]
        [Route("stock")]
        public IHttpActionResult GetStockItems(string search = null)
        {
            try
            {
                List<StockItem> items = _inventoryRepository.GetStockItems(search, true);
                return Ok(items);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // GET api/inventory/stock/5
        [HttpGet]
        [Route("stock/{id:int}")]
        public IHttpActionResult GetStockItemById(int id)
        {
            try
            {
                StockItem item = _inventoryRepository.GetStockItemById(id);

                if (item == null)
                    return NotFound();

                return Ok(item);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // POST api/inventory/stock
        [HttpPost]
        [Route("stock")]
        public IHttpActionResult CreateStockItem([FromBody] StockItem item)
        {
            try
            {
                if (item == null)
                    return BadRequest("Stock item cannot be null.");

                if (string.IsNullOrWhiteSpace(item.Code))
                    return BadRequest("Code is required.");

                if (string.IsNullOrWhiteSpace(item.Description))
                    return BadRequest("Description is required.");

                if (item.QuantityInStock < 0)
                    return BadRequest("Quantity cannot be negative.");

                if (item.MinimumStockHolding < 0)
                    return BadRequest("Minimum stock holding cannot be negative.");

                if (item.CostPrice < 0)
                    return BadRequest("Cost price cannot be negative.");

                if (item.SalePrice < 0)
                    return BadRequest("Sale price cannot be negative.");

                StockItem createdItem = _inventoryRepository.CreateStockItem(item);

                return Content(HttpStatusCode.Created, createdItem);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // PUT api/inventory/stock/5
        [HttpPut]
        [Route("stock/{id:int}")]
        public IHttpActionResult UpdateStockItem(int id, [FromBody] StockItem item)
        {
            try
            {
                if (item == null)
                    return BadRequest("Stock item cannot be null.");

                if (id != item.Id)
                    return BadRequest("The stock item ID in the URL does not match the ID in the body.");

                if (string.IsNullOrWhiteSpace(item.Code))
                    return BadRequest("Code is required.");

                if (string.IsNullOrWhiteSpace(item.Description))
                    return BadRequest("Description is required.");

                if (item.QuantityInStock < 0)
                    return BadRequest("Quantity cannot be negative.");

                if (item.MinimumStockHolding < 0)
                    return BadRequest("Minimum stock holding cannot be negative.");

                if (item.CostPrice < 0)
                    return BadRequest("Cost price cannot be negative.");

                if (item.SalePrice < 0)
                    return BadRequest("Sale price cannot be negative.");

                bool updated = _inventoryRepository.UpdateStockItem(item);

                if (!updated)
                    return NotFound();

                return StatusCode(HttpStatusCode.NoContent);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // DELETE api/inventory/stock/5
        [HttpDelete]
        [Route("stock/{id:int}")]
        public IHttpActionResult DeleteStockItem(int id)
        {
            try
            {
                bool deleted = _inventoryRepository.SoftDeleteStockItem(id);

                if (!deleted)
                    return NotFound();

                return StatusCode(HttpStatusCode.NoContent);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // POST api/inventory/stock/5/adjust
        [HttpPost]
        [Route("stock/{id:int}/adjust")]
        public IHttpActionResult AdjustStock(int id, [FromBody] AdjustStockRequest request)
        {
            try
            {
                if (request == null)
                    return BadRequest("Adjustment request cannot be null.");

                if (request.QuantityChange == 0)
                    return BadRequest("QuantityChange cannot be zero.");

                bool adjusted = _inventoryRepository.AdjustStock(id, request.QuantityChange, request.Note);

                if (!adjusted)
                    return NotFound();

                return StatusCode(HttpStatusCode.NoContent);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // POST api/inventory/sales
        [HttpPost]
        [Route("sales")]
        public IHttpActionResult CreateSale([FromBody] CreateSaleRequest request)
        {
            try
            {
                if (request == null)
                    return BadRequest("Sale request cannot be null.");

                if (string.IsNullOrWhiteSpace(request.PaymentMethod))
                    return BadRequest("Payment method is required.");

                if (request.Lines == null || request.Lines.Count == 0)
                    return BadRequest("Sale must contain at least one item.");

                foreach (var line in request.Lines)
                {
                    if (line.StockItemId <= 0)
                        return BadRequest("Each sale line must have a valid StockItemId.");

                    if (line.Quantity <= 0)
                        return BadRequest("Each sale line must have a quantity greater than zero.");
                }

                Sale createdSale = _inventoryRepository.CreateSale(request);

                return Content(HttpStatusCode.Created, createdSale);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // GET api/inventory/sales
        // GET api/inventory/sales?maxResults=50&dogId=5
        [HttpGet]
        [Route("sales")]
        public IHttpActionResult GetSalesHistory(int maxResults = 100, int? dogId = null)
        {
            try
            {
                if (maxResults <= 0)
                    maxResults = 100;

                if (maxResults > 500)
                    maxResults = 500;

                List<Sale> sales = _inventoryRepository.GetSalesHistory(maxResults, dogId);

                return Ok(sales);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // GET api/inventory/sales/10
        [HttpGet]
        [Route("sales/{id:int}")]
        public IHttpActionResult GetSaleById(int id)
        {
            try
            {
                Sale sale = _inventoryRepository.GetSaleById(id);

                if (sale == null)
                    return NotFound();

                return Ok(sale);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // GET api/inventory/stock/5/movements
        [HttpGet]
        [Route("stock/{id:int}/movements")]
        public IHttpActionResult GetStockMovements(int id)
        {
            try
            {
                List<StockMovement> movements = _inventoryRepository.GetStockMovements(id);
                return Ok(movements);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
    }

    public class AdjustStockRequest
    {
        public int QuantityChange { get; set; }

        public string Note { get; set; }
    }
}
