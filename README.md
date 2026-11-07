# Đồ án cuối kỳ: Quản Lý Kho Hàng — Nhóm 11

> Hồ sơ bàn giao đã được chuẩn hóa theo Nội dung 15: MVP, build Release, kiểm thử,
> dữ liệu mẫu và tài liệu hướng dẫn chạy.

## Môn: Lập trình trên Windows
**Giảng viên hướng dẫn:** ThS. Lê Thanh Thoại

---

## 1. Giới thiệu
Ứng dụng **Quản lý kho hàng** xây dựng trên nền tảng Windows Forms (.NET Framework 4.7.2), áp dụng kiến trúc **MVP (Model – View – Presenter)**, kết nối cơ sở dữ liệu SQL Server. Chương trình hỗ trợ:
- Đăng nhập phân quyền (Quản lý / Nhân viên kho)
- Quản lý danh mục: Hàng hóa, Loại hàng, Nhà cung cấp
- Nghiệp vụ: Lập phiếu nhập kho, phiếu xuất kho (dùng SqlTransaction)
- Báo cáo: Tính tồn kho (Stored Procedure), tra cứu lịch sử, xuất CSV, biểu đồ GDI+, in phiếu PrintDocument

---

## 2. Môi trường phát triển
| Thành phần | Phiên bản |
|---|---|
| IDE | Visual Studio 2022 trở lên |
| Framework | .NET Framework 4.7.2 |
| Cơ sở dữ liệu | SQL Server 2019 trở lên (hoặc SQL Server Express / LocalDB) |
| Ngôn ngữ | C# 7.3 |

---

## 3. Cấu trúc thư mục dự án (MVP)
```
QuanLyKhoHang/
├── Models/             # Lớp dữ liệu (HangHoa, PhieuKho, ThongKeModels...)
├── Views/              # Giao diện Form + Interface (IHangHoaView, FrmPhieuKho...)
├── Presenters/         # Xử lý logic nghiệp vụ (HangHoaPresenter, PhieuKhoPresenter...)
├── Repositories/       # Truy vấn CSDL (HangHoaRepository, TaiKhoanRepository...)
├── Services/           # Tiện ích hỗ trợ (BieuDoGdiHelper, CsvHelperCustom, PhieuKhoPrintHelper)
├── Database.cs         # Lớp kết nối dùng chung
├── PhienDangNhap.cs    # Lưu thông tin phiên đăng nhập
├── FrmMain.cs          # Màn hình chính
├── FrmLogin.cs         # Màn hình đăng nhập
├── Program.cs          # Entry point chính
└── ProgramThanhVien3.cs # Entry point chạy thử riêng module TV3
```

---

## 4. Hướng dẫn cài đặt và chạy

### Bước 1: Tạo cơ sở dữ liệu
1. Mở **SQL Server Management Studio** (SSMS).
2. Chạy file `Scripts/QuanLyKhoHangDB.sql` một lần — file này tạo database, tất cả bảng,
   stored procedures và dữ liệu mẫu đầy đủ.
3. Nếu database đã tồn tại, không chạy lại script tổng hợp; hãy dùng database hiện tại
   hoặc tạo một database thử nghiệm mới để tránh lỗi trùng tên/dữ liệu.

Nếu chạy bằng `sqlcmd` thay vì SSMS, dùng tùy chọn đọc UTF-8 `-f 65001`; SSMS tự
nhận dạng tệp script này.

Có thể phục hồi từ `Database/QuanLyKhoHangDB.bak` nếu cần. Hai cách trên là lựa chọn
thay thế, không chạy đồng thời.

### Bước 2: Cấu hình chuỗi kết nối
Mở file `SourceCode/QuanLyKhoHang/QuanLyKhoHang/Database.cs`, chỉnh sửa chuỗi kết nối phù hợp với SQL Server của bạn:
```csharp
public static string ChuoiKetNoi = @"Data Source=.\SQLEXPRESS;Initial Catalog=QuanLyKhoHangDB;Integrated Security=True";
```
> **Lưu ý:** Nếu dùng SQL Server khác (ví dụ: `localhost`, `(localdb)\MSSQLLocalDB`,...), thay `.\SQLEXPRESS` bằng tên instance tương ứng.

### Bước 3: Build và chạy
- Mở solution `SourceCode/QuanLyKhoHang/QuanLyKhoHang.slnx` bằng Visual Studio 2022 trở lên.
- Nhấn **F5** hoặc **Ctrl+F5** để build và chạy.

Trước khi nộp, chọn cấu hình **Release**, build lại và chạy thử bản trong
`SourceCode/QuanLyKhoHang/QuanLyKhoHang/bin/Release/`.

### Tài khoản đăng nhập mẫu
| Tài khoản | Mật khẩu | Vai trò |
|---|---|---|
| `admin` | `123456` | Quản lý (full quyền) |
| `nvkho` | `123456` | Nhân viên kho |

---

## 5. Phân công thành viên

### Thành viên 1: Thiết kế CSDL & Module cốt lõi
- Thiết kế cơ sở dữ liệu (6 bảng + ràng buộc + SP `sp_LayDanhSachHangHoa`)
- Xây dựng lớp kết nối dùng chung `Database.cs`
- Màn hình đăng nhập phân quyền (`FrmLogin`)
- Màn hình chính `FrmMain` (menu danh mục, nghiệp vụ, báo cáo)
- Phân hệ Quản lý Hàng hóa (`FrmHangHoa`) + Loại hàng (`FrmLoaiHang`)

### Thành viên 2: Module Nhà cung cấp & Phiếu kho
- Phân hệ Quản lý Nhà cung cấp (`FrmNhaCungCap`)
- Màn hình nghiệp vụ Lập phiếu nhập/xuất kho (`FrmPhieuKho`)
- Sử dụng `SqlTransaction` để lưu đồng thời thông tin chung và danh sách chi tiết phiếu

### Thành viên 3: Module Báo cáo & Thống kê
- Tính tồn kho qua Stored Procedure (`sp_TinhTonKhoChiTiet`) + cảnh báo màu hàng tồn
- Tra cứu lịch sử nhập xuất theo khoảng ngày (`sp_TraCuuLichSuNhapXuat`)
- Xuất file CSV (UTF-8 BOM, mở Excel không lỗi font)
- Vẽ biểu đồ nhập xuất tồn bằng GDI+ (biểu đồ cột nhóm + đường)
- In phiếu kho bằng PrintDocument (có PrintPreview)

---

## 6. Chạy thử riêng module Thành viên 3
Chạy file `SourceCode/QuanLyKhoHang/ChayThu_ThanhVien3.bat` để biên dịch và chạy riêng các tính năng của TV3 mà không cần mở Visual Studio.

---

## 7. Cấu trúc SQL Scripts
| File | Mô tả |
|---|---|
| `Scripts/QuanLyKhoHangDB.sql` | **Script tổng hợp đầy đủ:** tạo DB, bảng, tất cả SP, dữ liệu mẫu |
| `Database/01_CreateDatabase.sql` | Tạo database và bảng (chạy riêng) |
| `Database/02_InsertSampleData.sql` | Dữ liệu mẫu cơ bản |
| `Database/03_ThanhVien3_StoredProcedures_DuLieu.sql` | SP và dữ liệu mẫu bổ sung của TV3 |

## 8. Kiểm thử bàn giao

Danh sách test case đầy đủ nằm trong `TestCases/TestCase.xlsx`. Các luồng tối thiểu:

- Đăng nhập đúng/sai và kiểm tra phân quyền Quản lý/Nhân viên kho.
- Thêm, sửa, xóa, tìm kiếm hàng hóa, loại hàng và nhà cung cấp.
- Lập phiếu nhập/xuất, kiểm tra tồn kho và rollback khi dữ liệu không hợp lệ.
- Tra cứu lịch sử theo ngày, loại phiếu, từ khóa.
- Tính tồn kho, cảnh báo hết hàng/tồn thấp, xuất CSV, biểu đồ và in phiếu.

## 9. Báo cáo đồ án

`Report/BaoCao.pdf` là báo cáo đầy đủ trên 50 trang, gồm phân tích yêu cầu,
kiến trúc MVP, thiết kế CSDL, thiết kế giao diện, cài đặt, kiểm thử, kịch bản
demo, hạn chế/hướng phát triển và phụ lục mã nguồn/SQL.

## 10. Phân công và kiến trúc

- **Thành viên 1:** CSDL, `Database.cs`, đăng nhập/phân quyền, `FrmMain`, hàng hóa và loại hàng.
- **Thành viên 2:** Nhà cung cấp, phiếu nhập/xuất và `SqlTransaction`.
- **Thành viên 3:** Tồn kho bằng stored procedure, lịch sử, CSV, GDI+ và PrintDocument.

Các Form giao tiếp với Presenter qua interface View; Repository/Service chịu trách nhiệm
truy cập dữ liệu hoặc xử lý chuyên biệt, phù hợp mô hình MVP.
