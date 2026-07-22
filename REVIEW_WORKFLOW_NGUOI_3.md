# NOTE REVIEW CODE — NGƯỜI 3

## Phạm vi code thực tế đang chạy

Trong phần được giao, hiện tại có hai workflow phía user đã có logic backend:

1. Xem lịch sử mua hàng.
2. Xem và gửi đánh giá sách.

Các chức năng giỏ hàng, checkout, thanh toán giả lập và áp voucher phía user hiện chưa được nối backend. Model database đã có, nhưng nút thêm giỏ hiện chỉ hiển thị `alert`.

---

# PHẦN 1 — LỊCH SỬ MUA HÀNG

## 1. Câu mở đầu nên nói

> “Em xin trình bày workflow lịch sử mua hàng. Luồng này bắt đầu từ link Đơn hàng của tôi, chạy vào action `OrderHistory` trong `AccountController`. Controller lấy ID người đang đăng nhập từ claim, truy vấn những đơn thuộc user đó, đóng gói dữ liệu sang `UserOrderListViewModel`, rồi truyền danh sách sang `OrderHistory.cshtml` để hiển thị.”

## 2. Thứ tự mở file khi trình bày

Mở lần lượt:

1. `BookVerse/Views/Home/HomePage.cshtml`
2. `BookVerse/Controllers/AccountController.cs`
3. `BookVerse/Models/ViewModels/User/UserOrderViewModel.cs`
4. `BookVerse/Views/Account/OrderHistory.cshtml`

Luồng tổng quát:

```text
Link “Đơn hàng của tôi”
        ↓
GET /Account/OrderHistory
        ↓
AccountController.OrderHistory()
        ↓
Lấy UserId từ claim
        ↓
Truy vấn Orders và OrderDetails
        ↓
Đóng gói thành List<UserOrderListViewModel>
        ↓
return View(orders)
        ↓
OrderHistory.cshtml nhận Model và hiển thị
```

---

## 3. Mở link khởi đầu

### File cần mở

`BookVerse/Views/Home/HomePage.cshtml`

### Dòng cần tìm

Tìm:

```html
href="/Account/OrderHistory"
```

### Lời nói

> “Workflow bắt đầu khi user bấm Đơn hàng của tôi. Link này tạo một GET request đến `/Account/OrderHistory`. Theo quy tắc routing của MVC, `Account` là controller và `OrderHistory` là action.”

Không cần giải thích HTML hoặc CSS xung quanh.

---

## 4. Mở action xử lý chính

### File cần mở

`BookVerse/Controllers/AccountController.cs`

### Dòng cần show

Khoảng dòng 220–248, action:

```csharp
[HttpGet]
[Authorize]
public async Task<IActionResult> OrderHistory()
```

### Lời nói

> “Đây là action nhận request. `[HttpGet]` cho biết action dùng để đọc trang. `[Authorize]` yêu cầu user phải đăng nhập. Nếu chưa đăng nhập, authentication middleware sẽ chuyển user về trang Login.”

Giải thích phần khai báo:

```csharp
public async Task<IActionResult> OrderHistory()
```

> “Action có `async` vì truy vấn database bằng Entity Framework bất đồng bộ. Kết quả trả về là `IActionResult`, cụ thể ở cuối action sẽ trả một View.”

---

## 5. Show đoạn lấy UserId

### Code cần show

```csharp
var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                  ?? User.FindFirst("UserId")?.Value;

if (string.IsNullOrEmpty(userIdClaim)
    || !int.TryParse(userIdClaim, out int userId))
{
    return RedirectToAction("Login");
}
```

### Lời nói

> “Sau khi đăng nhập, thông tin định danh của user được lưu trong authentication cookie dưới dạng claim. Ở đây em lấy `NameIdentifier`, chính là UserId của tài khoản hiện tại.”

> “Giá trị claim ban đầu là chuỗi, ví dụ `"5"`. `int.TryParse` chuyển nó thành số nguyên `userId = 5`. Nếu claim bị thiếu hoặc không chuyển được thành số thì action chuyển về Login.”

Điểm liên quan:

```text
Cookie đăng nhập
      ↓
Claim NameIdentifier
      ↓
userId
      ↓
Dùng để lọc đơn hàng
```

---

## 6. Show câu truy vấn database

### Code cần show

```csharp
var orders = await _context.Orders
    .Include(o => o.OrderDetails)
    .Where(o => o.UserId == userId)
    .OrderByDescending(o => o.CreatedAt)
    .Select(o => new UserOrderListViewModel
    {
        OrderId = o.OrderId,
        CreatedAt = o.CreatedAt,
        TotalItems = o.OrderDetails.Sum(d => d.Quantity ?? 0),
        TotalAmount = o.TotalAmount ?? 0,
        Status = o.Status,
        PaymentStatus = o.PaymentStatus
    })
    .ToListAsync();
```

### Giải thích thực tế từng bước

#### `_context.Orders`

> “`_context` là Entity Framework DbContext được inject vào controller. `_context.Orders` đại diện cho bảng Orders trong database.”

#### `.Include(o => o.OrderDetails)`

> “Mỗi Order có nhiều OrderDetail. Phần này lấy thêm dữ liệu chi tiết để có thể tính tổng số lượng sách trong đơn.”

#### `.Where(o => o.UserId == userId)`

> “Đây là điều kiện quan trọng nhất. Nó chỉ lấy đơn có UserId bằng ID của người đang đăng nhập, tránh hiển thị đơn của user khác.”

#### `.OrderByDescending(o => o.CreatedAt)`

> “Đơn hàng được sắp xếp theo thời gian giảm dần, nghĩa là đơn mới nhất nằm trước.”

#### `.Select(...)`

> “Sau khi lọc dữ liệu, em không truyền trực tiếp entity Order sang View mà map sang `UserOrderListViewModel`. Đây là bước đóng gói dữ liệu: View chỉ nhận đúng những trường cần hiển thị.”

#### `TotalItems`

```csharp
TotalItems = o.OrderDetails.Sum(d => d.Quantity ?? 0)
```

> “Tổng số quyển được tính bằng cách cộng Quantity của tất cả OrderDetail trong đơn. Toán tử `?? 0` nghĩa là nếu Quantity bị null thì xem như bằng 0.”

Ví dụ:

```text
Order #12
  Sách A: Quantity = 2
  Sách B: Quantity = 1

TotalItems = 2 + 1 = 3
```

#### `TotalAmount`

```csharp
TotalAmount = o.TotalAmount ?? 0
```

> “Tổng tiền lấy từ đơn hàng. Nếu giá trị null thì ViewModel nhận 0 để tránh lỗi.”

#### `.ToListAsync()`

> “Các lệnh phía trên mới mô tả truy vấn. `ToListAsync` là lúc Entity Framework thực thi SQL và đưa kết quả về thành một danh sách.”

Sau đoạn này, biến `orders` có kiểu gần như:

```csharp
List<UserOrderListViewModel>
```

---

## 7. Mở ViewModel để giải thích “đóng gói”

### File cần mở

`BookVerse/Models/ViewModels/User/UserOrderViewModel.cs`

### Dòng cần show

Class `UserOrderListViewModel`, khoảng dòng 3–11:

```csharp
public class UserOrderListViewModel
{
    public int OrderId { get; set; }
    public DateTime? CreatedAt { get; set; }
    public int TotalItems { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Status { get; set; }
    public string? PaymentStatus { get; set; }
}
```

### Lời nói

> “ViewModel là gói dữ liệu trung gian giữa Controller và View. Trang lịch sử chỉ cần mã đơn, ngày đặt, số lượng, tổng tiền và trạng thái, nên ViewModel chỉ chứa các trường này.”

> “Controller là nơi tạo và đóng gói ViewModel. View chỉ có nhiệm vụ nhận và hiển thị, không tự truy vấn database.”

Quan hệ:

```text
Entity Order trong database
           ↓ Select/map
UserOrderListViewModel
           ↓ return View
OrderHistory.cshtml
```

---

## 8. Quay lại dòng return View

### File

`BookVerse/Controllers/AccountController.cs`

### Code cần show

```csharp
return View(orders);
```

### Lời nói

> “Sau khi truy vấn và đóng gói xong, controller truyền danh sách `orders` sang View. Theo convention của MVC, action `OrderHistory` sẽ tìm file `Views/Account/OrderHistory.cshtml`.”

---

## 9. Mở View hiển thị

### File cần mở

`BookVerse/Views/Account/OrderHistory.cshtml`

### Dòng đầu tiên cần show

```cshtml
@model IEnumerable<BookVerse.Models.ViewModels.User.UserOrderListViewModel>
```

### Lời nói

> “Dòng `@model` khai báo loại dữ liệu trang này được phép nhận. Ở đây là một tập hợp `UserOrderListViewModel`, tương ứng với danh sách `orders` controller vừa truyền sang.”

### Show phần kiểm tra danh sách rỗng

```cshtml
@if (Model == null || !Model.Any())
{
    // Hiển thị chưa có đơn hàng
}
```

> “Nếu controller trả danh sách rỗng, View hiển thị thông báo chưa có đơn hàng.”

### Show vòng lặp

```cshtml
foreach (var order in Model)
```

> “Nếu có dữ liệu, View lặp qua từng ViewModel và tạo một hàng trong bảng cho mỗi đơn.”

### Show phần chọn CSS theo trạng thái

```csharp
var statusClass = order.Status switch
{
    "Chờ xử lý" => "status-cho-xu-ly",
    "Đang giao" => "status-dang-giao",
    "Đã giao" => "status-da-giao",
    "Đã hủy" => "status-da-huy",
    _ => "status-cho-xu-ly"
};
```

> “Đoạn switch không thay đổi dữ liệu. Nó chỉ chọn CSS class để mỗi trạng thái có một màu khác nhau.”

### Show phần in dữ liệu

```cshtml
@order.OrderId
@order.CreatedAt
@order.TotalItems
@order.TotalAmount
@order.Status
@order.PaymentStatus
```

> “Các biểu thức Razor bắt đầu bằng `@` đọc property từ ViewModel và đưa giá trị vào HTML.”

---

## 10. Câu kết thúc workflow lịch sử

> “Tóm lại, link tạo GET request, `AccountController` xác định user bằng claim, Entity Framework lọc đơn hàng của user, dữ liệu được map sang danh sách `UserOrderListViewModel`, sau đó `OrderHistory.cshtml` lặp danh sách để hiển thị. View không truy vấn database và Controller không trực tiếp tạo HTML.”

---

# PHẦN 2 — ĐÁNH GIÁ SÁCH

## 1. Câu mở đầu nên nói

> “Workflow đánh giá gồm hai chiều. Chiều GET là mở trang chi tiết và lấy các đánh giá đã được admin duyệt. Chiều POST là user gửi form đánh giá, `ReviewController` kiểm tra dữ liệu, tạo Review ở trạng thái chờ duyệt, lưu database rồi redirect về trang chi tiết.”

## 2. Thứ tự mở file khi trình bày

Mở lần lượt:

1. `BookVerse/Controllers/BookController.cs`
2. `BookVerse/Models/ViewModels/User/BookDetailViewModel.cs`
3. `BookVerse/Views/User/Book/Detail.cshtml`
4. `BookVerse/Controllers/ReviewController.cs`
5. Quay lại `BookVerse/Views/User/Book/Detail.cshtml`

Luồng tổng quát:

```text
GET /User/Book/Detail/{id}
        ↓
BookController.Detail(id)
        ↓
Lấy Book + Reviews đã duyệt
        ↓
Đóng gói BookDetailViewModel
        ↓
Detail.cshtml hiển thị sách và review
        ↓
User submit form đánh giá
        ↓
POST /Review/Create
        ↓
ReviewController.Create(...)
        ↓
Tạo Review với IsApproved = false
        ↓
SaveChangesAsync
        ↓
Redirect về trang sách
        ↓
Admin duyệt thì review mới xuất hiện
```

---

## 3. Mở action hiển thị trang sách

### File cần mở

`BookVerse/Controllers/BookController.cs`

### Show route

```csharp
[Route("User/[controller]/[action]/{id?}")]
```

### Lời nói

> “Controller này có route riêng. Với controller Book và action Detail, URL thực tế là `/User/Book/Detail/{id}`. Phần `{id}` là ID sách.”

Ví dụ:

```text
/User/Book/Detail/10
                    ↓
                 id = 10
```

### Show action

```csharp
public async Task<IActionResult> Detail(int id)
```

> “ASP.NET model binding lấy ID trên URL và truyền vào tham số `id` của action.”

---

## 4. Show phần lấy sách

### Code

```csharp
var book = await _context.Books
    .Include(b => b.Category)
    .FirstOrDefaultAsync(b => b.BookId == id);

if (book == null) return NotFound();
```

### Lời nói

> “Controller tìm cuốn sách có `BookId` bằng ID trên URL. `Include Category` lấy thêm thể loại của sách. Nếu không tìm thấy thì action trả HTTP 404 bằng `NotFound()`.”

---

## 5. Show phần lấy review đã duyệt

### Code

```csharp
var approvedReviews = await _context.Reviews
    .Include(r => r.User)
    .Where(r => r.BookId == id && r.IsApproved == true)
    .OrderByDescending(r => r.CreatedAt)
    .ToListAsync();
```

### Lời nói

> “Tiếp theo controller lấy review của đúng cuốn sách. Điều kiện `IsApproved == true` bảo đảm chỉ đánh giá đã được admin duyệt mới xuất hiện công khai.”

> “`Include User` dùng để View có thể hiển thị tên người đánh giá. `OrderByDescending` đưa đánh giá mới nhất lên trước.”

---

## 6. Show bước đóng gói ViewModel

### Code

```csharp
var viewModel = new BookDetailViewModel
{
    Book = book,
    ExistingReviews = approvedReviews
};
```

### Lời nói

> “Trang chi tiết cần hai loại dữ liệu: một cuốn sách và một danh sách đánh giá. Vì một View chỉ nhận một model chính, controller đóng gói cả hai vào `BookDetailViewModel`.”

### Mở file ViewModel

`BookVerse/Models/ViewModels/User/BookDetailViewModel.cs`

### Show

```csharp
public Book Book { get; set; }
public List<Review> ExistingReviews { get; set; }
```

### Lời nói

> “Property `Book` phục vụ phần thông tin sản phẩm. `ExistingReviews` phục vụ danh sách đánh giá bên dưới.”

### Quay lại BookController và show

```csharp
return View("~/Views/User/Book/Detail.cshtml", viewModel);
```

> “Controller chỉ định file View và truyền object `viewModel` sang đó.”

---

## 7. Mở View và show form

### File

`BookVerse/Views/User/Book/Detail.cshtml`

### Show kiểm tra đăng nhập

```cshtml
@if (User.Identity != null && User.Identity.IsAuthenticated)
{
    // Hiển thị form
}
else
{
    // Yêu cầu đăng nhập
}
```

### Lời nói

> “View kiểm tra trạng thái đăng nhập. Nếu đã đăng nhập thì hiện form đánh giá; nếu chưa thì chỉ hiện link Login.”

### Show khai báo form

```cshtml
<form action="/Review/Create" method="post">
    @Html.AntiForgeryToken()
    <input type="hidden" name="BookId" value="@book.BookId" />
```

### Lời nói

> “Form dùng POST và gửi tới `/Review/Create`. Anti-forgery token chống request giả mạo. `BookId` là hidden input vì server cần biết đánh giá thuộc sách nào nhưng user không cần nhập lại ID.”

### Show các input

```html
<select name="Rating">
<textarea name="Comment">
```

### Lời nói

> “Tên input rất quan trọng. `BookId`, `Rating`, `Comment` trùng tên các tham số của action, nên ASP.NET tự đóng gói dữ liệu HTTP form vào các tham số bằng cơ chế model binding.”

Dữ liệu request gần giống:

```text
POST /Review/Create

BookId=10
Rating=5
Comment=Sách rất hay
__RequestVerificationToken=...
```

---

## 8. Mở action nhận form

### File

`BookVerse/Controllers/ReviewController.cs`

### Show đầu action

```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Create(
    int BookId,
    int Rating,
    string Comment)
```

### Lời nói

> “Action này nhận POST từ form. `[ValidateAntiForgeryToken]` kiểm tra token được View tạo. Ba tham số nhận dữ liệu từ ba input cùng tên.”

Liên hệ trực tiếp:

```text
input name="BookId"  → int BookId
select name="Rating" → int Rating
textarea name="Comment" → string Comment
```

---

## 9. Show kiểm tra đăng nhập và dữ liệu

### Code

```csharp
if (!User.Identity.IsAuthenticated)
{
    return RedirectToAction("Login", "Account");
}
```

### Lời nói

> “View đã ẩn form với người chưa đăng nhập, nhưng server vẫn phải kiểm tra lại vì client có thể tự tạo request. Kiểm tra phía server mới là kiểm tra bảo mật thực sự.”

### Code

```csharp
if (BookId <= 0 || string.IsNullOrWhiteSpace(Comment))
{
    return RedirectToAction("Detail", "Book", new { id = BookId });
}
```

### Lời nói

> “Nếu BookId không hợp lệ hoặc comment rỗng thì không lưu và quay lại trang sách.”

---

## 10. Show lấy UserId

### Code

```csharp
var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
int.TryParse(userIdString, out int userId);
```

### Lời nói

> “Tương tự lịch sử mua hàng, UserId không lấy từ form để tránh user giả mạo người khác. Nó được lấy từ claim của tài khoản đang đăng nhập.”

---

## 11. Show tạo object Review

### Code

```csharp
var newReview = new Review
{
    BookId = BookId,
    UserId = userId,
    Rating = Rating,
    Comment = Comment.Trim(),
    IsApproved = false,
    CreatedAt = DateTime.Now
};
```

### Lời nói

> “Action đóng gói dữ liệu đã nhận cùng UserId thành một entity Review mới.”

Giải thích:

```text
BookId       → review thuộc sách nào
UserId       → ai viết review
Rating       → số sao
Comment      → nội dung đã bỏ khoảng trắng thừa
IsApproved   → false, đang chờ admin duyệt
CreatedAt    → thời điểm gửi
```

---

## 12. Show lưu database

### Code

```csharp
_context.Reviews.Add(newReview);
await _context.SaveChangesAsync();
```

### Lời nói

> “`Add` đưa entity vào bộ theo dõi của Entity Framework ở trạng thái cần thêm. `SaveChangesAsync` mới thực thi câu lệnh INSERT xuống database.”

Luồng dữ liệu:

```text
Form HTTP
   ↓ model binding
Tham số action
   ↓ new Review
Entity Review
   ↓ Add + SaveChangesAsync
Bảng Reviews trong SQL Server
```

---

## 13. Show TempData và redirect

### Code

```csharp
TempData["SuccessMessage"] =
    "Cảm ơn bạn! Đánh giá đã được gửi...";

return RedirectToAction(
    "Detail",
    "Book",
    new { id = BookId });
```

### Lời nói

> “Sau khi lưu thành công, action không trả View trực tiếp mà redirect về trang chi tiết sách. `TempData` giữ thông báo qua lần redirect để View hiển thị một lần.”

Đây là mẫu:

```text
POST
  ↓
Lưu dữ liệu
  ↓
Redirect
  ↓
GET lại trang
```

Có thể nói thêm:

> “Cách Post/Redirect/Get này tránh việc người dùng refresh trình duyệt rồi gửi trùng form.”

---

## 14. Quay lại View để show kết quả

### File

`BookVerse/Views/User/Book/Detail.cshtml`

### Show TempData

```cshtml
@if (TempData["SuccessMessage"] != null)
{
    <div class="alert alert-success">
        @TempData["SuccessMessage"]
    </div>
}
```

### Lời nói

> “Sau redirect, View đọc `SuccessMessage` từ TempData và hiển thị thông báo gửi thành công.”

### Show danh sách review

```cshtml
foreach (var r in Model.ExistingReviews)
{
    // Hiển thị tên, số sao, ngày và nội dung
}
```

### Lời nói

> “Review vừa gửi chưa xuất hiện ngay vì lúc tạo có `IsApproved = false`, trong khi `BookController` chỉ truy vấn review có `IsApproved == true`. Sau khi admin duyệt, lần GET tiếp theo review mới nằm trong `ExistingReviews`.”

---

## 15. Câu kết thúc workflow đánh giá

> “Tóm lại, `BookController` xử lý chiều đọc: lấy sách và review đã duyệt rồi đóng gói vào `BookDetailViewModel`. `ReviewController` xử lý chiều ghi: nhận form bằng model binding, lấy UserId từ claim, tạo Review chờ duyệt, lưu database và redirect lại trang sách.”

---

# PHẦN 3 — NẾU THẦY BẤM “THÊM VÀO GIỎ”

## File cần mở

`BookVerse/Views/User/Book/Detail.cshtml`

Tìm:

```javascript
function addToCart(bookId) {
    alert('Đã thêm sách vào giỏ hàng!');
}
```

## Nên trả lời trung thực

> “Phần này hiện tại mới là giao diện mô phỏng. Hàm JavaScript chỉ hiển thị alert, chưa gọi action và chưa lưu bảng Cart. Model Cart đã có nhưng `CartController`, trang giỏ và checkout phía user chưa được triển khai trong phiên bản hiện tại.”

Không nên nói rằng giỏ hàng đã hoạt động.

## Workflow đúng nếu được yêu cầu mô tả hướng phát triển

```text
User bấm thêm giỏ
       ↓
POST /Cart/Add
       ↓
CartController lấy UserId từ claim
       ↓
Kiểm tra sách và tồn kho
       ↓
Tìm Cart theo UserId + BookId
       ↓
Có rồi: tăng Quantity
Chưa có: tạo Cart
       ↓
SaveChangesAsync
       ↓
Trả kết quả thành công
```

Checkout đúng ra phải:

```text
Load Cart của user
       ↓
Tính tổng Book.Price × Cart.Quantity
       ↓
Kiểm tra voucher nếu có
       ↓
Tạo Order
       ↓
Tạo các OrderDetail
       ↓
Trừ tồn kho
       ↓
Xóa Cart
       ↓
SaveChanges trong transaction
```

---

# PHẦN 4 — BÀI NÓI NGẮN 2–3 PHÚT

> “Phần code phía user hiện có hai workflow backend chính là lịch sử mua hàng và đánh giá sách.”

> “Với lịch sử mua hàng, user bấm link `/Account/OrderHistory`. Request đi vào action `OrderHistory` trong `AccountController`. Action được gắn `[Authorize]`, nên bắt buộc đăng nhập. Controller lấy UserId từ `NameIdentifier` claim trong cookie, sau đó dùng Entity Framework lọc bảng Orders theo UserId. Nó lấy thêm OrderDetails để tính tổng số lượng sách, sắp xếp đơn mới nhất trước, rồi map dữ liệu sang `UserOrderListViewModel`. Danh sách ViewModel được truyền bằng `return View(orders)` sang `OrderHistory.cshtml`. View kiểm tra danh sách rỗng, sau đó foreach từng đơn để hiển thị mã đơn, ngày đặt, số lượng, tổng tiền và trạng thái.”

> “Với đánh giá sách, khi mở `/User/Book/Detail/{id}`, `BookController.Detail` tìm sách theo ID và lấy các review của sách có `IsApproved == true`. Sách và danh sách review được đóng gói vào `BookDetailViewModel` rồi truyền sang `Detail.cshtml`.”

> “Khi user gửi form, trình duyệt POST `BookId`, `Rating` và `Comment` đến `ReviewController.Create`. ASP.NET model binding gán dữ liệu form vào tham số action. Controller kiểm tra đăng nhập, lấy UserId từ claim, tạo entity Review với `IsApproved = false`, rồi gọi `SaveChangesAsync` để insert vào database. Sau đó controller lưu thông báo vào TempData và redirect lại trang sách. Vì review mới chưa được duyệt nên chưa hiện; admin duyệt xong thì `BookController` mới lấy và hiển thị.”

> “Riêng giỏ hàng và checkout phía user, phiên bản hiện tại mới có model database và giao diện mô phỏng. Nút thêm giỏ chỉ gọi JavaScript alert, chưa có CartController hoặc action lưu database.”

---

# PHẦN 5 — CHECKLIST MỞ FILE KHI REVIEW

## Workflow lịch sử

```text
[1] HomePage.cshtml
    Show href="/Account/OrderHistory"

[2] AccountController.cs
    Show [Authorize]
    Show lấy UserId từ claim
    Show query Orders
    Show Select UserOrderListViewModel
    Show return View(orders)

[3] UserOrderViewModel.cs
    Show các property được đóng gói

[4] OrderHistory.cshtml
    Show @model
    Show kiểm tra Model.Any()
    Show foreach
    Show các property được in ra
```

## Workflow đánh giá

```text
[1] BookController.cs
    Show route
    Show Detail(int id)
    Show query Book
    Show query Reviews có IsApproved == true
    Show đóng gói BookDetailViewModel
    Show return View

[2] BookDetailViewModel.cs
    Show Book và ExistingReviews

[3] Detail.cshtml
    Show kiểm tra đăng nhập
    Show form action="/Review/Create"
    Show AntiForgeryToken
    Show name BookId, Rating, Comment

[4] ReviewController.cs
    Show [HttpPost]
    Show các tham số action
    Show kiểm tra đăng nhập
    Show lấy UserId từ claim
    Show new Review
    Show IsApproved = false
    Show Add + SaveChangesAsync
    Show TempData + Redirect

[5] Detail.cshtml
    Show SuccessMessage
    Show foreach ExistingReviews
```

---

# PHẦN 6 — TỪ KHÓA PHẢI NHỚ

```text
Action
    Method trong Controller nhận và xử lý HTTP request.

Model binding
    ASP.NET tự gán dữ liệu URL/form vào tham số action.

Claim
    Thông tin user được lưu sau đăng nhập, dùng để lấy UserId.

Entity Framework Core
    Công cụ truy vấn và lưu SQL Server bằng C#.

ViewModel
    Gói dữ liệu Controller cần truyền cho View.

return View(model)
    Truyền model sang Razor View để tạo HTML.

Include
    Lấy thêm dữ liệu có quan hệ.

Where
    Lọc dữ liệu.

Select
    Chọn và chuyển dữ liệu sang hình dạng mới.

ToListAsync
    Thực thi truy vấn và nhận danh sách.

SaveChangesAsync
    Ghi thay đổi xuống database.

TempData
    Giữ dữ liệu ngắn hạn qua một lần redirect.

RedirectToAction
    Yêu cầu trình duyệt tạo request mới tới action khác.

Authorize
    Chỉ cho người đã đăng nhập vào action.

Anti-forgery token
    Bảo vệ POST form khỏi request giả mạo.
```

---

# PHẦN 7 — ÁNH XẠ ĐẾN ĐÂU, DỮ LIỆU ĐI NHƯ THẾ NÀO

## A. Lịch sử mua hàng

### Sơ đồ ánh xạ đầy đủ

```text
Trình duyệt
GET /Account/OrderHistory
        ↓ routing
AccountController
        ↓ chọn action theo tên
OrderHistory()
        ↓ đọc authentication cookie
Claim NameIdentifier
        ↓ chuyển string thành int
userId
        ↓ dùng trong câu query
QuanLyBanSachContext.Orders
        ↓ ánh xạ tới bảng Orders trong SQL Server
Where(Order.UserId == userId)
        ↓ tìm OrderDetails liên quan
Tính TotalItems
        ↓ Select/map
List<UserOrderListViewModel>
        ↓ return View(orders)
Views/Account/OrderHistory.cshtml
        ↓ Razor render
HTML trả về trình duyệt
```

### 1. URL ánh xạ đến Controller và Action

URL:

```text
/Account/OrderHistory
```

Route mặc định trong `Program.cs`:

```csharp
pattern: "{controller=Home}/{action=HomePage}/{id?}"
```

ASP.NET tách URL:

```text
Account      → AccountController
OrderHistory → method OrderHistory()
Không có id  → không truyền id
```

Nếu tìm được controller và action:

```text
Chạy AccountController.OrderHistory()
```

Nếu không tìm được controller/action:

```text
Trả HTTP 404 Not Found
```

### 2. `[Authorize]` tạo hai nhánh

```text
Request vào OrderHistory
        ↓
User đã đăng nhập?
   ├── Không
   │     ↓
   │  Cookie Authentication chặn request
   │     ↓
   │  Redirect /Account/Login?ReturnUrl=...
   │
   └── Có
         ↓
      Cho action chạy
```

Lời nói:

> “Trước khi action chạy, middleware kiểm tra `[Authorize]`. Nếu chưa đăng nhập thì action chưa được thực thi mà hệ thống redirect về Login. Nếu đã đăng nhập thì request mới đi tiếp vào code.”

### 3. Claim ánh xạ thành `userId`

Cookie chứa claim gần giống:

```text
NameIdentifier = "5"
Name = "Nguyễn Văn A"
Role = "User"
```

Code:

```csharp
var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
```

Ánh xạ:

```text
Claim NameIdentifier
        ↓ .Value
Chuỗi "5"
        ↓ int.TryParse
Số nguyên userId = 5
```

Hai trường hợp:

```text
Có claim và parse được
        ↓
Tiếp tục query database

Không có claim hoặc parse thất bại
        ↓
return RedirectToAction("Login")
        ↓
HTTP redirect tới /Account/Login
```

### 4. DbContext ánh xạ đến database

```csharp
_context.Orders
```

được ánh xạ tới:

```text
DbSet<Order>
        ↓ Entity Framework Core
Bảng Orders trong SQL Server
```

`Order` có collection:

```csharp
Order.OrderDetails
```

được ánh xạ tới các dòng:

```text
Bảng OrderDetails
WHERE OrderDetails.OrderId = Orders.OrderId
```

### 5. Query tạo hai kết quả có/không

```csharp
.Where(o => o.UserId == userId)
```

Nếu user có đơn:

```text
Database trả 1 hoặc nhiều dòng Order
        ↓
Mỗi Order được map thành một UserOrderListViewModel
        ↓
orders là danh sách có phần tử
```

Nếu user không có đơn:

```text
Database không trả dòng nào
        ↓
ToListAsync không trả null
        ↓
orders là List rỗng
```

Điểm cần nhớ:

> “`ToListAsync()` thông thường trả danh sách rỗng nếu không có dữ liệu, chứ không trả null.”

### 6. Entity ánh xạ sang ViewModel

Ví dụ dữ liệu database:

```text
Orders
OrderId = 12
UserId = 5
CreatedAt = 20/07/2026
TotalAmount = 450000
Status = "Đang giao"
PaymentStatus = "Chưa thanh toán"

OrderDetails
OrderId = 12, Quantity = 2
OrderId = 12, Quantity = 1
```

Qua đoạn:

```csharp
.Select(o => new UserOrderListViewModel
{
    OrderId = o.OrderId,
    CreatedAt = o.CreatedAt,
    TotalItems = o.OrderDetails.Sum(d => d.Quantity ?? 0),
    TotalAmount = o.TotalAmount ?? 0,
    Status = o.Status,
    PaymentStatus = o.PaymentStatus
})
```

thì được đóng gói thành:

```text
UserOrderListViewModel
OrderId = 12
CreatedAt = 20/07/2026
TotalItems = 3
TotalAmount = 450000
Status = "Đang giao"
PaymentStatus = "Chưa thanh toán"
```

Các trường ánh xạ:

```text
Order.OrderId              → ViewModel.OrderId
Order.CreatedAt            → ViewModel.CreatedAt
Sum(OrderDetail.Quantity)  → ViewModel.TotalItems
Order.TotalAmount          → ViewModel.TotalAmount
Order.Status               → ViewModel.Status
Order.PaymentStatus        → ViewModel.PaymentStatus
```

### 7. Controller trả về đâu

```csharp
return View(orders);
```

Vì action tên `OrderHistory` và controller tên `AccountController`, MVC theo convention tìm:

```text
Views/Account/OrderHistory.cshtml
```

Dữ liệu đi như sau:

```text
Biến orders trong Controller
        ↓ return View(orders)
Thuộc tính Model trong Razor View
```

Trong View:

```cshtml
@model IEnumerable<UserOrderListViewModel>
```

Hai trường hợp:

```text
Model.Any() == false
        ↓
Hiện “Bạn chưa thực hiện đơn hàng nào”

Model.Any() == true
        ↓
foreach từng order
        ↓
Tạo từng <tr> trong bảng HTML
```

Kết quả cuối cùng:

```text
Controller trả ViewResult
        ↓ Razor render
Server trả HTTP 200 + HTML
        ↓
Trình duyệt hiển thị trang lịch sử
```

### Một câu nói đầy đủ

> “URL `/Account/OrderHistory` được route tới `AccountController.OrderHistory`. `[Authorize]` kiểm tra đăng nhập. Action lấy claim `NameIdentifier`, parse thành UserId rồi dùng `_context.Orders` để truy vấn bảng Orders. Dữ liệu Order và tổng Quantity từ OrderDetails được map sang danh sách `UserOrderListViewModel`. Nếu không có đơn thì danh sách rỗng; nếu có thì chứa các ViewModel. `return View(orders)` truyền danh sách sang `Views/Account/OrderHistory.cshtml`. View kiểm tra `Any()`: không có thì hiện thông báo, có thì foreach và render bảng. Kết quả cuối là HTTP 200 chứa HTML.”

---

## B. Mở trang chi tiết và đọc đánh giá

### Sơ đồ ánh xạ đầy đủ

```text
GET /User/Book/Detail/10
        ↓ attribute routing
BookController.Detail(int id)
        ↓ model binding từ URL
id = 10
        ↓ query
Books + Category
        ↓ có/không
Book hoặc HTTP 404
        ↓ query tiếp
Reviews của BookId 10 có IsApproved = true
        ↓ đóng gói
BookDetailViewModel
   ├── Book
   └── ExistingReviews
        ↓ return View
Views/User/Book/Detail.cshtml
        ↓ render
HTML trang chi tiết
```

### 1. URL ánh xạ đến action

Controller có:

```csharp
[Route("User/[controller]/[action]/{id?}")]
```

URL:

```text
/User/Book/Detail/10
```

được tách thành:

```text
User        → tiền tố cố định
Book        → BookController
Detail      → action Detail
10          → tham số id
```

Model binding thực hiện:

```text
Chuỗi "10" trên URL
        ↓
int id = 10
```

Nếu `"10"` không chuyển được sang số nguyên:

```text
Model binding không tạo được tham số hợp lệ
        ↓
Request không chạy đúng action/kết quả lỗi client tùy route binding
```

### 2. Query sách tạo hai nhánh

```csharp
var book = await _context.Books
    .Include(b => b.Category)
    .FirstOrDefaultAsync(b => b.BookId == id);
```

Ánh xạ:

```text
_context.Books → bảng Books
book.Category  → bảng Categories liên quan
```

Hai trường hợp:

```text
Có BookId = 10
        ↓
book chứa entity Book
        ↓
Tiếp tục lấy review

Không có BookId = 10
        ↓
book = null
        ↓
return NotFound()
        ↓
HTTP 404, dừng workflow
```

### 3. Query review tạo danh sách có/không

```csharp
var approvedReviews = await _context.Reviews
    .Include(r => r.User)
    .Where(r => r.BookId == id && r.IsApproved == true)
    .OrderByDescending(r => r.CreatedAt)
    .ToListAsync();
```

Ánh xạ:

```text
_context.Reviews → bảng Reviews
r.User           → bảng Users liên quan
```

Nếu có review đã duyệt:

```text
approvedReviews = danh sách Review có phần tử
```

Nếu chỉ có review chưa duyệt hoặc chưa có review:

```text
approvedReviews = danh sách rỗng
```

Review `IsApproved = false` không được đưa vào danh sách.

### 4. Đóng gói sang một ViewModel

```csharp
var viewModel = new BookDetailViewModel
{
    Book = book,
    ExistingReviews = approvedReviews
};
```

Dữ liệu:

```text
Entity Book ----------------┐
                            ├→ BookDetailViewModel
List<Review> đã duyệt -------┘
```

Controller trả:

```csharp
return View("~/Views/User/Book/Detail.cshtml", viewModel);
```

Ánh xạ:

```text
Biến viewModel
        ↓
Model trong Detail.cshtml
        ├── Model.Book
        └── Model.ExistingReviews
```

Trong View:

```text
Book có dữ liệu
   → hiện tên, tác giả, giá, tồn kho

ExistingReviews có phần tử
   → foreach và hiện từng đánh giá

ExistingReviews rỗng
   → hiện “Chưa có lượt đánh giá nào”
```

Kết quả:

```text
Sách tồn tại → HTTP 200 + HTML
Sách không tồn tại → HTTP 404
```

---

## C. Gửi đánh giá

### Sơ đồ ánh xạ đầy đủ

```text
User nhập form
BookId=10, Rating=5, Comment="Sách hay"
        ↓ submit
POST /Review/Create
        ↓ routing
ReviewController.Create(...)
        ↓ model binding theo name
BookId, Rating, Comment
        ↓ kiểm tra đăng nhập
        ↓ kiểm tra input
        ↓ lấy UserId từ claim
        ↓ đóng gói new Review
        ↓ Add + SaveChangesAsync
Bảng Reviews
        ↓ TempData
        ↓ RedirectToAction
GET /User/Book/Detail/10
        ↓
Hiển thị thông báo thành công
```

### 1. Input ánh xạ vào tham số action

View gửi:

```html
<input name="BookId" />
<select name="Rating"></select>
<textarea name="Comment"></textarea>
```

Action nhận:

```csharp
Create(int BookId, int Rating, string Comment)
```

Model binding dựa vào tên:

```text
Form name="BookId"  → tham số BookId
Form name="Rating"  → tham số Rating
Form name="Comment" → tham số Comment
```

Ví dụ:

```text
HTTP form                   Action
BookId = "10"      →        BookId = 10
Rating = "5"       →        Rating = 5
Comment = "Hay"    →        Comment = "Hay"
```

### 2. Anti-forgery tạo hai nhánh

```text
POST có token hợp lệ
        ↓
Action được phép chạy

POST thiếu/sai token
        ↓
[ValidateAntiForgeryToken] chặn
        ↓
HTTP 400 Bad Request
```

### 3. Đăng nhập tạo hai nhánh

```csharp
if (!User.Identity.IsAuthenticated)
{
    return RedirectToAction("Login", "Account");
}
```

```text
Chưa đăng nhập
        ↓
Redirect /Account/Login
        ↓
Không tạo Review

Đã đăng nhập
        ↓
Tiếp tục kiểm tra input
```

### 4. Input hợp lệ tạo hai nhánh

```csharp
if (BookId <= 0 || string.IsNullOrWhiteSpace(Comment))
```

```text
BookId <= 0 hoặc Comment rỗng
        ↓
Redirect về Book/Detail
        ↓
Không Add, không SaveChanges

BookId > 0 và Comment có nội dung
        ↓
Tiếp tục tạo Review
```

Lưu ý thực tế:

> “Code hiện tại chưa kiểm tra Rating có nằm trong 1–5 và chưa kiểm tra BookId có thật sự tồn tại hay không.”

### 5. Claim và form cùng được đóng gói vào entity

Dữ liệu đến từ hai nguồn:

```text
Từ form:
  BookId
  Rating
  Comment

Từ authentication claim:
  UserId

Do server tự tạo:
  IsApproved = false
  CreatedAt = DateTime.Now
```

Tất cả được đóng gói:

```csharp
var newReview = new Review
{
    BookId = BookId,
    UserId = userId,
    Rating = Rating,
    Comment = Comment.Trim(),
    IsApproved = false,
    CreatedAt = DateTime.Now
};
```

Ánh xạ xuống bảng:

```text
newReview.BookId       → Reviews.BookId
newReview.UserId       → Reviews.UserId
newReview.Rating       → Reviews.Rating
newReview.Comment      → Reviews.Comment
newReview.IsApproved   → Reviews.IsApproved
newReview.CreatedAt    → Reviews.CreatedAt
```

`ReviewId` không được nhập từ form; database tạo khóa chính khi insert.

### 6. Add và Save trả về như thế nào

```csharp
_context.Reviews.Add(newReview);
await _context.SaveChangesAsync();
```

Nếu lưu thành công:

```text
Database INSERT thành công
        ↓
newReview được gán ReviewId
        ↓
Chạy tiếp TempData và Redirect
```

Nếu database lỗi:

```text
SaveChangesAsync ném exception
        ↓
Code hiện tại không catch
        ↓
Request đi vào error handling của ASP.NET
        ↓
Không chạy đến TempData/Redirect
```

### 7. Redirect trả về gì

```csharp
return RedirectToAction("Detail", "Book", new { id = BookId });
```

Nó không trực tiếp trả HTML trang sách.

Nó trả response redirect:

```text
HTTP 302
Location: /User/Book/Detail/10
```

Trình duyệt nhận response rồi tự gửi request mới:

```text
GET /User/Book/Detail/10
```

Request GET mới lại chạy:

```text
BookController.Detail(10)
        ↓
Detail.cshtml
```

### 8. Vì sao hiện thông báo nhưng chưa hiện review?

Sau POST:

```text
TempData["SuccessMessage"] có dữ liệu
        ↓
View hiện alert thành công
```

Nhưng Review mới:

```text
IsApproved = false
```

Trong khi câu query hiển thị yêu cầu:

```text
IsApproved == true
```

Do đó:

```text
Thông báo thành công: Có
Review trong danh sách: Chưa có
```

Sau khi admin duyệt:

```text
IsApproved: false → true
        ↓
User mở lại trang
        ↓
BookController query thấy review
        ↓
ExistingReviews có review
        ↓
View hiển thị
```

### Một câu nói đầy đủ

> “Form POST ba field BookId, Rating và Comment. Model binding ánh xạ chúng vào ba tham số cùng tên của `ReviewController.Create`. Anti-forgery token sai thì trả 400. Chưa đăng nhập hoặc input không hợp lệ thì redirect và không lưu. Nếu hợp lệ, controller lấy thêm UserId từ claim, đóng gói dữ liệu thành entity Review với `IsApproved = false`, rồi Add và SaveChangesAsync xuống bảng Reviews. Nếu lưu thành công, action trả HTTP 302 redirect về trang chi tiết. Trình duyệt tạo GET request mới tới `BookController.Detail`. TempData làm thông báo xuất hiện, nhưng review chưa hiện cho tới khi admin đổi IsApproved thành true.”

---

# PHẦN 8 — BẢNG KẾT QUẢ “NẾU CÓ / NẾU KHÔNG”

| Tình huống | Code xử lý | Kết quả trả về |
|---|---|---|
| Mở lịch sử nhưng chưa đăng nhập | `[Authorize]` | Redirect tới Login |
| Đã đăng nhập nhưng claim lỗi | `RedirectToAction("Login")` | HTTP 302 tới Login |
| User không có đơn | `ToListAsync()` trả list rỗng | View hiện “chưa có đơn” |
| User có đơn | `Select` tạo ViewModel | View foreach và hiện bảng |
| URL sách không tồn tại | `return NotFound()` | HTTP 404 |
| Sách có nhưng chưa có review | List review rỗng | View hiện chưa có đánh giá |
| Có review chưa duyệt | Bị loại bởi `IsApproved == true` | Không hiển thị |
| Có review đã duyệt | Query lấy được | View foreach và hiển thị |
| POST review thiếu/sai token | Anti-forgery chặn | HTTP 400 |
| POST khi chưa đăng nhập | Redirect Login | Không lưu review |
| BookId <= 0 hoặc comment rỗng | Redirect Detail | Không lưu review |
| Review hợp lệ và lưu thành công | Save + TempData + Redirect | HTTP 302, rồi GET trang sách |
| Database lỗi khi lưu | Exception | Error handler xử lý, không redirect thành công |

