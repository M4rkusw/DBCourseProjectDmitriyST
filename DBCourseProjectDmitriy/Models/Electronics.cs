namespace DBCourseProjectDmitriy.Models
{
    public class Electronics : IEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public uint Price { get; set; }
        public uint ElectronicsInStock { get; set; } = 0;
        public uint SoldNumber { get; set; } = 0;
        public uint DeliveredNumber { get; set; } = 0;
        public ElectronicsType ElectronicsType { get; set; }
        public Manufacturer Manufacturer { get; set; }
    }
}
