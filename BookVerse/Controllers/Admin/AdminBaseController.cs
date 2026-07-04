using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookVerse.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    public class AdminBaseController : Controller
    {
        // Tất cả Admin controllers kế thừa base này.
        // [Authorize(Roles = "Admin")] → chỉ Admin mới vào được.
        // Views được tìm trong Views/Admin/{Controller}/ nhờ ViewLocationFormats trong Program.cs
    }
}
