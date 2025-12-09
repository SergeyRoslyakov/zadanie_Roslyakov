using System;

namespace MauiApp4.DTO
{
    public class ContactDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Email { get; set; } = "";

        public override string ToString()
        {
            return $"{FirstName} {LastName}";
        }
    }
}