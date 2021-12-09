namespace BookStoreAPI.Data.DTOs
{
    public class UserDTO
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class UserViewDTO
    {
        public string Id { get; set; }
        public string Username { get; set;}
        public string Email { get; set; }
    }
}
