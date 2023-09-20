namespace DBCourseProjectDmitriy.Models
{
    public class OrderedGoods : IEntity
    {
        public int Id { get; set; }
        public uint Quantity { get; set; }
        public Electronics Electronics { get; set; }
        public uint Sum { get; set; }
        public PurchaseOrder? PurchaseOrder { get; set; }
    }
}
