using Microsoft.AspNetCore.Mvc;
using Stock.API.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Stock.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        public ProductsController(IProductService productService) => _productService = productService;

        [HttpGet]
        public async Task<List<Models.Product>> Get()
        {
            return await _productService.ProductListAsync();
        }

        [HttpGet("/GetByProductId/{productId}")]
        public async Task<ActionResult<Models.Product>> GetByProductId(int productId)
        {
            var product = await _productService.GetProductByIdAsync(productId);
            if (product is null) return NotFound();
            return product;
        }

        [HttpPost]
        public async Task<IActionResult> Post(Models.Product product)
        {
            await _productService.AddProductAsync(product);
            return CreatedAtAction(nameof(GetByProductId), new { productId = product.ProductId }, product);
        }

        [HttpPut("{productId}")]
        public async Task<IActionResult> Update(int productId, Models.Product newProduct)
        {
            var existProduct = await _productService.GetProductByIdAsync(productId);
            
            if (existProduct is null) return NotFound();

            newProduct.Id = existProduct.Id;
            await _productService.UpdateProductAsync(productId, newProduct);

            return Ok();
        }

        [HttpDelete("{productId}")]
        public async Task<IActionResult> Delete(int productId)
        {
            var product = await _productService.GetProductByIdAsync(productId);
            if (product is null) return NotFound();
            await _productService.DeleteProductAsync(productId);

            return Ok();
        }
    }
}
