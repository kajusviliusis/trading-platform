using trading_platform.Data;
using trading_platform.Dtos;
using trading_platform.Models.Entities;

namespace trading_platform.Services
{
    public class OrderService
    {
        private readonly TradingDbContext _context;

        public OrderService(TradingDbContext context)
        {
            _context = context;
        }

        public IEnumerable<OrderResponseDto> GetOrders()
        {
            return _context.Orders
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
        }

        public OrderResponseDto? GetOrder(int id)
        {
            var order = _context.Orders.Find(id);
            if (order == null) return null;

            return new OrderResponseDto
            {
                Id = order.Id,
                Quantity = order.Quantity,
                Type = order.Type,
                PriceAtExecution = order.PriceAtExecution,
                Timestamp = order.Timestamp,
                UserId = order.UserId,
                StockId = order.StockId
            };
        }

        public OrderResponseDto PlaceOrder(CreateOrderDto dto)
        {
            var stock = _context.Stocks.Find(dto.StockId)
                ?? throw new Exception("Invalid StockId");

            var wallet = _context.Wallets.FirstOrDefault(w => w.UserId == dto.UserId)
                ?? throw new Exception("Wallet not found");

            var cost = dto.Quantity * stock.Price;

            if (dto.Type == "BUY")
            {
                if (wallet.Balance < cost)
                    throw new Exception("Insufficient funds");

                wallet.Balance -= cost;

                var holding = _context.Holdings
                    .FirstOrDefault(h => h.UserId == dto.UserId && h.StockId == dto.StockId);

                if (holding == null)
                    _context.Holdings.Add(new Holding { UserId = dto.UserId, StockId = dto.StockId, Quantity = dto.Quantity });
                else
                    holding.Quantity += dto.Quantity;
            }
            else if (dto.Type == "SELL")
            {
                var holding = _context.Holdings
                    .FirstOrDefault(h => h.UserId == dto.UserId && h.StockId == dto.StockId);

                if (holding == null || holding.Quantity < dto.Quantity)
                    throw new Exception("Not enough shares to sell");

                holding.Quantity -= dto.Quantity;
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

            var transaction = new Transaction
            {
                UserId = dto.UserId,
                StockId = dto.StockId,
                OrderId = order.Id,
                Quantity = dto.Quantity,
                PriceAtExecution = stock.Price,
                Timestamp = DateTime.UtcNow,
                Type = dto.Type
            };
            _context.Transactions.Add(transaction);
            _context.SaveChanges();

            return new OrderResponseDto
            {
                Id = order.Id,
                Quantity = order.Quantity,
                Type = order.Type,
                PriceAtExecution = order.PriceAtExecution,
                Timestamp = order.Timestamp,
                UserId = order.UserId,
                StockId = order.StockId
            };
        }

        public bool DeleteOrder(int id)
        {
            var order = _context.Orders.Find(id);
            if (order == null) return false;

            _context.Orders.Remove(order);
            _context.SaveChanges();
            return true;
        }

        public IEnumerable<Transaction> GetTransactionsByUser(int userId)
        {
            return _context.Transactions
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.Timestamp)
                .ToList();
        }
    }
}
