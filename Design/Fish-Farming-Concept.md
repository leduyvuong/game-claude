# Thiết kế nuôi cá và vận hành hồ

Ngày 09/10/2026. **Trạng thái: đề xuất gameplay, chưa triển khai.** Các mức tiền, thời gian, công suất và xác suất dưới đây là thông số thử nghiệm cho Harvest Isle, không phải hướng dẫn nuôi hoặc chữa bệnh cá thực tế.

Người chơi xây hồ, chuẩn bị nước, chọn giống, bố trí lọc và sục khí, chăm đàn, xử lý sự cố rồi thu hoạch phục vụ chợ và tiệm lẩu. Thành công đến từ chọn mật độ phù hợp với thiết bị và giữ môi trường ổn định. Hồ đẹp phải thể hiện được cách vận hành: dòng nước ở miệng lọc, bọt khí ở đáy, cá tụ về chỗ cho ăn, nước đục khi dư thức ăn.

## 1. Nền hiện có và phạm vi thay đổi

Trong `FarmLivestock.cs`, cá hiện là `LivestockState`: mua 60 coins/con, tối đa ba con; mỗi lượt trả 5 coins/con hoặc một ngô cho cả đàn; sau 140 giây thu hai cá tươi/con. Cá giống không mất sau thu. `fishStock` là kho cá tươi chung, bán 8 coins/đơn vị và được dùng bởi chế biến, Hành trình, quán lẩu. Quy đổi UI hiện tại: 1 coin = 1.000đ.

`FarmRanch` dựng hồ ở khoảng (9.5, 2.1) trên mặt phẳng x/z. Project đã có `Environment/Pond`, model Koi/Carp/Perch và `FarmFishSwim`. Hồ trang trí của thế giới không mặc nhiên là hồ sản xuất; không biến cá trang trí thành hàng thu hoạch.

Hệ mới chuyển sang **nuôi theo lứa**: mua giống, nuôi lớn, thu thì trừ đàn. Bản đầu mỗi hồ một loài và một lứa để thao tác cảm ứng rõ ràng; ghép đàn, cá săn mồi và phối giống chưa nằm trong bản đầu. Koi thuộc nhánh cá cảnh, không vào thực phẩm.

## 2. Vòng chơi và nhịp học

1. Mở nghề ở cấp farm 3, xem khu hồ qua Menu → Phát triển → Thủy sản.
2. Chọn vị trí cố định được dành riêng trên đảo, xem bóng công trình, giá và diện tích rồi xây.
3. Cấp nước, gắn thiết bị, chạy ổn định 30 giây chơi; bảng báo sức chứa khuyến nghị.
4. Mua cá giống và thả một lứa. UI dự báo tải khi cá trưởng thành trước khi xác nhận.
5. Chọn khẩu phần một lần cho cả vụ, cho ăn thủ công hoặc dùng máy. Theo dõi nước và hành vi cá.
6. Vệ sinh lọc, hút cặn, thay nước vừa đủ; cách ly và điều trị khi có bệnh.
7. Thu cá đạt cỡ, chuyển về kho, bán hoặc nhập bếp. Vệ sinh hồ giữa hai lứa rồi tiếp tục.

Vụ hướng dẫn dùng sáu cá rô, kéo dài khoảng bốn phút trong điều kiện tốt, có sục khí cơ bản và bảo vệ khỏi bệnh. Người chơi được học cho ăn, hút cặn và thu; chỉ mở bệnh sau khi hoàn tất vụ này. Vụ thường khoảng 6–12 phút tăng trưởng hiệu dụng, vẫn tiếp tục khi vắng mặt theo mục 10.

## 3. Xây hồ và mở rộng

| Công trình | Điều kiện | Giá xây hoặc nâng từng bước | Sức chứa trưởng thành | Vai trò |
| --- | --- | --- | --- | --- |
| Hồ vườn cấp 1 | Farm cấp 3 | 60.000đ + 10 gỗ | 12 điểm tải | Học nuôi, sáu cá rô hoặc bốn cá chép |
| Hồ vườn cấp 2 | Farm cấp 5, thu hai lứa | 180.000đ + 20 gỗ + 10 quặng | 24 điểm tải | Nuôi thương phẩm, thêm ổ thiết bị |
| Hồ vườn cấp 3 | Farm cấp 7, thu tám lứa khỏe | 450.000đ + 35 gỗ + 25 quặng | 40 điểm tải | Nhiều cá hơn, lọc và cảnh quan lớn |
| Bể cách ly | Farm cấp 4, có hồ | 45.000đ + 8 gỗ | 12 điểm tải | Chữa bệnh và theo dõi cá mới |
| Hồ thứ hai | Farm cấp 6, thu năm lứa | 120.000đ + 20 gỗ + 10 quặng | Khởi đầu cấp 1 | Tách loài và tách rủi ro |
| Hồ thứ ba | Farm cấp 9, thu 15 lứa | 240.000đ + 30 gỗ + 20 quặng | Khởi đầu cấp 1 | Chuyên môn hóa sản xuất |

Giới hạn bản đầu: ba hồ sản xuất, một bể cách ly. Vị trí hồ bổ sung phải khảo sát địa hình và camera trước khi chốt; không lấn ruộng, phố hoặc chuồng. Hồ trang trí vẫn giữ chức năng trang trí.

Xây gồm xem trước → xác nhận thanh toán → thi công 30/60/90 giây chơi theo cấp → cấp nước. Cho phép hủy trước thanh toán. Nâng cấp chỉ khi hồ trống; kiểm tra đủ tiền/vật liệu trước khi trừ. Không phá hồ có cá. Trang trí bờ hồ dùng ô gắn cố định, không cho chặn đường nước hay che nút thao tác.

Mỗi hồ có các ổ riêng cho lọc, sục khí, máy cho ăn, cảm biến và cảnh quan. Nâng cấp thay thiết bị trong cùng ổ; không cộng chồng vô hạn nhiều máy rẻ.

## 4. Loài cá và đầu ra

| Loài | Vai trò chơi | Điểm tải mỗi con trưởng thành | Thời gian chuẩn | Giống mỗi con | Thu mỗi con khỏe |
| --- | --- | --- | --- | --- | --- |
| Cá rô | Dễ khởi đầu, chịu điều kiện kém tốt hơn trong luật game | 2 | 360 giây | 2.000đ | 2 đơn vị cá tươi |
| Cá chép | Vụ dài, hiệu suất tốt nếu nước ổn định | 3 | 480 giây | 4.000đ | 3 đơn vị cá tươi |
| Cá koi | Sưu tập màu và bán cá cảnh | 4 | 720 giây | 12.000đ | 1 cá cảnh, giá cơ sở 32.000đ |

Một đơn vị cá tươi là đơn vị kho game, không quy đổi kg. Mỗi cá có bốn giai đoạn hiển thị: giống, cá nhỏ, đang lớn, đạt cỡ. Kích thước model đổi theo tiến độ; thông số mô phỏng lưu theo nhóm cá cùng trạng thái, không cần một đối tượng Core cho từng cá bơi.

Rô và chép trước mắt cùng về `fishStock` thường để không phá chuỗi cá tươi/viên cá/lẩu đang có. Điểm chăm sóc quyết định cá đạt chuẩn và thành tích; chưa cộng sao vào kho cá chung. Koi lưu kho cá cảnh riêng, bán bằng thao tác riêng, tuyệt đối không bị nút Bán nông sản hoặc nhập bếp lấy đi.

Mua giống hiển thị nguồn, giá, tải trưởng thành và sức khỏe. Bản đầu giống mua luôn khỏe, không có bệnh ẩn ngẫu nhiên ngay khi mua. Sau này nếu thêm nguồn giống giá rẻ thì phải công khai rủi ro trước giao dịch.

## 5. Lọc nước và sục khí

**Lọc cơ học** gom phân và thức ăn dư. **Lọc sinh học** giảm chỉ số độc nước. **Sục khí** hồi oxy. Ba công dụng độc lập: nước nhìn trong vẫn có thể thiếu oxy; bật máy khí không xóa cặn.

| Thiết bị | Giá bước nâng | Công dụng đề xuất |
| --- | --- | --- |
| Lọc cấp 1 | 18.000đ | Công suất 12 điểm tải; chủ yếu gom cặn |
| Lọc cấp 2 | 55.000đ | 24 điểm tải; thêm ngăn sinh học |
| Lọc cấp 3 | 130.000đ | 40 điểm tải; sinh học mạnh và báo bẩn |
| Sục khí cấp 1 | 12.000đ | Phục vụ 12 điểm tải |
| Sục khí cấp 2 | 38.000đ | Phục vụ 24 điểm tải |
| Sục khí cấp 3 | 90.000đ | Phục vụ 40 điểm tải; chế độ tự tăng khi oxy thấp |
| Máy cho ăn | 35.000đ | Phân phối khẩu phần đã dự trữ, tự dừng khi cá bệnh |
| Cảm biến | 25.000đ | Cảnh báo sớm và lịch sử nước; chỉ số cơ bản vẫn xem miễn phí |
| Mái che | 20.000đ | Giảm tác động nắng và mưa, đổi hình hồ |

Sức chứa khuyến nghị bằng mức thấp nhất trong giới hạn hồ, lọc và máy khí. Chặn thả nếu tổng tải trưởng thành vượt giới hạn này; không đánh lừa bằng tải thấp của cá giống. Thiết bị có thể xuống hiệu suất sau khi thả, làm hồ quá tải tạm thời và phát cảnh báo.

Lọc có thanh bẩn 0–100: dưới 70 chạy đủ công suất, 70–89 còn 75%, từ 90 còn 50%. Vệ sinh 10 giây chơi, miễn tiền, không tháo máy và không xóa bệnh ngay. Lõi sinh học có mức ổn định riêng; thay toàn bộ lõi làm giảm mức ổn định, UI ưu tiên nút rửa ngăn cơ học để tránh phạt người mới.

Không thêm mạng điện toàn đảo ở bản đầu. Chi phí chạy máy được hiển thị và trả trong gói vận hành của lứa trước khi thả; không tự tạo nợ trong lúc người chơi bận ở quán. Hết lứa không trừ phí. Sục khí dự phòng khẩn cấp là thao tác miễn phí có hồi 60 giây chơi, hồi 20 oxy, giúp người hết tiền vẫn cứu được cá.

## 6. Mô phỏng nước

UI chính chỉ có **Oxy, Độ sạch, Độc nước, Sức khỏe đàn**. Tab chi tiết thêm nhiệt, pH, stress và độ bẩn lọc. Nhiệt/pH chỉ là thang điểm môi trường game, không hiển thị như phép đo nuôi thực tế.

| Chỉ số | Khoảng an toàn | Vùng cần xử lý | Tác động |
| --- | --- | --- | --- |
| Oxy 0–100 | 60–100 | Dưới 40; nguy cấp dưới 20 | Ăn kém, chậm lớn, cá ngoi mặt |
| Độ sạch 0–100 | 70–100 | Dưới 50 | Tăng stress và áp lực bệnh |
| Độc nước 0–100 | 0–20 | Trên 40; nguy cấp trên 70 | Tổn thương sức khỏe dù nước còn trong |
| Độ lệch nhiệt 0–100 | 0–20 | Trên 40 | Giảm ăn và tăng stress |
| Độ lệch pH 0–100 | 0–20 | Trên 40 | Stress sau biến động nước |
| Stress 0–100 | 0–25 | Trên 60 | Giảm lớn và sức chống bệnh |

Core cập nhật bước cố định 5 giây; mọi hệ số tính theo phút rồi nhân `dt/60`. Đặt `L = tải sinh khối hiện tại / sức chứa hồ`, tải mỗi cá tăng từ 40% lên 100% theo tiến độ; giới hạn thả vẫn dùng tải trưởng thành. `U` là thức ăn dư chuẩn hóa 0–1; `M/B/A` là hiệu suất lọc cơ học/sinh học/sục khí 0–1 sau bẩn và tương quan công suất.

Thông số khởi đầu để mô phỏng cân bằng:

- Oxy/phút: `+10A + 2 − 6L − 3U − đêm(2) − nóng(2)`, chặn 0–100.
- Cặn/phút: `+5L + 8U − 7M`; độ sạch = `100 − cặn`.
- Độc/phút: `+3L + 5U − 6B`; lọc cấp 1 có B = 0,25 khi ổn định, cấp 2–3 tối đa 1.
- Bẩn lọc tăng theo lượng cặn thực tế giữ lại; không tăng khi hồ trống. Tốc độ chốt sau mô phỏng, mục tiêu tối đa một lần rửa mỗi lứa thường.
- Nhiệt và pH dịch chuyển từ từ về điểm cân bằng theo thời tiết và mái che. Tác động ngày đêm đọc qua dữ liệu Core do runtime cung cấp, không lấy màu ánh sáng làm luật.

Mưa nhẹ không được xem là luôn có lợi: thêm nước nhưng mưa lớn đưa cặn vào và làm tăng độ lệch môi trường. Biến động tối đa mỗi phút phải được chặn để chu kỳ thời tiết ngắn của farm không gây bệnh liên tục. Lứa đầu bỏ qua các tác động này.

Thay 20% nước làm cặn và độc còn 80%; tốn 2.000đ, hồi thao tác 30 giây chơi. Thay 50% còn 50%, tốn 4.000đ nhưng thêm 15 stress. Hút cặn miễn tiền trong 10 giây, giảm 15 cặn, hồi 60 giây. Không cho bấm lặp để nhân hiệu quả trong cùng thao tác; nước sạch không tự khôi phục cá đang bệnh.

## 7. Thức ăn và tăng trưởng

Trước khi thả, mua và dự trữ khẩu phần đủ cả lứa. Giá chuẩn 4.000đ/cá rô, 6.000đ/cá chép, 8.000đ/koi. Khẩu phần chia ba bữa theo các mốc tăng trưởng 0%, 35%, 70%; mua trọn gói nhưng chỉ trừ lượng phân phối vào từng lần cho ăn. Phần chưa dùng được giữ trong kho thức ăn khi kết thúc hoặc hủy lứa.

Người chơi có thể dùng thức ăn chế biến ở farm khi có công thức thủy sản riêng. Không dùng trực tiếp luật một ngô nuôi cả đàn của hệ cũ. Công thức quy đổi phải giữ chi phí cơ hội, tránh đàn lớn ăn với giá cố định.

Chế độ vừa đủ là mặc định. Cho thêm bữa tạo thức ăn dư, không tăng trưởng vượt 110% tốc độ chuẩn; máy tự dừng khi còn đồ ăn. Khi qua mốc cần bữa mới mà chưa cho ăn, cá dừng lớn nhưng không chết đói. Người chơi thấy rõ “Thiếu bữa 2” thay vì đồng hồ hoàn tất giả.

`tiến độ tăng = dt / thời gian chuẩn × hệ số nước × hệ số sức khỏe × hệ số thức ăn`. Các hệ số nằm trong 0–1,1; trạng thái nguy cấp hoặc bệnh nặng dừng lớn. Chất lượng chăm sóc là trung bình theo thời gian tăng trưởng, không lấy chỉ số nước tại đúng lúc bấm thu.

Thu một phần cho phép chọn nhóm khỏe đã đạt cỡ; nhóm bệnh hoặc chưa đủ thời gian sau điều trị bị khóa. Bản đầu cá tươi đạt chuẩn giữ giá 8.000đ/đơn vị; koi có hệ số chăm sóc 1–1,5. Không thưởng thêm sản lượng bằng cách chuyển đi chuyển lại giữa hồ.

## 8. Bệnh và sự cố chi tiết

Tên bệnh dưới đây phục vụ mô phỏng hoạt hình. Dùng vật phẩm điều trị hư cấu, không ghi tên hoạt chất hay liều dùng thực tế.

| Tình trạng | Điều kiện gameplay | Dấu hiệu trên cá và hồ | Xử lý đúng | Xử lý không hiệu quả |
| --- | --- | --- | --- | --- |
| Thiếu oxy | Oxy dưới 40 kéo dài | Cá tập trung mặt nước, há miệng, biểu tượng bọt khí đỏ | Sục khí, ngừng cho dư, giảm tải | Dùng gói trị nấm |
| Sốc nước | Thay nhiều nước hoặc độ lệch môi trường cao | Bơi giật, tách đàn, stress tăng nhanh | Ổn định môi trường, mái che, ngừng thay nước lớn | Thay toàn hồ liên tiếp |
| Nấm ngoài da | Nước bẩn cùng stress cao | Mảng bông trắng cách điệu trên một số cá | Cách ly, làm sạch, gói chăm sóc nấm | Chỉ tăng thức ăn |
| Đốm trắng | Áp lực ký sinh và stress cao | Chấm trắng, cọ mình, bơi khó chịu | Theo dõi toàn đàn, vệ sinh, liệu trình ký sinh | Thu con nhìn khỏe ngay để né kiểm dịch |
| Tổn thương vây | Độc nước cao và vệ sinh kém kéo dài | Vây sẫm, mép vây co, bơi chậm | Giảm độc, lọc sinh học, gói phục hồi vây | Chỉ làm nước nhìn trong |
| Rối loạn tiêu hóa | Cho dư nhiều lần | Bỏ ăn, lờ đờ, bụng phồng nhẹ | Dừng bữa thừa, hút cặn, đổi về khẩu phần chuẩn | Tiếp tục cho ăn tăng lực |

Thiếu oxy, sốc nước và rối loạn tiêu hóa là sự cố không lây trong luật bản đầu. Nấm, đốm trắng và tổn thương vây là ba nhóm bệnh có thể lan trong cùng hồ theo quy tắc game. Mỗi nhóm cá chỉ có một bệnh chính để UI dễ hiểu; vẫn có thể chịu stress môi trường đồng thời.

**Quy trình phát bệnh:** mỗi 30 giây chơi, tích lũy phơi nhiễm khi nước ngoài vùng an toàn. Sau ít nhất 60 giây có cảnh báo, bắt đầu xét bệnh bằng hash của seed farm, ID hồ, ID lứa và chỉ số lần xét. Xác suất mỗi lượt = `min(15%, 2% × số yếu tố xấu + 1% × floor(stress/20))`. Không xét khi không có yếu tố xấu hoặc trong vụ hướng dẫn. Lưu chỉ số lần xét để tải lại không đổi kết quả.

**Diễn tiến:** khỏe → có dấu hiệu → nhẹ → nặng → hồi phục. Mức bệnh 0–100, nhẹ 1–39, vừa 40–69, nặng 70 trở lên. Môi trường xấu tăng 5 điểm/phút; phù hợp nhưng chưa chữa giữ mức; liệu trình đúng và môi trường tốt giảm 15 điểm/phút. Nặng giảm sức khỏe 5 điểm/phút; nước nguy cấp giảm thêm 5 điểm/phút. Hồi phục chỉ kết thúc khi bệnh bằng 0 và sức khỏe ít nhất 70.

Mỗi 30 giây, bệnh lây có tối đa một nhóm khỏe mới bị ảnh hưởng; xác suất đề xuất `min(20%, 5% × tỷ lệ cá bệnh × hệ số nước)` với hệ số nước 1–3. Cá đã chuyển cách ly không đóng góp vào tỷ lệ của hồ gốc. Hồ khác dùng hệ nước riêng nên không lây tự động qua khoảng cách.

**Kiểm tra cá:** miễn phí, mất 5 giây chơi; công bố vấn đề và nguyên nhân đã quan sát được. Không bắt người chơi đoán thuốc bằng màu cá. Khi nhiều nguyên nhân cùng tồn tại, ưu tiên thiếu oxy → độc nước → bệnh; thẻ hướng dẫn nêu thao tác đầu tiên và lý do.

**Điều trị:** một gói cho tối đa sáu cá trong bể cách ly, chi phí dự kiến 3.000–6.000đ/gói; bệnh nhiều hơn cần `ceil(số cá/6)` gói. Liệu trình tối thiểu 180 giây chơi và kéo dài đến khi bệnh về 0, tự chạy sau khi bắt đầu; nếu môi trường chưa đạt thì tạm dừng hiệu quả và giải thích. Sức khỏe hồi 10 điểm/phút khi nước tốt và bệnh dưới 40. Không trừ thêm tiền mỗi tick hoặc yêu cầu mua lại gói vì liệu trình kéo dài. Dùng sai gói bị chặn ngay trước giao dịch.

Sau liệu trình cần 60 giây chơi theo dõi, không bán/nhập bếp cá đang điều trị hoặc còn thời gian theo dõi. Đồng hồ này được ghi save, không xóa khi chuyển hồ. Bệnh toàn đàn có thể chữa tại hồ với cùng chi phí theo số cá, vì bể cách ly không đủ chỗ cho mọi trường hợp.

**Mức phạt:** chế độ mặc định không chết cá. Sức khỏe về 10 thì cá suy kiệt, dừng lớn và khóa thu. Luôn có nút dưỡng hồi miễn phí: tách nhóm hoặc đóng hồ để dưỡng, ổn định nước rồi giảm bệnh 5 điểm/phút; không cần vật phẩm, không sản xuất trong thời gian này. Sức khỏe hồi theo quy tắc trên, nên người hết tiền vẫn cứu được đàn. Không cộng bệnh/phạt thêm trong lúc vắng mặt. Nếu sau này có chế độ hao hụt, phải là lựa chọn riêng được thông báo trước; không đưa chết cá vào cập nhật âm thầm.

## 9. Cách ly và vệ sinh giữa lứa

Bể cách ly chứa tối đa 12 điểm tải trưởng thành, chỉ nhận một nhóm cùng loài/cùng bệnh trong mỗi lượt. Khi chuyển, giữ ID lứa, tiến độ, số cá, bệnh, thời gian điều trị và chất lượng tích lũy. Chuyển là giao dịch nguyên tử: kiểm tra đích rồi mới trừ nguồn; không đủ chỗ thì cả hai không đổi.

Không cho chuyển cá bệnh vào hồ khỏe. Có thể chuyển nhóm đã hồi phục về hồ gốc nếu còn sức chứa đã dự trữ cho nhóm đó; UI hiển thị rõ “đang nuôi” và “chỗ dành cho cá cách ly”. Không dùng chỗ này để mua giống mới rồi khiến cá không có nơi trở về.

Thu hết lứa mở nút vệ sinh 15 giây chơi, miễn phí; xóa cặn bệnh còn lại, giữ một phần hệ sinh học. Bỏ qua vệ sinh vẫn phải hiện cảnh báo và tăng áp lực bệnh lứa sau; vụ hướng dẫn yêu cầu vệ sinh để dạy vòng chơi đầy đủ. Nếu không còn tiền mua giống, nhiệm vụ hỗ trợ cho một lứa nhỏ không bán lại được ngay, chỉ cấp khi không có cá đang nuôi và chưa nhận hỗ trợ của lần thất bại đó.

## 10. Khi vắng mặt và khi mở màn khác

Khi đóng game, mô phỏng tối đa tám giờ từ mốc UTC đã ghi. Chỉ tăng trưởng đến bữa ăn tiếp theo nếu không có máy; có máy thì dùng khẩu phần thật đã dự trữ. Cá đủ cỡ dừng, không tự thu, không mua/thả vụ tiếp theo.

Chất lượng nước dùng trạng thái lúc rời game để tính tốc độ bảo thủ, không tự tốt lên; không tích lũy bệnh mới, hỏng máy hoặc thiệt hại. Cá bệnh dừng lớn và dừng liệu trình khi offline, hiện rõ khi quay lại. Điều này tránh thoát game để chữa tức thì, đồng thời không phạt việc nghỉ chơi.

Trong phiên, bảng Thủy sản đang mở vẫn mô phỏng để người chơi thấy kết quả; khi chuyển vào quán lẩu, gallery, minigame hoặc ứng dụng xuống nền thì dùng cùng quy tắc bảo vệ vắng mặt. Không để đàn bị hại trong lúc UI không cho xử lý. Một khoảng thời gian chỉ thuộc một chế độ; chống tính kép giữa realtime và offline.

Đồng hồ quay lùi cho delta = 0, tải lại không nhận tăng trưởng lần hai. Đồng hồ tiến bất thường vẫn bị trần tám giờ và giới hạn một lứa. Đây là bảo vệ save local, chưa phải chống gian lận online.

## 11. Giao diện và hình ảnh

Chạm hồ mở bảng cạnh phải, giữ hồ 3D ở bên trái. Đầu bảng là tên hồ, loài, số cá, sức chứa và nút thu; kế tiếp bốn chỉ số nước chính. Tab dưới gồm Đàn cá / Nước / Thiết bị / Sức khỏe. Không dồn mọi thông số lên HUD farm.

Thông báo có mức ưu tiên, biểu tượng và chữ; không chỉ dựa màu. Ví dụ: “Oxy 28 — máy khí yếu khi đàn lớn. Sục khí khẩn cấp ngay; nâng máy sau vụ này.” Nút điều trị nêu tổng giá, số cá và thời gian khóa thu trước khi bấm.

Nút chạm mục tiêu ít nhất 48dp sau quy đổi Canvas; thao tác bán koi, thu cả đàn và thay 50% nước có bước xem hậu quả rõ ràng. Chặn raycast xuyên UI; kéo camera và chọn cá không đồng thời kích hoạt giao dịch.

Hình ảnh dùng model cá hiện có, bổ sung vỏ lọc ba cấp, máy khí ba cấp, ống, máy cho ăn, bể cách ly và mái che. Đây là **asset cần làm**, chưa nằm trong danh mục đã bake. Bọt khí và dòng nước dùng particle tiết chế; bệnh dùng overlay/material và kiểu bơi, không tạo cảnh tổn thương ghê rợn.

Chỉ hiển thị tối đa 12 cá đại diện/hồ ở gần và bốn ở xa; số lượng thật nằm trong Core. Bơi theo đường cong rẻ, không Rigidbody từng cá; bóng, particle và mặt nước phải có mức chất lượng điện thoại. Ngân sách hiệu năng chỉ chốt sau profile Android/iOS.

## 12. Kinh tế và tiến trình

Gói vận hành đề xuất 1.000đ/điểm tải trưởng thành/vụ. Ví dụ vụ sáu cá rô khỏe: giống 12.000đ + thức ăn 24.000đ + vận hành 12.000đ = 48.000đ. Thu 12 cá tươi × 8.000đ = 96.000đ; lãi vận hành 48.000đ trước thay nước/điều trị, chưa tính công trình. Nếu tốn một lần thay nước 2.000đ và một gói điều trị 6.000đ thì còn 40.000đ.

So với hệ cũ, thời gian dài hơn và cá giống bị tiêu hao, nên phải đo lợi nhuận/phút, số thao tác/vụ và vốn ban đầu song song với trồng cây. Mục tiêu trải nghiệm: vụ thường cần 3–5 thao tác, xử lý nước tối đa một lần nếu đúng mật độ; bệnh là hậu quả có thể phòng, không phải phí ngẫu nhiên bắt buộc.

Tay nghề thủy sản có mốc 5/20/50/100 lứa khỏe, mở họa tiết hồ, skin thiết bị và đơn hàng; không nhân sản lượng vô hạn. Hành trình thêm hợp đồng “thu lứa mới”, “giữ nước tốt”, “cung cấp cá cho bếp” sau khi hệ đã mở. Chỉ tính hoạt động sau khi nhận và hàng thật, không truy ngược từ `fishStock` cũ.

Koi có bộ sưu tập màu hữu hạn rõ ràng; chưa thêm gacha hay lai giống. Hồ chuyên cá thực phẩm và hồ cảnh tạo mục tiêu khác nhau, không bắt nuôi koi để mở nguyên liệu lẩu.

## 13. Core và tương thích save

Các tên dự kiến: `AquacultureState`, `FishPondState`, `FishBatchState`, `PondEquipmentState`, `FishDiseaseRules`, `FarmAquacultureModel`; runtime `FarmAquaculture`, `FishPondView`, `FishHealthVisuals`. Luật nước, tiền, tăng trưởng và bệnh nằm trong Core thuần C#; runtime truyền thời gian/thời tiết và dựng hình.

Save gồm version/initialized, ID hồ/lứa không trùng, cấp hồ, thiết bị, trạng thái nước, số cá theo nhóm, tiến độ, sức khỏe, bệnh, thức ăn dự trữ, liệu trình, chỗ cách ly dự trữ, chỉ số xét bệnh, mốc thời gian đã tính và lịch sử chăm sóc tích lũy. Giới hạn số hồ, số nhóm, tiền/kho/counter theo guard hiện hữu; chặn âm, NaN, vô cực, ID lỗi và tham chiếu vòng.

Kế hoạch nối field cuối schema 1 chỉ được chốt sau kiểm tra serializer hiện tại. Phân biệt thiếu/null/object placeholder cũ với dữ liệu mới bị hỏng bằng `initialized` và version; không normalize mọi dữ liệu lỗi thành farm trống.

Migration đề xuất theo hai giai đoạn:

1. Có cá cũ thì giữ nguyên vòng đang chạy và kho cá. Hiện thông báo chuyển sang hồ mới sau khi nhận sản phẩm; nếu đang nghỉ giữa hai lượt thì chuyển ngay, không bắt mua thêm thức ăn. Chưa có cá thì mở hệ mới theo cấp bình thường.
2. Sau thu cuối, nhận miễn phí hồ cấp 1, lọc/khí cấp 1 và đổi mỗi con cũ thành một quyền giống chép tái sử dụng: quyền này tự cấp một cá giống miễn phí mỗi vụ, tối đa số con cũ (0–3). Lưu quyền riêng, không thể bán; giữ lợi ích sở hữu vĩnh viễn của hệ cũ, không biến tài sản mua trước thành một vụ dùng hết. Giữ quyền cho save cấp thấp và không khóa lại vì điều kiện mở nghề mới.

Nếu ba cá chép chiếm chín điểm tải thì vẫn vừa hồ cấp 1. Sau commit migration, chặn `BuyAnimal/FeedAnimal/CollectAnimal("fish")` trên save đã chuyển để không có hai nguồn cá; cập nhật UI, Hành trình và bridge đang đọc `State.fish.count`. API cũ vẫn xử lý save chưa chuyển. `fishStock` và nguyên liệu `fish` giữ nguyên.

Koi, cá đang nuôi và cá tươi là ba loại dữ liệu riêng. Thu kiểm tra kho đích trước, chỉ thêm kho và trừ đàn cùng một giao dịch; đầy kho thì không đổi gì. Bán, chữa, chuyển, nâng cấp phải chống nhấn lặp. Online bị chặn cho hệ mới đến khi có command và validation server tương ứng.

## 14. Các đợt triển khai và điều kiện đạt

**Đợt 1 — vòng nuôi hoàn chỉnh:** một hồ có ba cấp, rô/chép, xây và thiết bị, khẩu phần, nước/oxy, tăng trưởng/thu, kho cá cũ, migration. Hoàn thành khi chơi được hai vụ liên tiếp và save/reload không mất tiền/cá.

**Đợt 2 — sức khỏe:** stress, ba nhóm bệnh, sự cố không lây, cách ly, chữa tại hồ, liệu trình, cảnh báo và bảo vệ vắng mặt. Hoàn thành khi người chơi xác định được nguyên nhân và cứu được đàn kể cả lúc hết tiền.

**Đợt 3 — chiều sâu:** nhiều hồ, koi và kho cá cảnh, máy/cảm biến, hợp đồng Hành trình, trang trí và tay nghề. Sau đó cân bằng dài hạn, profile điện thoại và sửa UI theo thử cảm ứng.

Kiểm tra bắt buộc khi triển khai:

- Đủ/thiếu tiền và vật liệu; sức chứa tính lúc trưởng thành; nâng hồ có cá bị chặn.
- Lọc không thay thế sục khí; sục khí không xóa độc; cho dư tạo cặn; đúng mật độ giữ nước ổn định qua nhiều vụ.
- Bệnh tất định qua reload, không có bệnh trong vụ đầu, không lây sang hồ độc lập, không nhân cá khi cách ly.
- Bệnh/điều trị/thời gian theo dõi giữ qua chuyển hồ và restart; cá bệnh không vào kho thực phẩm.
- Thu hai lần, kho đầy, bán koi, nhập bếp và chế biến không làm âm hoặc nhân kho.
- Save cũ ở cả ba trạng thái chưa nuôi/đang nuôi/chờ thu; quyền cá giống cũ nhận đúng một lần; JSON placeholder của Unity.
- Offline 0/1/8/48 giờ, giờ lùi, chuyển màn, pause/resume và crash giữa giao dịch không tính thời gian hai lần.
- Mô phỏng ít nhất 100 vụ từng loài và từng cấp thiết bị, cả chăm tốt/cho dư/bỏ bữa; ghi lợi nhuận, tỷ lệ bệnh, số thao tác và thời gian cứu đàn.
- Bridge đề xuất `aquaculture-check` chạy farm cô lập, chụp hồ sạch/bẩn/thiếu oxy/cách ly/thu và xác nhận save farm thật không đổi. Chỉ công bố đạt khi bridge và tests thực sự chạy.
