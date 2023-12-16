﻿using Microsoft.AspNetCore.Identity;

namespace ShittyOne.Entities
{
    public class User : IdentityUser<Guid>
    {
        public List<UserRefresh> Refreshes { get; set; } = new List<UserRefresh>();
        public List<Group> Groups { get; set; }
    }

   // public class UserConfiguraion
}
