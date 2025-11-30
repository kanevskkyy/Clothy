using Pulumi;
using Pulumi.Keycloak;
using Pulumi.Keycloak.Inputs;
using Pulumi.Keycloak.OpenId;
using System.Collections.Generic;

return await Deployment.RunAsync(() =>
{
    var config = new Pulumi.Config();
    var keycloakUrl = config.Get("keycloakUrl") ?? "http://localhost:8080";
    var realmName = "clothy-realm";
    var clientId = "clothy-api";
    var defaultPhotoUrl = "https://res.cloudinary.com/dkdljnfja/image/upload/v1763818143/Profile_Avatar_cfazhc.png";

    var realm = new Realm("clothy-realm", new RealmArgs
    {
        RealmName = realmName,
        Enabled = true,
        DisplayName = "Clothy Application",
        DisplayNameHtml = "<b>Clothy</b>",

        LoginWithEmailAllowed = true,
        DuplicateEmailsAllowed = false,
        RegistrationAllowed = true,
        RegistrationEmailAsUsername = true,
        EditUsernameAllowed = false,
        ResetPasswordAllowed = false,
        RememberMe = true,
        VerifyEmail = false,

        SslRequired = "none",
        PasswordPolicy = "length(8) and digits(1) and lowerCase(1) and upperCase(1) and specialChars(1)",

        AccessTokenLifespan = "15m",
        AccessTokenLifespanForImplicitFlow = "15m",
        SsoSessionIdleTimeout = "30m",
        SsoSessionMaxLifespan = "10h",
        OfflineSessionIdleTimeout = "720h",
        OfflineSessionMaxLifespan = "1440h",

        // SMTP налаштування для SendGrid
        SmtpServer = new RealmSmtpServerArgs
        {
            Host = "smtp.sendgrid.net",
            Port = "587",
            From = config.Require("smtpFrom"),
            FromDisplayName = "Clothy Application",
            ReplyTo = config.Get("smtpReplyTo") ?? config.Require("smtpFrom"),
            ReplyToDisplayName = "Clothy Support",

            Auth = new RealmSmtpServerAuthArgs
            {
                Username = "apikey",
                Password = config.RequireSecret("sendGridApiKey")
            },

            Starttls = true,
            Ssl = false,
            EnvelopeFrom = config.Get("smtpEnvelopeFrom")
        },

        Attributes = new InputMap<string>
        {
            ["internationalizationEnabled"] = "true",
            ["supportedLocales"] = "en,uk,ru",
            ["defaultLocale"] = "uk",
        }
    });

    var adminRole = new Role("admin-role", new RoleArgs
    {
        RealmId = realm.Id,
        Name = "Admin",
        Description = "Administrator role with full access"
    });

    var managerRole = new Role("manager-role", new RoleArgs
    {
        RealmId = realm.Id,
        Name = "Manager",
        Description = "Manager role with limited administrative access"
    });

    var userRole = new Role("user-role", new RoleArgs
    {
        RealmId = realm.Id,
        Name = "User",
        Description = "Regular user role"
    });

    var userProfile = new RealmUserProfile("user-profile", new RealmUserProfileArgs
    {
        RealmId = realm.Id,
        Attributes = new InputList<RealmUserProfileAttributeArgs>
        {
            new RealmUserProfileAttributeArgs
            {
                Name = "username",
                DisplayName = "${username}",
                Permissions = new RealmUserProfileAttributePermissionsArgs
                {
                    Edits = new InputList<string> { "admin", "user" },
                    Views = new InputList<string> { "admin", "user" }
                },
                Annotations = new InputMap<string>(),
            },
            new RealmUserProfileAttributeArgs
            {
                Name = "email",
                DisplayName = "${email}",
                RequiredForRoles = new InputList<string> { "user", "admin" },
                Permissions = new RealmUserProfileAttributePermissionsArgs
                {
                    Edits = new InputList<string> { "admin", "user" },
                    Views = new InputList<string> { "admin", "user" }
                },
            },
            new RealmUserProfileAttributeArgs
            {
                Name = "firstName",
                DisplayName = "${firstName}",
                RequiredForRoles = new InputList<string> { "user", "admin" },
                Permissions = new RealmUserProfileAttributePermissionsArgs
                {
                    Edits = new InputList<string> { "admin", "user" },
                    Views = new InputList<string> { "admin", "user" }
                },
            },
            new RealmUserProfileAttributeArgs
            {
                Name = "lastName",
                DisplayName = "${lastName}",
                RequiredForRoles = new InputList<string> { "user", "admin" },
                Permissions = new RealmUserProfileAttributePermissionsArgs
                {
                    Edits = new InputList<string> { "admin", "user" },
                    Views = new InputList<string> { "admin", "user" }
                },
            },
            new RealmUserProfileAttributeArgs
            {
                Name = "phoneNumber",
                DisplayName = "Phone Number",
                RequiredForRoles = new InputList<string> { "user", "admin" },
                Permissions = new RealmUserProfileAttributePermissionsArgs
                {
                    Edits = new InputList<string> { "admin", "user" },
                    Views = new InputList<string> { "admin", "user" }
                },
                Annotations = new InputMap<string>
                {
                    ["inputType"] = "tel"
                }
            },
            new RealmUserProfileAttributeArgs
            {
                Name = "photoURL",
                DisplayName = "Photo URL",
                RequiredForRoles = new InputList<string> { "user", "admin" },
                Permissions = new RealmUserProfileAttributePermissionsArgs
                {
                    Edits = new InputList<string> { "admin", "user" },
                    Views = new InputList<string> { "admin", "user" }
                },
                Annotations = new InputMap<string>
                {
                    ["inputType"] = "url"
                }
            }
        }
    });

    var client = new Client("clothy-api-client", new ClientArgs
    {
        RealmId = realm.Id,
        ClientId = clientId,
        Name = "Clothy API",
        Description = "Main API client for Clothy application",
        Enabled = true,

        AccessType = "CONFIDENTIAL",
        StandardFlowEnabled = true,
        ImplicitFlowEnabled = false,
        DirectAccessGrantsEnabled = true,
        ServiceAccountsEnabled = true,

        ValidRedirectUris = new InputList<string>
        {
            "https://localhost:5000/*",
            "https://localhost:5003/*",
            "http://localhost:3000/*",
            "http://localhost:5173/*"
        },

        ValidPostLogoutRedirectUris = new InputList<string>
        {
            "https://localhost:5000/*",
            "https://localhost:5003/*",
            "http://localhost:3000/*"
        },

        WebOrigins = new InputList<string>
        {
            "https://localhost:5000",
            "https://localhost:5003",
            "http://localhost:3000",
            "http://localhost:5173"
        },

        AdminUrl = "https://localhost:5003",
        BaseUrl = "https://localhost:5003",
        RootUrl = "https://localhost:5003",

        FrontchannelLogoutEnabled = true,
        BackchannelLogoutSessionRequired = true,
        ConsentRequired = false,

        PkceCodeChallengeMethod = "S256",
        UseRefreshTokens = true,
        UseRefreshTokensClientCredentials = true,

        ClientSessionIdleTimeout = "1800",
        ClientSessionMaxLifespan = "36000",
        ClientOfflineSessionIdleTimeout = "2592000",
        ClientOfflineSessionMaxLifespan = "5184000",
    });

    var audienceMapper = new AudienceProtocolMapper("audience-mapper", new AudienceProtocolMapperArgs
    {
        RealmId = realm.Id,
        ClientId = client.Id,
        Name = "clothy-api-audience",

        IncludedClientAudience = clientId,
        AddToAccessToken = true,
        AddToIdToken = true,
    });

    var realmRoleMapper = new UserRealmRoleProtocolMapper("realm-role-mapper", new UserRealmRoleProtocolMapperArgs
    {
        RealmId = realm.Id,
        ClientId = client.Id,
        Name = "realm-roles-mapper",

        ClaimName = "roles",
        ClaimValueType = "String",
        Multivalued = true,

        AddToAccessToken = true,
        AddToIdToken = true,
        AddToUserinfo = true,
    });

    var phoneNumberMapper = new UserAttributeProtocolMapper("phone-mapper", new UserAttributeProtocolMapperArgs
    {
        RealmId = realm.Id,
        ClientId = client.Id,
        Name = "phone-number-mapper",

        UserAttribute = "phoneNumber",
        ClaimName = "phone_number",
        ClaimValueType = "String",

        AddToAccessToken = true,
        AddToIdToken = true,
        AddToUserinfo = true,
    });

    var photoUrlMapper = new UserAttributeProtocolMapper("photo-url-mapper", new UserAttributeProtocolMapperArgs
    {
        RealmId = realm.Id,
        ClientId = client.Id,
        Name = "photo-url-mapper",

        UserAttribute = "photoURL",
        ClaimName = "photo_url",
        ClaimValueType = "String",

        AddToAccessToken = true,
        AddToIdToken = true,
        AddToUserinfo = true,
    });

    var userIdMapper = new UserAttributeProtocolMapper("user-id-mapper", new UserAttributeProtocolMapperArgs
    {
        RealmId = realm.Id,
        ClientId = client.Id,
        Name = "user-id-mapper",

        UserAttribute = "id",
        ClaimName = "user_id",
        ClaimValueType = "String",

        AddToAccessToken = true,
        AddToIdToken = true,
        AddToUserinfo = true,
    });

    var adminUser = new User("admin-user", new UserArgs
    {
        RealmId = realm.Id,
        Username = "admin",
        Email = config.Require("adminEmail"),
        EmailVerified = true,
        Enabled = true,

        FirstName = "Admin",
        LastName = "User",

        InitialPassword = new UserInitialPasswordArgs
        {
            Value = config.RequireSecret("adminPassword"),
            Temporary = false
        },

        Attributes = new InputMap<string>
        {
            ["phoneNumber"] = "+380123456789",
            ["photoURL"] = defaultPhotoUrl
        }
    }, new CustomResourceOptions
    {
        DependsOn = { userProfile }
    });

    var managerUser = new User("manager-user", new UserArgs
    {
        RealmId = realm.Id,
        Username = "manager",
        Email = config.Require("managerEmail"),
        EmailVerified = true,
        Enabled = true,

        FirstName = "Manager",
        LastName = "User",

        InitialPassword = new UserInitialPasswordArgs
        {
            Value = config.RequireSecret("managerPassword"),
            Temporary = false
        },

        Attributes = new InputMap<string>
        {
            ["phoneNumber"] = "+380956145258",
            ["photoURL"] = defaultPhotoUrl
        }
    }, new CustomResourceOptions
    {
        DependsOn = { userProfile }
    });

    var regularUser = new User("regular-user", new UserArgs
    {
        RealmId = realm.Id,
        Username = "user",
        Email = config.Require("userEmail"),
        EmailVerified = true,
        Enabled = true,

        FirstName = "Regular",
        LastName = "User",

        InitialPassword = new UserInitialPasswordArgs
        {
            Value = config.RequireSecret("userPassword"),
            Temporary = false
        },

        Attributes = new InputMap<string>
        {
            ["phoneNumber"] = "+380987654321",
            ["photoURL"] = defaultPhotoUrl
        }
    }, new CustomResourceOptions
    {
        DependsOn = { userProfile }
    });

    var adminUserRoles = new UserRoles("admin-user-roles", new UserRolesArgs
    {
        RealmId = realm.Id,
        UserId = adminUser.Id,
        RoleIds = new InputList<string>
        {
            adminRole.Id,
            userRole.Id
        }
    });

    var managerUserRoles = new UserRoles("manager-user-roles", new UserRolesArgs
    {
        RealmId = realm.Id,
        UserId = managerUser.Id,
        RoleIds = new InputList<string>
        {
            managerRole.Id,
            userRole.Id
        }
    });

    var regularUserRoles = new UserRoles("regular-user-roles", new UserRolesArgs
    {
        RealmId = realm.Id,
        UserId = regularUser.Id,
        RoleIds = new InputList<string>
        {
            userRole.Id
        }
    });

    return new Dictionary<string, object?>
    {
        ["realmName"] = realm.RealmName,
        ["realmId"] = realm.Id,
        ["clientId"] = client.ClientId,
        ["clientSecret"] = client.ClientSecret,
        ["keycloakUrl"] = keycloakUrl,
        ["tokenEndpoint"] = Output.Format($"{keycloakUrl}/realms/{realmName}/protocol/openid-connect/token"),
        ["authEndpoint"] = Output.Format($"{keycloakUrl}/realms/{realmName}/protocol/openid-connect/auth"),
        ["userInfoEndpoint"] = Output.Format($"{keycloakUrl}/realms/{realmName}/protocol/openid-connect/userinfo"),
        ["logoutEndpoint"] = Output.Format($"{keycloakUrl}/realms/{realmName}/protocol/openid-connect/logout"),
        ["adminUsername"] = adminUser.Username,
        ["adminEmail"] = adminUser.Email,
        ["managerUsername"] = managerUser.Username,
        ["managerEmail"] = managerUser.Email,
        ["regularUsername"] = regularUser.Username,
        ["regularEmail"] = regularUser.Email,
    };
});