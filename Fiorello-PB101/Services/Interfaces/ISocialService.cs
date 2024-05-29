﻿using Fiorello_PB101.Models;

namespace Fiorello_PB101.Services.Interfaces
{
    public interface ISocialService
    {
        Task<IEnumerable<Social>> GetAllAsync();
    }
}
