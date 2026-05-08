using Microsoft.AspNetCore.Identity;
namespace CanteenReservationSystem.Models;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; }
    public string Role { get; set; } // Client, Admin, Manager, Staff
    public int BlackPoints { get; set; }
}