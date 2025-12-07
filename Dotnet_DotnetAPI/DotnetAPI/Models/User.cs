namespace DotnetAPI.Models
{
    // Model: 特指表示数据的类, 它可能直接映射数据库表，也可能用于业务或前端传输
    public partial class User
    {
        public int UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Gender { get; set; }
        public bool Active { get; set; }

        public User()
        {
            if (FirstName == null)
                FirstName = "";
            if (LastName == null)
                LastName = "";
            if (Email == null)
                Email = "";
            if (Gender == null)
                Gender = "";
        }
    }
}
