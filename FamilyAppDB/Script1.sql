CREATE TABLE [dbo].[Users]
(
	[Id] INT NOT NULL PRIMARY KEY, 
    [FirstName] VARCHAR(50) NOT NULL, 
    [LastName] VARCHAR(50) NOT NULL, 
    [Password] VARCHAR(50) NOT NULL
)
insert into Users
values (1, 'Josh', 'Robertson', 'password');

select * from users