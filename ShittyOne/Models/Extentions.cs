﻿using System.Security.Claims;

namespace ShittyOne.Models
{
    public static class TokenExtentions
    {
        public static string GetId(this ClaimsPrincipal claimsPrincipal) => claimsPrincipal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value ?? "";
    }
}
