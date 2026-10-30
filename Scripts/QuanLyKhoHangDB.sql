CREATE DATABASE QuanLyKhoHangDB;
GO
USE QuanLyKhoHangDB;
GO

-- 1. Bảng Người dùng (Phục vụ đăng nhập và phân quyền: QuanLy / NhanVienKho)
CREATE TABLE NguoiDung (
    MaNguoiDung VARCHAR(20) PRIMARY KEY,
    TenDangNhap VARCHAR(50) NOT NULL UNIQUE,
    MatKhau VARCHAR(100) NOT NULL,
    HoTen NVARCHAR(100) NOT NULL,
    VaiTro NVARCHAR(30) NOT NULL CHECK (VaiTro IN (N'Quản lý', N'Nhân viên kho')),
    TrangThai BIT DEFAULT 1
);

-- 2. Bảng Loại hàng
CREATE TABLE LoaiHang (
    MaLoai VARCHAR(20) PRIMARY KEY,
    TenLoai NVARCHAR(100) NOT NULL UNIQUE,
    MoTa NVARCHAR(255)
);

-- 3. Bảng Nhà cung cấp
CREATE TABLE NhaCungCap (
    MaNCC VARCHAR(20) PRIMARY KEY,
    TenNCC NVARCHAR(150) NOT NULL,
    DienThoai VARCHAR(15) NOT NULL,
    DiaChi NVARCHAR(255)
);

-- 4. Bảng Hàng hóa
CREATE TABLE HangHoa (
    MaHang VARCHAR(20) PRIMARY KEY,
    TenHang NVARCHAR(150) NOT NULL,
    MaLoai VARCHAR(20) NOT NULL FOREIGN KEY REFERENCES LoaiHang(MaLoai),
    DonViTinh NVARCHAR(30) NOT NULL,
    DonGiaNhap DECIMAL(18,2) NOT NULL CHECK (DonGiaNhap >= 0),
    SoLuongTon INT NOT NULL DEFAULT 0 CHECK (SoLuongTon >= 0),
    MucTonToiThieu INT NOT NULL DEFAULT 10,
    HinhAnh NVARCHAR(255) NULL
);

-- 5. Bảng Phiếu kho (Dùng chung cho Phiếu Nhập và Phiếu Xuất)
CREATE TABLE PhieuKho (
    MaPhieu VARCHAR(20) PRIMARY KEY,
    LoaiPhieu NVARCHAR(10) NOT NULL CHECK (LoaiPhieu IN (N'Nhập', N'Xuất')),
    NgayLap DATETIME NOT NULL DEFAULT GETDATE(),
    MaNguoiDung VARCHAR(20) NOT NULL FOREIGN KEY REFERENCES NguoiDung(MaNguoiDung),
    MaNCC VARCHAR(20) NULL FOREIGN KEY REFERENCES NhaCungCap(MaNCC),
    GhiChu NVARCHAR(255),
    TongTien DECIMAL(18,2) NOT NULL DEFAULT 0
);

-- 6. Bảng Chi tiết phiếu
CREATE TABLE ChiTietPhieu (
    MaPhieu VARCHAR(20) NOT NULL FOREIGN KEY REFERENCES PhieuKho(MaPhieu),
    MaHang VARCHAR(20) NOT NULL FOREIGN KEY REFERENCES HangHoa(MaHang),
    SoLuong INT NOT NULL CHECK (SoLuong > 0),
    DonGia DECIMAL(18,2) NOT NULL CHECK (DonGia >= 0),
    ThanhTien AS (SoLuong * DonGia),
    PRIMARY KEY (MaPhieu, MaHang)
);
GO

-- 7. Stored Procedure lấy danh sách hàng hóa kèm tên loại và cảnh báo tồn thấp
CREATE PROCEDURE sp_LayDanhSachHangHoa
    @TuKhoa NVARCHAR(100) = N'',
    @MaLoai VARCHAR(20) = ''
AS
BEGIN
    SELECT 
        h.MaHang,
        h.TenHang,
        h.MaLoai,
        l.TenLoai,
        h.DonViTinh,
        h.DonGiaNhap,
        h.SoLuongTon,
        h.MucTonToiThieu,
        h.HinhAnh,
        CASE 
            WHEN h.SoLuongTon <= h.MucTonToiThieu THEN N'Cảnh báo tồn thấp'
            ELSE N'Bình thường'
        END AS TrangThaiTon
    FROM HangHoa h
    INNER JOIN LoaiHang l ON h.MaLoai = l.MaLoai
    WHERE (@TuKhoa = N'' OR h.MaHang LIKE '%' + @TuKhoa + '%' OR h.TenHang LIKE N'%' + @TuKhoa + '%')
      AND (@MaLoai = '' OR h.MaLoai = @MaLoai)
    ORDER BY h.MaHang ASC;
END;
GO

-- 8. Stored Procedure thống kê Nhập - Xuất - Tồn theo từng mặt hàng (Phục vụ vẽ biểu đồ GDI+)
CREATE PROCEDURE sp_ThongKeNhapXuatTon
    @TuNgay DATETIME,
    @DenNgay DATETIME
AS
BEGIN
    SELECT 
        h.MaHang,
        h.TenHang,
        ISNULL(SUM(CASE WHEN p.LoaiPhieu = N'Nhập' AND p.NgayLap BETWEEN @TuNgay AND @DenNgay THEN ct.SoLuong ELSE 0 END), 0) AS TongNhap,
        ISNULL(SUM(CASE WHEN p.LoaiPhieu = N'Xuất' AND p.NgayLap BETWEEN @TuNgay AND @DenNgay THEN ct.SoLuong ELSE 0 END), 0) AS TongXuat,
        h.SoLuongTon AS TonHienTai
    FROM HangHoa h
    LEFT JOIN ChiTietPhieu ct ON h.MaHang = ct.MaHang
    LEFT JOIN PhieuKho p ON ct.MaPhieu = p.MaPhieu
    GROUP BY h.MaHang, h.TenHang, h.SoLuongTon;
END;
GO

INSERT INTO NguoiDung VALUES ('ND01', 'admin', '123456', N'Nguyễn Văn Quản Lý', N'Quản lý', 1);
INSERT INTO NguoiDung VALUES ('ND02', 'nvkho', '123456', N'Trần Thị Thủ Kho', N'Nhân viên kho', 1);

INSERT INTO LoaiHang VALUES ('LH01', N'Thiết bị điện tử', N'Linh kiện, máy tính, màn hình');
INSERT INTO LoaiHang VALUES ('LH02', N'Phụ kiện văn phòng', N'Chuột, bàn phím, cáp kết nối');

INSERT INTO NhaCungCap VALUES ('NCC01', N'Công ty TNHH Công nghệ Phong Vũ', '0901234567', N'Quận 1, TP.HCM');
INSERT INTO NhaCungCap VALUES ('NCC02', N'Nhà phân phối Digiworld', '0909888777', N'Quận 3, TP.HCM');

INSERT INTO HangHoa VALUES ('HH01', N'Màn hình Dell UltraSharp 24 inch', 'LH01', N'Cái', 4500000, 25, 10, '');
INSERT INTO HangHoa VALUES ('HH02', N'Bàn phím cơ Logitech G Pro', 'LH02', N'Cái', 2100000, 5, 10, ''); -- Dòng này sẽ kích hoạt cảnh báo đỏ do SoLuongTon (5) <= MucTonToiThieu (10)
INSERT INTO HangHoa VALUES ('HH03', N'Chuột không dây Razer DeathAdder', 'LH02', N'Cái', 850000, 40, 15, '');
GO