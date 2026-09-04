// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

namespace SalekhPos.Application.Identity.ResetPassword;

public sealed record ResetPasswordCommand(string RawToken, string NewPassword);
