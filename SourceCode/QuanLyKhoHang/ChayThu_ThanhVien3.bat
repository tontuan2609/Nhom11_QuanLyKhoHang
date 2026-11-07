@echo off
chcp 65001 > nul
echo =========================================================================
echo ĐỒ ÁN CUỐI KỲ: LẬP TRÌNH TRÊN WINDOWS - QUẢN LÝ KHO HÀNG
echo CHẠY THỬ CÁC TÍNH NĂNG CỦA THÀNH VIÊN 3 (CHUẨN MÔ HÌNH MVP):
echo 1. Tính tồn kho qua Stored Procedure
echo 2. Tra cứu lịch sử nhập xuất theo khoảng ngày
echo 3. Xuất file CSV (Chuẩn UTF-8 mở Excel không lỗi font)
echo 4. Vẽ biểu đồ nhập xuất tồn bằng GDI+ (Cột nhóm ^& Đường)
echo 5. In phiếu kho bằng PrintDocument (Có PrintPreview)
echo =========================================================================
echo.
echo Đang biên dịch mã nguồn theo kiến trúc MVP (Models - Views - Presenters - Services)...

set "PROJECT_ROOT=%~dp0"
set "SOURCE_ROOT=%PROJECT_ROOT%QuanLyKhoHang"
set "CSC_PATH=%SystemRoot%\Microsoft.NET\Framework64\v4.0.30319\csc.exe"

"%CSC_PATH%" /nologo /target:winexe /out:"%PROJECT_ROOT%Test_ThanhVien3.exe" /main:QuanLyKhoHang.ProgramThanhVien3 /r:System.dll,System.Core.dll,System.Data.dll,System.Drawing.dll,System.Windows.Forms.dll,System.Xml.dll,System.Configuration.dll "%SOURCE_ROOT%\Database.cs" "%SOURCE_ROOT%\Models\ThongKeModels.cs" "%SOURCE_ROOT%\Services\BieuDoGdiHelper.cs" "%SOURCE_ROOT%\Services\CsvHelperCustom.cs" "%SOURCE_ROOT%\Services\PhieuKhoPrintHelper.cs" "%SOURCE_ROOT%\Views\IThongKeLichSuView.cs" "%SOURCE_ROOT%\Views\FrmThongKeLichSu.cs" "%SOURCE_ROOT%\Presenters\ThongKeLichSuPresenter.cs" "%SOURCE_ROOT%\ProgramThanhVien3.cs"

if %ERRORLEVEL% EQU 0 (
    echo Biên dịch thành công! Đang khởi chạy ứng dụng...
    start "" "%PROJECT_ROOT%Test_ThanhVien3.exe"
) else (
    echo [LỖI] Không thể biên dịch! Vui lòng kiểm tra lại.
    pause
)
