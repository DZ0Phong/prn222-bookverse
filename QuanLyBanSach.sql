-- ============================================
-- DATABASE SCRIPT: QuanLyBanSach
-- ============================================
CREATE DATABASE QuanLyBanSach;
GO

USE QuanLyBanSach;
GO

-- ============================================
-- 1. BẢNG ROLES (Vai trò)
-- ============================================
CREATE TABLE Roles (
    RoleId INT IDENTITY(1,1) PRIMARY KEY,
    RoleName NVARCHAR(50) NOT NULL UNIQUE
);
GO

INSERT INTO Roles (RoleName) VALUES (N'Admin'), (N'User');
GO

-- ============================================
-- 2. BẢNG USERS (Người dùng)
-- ============================================
CREATE TABLE Users (
    UserId INT IDENTITY(1,1) PRIMARY KEY,
    FullName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100) NOT NULL UNIQUE,
    Password NVARCHAR(255) NOT NULL,       -- hash password
    Phone NVARCHAR(20),
    Address NVARCHAR(255),
    Avatar NVARCHAR(255),
    RoleId INT NOT NULL DEFAULT 2,         -- 2 = User
    IsActive BIT NOT NULL DEFAULT 1,       -- 1 = Active, 0 = Banned
    CreatedAt DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_Users_Roles FOREIGN KEY (RoleId) REFERENCES Roles(RoleId)
);
GO

-- ============================================
-- 3. BẢNG CATEGORIES (Danh mục sách)
-- ============================================
CREATE TABLE Categories (
    CategoryId INT IDENTITY(1,1) PRIMARY KEY,
    CategoryName NVARCHAR(100) NOT NULL,
    Description NVARCHAR(255),
    CreatedAt DATETIME DEFAULT GETDATE()
);
GO

-- ============================================
-- 4. BẢNG BOOKS (Sách)
-- ============================================
CREATE TABLE Books (
    BookId INT IDENTITY(1,1) PRIMARY KEY,
    Title NVARCHAR(255) NOT NULL,
    Author NVARCHAR(150),
    Publisher NVARCHAR(150),
    CategoryId INT NOT NULL,
    Price DECIMAL(18,0) NOT NULL,
    DiscountPrice DECIMAL(18,0) NULL,
    Quantity INT NOT NULL DEFAULT 0,       
    Image NVARCHAR(255),
    Description NVARCHAR(MAX),
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_Books_Categories FOREIGN KEY (CategoryId) REFERENCES Categories(CategoryId)
);
GO

-- ============================================
-- 5. BẢNG CART (Giỏ hàng)
-- ============================================
CREATE TABLE Cart (
    CartId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    BookId INT NOT NULL,
    Quantity INT NOT NULL DEFAULT 1,
    CreatedAt DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_Cart_Users FOREIGN KEY (UserId) REFERENCES Users(UserId),
    CONSTRAINT FK_Cart_Books FOREIGN KEY (BookId) REFERENCES Books(BookId)
);
GO

-- ============================================
-- 6. BẢNG VOUCHERS (Mã giảm giá)
-- ============================================
CREATE TABLE Vouchers (
    VoucherId INT IDENTITY(1,1) PRIMARY KEY,
    Code NVARCHAR(50) NOT NULL UNIQUE,
    DiscountPercent INT NOT NULL,          
    MaxDiscount DECIMAL(18,0) NULL,
    ExpiryDate DATETIME,
    IsActive BIT NOT NULL DEFAULT 1
);
GO

-- ============================================
-- 7. BẢNG ORDERS (Đơn hàng)
-- ============================================
CREATE TABLE Orders (
    OrderId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    VoucherId INT NULL,
    TotalAmount DECIMAL(18,0) NOT NULL,
    ShippingAddress NVARCHAR(255),
    PhoneNumber NVARCHAR(20),
    Status NVARCHAR(50) NOT NULL DEFAULT N'Chờ xử lý', -- Chờ xử lý / Đang giao / Đã giao / Đã hủy
    PaymentMethod NVARCHAR(50) DEFAULT N'COD', 
    PaymentStatus NVARCHAR(50) DEFAULT N'Chưa thanh toán', -- Chưa thanh toán / Đã thanh toán
    CreatedAt DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_Orders_Users FOREIGN KEY (UserId) REFERENCES Users(UserId),
    CONSTRAINT FK_Orders_Vouchers FOREIGN KEY (VoucherId) REFERENCES Vouchers(VoucherId)
);
GO

-- ============================================
-- 8. BẢNG ORDERDETAILS (Chi tiết đơn hàng)
-- ============================================
CREATE TABLE OrderDetails (
    OrderDetailId INT IDENTITY(1,1) PRIMARY KEY,
    OrderId INT NOT NULL,
    BookId INT NOT NULL,
    Quantity INT NOT NULL,
    Price DECIMAL(18,0) NOT NULL,
    CONSTRAINT FK_OrderDetails_Orders FOREIGN KEY (OrderId) REFERENCES Orders(OrderId),
    CONSTRAINT FK_OrderDetails_Books FOREIGN KEY (BookId) REFERENCES Books(BookId)
);
GO

-- ============================================
-- 9. BẢNG REVIEWS (Đánh giá sách)
-- ============================================
CREATE TABLE Reviews (
    ReviewId INT IDENTITY(1,1) PRIMARY KEY,
    BookId INT NOT NULL,
    UserId INT NOT NULL,
    Rating INT NOT NULL CHECK (Rating BETWEEN 1 AND 5),
    Comment NVARCHAR(MAX),
    IsApproved BIT NOT NULL DEFAULT 1,     
    CreatedAt DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_Reviews_Books FOREIGN KEY (BookId) REFERENCES Books(BookId),
    CONSTRAINT FK_Reviews_Users FOREIGN KEY (UserId) REFERENCES Users(UserId)
);
GO

-- ============================================
-- 10. BẢNG WISHLIST (Sách yêu thích)
-- ============================================
CREATE TABLE Wishlist (
    WishlistId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    BookId INT NOT NULL,
    CreatedAt DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_Wishlist_Users FOREIGN KEY (UserId) REFERENCES Users(UserId),
    CONSTRAINT FK_Wishlist_Books FOREIGN KEY (BookId) REFERENCES Books(BookId),
    CONSTRAINT UQ_Wishlist UNIQUE (UserId, BookId)  
);
GO

-- ============================================
-- DỮ LIỆU MẪU SẴN (Seed Data)
-- ============================================
INSERT INTO Categories (CategoryName, Description) VALUES
(N'Văn học', N'Sách văn học trong và ngoài nước'),
(N'Kinh tế', N'Sách kinh doanh, tài chính'),
(N'Kỹ năng sống', N'Sách phát triển bản thân'),
(N'Công nghệ thông tin', N'Sách lập trình, máy tính'),
(N'Thiếu nhi', N'Sách dành cho trẻ em');
GO

-- Mật khẩu tạm thời là '123456' (nên thay bằng Hash trong C# sau này)
INSERT INTO Users (FullName, Email, Password, Phone, Address, RoleId) VALUES
(N'Admin Quản Trị 1', N'admin1@bookverse.com', N'123456', N'0901234567', N'Hà Nội', 1),
(N'Admin Quản Trị 2', N'admin2@bookverse.com', N'123456', N'0909876543', N'Hồ Chí Minh', 1),
(N'Nguyễn Văn User', N'user1@gmail.com', N'123456', N'0912345678', N'Đà Nẵng', 2),
(N'Trần Thị Khách Hàng', N'user2@gmail.com', N'123456', N'0987654321', N'Cần Thơ', 2),
(N'Lê Minh Tester', N'tester@gmail.com', N'123456', N'0933333333', N'Hải Phòng', 2);
GO

INSERT INTO Books (Title, Author, Publisher, CategoryId, Price, Quantity, Description) VALUES
(N'Đắc Nhân Tâm', N'Dale Carnegie', N'NXB Tổng Hợp', 3, 86000, 50, N'Sách kỹ năng sống kinh điển, giúp bạn cải thiện kỹ năng giao tiếp.'),
(N'Nhà Giả Kim', N'Paulo Coelho', N'NXB Văn Học', 1, 79000, 30, N'Tiểu thuyết nổi tiếng thế giới về hành trình theo đuổi giấc mơ.'),
(N'Clean Code', N'Robert C. Martin', N'Prentice Hall', 4, 350000, 20, N'Sách gối đầu giường cho dân lập trình viên để viết mã sạch.'),
(N'Dế Mèn Phiêu Lưu Ký', N'Tô Hoài', N'NXB Kim Đồng', 5, 45000, 100, N'Tác phẩm thiếu nhi kinh điển của Việt Nam.'),
(N'Cha Giàu Cha Nghèo', N'Robert Kiyosaki', N'NXB Trẻ', 2, 110000, 45, N'Sách tài chính cá nhân nổi tiếng.');
GO

PRINT N'Tạo database QuanLyBanSach thành công! Đã chèn dữ liệu mẫu chuẩn Tiếng Việt.';
