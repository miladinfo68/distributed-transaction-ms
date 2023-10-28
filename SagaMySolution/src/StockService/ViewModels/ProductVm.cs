using StockService.Models;

namespace StockService.ViewModels
{
    public record ProductVm(int ProductId, int Count)
    {
        public static implicit operator Product(ProductVm model)
        {
            return new Product()
            {
                ProductId = model.ProductId,
                Count = model.Count,
            };
        }
    }
}
