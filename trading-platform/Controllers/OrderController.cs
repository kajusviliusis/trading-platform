using Microsoft.AspNetCore.Mvc;
using trading_platform.Data;
using trading_platform.Dtos;
using trading_platform.Models.Entities;
using trading_platform.Services;

namespace trading_platform.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly OrderService _orderService;

        public OrderController(OrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet]
        public IActionResult GetOrders()
        {
            return Ok(_orderService.GetOrders());
        }

        [HttpGet("{id}")]
        public IActionResult GetOrder(int id)
        {
            var order = _orderService.GetOrder(id);
            if (order == null) return NotFound();
            return Ok(order);
        }

        [HttpPost]
        public IActionResult CreateOrder(CreateOrderDto dto)
        {
            try
            {
                var response = _orderService.PlaceOrder(dto);
                return CreatedAtAction(nameof(GetOrder), new { id = response.Id }, response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteOrder(int id)
        {
            var success = _orderService.DeleteOrder(id);
            return success ? NoContent() : NotFound();
        }

        [HttpGet("user/{userId}/transactions")]
        public IActionResult GetTransactionsByUser(int userId)
        {
            return Ok(_orderService.GetTransactionsByUser(userId));
        }
    }

}
