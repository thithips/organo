using Dapper;
using Microsoft.AspNetCore.Identity;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Security.Claims;

namespace Organo.Auth.Data
{
    public class IdentityUserRepository : UserStoreBase<IdentityUser, string, IdentityUserClaim<string>, IdentityUserLogin<string>, IdentityUserToken<string>>
    {
        private IList<IdentityUserClaim<string>> UserClaims { get; set; }
        private IList<IdentityUserRole<string>> UserRoles { get; set; }
        private IList<IdentityUserLogin<string>> UserLogins { get; set; }
        private IList<IdentityUserToken<string>> UserTokens { get; set; }

        public static DbConnection GetOpenConnection()
        {
            var connection = new SqlConnection("Server=DESKTOP-R9JFMSC\\SQLEXPRESS;Database=OrganoAuth;Trusted_Connection=True;MultipleActiveResultSets=true");
            connection.Open();
            return connection;
        }

        public IdentityUserRepository(IdentityErrorDescriber describer) : base(describer)
        {

        }

        public override IQueryable<IdentityUser> Users => throw new NotImplementedException();

        public override async Task<IdentityUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken = default)
        {
            using (var connection = GetOpenConnection())
            {
                var p = new DynamicParameters();
                p.Add("@NormalizedUserName", normalizedUserName);
                var user = await connection.QueryAsync<IdentityUser>("aspnetuser_select", p, commandType: CommandType.StoredProcedure);
                return await connection.QueryFirstOrDefaultAsync<IdentityUser>("select * from [dbo].[AspNetUsers] where normalizedUserName = @name", new { name = normalizedUserName });
            }
        }

        public override async Task<IdentityUser> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default)
        {
            using (var connection = GetOpenConnection())
            {
                return await connection.QueryFirstOrDefaultAsync<IdentityUser>("select * from [dbo].[AspNetUsers] where email = @email", new { email = normalizedEmail });
            }
        }

        public override async Task<IdentityResult> CreateAsync(IdentityUser user, CancellationToken cancellationToken = default)
        {
            using (var connection = GetOpenConnection())
            {
                await connection.ExecuteAsync("insert into [dbo].[AspNetUsers](id, username,normalizedusername,PasswordHash,email) values(@id, @userName, @normalizedUserName, @pass, @email)", new { id = user.Id, userName = user.UserName, normalizedUserName = user.NormalizedUserName, pass = user.PasswordHash, email = user.Email });
            }

            return IdentityResult.Success;
        }

        public override async Task AddClaimsAsync(IdentityUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken = default)
        {
            using (var connection = GetOpenConnection())
            {
                UserClaims = (await connection.QueryAsync<IdentityUserClaim<string>>(@"SELECT * FROM [dbo].[AspNetUserClaims]
                WHERE [UserId] = @UserId;", new { UserId = user.Id })).ToList();

                foreach (var claim in claims)
                {
                    UserClaims.Add(CreateUserClaim(user, claim));
                }
            }
        }

        public override Task AddLoginAsync(IdentityUser user, UserLoginInfo login, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public override Task<IdentityResult> DeleteAsync(IdentityUser user, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public override Task<IdentityUser> FindByIdAsync(string userId, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public override async Task<IList<Claim>> GetClaimsAsync(IdentityUser user, CancellationToken cancellationToken = default)
        {
            using (var connection = GetOpenConnection())
            {
                const string sql = @"
                SELECT *
                FROM [dbo].[AspNetUserClaims]
                WHERE [UserId] = @UserId;";
                var userClaims = await connection.QueryAsync<IdentityUserClaim<string>>(sql, new { UserId = user.Id });
                return userClaims.Select(x => new Claim(x.ClaimType, x.ClaimValue)).ToList();
            }
        }

        public override Task<IList<UserLoginInfo>> GetLoginsAsync(IdentityUser user, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public override Task<IList<IdentityUser>> GetUsersForClaimAsync(Claim claim, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public override Task RemoveClaimsAsync(IdentityUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public override Task RemoveLoginAsync(IdentityUser user, string loginProvider, string providerKey, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public override Task ReplaceClaimAsync(IdentityUser user, Claim claim, Claim newClaim, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public override async Task<IdentityResult> UpdateAsync(IdentityUser user, CancellationToken cancellationToken = default)
        {
            using (var connection = GetOpenConnection())
            {
                var updated = await UpdateAsyncTeste(user, UserClaims, UserRoles, UserLogins, UserTokens);
                return updated ? IdentityResult.Success : IdentityResult.Failed(new IdentityError
                {
                    Code = string.Empty,
                    Description = $"User '{user.UserName}' could not be deleted."
                });
            }
        }

        private async Task<bool> UpdateAsyncTeste(IdentityUser user, IList<IdentityUserClaim<string>> claims, IList<IdentityUserRole<string>> roles, IList<IdentityUserLogin<string>> logins, IList<IdentityUserToken<string>> tokens)
        {
            using (var connection = GetOpenConnection())
            {
                const string updateUserSql = @"
                UPDATE [dbo].[AspNetUsers]
                SET [UserName] = @UserName, 
                    [NormalizedUserName] = @NormalizedUserName, 
                    [Email] = @Email, 
                    [NormalizedEmail] = @NormalizedEmail, 
                    [EmailConfirmed] = @EmailConfirmed, 
                    [PasswordHash] = @PasswordHash, 
                    [SecurityStamp] = @SecurityStamp, 
                    [ConcurrencyStamp] = @ConcurrencyStamp, 
                    [PhoneNumber] = @PhoneNumber, 
                    [PhoneNumberConfirmed] = @PhoneNumberConfirmed, 
                    [TwoFactorEnabled] = @TwoFactorEnabled, 
                    [LockoutEnd] = @LockoutEnd, 
                    [LockoutEnabled] = @LockoutEnabled, 
                    [AccessFailedCount] = @AccessFailedCount
                WHERE [Id] = @Id;
            ";
                using (var transaction = connection.BeginTransaction())
                {
                    await connection.ExecuteAsync(updateUserSql, new
                    {
                        user.UserName,
                        user.NormalizedUserName,
                        user.Email,
                        user.NormalizedEmail,
                        user.EmailConfirmed,
                        user.PasswordHash,
                        user.SecurityStamp,
                        user.ConcurrencyStamp,
                        user.PhoneNumber,
                        user.PhoneNumberConfirmed,
                        user.TwoFactorEnabled,
                        user.LockoutEnd,
                        user.LockoutEnabled,
                        user.AccessFailedCount,
                        user.Id
                    }, transaction);
                    if (claims?.Count() > 0)
                    {
                        const string deleteClaimsSql = @"
                        DELETE 
                        FROM [dbo].[AspNetUserClaims]
                        WHERE [UserId] = @UserId;
                    ";
                        await connection.ExecuteAsync(deleteClaimsSql, new { UserId = user.Id }, transaction);
                        const string insertClaimsSql = @"
                        INSERT INTO [dbo].[AspNetUserClaims] (UserId, ClaimType, ClaimValue)
                        VALUES (@UserId, @ClaimType, @ClaimValue);
                    ";
                        await connection.ExecuteAsync(insertClaimsSql, claims.Select(x => new
                        {
                            UserId = user.Id,
                            x.ClaimType,
                            x.ClaimValue
                        }), transaction);
                    }
                    if (roles?.Count() > 0)
                    {
                        const string deleteRolesSql = @"
                        DELETE
                        FROM [dbo].[AspNetUserRoles]
                        WHERE [UserId] = @UserId;
                    ";
                        await connection.ExecuteAsync(deleteRolesSql, new { UserId = user.Id }, transaction);
                        const string insertRolesSql = @"
                        INSERT INTO [dbo].[AspNetUserRoles] (UserId, RoleId)
                        VALUES (@UserId, @RoleId);
                    ";
                        await connection.ExecuteAsync(insertRolesSql, roles.Select(x => new
                        {
                            UserId = user.Id,
                            x.RoleId
                        }), transaction);
                    }
                    if (logins?.Count() > 0)
                    {
                        const string deleteLoginsSql = @"
                        DELETE
                        FROM [dbo].[AspNetUserLogins]
                        WHERE [UserId] = @UserId;
                    ";
                        await connection.ExecuteAsync(deleteLoginsSql, new { UserId = user.Id }, transaction);
                        const string insertLoginsSql = @"
                        INSERT INTO [dbo].[AspNetUserLogins] (LoginProvider, ProviderKey, ProviderDisplayName, UserId)
                        VALUES (@LoginProvider, @ProviderKey, @ProviderDisplayName, @UserId);
                    ";
                        await connection.ExecuteAsync(insertLoginsSql, logins.Select(x => new
                        {
                            x.LoginProvider,
                            x.ProviderKey,
                            x.ProviderDisplayName,
                            UserId = user.Id
                        }), transaction);
                    }
                    if (tokens?.Count() > 0)
                    {
                        const string deleteTokensSql = @"
                        DELETE
                        FROM [dbo].[AspNetUserTokens]
                        WHERE [UserId] = @UserId;
                    ";
                        await connection.ExecuteAsync(deleteTokensSql, new { UserId = user.Id }, transaction);
                        const string insertTokensSql = @"
                        INSERT INTO [dbo].[AspNetUserTokens] (UserId, LoginProvider, Name, Value)
                        VALUES (@UserId, @LoginProvider, @Name, @Value);
                    ";
                        await connection.ExecuteAsync(insertTokensSql, tokens.Select(x => new
                        {
                            x.UserId,
                            x.LoginProvider,
                            x.Name,
                            x.Value
                        }), transaction);
                    }
                    try
                    {
                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        return false;
                    }
                }
                return true;
            }
        }

        protected override Task AddUserTokenAsync(IdentityUserToken<string> token)
        {
            throw new NotImplementedException();
        }

        protected override Task<IdentityUserToken<string>> FindTokenAsync(IdentityUser user, string loginProvider, string name, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        protected override Task<IdentityUser> FindUserAsync(string userId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        protected override Task<IdentityUserLogin<string>> FindUserLoginAsync(string userId, string loginProvider, string providerKey, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        protected override Task<IdentityUserLogin<string>> FindUserLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        protected override Task RemoveUserTokenAsync(IdentityUserToken<string> token)
        {
            throw new NotImplementedException();
        }
    }
}
