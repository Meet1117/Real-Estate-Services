using System.ComponentModel.DataAnnotations;

namespace Real_Estate_Services.Models
{
    public class Users
    {
        [Key]
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public bool IsActive { get; set; }
        public int LoginAttempt { get; set; }

        //Last Login Time 
        public DateTime? LastLogin { get; set; } = DateTime.Now;


        //Failed Attempt
        public DateTime? LastLoginAttemptTime { get; set; }
        public DateTime? LastLogoutEndTime { get; set; }
    }
}
