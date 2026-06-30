import { useState, useMemo } from "react";
import {
  ShoppingCart, Search, Star, X, Menu, Package, Users,
  BarChart2, BookOpen, TrendingUp, Plus, Minus, Trash2,
  Eye, CheckCircle, Clock, Truck, AlertCircle, Edit2,
  User, Bell, ArrowLeft, Home, Tag, ChevronDown,
  Filter, Heart, ShoppingBag, ChevronRight, FileText,
  DollarSign, RefreshCcw, Check, BookMarked, LogOut,
  AlertTriangle, Layers, ListOrdered, UserCheck, Grid3X3
} from "lucide-react";
import {
  AreaChart, Area, BarChart, Bar, PieChart, Pie, Cell,
  XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, Legend
} from "recharts";

// ─── TYPES ────────────────────────────────────────────────────────────────────
type Role = "customer" | "staff" | "admin";
type OrderStatus = "pending" | "confirmed" | "shipping" | "delivered" | "cancelled";

interface Book {
  id: number;
  title: string;
  author: string;
  price: number;
  originalPrice: number;
  category: string;
  cover: string;
  stock: number;
  rating: number;
  reviews: number;
  description: string;
  sold: number;
  isbn: string;
  publisher: string;
  year: number;
  pages: number;
}

interface CartItem { book: Book; quantity: number; }

interface Order {
  id: string;
  customerId: number;
  customerName: string;
  items: CartItem[];
  total: number;
  status: OrderStatus;
  date: string;
  address: string;
  phone: string;
}

interface AppUser {
  id: number;
  name: string;
  email: string;
  phone: string;
  role: "customer" | "staff" | "admin";
  joinDate: string;
  totalOrders: number;
  totalSpent: number;
  active: boolean;
}

// ─── MOCK DATA ────────────────────────────────────────────────────────────────
const CATEGORIES = [
  "Văn học Việt Nam", "Văn học nước ngoài",
  "Kinh tế - Kinh doanh", "Kỹ năng sống",
  "Khoa học - Công nghệ", "Thiếu nhi"
];

const BOOKS_INITIAL: Book[] = [
  { id: 1, title: "Số Đỏ", author: "Vũ Trọng Phụng", price: 89000, originalPrice: 110000, category: "Văn học Việt Nam", cover: "https://images.unsplash.com/photo-1544947950-fa07a98d237f?w=280&h=380&fit=crop&auto=format", stock: 45, rating: 4.8, reviews: 234, sold: 890, description: "Tiểu thuyết trào phúng kinh điển của văn học hiện đại Việt Nam, phản ánh sắc sảo xã hội thời thuộc địa với những mâu thuẫn giai cấp và bi hài kịch đời thường.", isbn: "978-604-2-15234-1", publisher: "NXB Hội Nhà Văn", year: 2023, pages: 320 },
  { id: 2, title: "Chí Phèo", author: "Nam Cao", price: 65000, originalPrice: 75000, category: "Văn học Việt Nam", cover: "https://images.unsplash.com/photo-1512820790803-83ca734da794?w=280&h=380&fit=crop&auto=format", stock: 38, rating: 4.9, reviews: 456, sold: 1200, description: "Tập truyện ngắn bất hủ của nhà văn Nam Cao, đỉnh cao của chủ nghĩa hiện thực trong văn học Việt Nam. Chí Phèo là nhân vật bi kịch nhất trong nền văn học nước nhà.", isbn: "978-604-2-18765-3", publisher: "NXB Văn Học", year: 2022, pages: 248 },
  { id: 3, title: "Truyện Kiều", author: "Nguyễn Du", price: 95000, originalPrice: 120000, category: "Văn học Việt Nam", cover: "https://images.unsplash.com/photo-1481627834876-b7833e8f5570?w=280&h=380&fit=crop&auto=format", stock: 60, rating: 4.9, reviews: 789, sold: 2100, description: "Kiệt tác văn chương của đại thi hào Nguyễn Du, viên ngọc sáng của nền văn học cổ điển Việt Nam với 3254 câu thơ lục bát đầy xúc cảm và triết lý nhân sinh.", isbn: "978-604-2-11234-5", publisher: "NXB Giáo Dục", year: 2023, pages: 380 },
  { id: 4, title: "Tắt Đèn", author: "Ngô Tất Tố", price: 72000, originalPrice: 85000, category: "Văn học Việt Nam", cover: "https://images.unsplash.com/photo-1495640388908-05fa85288e61?w=280&h=380&fit=crop&auto=format", stock: 27, rating: 4.7, reviews: 312, sold: 750, description: "Tiểu thuyết hiện thực xuất sắc về cuộc sống khổ cực của người nông dân Việt Nam trước Cách mạng. Nhân vật chị Dậu đã trở thành biểu tượng của người phụ nữ Việt Nam kiên cường.", isbn: "978-604-2-19876-2", publisher: "NXB Văn Học", year: 2021, pages: 276 },
  { id: 5, title: "Nhà Thờ Đức Bà Paris", author: "Victor Hugo", price: 125000, originalPrice: 150000, category: "Văn học nước ngoài", cover: "https://images.unsplash.com/photo-1516979187457-637abb4f9353?w=280&h=380&fit=crop&auto=format", stock: 33, rating: 4.8, reviews: 567, sold: 980, description: "Kiệt tác của đại văn hào Victor Hugo — câu chuyện bi tình của gã gù Quasimodo và vũ nữ Esmeralda giữa bức tranh Paris thời Trung Cổ huy hoàng.", isbn: "978-604-2-22345-6", publisher: "NXB Hội Nhà Văn", year: 2023, pages: 624 },
  { id: 6, title: "Chiến Tranh Và Hòa Bình", author: "Lev Tolstoy", price: 285000, originalPrice: 350000, category: "Văn học nước ngoài", cover: "https://images.unsplash.com/photo-1589998059171-988d887df646?w=280&h=380&fit=crop&auto=format", stock: 18, rating: 4.9, reviews: 423, sold: 650, description: "Sử thi đồ sộ về cuộc chiến tranh vệ quốc của nước Nga chống lại Napoleon năm 1812. Bộ tiểu thuyết vĩ đại nhất của văn học Nga với số phận nhiều gia đình quý tộc.", isbn: "978-604-2-33456-7", publisher: "NXB Văn Học", year: 2022, pages: 1456 },
  { id: 7, title: "Tội Ác Và Hình Phạt", author: "Fyodor Dostoevsky", price: 165000, originalPrice: 195000, category: "Văn học nước ngoài", cover: "https://images.unsplash.com/photo-1532012197267-da84d127e765?w=280&h=380&fit=crop&auto=format", stock: 24, rating: 4.8, reviews: 389, sold: 720, description: "Kiệt tác tâm lý học của Dostoevsky — câu chuyện sinh viên Raskolnikov phạm tội giết người và hành trình tìm kiếm sự cứu rỗi. Đỉnh cao tâm lý học trong văn học thế giới.", isbn: "978-604-2-44567-8", publisher: "NXB Hội Nhà Văn", year: 2023, pages: 592 },
  { id: 8, title: "Một Trăm Năm Cô Đơn", author: "Gabriel García Márquez", price: 135000, originalPrice: 165000, category: "Văn học nước ngoài", cover: "https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=280&h=380&fit=crop&auto=format", stock: 29, rating: 4.9, reviews: 612, sold: 1100, description: "Tiểu thuyết huyền ảo - hiện thực xuất sắc nhất của Nobel văn học, kể về bảy thế hệ gia đình Buendía trong ngôi làng huyền bí Macondo đầy mê hoặc.", isbn: "978-604-2-55678-9", publisher: "NXB Hội Nhà Văn", year: 2022, pages: 488 },
  { id: 9, title: "Đắc Nhân Tâm", author: "Dale Carnegie", price: 88000, originalPrice: 105000, category: "Kỹ năng sống", cover: "https://images.unsplash.com/photo-1553729459-efe14ef6055d?w=280&h=380&fit=crop&auto=format", stock: 85, rating: 4.7, reviews: 1245, sold: 3500, description: "Cuốn sách self-help kinh điển nhất mọi thời đại, hướng dẫn nghệ thuật giao tiếp và xây dựng mối quan hệ với hơn 30 triệu bản được bán trên toàn thế giới.", isbn: "978-604-2-66789-0", publisher: "NXB Tổng Hợp TP.HCM", year: 2023, pages: 368 },
  { id: 10, title: "Nghĩ Giàu Làm Giàu", author: "Napoleon Hill", price: 92000, originalPrice: 115000, category: "Kỹ năng sống", cover: "https://images.unsplash.com/photo-1580894894513-541e068a3e2b?w=280&h=380&fit=crop&auto=format", stock: 62, rating: 4.6, reviews: 987, sold: 2800, description: "Đúc kết 13 nguyên tắc vàng để thành công trong sự nghiệp và cuộc sống từ nghiên cứu 500 người thành đạt nhất nước Mỹ. Cuốn sách thay đổi cuộc đời hàng triệu người.", isbn: "978-604-2-77890-1", publisher: "NXB Lao Động", year: 2022, pages: 312 },
  { id: 11, title: "7 Thói Quen Hiệu Quả", author: "Stephen R. Covey", price: 115000, originalPrice: 138000, category: "Kỹ năng sống", cover: "https://images.unsplash.com/photo-1434030216411-0b793f4b6f61?w=280&h=380&fit=crop&auto=format", stock: 41, rating: 4.8, reviews: 876, sold: 2200, description: "7 thói quen then chốt giúp bạn trở thành người hiệu quả toàn diện trong công việc, gia đình và cuộc sống. Một trong những cuốn sách phát triển bản thân hàng đầu thế giới.", isbn: "978-604-2-88901-2", publisher: "NXB Tổng Hợp TP.HCM", year: 2023, pages: 424 },
  { id: 12, title: "Atomic Habits", author: "James Clear", price: 98000, originalPrice: 120000, category: "Kỹ năng sống", cover: "https://images.unsplash.com/photo-1506784983877-45594efa4cbe?w=280&h=380&fit=crop&auto=format", stock: 73, rating: 4.9, reviews: 1567, sold: 4200, description: "Hệ thống xây dựng thói quen hiệu quả nhất từ trước đến nay. James Clear chỉ ra rằng thay đổi nhỏ 1% mỗi ngày sẽ tạo ra sự khác biệt khổng lồ sau một năm.", isbn: "978-604-2-99012-3", publisher: "NXB Công Thương", year: 2023, pages: 296 },
  { id: 13, title: "Khởi Nghiệp Tinh Gọn", author: "Eric Ries", price: 105000, originalPrice: 128000, category: "Kinh tế - Kinh doanh", cover: "https://images.unsplash.com/photo-1611974789855-9c2a0a7236a3?w=280&h=380&fit=crop&auto=format", stock: 35, rating: 4.6, reviews: 654, sold: 1450, description: "Phương pháp luận khởi nghiệp hiện đại đã thay đổi cách thế giới xây dựng công ty. Lean Startup giúp bạn kiểm tra ý tưởng nhanh và xoay chuyển đúng lúc.", isbn: "978-604-2-10123-4", publisher: "NXB Trẻ", year: 2022, pages: 336 },
  { id: 14, title: "Từ Tốt Đến Vĩ Đại", author: "Jim Collins", price: 128000, originalPrice: 155000, category: "Kinh tế - Kinh doanh", cover: "https://images.unsplash.com/photo-1486312338219-ce68d2c6f44d?w=280&h=380&fit=crop&auto=format", stock: 22, rating: 4.7, reviews: 432, sold: 980, description: "Nghiên cứu kỳ công về những công ty vĩ đại nhất thế giới và bí quyết chuyển hóa từ doanh nghiệp tốt thành doanh nghiệp xuất sắc qua 5 năm nghiên cứu thực địa.", isbn: "978-604-2-11234-5", publisher: "NXB Trẻ", year: 2021, pages: 368 },
  { id: 15, title: "Nhà Giả Kim", author: "Paulo Coelho", price: 79000, originalPrice: 95000, category: "Kinh tế - Kinh doanh", cover: "https://images.unsplash.com/photo-1588776814546-1ffbb2aa5cad?w=280&h=380&fit=crop&auto=format", stock: 95, rating: 4.8, reviews: 2345, sold: 5600, description: "Tiểu thuyết triết học nổi tiếng nhất thế giới về hành trình theo đuổi giấc mơ của người chăn cừu trẻ Santiago. Đã được dịch ra hơn 80 ngôn ngữ trên khắp thế giới.", isbn: "978-604-2-22345-6", publisher: "NXB Văn Học", year: 2023, pages: 224 },
  { id: 16, title: "Sapiens: Lược Sử Loài Người", author: "Yuval Noah Harari", price: 189000, originalPrice: 225000, category: "Khoa học - Công nghệ", cover: "https://images.unsplash.com/photo-1451187580459-43490279c0fa?w=280&h=380&fit=crop&auto=format", stock: 48, rating: 4.9, reviews: 1876, sold: 4100, description: "Lịch sử toàn diện về loài người từ thời tiền sử đến hiện đại. Harari trả lời câu hỏi Homo Sapiens đã thống trị Trái Đất như thế nào bằng ngôn từ hấp dẫn và tư duy đột phá.", isbn: "978-604-2-33456-7", publisher: "NXB Thế Giới", year: 2023, pages: 496 },
  { id: 17, title: "Lược Sử Thời Gian", author: "Stephen Hawking", price: 115000, originalPrice: 140000, category: "Khoa học - Công nghệ", cover: "https://images.unsplash.com/photo-1446776877081-d282a0f896e2?w=280&h=380&fit=crop&auto=format", stock: 31, rating: 4.8, reviews: 987, sold: 2200, description: "Cuốn sách vật lý nổi tiếng nhất mọi thời đại của thiên tài Stephen Hawking giúp độc giả phổ thông hiểu được những bí ẩn về vũ trụ, không gian, thời gian và lỗ đen.", isbn: "978-604-2-44567-8", publisher: "NXB Trẻ", year: 2022, pages: 256 },
  { id: 18, title: "Python: Từ Cơ Bản Đến Nâng Cao", author: "Nguyễn Minh Tuấn", price: 195000, originalPrice: 240000, category: "Khoa học - Công nghệ", cover: "https://images.unsplash.com/photo-1555949963-aa79dcee981c?w=280&h=380&fit=crop&auto=format", stock: 44, rating: 4.6, reviews: 765, sold: 1800, description: "Tài liệu học Python toàn diện nhất tiếng Việt — từ cú pháp cơ bản đến lập trình hướng đối tượng, xử lý dữ liệu với Pandas và xây dựng ứng dụng thực tế.", isbn: "978-604-2-99012-3", publisher: "NXB Thông Tin Truyền Thông", year: 2023, pages: 512 },
  { id: 19, title: "Thế Giới Phẳng", author: "Thomas L. Friedman", price: 145000, originalPrice: 175000, category: "Khoa học - Công nghệ", cover: "https://images.unsplash.com/photo-1526374965328-7f61d4dc18c5?w=280&h=380&fit=crop&auto=format", stock: 20, rating: 4.5, reviews: 543, sold: 1100, description: "Phân tích sắc bén về toàn cầu hóa và cuộc cách mạng công nghệ đang làm phẳng thế giới, xóa bỏ ranh giới quốc gia trong kinh doanh và cạnh tranh toàn cầu.", isbn: "978-604-2-55678-9", publisher: "NXB Trẻ", year: 2021, pages: 488 },
  { id: 20, title: "Harry Potter và Hòn Đá Phù Thủy", author: "J.K. Rowling", price: 120000, originalPrice: 145000, category: "Thiếu nhi", cover: "https://images.unsplash.com/photo-1456513080510-7bf3a84b82f8?w=280&h=380&fit=crop&auto=format", stock: 67, rating: 4.9, reviews: 4567, sold: 11200, description: "Câu chuyện huyền diệu về cậu bé phù thủy Harry Potter và ngôi trường phép thuật Hogwarts. Cuốn sách mở đầu cho bộ series vĩ đại nhất lịch sử văn học thiếu nhi thế giới.", isbn: "978-604-2-77890-1", publisher: "NXB Trẻ", year: 2023, pages: 328 },
  { id: 21, title: "Hoàng Tử Bé", author: "Antoine de Saint-Exupéry", price: 68000, originalPrice: 82000, category: "Thiếu nhi", cover: "https://images.unsplash.com/photo-1462275646964-a0e3386b89fa?w=280&h=380&fit=crop&auto=format", stock: 54, rating: 4.9, reviews: 2890, sold: 6700, description: "Câu chuyện triết lý đầy thơ mộng về Hoàng Tử Bé từ tiểu hành tinh B-612 du hành đến Trái Đất, mang theo những bài học sâu sắc về tình yêu và ý nghĩa cuộc sống.", isbn: "978-604-2-88901-2", publisher: "NXB Hội Nhà Văn", year: 2022, pages: 144 },
  { id: 22, title: "Dế Mèn Phiêu Lưu Ký", author: "Tô Hoài", price: 55000, originalPrice: 68000, category: "Thiếu nhi", cover: "https://images.unsplash.com/photo-1497633762265-9d179a990aa6?w=280&h=380&fit=crop&auto=format", stock: 88, rating: 4.8, reviews: 1234, sold: 3400, description: "Thiên phiêu lưu ký bất hủ của nhà văn Tô Hoài về chú dế mèn tinh nghịch trong thế giới loài vật sinh động. Tác phẩm kinh điển của văn học thiếu nhi Việt Nam.", isbn: "978-604-2-10234-6", publisher: "NXB Kim Đồng", year: 2023, pages: 192 },
  { id: 23, title: "Bước Đi Của Người Khổng Lồ", author: "Sam Walton", price: 115000, originalPrice: 138000, category: "Kinh tế - Kinh doanh", cover: "https://images.unsplash.com/photo-1507679799987-c73779587ccf?w=280&h=380&fit=crop&auto=format", stock: 16, rating: 4.5, reviews: 321, sold: 680, description: "Câu chuyện thật về hành trình xây dựng đế chế bán lẻ Walmart từ một cửa hàng nhỏ đến tập đoàn lớn nhất thế giới của Sam Walton — người đã viết lại luật chơi kinh doanh.", isbn: "978-604-2-11345-7", publisher: "NXB Trẻ", year: 2021, pages: 344 },
  { id: 24, title: "Doraemon - Tập 1", author: "Fujiko F. Fujio", price: 35000, originalPrice: 42000, category: "Thiếu nhi", cover: "https://images.unsplash.com/photo-1501286353178-1ec881214838?w=280&h=380&fit=crop&auto=format", stock: 120, rating: 4.9, reviews: 3456, sold: 8900, description: "Bộ truyện tranh huyền thoại về chú mèo máy đến từ tương lai Doraemon và cậu bé Nobita với những bảo bối thần kỳ và những chuyến phiêu lưu không bao giờ chán.", isbn: "978-604-2-66789-0", publisher: "NXB Kim Đồng", year: 2023, pages: 176 },
];

const ORDERS_INITIAL: Order[] = [
  { id: "ORD-2024-001", customerId: 1, customerName: "Nguyễn Văn An", items: [{ book: BOOKS_INITIAL[0], quantity: 2 }, { book: BOOKS_INITIAL[8], quantity: 1 }], total: 267000, status: "delivered", date: "2024-03-15", address: "123 Lê Lợi, Quận 1, TP.HCM", phone: "0901234567" },
  { id: "ORD-2024-002", customerId: 2, customerName: "Trần Thị Bình", items: [{ book: BOOKS_INITIAL[4], quantity: 1 }, { book: BOOKS_INITIAL[7], quantity: 1 }], total: 260000, status: "shipping", date: "2024-03-18", address: "456 Nguyễn Huệ, Quận 1, TP.HCM", phone: "0912345678" },
  { id: "ORD-2024-003", customerId: 3, customerName: "Lê Minh Cường", items: [{ book: BOOKS_INITIAL[15], quantity: 1 }, { book: BOOKS_INITIAL[16], quantity: 1 }], total: 304000, status: "confirmed", date: "2024-03-20", address: "789 Trần Hưng Đạo, Quận 5, TP.HCM", phone: "0923456789" },
  { id: "ORD-2024-004", customerId: 4, customerName: "Phạm Thị Dung", items: [{ book: BOOKS_INITIAL[11], quantity: 3 }], total: 294000, status: "pending", date: "2024-03-21", address: "101 Điện Biên Phủ, Bình Thạnh, TP.HCM", phone: "0934567890" },
  { id: "ORD-2024-005", customerId: 5, customerName: "Hoàng Văn Em", items: [{ book: BOOKS_INITIAL[19], quantity: 2 }, { book: BOOKS_INITIAL[20], quantity: 1 }], total: 308000, status: "delivered", date: "2024-03-10", address: "202 Nguyễn Trãi, Quận 5, TP.HCM", phone: "0945678901" },
  { id: "ORD-2024-006", customerId: 6, customerName: "Vũ Thị Phương", items: [{ book: BOOKS_INITIAL[5], quantity: 1 }], total: 285000, status: "cancelled", date: "2024-03-08", address: "303 CMT8, Quận 10, TP.HCM", phone: "0956789012" },
  { id: "ORD-2024-007", customerId: 7, customerName: "Đặng Quốc Giang", items: [{ book: BOOKS_INITIAL[21], quantity: 1 }, { book: BOOKS_INITIAL[17], quantity: 2 }], total: 445000, status: "shipping", date: "2024-03-19", address: "404 Nguyễn Văn Cừ, Quận 5, TP.HCM", phone: "0967890123" },
  { id: "ORD-2024-008", customerId: 1, customerName: "Nguyễn Văn An", items: [{ book: BOOKS_INITIAL[12], quantity: 1 }, { book: BOOKS_INITIAL[13], quantity: 1 }], total: 233000, status: "delivered", date: "2024-02-28", address: "123 Lê Lợi, Quận 1, TP.HCM", phone: "0901234567" },
  { id: "ORD-2024-009", customerId: 8, customerName: "Bùi Thị Hoa", items: [{ book: BOOKS_INITIAL[8], quantity: 2 }, { book: BOOKS_INITIAL[9], quantity: 1 }], total: 268000, status: "confirmed", date: "2024-03-22", address: "505 Hoàng Diệu, Quận 4, TP.HCM", phone: "0978901234" },
  { id: "ORD-2024-010", customerId: 9, customerName: "Lý Văn Thành", items: [{ book: BOOKS_INITIAL[14], quantity: 1 }], total: 79000, status: "pending", date: "2024-03-23", address: "606 Tô Hiến Thành, Quận 10, TP.HCM", phone: "0989012345" },
  { id: "ORD-2024-011", customerId: 2, customerName: "Trần Thị Bình", items: [{ book: BOOKS_INITIAL[3], quantity: 2 }], total: 144000, status: "delivered", date: "2024-02-14", address: "456 Nguyễn Huệ, Quận 1, TP.HCM", phone: "0912345678" },
  { id: "ORD-2024-012", customerId: 5, customerName: "Hoàng Văn Em", items: [{ book: BOOKS_INITIAL[10], quantity: 1 }, { book: BOOKS_INITIAL[11], quantity: 1 }], total: 213000, status: "shipping", date: "2024-03-24", address: "202 Nguyễn Trãi, Quận 5, TP.HCM", phone: "0945678901" },
];

const USERS_INITIAL: AppUser[] = [
  { id: 1, name: "Nguyễn Văn An", email: "nguyenvanan@gmail.com", phone: "0901234567", role: "customer", joinDate: "2023-01-15", totalOrders: 8, totalSpent: 1250000, active: true },
  { id: 2, name: "Trần Thị Bình", email: "tranthibinh@gmail.com", phone: "0912345678", role: "customer", joinDate: "2023-03-22", totalOrders: 5, totalSpent: 680000, active: true },
  { id: 3, name: "Lê Minh Cường", email: "leminhcuong@gmail.com", phone: "0923456789", role: "customer", joinDate: "2023-05-10", totalOrders: 3, totalSpent: 450000, active: true },
  { id: 4, name: "Phạm Thị Dung", email: "phamthidung@gmail.com", phone: "0934567890", role: "customer", joinDate: "2023-07-08", totalOrders: 6, totalSpent: 890000, active: false },
  { id: 5, name: "Hoàng Văn Em", email: "hoangvanem@gmail.com", phone: "0945678901", role: "customer", joinDate: "2023-09-14", totalOrders: 12, totalSpent: 2100000, active: true },
  { id: 6, name: "Vũ Thị Phương", email: "vuthiphuong@gmail.com", phone: "0956789012", role: "customer", joinDate: "2023-11-20", totalOrders: 2, totalSpent: 320000, active: true },
  { id: 7, name: "Đặng Quốc Giang", email: "dangquocgiang@gmail.com", phone: "0967890123", role: "customer", joinDate: "2024-01-05", totalOrders: 4, totalSpent: 560000, active: true },
  { id: 8, name: "Bùi Thị Hoa", email: "buithihoa@gmail.com", phone: "0978901234", role: "customer", joinDate: "2024-02-12", totalOrders: 7, totalSpent: 1050000, active: true },
  { id: 9, name: "Minh Tuấn (Nhân viên)", email: "minhtuan@bookverse.vn", phone: "0989012345", role: "staff", joinDate: "2023-06-01", totalOrders: 0, totalSpent: 0, active: true },
  { id: 10, name: "Admin Bookverse", email: "admin@bookverse.vn", phone: "0990123456", role: "admin", joinDate: "2023-01-01", totalOrders: 0, totalSpent: 0, active: true },
];

const REVENUE_DATA = [
  { month: "T1", revenue: 12500000, orders: 145 }, { month: "T2", revenue: 15800000, orders: 189 },
  { month: "T3", revenue: 13200000, orders: 162 }, { month: "T4", revenue: 18900000, orders: 234 },
  { month: "T5", revenue: 22100000, orders: 278 }, { month: "T6", revenue: 19600000, orders: 248 },
  { month: "T7", revenue: 25300000, orders: 312 }, { month: "T8", revenue: 28700000, orders: 356 },
  { month: "T9", revenue: 24100000, orders: 298 }, { month: "T10", revenue: 31200000, orders: 389 },
  { month: "T11", revenue: 38900000, orders: 478 }, { month: "T12", revenue: 45600000, orders: 567 },
];

const CAT_CHART = [
  { name: "Kỹ năng sống", value: 35, color: "#9B2018" },
  { name: "Văn học VN", value: 20, color: "#D4873A" },
  { name: "Thiếu nhi", value: 18, color: "#5C8A3C" },
  { name: "KD-Kinh tế", value: 15, color: "#2E6B9E" },
  { name: "Nước ngoài", value: 8, color: "#7B4E9E" },
  { name: "KH-CN", value: 4, color: "#C08040" },
];

// ─── HELPERS ──────────────────────────────────────────────────────────────────
const fmt = (n: number) =>
  new Intl.NumberFormat("vi-VN", { style: "currency", currency: "VND" }).format(n);

const fmtDate = (s: string) =>
  new Date(s).toLocaleDateString("vi-VN", { day: "2-digit", month: "2-digit", year: "numeric" });

const fmtM = (n: number) => {
  if (n >= 1000000) return `${(n / 1000000).toFixed(1)}M`;
  if (n >= 1000) return `${(n / 1000).toFixed(0)}K`;
  return n.toString();
};

const STATUS_LABEL: Record<OrderStatus, string> = {
  pending: "Chờ xác nhận", confirmed: "Đã xác nhận",
  shipping: "Đang giao hàng", delivered: "Đã giao", cancelled: "Đã hủy"
};

const STATUS_COLOR: Record<OrderStatus, string> = {
  pending: "bg-amber-100 text-amber-800 border-amber-200",
  confirmed: "bg-blue-100 text-blue-800 border-blue-200",
  shipping: "bg-purple-100 text-purple-800 border-purple-200",
  delivered: "bg-green-100 text-green-800 border-green-200",
  cancelled: "bg-red-100 text-red-800 border-red-200",
};

const NEXT_STATUS: Partial<Record<OrderStatus, OrderStatus>> = {
  pending: "confirmed", confirmed: "shipping", shipping: "delivered"
};

// ─── MICRO COMPONENTS ─────────────────────────────────────────────────────────
function Stars({ rating, size = "sm" }: { rating: number; size?: "sm" | "md" }) {
  const s = size === "sm" ? "w-3 h-3" : "w-4 h-4";
  return (
    <div className="flex gap-0.5">
      {[1, 2, 3, 4, 5].map(i => (
        <Star key={i} className={`${s} ${i <= Math.round(rating) ? "fill-amber-400 text-amber-400" : "text-gray-300 fill-gray-100"}`} />
      ))}
    </div>
  );
}

function Badge({ status }: { status: OrderStatus }) {
  return (
    <span className={`text-xs font-medium px-2.5 py-1 rounded-full border ${STATUS_COLOR[status]}`}>
      {STATUS_LABEL[status]}
    </span>
  );
}

function Disc({ pct }: { pct: number }) {
  return (
    <span className="bg-primary text-primary-foreground text-xs font-bold px-1.5 py-0.5 rounded">
      -{pct}%
    </span>
  );
}

// ─── BOOK CARD ────────────────────────────────────────────────────────────────
function BookCard({ book, onSelect, onAddToCart }: {
  book: Book;
  onSelect: (b: Book) => void;
  onAddToCart: (b: Book) => void;
}) {
  const disc = Math.round((1 - book.price / book.originalPrice) * 100);
  return (
    <div
      className="bg-card rounded-xl overflow-hidden border border-border group cursor-pointer hover:shadow-lg hover:-translate-y-0.5 transition-all duration-200"
      onClick={() => onSelect(book)}
    >
      <div className="relative overflow-hidden bg-secondary aspect-[3/4]">
        <img
          src={book.cover}
          alt={book.title}
          className="w-full h-full object-cover group-hover:scale-105 transition-transform duration-300"
        />
        {disc > 0 && (
          <div className="absolute top-2 left-2">
            <Disc pct={disc} />
          </div>
        )}
        {book.stock <= 10 && book.stock > 0 && (
          <div className="absolute top-2 right-2 bg-orange-500 text-white text-xs font-semibold px-2 py-0.5 rounded">
            Sắp hết
          </div>
        )}
        {book.stock === 0 && (
          <div className="absolute inset-0 bg-black/50 flex items-center justify-center">
            <span className="text-white font-semibold text-sm">Hết hàng</span>
          </div>
        )}
        <button
          onClick={e => { e.stopPropagation(); onAddToCart(book); }}
          disabled={book.stock === 0}
          className="absolute bottom-2 left-2 right-2 bg-primary text-primary-foreground text-sm font-medium py-2 rounded-lg opacity-0 group-hover:opacity-100 transition-opacity disabled:opacity-50 disabled:cursor-not-allowed hover:bg-primary/90 active:scale-95"
        >
          Thêm vào giỏ
        </button>
      </div>
      <div className="p-3">
        <p className="text-xs text-muted-foreground mb-0.5 truncate">{book.author}</p>
        <h3 className="font-semibold text-sm leading-snug line-clamp-2 mb-2 group-hover:text-primary transition-colors" style={{ fontFamily: "'Playfair Display', serif" }}>
          {book.title}
        </h3>
        <div className="flex items-center gap-1 mb-2">
          <Stars rating={book.rating} />
          <span className="text-xs text-muted-foreground">({book.reviews})</span>
        </div>
        <div className="flex items-center gap-2">
          <span className="font-bold text-primary text-sm">{fmt(book.price)}</span>
          {disc > 0 && (
            <span className="text-xs text-muted-foreground line-through">{fmt(book.originalPrice)}</span>
          )}
        </div>
      </div>
    </div>
  );
}

// ─── BOOK DETAIL MODAL ────────────────────────────────────────────────────────
function BookDetailModal({ book, onClose, onAddToCart }: {
  book: Book;
  onClose: () => void;
  onAddToCart: (b: Book, qty: number) => void;
}) {
  const [qty, setQty] = useState(1);
  const disc = Math.round((1 - book.price / book.originalPrice) * 100);

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/50 backdrop-blur-sm" onClick={onClose}>
      <div
        className="bg-card rounded-2xl shadow-2xl max-w-3xl w-full max-h-[90vh] overflow-y-auto"
        onClick={e => e.stopPropagation()}
      >
        <div className="flex gap-8 p-8">
          <div className="w-52 flex-shrink-0">
            <div className="rounded-xl overflow-hidden shadow-lg bg-secondary aspect-[3/4]">
              <img src={book.cover} alt={book.title} className="w-full h-full object-cover" />
            </div>
          </div>
          <div className="flex-1 min-w-0">
            <button onClick={onClose} className="float-right p-1 hover:bg-muted rounded-lg transition-colors">
              <X className="w-5 h-5" />
            </button>
            <p className="text-sm text-muted-foreground mb-1">{book.category}</p>
            <h2 className="text-2xl font-bold mb-1 leading-tight" style={{ fontFamily: "'Playfair Display', serif" }}>
              {book.title}
            </h2>
            <p className="text-muted-foreground mb-3">bởi <span className="text-foreground font-medium">{book.author}</span></p>

            <div className="flex items-center gap-2 mb-4">
              <Stars rating={book.rating} size="md" />
              <span className="text-sm text-muted-foreground">{book.rating} ({book.reviews.toLocaleString("vi-VN")} đánh giá)</span>
              <span className="text-sm text-muted-foreground">• {book.sold.toLocaleString("vi-VN")} đã bán</span>
            </div>

            <div className="flex items-baseline gap-3 mb-4">
              <span className="text-3xl font-bold text-primary">{fmt(book.price)}</span>
              {disc > 0 && (
                <>
                  <span className="text-lg text-muted-foreground line-through">{fmt(book.originalPrice)}</span>
                  <Disc pct={disc} />
                </>
              )}
            </div>

            <p className="text-sm text-foreground/80 leading-relaxed mb-5">{book.description}</p>

            <div className="grid grid-cols-2 gap-2 text-sm mb-5 bg-secondary/50 rounded-xl p-4">
              <div><span className="text-muted-foreground">NXB:</span> <span className="font-medium">{book.publisher}</span></div>
              <div><span className="text-muted-foreground">Năm:</span> <span className="font-medium">{book.year}</span></div>
              <div><span className="text-muted-foreground">Trang:</span> <span className="font-medium">{book.pages}</span></div>
              <div><span className="text-muted-foreground">ISBN:</span> <span className="font-medium">{book.isbn}</span></div>
              <div><span className="text-muted-foreground">Kho:</span> <span className={`font-medium ${book.stock <= 10 ? "text-orange-600" : "text-green-600"}`}>{book.stock} cuốn</span></div>
            </div>

            {book.stock > 0 ? (
              <div className="flex items-center gap-3">
                <div className="flex items-center border border-border rounded-lg overflow-hidden">
                  <button onClick={() => setQty(q => Math.max(1, q - 1))} className="px-3 py-2 hover:bg-muted transition-colors">
                    <Minus className="w-4 h-4" />
                  </button>
                  <span className="px-4 py-2 font-medium border-x border-border min-w-[3rem] text-center">{qty}</span>
                  <button onClick={() => setQty(q => Math.min(book.stock, q + 1))} className="px-3 py-2 hover:bg-muted transition-colors">
                    <Plus className="w-4 h-4" />
                  </button>
                </div>
                <button
                  onClick={() => { onAddToCart(book, qty); onClose(); }}
                  className="flex-1 bg-primary text-primary-foreground font-semibold py-2.5 px-6 rounded-lg hover:bg-primary/90 transition-colors flex items-center justify-center gap-2"
                >
                  <ShoppingCart className="w-4 h-4" />
                  Thêm vào giỏ — {fmt(book.price * qty)}
                </button>
              </div>
            ) : (
              <div className="text-center py-3 bg-red-50 rounded-lg text-red-600 font-medium">Tạm hết hàng</div>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}

// ─── CART DRAWER ──────────────────────────────────────────────────────────────
function CartDrawer({ items, onClose, onUpdate, onRemove, onCheckout }: {
  items: CartItem[];
  onClose: () => void;
  onUpdate: (id: number, qty: number) => void;
  onRemove: (id: number) => void;
  onCheckout: () => void;
}) {
  const total = items.reduce((s, i) => s + i.book.price * i.quantity, 0);
  return (
    <div className="fixed inset-0 z-50 flex justify-end">
      <div className="absolute inset-0 bg-black/40 backdrop-blur-sm" onClick={onClose} />
      <div className="relative w-full max-w-md bg-card shadow-2xl flex flex-col h-full">
        <div className="flex items-center justify-between p-5 border-b border-border">
          <div className="flex items-center gap-2">
            <ShoppingCart className="w-5 h-5 text-primary" />
            <h2 className="font-bold text-lg" style={{ fontFamily: "'Playfair Display', serif" }}>
              Giỏ hàng ({items.length} sản phẩm)
            </h2>
          </div>
          <button onClick={onClose} className="p-1.5 hover:bg-muted rounded-lg transition-colors">
            <X className="w-5 h-5" />
          </button>
        </div>

        <div className="flex-1 overflow-y-auto p-5 space-y-4">
          {items.length === 0 ? (
            <div className="text-center py-16">
              <ShoppingBag className="w-14 h-14 text-muted-foreground mx-auto mb-3" />
              <p className="text-muted-foreground font-medium">Giỏ hàng trống</p>
              <p className="text-sm text-muted-foreground mt-1">Hãy thêm sách bạn yêu thích!</p>
            </div>
          ) : items.map(item => (
            <div key={item.book.id} className="flex gap-3 bg-secondary/40 rounded-xl p-3">
              <div className="w-14 h-18 rounded-lg overflow-hidden flex-shrink-0 bg-muted aspect-[3/4]">
                <img src={item.book.cover} alt={item.book.title} className="w-full h-full object-cover" />
              </div>
              <div className="flex-1 min-w-0">
                <h4 className="font-medium text-sm line-clamp-2 leading-snug mb-1" style={{ fontFamily: "'Playfair Display', serif" }}>
                  {item.book.title}
                </h4>
                <p className="text-xs text-muted-foreground mb-2">{item.book.author}</p>
                <div className="flex items-center justify-between">
                  <div className="flex items-center border border-border rounded-lg bg-card overflow-hidden">
                    <button onClick={() => item.quantity > 1 ? onUpdate(item.book.id, item.quantity - 1) : onRemove(item.book.id)} className="px-2 py-1 hover:bg-muted transition-colors">
                      <Minus className="w-3 h-3" />
                    </button>
                    <span className="px-2 py-1 text-sm font-medium">{item.quantity}</span>
                    <button onClick={() => onUpdate(item.book.id, item.quantity + 1)} className="px-2 py-1 hover:bg-muted transition-colors">
                      <Plus className="w-3 h-3" />
                    </button>
                  </div>
                  <div className="flex items-center gap-2">
                    <span className="font-bold text-primary text-sm">{fmt(item.book.price * item.quantity)}</span>
                    <button onClick={() => onRemove(item.book.id)} className="p-1 text-red-400 hover:text-red-600 hover:bg-red-50 rounded transition-colors">
                      <Trash2 className="w-3.5 h-3.5" />
                    </button>
                  </div>
                </div>
              </div>
            </div>
          ))}
        </div>

        {items.length > 0 && (
          <div className="p-5 border-t border-border space-y-4">
            <div className="flex justify-between text-sm text-muted-foreground">
              <span>Tạm tính ({items.reduce((s, i) => s + i.quantity, 0)} sản phẩm)</span>
              <span>{fmt(total)}</span>
            </div>
            <div className="flex justify-between font-bold text-lg">
              <span>Tổng cộng</span>
              <span className="text-primary">{fmt(total)}</span>
            </div>
            <button
              onClick={() => { onCheckout(); onClose(); }}
              className="w-full bg-primary text-primary-foreground font-semibold py-3 rounded-xl hover:bg-primary/90 transition-colors flex items-center justify-center gap-2"
            >
              <CheckCircle className="w-5 h-5" />
              Đặt hàng ngay
            </button>
          </div>
        )}
      </div>
    </div>
  );
}

// ─── CUSTOMER NAVBAR ──────────────────────────────────────────────────────────
function CustomerNav({ cartCount, view, onView, onCartOpen, searchQ, onSearch }: {
  cartCount: number; view: string; onView: (v: string) => void;
  onCartOpen: () => void; searchQ: string; onSearch: (q: string) => void;
}) {
  const [menuOpen, setMenuOpen] = useState(false);
  return (
    <header className="bg-card border-b border-border sticky top-0 z-40 shadow-sm">
      <div className="max-w-7xl mx-auto px-4 sm:px-6">
        <div className="flex items-center gap-4 h-16">
          <button onClick={() => onView("home")} className="flex items-center gap-2 flex-shrink-0">
            <div className="w-8 h-8 bg-primary rounded-lg flex items-center justify-center">
              <BookOpen className="w-4.5 h-4.5 text-white w-5 h-5" />
            </div>
            <span className="font-bold text-lg text-foreground hidden sm:block" style={{ fontFamily: "'Playfair Display', serif" }}>
              Bookverse
            </span>
          </button>

          <div className="flex-1 max-w-xl relative hidden sm:block">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-muted-foreground" />
            <input
              type="text"
              placeholder="Tìm kiếm sách, tác giả..."
              value={searchQ}
              onChange={e => { onSearch(e.target.value); if (e.target.value) onView("catalog"); }}
              className="w-full pl-10 pr-4 py-2 bg-secondary/60 border border-border rounded-full text-sm focus:outline-none focus:ring-2 focus:ring-primary/30 focus:border-primary/50 transition-all"
            />
          </div>

          <nav className="hidden md:flex items-center gap-1 ml-auto">
            {[["home", "Trang chủ"], ["catalog", "Sách"], ["orders", "Đơn hàng"], ["profile", "Tài khoản"]].map(([v, l]) => (
              <button
                key={v}
                onClick={() => onView(v)}
                className={`px-3 py-2 text-sm font-medium rounded-lg transition-colors ${view === v ? "bg-primary/10 text-primary" : "text-foreground hover:bg-secondary"}`}
              >
                {l}
              </button>
            ))}
          </nav>

          <button
            onClick={onCartOpen}
            className="relative p-2 hover:bg-secondary rounded-lg transition-colors ml-2"
          >
            <ShoppingCart className="w-5 h-5" />
            {cartCount > 0 && (
              <span className="absolute -top-1 -right-1 w-5 h-5 bg-primary text-primary-foreground text-xs font-bold rounded-full flex items-center justify-center">
                {cartCount > 9 ? "9+" : cartCount}
              </span>
            )}
          </button>

          <button onClick={() => setMenuOpen(!menuOpen)} className="md:hidden p-2 hover:bg-secondary rounded-lg transition-colors">
            {menuOpen ? <X className="w-5 h-5" /> : <Menu className="w-5 h-5" />}
          </button>
        </div>

        <div className="sm:hidden pb-3">
          <div className="relative">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-muted-foreground" />
            <input
              type="text"
              placeholder="Tìm kiếm sách..."
              value={searchQ}
              onChange={e => { onSearch(e.target.value); if (e.target.value) onView("catalog"); }}
              className="w-full pl-10 pr-4 py-2 bg-secondary/60 border border-border rounded-full text-sm focus:outline-none"
            />
          </div>
        </div>

        {menuOpen && (
          <div className="md:hidden pb-3 border-t border-border pt-3 space-y-1">
            {[["home", "Trang chủ"], ["catalog", "Danh mục sách"], ["orders", "Đơn hàng của tôi"], ["profile", "Tài khoản"]].map(([v, l]) => (
              <button
                key={v}
                onClick={() => { onView(v); setMenuOpen(false); }}
                className={`w-full text-left px-3 py-2 text-sm rounded-lg transition-colors ${view === v ? "bg-primary/10 text-primary font-medium" : "hover:bg-secondary"}`}
              >
                {l}
              </button>
            ))}
          </div>
        )}
      </div>
    </header>
  );
}

// ─── HOME PAGE ────────────────────────────────────────────────────────────────
function HomePage({ books, onView, onSelect, onAddToCart }: {
  books: Book[]; onView: (v: string) => void;
  onSelect: (b: Book) => void; onAddToCart: (b: Book) => void;
}) {
  const featured = books.filter(b => b.sold > 1000).slice(0, 4);
  const bestsellers = [...books].sort((a, b) => b.sold - a.sold).slice(0, 8);
  const newArrivals = [...books].sort((a, b) => b.year - a.year).slice(0, 8);

  return (
    <main className="min-h-screen">
      {/* Hero */}
      <section className="relative overflow-hidden bg-gradient-to-br from-[#2A1A12] via-[#3D2518] to-[#1C1410] text-white">
        <div className="absolute inset-0 opacity-10">
          <div className="absolute top-10 left-20 w-64 h-64 rounded-full bg-accent blur-3xl" />
          <div className="absolute bottom-10 right-20 w-96 h-96 rounded-full bg-primary blur-3xl" />
        </div>
        <div className="relative max-w-7xl mx-auto px-6 py-20 md:py-28 flex flex-col md:flex-row items-center gap-10">
          <div className="flex-1">
            <div className="inline-flex items-center gap-2 bg-white/10 border border-white/20 rounded-full px-4 py-1.5 text-sm mb-6">
              <span className="w-2 h-2 bg-accent rounded-full animate-pulse" />
              Khuyến mãi đến 30% — Hôm nay
            </div>
            <h1 className="text-4xl md:text-6xl font-bold leading-tight mb-4" style={{ fontFamily: "'Playfair Display', serif" }}>
              Thế giới tri thức<br />
              <span className="text-accent italic">trong tầm tay bạn</span>
            </h1>
            <p className="text-white/70 text-lg mb-8 max-w-md leading-relaxed">
              Hơn 10.000 đầu sách chất lượng — từ văn học kinh điển đến sách kỹ năng hiện đại. Giao hàng toàn quốc trong 24 giờ.
            </p>
            <div className="flex gap-3 flex-wrap">
              <button onClick={() => onView("catalog")} className="bg-accent hover:bg-accent/90 text-white font-semibold px-7 py-3 rounded-xl transition-colors flex items-center gap-2">
                Khám phá ngay <ChevronRight className="w-4 h-4" />
              </button>
              <button className="border border-white/30 hover:bg-white/10 text-white font-medium px-7 py-3 rounded-xl transition-colors">
                Ưu đãi hôm nay
              </button>
            </div>
          </div>
          <div className="flex-shrink-0 flex gap-3">
            {featured.slice(0, 3).map((b, i) => (
              <div
                key={b.id}
                onClick={() => onSelect(b)}
                className={`cursor-pointer rounded-xl overflow-hidden shadow-2xl transition-transform hover:-translate-y-2 duration-200 ${i === 1 ? "mt-8" : ""} ${i === 2 ? "hidden lg:block" : ""}`}
                style={{ width: 110 }}
              >
                <img src={b.cover} alt={b.title} className="w-full aspect-[3/4] object-cover" />
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* Category pills */}
      <section className="bg-card border-b border-border">
        <div className="max-w-7xl mx-auto px-6 py-4">
          <div className="flex gap-2 overflow-x-auto pb-1 scrollbar-hide">
            <button
              onClick={() => onView("catalog")}
              className="flex-shrink-0 flex items-center gap-1.5 px-4 py-2 bg-primary text-primary-foreground text-sm font-medium rounded-full transition-colors"
            >
              <Grid3X3 className="w-3.5 h-3.5" /> Tất cả
            </button>
            {CATEGORIES.map(cat => (
              <button
                key={cat}
                onClick={() => onView("catalog")}
                className="flex-shrink-0 flex items-center gap-1.5 px-4 py-2 bg-secondary hover:bg-primary hover:text-primary-foreground text-sm font-medium rounded-full transition-colors whitespace-nowrap"
              >
                {cat}
              </button>
            ))}
          </div>
        </div>
      </section>

      <div className="max-w-7xl mx-auto px-6 py-10 space-y-14">
        {/* Bestsellers */}
        <section>
          <div className="flex items-center justify-between mb-6">
            <div>
              <h2 className="text-2xl font-bold" style={{ fontFamily: "'Playfair Display', serif" }}>Sách bán chạy nhất</h2>
              <p className="text-muted-foreground text-sm mt-0.5">Được độc giả yêu thích nhất tháng này</p>
            </div>
            <button onClick={() => onView("catalog")} className="text-primary text-sm font-medium hover:underline flex items-center gap-1">
              Xem tất cả <ChevronRight className="w-4 h-4" />
            </button>
          </div>
          <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5 xl:grid-cols-6 gap-4">
            {bestsellers.slice(0, 6).map(b => (
              <BookCard key={b.id} book={b} onSelect={onSelect} onAddToCart={onAddToCart} />
            ))}
          </div>
        </section>

        {/* Stats banner */}
        <section className="bg-gradient-to-r from-primary to-primary/80 rounded-2xl p-8 text-white">
          <div className="grid grid-cols-2 md:grid-cols-4 gap-6 text-center">
            {[
              ["10.000+", "Đầu sách"],
              ["50.000+", "Khách hàng"],
              ["24h", "Giao hàng"],
              ["4.8★", "Đánh giá trung bình"],
            ].map(([n, l]) => (
              <div key={l}>
                <div className="text-3xl font-bold mb-1" style={{ fontFamily: "'Playfair Display', serif" }}>{n}</div>
                <div className="text-white/70 text-sm">{l}</div>
              </div>
            ))}
          </div>
        </section>

        {/* New arrivals */}
        <section>
          <div className="flex items-center justify-between mb-6">
            <div>
              <h2 className="text-2xl font-bold" style={{ fontFamily: "'Playfair Display', serif" }}>Sách mới nhất</h2>
              <p className="text-muted-foreground text-sm mt-0.5">Cập nhật liên tục từ các nhà xuất bản uy tín</p>
            </div>
            <button onClick={() => onView("catalog")} className="text-primary text-sm font-medium hover:underline flex items-center gap-1">
              Xem tất cả <ChevronRight className="w-4 h-4" />
            </button>
          </div>
          <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5 xl:grid-cols-6 gap-4">
            {newArrivals.slice(0, 6).map(b => (
              <BookCard key={b.id} book={b} onSelect={onSelect} onAddToCart={onAddToCart} />
            ))}
          </div>
        </section>
      </div>

      <footer className="bg-[#2A1A12] text-white/70 py-12 mt-10">
        <div className="max-w-7xl mx-auto px-6 grid grid-cols-2 md:grid-cols-4 gap-8">
          <div>
            <div className="flex items-center gap-2 mb-4">
              <div className="w-7 h-7 bg-primary rounded-lg flex items-center justify-center">
                <BookOpen className="w-4 h-4 text-white" />
              </div>
              <span className="text-white font-bold text-lg" style={{ fontFamily: "'Playfair Display', serif" }}>Bookverse</span>
            </div>
            <p className="text-sm leading-relaxed">Nền tảng bán sách trực tuyến uy tín hàng đầu Việt Nam.</p>
          </div>
          {[
            ["Danh mục", CATEGORIES.slice(0, 4)],
            ["Hỗ trợ", ["Chính sách đổi trả", "Hướng dẫn đặt hàng", "Theo dõi đơn hàng", "Liên hệ"]],
            ["Kết nối", ["Facebook", "Instagram", "Zalo: 0901234567", "Email: hello@bookverse.vn"]],
          ].map(([title, items]) => (
            <div key={title as string}>
              <h4 className="text-white font-semibold mb-3 text-sm">{title as string}</h4>
              <ul className="space-y-2">
                {(items as string[]).map(item => (
                  <li key={item} className="text-sm hover:text-white cursor-pointer transition-colors">{item}</li>
                ))}
              </ul>
            </div>
          ))}
        </div>
        <div className="max-w-7xl mx-auto px-6 mt-8 pt-6 border-t border-white/10 text-center text-xs">
          © 2024 Bookverse. Tất cả quyền được bảo lưu.
        </div>
      </footer>
    </main>
  );
}

// ─── CATALOG PAGE ─────────────────────────────────────────────────────────────
function CatalogPage({ books, searchQ, onSelect, onAddToCart }: {
  books: Book[]; searchQ: string;
  onSelect: (b: Book) => void; onAddToCart: (b: Book) => void;
}) {
  const [category, setCategory] = useState("Tất cả");
  const [sort, setSort] = useState("popular");
  const [priceMin, setPriceMin] = useState("");
  const [priceMax, setPriceMax] = useState("");
  const [filterOpen, setFilterOpen] = useState(false);

  const filtered = useMemo(() => {
    let arr = [...books];
    if (searchQ) arr = arr.filter(b => b.title.toLowerCase().includes(searchQ.toLowerCase()) || b.author.toLowerCase().includes(searchQ.toLowerCase()));
    if (category !== "Tất cả") arr = arr.filter(b => b.category === category);
    if (priceMin) arr = arr.filter(b => b.price >= parseInt(priceMin));
    if (priceMax) arr = arr.filter(b => b.price <= parseInt(priceMax));
    switch (sort) {
      case "popular": return arr.sort((a, b) => b.sold - a.sold);
      case "rating": return arr.sort((a, b) => b.rating - a.rating);
      case "priceLow": return arr.sort((a, b) => a.price - b.price);
      case "priceHigh": return arr.sort((a, b) => b.price - a.price);
      case "newest": return arr.sort((a, b) => b.year - a.year);
      default: return arr;
    }
  }, [books, searchQ, category, sort, priceMin, priceMax]);

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 py-8 min-h-screen">
      <div className="flex flex-col md:flex-row gap-6">
        {/* Sidebar Filter */}
        <aside className={`w-full md:w-56 flex-shrink-0 ${filterOpen ? "block" : "hidden md:block"}`}>
          <div className="bg-card rounded-xl border border-border p-5 sticky top-24">
            <h3 className="font-bold text-base mb-4 flex items-center gap-2">
              <Filter className="w-4 h-4 text-primary" /> Bộ lọc
            </h3>

            <div className="mb-5">
              <p className="text-sm font-semibold mb-2 text-muted-foreground uppercase tracking-wide text-xs">Danh mục</p>
              <div className="space-y-1">
                {["Tất cả", ...CATEGORIES].map(cat => (
                  <button
                    key={cat}
                    onClick={() => setCategory(cat)}
                    className={`w-full text-left text-sm px-3 py-2 rounded-lg transition-colors ${category === cat ? "bg-primary/10 text-primary font-medium" : "hover:bg-secondary"}`}
                  >
                    {cat}
                    {cat !== "Tất cả" && (
                      <span className="float-right text-xs text-muted-foreground">
                        {books.filter(b => b.category === cat).length}
                      </span>
                    )}
                  </button>
                ))}
              </div>
            </div>

            <div className="mb-5">
              <p className="text-sm font-semibold mb-2 text-muted-foreground uppercase tracking-wide text-xs">Khoảng giá (VNĐ)</p>
              <div className="space-y-2">
                <input type="number" placeholder="Từ" value={priceMin} onChange={e => setPriceMin(e.target.value)} className="w-full px-3 py-2 text-sm border border-border rounded-lg bg-secondary/50 focus:outline-none focus:border-primary/50" />
                <input type="number" placeholder="Đến" value={priceMax} onChange={e => setPriceMax(e.target.value)} className="w-full px-3 py-2 text-sm border border-border rounded-lg bg-secondary/50 focus:outline-none focus:border-primary/50" />
              </div>
            </div>

            <button
              onClick={() => { setCategory("Tất cả"); setPriceMin(""); setPriceMax(""); }}
              className="w-full text-sm text-primary hover:bg-primary/10 py-2 rounded-lg transition-colors font-medium"
            >
              Xóa bộ lọc
            </button>
          </div>
        </aside>

        {/* Main content */}
        <div className="flex-1 min-w-0">
          <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3 mb-6">
            <div>
              <h2 className="text-xl font-bold" style={{ fontFamily: "'Playfair Display', serif" }}>
                {searchQ ? `Kết quả tìm kiếm: "${searchQ}"` : category !== "Tất cả" ? category : "Tất cả sách"}
              </h2>
              <p className="text-sm text-muted-foreground mt-0.5">Tìm thấy {filtered.length} cuốn sách</p>
            </div>
            <div className="flex items-center gap-2">
              <button
                onClick={() => setFilterOpen(!filterOpen)}
                className="md:hidden flex items-center gap-2 text-sm border border-border rounded-lg px-3 py-2 hover:bg-secondary"
              >
                <Filter className="w-4 h-4" /> Lọc
              </button>
              <select
                value={sort}
                onChange={e => setSort(e.target.value)}
                className="text-sm border border-border rounded-lg px-3 py-2 bg-card focus:outline-none focus:border-primary/50"
              >
                <option value="popular">Phổ biến nhất</option>
                <option value="rating">Đánh giá cao</option>
                <option value="priceLow">Giá: Thấp → Cao</option>
                <option value="priceHigh">Giá: Cao → Thấp</option>
                <option value="newest">Mới nhất</option>
              </select>
            </div>
          </div>

          {filtered.length === 0 ? (
            <div className="text-center py-20">
              <BookOpen className="w-14 h-14 text-muted-foreground mx-auto mb-4" />
              <p className="text-muted-foreground font-medium text-lg">Không tìm thấy sách phù hợp</p>
              <p className="text-sm text-muted-foreground mt-1">Hãy thử tìm kiếm với từ khóa khác</p>
            </div>
          ) : (
            <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-4 xl:grid-cols-5 gap-4">
              {filtered.map(b => (
                <BookCard key={b.id} book={b} onSelect={onSelect} onAddToCart={onAddToCart} />
              ))}
            </div>
          )}
        </div>
      </div>
    </div>
  );
}

// ─── CHECKOUT PAGE ────────────────────────────────────────────────────────────
function CheckoutPage({ cart, onPlaceOrder, onBack }: {
  cart: CartItem[]; onPlaceOrder: (name: string, phone: string, address: string) => void; onBack: () => void;
}) {
  const [form, setForm] = useState({ name: "Nguyễn Văn An", phone: "0901234567", address: "123 Lê Lợi, Quận 1, TP.HCM", note: "" });
  const [payment, setPayment] = useState("cod");
  const total = cart.reduce((s, i) => s + i.book.price * i.quantity, 0);
  const ship = total >= 300000 ? 0 : 25000;

  return (
    <div className="max-w-5xl mx-auto px-4 sm:px-6 py-8 min-h-screen">
      <button onClick={onBack} className="flex items-center gap-2 text-muted-foreground hover:text-foreground mb-6 transition-colors text-sm">
        <ArrowLeft className="w-4 h-4" /> Quay lại giỏ hàng
      </button>
      <h1 className="text-2xl font-bold mb-8" style={{ fontFamily: "'Playfair Display', serif" }}>Đặt hàng</h1>

      <div className="grid md:grid-cols-5 gap-8">
        <div className="md:col-span-3 space-y-5">
          <div className="bg-card rounded-xl border border-border p-6">
            <h2 className="font-bold text-base mb-4 flex items-center gap-2">
              <User className="w-4 h-4 text-primary" /> Thông tin giao hàng
            </h2>
            <div className="space-y-4">
              {[
                ["Họ và tên *", "name", "text", "Nguyễn Văn A"],
                ["Số điện thoại *", "phone", "tel", "0901234567"],
                ["Địa chỉ giao hàng *", "address", "text", "Số nhà, đường, phường/xã, quận/huyện, tỉnh/TP"],
              ].map(([label, key, type, ph]) => (
                <div key={key as string}>
                  <label className="block text-sm font-medium mb-1.5">{label as string}</label>
                  <input
                    type={type as string}
                    placeholder={ph as string}
                    value={form[key as keyof typeof form]}
                    onChange={e => setForm(f => ({ ...f, [key as string]: e.target.value }))}
                    className="w-full px-4 py-2.5 border border-border rounded-xl bg-secondary/30 focus:outline-none focus:ring-2 focus:ring-primary/30 focus:border-primary/50 text-sm transition-all"
                  />
                </div>
              ))}
              <div>
                <label className="block text-sm font-medium mb-1.5">Ghi chú (không bắt buộc)</label>
                <textarea
                  rows={3}
                  placeholder="Ghi chú cho đơn hàng..."
                  value={form.note}
                  onChange={e => setForm(f => ({ ...f, note: e.target.value }))}
                  className="w-full px-4 py-2.5 border border-border rounded-xl bg-secondary/30 focus:outline-none focus:ring-2 focus:ring-primary/30 text-sm resize-none"
                />
              </div>
            </div>
          </div>

          <div className="bg-card rounded-xl border border-border p-6">
            <h2 className="font-bold text-base mb-4 flex items-center gap-2">
              <DollarSign className="w-4 h-4 text-primary" /> Phương thức thanh toán
            </h2>
            <div className="space-y-3">
              {[
                ["cod", "Thanh toán khi nhận hàng (COD)", "Trả tiền mặt khi nhận hàng"],
                ["transfer", "Chuyển khoản ngân hàng", "MB Bank: 0901234567 — Bookverse"],
                ["momo", "Ví MoMo", "Thanh toán nhanh qua QR MoMo"],
              ].map(([val, label, desc]) => (
                <label key={val as string} className={`flex items-start gap-3 p-4 rounded-xl border-2 cursor-pointer transition-all ${payment === val ? "border-primary bg-primary/5" : "border-border hover:border-primary/30"}`}>
                  <input type="radio" name="payment" value={val as string} checked={payment === val} onChange={() => setPayment(val as string)} className="mt-0.5 accent-primary" />
                  <div>
                    <p className="font-medium text-sm">{label as string}</p>
                    <p className="text-xs text-muted-foreground mt-0.5">{desc as string}</p>
                  </div>
                </label>
              ))}
            </div>
          </div>
        </div>

        <div className="md:col-span-2">
          <div className="bg-card rounded-xl border border-border p-6 sticky top-24">
            <h2 className="font-bold text-base mb-4">Đơn hàng ({cart.length} sản phẩm)</h2>
            <div className="space-y-3 max-h-64 overflow-y-auto mb-4">
              {cart.map(item => (
                <div key={item.book.id} className="flex gap-3 text-sm">
                  <div className="w-10 h-14 rounded overflow-hidden flex-shrink-0 bg-secondary">
                    <img src={item.book.cover} alt={item.book.title} className="w-full h-full object-cover" />
                  </div>
                  <div className="flex-1 min-w-0">
                    <p className="font-medium line-clamp-2 leading-snug text-xs">{item.book.title}</p>
                    <p className="text-muted-foreground text-xs">SL: {item.quantity}</p>
                  </div>
                  <span className="font-semibold text-primary flex-shrink-0 text-xs">{fmt(item.book.price * item.quantity)}</span>
                </div>
              ))}
            </div>

            <div className="border-t border-border pt-4 space-y-2">
              <div className="flex justify-between text-sm">
                <span className="text-muted-foreground">Tạm tính</span>
                <span>{fmt(total)}</span>
              </div>
              <div className="flex justify-between text-sm">
                <span className="text-muted-foreground">Phí giao hàng</span>
                <span className={ship === 0 ? "text-green-600 font-medium" : ""}>{ship === 0 ? "Miễn phí" : fmt(ship)}</span>
              </div>
              {ship === 0 && <p className="text-xs text-green-600">✓ Miễn phí giao cho đơn từ 300.000đ</p>}
              <div className="flex justify-between font-bold text-base pt-2 border-t border-border">
                <span>Tổng thanh toán</span>
                <span className="text-primary">{fmt(total + ship)}</span>
              </div>
            </div>

            <button
              onClick={() => { if (form.name && form.phone && form.address) onPlaceOrder(form.name, form.phone, form.address); }}
              disabled={!form.name || !form.phone || !form.address}
              className="w-full mt-5 bg-primary text-primary-foreground font-bold py-3.5 rounded-xl hover:bg-primary/90 transition-colors disabled:opacity-50 disabled:cursor-not-allowed flex items-center justify-center gap-2"
            >
              <Check className="w-5 h-5" /> Xác nhận đặt hàng
            </button>
            <p className="text-xs text-muted-foreground text-center mt-3">
              Bằng cách đặt hàng, bạn đồng ý với điều khoản dịch vụ của chúng tôi.
            </p>
          </div>
        </div>
      </div>
    </div>
  );
}

// ─── ORDERS PAGE ──────────────────────────────────────────────────────────────
function OrdersPage({ orders, customerId = 1 }: { orders: Order[]; customerId?: number }) {
  const [tab, setTab] = useState<"all" | OrderStatus>("all");
  const myOrders = orders.filter(o => o.customerId === customerId);
  const filtered = tab === "all" ? myOrders : myOrders.filter(o => o.status === tab);
  const [expanded, setExpanded] = useState<string | null>(null);

  return (
    <div className="max-w-3xl mx-auto px-4 sm:px-6 py-8 min-h-screen">
      <h1 className="text-2xl font-bold mb-6" style={{ fontFamily: "'Playfair Display', serif" }}>Đơn hàng của tôi</h1>

      <div className="flex gap-2 overflow-x-auto pb-2 mb-6">
        {[["all", "Tất cả"], ["pending", "Chờ xác nhận"], ["confirmed", "Đã xác nhận"], ["shipping", "Đang giao"], ["delivered", "Đã nhận"], ["cancelled", "Đã hủy"]].map(([v, l]) => (
          <button
            key={v}
            onClick={() => setTab(v as "all" | OrderStatus)}
            className={`flex-shrink-0 px-4 py-2 text-sm rounded-full font-medium transition-colors ${tab === v ? "bg-primary text-primary-foreground" : "bg-secondary hover:bg-muted"}`}
          >
            {l}
          </button>
        ))}
      </div>

      {filtered.length === 0 ? (
        <div className="text-center py-20">
          <FileText className="w-14 h-14 text-muted-foreground mx-auto mb-4" />
          <p className="text-muted-foreground font-medium">Chưa có đơn hàng nào</p>
        </div>
      ) : (
        <div className="space-y-4">
          {filtered.map(order => (
            <div key={order.id} className="bg-card rounded-xl border border-border overflow-hidden">
              <div
                className="flex flex-col sm:flex-row sm:items-center justify-between p-5 gap-3 cursor-pointer hover:bg-secondary/20 transition-colors"
                onClick={() => setExpanded(expanded === order.id ? null : order.id)}
              >
                <div>
                  <div className="flex items-center gap-3 mb-1.5">
                    <span className="font-bold text-sm font-mono">{order.id}</span>
                    <Badge status={order.status} />
                  </div>
                  <p className="text-sm text-muted-foreground">{fmtDate(order.date)} · {order.items.length} sản phẩm</p>
                </div>
                <div className="flex items-center gap-3">
                  <span className="font-bold text-primary">{fmt(order.total)}</span>
                  <ChevronDown className={`w-4 h-4 text-muted-foreground transition-transform ${expanded === order.id ? "rotate-180" : ""}`} />
                </div>
              </div>

              {expanded === order.id && (
                <div className="border-t border-border px-5 pb-5 pt-4">
                  <div className="space-y-3 mb-4">
                    {order.items.map(item => (
                      <div key={item.book.id} className="flex gap-3 text-sm">
                        <div className="w-10 h-14 rounded overflow-hidden flex-shrink-0 bg-secondary">
                          <img src={item.book.cover} alt={item.book.title} className="w-full h-full object-cover" />
                        </div>
                        <div className="flex-1 min-w-0">
                          <p className="font-medium line-clamp-1">{item.book.title}</p>
                          <p className="text-muted-foreground text-xs">{item.book.author} · SL: {item.quantity}</p>
                        </div>
                        <span className="font-semibold text-primary flex-shrink-0">{fmt(item.book.price * item.quantity)}</span>
                      </div>
                    ))}
                  </div>
                  <div className="grid grid-cols-1 sm:grid-cols-2 gap-2 text-sm text-muted-foreground border-t border-border pt-3">
                    <p>📍 {order.address}</p>
                    <p>📞 {order.phone}</p>
                  </div>
                </div>
              )}
            </div>
          ))}
        </div>
      )}
    </div>
  );
}

// ─── PROFILE PAGE ─────────────────────────────────────────────────────────────
function ProfilePage({ orders }: { orders: Order[] }) {
  const myOrders = orders.filter(o => o.customerId === 1);
  const totalSpent = myOrders.filter(o => o.status === "delivered").reduce((s, o) => s + o.total, 0);

  return (
    <div className="max-w-2xl mx-auto px-4 sm:px-6 py-8 min-h-screen">
      <h1 className="text-2xl font-bold mb-6" style={{ fontFamily: "'Playfair Display', serif" }}>Tài khoản của tôi</h1>

      <div className="bg-card rounded-xl border border-border p-6 mb-5">
        <div className="flex items-center gap-4 mb-5">
          <div className="w-16 h-16 bg-primary/10 rounded-full flex items-center justify-center">
            <User className="w-8 h-8 text-primary" />
          </div>
          <div>
            <h2 className="font-bold text-lg" style={{ fontFamily: "'Playfair Display', serif" }}>Nguyễn Văn An</h2>
            <p className="text-muted-foreground text-sm">nguyenvanan@gmail.com</p>
            <p className="text-muted-foreground text-sm">0901234567</p>
          </div>
        </div>
        <div className="grid grid-cols-3 gap-4">
          {[
            ["Đơn hàng", myOrders.length],
            ["Đã nhận", myOrders.filter(o => o.status === "delivered").length],
            ["Đã chi", `${fmtM(totalSpent)}đ`],
          ].map(([l, v]) => (
            <div key={l as string} className="bg-secondary/50 rounded-xl p-4 text-center">
              <div className="text-xl font-bold text-primary">{v}</div>
              <div className="text-xs text-muted-foreground mt-0.5">{l as string}</div>
            </div>
          ))}
        </div>
      </div>

      <div className="bg-card rounded-xl border border-border p-6">
        <h3 className="font-bold mb-4">Cài đặt tài khoản</h3>
        <div className="space-y-3">
          {[
            ["Địa chỉ giao hàng", "123 Lê Lợi, Quận 1, TP.HCM"],
            ["Họ và tên", "Nguyễn Văn An"],
            ["Email", "nguyenvanan@gmail.com"],
            ["Số điện thoại", "0901234567"],
          ].map(([l, v]) => (
            <div key={l as string} className="flex justify-between items-center py-3 border-b border-border last:border-0">
              <div>
                <p className="text-sm font-medium">{l as string}</p>
                <p className="text-xs text-muted-foreground mt-0.5">{v as string}</p>
              </div>
              <button className="text-primary text-sm font-medium hover:underline flex items-center gap-1">
                <Edit2 className="w-3 h-3" /> Sửa
              </button>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}

// ─── CUSTOMER APP ─────────────────────────────────────────────────────────────
function CustomerApp({ books, orders, cart, onAddToCart, onUpdateCart, onRemoveFromCart, onPlaceOrder }: {
  books: Book[]; orders: Order[]; cart: CartItem[];
  onAddToCart: (b: Book, qty?: number) => void;
  onUpdateCart: (id: number, qty: number) => void;
  onRemoveFromCart: (id: number) => void;
  onPlaceOrder: (name: string, phone: string, address: string) => void;
}) {
  const [view, setView] = useState("home");
  const [cartOpen, setCartOpen] = useState(false);
  const [selectedBook, setSelectedBook] = useState<Book | null>(null);
  const [searchQ, setSearchQ] = useState("");

  return (
    <div className="min-h-screen bg-background" style={{ fontFamily: "'Inter', sans-serif" }}>
      <CustomerNav
        cartCount={cart.reduce((s, i) => s + i.quantity, 0)}
        view={view}
        onView={setView}
        onCartOpen={() => setCartOpen(true)}
        searchQ={searchQ}
        onSearch={setSearchQ}
      />

      {view === "home" && <HomePage books={books} onView={setView} onSelect={setSelectedBook} onAddToCart={b => onAddToCart(b)} />}
      {view === "catalog" && <CatalogPage books={books} searchQ={searchQ} onSelect={setSelectedBook} onAddToCart={b => onAddToCart(b)} />}
      {view === "checkout" && <CheckoutPage cart={cart} onPlaceOrder={(n, p, a) => { onPlaceOrder(n, p, a); setView("orders"); }} onBack={() => setView("catalog")} />}
      {view === "orders" && <OrdersPage orders={orders} />}
      {view === "profile" && <ProfilePage orders={orders} />}

      {cartOpen && (
        <CartDrawer
          items={cart}
          onClose={() => setCartOpen(false)}
          onUpdate={onUpdateCart}
          onRemove={onRemoveFromCart}
          onCheckout={() => { setCartOpen(false); setView("checkout"); }}
        />
      )}

      {selectedBook && (
        <BookDetailModal
          book={selectedBook}
          onClose={() => setSelectedBook(null)}
          onAddToCart={(b, qty) => { onAddToCart(b, qty); setSelectedBook(null); }}
        />
      )}
    </div>
  );
}

// ─── STAFF SIDEBAR ────────────────────────────────────────────────────────────
function StaffSidebar({ view, onView }: { view: string; onView: (v: string) => void }) {
  return (
    <aside className="w-56 flex-shrink-0 bg-sidebar text-sidebar-foreground min-h-screen flex flex-col">
      <div className="p-5 border-b border-sidebar-border">
        <div className="flex items-center gap-2">
          <div className="w-8 h-8 bg-primary rounded-lg flex items-center justify-center">
            <BookOpen className="w-4 h-4 text-white" />
          </div>
          <div>
            <p className="font-bold text-sm" style={{ fontFamily: "'Playfair Display', serif" }}>Bookverse</p>
            <p className="text-xs text-sidebar-foreground/60">Nhân viên</p>
          </div>
        </div>
      </div>
      <nav className="flex-1 p-3 space-y-1">
        {[
          [ListOrdered, "staff-orders", "Xử lý đơn hàng"],
          [Package, "staff-stock", "Quản lý kho"],
        ].map(([Icon, v, l]) => (
          <button
            key={v as string}
            onClick={() => onView(v as string)}
            className={`w-full flex items-center gap-3 px-3 py-2.5 rounded-xl text-sm font-medium transition-colors ${view === v ? "bg-sidebar-accent text-sidebar-accent-foreground" : "text-sidebar-foreground/70 hover:bg-sidebar-accent/50 hover:text-sidebar-accent-foreground"}`}
          >
            <Icon className="w-4 h-4 flex-shrink-0" />
            {l as string}
          </button>
        ))}
      </nav>
      <div className="p-4 border-t border-sidebar-border">
        <div className="flex items-center gap-2 text-xs text-sidebar-foreground/60">
          <User className="w-4 h-4" />
          <span>Minh Tuấn</span>
        </div>
      </div>
    </aside>
  );
}

// ─── STAFF ORDERS VIEW ────────────────────────────────────────────────────────
function StaffOrdersView({ orders, onUpdateStatus }: {
  orders: Order[]; onUpdateStatus: (id: string, s: OrderStatus) => void;
}) {
  const [tab, setTab] = useState<OrderStatus | "all">("all");
  const filtered = tab === "all" ? orders : orders.filter(o => o.status === tab);

  return (
    <div className="flex-1 p-6">
      <div className="mb-6">
        <h1 className="text-xl font-bold" style={{ fontFamily: "'Playfair Display', serif" }}>Xử lý đơn hàng</h1>
        <p className="text-sm text-muted-foreground mt-1">{orders.filter(o => o.status === "pending").length} đơn chờ xác nhận</p>
      </div>

      <div className="flex gap-2 flex-wrap mb-5">
        {[["all", "Tất cả"], ["pending", "Chờ xác nhận"], ["confirmed", "Đã xác nhận"], ["shipping", "Đang giao"], ["delivered", "Đã giao"], ["cancelled", "Đã hủy"]].map(([v, l]) => (
          <button
            key={v}
            onClick={() => setTab(v as any)}
            className={`px-3 py-1.5 text-sm rounded-lg font-medium transition-colors ${tab === v ? "bg-primary text-primary-foreground" : "bg-secondary hover:bg-muted"}`}
          >
            {l}
            {v !== "all" && <span className="ml-1.5 text-xs opacity-70">{orders.filter(o => o.status === v).length}</span>}
          </button>
        ))}
      </div>

      <div className="bg-card rounded-xl border border-border overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b border-border bg-secondary/50">
                <th className="text-left px-4 py-3 font-semibold text-muted-foreground text-xs uppercase tracking-wide">Mã đơn</th>
                <th className="text-left px-4 py-3 font-semibold text-muted-foreground text-xs uppercase tracking-wide">Khách hàng</th>
                <th className="text-left px-4 py-3 font-semibold text-muted-foreground text-xs uppercase tracking-wide">Ngày</th>
                <th className="text-left px-4 py-3 font-semibold text-muted-foreground text-xs uppercase tracking-wide">Tổng tiền</th>
                <th className="text-left px-4 py-3 font-semibold text-muted-foreground text-xs uppercase tracking-wide">Trạng thái</th>
                <th className="text-left px-4 py-3 font-semibold text-muted-foreground text-xs uppercase tracking-wide">Hành động</th>
              </tr>
            </thead>
            <tbody>
              {filtered.map(order => (
                <tr key={order.id} className="border-b border-border last:border-0 hover:bg-secondary/20 transition-colors">
                  <td className="px-4 py-3 font-mono font-medium text-xs">{order.id}</td>
                  <td className="px-4 py-3">
                    <p className="font-medium">{order.customerName}</p>
                    <p className="text-xs text-muted-foreground">{order.phone}</p>
                  </td>
                  <td className="px-4 py-3 text-muted-foreground">{fmtDate(order.date)}</td>
                  <td className="px-4 py-3 font-bold text-primary">{fmt(order.total)}</td>
                  <td className="px-4 py-3"><Badge status={order.status} /></td>
                  <td className="px-4 py-3">
                    {NEXT_STATUS[order.status] ? (
                      <button
                        onClick={() => onUpdateStatus(order.id, NEXT_STATUS[order.status]!)}
                        className="text-xs bg-primary/10 text-primary hover:bg-primary hover:text-primary-foreground px-3 py-1.5 rounded-lg font-medium transition-colors"
                      >
                        → {STATUS_LABEL[NEXT_STATUS[order.status]!]}
                      </button>
                    ) : (
                      <span className="text-xs text-muted-foreground italic">—</span>
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
        {filtered.length === 0 && (
          <div className="text-center py-12 text-muted-foreground text-sm">Không có đơn hàng nào</div>
        )}
      </div>
    </div>
  );
}

// ─── STAFF STOCK VIEW ─────────────────────────────────────────────────────────
function StaffStockView({ books, onUpdateStock }: {
  books: Book[]; onUpdateStock: (id: number, newStock: number) => void;
}) {
  const [editing, setEditing] = useState<number | null>(null);
  const [editVal, setEditVal] = useState("");
  const lowStock = books.filter(b => b.stock <= 20);

  return (
    <div className="flex-1 p-6">
      <div className="mb-6">
        <h1 className="text-xl font-bold" style={{ fontFamily: "'Playfair Display', serif" }}>Quản lý kho hàng</h1>
        <p className="text-sm text-muted-foreground mt-1">{books.length} đầu sách · {lowStock.length} sách sắp hết hàng</p>
      </div>

      {lowStock.length > 0 && (
        <div className="bg-amber-50 border border-amber-200 rounded-xl p-4 mb-6 flex items-start gap-3">
          <AlertTriangle className="w-5 h-5 text-amber-600 flex-shrink-0 mt-0.5" />
          <div>
            <p className="text-sm font-semibold text-amber-800">Cảnh báo tồn kho thấp</p>
            <p className="text-xs text-amber-700 mt-1">{lowStock.map(b => b.title).join(", ")} — cần nhập hàng sớm.</p>
          </div>
        </div>
      )}

      <div className="bg-card rounded-xl border border-border overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b border-border bg-secondary/50">
                <th className="text-left px-4 py-3 font-semibold text-muted-foreground text-xs uppercase tracking-wide">Sách</th>
                <th className="text-left px-4 py-3 font-semibold text-muted-foreground text-xs uppercase tracking-wide">Danh mục</th>
                <th className="text-left px-4 py-3 font-semibold text-muted-foreground text-xs uppercase tracking-wide">Giá</th>
                <th className="text-left px-4 py-3 font-semibold text-muted-foreground text-xs uppercase tracking-wide">Tồn kho</th>
                <th className="text-left px-4 py-3 font-semibold text-muted-foreground text-xs uppercase tracking-wide">Cập nhật</th>
              </tr>
            </thead>
            <tbody>
              {books.map(book => (
                <tr key={book.id} className={`border-b border-border last:border-0 hover:bg-secondary/20 transition-colors ${book.stock <= 10 ? "bg-red-50/50" : book.stock <= 20 ? "bg-amber-50/50" : ""}`}>
                  <td className="px-4 py-3">
                    <div className="flex items-center gap-3">
                      <div className="w-8 h-11 rounded overflow-hidden flex-shrink-0 bg-secondary">
                        <img src={book.cover} alt={book.title} className="w-full h-full object-cover" />
                      </div>
                      <div>
                        <p className="font-medium line-clamp-1">{book.title}</p>
                        <p className="text-xs text-muted-foreground">{book.author}</p>
                      </div>
                    </div>
                  </td>
                  <td className="px-4 py-3 text-muted-foreground text-xs">{book.category}</td>
                  <td className="px-4 py-3 font-medium">{fmt(book.price)}</td>
                  <td className="px-4 py-3">
                    <span className={`font-bold ${book.stock === 0 ? "text-red-600" : book.stock <= 10 ? "text-orange-600" : book.stock <= 20 ? "text-amber-600" : "text-green-600"}`}>
                      {book.stock}
                    </span>
                    {book.stock <= 10 && <span className="ml-2 text-xs text-red-500">Sắp hết!</span>}
                  </td>
                  <td className="px-4 py-3">
                    {editing === book.id ? (
                      <div className="flex items-center gap-2">
                        <input
                          type="number"
                          value={editVal}
                          onChange={e => setEditVal(e.target.value)}
                          className="w-20 text-sm border border-border rounded px-2 py-1 focus:outline-none focus:border-primary/50"
                        />
                        <button
                          onClick={() => { onUpdateStock(book.id, parseInt(editVal) || 0); setEditing(null); }}
                          className="p-1 text-green-600 hover:bg-green-50 rounded transition-colors"
                        >
                          <Check className="w-4 h-4" />
                        </button>
                        <button onClick={() => setEditing(null)} className="p-1 text-red-400 hover:bg-red-50 rounded transition-colors">
                          <X className="w-4 h-4" />
                        </button>
                      </div>
                    ) : (
                      <button
                        onClick={() => { setEditing(book.id); setEditVal(book.stock.toString()); }}
                        className="text-xs flex items-center gap-1 text-primary hover:underline"
                      >
                        <Edit2 className="w-3 h-3" /> Cập nhật
                      </button>
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
}

// ─── STAFF APP ────────────────────────────────────────────────────────────────
function StaffApp({ books, orders, onUpdateStatus, onUpdateStock }: {
  books: Book[]; orders: Order[];
  onUpdateStatus: (id: string, s: OrderStatus) => void;
  onUpdateStock: (id: number, s: number) => void;
}) {
  const [view, setView] = useState("staff-orders");
  return (
    <div className="flex min-h-screen" style={{ fontFamily: "'Inter', sans-serif" }}>
      <StaffSidebar view={view} onView={setView} />
      {view === "staff-orders" && <StaffOrdersView orders={orders} onUpdateStatus={onUpdateStatus} />}
      {view === "staff-stock" && <StaffStockView books={books} onUpdateStock={onUpdateStock} />}
    </div>
  );
}

// ─── ADMIN SIDEBAR ────────────────────────────────────────────────────────────
function AdminSidebar({ view, onView }: { view: string; onView: (v: string) => void }) {
  return (
    <aside className="w-56 flex-shrink-0 bg-sidebar text-sidebar-foreground min-h-screen flex flex-col">
      <div className="p-5 border-b border-sidebar-border">
        <div className="flex items-center gap-2">
          <div className="w-8 h-8 bg-primary rounded-lg flex items-center justify-center">
            <BookOpen className="w-4 h-4 text-white" />
          </div>
          <div>
            <p className="font-bold text-sm" style={{ fontFamily: "'Playfair Display', serif" }}>Bookverse</p>
            <p className="text-xs text-sidebar-foreground/60">Quản trị viên</p>
          </div>
        </div>
      </div>
      <nav className="flex-1 p-3 space-y-1">
        {[
          [BarChart2, "admin-dashboard", "Tổng quan"],
          [BookMarked, "admin-books", "Quản lý sách"],
          [ListOrdered, "admin-orders", "Đơn hàng"],
          [Users, "admin-users", "Người dùng"],
        ].map(([Icon, v, l]) => (
          <button
            key={v as string}
            onClick={() => onView(v as string)}
            className={`w-full flex items-center gap-3 px-3 py-2.5 rounded-xl text-sm font-medium transition-colors ${view === v ? "bg-sidebar-accent text-sidebar-accent-foreground" : "text-sidebar-foreground/70 hover:bg-sidebar-accent/50 hover:text-sidebar-accent-foreground"}`}
          >
            <Icon className="w-4 h-4 flex-shrink-0" />
            {l as string}
          </button>
        ))}
      </nav>
      <div className="p-4 border-t border-sidebar-border">
        <div className="flex items-center gap-2 text-xs text-sidebar-foreground/60">
          <UserCheck className="w-4 h-4" />
          <span>Admin Bookverse</span>
        </div>
      </div>
    </aside>
  );
}

// ─── ADMIN DASHBOARD ──────────────────────────────────────────────────────────
function AdminDashboard({ books, orders, users }: {
  books: Book[]; orders: Order[]; users: AppUser[];
}) {
  const totalRevenue = orders.filter(o => o.status === "delivered").reduce((s, o) => s + o.total, 0);
  const pendingOrders = orders.filter(o => o.status === "pending").length;
  const activeUsers = users.filter(u => u.active && u.role === "customer").length;

  return (
    <div className="flex-1 p-6 overflow-y-auto">
      <div className="mb-6 flex items-center justify-between">
        <div>
          <h1 className="text-xl font-bold" style={{ fontFamily: "'Playfair Display', serif" }}>Tổng quan hệ thống</h1>
          <p className="text-sm text-muted-foreground mt-0.5">Cập nhật đến tháng 3/2024</p>
        </div>
        <button className="flex items-center gap-2 text-sm bg-secondary px-4 py-2 rounded-lg hover:bg-muted transition-colors">
          <RefreshCcw className="w-4 h-4" /> Làm mới
        </button>
      </div>

      {/* KPI cards */}
      <div className="grid grid-cols-2 lg:grid-cols-4 gap-4 mb-6">
        {[
          { label: "Doanh thu năm", value: fmt(totalRevenue), sub: "+18% so tháng trước", icon: DollarSign, color: "text-green-600", bg: "bg-green-50" },
          { label: "Tổng đơn hàng", value: orders.length, sub: `${pendingOrders} đơn chờ xử lý`, icon: ShoppingBag, color: "text-blue-600", bg: "bg-blue-50" },
          { label: "Đầu sách", value: books.length, sub: `${books.filter(b => b.stock <= 10).length} sắp hết hàng`, icon: BookOpen, color: "text-amber-600", bg: "bg-amber-50" },
          { label: "Khách hàng", value: activeUsers, sub: `${users.length} tổng tài khoản`, icon: Users, color: "text-purple-600", bg: "bg-purple-50" },
        ].map(item => (
          <div key={item.label} className="bg-card rounded-xl border border-border p-5">
            <div className="flex items-start justify-between mb-3">
              <div className={`w-10 h-10 ${item.bg} rounded-xl flex items-center justify-center`}>
                <item.icon className={`w-5 h-5 ${item.color}`} />
              </div>
              <TrendingUp className="w-4 h-4 text-green-500" />
            </div>
            <p className="text-2xl font-bold mb-0.5" style={{ fontFamily: "'Playfair Display', serif" }}>{item.value}</p>
            <p className="text-xs text-muted-foreground font-medium">{item.label}</p>
            <p className="text-xs text-green-600 mt-1">{item.sub}</p>
          </div>
        ))}
      </div>

      <div className="grid lg:grid-cols-3 gap-5 mb-5">
        {/* Revenue chart */}
        <div className="lg:col-span-2 bg-card rounded-xl border border-border p-5">
          <h3 className="font-bold mb-1">Doanh thu năm 2024</h3>
          <p className="text-xs text-muted-foreground mb-4">Doanh thu theo tháng (VNĐ)</p>
          <ResponsiveContainer width="100%" height={220}>
            <AreaChart data={REVENUE_DATA}>
              <defs>
                <linearGradient id="revGrad" x1="0" y1="0" x2="0" y2="1">
                  <stop offset="5%" stopColor="#9B2018" stopOpacity={0.2} />
                  <stop offset="95%" stopColor="#9B2018" stopOpacity={0} />
                </linearGradient>
              </defs>
              <CartesianGrid strokeDasharray="3 3" stroke="rgba(28,20,16,0.06)" />
              <XAxis dataKey="month" tick={{ fontSize: 11 }} />
              <YAxis tickFormatter={v => `${(v / 1000000).toFixed(0)}M`} tick={{ fontSize: 11 }} />
              <Tooltip formatter={(v: number) => [fmt(v), "Doanh thu"]} />
              <Area type="monotone" dataKey="revenue" stroke="#9B2018" strokeWidth={2} fill="url(#revGrad)" />
            </AreaChart>
          </ResponsiveContainer>
        </div>

        {/* Category pie */}
        <div className="bg-card rounded-xl border border-border p-5">
          <h3 className="font-bold mb-1">Doanh số theo danh mục</h3>
          <p className="text-xs text-muted-foreground mb-4">Tỉ lệ % tổng bán ra</p>
          <ResponsiveContainer width="100%" height={180}>
            <PieChart>
              <Pie data={CAT_CHART} cx="50%" cy="50%" innerRadius={50} outerRadius={80} dataKey="value" paddingAngle={2}>
                {CAT_CHART.map((entry, i) => <Cell key={i} fill={entry.color} />)}
              </Pie>
              <Tooltip formatter={(v: number) => [`${v}%`, ""]} />
            </PieChart>
          </ResponsiveContainer>
          <div className="space-y-1.5 mt-2">
            {CAT_CHART.map(c => (
              <div key={c.name} className="flex items-center justify-between text-xs">
                <div className="flex items-center gap-2">
                  <div className="w-2.5 h-2.5 rounded-full flex-shrink-0" style={{ backgroundColor: c.color }} />
                  <span className="text-muted-foreground">{c.name}</span>
                </div>
                <span className="font-medium">{c.value}%</span>
              </div>
            ))}
          </div>
        </div>
      </div>

      {/* Monthly orders bar */}
      <div className="bg-card rounded-xl border border-border p-5">
        <h3 className="font-bold mb-1">Số đơn hàng theo tháng</h3>
        <p className="text-xs text-muted-foreground mb-4">Tổng đơn hàng từng tháng năm 2024</p>
        <ResponsiveContainer width="100%" height={180}>
          <BarChart data={REVENUE_DATA}>
            <CartesianGrid strokeDasharray="3 3" stroke="rgba(28,20,16,0.06)" />
            <XAxis dataKey="month" tick={{ fontSize: 11 }} />
            <YAxis tick={{ fontSize: 11 }} />
            <Tooltip />
            <Bar dataKey="orders" fill="#D4873A" radius={[4, 4, 0, 0]} name="Đơn hàng" />
          </BarChart>
        </ResponsiveContainer>
      </div>
    </div>
  );
}

// ─── ADMIN BOOKS VIEW ─────────────────────────────────────────────────────────
function AdminBooksView({ books, onAdd, onEdit, onDelete }: {
  books: Book[];
  onAdd: (b: Omit<Book, "id">) => void;
  onEdit: (b: Book) => void;
  onDelete: (id: number) => void;
}) {
  const [search, setSearch] = useState("");
  const [showForm, setShowForm] = useState(false);
  const [editingBook, setEditingBook] = useState<Book | null>(null);
  const [form, setForm] = useState({ title: "", author: "", price: "", originalPrice: "", category: CATEGORIES[0], stock: "", description: "", publisher: "", year: "2024", pages: "", isbn: "" });

  const filtered = books.filter(b => b.title.toLowerCase().includes(search.toLowerCase()) || b.author.toLowerCase().includes(search.toLowerCase()));

  const openNew = () => {
    setEditingBook(null);
    setForm({ title: "", author: "", price: "", originalPrice: "", category: CATEGORIES[0], stock: "", description: "", publisher: "", year: "2024", pages: "", isbn: "" });
    setShowForm(true);
  };

  const openEdit = (b: Book) => {
    setEditingBook(b);
    setForm({ title: b.title, author: b.author, price: b.price.toString(), originalPrice: b.originalPrice.toString(), category: b.category, stock: b.stock.toString(), description: b.description, publisher: b.publisher, year: b.year.toString(), pages: b.pages.toString(), isbn: b.isbn });
    setShowForm(true);
  };

  const handleSave = () => {
    const base = {
      title: form.title, author: form.author, price: parseInt(form.price), originalPrice: parseInt(form.originalPrice),
      category: form.category, stock: parseInt(form.stock), description: form.description,
      publisher: form.publisher, year: parseInt(form.year), pages: parseInt(form.pages),
      isbn: form.isbn, rating: 4.5, reviews: 0, sold: 0,
      cover: `https://images.unsplash.com/photo-1544947950-fa07a98d237f?w=280&h=380&fit=crop&auto=format`,
    };
    if (editingBook) onEdit({ ...editingBook, ...base });
    else onAdd(base);
    setShowForm(false);
  };

  return (
    <div className="flex-1 p-6 overflow-y-auto">
      <div className="flex items-center justify-between mb-6">
        <div>
          <h1 className="text-xl font-bold" style={{ fontFamily: "'Playfair Display', serif" }}>Quản lý sách</h1>
          <p className="text-sm text-muted-foreground mt-0.5">{books.length} đầu sách trong hệ thống</p>
        </div>
        <button onClick={openNew} className="flex items-center gap-2 bg-primary text-primary-foreground px-4 py-2 rounded-xl text-sm font-medium hover:bg-primary/90 transition-colors">
          <Plus className="w-4 h-4" /> Thêm sách mới
        </button>
      </div>

      <div className="relative mb-5">
        <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-muted-foreground" />
        <input
          type="text"
          placeholder="Tìm kiếm theo tên sách hoặc tác giả..."
          value={search}
          onChange={e => setSearch(e.target.value)}
          className="w-full pl-10 pr-4 py-2.5 border border-border rounded-xl bg-card focus:outline-none focus:border-primary/50 text-sm"
        />
      </div>

      <div className="bg-card rounded-xl border border-border overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b border-border bg-secondary/50">
                <th className="text-left px-4 py-3 font-semibold text-muted-foreground text-xs uppercase tracking-wide">Sách</th>
                <th className="text-left px-4 py-3 font-semibold text-muted-foreground text-xs uppercase tracking-wide">Danh mục</th>
                <th className="text-left px-4 py-3 font-semibold text-muted-foreground text-xs uppercase tracking-wide">Giá</th>
                <th className="text-left px-4 py-3 font-semibold text-muted-foreground text-xs uppercase tracking-wide">Kho</th>
                <th className="text-left px-4 py-3 font-semibold text-muted-foreground text-xs uppercase tracking-wide">Đã bán</th>
                <th className="text-left px-4 py-3 font-semibold text-muted-foreground text-xs uppercase tracking-wide">Thao tác</th>
              </tr>
            </thead>
            <tbody>
              {filtered.map(book => (
                <tr key={book.id} className="border-b border-border last:border-0 hover:bg-secondary/20 transition-colors">
                  <td className="px-4 py-3">
                    <div className="flex items-center gap-3">
                      <div className="w-8 h-11 rounded overflow-hidden flex-shrink-0 bg-secondary">
                        <img src={book.cover} alt={book.title} className="w-full h-full object-cover" />
                      </div>
                      <div>
                        <p className="font-medium line-clamp-1">{book.title}</p>
                        <p className="text-xs text-muted-foreground">{book.author}</p>
                      </div>
                    </div>
                  </td>
                  <td className="px-4 py-3 text-xs text-muted-foreground">{book.category}</td>
                  <td className="px-4 py-3">
                    <p className="font-medium text-primary">{fmt(book.price)}</p>
                    <p className="text-xs text-muted-foreground line-through">{fmt(book.originalPrice)}</p>
                  </td>
                  <td className="px-4 py-3">
                    <span className={`font-semibold ${book.stock <= 10 ? "text-red-600" : book.stock <= 20 ? "text-amber-600" : "text-green-600"}`}>
                      {book.stock}
                    </span>
                  </td>
                  <td className="px-4 py-3 text-muted-foreground">{book.sold.toLocaleString("vi-VN")}</td>
                  <td className="px-4 py-3">
                    <div className="flex items-center gap-2">
                      <button onClick={() => openEdit(book)} className="p-1.5 text-blue-600 hover:bg-blue-50 rounded-lg transition-colors">
                        <Edit2 className="w-4 h-4" />
                      </button>
                      <button
                        onClick={() => { if (confirm(`Xóa sách "${book.title}"?`)) onDelete(book.id); }}
                        className="p-1.5 text-red-500 hover:bg-red-50 rounded-lg transition-colors"
                      >
                        <Trash2 className="w-4 h-4" />
                      </button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
        {filtered.length === 0 && <div className="text-center py-12 text-muted-foreground text-sm">Không tìm thấy sách</div>}
      </div>

      {/* Add/Edit modal */}
      {showForm && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/50 backdrop-blur-sm">
          <div className="bg-card rounded-2xl shadow-2xl max-w-xl w-full max-h-[90vh] overflow-y-auto">
            <div className="flex items-center justify-between p-6 border-b border-border">
              <h2 className="font-bold text-lg" style={{ fontFamily: "'Playfair Display', serif" }}>
                {editingBook ? "Chỉnh sửa sách" : "Thêm sách mới"}
              </h2>
              <button onClick={() => setShowForm(false)} className="p-1 hover:bg-muted rounded-lg transition-colors">
                <X className="w-5 h-5" />
              </button>
            </div>
            <div className="p-6 space-y-4">
              <div className="grid grid-cols-1 gap-4">
                {[
                  ["Tên sách *", "title", "text"],
                  ["Tác giả *", "author", "text"],
                  ["ISBN", "isbn", "text"],
                  ["Nhà xuất bản", "publisher", "text"],
                ].map(([l, k, t]) => (
                  <div key={k as string}>
                    <label className="block text-sm font-medium mb-1.5">{l as string}</label>
                    <input type={t as string} value={form[k as keyof typeof form]} onChange={e => setForm(f => ({ ...f, [k as string]: e.target.value }))} className="w-full px-3 py-2 border border-border rounded-xl bg-secondary/30 text-sm focus:outline-none focus:border-primary/50" />
                  </div>
                ))}
                <div className="grid grid-cols-2 gap-4">
                  {[["Giá bán *", "price"], ["Giá gốc *", "originalPrice"], ["Tồn kho *", "stock"], ["Số trang", "pages"]].map(([l, k]) => (
                    <div key={k}>
                      <label className="block text-sm font-medium mb-1.5">{l}</label>
                      <input type="number" value={form[k as keyof typeof form]} onChange={e => setForm(f => ({ ...f, [k]: e.target.value }))} className="w-full px-3 py-2 border border-border rounded-xl bg-secondary/30 text-sm focus:outline-none focus:border-primary/50" />
                    </div>
                  ))}
                </div>
                <div>
                  <label className="block text-sm font-medium mb-1.5">Danh mục</label>
                  <select value={form.category} onChange={e => setForm(f => ({ ...f, category: e.target.value }))} className="w-full px-3 py-2 border border-border rounded-xl bg-secondary/30 text-sm focus:outline-none focus:border-primary/50">
                    {CATEGORIES.map(c => <option key={c} value={c}>{c}</option>)}
                  </select>
                </div>
                <div>
                  <label className="block text-sm font-medium mb-1.5">Mô tả</label>
                  <textarea rows={3} value={form.description} onChange={e => setForm(f => ({ ...f, description: e.target.value }))} className="w-full px-3 py-2 border border-border rounded-xl bg-secondary/30 text-sm focus:outline-none focus:border-primary/50 resize-none" />
                </div>
              </div>
              <div className="flex gap-3 pt-2">
                <button onClick={() => setShowForm(false)} className="flex-1 border border-border py-2.5 rounded-xl text-sm font-medium hover:bg-secondary transition-colors">Hủy</button>
                <button onClick={handleSave} disabled={!form.title || !form.author || !form.price} className="flex-1 bg-primary text-primary-foreground py-2.5 rounded-xl text-sm font-medium hover:bg-primary/90 transition-colors disabled:opacity-50">
                  {editingBook ? "Lưu thay đổi" : "Thêm sách"}
                </button>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}

// ─── ADMIN ORDERS VIEW ────────────────────────────────────────────────────────
function AdminOrdersView({ orders, onUpdateStatus }: {
  orders: Order[]; onUpdateStatus: (id: string, s: OrderStatus) => void;
}) {
  const [search, setSearch] = useState("");
  const [statusFilter, setStatusFilter] = useState<OrderStatus | "all">("all");

  const filtered = orders.filter(o => {
    const matchSearch = o.id.toLowerCase().includes(search.toLowerCase()) || o.customerName.toLowerCase().includes(search.toLowerCase());
    const matchStatus = statusFilter === "all" || o.status === statusFilter;
    return matchSearch && matchStatus;
  });

  return (
    <div className="flex-1 p-6 overflow-y-auto">
      <div className="mb-6 flex items-center justify-between flex-wrap gap-3">
        <div>
          <h1 className="text-xl font-bold" style={{ fontFamily: "'Playfair Display', serif" }}>Quản lý đơn hàng</h1>
          <p className="text-sm text-muted-foreground mt-0.5">{orders.length} đơn hàng tổng · {orders.filter(o => o.status === "pending").length} chờ xử lý</p>
        </div>
      </div>

      <div className="flex flex-col sm:flex-row gap-3 mb-5">
        <div className="relative flex-1">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-muted-foreground" />
          <input type="text" placeholder="Tìm mã đơn, tên khách..." value={search} onChange={e => setSearch(e.target.value)} className="w-full pl-10 pr-4 py-2.5 border border-border rounded-xl bg-card text-sm focus:outline-none focus:border-primary/50" />
        </div>
        <select value={statusFilter} onChange={e => setStatusFilter(e.target.value as any)} className="px-3 py-2.5 border border-border rounded-xl bg-card text-sm focus:outline-none focus:border-primary/50">
          <option value="all">Tất cả trạng thái</option>
          {Object.entries(STATUS_LABEL).map(([v, l]) => <option key={v} value={v}>{l}</option>)}
        </select>
      </div>

      <div className="bg-card rounded-xl border border-border overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b border-border bg-secondary/50">
                {["Mã đơn", "Khách hàng", "Sản phẩm", "Ngày đặt", "Tổng tiền", "Trạng thái", "Hành động"].map(h => (
                  <th key={h} className="text-left px-4 py-3 font-semibold text-muted-foreground text-xs uppercase tracking-wide whitespace-nowrap">{h}</th>
                ))}
              </tr>
            </thead>
            <tbody>
              {filtered.map(order => (
                <tr key={order.id} className="border-b border-border last:border-0 hover:bg-secondary/20 transition-colors">
                  <td className="px-4 py-3 font-mono font-medium text-xs">{order.id}</td>
                  <td className="px-4 py-3">
                    <p className="font-medium whitespace-nowrap">{order.customerName}</p>
                    <p className="text-xs text-muted-foreground">{order.phone}</p>
                  </td>
                  <td className="px-4 py-3 text-muted-foreground text-xs">{order.items.length} sản phẩm</td>
                  <td className="px-4 py-3 text-muted-foreground whitespace-nowrap">{fmtDate(order.date)}</td>
                  <td className="px-4 py-3 font-bold text-primary whitespace-nowrap">{fmt(order.total)}</td>
                  <td className="px-4 py-3"><Badge status={order.status} /></td>
                  <td className="px-4 py-3">
                    <select
                      value={order.status}
                      onChange={e => onUpdateStatus(order.id, e.target.value as OrderStatus)}
                      className="text-xs border border-border rounded-lg px-2 py-1 bg-card focus:outline-none focus:border-primary/50"
                    >
                      {Object.entries(STATUS_LABEL).map(([v, l]) => <option key={v} value={v}>{l}</option>)}
                    </select>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
        {filtered.length === 0 && <div className="text-center py-12 text-muted-foreground text-sm">Không tìm thấy đơn hàng</div>}
      </div>
    </div>
  );
}

// ─── ADMIN USERS VIEW ─────────────────────────────────────────────────────────
function AdminUsersView({ users, onToggleActive, onChangeRole }: {
  users: AppUser[];
  onToggleActive: (id: number) => void;
  onChangeRole: (id: number, role: AppUser["role"]) => void;
}) {
  const [search, setSearch] = useState("");
  const [roleFilter, setRoleFilter] = useState("all");

  const filtered = users.filter(u => {
    const matchSearch = u.name.toLowerCase().includes(search.toLowerCase()) || u.email.toLowerCase().includes(search.toLowerCase());
    const matchRole = roleFilter === "all" || u.role === roleFilter;
    return matchSearch && matchRole;
  });

  return (
    <div className="flex-1 p-6 overflow-y-auto">
      <div className="mb-6">
        <h1 className="text-xl font-bold" style={{ fontFamily: "'Playfair Display', serif" }}>Quản lý người dùng</h1>
        <p className="text-sm text-muted-foreground mt-0.5">{users.length} tài khoản · {users.filter(u => u.active).length} đang hoạt động</p>
      </div>

      <div className="flex flex-col sm:flex-row gap-3 mb-5">
        <div className="relative flex-1">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-muted-foreground" />
          <input type="text" placeholder="Tìm tên, email..." value={search} onChange={e => setSearch(e.target.value)} className="w-full pl-10 pr-4 py-2.5 border border-border rounded-xl bg-card text-sm focus:outline-none focus:border-primary/50" />
        </div>
        <select value={roleFilter} onChange={e => setRoleFilter(e.target.value)} className="px-3 py-2.5 border border-border rounded-xl bg-card text-sm focus:outline-none focus:border-primary/50">
          <option value="all">Tất cả vai trò</option>
          <option value="customer">Khách hàng</option>
          <option value="staff">Nhân viên</option>
          <option value="admin">Quản trị viên</option>
        </select>
      </div>

      <div className="bg-card rounded-xl border border-border overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b border-border bg-secondary/50">
                {["Người dùng", "Liên hệ", "Vai trò", "Đơn hàng", "Tổng chi", "Ngày tham gia", "Trạng thái", "Thao tác"].map(h => (
                  <th key={h} className="text-left px-4 py-3 font-semibold text-muted-foreground text-xs uppercase tracking-wide whitespace-nowrap">{h}</th>
                ))}
              </tr>
            </thead>
            <tbody>
              {filtered.map(user => (
                <tr key={user.id} className="border-b border-border last:border-0 hover:bg-secondary/20 transition-colors">
                  <td className="px-4 py-3">
                    <div className="flex items-center gap-3">
                      <div className="w-8 h-8 bg-primary/10 rounded-full flex items-center justify-center flex-shrink-0">
                        <User className="w-4 h-4 text-primary" />
                      </div>
                      <span className="font-medium whitespace-nowrap">{user.name}</span>
                    </div>
                  </td>
                  <td className="px-4 py-3 text-muted-foreground text-xs">
                    <p>{user.email}</p>
                    <p>{user.phone}</p>
                  </td>
                  <td className="px-4 py-3">
                    <select
                      value={user.role}
                      onChange={e => onChangeRole(user.id, e.target.value as AppUser["role"])}
                      className="text-xs border border-border rounded-lg px-2 py-1 bg-card focus:outline-none"
                    >
                      <option value="customer">Khách hàng</option>
                      <option value="staff">Nhân viên</option>
                      <option value="admin">Quản trị viên</option>
                    </select>
                  </td>
                  <td className="px-4 py-3 text-muted-foreground">{user.totalOrders}</td>
                  <td className="px-4 py-3 font-medium text-primary">{user.totalSpent > 0 ? fmt(user.totalSpent) : "—"}</td>
                  <td className="px-4 py-3 text-muted-foreground whitespace-nowrap">{fmtDate(user.joinDate)}</td>
                  <td className="px-4 py-3">
                    <span className={`text-xs font-medium px-2.5 py-1 rounded-full border ${user.active ? "bg-green-100 text-green-800 border-green-200" : "bg-red-100 text-red-800 border-red-200"}`}>
                      {user.active ? "Hoạt động" : "Khóa"}
                    </span>
                  </td>
                  <td className="px-4 py-3">
                    <button
                      onClick={() => onToggleActive(user.id)}
                      className={`text-xs px-3 py-1.5 rounded-lg font-medium transition-colors ${user.active ? "bg-red-50 text-red-600 hover:bg-red-100" : "bg-green-50 text-green-600 hover:bg-green-100"}`}
                    >
                      {user.active ? "Khóa" : "Mở khóa"}
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
        {filtered.length === 0 && <div className="text-center py-12 text-muted-foreground text-sm">Không tìm thấy người dùng</div>}
      </div>
    </div>
  );
}

// ─── ADMIN APP ────────────────────────────────────────────────────────────────
function AdminApp({ books, orders, users, onAddBook, onEditBook, onDeleteBook, onUpdateStatus, onToggleUserActive, onChangeUserRole }: {
  books: Book[]; orders: Order[]; users: AppUser[];
  onAddBook: (b: Omit<Book, "id">) => void;
  onEditBook: (b: Book) => void;
  onDeleteBook: (id: number) => void;
  onUpdateStatus: (id: string, s: OrderStatus) => void;
  onToggleUserActive: (id: number) => void;
  onChangeUserRole: (id: number, role: AppUser["role"]) => void;
}) {
  const [view, setView] = useState("admin-dashboard");
  return (
    <div className="flex min-h-screen" style={{ fontFamily: "'Inter', sans-serif" }}>
      <AdminSidebar view={view} onView={setView} />
      {view === "admin-dashboard" && <AdminDashboard books={books} orders={orders} users={users} />}
      {view === "admin-books" && <AdminBooksView books={books} onAdd={onAddBook} onEdit={onEditBook} onDelete={onDeleteBook} />}
      {view === "admin-orders" && <AdminOrdersView orders={orders} onUpdateStatus={onUpdateStatus} />}
      {view === "admin-users" && <AdminUsersView users={users} onToggleActive={onToggleUserActive} onChangeRole={onChangeUserRole} />}
    </div>
  );
}

// ─── ROLE SWITCHER ────────────────────────────────────────────────────────────
function RoleSwitcher({ role, onRole }: { role: Role; onRole: (r: Role) => void }) {
  const roles: [Role, string, string][] = [
    ["customer", "Khách hàng", "👤"],
    ["staff", "Nhân viên", "🏪"],
    ["admin", "Quản trị viên", "⚙️"],
  ];
  return (
    <div className="fixed bottom-5 left-1/2 -translate-x-1/2 z-50 bg-[#2A1A12]/95 backdrop-blur-sm text-white rounded-2xl shadow-2xl border border-white/10 px-4 py-3 flex items-center gap-1">
      <span className="text-xs text-white/50 mr-2 font-medium">Demo:</span>
      {roles.map(([r, l, emoji]) => (
        <button
          key={r}
          onClick={() => onRole(r)}
          className={`flex items-center gap-1.5 px-3 py-1.5 rounded-xl text-xs font-medium transition-all ${role === r ? "bg-primary text-white shadow-sm scale-105" : "text-white/70 hover:text-white hover:bg-white/10"}`}
        >
          <span>{emoji}</span>
          {l}
        </button>
      ))}
    </div>
  );
}

// ─── ROOT APP ─────────────────────────────────────────────────────────────────
export default function App() {
  const [role, setRole] = useState<Role>("customer");
  const [books, setBooks] = useState<Book[]>(BOOKS_INITIAL);
  const [orders, setOrders] = useState<Order[]>(ORDERS_INITIAL);
  const [users, setUsers] = useState<AppUser[]>(USERS_INITIAL);
  const [cart, setCart] = useState<CartItem[]>([]);

  const addToCart = (book: Book, qty = 1) => {
    setCart(c => {
      const ex = c.find(i => i.book.id === book.id);
      if (ex) return c.map(i => i.book.id === book.id ? { ...i, quantity: i.quantity + qty } : i);
      return [...c, { book, quantity: qty }];
    });
  };

  const updateCart = (id: number, qty: number) => setCart(c => c.map(i => i.book.id === id ? { ...i, quantity: qty } : i));
  const removeFromCart = (id: number) => setCart(c => c.filter(i => i.book.id !== id));

  const placeOrder = (name: string, phone: string, address: string) => {
    const total = cart.reduce((s, i) => s + i.book.price * i.quantity, 0);
    const newOrder: Order = {
      id: `ORD-2024-${String(orders.length + 1).padStart(3, "0")}`,
      customerId: 1, customerName: name,
      items: [...cart], total, status: "pending",
      date: new Date().toISOString().split("T")[0],
      address, phone,
    };
    setOrders(o => [newOrder, ...o]);
    setCart([]);
  };

  const updateOrderStatus = (id: string, status: OrderStatus) =>
    setOrders(o => o.map(order => order.id === id ? { ...order, status } : order));

  const updateStock = (id: number, stock: number) =>
    setBooks(b => b.map(book => book.id === id ? { ...book, stock } : book));

  const addBook = (book: Omit<Book, "id">) =>
    setBooks(b => [...b, { ...book, id: Math.max(...b.map(x => x.id)) + 1 }]);

  const editBook = (book: Book) => setBooks(b => b.map(x => x.id === book.id ? book : x));
  const deleteBook = (id: number) => setBooks(b => b.filter(x => x.id !== id));

  const toggleUserActive = (id: number) => setUsers(u => u.map(x => x.id === id ? { ...x, active: !x.active } : x));
  const changeUserRole = (id: number, r: AppUser["role"]) => setUsers(u => u.map(x => x.id === id ? { ...x, role: r } : x));

  return (
    <>
      {role === "customer" && (
        <CustomerApp
          books={books} orders={orders} cart={cart}
          onAddToCart={addToCart} onUpdateCart={updateCart}
          onRemoveFromCart={removeFromCart} onPlaceOrder={placeOrder}
        />
      )}
      {role === "staff" && (
        <StaffApp
          books={books} orders={orders}
          onUpdateStatus={updateOrderStatus} onUpdateStock={updateStock}
        />
      )}
      {role === "admin" && (
        <AdminApp
          books={books} orders={orders} users={users}
          onAddBook={addBook} onEditBook={editBook} onDeleteBook={deleteBook}
          onUpdateStatus={updateOrderStatus}
          onToggleUserActive={toggleUserActive} onChangeUserRole={changeUserRole}
        />
      )}
      <RoleSwitcher role={role} onRole={setRole} />
    </>
  );
}
