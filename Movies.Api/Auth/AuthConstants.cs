﻿namespace Movies.Api.Auth;

public static class AuthConstants
{
    public const string AdminUserPolicyName = "Admin";
    public const string AdminUserClaimName = "admin";

    public const string TrustMemberPolicyName = "Trusted";
    public const string TrustMemberClaimName = "trusted_member";

    public const string ApiKeyHeaderName = "x-api-key";
}
