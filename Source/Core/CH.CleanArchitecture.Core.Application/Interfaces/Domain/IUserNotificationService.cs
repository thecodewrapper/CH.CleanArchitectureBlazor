﻿using System;
using System.Threading.Tasks;
using CH.CleanArchitecture.Core.Domain.Entities.UserAggregate;

namespace CH.CleanArchitecture.Core.Application
{
    public interface IUserNotificationService
    {
        Task NotifyUserForAccountConfirmationAsync(User user, string confirmationUrl);
        Task NotifyUserForResetPasswordAsync(User user, string passwordResetUrl);
    }
}
