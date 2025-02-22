namespace Order.API.DTO
{
    public class OrderRequest
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
