USE master;
GO

IF EXISTS (SELECT name FROM sys.databases WHERE name = N'QuanLyKhoHangDB')
BEGIN
    ALTER DATABASE QuanLyKhoHangDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE QuanLyKhoHangDB;
END
GO

CREATE DATABASE QuanLyKhoHangDB;
GO

USE QuanLyKhoHangDB;
GO

-- 1. Bảng Tài khoản (Phục vụ đăng nhập, đổi mật khẩu, phân quyền: Quản lý / Nhân viên kho)
CREATE TABLE TaiKhoan (
    TenDangNhap VARCHAR(50) PRIMARY KEY,
    MatKhau VARCHAR(100) NOT NULL,
    HoTen NVARCHAR(100) NOT NULL,
    VaiTro NVARCHAR(50) NOT NULL CHECK (VaiTro IN (N'Quản lý', N'Nhân viên kho')),
    TrangThai BIT DEFAULT 1
);
GO

-- 2. Bảng Loại hàng
CREATE TABLE LoaiHang (
    MaLoai VARCHAR(20) PRIMARY KEY,
    TenLoai NVARCHAR(100) NOT NULL UNIQUE,
    MoTa NVARCHAR(255)
);
GO

-- 3. Bảng Nhà cung cấp
CREATE TABLE NhaCungCap (
    MaNCC VARCHAR(20) PRIMARY KEY,
    TenNCC NVARCHAR(150) NOT NULL,
    DienThoai VARCHAR(20) NOT NULL,
    DiaChi NVARCHAR(255),
    Email VARCHAR(100)
);
GO

-- 4. Bảng Hàng hóa
CREATE TABLE HangHoa (
    MaHang VARCHAR(20) PRIMARY KEY,
    TenHang NVARCHAR(150) NOT NULL,
    MaLoai VARCHAR(20) NOT NULL FOREIGN KEY REFERENCES LoaiHang(MaLoai),
    DonViTinh NVARCHAR(50) NOT NULL,
    DonGiaNhap DECIMAL(18, 2) NOT NULL CHECK (DonGiaNhap >= 0),
    DonGiaXuat DECIMAL(18, 2) NOT NULL CHECK (DonGiaXuat >= 0),
    SoLuongTon INT NOT NULL DEFAULT 0 CHECK (SoLuongTon >= 0),
    MucTonToiThieu INT NOT NULL DEFAULT 10 CHECK (MucTonToiThieu >= 0) -- Dùng để cảnh báo tồn thấp
);
GO

-- 5. Bảng Phiếu nhập
CREATE TABLE PhieuNhap (
    MaPhieuNhap VARCHAR(20) PRIMARY KEY,
    NgayNhap DATETIME NOT NULL DEFAULT GETDATE(),
    MaNCC VARCHAR(20) NOT NULL FOREIGN KEY REFERENCES NhaCungCap(MaNCC),
    NguoiLap VARCHAR(50) NOT NULL FOREIGN KEY REFERENCES TaiKhoan(TenDangNhap),
    TongTien DECIMAL(18, 2) NOT NULL DEFAULT 0,
    GhiChu NVARCHAR(255)
);
GO

-- 6. Bảng Chi tiết phiếu nhập
CREATE TABLE ChiTietPhieuNhap (
    MaPhieuNhap VARCHAR(20) NOT NULL FOREIGN KEY REFERENCES PhieuNhap(MaPhieuNhap) ON DELETE CASCADE,
    MaHang VARCHAR(20) NOT NULL FOREIGN KEY REFERENCES HangHoa(MaHang),
    SoLuong INT NOT NULL CHECK (SoLuong > 0),
    DonGia DECIMAL(18, 2) NOT NULL CHECK (DonGia >= 0),
    ThanhTien AS (SoLuong * DonGia),
    PRIMARY KEY (MaPhieuNhap, MaHang)
);
GO

-- 7. Bảng Phiếu xuất
CREATE TABLE PhieuXuat (
    MaPhieuXuat VARCHAR(20) PRIMARY KEY,
    NgayXuat DATETIME NOT NULL DEFAULT GETDATE(),
    KhachHangHoacBoPhan NVARCHAR(150) NOT NULL,
    NguoiLap VARCHAR(50) NOT NULL FOREIGN KEY REFERENCES TaiKhoan(TenDangNhap),
    TongTien DECIMAL(18, 2) NOT NULL DEFAULT 0,
    GhiChu NVARCHAR(255)
);
GO

-- 8. Bảng Chi tiết phiếu xuất
CREATE TABLE ChiTietPhieuXuat (
    MaPhieuXuat VARCHAR(20) NOT NULL FOREIGN KEY REFERENCES PhieuXuat(MaPhieuXuat) ON DELETE CASCADE,
    MaHang VARCHAR(20) NOT NULL FOREIGN KEY REFERENCES HangHoa(MaHang),
    SoLuong INT NOT NULL CHECK (SoLuong > 0),
    DonGia DECIMAL(18, 2) NOT NULL CHECK (DonGia >= 0),
    ThanhTien AS (SoLuong * DonGia),
    PRIMARY KEY (MaPhieuXuat, MaHang)
);
GO

-- 9. STORED PROCEDURE: Tính tồn kho hiện tại và kiểm tra cảnh báo mức tồn thấp
CREATE PROCEDURE sp_LayDanhSachTonKho
    @TuKhoa NVARCHAR(100) = N''
AS
BEGIN
    SELECT 
        h.MaHang,
        h.TenHang,
        l.TenLoai,
        h.DonViTinh,
        ISNULL((SELECT SUM(SoLuong) FROM ChiTietPhieuNhap ct WHERE ct.MaHang = h.MaHang), 0) AS TongNhap,
        ISNULL((SELECT SUM(SoLuong) FROM ChiTietPhieuXuat cx WHERE cx.MaHang = h.MaHang), 0) AS TongXuat,
        h.SoLuongTon,
        h.MucTonToiThieu,
        CASE 
            WHEN h.SoLuongTon = 0 THEN N'Hết hàng'
            WHEN h.SoLuongTon <= h.MucTonToiThieu THEN N'Cảnh báo tồn thấp'
            ELSE N'Bình thường'
        END AS TrangThaiTon
    FROM HangHoa h
    INNER JOIN LoaiHang l ON h.MaLoai = l.MaLoai
    WHERE h.MaHang LIKE N'%' + @TuKhoa + N'%' 
       OR h.TenHang LIKE N'%' + @TuKhoa + N'%';
END
GO

-- 10. VIEW: Lịch sử biến động Nhập - Xuất kho
CREATE VIEW vw_LichSuNhapXuat AS
SELECT 
    pn.MaPhieuNhap AS MaPhieu,
    N'Nhập kho' AS LoaiPhieu,
    pn.NgayNhap AS NgayGiaoDich,
    ct.MaHang,
    hh.TenHang,
    ct.SoLuong,
    ct.DonGia,
    ct.ThanhTien,
    tk.HoTen AS NguoiLap
FROM PhieuNhap pn
INNER JOIN ChiTietPhieuNhap ct ON pn.MaPhieuNhap = ct.MaPhieuNhap
INNER JOIN HangHoa hh ON ct.MaHang = hh.MaHang
INNER JOIN TaiKhoan tk ON pn.NguoiLap = tk.TenDangNhap

UNION ALL

SELECT 
    px.MaPhieuXuat AS MaPhieu,
    N'Xuất kho' AS LoaiPhieu,
    px.NgayXuat AS NgayGiaoDich,
    cx.MaHang,
    hh.TenHang,
    cx.SoLuong,
    cx.DonGia,
    cx.ThanhTien,
    tk.HoTen AS NguoiLap
FROM PhieuXuat px
INNER JOIN ChiTietPhieuXuat cx ON px.MaPhieuXuat = cx.MaPhieuXuat
INNER JOIN HangHoa hh ON cx.MaHang = hh.MaHang
INNER JOIN TaiKhoan tk ON px.NguoiLap = tk.TenDangNhap;
GO