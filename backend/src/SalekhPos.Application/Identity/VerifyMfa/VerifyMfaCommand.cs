// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

namespace SalekhPos.Application.Identity.VerifyMfa;

public sealed record VerifyMfaCommand(Guid UserId, string Code);
