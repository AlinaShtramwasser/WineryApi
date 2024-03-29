﻿using System.ComponentModel.DataAnnotations;

namespace WineryApi.Models
{
    //to add users through postman
    public class Register
    {
        [Required]
        public string ConfirmPassword { get; set; }
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
