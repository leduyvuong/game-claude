# Máy chủ local — Harvest Isle

Bản thử nghiệm chạy trên chính máy Windows đang mở Unity. Không cần thuê dịch vụ. Nông trại online và bản lưu solo tách biệt; không tự chuyển tiền hoặc kho giữa hai chế độ.

## Chạy

Máy hiện tại đã có Node 24.15.0 và trình biên dịch .NET Framework của Windows. Từ thư mục gốc project:

```powershell
powershell -ExecutionPolicy Bypass -File Server/start-local.ps1
```

Script biên dịch toàn bộ Core C# vào `.tools/server/CoreWorker.exe`, rồi chạy Node ẩn cửa sổ tại `http://127.0.0.1:8787`. Nếu cổng đang dùng, script giữ nguyên tiến trình đó. Kiểm tra `GET /health`; log nằm trong `.tools/server/server.log` và `server-error.log`.

Để dừng, dùng `Stop-Process -Id <PID>` với đúng PID máy chủ do script vừa in ra, sau khi kiểm tra tiến trình đó vẫn là Node của project này. Dữ liệu nằm trên đĩa và được mở lại lần chạy sau. Thay đổi Core cần biên dịch lại; thay đổi `server.mjs` cần khởi động lại tiến trình.

Trong Unity, Play scene `HarvestIsle` → **Menu → Online local → Tạo tài khoản**. Tên gồm 3–24 chữ ASCII/số/gạch dưới, mật khẩu 8–128 ký tự. Sau đó chọn **Về nông trại** để chơi tài khoản online. Mở lại Online local để xem mã mời, nhập mã đảo, tải danh sách bạn và tưới/thoát nước giúp họ. **Trở lại nông trại solo** đăng xuất và khôi phục farm solo.

Máy chủ chỉ lắng nghe loopback: bản này dùng Editor hoặc client trên cùng PC, chưa kết nối từ điện thoại/LAN/Internet. Mật khẩu được băm scrypt; token lưu dạng băm ở máy chủ và chỉ giữ trong bộ nhớ client. Chưa có khôi phục mật khẩu. Không dùng mật khẩu tài khoản thật khác cho thử nghiệm.

## Dữ liệu và luật chơi

- SQLite: `Server/.data/harvest.sqlite`, không đưa vào Git. Khi sao lưu, dừng máy chủ rồi sao chép cả thư mục `.data` để giữ cả WAL nếu còn. Không xóa dữ liệu khi cập nhật code.
- Mỗi tài khoản khởi đầu 120 coins (client hiển thị 120.000đ), 24 ô và đảo riêng. Mã mời ghép nhóm tối đa 10 người; mỗi người vẫn có farm riêng.
- Client gửi lệnh, máy chủ chạy chung Core C# để kiểm tra chi phí, kho, mở khóa và thời gian. Client không được ghi đè farm hoặc tự chấm điểm thu hoạch.
- Gieo/tưới/thu/bán, rãnh thoát nước, chăn nuôi gà/bò, xây/nâng cấp, chế biến, đơn hàng, quà ngày, nhật ký, vườn ươm và nhà kính đã có lệnh.
- Mỗi lệnh có mã duy nhất; gửi lại không trừ tiền/nhận thưởng lần hai. Nếu mất kết nối sau khi gửi, mở Online local và chọn thử lại lệnh chưa xác nhận.
- Giúp bạn chỉ gồm tưới và thoát nước, tối đa 10 lượt thành công/ngày, cộng 2/4 điểm. Không được thu hoạch hoặc tiêu tiền hộ bạn.
- Cây và sản xuất dùng giờ máy chủ. Sinh thái chỉ tiến từng đoạn ngắn khi client hoạt động; chưa có mô phỏng thiên tai offline hoặc mùa dùng chung.
- Thu hoạch online hiện dùng chất lượng chuẩn; mini-game có thưởng vẫn ở solo, chưa có xác minh mini-game trên máy chủ.

## API và kiểm tra

`POST /api/register`, `/api/login`, `/api/logout`; `GET /api/state`, `/api/island`, `/api/visit/{userId}`; `POST /api/join` và `/api/command`. Ngoài health/register/login, các đường dẫn yêu cầu header Bearer. Xem `test-local.mjs` để có ví dụ yêu cầu và kiểm tra hợp đồng thực tế.

```powershell
powershell -ExecutionPolicy Bypass -File Server/start-local.ps1 -BuildOnly
node Server/test-local.mjs
powershell -ExecutionPolicy Bypass -File Tools/test-core.ps1
```

31 kiểm tra máy chủ đã qua: tài khoản, quyền đảo, giúp bạn, gửi trùng/đồng thời, chống sửa giờ/kho, mở khóa, xây dựng, chế biến, chăn nuôi và lưu qua restart. Test dùng cổng 18787, DB riêng trong `.tools/server/test-*`, đồng hồ thử và tự dừng tiến trình. Không sửa dữ liệu người chơi. Bridge Unity `online-check` kiểm tra đăng ký, gieo/tưới, mua gà, quà ngày, chống nhận trùng, đăng xuất/đăng nhập và giữ farm solo; tài khoản `review_*` là tài khoản kiểm tra riêng.

Đây là nền backend local, chưa phải hệ thống vận hành công khai: xử lý lệnh nối tiếp, khởi chạy worker mỗi lệnh, giới hạn 10.000 lệnh/tài khoản thử. Chưa có avatar realtime, chat, giao dịch/quà, đơn làng/công trình chung, quản trị, triển khai HTTPS, giám sát hoặc kiểm thử tải. Không mở cổng ra Internet ở trạng thái này.
# Bổ sung giao diện giao dịch — 14/10/2026

Hai lệnh đã thêm vào `/api/command`: `sellStock` dùng `id`, `tool` (phẩm chất 1–3), `job` (số lượng); `choiceOrder` dùng `index` (lựa chọn 0–2), `job` (serial bằng ordersCompleted hiện tại). Core kiểm tra tồn kho, giới hạn tiền và serial trước khi trừ. Request ID vẫn dùng để chống gửi lặp. Các lệnh cũ giữ tương thích. Bộ kiểm tra local hiện có **35 checks**, gồm bán đúng số lượng, retry không bán lần hai, số lượng âm và serial đơn cũ. Quy tắc sinh thái online chưa đổi theo chế độ tạm dừng menu của solo.

