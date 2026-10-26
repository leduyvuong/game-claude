# Chiều sâu gameplay và tiến trình dài hạn

## Hành trình bốn mùa — concept và phần đã triển khai 08/10/2026

Mục tiêu thiết kế: sau khi mở đủ 30 giống/36 ô/công trình cấp 3, người chơi vẫn có lựa chọn về hướng sản xuất và mục tiêu tiếp theo. Tiền là tài nguyên vận hành; tay nghề và huy hiệu phải đến từ hoạt động chơi. Không đặt lại farm để ép chơi lại. Không biến nghỉ chơi thành mất phần thưởng. Một năm Hành trình là bốn mùa do người chơi hoàn thành, không phải một năm dương lịch; không khẳng định số giờ chơi hay khả năng giữ chân nhiều năm khi chưa có dữ liệu chơi thử.

### Vòng chơi đã có trong bản này

**Menu → Phát triển → Hành trình**, mở hợp đồng từ cấp 3. Có ba đề nghị cùng lúc, nhận một; hoàn tất sẽ tạo bộ tiếp theo. Mỗi mùa cần 12 hợp đồng, người chơi chủ động tổng kết nhận 12 huy hiệu. Bốn chủ đề nối tiếp Mầm xanh → Nắng vàng → Mùa quả → Bếp ấm, rồi sang năm kế tiếp. Chủ đề đổi nhóm cây ưu tiên trong đề nghị đầu; không đổi khí hậu hay cấm trồng cây khác. Năm mới giữ nguyên ruộng, kho, tiền, công trình, tay nghề và các dự án. Không có hạn chót hoặc giới hạn lượt theo ngày.

| Hướng hợp đồng | Hoạt động tính sau lúc nhận | Hàng phải giao |
| --- | --- | --- |
| Vụ mùa mới | 3–8 vụ của một cây đã mở | 6–16 nông sản, dùng thường rồi ★★ rồi ★★★ |
| Đơn hàng xưởng | Chế biến và thu 2–4 món đã có chuỗi nguyên liệu khả dụng | 2–4 thành phẩm tương ứng |
| Nông sản ba sao | 2–4 vụ đúng cây đạt ★★★ | 3–8 nông sản ★★★, không lấy phẩm chất khác |
| Bếp làng sum họp | Phục vụ 3–8 nồi qua luật Tiệm lẩu hiện có | Không trừ thêm nguyên liệu khi nhận thưởng |

Đề nghị xưởng chỉ xuất hiện khi xưởng và các xưởng nguyên liệu đã xây, giống nguyên liệu đã mở; trứng/sữa cần đang sở hữu vật nuôi tương ứng. Chưa phù hợp thì thay bằng hợp đồng cây. Đề nghị bếp luân phiên với chất lượng khi đã có món trên thực đơn. Khối lượng tăng nhẹ theo năm và dừng tăng từ năm 6; không tăng mũ để kéo dài thời gian chờ. Có thể hủy không mất tiền/hàng, nhưng mất tiến độ hợp đồng và nhận lại bắt đầu tính mới. UI yêu cầu nhấn hủy lần hai.

Kho dự trữ không thay thế hoạt động mới. Cây đã trồng trước lúc nhận vẫn tính nếu thu sau lúc nhận; sản phẩm đang chế trước đó tính khi thu sau lúc nhận. Đây là lựa chọn thiết kế để việc chuẩn bị có ích. Hợp đồng không tách riêng hàng mới/cũ trong kho. Serial tăng mỗi lần nhận, chặn hoàn tất lại/hủy nhầm hợp đồng mới; tiền đầy hoặc thiếu hàng thì không trừ hàng và không cộng tiến độ. Phần thưởng cố định tại lúc nhận, nên nâng tay nghề/bến sau đó áp dụng cho hợp đồng sau.

Tiền thưởng coins = ceil((80 + số hàng × giá đơn vị × 1,3 + số lượt mới × 12) × (1 + thưởng bến + thưởng tay nghề)). Hàng ba sao dùng giá ba sao. Vụ mùa/xưởng/chất lượng hoặc bếp trả lần lượt 3/4/5 huy hiệu, cùng 20 XP. Huy hiệu không mua bằng tiền; không có monetization trong hệ này.

### Những thứ giữ lại lâu dài

- **30 tay nghề độc lập:** số vụ thu thành công của từng cây, gồm tái quả cây lâu năm. Mốc 10/50/150/400/1.000 vụ cho bậc 1–5; mỗi bậc tăng 2% tiền hợp đồng cây tương ứng, tối đa 10%. Đếm từ bản cập nhật vì save cũ không lưu lịch sử theo từng cây; không suy đoán phân bổ từ tổng thu hoạch cũ. Sau bậc 5 vẫn lưu số vụ.
- **Ba dự án đảo, mỗi dự án năm cấp:** Xưởng làng giảm 4% thời gian chế biến/cấp; Vườn giống giảm 4% thời gian ươm/cấp; Bến giao thương tăng 4% tiền hợp đồng/cấp. Mỗi lợi ích tối đa 20%; không cộng dồn vô hạn làm mất cân bằng. Cấp N tốn 12×N huy hiệu và 300×N² coins. Chỉ áp dụng cho lượt mới, không sửa thời điểm hoàn tất các hàng chờ cũ. Đây là nâng cấp luật, không thêm model công trình.
- **Kỷ niệm mùa, danh tiếng và di sản:** giữ tổng số mùa đã tổng kết và danh tiếng từ số hợp đồng. Sau bốn mùa mở đầu tư di sản, mỗi bậc là ghi danh vĩnh viễn trên màn Hành trình, không buff sức mạnh. Giá 24 + min(bậc hiện tại,20)×2 huy hiệu và 1.000 + min(bậc hiện tại,100)×250 coins. Đây là nơi tiêu huy hiệu tùy chọn khi đã hoàn thiện dự án, chưa có tượng 3D hoặc bộ trang trí độc quyền. Không lưu một danh sách lịch sử vô hạn; save lưu bộ đếm gọn.

### Hướng concept tiếp theo — chưa triển khai

Để duy trì hứng thú ngoài việc lặp hợp đồng, cần mở rộng ba lớp nội dung có khác biệt thực sự: (1) vùng đảo có địa hình và chuỗi nguyên liệu riêng, chuyến thám hiểm chọn rủi ro/phần thưởng; (2) bố trí trang trại tự do, lai/ghép giống và triển lãm bộ sưu tập để người chơi tự đặt mục tiêu; (3) đơn làng và công trình hợp tác khi backend online đã sẵn sàng. Mỗi đợt nội dung nên thêm một cách ra quyết định mới, thay vì chỉ tăng trần cấp hoặc giá. Không đưa các nhánh này vào UI như thể đã chơi được.

Thứ tự cân bằng: chơi thử farm thường (không dùng ví đầy của công viên), đo thời gian tới hợp đồng đầu/12 hợp đồng/đầu tư đầu tiên; theo dõi tỷ lệ bỏ từng loại hợp đồng, mức tồn kho và nguồn/tiêu tiền; điều chỉnh độ đa dạng và lựa chọn trước khi tăng thời gian cây lớn. Thời gian cây hiện vẫn là nhịp thử nghiệm ngắn. Hệ lặp không thay cho việc bổ sung nội dung và đo retention thực tế.

### Lưu và kiểm chứng

`FarmJourney.cs` độc lập Unity; `FarmState.journey` nối cuối schema 1, save cũ null được tạo mới sau validation. Validation giới hạn danh sách 30 cây/10 thành phẩm, giá trị âm/tràn, bản ghi trùng, thứ tự mùa và serial/baseline hợp đồng. UI solo-only, chưa thêm lệnh Hành trình lên server. Các bộ đếm kỹ thuật tối đa 1 tỷ; không mô tả là vô hạn toán học.

Core kiểm tra chuyển 40 mùa/480 hợp đồng bằng gieo–thu thật trong mô phỏng, khảo sát đề nghị 800 mùa, chống nhận trùng, giao đúng phẩm chất, chuỗi nguyên liệu, đầu tư và đồng hồ tiến/lùi. Bridge `journey-check` dùng farm cô lập để kiểm tra JSON/save/đọc lại hợp đồng và dự án, chặn thao tác xuyên UI, chụp các trạng thái; không sửa farm người chơi. Đây chưa phải thử nghiệm người chơi nhiều năm hoặc kiểm thử thiết bị.

## Mưa, úng, bệnh cây và thu hoạch

Bản chơi hiện tại áp dụng cho 24 ô và 30 giống, mở theo cấp 1–10. Ba giống khởi đầu giữ nguyên giá/thời gian. Đây là quy luật game, không phải mô hình nông học thực tế.

Tiền trong game hiển thị bằng đồng Việt Nam qua `FarmModel.Money`: 1 đơn vị coins trong save = 1.000đ (khởi đầu 120 coins hiển thị 120.000đ). Dữ liệu save và luật kinh tế vẫn tính bằng coins như cũ.

## Chu kỳ thời tiết và ô trũng

Một chu kỳ gồm 90 giây nắng, 100 giây mưa, 40 giây mưa lớn, 70 giây khô ráo. Dự báo trên HUD đếm ngược lần chuyển tiếp. Đồng hồ thời tiết và lượng nước được lưu; chỉ tiến khi đang chơi, tạm dừng trong trò thu hoạch. Thoát game vẫn cho cây lớn theo UTC nhưng không mô phỏng thêm thiệt hại thời tiết.

Hàng đầu và cột thứ hai/thứ năm là ô trũng, nhận nước nhanh hơn. Ô nào đang chọn được ghi rõ là ô trũng hay ô cao. Nước từ 55% hiện vũng đọng và trừ một nông sản; từ 85% trừ hai. Từ 65% chặn gieo mới. Phạt úng cộng với phạt bệnh, sản lượng cuối cùng tối thiểu một. Đây là phạt theo nước tại lúc thu: cứu úng trước khi thu khôi phục phần sản lượng do úng, nhưng bệnh đã phát sinh phải xử lý riêng. Nắng làm nước rút dần. Mưa tự cấp nước cho cây, cho cùng lợi ích giảm 35% thời gian như tưới tay.

Nút **Thoát nước** miễn phí hạ lượng nước xuống 5%, đồng thời tạo hiệu lực rãnh 45 giây giảm nước mưa chảy vào. Có thể đào trước trận mưa hoặc xử lý khi thấy nước dâng.

**Xây rãnh · 12.000đ** trong bảng ô đất tạo rãnh cố định cho riêng ô đó. Trả tiền một lần, giữ qua thu hoạch/luân canh và lưu game; không cần mua lại. Rãnh giảm 75% nước mưa chảy vào và rút thêm 0,5 điểm phần trăm nước mỗi giây mô phỏng. Khi đang úng, rãnh rút dần; kết hợp Thoát nước để cứu cây ngay. Rãnh không ngăn lây bệnh từ ô bên cạnh. UI hiển thị mức phạt úng/bệnh, sản lượng chuẩn dự kiến và trạng thái bảo vệ; model mương nhỏ nằm cạnh ô đã xây.

## Bệnh và cách xử lý

Bệnh có mức 0–100. Cà rốt có nguy cơ khi nước trên 55%, bí đỏ trên 62%, ngô trên 72%. Cây bệnh từ 35 điểm có thể lây qua cạnh chung sang cây bên cạnh nếu đất bên đó còn ẩm. Không lây chéo góc; một bước mô phỏng dùng trạng thái trước đó để tránh lây cả ruộng trong cùng một giây.

Từ 35 điểm, cây đổi model bệnh và mất một sản phẩm; từ 70 điểm đổi model héo và mất hai. Thu vẫn cứu được tối thiểu một sản phẩm. **Trị bệnh** tốn 3.000đ, giảm 50 điểm bệnh và ngăn tăng bệnh trong 45 giây. Kết hợp trị bệnh với thoát nước để hạn chế tái phát. Bệnh nhẹ giảm từ từ khi trời nắng và đất khô.

Ô nhớ hai vụ trước, phân cây thành Lá/Củ/Quả/Đậu/Hoa. Khác nhóm vụ trước cộng ceil(10% sản lượng gốc); ba nhóm khác nhau liên tiếp cộng ceil(20%). Đổi nhóm giảm nửa rủi ro bệnh; cùng nhóm ba vụ liên tiếp tăng gấp đôi và khi thu mất một bậc độ phì. Thu đậu hồi một bậc độ phì. Độ phì 0–2 cộng tương ứng 0–2 nông sản; chưa phải hệ thống nông sản 1–3 sao và giá bán theo sao. Thu hoạch giữ 40% mức bệnh như tàn dư trên ô đất.

Cà rốt–hành cạnh chung giảm 50% rủi ro bệnh; cúc vạn thọ bảo vệ tám ô xung quanh giảm 30%. Ngô–đậu–bí cùng xuất hiện trong lân cận 3×3 cộng ceil(15% sản lượng gốc). Sen, rau muống và khoai môn trồng được ô úng và không chịu phạt sản lượng/nấm do nước; vẫn có thể lây bệnh từ cây khác.

## Phát triển, chế biến và canh tác

`FarmDevelopmentModel` chứa XP, công trình, kho thành phẩm, hàng chờ, khay ươm, đơn hàng, quà ngày và năm trang nhật ký khởi đầu. Cấp = 1 + floor(XP/40), tối đa 10. Gieo +2, thu +8, nâng công trình/nhận thành phẩm +10, đơn hàng và trang nhật ký +20 XP. Đơn hàng cho thêm 3 gỗ/1 quặng; vật liệu dùng nâng công trình.

Máy cấp 1–3 có 1–3 chỗ hàng chờ nối tiếp, tốc độ 1×/1,25×/1,5×. Thu hàng giải phóng chỗ; không nhận trùng, không trừ đầu vào khi thiếu nguyên liệu/hết chỗ. Chuỗi Lúa mì → Bột → Bánh mì → Bánh kẹp; có thêm đường, mứt, bánh gạo, phô mai, bánh trứng. Thành phẩm bán riêng trong Kho đầy đủ; Bán toàn bộ trên HUD chỉ bán nông sản/trứng/sữa để không mất nguyên liệu chế biến vô ý.

Vườn ươm: 2 khay/cấp, trả giống trước, ươm 25% thời gian; gieo giống có khay sẵn sàng tự dùng cây con và bắt đầu ở 25%. Nhà kính phân bổ che mưa 4 ô/cấp, không tự tưới, không chữa úng có sẵn. Phân ủ tăng một bậc độ phì (tối đa 2); thuốc thảo mộc giảm 35 bệnh, bảo vệ 45 giây. UI nằm trong Menu → Phát triển đảo; chọn ô trên ruộng trước khi dùng tab Canh tác.

Đây chưa phải toàn bộ concept: chưa lịch mùa theo tuần, ghép cành, chất lượng sao, mở vùng đảo hay online đầy đủ. Xem bảng tiến độ đối chiếu trong AGENTS.md. Concept gốc được giữ tại Design/Concept-v0.5-original.txt; quyết định Unity 3D của người dùng thay thế phần web/2D trong tài liệu đó.

## Ba trò thu hoạch

Mỗi trò gồm ba lượt. Không tự thu khi chỉ chọn ô. Nút dừng giữ cây nguyên trên ruộng; hoàn tất mới ghi sản lượng và lưu game.

| Giống | Trò chơi | Kỹ năng |
| --- | --- | --- |
| Cà rốt | Nhổ khi vạch chuyển động vào vùng xanh | Canh nhịp ba lần |
| Ngô | Nhớ dãy ba bắp, sau khi dãy ẩn thì chọn đúng thứ tự | Ghi nhớ |
| Bí đỏ | Bấm Lấy đà rồi Cắt lúc lực vào vùng xanh | Canh thời gian tích lực |

0 lượt đúng trừ một sản phẩm; 1 lượt giữ mức chuẩn; 2 lượt cộng một; 3 lượt cộng hai. Sau đó cộng thưởng luân canh và trừ thiệt hại bệnh/úng, tối thiểu một sản phẩm. Mỗi năm vụ thu hoạch tăng một bậc khó, tối đa sáu bậc: thanh chạy nhanh hơn, vùng chuẩn hẹp hơn, thời gian xem dãy ngô ngắn hơn. Không yêu cầu thao tác giữ nhiều ngón.

## Khai hoang đất, tiệm lẩu và nhân công

**Nông sản theo sao**: mỗi lần thu hoạch được chấm phẩm chất. ★★★ khi đạt 3 lượt mini-game đúng, độ phì tối đa và ô sạch bệnh/úng/sâu; ★★ khi đạt từ 2 lượt và ô sạch; còn lại là hàng thường. Hàng ★★ bán ×1,35, ★★★ bán ×1,8 (làm tròn lên từng món), nằm kho riêng và bán kèm nút Bán hàng; chế biến và đơn hàng dùng hàng thường; tiệm lẩu nhập riêng từng phẩm chất vào kho bếp.

**Sâu phá hoại**: trời nắng và đất khô có thể sinh sâu trên ô đang trồng (hiện con ốc sên + nút đỏ "Bắt sâu"). Sâu trừ 1 nông sản mỗi vụ cho tới khi bị đuổi. Minigame: 3 con sâu lần lượt nhô lên ~1,2 giây — chạm kịp 2/3 con để đuổi sâu, 3/3 con được cộng bảo vệ 30 giây, +4 XP.

**Cấy mạ**: gieo giống đã có khay ươm sẵn sàng sẽ mở minigame canh vạch; trúng vùng xanh thì cây con bắt đầu ở 35% tiến độ thay vì 25%.

**Vịt và heo**: vịt 45.000đ/con, ăn 4.000đ/con, 80 giây ra 2 trứng; heo 120.000đ/con, ăn 8.000đ/con, 150 giây ủ ra 2 **phân ủ** (vào kho thành phẩm — bón ruộng tăng độ phì hoặc bán 25.000đ/món). Mỗi chuồng tối đa 3 con, dùng chung luật cho ăn/ngô với gà bò.

**Ong và ao cá**: tổ ong 150.000đ — thả ong **miễn phí** nhưng ruộng phải có ít nhất 2 ô đang trồng nhóm Hoa hoặc Đậu; 120 giây ra 2 mật ong/đàn (18.000đ/món). Khi có ong và có hoa nở, mọi vụ thu nhóm **Quả được +1 sản lượng** (ong thụ phấn). Ao cá 60.000đ, thức ăn 5.000đ/con, 140 giây ra 2 cá tươi (8.000đ/món). Mật và cá bán kèm nút Bán hàng.

**Nhiệm vụ tuần**: 3 nhiệm vụ sinh theo tuần (thu hoạch / nồi lẩu / thành phẩm / đơn hàng) với tiến độ tính từ đầu tuần; thưởng theo độ khó + 20 XP, mỗi nhiệm vụ nhận một lần, sang tuần tự làm mới. Xem ở đầu tab Đơn hàng.

**Lễ hội Đom Đóm**: cứ 10 phút lại có 2 phút lễ hội — mọi đơn Bán hàng +20% giá và khách tiệm lẩu đến nhanh gấp đôi; có thông báo khi mở/đóng hội.

**Thành tích**: 10 mốc (thu hoạch, bán hàng, nồi lẩu, khai hoang, nhân công, chế biến, đơn hàng) xem trong tab Nhật ký; mỗi mốc nhận một lần 20.000–200.000đ + 15 XP.

**Khai hoang**: hai khu đầu tab Xây dựng, mỗi khu mở thêm một hàng 6 ô phía nam ruộng (24 → 30 → 36 ô). Khu 1 cần cấp 3, thu hoạch 10 vụ, bán nông sản một lần và 100.000đ. Khu 2 cần cấp 5, thu hoạch 30 vụ, giao 5 đơn hàng và 280.000đ. Mở tuần tự, +10 XP mỗi lần; nhiệm vụ tính tích lũy từ đầu game, không trừ tiến độ. Giữ đất đã mở ở save cũ. Đất mới dùng chung mọi luật nước/bệnh/luân canh (cột 2 và 5 vẫn là ô trũng).

**Tiệm lẩu**: phòng ăn 3D; kho bếp riêng, bắt buộc chuyển nông sản từ farm. Nước dùng/gia vị và đạm chưa sản xuất được mua từ nhà cung cấp. Công thức do người chơi phối (2–6 nguyên liệu, ít nhất một hàng farm), phải nấu thử tiêu hao một nồi rồi đưa lên thực đơn. Chấm điểm tất định 0–100; nông sản theo sao nâng chất lượng. Giá = ceil(giá trị nguyên liệu theo sao × (1,35 + điểm × 0,006) × (1 + 0,1 × hạng)). Phục vụ +6 XP; điểm quán là trung bình nồi đã phục vụ. Khách chờ 60 giây, ăn 12 giây; bảng quản lý tạm dừng quán. Đơn khách chỉ giữ trong phiên. Xem bảng cập nhật 08/10/2026 trong README về tầng, bàn và giới hạn bản đầu.

**Hạng quán lẩu**: tổng số nồi đã phục vụ được lưu vào save. Đạt 15 nồi quán lên hạng ★★ (mở bàn 5), 40 nồi lên ★★★ (mở bàn 6); mỗi hạng cộng 10% vào giá mọi nồi.

**Cây lâu năm**: sáu cây ăn quả (xoài, dừa, cam, chanh, chuối, đu đủ) không biến mất khi thu hoạch — cây giữ nguyên trên ô và từ vụ thứ hai chỉ cần **60% thời gian** để ra quả tiếp (vẫn giảm thêm 35% nếu tưới). Cây lâu năm không tính vào luân canh hay độ phì; muốn đổi giống phải **Chặt cây lâu năm** trong tab Canh tác (không hoàn tiền, cây được ghi vào lịch sử vụ của ô).

**Nhân công**: xây Nhà nhân công trong tab Nhân công (cấp 1/2/3 giá 60.000/150.000/300.000đ + gỗ/quặng). Nhà cấp N chứa N người và người mới thuê mang cấp N. Tiền cọc thuê: Tạp vụ 40.000đ, Thợ trồng trọt 65.000đ, Thợ chăn nuôi 90.000đ. Mỗi 4 giây mỗi người làm một việc khi đang mở game: Tạp vụ chỉ tưới nước; Thợ trồng trọt tưới và từ cấp 2 tự thu hoạch (điểm kỹ năng chuẩn, không thưởng mini-game); Thợ chăn nuôi thu trứng/sữa và từ cấp 2 cho đàn ăn bằng ngô trong kho. Nhân công không làm việc khi thoát game.

## Chăn nuôi gà và bò

Bấm **Chăn nuôi** hoặc chạm chuồng trên đảo. Hai chuồng ban đầu miễn phí, trống; vật nuôi phải mua bằng tiền. Mỗi chuồng chứa tối đa ba con. Gà/bò trong hai khu chuồng là những con đã mua; những loài còn lại vẫn là cảnh trang trí.

| Vật nuôi | Giá mua/con | Thức ăn/con/lượt | Thời gian | Sản lượng/con | Giá bán/món |
| --- | --- | --- | --- | --- | --- |
| Gà | 30.000đ | 3.000đ | 60 giây | 2 trứng | 5.000đ |
| Bò | 75.000đ | 6.000đ | 100 giây | 2 sữa | 12.000đ |

Có thể dùng **1 ngô trong kho cho cả đàn** thay tiền thức ăn. Vì vậy nuôi nhiều con giúp sử dụng ngô hiệu quả hơn. Ngô được trừ khi cho ăn, không phải khi thu sản phẩm.

Đói → cho ăn → chờ sản xuất → thu vào kho → bán. Một lần cho ăn tạo đúng một lứa, vẫn tiến triển khi thoát game. Sản phẩm chờ thu không tự biến mất. Sau khi thu cần cho ăn lại; không sinh vô hạn khi offline. Không mua thêm con khi đàn đang sản xuất để tránh thay đổi sản lượng giữa lứa. Chưa có bệnh vật nuôi, sinh sản hay nâng cấp chuồng.

### Kiểm tra chăn nuôi

`ranch-check` trong Unity Play dùng bản lưu riêng, mua một gà và một bò, trả thức ăn, chờ theo đồng hồ thử, thu và bán; kiểm tra model, tiền, save/reload và chặn thu trùng. Báo cáo `.tools/model-work/ranch-result.json`, ảnh trong `Art/Previews/Farm`. Save cũ thiếu trường chăn nuôi được mở với chuồng trống, giữ nguyên tiền và cây.

## Lưu và kiểm chứng chung

Các trường mới trong schema 1 mặc định bằng 0 khi đọc save cũ. Không tự xóa bản lưu hay thay tiền của người chơi. Validation loại các giá trị nước, bệnh, thời gian bảo vệ và đồng hồ thời tiết ngoài khoảng hoặc NaN.

`Tools/test-core.ps1` kiểm tra luật, lây bệnh, trị bệnh, luân canh, thưởng trò chơi và chống thu trùng. Bridge `farm-check` kiểm tra trong một nông trại riêng: ba trò thu hoạch, 12 giai đoạn, UI input, tiền/kho/save, model bệnh, vũng nước và thao tác thoát nước/trị bệnh. Xem báo cáo `farm-ecology-result.json` trong `.tools/model-work`. Chưa kiểm thử phần cứng điện thoại; gameplay vẫn là local prototype, chưa có xác thực điểm trên server.
