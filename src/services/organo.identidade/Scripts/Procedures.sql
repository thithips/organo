-- Select User By Id
GO  
CREATE PROCEDURE aspnetuser_select_id  
    @id nvarchar(50)   
AS   
    SET NOCOUNT ON;  
    SELECT *  
    FROM  [dbo].[AspNetUsers] 
    WHERE Id = @id 
GO  

-- Select user by userName
GO  
CREATE PROCEDURE aspnetuser_select  
    @NormalizedUserName nvarchar(50)   
AS   
    SET NOCOUNT ON;  
    SELECT *  
    FROM  [dbo].[AspNetUsers] 
    WHERE NormalizedUserName = @NormalizedUserName 
GO  

-- Select user by email
GO  
CREATE PROCEDURE aspnetuser_select_email
    @email nvarchar(50)   
AS   
    SET NOCOUNT ON;  
    SELECT *  
    FROM  [dbo].[AspNetUsers] 
    WHERE Email = @email 
GO  

-- create user
GO  
CREATE PROCEDURE aspnetuser_create
@id NVARCHAR (450),
    @username nvarchar(50),  
	@passwordHash nvarchar(MAX),
	@email nvarchar(50)
AS   
    insert into  [dbo].[AspNetUsers] (Id, UserName, PasswordHash, Email)
    values(@id, @username, @passwordHash, @email) 
GO 

-- update user
GO  
CREATE PROCEDURE AspNetUsers_updateUser
@id NVARCHAR (450),
@userName  NVARCHAR (256), 
@normalizedUserName  NVARCHAR (256), 
@email  NVARCHAR (256), 
@normalizedEmail NVARCHAR (256), 
@passwordHash  NVARCHAR (MAX) 
--@EmailConfirmed bit, @SecurityStamp  NVARCHAR (MAX) , @ConcurrencyStamp  NVARCHAR (MAX) , @PhoneNumber NVARCHAR (MAX) , @PhoneNumberConfirmed bit, @TwoFactorEnabled bit, @LockoutEnd  DATETIMEOFFSET (7), @LockoutEnabled bit, @AccessFailedCount int
 
AS   
                UPDATE [dbo].[AspNetUsers]
                SET [UserName] = @UserName, 
                    [NormalizedUserName] = @NormalizedUserName, 
                    [Email] = @Email, 
                    [NormalizedEmail] = @NormalizedEmail,
                    [PasswordHash] = @PasswordHash
-- [EmailConfirmed] = @EmailConfirmed, [SecurityStamp] = @SecurityStamp, [ConcurrencyStamp] = @ConcurrencyStamp, [PhoneNumber] = @PhoneNumber, [PhoneNumberConfirmed] = @PhoneNumberConfirmed, [TwoFactorEnabled] = @TwoFactorEnabled, [LockoutEnd] = @LockoutEnd, [LockoutEnabled] = @LockoutEnabled, [AccessFailedCount] = @AccessFailedCount
                WHERE [Id] = @Id;
GO  
 

-- Delete user
GO  
CREATE PROCEDURE aspnetuser_del
@id NVARCHAR (450)
AS   
    DELETE FROM AspNetUsers WHERE Id = @Id;
GO 

-- select all users by claim
CREATE PROCEDURE AspNetUser_selectUsersByClaim
@claimType NVARCHAR (100),
@claimValue NVARCHAR (100)
AS   
    SELECT *
                FROM AspNetUsers AS user_
                INNER JOIN AspNetUserClaims AS user_claim ON user_.Id = user_claim.UserId
                WHERE user_claim.ClaimType = @claimType AND user_claim.ClaimValue = @claimValue;
GO 

-- select claims of user by userId
GO  
CREATE PROCEDURE AspNetUserClaims_selectByUserId
@userId NVARCHAR (450)
AS   
    select * from AspNetUserClaims where userId = @userId
GO  

-- delete all claims of a user
GO  
CREATE PROCEDURE AspNetUserClaims_deleteByUserId
@userId NVARCHAR (450)
AS   
    DELETE FROM [AspNetUserClaims] WHERE [UserId] = @UserId;
GO  

-- insert claims 
GO  
CREATE PROCEDURE AspNetUserClaims_insert
@userId NVARCHAR (450),
@claimType NVARCHAR (MAX),
@claimValue NVARCHAR (MAX)
AS   
    insert into AspNetUserClaims (userId, ClaimType, ClaimValue) values (@userId, @claimType, @claimValue);
GO  

use organoauth
select * from aspnetusers
select * from aspnetuserclaims
delete from aspnetusers 
delete from AspNetUserClaims where userid='b0600615-ef00-4ec7-b6b2-512893137200'
execute AspNetUserClaims_deleteByUserId @userId ='', @email = 'teste@GMAIL.COM', @passwordHash = 'teste', @username = 'testezao';

{
  "email": "vini.souza00@gmail.com",
  "password": "123456Zx!",
  "confirmedPassword": "123456Zx!"
}
