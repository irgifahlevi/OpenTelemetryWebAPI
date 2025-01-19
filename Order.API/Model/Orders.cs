namespace Order.API.Model
{
    public class Orders
    {
        public int Id { get; set; }
        public int ProductId {get; set; }
        public int Quantity { get; set; }
        public DateTime OrderDate { get; set; }
    }
}
