namespace ChatServer.Models
{
    public class UserRecord(string id, string nickname, string passwordHash)
    {
        public string Id { get; set; } = id;
        public string Nickname { get; set; } = nickname;
        public string PasswordHash { get; set; } = passwordHash;
    }
}
