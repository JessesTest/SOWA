
ALTER TABLE [AspNetUsers] ADD [ConcurrencyStamp] nvarchar(max) NULL;
GO

ALTER TABLE [AspNetUsers] ADD [LockoutEnd] datetimeoffset NULL;
GO

ALTER TABLE [AspNetUsers] ADD [NormalizedEmail] nvarchar(256) NULL;
GO

ALTER TABLE [AspNetUsers] ADD [NormalizedUserName] nvarchar(256) NULL;
GO

ALTER TABLE [AspNetUserLogins] ADD [ProviderDisplayName] nvarchar(max) NULL;
GO

ALTER TABLE [AspNetRoles] ADD [ConcurrencyStamp] nvarchar(max) NULL;
GO

ALTER TABLE [AspNetRoles] ADD [NormalizedName] nvarchar(256) NULL;
GO

CREATE TABLE [AspNetRoleClaims] (
    [Id] int NOT NULL IDENTITY,
    [RoleId] nvarchar(128) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserTokens] (
    [UserId] nvarchar(128) NOT NULL,
    [LoginProvider] nvarchar(128) NOT NULL,
    [Name] nvarchar(128) NOT NULL,
    [Value] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
    CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [NormalizedEmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
GO

CREATE UNIQUE INDEX [NormalizedUserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE ([NormalizedUserName] IS NOT NULL);
GO

CREATE UNIQUE INDEX [RoleNormalizedNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE ([NormalizedName] IS NOT NULL);
GO

CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
GO

-- commented out when the identity database was split between solutions into old Identity and new Identity_SW
--
--CREATE TRIGGER dbo.OldIdentity_2_CoreIdentity ON dbo.AspNetUsers
--AFTER INSERT, UPDATE
--AS
--BEGIN
--UPDATE AspNetUsers SET NormalizedEmail = UPPER(email) WHERE NormalizedEmail IS NULL OR email <> UPPER(email)
--UPDATE AspNetUsers SET NormalizedUserName = UPPER(userName) WHERE NormalizedUserName IS NULL OR userName <> UPPER(userName)
--END
--GO

UPDATE AspNetUsers SET NormalizedEmail = UPPER(email) WHERE NormalizedEmail IS NULL OR email <> UPPER(email)
UPDATE AspNetUsers SET NormalizedUserName = UPPER(userName) WHERE NormalizedUserName IS NULL OR userName <> UPPER(userName)
