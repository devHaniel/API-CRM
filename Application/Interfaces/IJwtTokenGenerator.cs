using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain;
using Domain.Entities;

namespace Application.Interfaces{
    public interface IJwtTokenGenerator
    {
        string GenerarToken(Usuario usuario);
    }
}