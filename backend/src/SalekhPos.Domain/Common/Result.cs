// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

namespace SalekhPos.Domain.Common;

/// <summary>
/// Discriminated result of a use case: either <typeparamref name="T"/> on
/// success, or a <see cref="DomainError"/> on a known business failure.
/// We never throw for expected outcomes; we throw only for unexpected
/// programmer/infrastructure errors.
/// </summary>
public readonly record struct Result<T>(bool IsSuccess, T? Value, DomainError? Error)
{
    // CA1000 discourages static members on generic types, but for a
    // discriminated union the canonical pattern is a static Success /
    // Failure factory on the type itself. We suppress the rule locally
    // rather than introduce a non-generic base type.
#pragma warning disable CA1000
    public static Result<T> Success(T value) => new(true, value, null);

    public static Result<T> Failure(DomainError error) => new(false, default, error);

    public static Result<T> Failure(string code, string message, string? field = null) =>
        new(false, default, new DomainError(code, message, field));
#pragma warning restore CA1000
}
