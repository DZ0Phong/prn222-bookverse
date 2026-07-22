# Cài đặt và cấu hình BookVerse

## Chạy lần đầu

1. Khởi động SQL Server LocalDB: `sqllocaldb start MSSQLLocalDB`.
2. Nạp dữ liệu UTF-8: `sqlcmd -S "(localdb)\MSSQLLocalDB" -E -f 65001 -i db.sql -b`.
3. Chạy web: `dotnet run --project BookVerse/BookVerse.csproj`.

Tài khoản demo: `admin@bookverse.vn / Admin@123` và `user@bookverse.vn / User@123`.

Phải giữ tham số `-f 65001`. Nếu bỏ tham số này, bản `sqlcmd` cũ trên Windows có thể đọc tiếng Việt UTF-8 theo bảng mã ANSI và tạo dữ liệu dạng `VÄƒn há»c`.

## Cấu hình không cần sửa code

Admin mở `/Admin/Settings` để đổi tên cửa hàng, địa chỉ, liên hệ, phí giao hàng, ngưỡng miễn phí giao hàng, giới hạn số lượng, thời hạn thanh toán và bật/tắt COD hoặc VNPay. Dữ liệu được lưu vào `BookVerse/Config/site-settings.json` và tự nạp lại.

Ngôn ngữ nằm tại `BookVerse/Resources/vi.json` và `BookVerse/Resources/en.json`. Phần `strings` dùng cho màn hình mới; `textMap` dịch các view cũ. Khi thêm nhãn mới, thêm cùng một khóa ở cả hai file.

## VNPay Sandbox

Đăng ký merchant Sandbox với VNPay, sau đó điền `TmnCode` và `HashSecret` vào `BookVerse/Config/vnpay.json`. Có thể để `ReturnUrl` trống khi chạy local; ứng dụng tự tạo URL `/Payment/VnPayReturn`.

`AllowReturnToConfirmPayment=true` phù hợp demo local vì VNPay không gọi được IPN vào localhost. Khi triển khai server public, đặt thành `false` và đăng ký IPN `/Payment/VnPayIpn`; IPN mới là nguồn xác nhận thanh toán chính thức.

Không commit `HashSecret` thật lên Git. Với môi trường production, nên ghi đè bằng biến môi trường `VnPay__TmnCode` và `VnPay__HashSecret`.
