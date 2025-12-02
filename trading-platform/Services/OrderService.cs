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

        public Order PlaceOrder(CreateOrderDto dto)
        {
            var stock = _context.Stocks.Find(dto.StockId);
            if (stock == null) throw new Exception("Invalid StockId");

            var wallet = _context.Wallets.FirstOrDefault(w => w.UserId == dto.UserId);
            if (wallet == null) throw new Exception("Wallet not found");

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

            return order;
        }
    }

}
