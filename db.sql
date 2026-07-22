/*
    BOOKVERSE - PORTABLE DATABASE SETUP & DEMO DATA
    SQL Server 2019+ / LocalDB / SQL Express

    Cach chay trong SSMS:
    1. Mo file nay.
    2. Ket noi SQL Server cua may.
    3. Bam Execute.

    Script khong hard-code duong dan MDF/LDF va co the chay lai.

    Tai khoan demo:
      Admin: admin@bookverse.vn / Admin@123
      User : user@bookverse.vn  / User@123

    Luu y: Mat khau seed de plain text de tuong thich co che fallback cua
    AccountController hien tai. Tai khoan dang ky tu giao dien van duoc bam BCrypt.
*/

USE [master];
GO

IF DB_ID(N'QuanLyBanSach') IS NULL
BEGIN
    CREATE DATABASE [QuanLyBanSach];
END;
GO

USE [QuanLyBanSach];
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

/* =========================
   1. TABLES
   ========================= */

IF OBJECT_ID(N'dbo.Roles', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Roles
    (
        RoleId   INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Roles PRIMARY KEY,
        RoleName NVARCHAR(50) NOT NULL CONSTRAINT UQ_Roles_RoleName UNIQUE
    );
END;
GO

IF OBJECT_ID(N'dbo.Users', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Users
    (
        UserId    INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Users PRIMARY KEY,
        FullName  NVARCHAR(100) NOT NULL,
        Email     NVARCHAR(100) NOT NULL,
        [Password] NVARCHAR(100) NOT NULL,
        Phone     NVARCHAR(20) NULL,
        [Address] NVARCHAR(255) NULL,
        RoleId    INT NOT NULL,
        IsActive  BIT NOT NULL CONSTRAINT DF_Users_IsActive DEFAULT (1),
        CreatedAt DATETIME NOT NULL CONSTRAINT DF_Users_CreatedAt DEFAULT (GETDATE()),
        CONSTRAINT UQ_Users_Email UNIQUE (Email),
        CONSTRAINT FK_Users_Roles FOREIGN KEY (RoleId) REFERENCES dbo.Roles(RoleId)
    );
END;
GO

IF OBJECT_ID(N'dbo.Categories', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Categories
    (
        CategoryId   INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Categories PRIMARY KEY,
        CategoryName NVARCHAR(100) NOT NULL,
        [Description] NVARCHAR(255) NULL,
        CONSTRAINT UQ_Categories_CategoryName UNIQUE (CategoryName)
    );
END;
GO

IF OBJECT_ID(N'dbo.Books', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Books
    (
        BookId      INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Books PRIMARY KEY,
        Title       NVARCHAR(255) NOT NULL,
        Author      NVARCHAR(150) NULL,
        Publisher   NVARCHAR(150) NULL,
        CategoryId  INT NULL,
        Price       DECIMAL(18,0) NOT NULL CONSTRAINT DF_Books_Price DEFAULT (0),
        Quantity    INT NOT NULL CONSTRAINT DF_Books_Quantity DEFAULT (0),
        [Image]     NVARCHAR(255) NULL,
        [Description] NVARCHAR(MAX) NULL,
        IsActive    BIT NOT NULL CONSTRAINT DF_Books_IsActive DEFAULT (1),
        CreatedAt   DATETIME NOT NULL CONSTRAINT DF_Books_CreatedAt DEFAULT (GETDATE()),
        CONSTRAINT CK_Books_Price CHECK (Price >= 0),
        CONSTRAINT CK_Books_Quantity CHECK (Quantity >= 0),
        CONSTRAINT FK_Books_Categories FOREIGN KEY (CategoryId)
            REFERENCES dbo.Categories(CategoryId)
    );
END;
GO

IF OBJECT_ID(N'dbo.Vouchers', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Vouchers
    (
        VoucherId       INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Vouchers PRIMARY KEY,
        Code            NVARCHAR(50) NOT NULL,
        DiscountPercent INT NOT NULL,
        ExpiryDate      DATETIME NULL,
        IsActive        BIT NOT NULL CONSTRAINT DF_Vouchers_IsActive DEFAULT (1),
        CONSTRAINT UQ_Vouchers_Code UNIQUE (Code),
        CONSTRAINT CK_Vouchers_Discount CHECK (DiscountPercent BETWEEN 1 AND 100)
    );
END;
GO

IF OBJECT_ID(N'dbo.Orders', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Orders
    (
        OrderId         INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Orders PRIMARY KEY,
        UserId          INT NOT NULL,
        VoucherId       INT NULL,
        TotalAmount     DECIMAL(18,0) NOT NULL CONSTRAINT DF_Orders_TotalAmount DEFAULT (0),
        ShippingAddress NVARCHAR(255) NULL,
        PhoneNumber     NVARCHAR(20) NULL,
        [Status]        NVARCHAR(50) NOT NULL CONSTRAINT DF_Orders_Status DEFAULT (N'Chờ xử lý'),
        PaymentMethod   NVARCHAR(50) NOT NULL CONSTRAINT DF_Orders_PaymentMethod DEFAULT (N'COD'),
        PaymentStatus   NVARCHAR(50) NOT NULL CONSTRAINT DF_Orders_PaymentStatus DEFAULT (N'Chưa thanh toán'),
        CreatedAt       DATETIME NOT NULL CONSTRAINT DF_Orders_CreatedAt DEFAULT (GETDATE()),
        CONSTRAINT CK_Orders_TotalAmount CHECK (TotalAmount >= 0),
        CONSTRAINT FK_Orders_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(UserId),
        CONSTRAINT FK_Orders_Vouchers FOREIGN KEY (VoucherId) REFERENCES dbo.Vouchers(VoucherId)
    );
END;
GO

IF OBJECT_ID(N'dbo.OrderDetails', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.OrderDetails
    (
        OrderDetailId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_OrderDetails PRIMARY KEY,
        OrderId       INT NOT NULL,
        BookId        INT NOT NULL,
        Quantity      INT NOT NULL,
        Price         DECIMAL(18,0) NOT NULL,
        CONSTRAINT CK_OrderDetails_Quantity CHECK (Quantity > 0),
        CONSTRAINT CK_OrderDetails_Price CHECK (Price >= 0),
        CONSTRAINT FK_OrderDetails_Orders FOREIGN KEY (OrderId)
            REFERENCES dbo.Orders(OrderId),
        CONSTRAINT FK_OrderDetails_Books FOREIGN KEY (BookId)
            REFERENCES dbo.Books(BookId)
    );
END;
GO

IF OBJECT_ID(N'dbo.Cart', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Cart
    (
        CartId   INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Cart PRIMARY KEY,
        UserId   INT NOT NULL,
        BookId   INT NOT NULL,
        Quantity INT NOT NULL CONSTRAINT DF_Cart_Quantity DEFAULT (1),
        CONSTRAINT CK_Cart_Quantity CHECK (Quantity > 0),
        CONSTRAINT UQ_Cart_User_Book UNIQUE (UserId, BookId),
        CONSTRAINT FK_Cart_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(UserId),
        CONSTRAINT FK_Cart_Books FOREIGN KEY (BookId) REFERENCES dbo.Books(BookId)
    );
END;
GO

IF OBJECT_ID(N'dbo.Reviews', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Reviews
    (
        ReviewId  INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Reviews PRIMARY KEY,
        BookId    INT NOT NULL,
        UserId    INT NOT NULL,
        Rating    INT NOT NULL,
        Comment   NVARCHAR(MAX) NOT NULL,
        IsApproved BIT NOT NULL CONSTRAINT DF_Reviews_IsApproved DEFAULT (0),
        CreatedAt DATETIME NOT NULL CONSTRAINT DF_Reviews_CreatedAt DEFAULT (GETDATE()),
        CONSTRAINT CK_Reviews_Rating CHECK (Rating BETWEEN 1 AND 5),
        CONSTRAINT FK_Reviews_Books FOREIGN KEY (BookId) REFERENCES dbo.Books(BookId),
        CONSTRAINT FK_Reviews_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(UserId)
    );
END;
GO

IF OBJECT_ID(N'dbo.Wishlist', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Wishlist
    (
        WishlistId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Wishlist PRIMARY KEY,
        UserId      INT NOT NULL,
        BookId      INT NOT NULL,
        CONSTRAINT UQ_Wishlist_User_Book UNIQUE (UserId, BookId),
        CONSTRAINT FK_Wishlist_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(UserId),
        CONSTRAINT FK_Wishlist_Books FOREIGN KEY (BookId) REFERENCES dbo.Books(BookId)
    );
END;
GO

/* =========================
   2. INDEXES
   ========================= */

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Books_Title' AND object_id = OBJECT_ID(N'dbo.Books'))
    CREATE INDEX IX_Books_Title ON dbo.Books(Title);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Orders_User_CreatedAt' AND object_id = OBJECT_ID(N'dbo.Orders'))
    CREATE INDEX IX_Orders_User_CreatedAt ON dbo.Orders(UserId, CreatedAt DESC);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Reviews_Book_Approved' AND object_id = OBJECT_ID(N'dbo.Reviews'))
    CREATE INDEX IX_Reviews_Book_Approved ON dbo.Reviews(BookId, IsApproved, CreatedAt DESC);
GO

/* =========================
   3. DEMO DATA (IDEMPOTENT)
   ========================= */

IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE RoleName = N'Admin')
    INSERT dbo.Roles(RoleName) VALUES (N'Admin');

IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE RoleName = N'User')
    INSERT dbo.Roles(RoleName) VALUES (N'User');
GO

DECLARE @AdminRoleId INT = (SELECT RoleId FROM dbo.Roles WHERE RoleName = N'Admin');
DECLARE @UserRoleId  INT = (SELECT RoleId FROM dbo.Roles WHERE RoleName = N'User');

IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Email = N'admin@bookverse.vn')
BEGIN
    INSERT dbo.Users(FullName, Email, [Password], Phone, [Address], RoleId, IsActive, CreatedAt)
    VALUES (N'Quản trị BookVerse', N'admin@bookverse.vn', N'Admin@123',
            N'0901000001', N'Hà Nội', @AdminRoleId, 1, DATEADD(DAY, -120, GETDATE()));
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Email = N'user@bookverse.vn')
BEGIN
    INSERT dbo.Users(FullName, Email, [Password], Phone, [Address], RoleId, IsActive, CreatedAt)
    VALUES (N'Nguyễn Văn Demo', N'user@bookverse.vn', N'User@123',
            N'0902000002', N'123 Đường Láng, Đống Đa, Hà Nội', @UserRoleId, 1, DATEADD(DAY, -60, GETDATE()));
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Email = N'lan@bookverse.vn')
BEGIN
    INSERT dbo.Users(FullName, Email, [Password], Phone, [Address], RoleId, IsActive, CreatedAt)
    VALUES (N'Trần Ngọc Lan', N'lan@bookverse.vn', N'User@123',
            N'0903000003', N'Cầu Giấy, Hà Nội', @UserRoleId, 1, DATEADD(DAY, -30, GETDATE()));
END;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Categories WHERE CategoryName = N'Văn học')
    INSERT dbo.Categories(CategoryName, [Description]) VALUES (N'Văn học', N'Tiểu thuyết, truyện ngắn và tác phẩm văn học.');
IF NOT EXISTS (SELECT 1 FROM dbo.Categories WHERE CategoryName = N'Kinh tế')
    INSERT dbo.Categories(CategoryName, [Description]) VALUES (N'Kinh tế', N'Kinh doanh, tài chính và quản trị.');
IF NOT EXISTS (SELECT 1 FROM dbo.Categories WHERE CategoryName = N'Kỹ năng sống')
    INSERT dbo.Categories(CategoryName, [Description]) VALUES (N'Kỹ năng sống', N'Phát triển bản thân và kỹ năng thực hành.');
IF NOT EXISTS (SELECT 1 FROM dbo.Categories WHERE CategoryName = N'Công nghệ thông tin')
    INSERT dbo.Categories(CategoryName, [Description]) VALUES (N'Công nghệ thông tin', N'Lập trình, hệ thống và công nghệ.');
IF NOT EXISTS (SELECT 1 FROM dbo.Categories WHERE CategoryName = N'Thiếu nhi')
    INSERT dbo.Categories(CategoryName, [Description]) VALUES (N'Thiếu nhi', N'Sách phù hợp cho trẻ em và gia đình.');
GO

DECLARE @VanHoc INT = (SELECT CategoryId FROM dbo.Categories WHERE CategoryName = N'Văn học');
DECLARE @KinhTe INT = (SELECT CategoryId FROM dbo.Categories WHERE CategoryName = N'Kinh tế');
DECLARE @KyNang INT = (SELECT CategoryId FROM dbo.Categories WHERE CategoryName = N'Kỹ năng sống');
DECLARE @CNTT INT = (SELECT CategoryId FROM dbo.Categories WHERE CategoryName = N'Công nghệ thông tin');
DECLARE @ThieuNhi INT = (SELECT CategoryId FROM dbo.Categories WHERE CategoryName = N'Thiếu nhi');

IF NOT EXISTS (SELECT 1 FROM dbo.Books WHERE Title = N'Nhà Giả Kim')
    INSERT dbo.Books(Title, Author, Publisher, CategoryId, Price, Quantity, [Image], [Description], IsActive)
    VALUES (N'Nhà Giả Kim', N'Paulo Coelho', N'NXB Hội Nhà Văn', @VanHoc, 79000, 30,
            N'https://images.unsplash.com/photo-1544947950-fa07a98d237f?w=600',
            N'Hành trình đi tìm kho báu và ý nghĩa của ước mơ.', 1);

IF NOT EXISTS (SELECT 1 FROM dbo.Books WHERE Title = N'Tuổi Trẻ Đáng Giá Bao Nhiêu')
    INSERT dbo.Books(Title, Author, Publisher, CategoryId, Price, Quantity, [Image], [Description], IsActive)
    VALUES (N'Tuổi Trẻ Đáng Giá Bao Nhiêu', N'Rosie Nguyễn', N'NXB Hội Nhà Văn', @KyNang, 90000, 25,
            N'https://images.unsplash.com/photo-1512820790803-83ca734da794?w=600',
            N'Những chia sẻ thực tế về học tập, trải nghiệm và trưởng thành.', 1);

IF NOT EXISTS (SELECT 1 FROM dbo.Books WHERE Title = N'Đắc Nhân Tâm')
    INSERT dbo.Books(Title, Author, Publisher, CategoryId, Price, Quantity, [Image], [Description], IsActive)
    VALUES (N'Đắc Nhân Tâm', N'Dale Carnegie', N'NXB Tổng Hợp TP.HCM', @KyNang, 86000, 40,
            N'https://images.unsplash.com/photo-1543002588-bfa74002ed7e?w=600',
            N'Các nguyên tắc giao tiếp và xây dựng mối quan hệ.', 1);

IF NOT EXISTS (SELECT 1 FROM dbo.Books WHERE Title = N'Tư Duy Nhanh Và Chậm')
    INSERT dbo.Books(Title, Author, Publisher, CategoryId, Price, Quantity, [Image], [Description], IsActive)
    VALUES (N'Tư Duy Nhanh Và Chậm', N'Daniel Kahneman', N'NXB Thế Giới', @KinhTe, 199000, 18,
            N'https://images.unsplash.com/photo-1495446815901-a7297e633e8d?w=600',
            N'Khám phá hai hệ thống tư duy chi phối quyết định của con người.', 1);

IF NOT EXISTS (SELECT 1 FROM dbo.Books WHERE Title = N'Clean Code')
    INSERT dbo.Books(Title, Author, Publisher, CategoryId, Price, Quantity, [Image], [Description], IsActive)
    VALUES (N'Clean Code', N'Robert C. Martin', N'Prentice Hall', @CNTT, 320000, 15,
            N'https://images.unsplash.com/photo-1532012197267-da84d127e765?w=600',
            N'Nguyên tắc và thực hành viết mã nguồn rõ ràng, dễ bảo trì.', 1);

IF NOT EXISTS (SELECT 1 FROM dbo.Books WHERE Title = N'ASP.NET Core in Action')
    INSERT dbo.Books(Title, Author, Publisher, CategoryId, Price, Quantity, [Image], [Description], IsActive)
    VALUES (N'ASP.NET Core in Action', N'Andrew Lock', N'Manning', @CNTT, 450000, 12,
            N'https://images.unsplash.com/photo-1516979187457-637abb4f9353?w=600',
            N'Hướng dẫn xây dựng ứng dụng web hiện đại với ASP.NET Core.', 1);

IF NOT EXISTS (SELECT 1 FROM dbo.Books WHERE Title = N'Dế Mèn Phiêu Lưu Ký')
    INSERT dbo.Books(Title, Author, Publisher, CategoryId, Price, Quantity, [Image], [Description], IsActive)
    VALUES (N'Dế Mèn Phiêu Lưu Ký', N'Tô Hoài', N'NXB Kim Đồng', @ThieuNhi, 65000, 35,
            N'https://images.unsplash.com/photo-1511108690759-009324a90311?w=600',
            N'Tác phẩm thiếu nhi kinh điển về hành trình của Dế Mèn.', 1);

IF NOT EXISTS (SELECT 1 FROM dbo.Books WHERE Title = N'Mắt Biếc')
    INSERT dbo.Books(Title, Author, Publisher, CategoryId, Price, Quantity, [Image], [Description], IsActive)
    VALUES (N'Mắt Biếc', N'Nguyễn Nhật Ánh', N'NXB Trẻ', @VanHoc, 110000, 22,
            N'https://images.unsplash.com/photo-1524578271613-d550eacf6090?w=600',
            N'Câu chuyện trong trẻo và day dứt về tình yêu tuổi trẻ.', 1);
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Vouchers WHERE Code = N'WELCOME10')
    INSERT dbo.Vouchers(Code, DiscountPercent, ExpiryDate, IsActive)
    VALUES (N'WELCOME10', 10, DATEADD(MONTH, 6, GETDATE()), 1);

IF NOT EXISTS (SELECT 1 FROM dbo.Vouchers WHERE Code = N'BOOK20')
    INSERT dbo.Vouchers(Code, DiscountPercent, ExpiryDate, IsActive)
    VALUES (N'BOOK20', 20, DATEADD(MONTH, 3, GETDATE()), 1);

IF NOT EXISTS (SELECT 1 FROM dbo.Vouchers WHERE Code = N'EXPIRED15')
    INSERT dbo.Vouchers(Code, DiscountPercent, ExpiryDate, IsActive)
    VALUES (N'EXPIRED15', 15, DATEADD(DAY, -10, GETDATE()), 0);

-- Keep reusable demo vouchers valid even when this idempotent script is rerun months later.
UPDATE dbo.Vouchers SET DiscountPercent = 10, ExpiryDate = DATEADD(MONTH, 6, GETDATE()), IsActive = 1 WHERE Code = N'WELCOME10';
UPDATE dbo.Vouchers SET DiscountPercent = 20, ExpiryDate = DATEADD(MONTH, 3, GETDATE()), IsActive = 1 WHERE Code = N'BOOK20';
UPDATE dbo.Vouchers SET DiscountPercent = 15, ExpiryDate = DATEADD(DAY, -10, GETDATE()), IsActive = 0 WHERE Code = N'EXPIRED15';
GO

DECLARE @DemoUserId INT = (SELECT UserId FROM dbo.Users WHERE Email = N'user@bookverse.vn');
DECLARE @LanUserId INT = (SELECT UserId FROM dbo.Users WHERE Email = N'lan@bookverse.vn');
DECLARE @WelcomeVoucherId INT = (SELECT VoucherId FROM dbo.Vouchers WHERE Code = N'WELCOME10');
DECLARE @NhaGiaKimId INT = (SELECT BookId FROM dbo.Books WHERE Title = N'Nhà Giả Kim');
DECLARE @DacNhanTamId INT = (SELECT BookId FROM dbo.Books WHERE Title = N'Đắc Nhân Tâm');
DECLARE @CleanCodeId INT = (SELECT BookId FROM dbo.Books WHERE Title = N'Clean Code');
DECLARE @MatBiecId INT = (SELECT BookId FROM dbo.Books WHERE Title = N'Mắt Biếc');

IF NOT EXISTS (SELECT 1 FROM dbo.Cart WHERE UserId = @DemoUserId AND BookId = @CleanCodeId)
    INSERT dbo.Cart(UserId, BookId, Quantity) VALUES (@DemoUserId, @CleanCodeId, 1);

IF NOT EXISTS (SELECT 1 FROM dbo.Cart WHERE UserId = @DemoUserId AND BookId = @MatBiecId)
    INSERT dbo.Cart(UserId, BookId, Quantity) VALUES (@DemoUserId, @MatBiecId, 2);

IF NOT EXISTS (SELECT 1 FROM dbo.Wishlist WHERE UserId = @DemoUserId AND BookId = @NhaGiaKimId)
    INSERT dbo.Wishlist(UserId, BookId) VALUES (@DemoUserId, @NhaGiaKimId);

IF NOT EXISTS (SELECT 1 FROM dbo.Reviews WHERE UserId = @DemoUserId AND BookId = @NhaGiaKimId AND Comment = N'Câu chuyện truyền cảm hứng và rất dễ đọc.')
    INSERT dbo.Reviews(BookId, UserId, Rating, Comment, IsApproved, CreatedAt)
    VALUES (@NhaGiaKimId, @DemoUserId, 5, N'Câu chuyện truyền cảm hứng và rất dễ đọc.', 1, DATEADD(DAY, -15, GETDATE()));

IF NOT EXISTS (SELECT 1 FROM dbo.Reviews WHERE UserId = @LanUserId AND BookId = @CleanCodeId AND Comment = N'Nội dung hữu ích cho sinh viên lập trình.')
    INSERT dbo.Reviews(BookId, UserId, Rating, Comment, IsApproved, CreatedAt)
    VALUES (@CleanCodeId, @LanUserId, 4, N'Nội dung hữu ích cho sinh viên lập trình.', 1, DATEADD(DAY, -8, GETDATE()));

IF NOT EXISTS (SELECT 1 FROM dbo.Reviews WHERE UserId = @DemoUserId AND BookId = @MatBiecId AND Comment = N'Đánh giá mẫu đang chờ quản trị viên duyệt.')
    INSERT dbo.Reviews(BookId, UserId, Rating, Comment, IsApproved, CreatedAt)
    VALUES (@MatBiecId, @DemoUserId, 5, N'Đánh giá mẫu đang chờ quản trị viên duyệt.', 0, DATEADD(DAY, -1, GETDATE()));

IF NOT EXISTS (SELECT 1 FROM dbo.Orders WHERE UserId = @DemoUserId AND ShippingAddress = N'DEMO-ORDER-001')
BEGIN
    INSERT dbo.Orders(UserId, VoucherId, TotalAmount, ShippingAddress, PhoneNumber, [Status], PaymentMethod, PaymentStatus, CreatedAt)
    VALUES (@DemoUserId, @WelcomeVoucherId, 219600, N'DEMO-ORDER-001', N'0902000002',
            N'Đã giao', N'COD', N'Đã thanh toán', DATEADD(DAY, -25, GETDATE()));

    DECLARE @Order1 INT = SCOPE_IDENTITY();
    INSERT dbo.OrderDetails(OrderId, BookId, Quantity, Price)
    VALUES (@Order1, @NhaGiaKimId, 1, 79000),
           (@Order1, @DacNhanTamId, 2, 86000);
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Orders WHERE UserId = @DemoUserId AND ShippingAddress = N'DEMO-ORDER-002')
BEGIN
    INSERT dbo.Orders(UserId, VoucherId, TotalAmount, ShippingAddress, PhoneNumber, [Status], PaymentMethod, PaymentStatus, CreatedAt)
    VALUES (@DemoUserId, NULL, 430000, N'DEMO-ORDER-002', N'0902000002',
            N'Đang giao', N'COD', N'Chưa thanh toán', DATEADD(DAY, -5, GETDATE()));

    DECLARE @Order2 INT = SCOPE_IDENTITY();
    INSERT dbo.OrderDetails(OrderId, BookId, Quantity, Price)
    VALUES (@Order2, @CleanCodeId, 1, 320000),
           (@Order2, @MatBiecId, 1, 110000);
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Orders WHERE UserId = @LanUserId AND ShippingAddress = N'DEMO-ORDER-003')
BEGIN
    INSERT dbo.Orders(UserId, VoucherId, TotalAmount, ShippingAddress, PhoneNumber, [Status], PaymentMethod, PaymentStatus, CreatedAt)
    VALUES (@LanUserId, NULL, 79000, N'DEMO-ORDER-003', N'0903000003',
            N'Chờ xử lý', N'COD', N'Chưa thanh toán', DATEADD(DAY, -1, GETDATE()));

    DECLARE @Order3 INT = SCOPE_IDENTITY();
    INSERT dbo.OrderDetails(OrderId, BookId, Quantity, Price)
    VALUES (@Order3, @NhaGiaKimId, 1, 79000);
END;
GO

/* Hien thi tom tat sau khi chay */
SELECT N'Roles' AS TableName, COUNT(*) AS [RowCount] FROM dbo.Roles
UNION ALL SELECT N'Users', COUNT(*) FROM dbo.Users
UNION ALL SELECT N'Categories', COUNT(*) FROM dbo.Categories
UNION ALL SELECT N'Books', COUNT(*) FROM dbo.Books
UNION ALL SELECT N'Vouchers', COUNT(*) FROM dbo.Vouchers
UNION ALL SELECT N'Orders', COUNT(*) FROM dbo.Orders
UNION ALL SELECT N'OrderDetails', COUNT(*) FROM dbo.OrderDetails
UNION ALL SELECT N'Cart', COUNT(*) FROM dbo.Cart
UNION ALL SELECT N'Reviews', COUNT(*) FROM dbo.Reviews
UNION ALL SELECT N'Wishlist', COUNT(*) FROM dbo.Wishlist;
GO
