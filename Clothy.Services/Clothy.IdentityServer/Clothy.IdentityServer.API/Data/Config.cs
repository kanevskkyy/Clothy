using Duende.IdentityServer.Models;

namespace Clothy.IdentityServer.API.Data
{
    public static class Config
    {
        public static IEnumerable<IdentityResource> IdentityResources =>
            new IdentityResource[]
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new IdentityResources.Email()
            };

        public static IEnumerable<ApiScope> ApiScopes =>
            new[]
            {
                new ApiScope("catalog-api", "Catalog Service API")
                {
                    UserClaims = {
                        "role",
                        "name",
                        "email",
                        "given_name",
                        "family_name",
                        "phone_number",
                        "photo_url"
                    }
                },
                new ApiScope("orders-api", "Orders Service API")
                {
                    UserClaims = {
                        "role",
                        "name",
                        "email",
                        "given_name",
                        "family_name",
                        "phone_number",
                        "photo_url"
                    }
                },
                new ApiScope("reviews-api", "Reviews Service API")
                {
                    UserClaims = {
                        "role",
                        "name",
                        "email",
                        "given_name",
                        "family_name",
                        "phone_number",
                        "photo_url"
                    }
                },
                new ApiScope("aggregator-api", "Aggregator Service API")
                {
                    UserClaims = {
                        "role",
                        "name",
                        "email",
                        "given_name",
                        "family_name",
                        "phone_number",
                        "photo_url"
                    }
                },
                new ApiScope("basket-api", "Basket Service API")
                {
                    UserClaims = {
                        "role",
                        "name",
                        "email",
                        "given_name",
                        "family_name",
                        "phone_number",
                        "photo_url"
                    }
                }
            };

        public static IEnumerable<Client> Clients =>
            new[]
            {
                new Client
                {
                    ClientId = "swagger",
                    ClientName = "Swagger UI (Clothy Services)",
                    AllowedGrantTypes = GrantTypes.Code,
                    RequirePkce = true,
                    RequireClientSecret = false,
                    AllowAccessTokensViaBrowser = true,
                    AllowOfflineAccess = true,
                    AccessTokenLifetime = 3600,

                    RefreshTokenUsage = TokenUsage.ReUse,
                    RefreshTokenExpiration = TokenExpiration.Sliding,
                    SlidingRefreshTokenLifetime = 2592000,

                    RedirectUris =
                    {
                        "https://localhost:5001/swagger/oauth2-redirect.html",
                        "https://localhost:5002/swagger/oauth2-redirect.html",
                        "https://localhost:5003/swagger/oauth2-redirect.html",
                        "https://localhost:5004/swagger/oauth2-redirect.html",
                        "https://localhost:5006/swagger/oauth2-redirect.html"
                    },

                    AllowedCorsOrigins =
                    {
                        "https://localhost:5001",
                        "https://localhost:5002",
                        "https://localhost:5003",
                        "https://localhost:5004",
                        "https://localhost:5006"
                    },

                    AllowedScopes =
                    {
                        "openid",
                        "profile",
                        "email",
                        "offline_access",
                        "catalog-api",
                        "orders-api",
                        "reviews-api",
                        "aggregator-api",
                        "basket-api"
                    }
                },

                new Client
                {
                    ClientId = "postman",
                    ClientName = "Postman Testing Client",
                    AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                    ClientSecrets = { new Secret("postman-secret".Sha256()) },
                    AllowAccessTokensViaBrowser = true,
                    AllowOfflineAccess = true,
                    AccessTokenLifetime = 3600,

                    RefreshTokenUsage = TokenUsage.ReUse,
                    RefreshTokenExpiration = TokenExpiration.Sliding,
                    SlidingRefreshTokenLifetime = 2592000,
                    AbsoluteRefreshTokenLifetime = 2592000,

                    AllowedScopes =
                    {
                        "openid",
                        "profile",
                        "email",
                        "offline_access",
                        "catalog-api",
                        "orders-api",
                        "reviews-api",
                        "aggregator-api",
                        "basket-api"
                    }
                },

                new Client
                {
                    ClientId = "clothy-web",
                    ClientName = "Clothy Web Application",
                    AllowedGrantTypes = GrantTypes.Code,

                    RequirePkce = true,
                    RequireClientSecret = false,

                    AllowAccessTokensViaBrowser = true,
                    AllowOfflineAccess = true,

                    AccessTokenLifetime = 3600,
                    RefreshTokenUsage = TokenUsage.OneTimeOnly,
                    RefreshTokenExpiration = TokenExpiration.Sliding,
                    SlidingRefreshTokenLifetime = 2592000,

                    RedirectUris =
                    {
                        "https://localhost:3000/callback",
                        "https://localhost:3000/silent-renew"
                    },

                    PostLogoutRedirectUris =
                    {
                        "https://localhost:3000/signout-callback"
                    },

                    AllowedCorsOrigins =
                    {
                        "https://localhost:3000"
                    },

                    AllowedScopes =
                    {
                        "openid",
                        "profile",
                        "email",
                        "offline_access",
                        "catalog-api",
                        "orders-api",
                        "reviews-api",
                        "aggregator-api",
                        "basket-api"
                    }
                },

                new Client
                {
                    ClientId = "aggregator-service",
                    ClientName = "Aggregator Service (M2M)",
                    AllowedGrantTypes = GrantTypes.ClientCredentials, 
                    
                    ClientSecrets =
                    {
                        new Secret("aggregator-super-secret-key-12345".Sha256())
                    },

                    AccessTokenLifetime = 3600,

                    AllowedScopes =
                    {
                        "catalog-api",
                        "orders-api",
                        "reviews-api",
                        "basket-api"
                    },

                    Claims =
                    {
                        new ClientClaim("service_name", "aggregator"),
                        new ClientClaim("service_type", "internal")
                    }
                },

                new Client
                {
                    ClientId = "orders-service",
                    ClientName = "Orders Service (M2M)",
                    AllowedGrantTypes = GrantTypes.ClientCredentials,

                    ClientSecrets =
                    {
                        new Secret("orders-super-secret-key-67890".Sha256())
                    },

                    AccessTokenLifetime = 3600,

                    AllowedScopes =
                    {
                        "catalog-api",
                        "basket-api"
                    },

                    Claims =
                    {
                        new ClientClaim("service_name", "orders"),
                        new ClientClaim("service_type", "internal")
                    }
                },

                new Client
                {
                    ClientId = "basket-service",
                    ClientName = "Basket Service (M2M)",
                    AllowedGrantTypes = GrantTypes.ClientCredentials,

                    ClientSecrets =
                    {
                        new Secret("basket-super-secret-key-abcde".Sha256())
                    },

                    AccessTokenLifetime = 3600,

                    AllowedScopes =
                    {
                        "catalog-api",
                        "orders-api"
                    },

                    Claims =
                    {
                        new ClientClaim("service_name", "basket"),
                        new ClientClaim("service_type", "internal")
                    }
                }
            };
    }
}