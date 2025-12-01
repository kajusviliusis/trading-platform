using Microsoft.AspNetCore.Mvc;
using trading_platform.Data;
using trading_platform.Dtos;
using trading_platform.Models.Entities;

namespace trading_platform.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly TradingDbContext _context;

        public OrderController(TradingDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetOrders()
        {
            var orders = _context.Orders
                .Select(o => new OrderResponseDto
                {
                    Id = o.Id,
                    Quantity = o.Quantity,
                    Type = o.Type,
                    PriceAtExecution = o.PriceAtExecution,
                    Timestamp = o.Timestamp,
                    UserId = o.UserId,
                    StockId = o.StockId
                })
                .ToList();
            return Ok(orders);
        }

        [HttpGet("{id}")]
        public IActionResult GetOrder(int id)
        {
            var order = _context.Orders.Find(id);
            if (order == null) return NotFound();
            var response = new OrderResponseDto
            {
                Id = order.Id,
                Quantity = order.Quantity,
                Type = order.Type,
                PriceAtExecution = order.PriceAtExecution,
                Timestamp = order.Timestamp,
                UserId = order.UserId,
                StockId = order.StockId

            };
            return Ok(response);
        }

        [HttpPost]
        public IActionResult CreateOrder(CreateOrderDto dto)
        {
            var stock = _context.Stocks.Find(dto.StockId);
            if (stock == null) return BadRequest("Invalid StockId");

            var wallet = _context.Wallets.FirstOrDefault(w => w.UserId == dto.UserId);
            if (wallet == null) return BadRequest("Wallet not found");

            var cost = dto.Quantity * stock.Price;

            if(dto.Type == "BUY")
            {
                if(wallet.Balance<cost)
                {
                    return BadRequest("Insufficient funds");
                }
                wallet.Balance -= cost;
            }
            else if(dto.Type == "SELL")
            {
                //to do : paziuret holdingus
                wallet.Balance += cost;
            }

                var order = new Order
                {
                    UserId = dto.UserId,
                    StockId = dto.StockId,
                    Quantity = dto.Quantity,
                    Type = dto.Type,
                    PriceAtExecution = stock.Price,
                    Timestamp = DateTime.UtcNow
                };
            _context.Orders.Add(order);
            _context.SaveChanges();

            var response = new OrderResponseDto
            {
                Id = order.Id,
                Quantity = order.Quantity,
                Type = order.Type,
                PriceAtExecution = order.PriceAtExecution,
                Timestamp = order.Timestamp,
                UserId = order.UserId,
                StockId = order.StockId
            };

            return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, response);
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteOrder(int id)
        {
            var order = _context.Orders.Find(id);
            if (order == null) return NotFound();

            _context.Orders.Remove(order);
            _context.SaveChanges();
            return NoContent();
        }
    }
}
