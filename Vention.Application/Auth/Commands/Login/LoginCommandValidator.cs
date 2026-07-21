using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vention.Application.Auth.Commands.Login
{
    public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
    {
        public LoginCommandValidator()
        {
            RuleFor(x => x.Email).NotEmpty().MaximumLength(320);
            RuleFor(x => x.Password).NotEmpty().MaximumLength(128);
        }
    }
}
