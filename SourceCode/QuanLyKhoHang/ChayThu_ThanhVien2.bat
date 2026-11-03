@echo off
chcp 65001 > nul
echo =========================================================================
echo ĐỒ ÁN CUỐI KỲ: LẬP TRÌNH TRÊN WINDOWS - QUẢN LÝ KHO HÀNG
echo CHẠY THỬ CÁC TÍNH NĂNG CỦA THÀNH VIÊN 2:
echo 1. Tính tồn kho qua Stored Procedure
echo 2. Tra cứu lịch sử nhập xuất theo khoảng ngày
echo 3. Xuất file CSV (Chuẩn UTF-8 mở Excel không lỗi font)
echo 4. Vẽ biểu đồ nhập xuất tồn bằng GDI+ (Cột nhóm ^& Đường)
echo 5. In phiếu kho bằng PrintDocument (Có PrintPreview)
echo =========================================================================
echo.
echo Đang biên dịch mã nguồn...

set CSC_PATH=%SystemRoot%\Microsoft.NET\Framework64\v4.0.30319\csc.exe

"%CSC_PATH%" /nologo /target:winexe /out:"QuanLyKhoHang\Test_ThanhVien2.exe" /main:QuanLyKhoHang.ProgramThanhVien2 /r:System.dll,System.Core.dll,System.Data.dll,System.Drawing.dll,System.Windows.Forms.dll,System.Xml.dll,System.Configuration.dll "QuanLyKhoHang\Database.cs" "QuanLyKhoHang\BieuDoGdiHelper.cs" "QuanLyKhoHang\CsvHelperCustom.cs" "QuanLyKhoHang\PhieuKhoPrintHelper.cs" "QuanLyKhoHang\FrmThongKeLichSu.cs" "QuanLyKhoHang\ProgramThanhVien2.cs"

if %ERRORLEVEL% EQU 0 (
    echo Biên dịch thành công! Đang khởi chạy ứng dụng...
    start "" "QuanLyKhoHang\Test_ThanhVien2.exe"
) else (
    echo [LỖI] Không thể biên dịch! Vui lòng kiểm tra lại.
    pause
)
