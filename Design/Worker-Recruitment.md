# Nhân công và lật thẻ tuyển dụng

Bản triển khai ngày 25/10/2026. Mở **Menu → Phát triển → Chăm đảo → Nhân công**, hoặc chạm Nhà nhân công đã xây. Đây là cơ chế solo; nhân viên quán lẩu vẫn là đội riêng.

## Vòng chơi

1. Xây Nhà nhân công. Cấp 1/2/3 mở 2/4/6 chỗ làm, tính cả nhân công cũ. Danh sách đã tuyển chứa tối đa đủ 72 người, không bị giới hạn bởi số chỗ.
2. Thuê đúng người trong danh mục, hoặc dùng xu kiếm trong game để lật thẻ. Tuyển người chưa tự động phân công hoặc mua ca.
3. Mua ca 10 việc, rồi phân công. Mỗi người chỉ thực hiện một việc thành công mỗi nhịp của mình. Hết ca thì dừng; không tự trừ tiền hay tự gia hạn.
4. Chuyên gia cây ưu tiên và mặc định chỉ thu cây đúng nghề. Bật **Thu mọi cây** cho phép thu cây khác khi không có cây sở trường chín, với kỹ năng 0/3.
5. Có thể cho nghỉ, thay người, giữ lại việc trả trước và huấn luyện bằng tài liệu nhận từ thẻ trùng.

## Giá và năng lực

| Bậc | Số người | Thuê đích danh | Ca 10 việc | Nhịp làm cơ bản | Kỹ năng thu đúng nghề | Tài liệu khi trùng |
|---|---:|---:|---:|---:|---:|---:|
| Phổ thông | 30 | 120 xu | 4 xu | 24 giây | 0/3 | 1 |
| Lành nghề | 19 | 360 xu | 9 xu | 20 giây | 1/3 | 3 |
| Chuyên gia | 14 | 900 xu | 18 xu | 12 giây | 2/3 | 8 |
| Bậc thầy | 9 | 1.800 xu | 30 xu | 8 giây | 3/3 | 20 |

Tiền thuê đích danh trả một lần để sở hữu hồ sơ. Giá một ca bao gồm công lao động; thợ chăn nuôi bậc 3–4 còn dùng **1 ngô thật trong kho cho mỗi lần cho cả đàn ăn**. Ong dùng điều kiện hoa/đậu như luật sẵn có. Tưới, chăm đất không tự mua thuốc hay vật tư bằng tiền của người chơi.

Không có việc, kho đầy hoặc thao tác thất bại thì giữ nguyên lượt. Có thể dự trữ tối đa 100 việc/người. Không làm bù nhiều lượt khi tải lại game; không sinh thưởng offline. Khi đọc bảng, chơi minigame hoặc ở quán, đội farm tạm dừng. Thời hạn chờ theo đồng hồ UTC vẫn trôi, nên sau khi quay lại người đủ thời gian có thể làm một việc ngay.

Kỹ năng thu đi qua luật `Harvest` thật: tăng sản lượng và có thể nhận hàng 2–3 sao. Bậc thầy không bảo đảm 3 sao nếu ruộng còn sâu, bệnh, úng hoặc đất chưa tốt. Ví dụ Chị Hồng Cà Chua và Ông Tư Lúa Mì đạt kỹ năng 3/3 trên đúng cây; người mới cùng nghề dùng 0/3. Thu cây khác luôn 0/3, kể cả đã huấn luyện.

Huấn luyện bậc 1 tốn 8 tài liệu + 60 xu; bậc 2 tốn thêm 20 tài liệu + 180 xu. Mỗi bậc giảm 4 giây/việc, sàn 4 giây. Bậc 2 cộng 1 kỹ năng thu đúng nghề, trần 3. Thợ tưới/chăm đất/chăn nuôi nhận lợi ích tốc độ; huấn luyện không mở năng lực riêng của độ hiếm cao hơn.

## Mười hai người đa năng

| Nghề | Lành nghề | Chuyên gia | Bậc thầy | Công việc |
|---|---|---|---|---|
| Miệt vườn | Anh Vườn | Cô Sáu Miệt Vườn | Lão Vườn Bốn Mùa | Thu cả xoài, dừa, cam, chanh, chuối, đu đủ; dùng kỹ năng theo bậc |
| Tưới tiêu | Chị Mưa | Bác Thủy Lợi | Thầy Nước An Bình | Bậc 2 tưới cây đang lớn; bậc 3–4 còn thoát úng cho cây cạn |
| Chăm đất | Anh Đất | Cô Út Sinh Thái | Bà Đất Lành | Bậc 2 bắt sâu; bậc 3 thêm giảm 20 bệnh; bậc 4 giảm 35 bệnh và cải đất từng điểm đến 2. Ưu tiên sâu → bệnh → đất |
| Chăn nuôi | Chị Mến | Chú Sáu Chuồng Trại | Ông Tám Chăn Nuôi | Bậc 2 thu sản phẩm sáu loài; bậc 3–4 thêm cho ăn/thả ong, tốc độ khác nhau |

Thợ thu chỉ thu cây chín, không tự gieo lại. Cây lâu năm tiếp tục tái quả theo luật cũ. Các nhân công quét luân phiên vị trí để tránh luôn làm một ô đầu tiên; nhiều người không thể nhận cùng một vụ thu hai lần. Thợ chuyên môn chạy trước đội hợp đồng cũ trong mỗi lượt.

## Gacha lật thẻ

- 30 xu/lượt; gói 5 là 150 xu, cùng bảng tỷ lệ và cùng bộ đếm. Không bán lượt bằng tiền thật.
- Tỷ lệ cơ bản trước bảo đảm: **60% / 28% / 10% / 2%** theo bốn bậc. Trong một bậc, mọi hồ sơ có xác suất ngang nhau. Xác suất cơ bản từng người bằng tỷ lệ bậc chia 30 / 19 / 14 / 9.
- Tối đa 10 lượt liên tiếp không có bậc 3+: lượt kế tiếp trong mốc đó nâng kết quả thấp lên Chuyên gia. Gặp Chuyên gia hoặc Bậc thầy thì bộ đếm này về 0.
- Tối đa 50 lượt liên tiếp không có Bậc thầy: lượt thứ 50 chắc chắn Bậc thầy. Bảo đảm này ưu tiên cao hơn bảo đảm 10; Bậc thầy đặt lại cả hai bộ đếm.
- Các bảo đảm không yêu cầu người mới, vẫn có thể ra thẻ trùng. Tỷ lệ thực sau bảo đảm khác tỷ lệ cơ bản.
- Thẻ trùng đổi ngay thành tài liệu theo bảng. Không tạo bản sao nhân công, không nhân đôi chỗ làm; tài liệu dùng chung để chủ động chọn người cần học.
- Kết quả, tiền, hồ sơ, tài liệu và bộ đếm được chốt cùng giao dịch trước hiệu ứng lật. Lật chỉ mở mặt thẻ; không trao thưởng lần nữa. Chưa lật hết thì chưa được mua gói tiếp. Đóng bảng không mất kết quả.

## Dữ liệu và giới hạn

`FarmState.recruitment` nối cuối schema 1. Save cũ thiếu object hoặc Unity tạo object rỗng đều khởi tạo danh sách mới, giữ tiền và các `WorkerState` cũ. Hợp đồng cũ giữ mức hiệu quả/miễn ca như trước, vẫn chiếm chỗ; UI không bán thêm hợp đồng cũ. Kết thúc hợp đồng cũ cần xác nhận, không hoàn cọc.

Core dùng RNG do caller cung cấp; runtime dùng `RandomNumberGenerator` với rejection sampling. Serial chặn xử lý lại gói/reveal, kiểm tra tiền trước khi trừ. Đây là save solo trên máy, không phải bảo đảm chống sửa file hay quay lui bản sao save. Server chưa nhận lệnh tuyển, lật thẻ hoặc phân công mới; UI và caller runtime chặn online.

72 hồ sơ dùng lại các prefab/portrait nam, nữ và Già Tư hiện có; chưa tạo 72 ngoại hình riêng. Nhân công được phân công xuất hiện ở nhà và di chuyển minh họa đến vị trí công việc sau giao dịch Core. Chưa có tìm đường né công trình, IK bám đất hoặc benchmark đội sáu người trên điện thoại. Giá hiện là bản cân bằng đầu tiên, chưa kiểm chứng bằng nhiều mùa chơi dài hạn.

Kiểm tra: `Tools/test-core.ps1`; server local; bridge Play `recruitment-check`, `farm-check`, `play-interface-check`, `development-ui-preview`, `online-check`. Ảnh trong `Art/Previews/Farm/Workers`, kết quả trong `.tools/model-work/recruitment-result.json`.

## Sáu mươi chuyên gia cây trồng

Mỗi hàng gồm hai hồ sơ riêng; người mới luôn Phổ thông, người nhiều kinh nghiệm có bậc ghi bên cạnh.

| Cây | Người mới | Người nhiều kinh nghiệm | Bậc |
|---|---|---|---|
| Cà rốt | An Nhi | Cô Sáu Cà Rốt | Lành nghề |
| Ngô | Bảo Nam | Chú Bảy Bắp | Chuyên gia |
| Bí đỏ | Bé Mận | Dì Năm Bí | Chuyên gia |
| Lúa | Tấn Lộc | Ông Ba Lúa | Bậc thầy |
| Cải ngọt | Diệu Vy | Cô Tám Cải | Lành nghề |
| Cà chua | Minh Khang | Chị Hồng Cà Chua | Bậc thầy |
| Hành | Hồng Anh | Chú Tư Hành | Lành nghề |
| Đậu leo | Quốc Bảo | Cô Lan Đậu | Lành nghề |
| Dưa hấu | Thảo My | Anh Hậu Dưa | Chuyên gia |
| Mía | Đức Tài | Bác Mười Mía | Chuyên gia |
| Đậu Hà Lan | Trúc Linh | Chị Hạnh Hà Lan | Lành nghề |
| Dâu tây | Gia Hân | Cô Dâu Đỏ | Chuyên gia |
| Cúc vạn thọ | Ngọc Mai | Dì Cúc Vàng | Chuyên gia |
| Ớt | Hữu Phúc | Chú Chín Ớt | Lành nghề |
| Sả | Khánh An | Cô Hương Sả | Lành nghề |
| Rau muống | Thanh Thủy | Dì Thủy Rau | Lành nghề |
| Khoai môn | Tuấn Kiệt | Bác Tín Khoai | Lành nghề |
| Sen | Liên Chi | Bà Sen Hồng | Bậc thầy |
| Nấm | Hoài Sơn | Chú Lâm Nấm | Chuyên gia |
| Củ cải | Bích Ngân | Cô Ngọc Củ Cải | Lành nghề |
| Bắp cải | Việt Hùng | Bác Bình Bắp Cải | Lành nghề |
| Lúa mì | Hải Đăng | Ông Tư Lúa Mì | Bậc thầy |
| Trà tuyết | Tuyết Nhi | Bà Tuyết Trà | Chuyên gia |
| Tỏi | Phương Thảo | Chị Tỏi Trắng | Lành nghề |
| Xoài | Nhật Minh | Ông Năm Xoài | Bậc thầy |
| Dừa | Kim Oanh | Chú Dừa Nước | Chuyên gia |
| Cam | Thanh Tâm | Cô Cam Ngọt | Chuyên gia |
| Chanh | Bảo Trân | Anh Chanh Xanh | Lành nghề |
| Chuối | Hoàng Long | Dì Chuối Mật | Lành nghề |
| Đu đủ | Ánh Dương | Bác Đu Đủ | Lành nghề |
