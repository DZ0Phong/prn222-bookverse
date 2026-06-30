# Đặc tả dự án Bookverse - Cửa hàng bán sách (ASP.NET Core 8 MVC)

## 1. Tổng quan dự án
- **Tên dự án**: Bookverse (Hệ thống quản lý bán sách trực tuyến)
- **Công nghệ**: C# .NET 8, ASP.NET Core MVC
- **Cơ sở dữ liệu**: SQL Server
- **Mục tiêu**: Xây dựng website bán sách với đầy đủ chức năng quản lý, phân quyền, giỏ hàng, thanh toán và thống kê. Hệ thống được thiết kế dưới dạng khung xương (skeleton), cho phép làm việc nhóm hiệu quả.

---

## 2. Phân chia công việc & Đặc tả chi tiết (4 thành viên)

### Thành viên 1: Authentication, Quản lý người dùng & Phân quyền (Admin/User)
**Nhiệm vụ chính:** Quản lý toàn bộ vòng đời của người dùng và bảo mật hệ thống.
- **Tính năng chi tiết**:
  - Đăng ký, Đăng nhập (Sử dụng JWT hoặc Session/Cookie Authentication trong .NET Core).
  - Phân quyền: Cấu hình Role-based authorization (`[Authorize(Roles = "Admin")]`).
  - Quản lý hồ sơ cá nhân: Xem thông tin, Cập nhật thông tin (tên, số điện thoại, địa chỉ, avatar), Đổi mật khẩu.
  - Quản lý Admin (User Management): Giao diện Admin để xem danh sách User, thực hiện Khóa (Ban) / Mở khóa tài khoản (IsActive).
  - Bảo mật: Middleware kiểm tra quyền truy cập, mã hóa mật khẩu (hashing).

### Thành viên 2: Quản lý sản phẩm (Sách) & Tìm kiếm
**Nhiệm vụ chính:** Đảm bảo dữ liệu Sách và Danh mục được hiển thị và quản lý tốt.
- **Tính năng chi tiết**:
  - CRUD Danh mục sách: Thêm, sửa, xóa, xem danh mục.
  - CRUD Sách: Thêm, sửa, xóa, xem chi tiết sách (liên kết với Danh mục qua Khóa ngoại).
  - Xử lý hình ảnh: Upload và lưu trữ ảnh bìa sách vào thư mục `wwwroot/images/books`.
  - Quản lý kho: Cập nhật số lượng sách còn lại (Quantity).
  - Tìm kiếm & Lọc:
    - Tìm kiếm cơ bản theo tên sách.
    - Lọc nâng cao theo tác giả, khoảng giá, nhà xuất bản, danh mục.
  - Phân trang (Pagination): Chia trang cho danh sách sản phẩm.

### Thành viên 3: Giỏ hàng, Đơn hàng & Đánh giá
**Nhiệm vụ chính:** Đảm nhận luồng mua hàng và tương tác của khách hàng.
- **Tính năng chi tiết**:
  - Giỏ hàng: Thêm sách vào giỏ, cập nhật số lượng, xóa sách, tính tổng tiền (lưu bằng Session hoặc DB).
  - Mã giảm giá (Voucher): Áp dụng mã giảm giá và tính toán lại tổng tiền.
  - Đặt hàng & Thanh toán: Điền thông tin giao hàng, chọn phương thức thanh toán (COD hoặc giả lập chuyển khoản). Sinh bản ghi Order và OrderDetails.
  - Lịch sử mua hàng: Người dùng xem lại các đơn hàng đã đặt và theo dõi trạng thái.
  - Đánh giá & Bình luận (Review): Chấm điểm 1-5 sao và để lại nhận xét (chỉ dành cho User đã mua hoặc đăng nhập).

### Thành viên 4: Dashboard Admin, Quản lý đơn hàng & Thống kê
**Nhiệm vụ chính:** Xây dựng trung tâm điều hành cho Admin.
- **Tính năng chi tiết**:
  - Dashboard tổng quan: Hiển thị các chỉ số nhanh (Tổng số sách, số người dùng, đơn hàng chờ xử lý, doanh thu).
  - Quản lý đơn hàng: Cập nhật trạng thái đơn hàng (Chờ xử lý -> Đang giao -> Đã giao / Hủy).
  - Quản lý đánh giá: Duyệt hoặc xóa các đánh giá/bình luận không phù hợp (IsApproved).
  - Thống kê & Báo cáo:
    - Biểu đồ doanh thu theo ngày/tháng.
    - Thống kê top 5-10 sách bán chạy nhất.
  - Giao diện Admin: Sử dụng template HTML/CSS tích hợp sẵn, đảm bảo responsive.
  - Xuất báo cáo (Nâng cao): Xuất danh sách đơn hàng hoặc doanh thu ra file Excel/PDF (Dùng thư viện EPPlus hoặc iText7).

---

## 3. Cấu trúc Database (Lược đồ ERD cơ bản)
- `Users` (1) - (n) `Roles`
- `Categories` (1) - (n) `Books`
- `Users` (1) - (n) `Cart` (n) - (1) `Books`
- `Users` (1) - (n) `Orders` (n) - (1) `Vouchers`
- `Orders` (1) - (n) `OrderDetails` (n) - (1) `Books`
- `Books` (1) - (n) `Reviews` (n) - (1) `Users`

---

## 4. Hướng dẫn làm việc với Project Khung (Skeleton)
1. Cấu hình chuỗi kết nối (`ConnectionStrings`) trong `appsettings.json`.
2. Chạy file script SQL `QuanLyBanSach.sql` để khởi tạo database.
3. Import Layout từ thư mục `bookverse` (Figma template) vào thư mục `Views/Shared/_Layout.cshtml` của dự án MVC.
4. Mỗi thành viên tạo các Controller tương ứng (VD: `UserController`, `BookController`, `OrderController`, `AdminController`).

---
*(Tài liệu này được tạo tự động nhằm mục đích phân chia công việc nhóm)*
