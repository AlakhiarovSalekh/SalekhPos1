// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

using SalekhPos.Domain.Common;

namespace SalekhPos.Domain.Errors;

/// <summary>
/// Raised when a store code value is malformed. In normal operation the
/// <c>StoreCode.Create</c> factory throws <see cref="ArgumentException"/>
/// directly; this domain exception is the typed carrier used by the
/// application / API layer to surface the error as an RFC 7807 response
/// (the mapping lands in Phase 3 Slice 4).
/// </summary>
public sealed class InvalidStoreCodeException : DomainException
{
    public InvalidStoreCodeException(string attempted)
        : base($"Store code '{attempted}' is not valid.")
    {
    }
}
