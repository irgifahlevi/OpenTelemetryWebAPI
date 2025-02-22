namespace Order.API.DTO
{
    public class ProductSyncResponse
    {
        public int StatusCode { get; set; }
        public string? Message { get; set; }
        public ProductData? Data { get; set; }
        public DateTime TimeStamp { get; set; }
        public double ExecutionTime { get; set; }
    }

    public class ProductData
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Sku { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
    }
}
