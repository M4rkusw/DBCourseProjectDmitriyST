namespace DBCourseProjectDmitriy.Models
{
    public class Manufacturer : IEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Director { get; set; }
        public long BankDetails { get; set; }
    }
}
