using CoreCrudWithJwt.Migrations;
using Microsoft.Extensions.Caching.Memory;

namespace CoreCrudWithJwt.Services
{
    public class CacheServices
    {

    }
    public class ProductService
    {
        private readonly IMemoryCache _cache;

        public ProductService(IMemoryCache cache) => _cache = cache; //

        public async Task<List<Product>> GetProductsAsync()
        {
            string cacheKey = "product_list";

            // Attempt to fetch from cache, otherwise pull from DB and store it
            if (!_cache.TryGetValue(cacheKey, value: out List<Product> products)) //
            {
                products = await GetProductsAsync();

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(10)) // Max total lifespan
                    .SetSlidingExpiration(TimeSpan.FromMinutes(2))   // Lifespan resets if accessed
                    .SetPriority(CacheItemPriority.Normal);          // Priority for memory pressure eviction

                _cache.Set(cacheKey, products, cacheOptions); //
            }

            return products;
        }
    }
}
