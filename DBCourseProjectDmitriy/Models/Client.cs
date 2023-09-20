namespace DBCourseProjectDmitriy.Models
{
    public class Client : IEntity
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Telephone { get; set; }
        public long BankAccount { get; set; }
    }
}
