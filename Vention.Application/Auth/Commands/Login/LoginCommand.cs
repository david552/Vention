using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vention.Application.Auth.Contracts;
using Vention.Application.Messaging;

namespace Vention.Application.Auth.Commands.Login
{
    public sealed record LoginCommand(string Email, string Password) : ICommand<AuthResponse>;
}
