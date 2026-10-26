# Đảo trăm nghề — Hồ sơ thiết kế mở rộng

Ngày 25/10/2026. Designer thực hiện trước bước triển khai. **Danh mục chốt: đúng 30 công trình mới, 30 giống cây mới và 60 loại trang trí mới.** Tài liệu tích hợp: [Island-Expansion-Concept.md](Island-Expansion-Concept.md).

Cập nhật 26/10/2026: đã dựng model procedural và gameplay solo theo danh mục này. [Gallery 330 prefab, mỗi mẫu ba góc](../Art/Previews/IslandExpansion/index.html); [ảnh công trình](../Art/Previews/IslandExpansion/Overview-B.png), [cây chín](../Art/Previews/IslandExpansion/Overview-C_5.png), [đồ trang trí](../Art/Previews/IslandExpansion/Overview-D.png). Các yêu cầu chất lượng/biến thể bên dưới vẫn là hướng mỹ thuật; model công trình hiện dùng một cấp ngoại hình. Giá đỡ sân trước đi kèm đồ treo/đồ bàn, không tính thêm vào 60 thiết kế. Các giới hạn triển khai được ghi rõ ở đầu tài liệu tích hợp.

## 1. Ý đồ của Designer

Đảo cần thêm những nơi có lý do để ghé: sân nghề đang làm dở, vườn có nhịp thu khác nhau, góc nghỉ bên nước và một làng có sinh hoạt. Người chơi chọn vài nghề hợp cách chơi của mình, nuôi những chuỗi hàng ngắn rồi dùng thành quả tạo dấu ấn cho đảo. Không yêu cầu xây cả 30 công trình để duy trì nông trại.

Phong cách giữ hoạt hình 3D nhiệt đới Việt Nam: khối chính tròn vừa phải, ngói đỏ đất, gỗ nâu mật, tre vàng, vôi kem, mảng lá lớn và điểm nhấn xanh ngọc. Công trình phải đọc được nghề từ góc camera farm bằng hình mái, sân thao tác và một đạo cụ lớn. Biển nghề là relief có viền/giá đỡ; không lấy chữ nhỏ làm tín hiệu duy nhất. Mặt sau có kết cấu hợp lý, không để mái nổi hoặc cột xuyên ngói.

Sáu bộ cảnh dùng chung bảng màu nhưng có hình riêng:

| Bộ | Hình khối và chất liệu chủ đạo | Nhịp cảnh | Bộ đồ tương ứng |
| --- | --- | --- | --- |
| Sân quê | Hiên thấp, gạch tàu, tre đan, vải mộc | Khoảng trống rộng để nhìn ruộng | D01–D10 |
| Chợ & bếp | Mái lệch, vỉ phơi, gốm nâu, khăn sọc | Món ăn và thao tác hướng về đường | D11–D20 |
| Bờ nước | Sàn thấp, cọc neo, nan tre, đá sáng | Điểm nhìn nằm ngang, chuyển động chậm | D21–D30 |
| Vườn trái | Vòm nhẹ, tán phân tầng, dây buộc, cói | Xen mảng kín và góc nghỉ | D31–D40 |
| Làng nghề | Mái răng cưa thấp, đất nung, khung gỗ | Trưng bày công đoạn, có sân giao hàng | D41–D50 |
| Hội làng | Mái mở, vải đỏ gạch, đồng sẫm, giấy màu | Một điểm nhấn cao, nhiều khoảng thở | D51–D60 |

Không gom cả sáu bộ vào mọi công trình. Một công trình có một bộ chính, tối đa một bộ phụ. Vật trang trí không cộng sức mạnh kinh tế bắt buộc; ưu tiên quyền sắp xếp, sưu tập và câu chuyện. Cây quả đã là nguồn màu rực, nên nền kiến trúc dịu hơn.

## 2. Đối chiếu với game hiện có

Đã đọc `AGENTS.md`, `Design/Concept-v0.5-original.txt`, `Design/Fish-Farming-Concept.md`, `Assets/HarvestIsle/Scripts/Core/FarmModel.cs`, `FarmDevelopmentModel.cs` và toàn bộ nhãn trong `Assets/HarvestIsle/Resources/FullModelCatalog.json`. Nền hiện có là 30 giống gameplay, 353 loại model / 600 biến thể / 635 prefab Resources. Những con số này **không thay đổi** vì tài liệu này.

Không đưa Hải đăng, Trạm thời tiết, Bàn ghép, Nhà kính, Vườn ươm, Bến thuyền, Tháp canh, Kho làng, Bồn nước mưa, Cầu gỗ, Giếng, Chuồng cũ vào 30 công trình mới: chúng đã có model dù một số chưa có đủ gameplay. Đồ nội thất HP, đèn lễ hội, cổng lễ hội, ghế gỗ, đường lát, chong chóng và hàng rào tre cũng không được đếm lại. Có thể tái sử dụng chất liệu hoặc phần mesh hiện có khi sản xuất, nhưng phải tạo loại có công dụng/hình tổng thể riêng.

30 cây mới không bao gồm nấm mới để lách số lượng, không tách màu quả thành giống mới và không đổi tên 30 giống hiện có. Đậu nành C05 là cây mới; tương và đậu phụ đã tồn tại nên không gọi chúng là sản phẩm hoàn toàn mới. Bún gạo đã có hình nguyên liệu lẩu; B01 bổ sung nơi sản xuất, không tính lại bún thành loại model mới trong bộ 120 này.

## 3. Quy tắc dùng bảng

Mã B/C/D là mã thiết kế ổn định, chưa phải enum hay khóa save. Kích thước công trình/trang trí tính bằng mét theo mặt nền, là footprint đề xuất để dựng blockout; chừa thêm 1 m trước cửa thao tác. Một ô ruộng hiện có rộng 2 × 2 m; cây C phải có bản gameplay gọn trong ô, bản trưng bày tự do có thể lớn hơn và không làm thay đổi diện tích ruộng ngầm.

Ba mốc mở nội dung dự kiến: **M1 Bếp vườn**, **M2 Nghề đảo**, **M3 Làng hồi sinh**. Đây là chương/thành tích riêng, không yêu cầu nâng cap cấp farm hiện tại quá 10. Thứ tự và điều kiện định lượng được chốt trong concept tích hợp. Mốc ghi ở mỗi dòng là mốc sớm nhất, không buộc mở đồng thời mọi mục trong mốc.

Mọi input/output bên dưới là đề xuất. Giá dùng Xu Mùa Vàng khi cân bằng, không dùng VND hoặc tỷ lệ ×1.000. Không lấy nhịp hàng giờ của concept gốc áp thẳng vào gameplay giây đang có.

## 4. 30 công trình

Mỗi dòng là một loại. Nâng cấp, trạng thái đang chạy và màu mái không tăng số loại. Với xưởng, cấp 1 có một thao tác chính; cấp 2 cải thiện hàng đợi hoặc lựa chọn; cấp 3 thêm dấu ấn nghề/hiệu ứng dễ đọc. Công trình dịch vụ nâng bằng nội dung tương tác, không mặc định cộng phần trăm tiền.

| ID · Công trình | Mốc · bộ · nền | Thiết kế hình và điểm nhận dạng | Hành động / đầu vào → đầu ra | Khác loại sẵn có và lựa chọn chơi |
| --- | --- | --- | --- | --- |
| B01 · Xưởng bún gạo | M1 · Chợ & bếp · 5×4 | Mái chữ L thấp, bồn gạo ngâm, khuôn ép lớn, sào bún trắng dưới hiên | Lúa trong kho + thời gian → bún gạo; mẻ bao gồm xay/ngâm, chọn mẻ chợ hoặc nhập bếp | Không thay cối xay bột mì; dùng bún vào chuỗi lẩu đã có khi nối dữ liệu |
| B02 · Lò bánh tráng | M1 · Chợ & bếp · 5×5 | Ống khói thấp lệch bên, mặt tráng tròn và sân vỉ bánh dạng quạt | Lúa + mè C06 → bánh tráng mè; lúa → bánh tráng thường; xay/ngâm nằm trong mẻ | Bánh gạo mỏng, không phải lò bánh mì đổi mái; sân phơi là hình chính |
| B03 · Nhà tinh bột | M1 · Chợ & bếp · 5×4 | Máng rửa dọc, bể lắng xếp bậc, túi lọc treo dưới mái đôi | Khoai lang C01 / khoai tây C02 / sắn C03 → tinh bột theo công thức | Xử lý củ, không nghiền lúa mì; chọn nguyên liệu theo kho, không cộng gộp chúng thành hàng miễn phí |
| B04 · Xưởng ép dầu | M1 · Chợ & bếp · 4×4 | Vít ép ngang bằng gỗ, phễu hạt to, vò dầu có rãnh rót | Lạc C04 / mè C06 / đậu nành C05 → dầu ăn tương ứng | Khác JuicePress ép quả; hạt là nguồn tiêu hao mới, không thêm quyền ép mọi vật phẩm |
| B05 · Bếp kẹo vườn | M1 · Chợ & bếp · 4×4 | Chảo đồng nông, bàn kéo kẹo và khuôn vuông trên sạp mở | Đường hiện có + lạc C04 / mè C06 / gừng C07 → kẹo lạc, mè xửng hoặc kẹo gừng; tinh bột B03 + C10 + đường → bánh dẻo lá dứa | Món quà gọn cho B15; dùng sugar từ mía qua cối xay hiện có, không tạo đường trùng ID |
| B06 · Nhà rang cà phê | M2 · Vườn trái · 5×4 | Trống rang đồng ngang, mái ngói nâu, quạt thoát khí và bao quả đỏ | Quả cà phê C29 → hạt rang; chọn rang dịu hoặc đậm cho đơn tương ứng | Quy trình hạt mới; không thay công thức trà tuyết |
| B07 · Xưởng ca cao | M2 · Vườn trái · 5×5 | Hộp ủ ba ngăn, khay phơi, bánh nghiền đứng và biển quả bổ đôi | Ca cao C30 + đường hiện có → thanh ca cao; thêm sữa hiện có cho nhánh ca cao sữa | Một queue biểu diễn các công đoạn, không bắt người chơi canh từng công đoạn |
| B08 · Bếp gia vị | M1 · Chợ & bếp · 4×3 | Mái giật cấp, cối giã dài, khay lát gừng/nghệ, ống bếp bằng đất | Gừng C07 + nghệ C08 + riềng C09 → gói gia vị; các rau thơm C11–C13 thành bó pha theo công thức | Chỉ thực phẩm; HerbalBench tiếp tục làm thuốc, không đổi công dụng thuốc thành món ăn |
| B09 · Nhà sấy trái cây | M2 · Vườn trái · 5×4 | Mái kính mờ một dốc, khay lưới rút ngang, quạt gió nhỏ | Mít C28, ổi C22, nhãn C24 hoặc trái cũ → hàng sấy đúng loại; khổ qua C17 có công thức riêng | Đổi thời gian lấy hàng có đầu ra khác, không đặt hạn hỏng mới lên toàn kho |
| B10 · Nhà muối chua | M1 · Chợ & bếp · 4×4 | Dãy chum nắp nặng dưới mái vòm gạch, bàn cắt và kẹp tre | Dưa leo C14 / cà tím C15 / đậu bắp C16 + muối NPC → hũ rau chua; nhánh cao thêm C07/C09 | Không thay SoyVat và tương; mẻ có thời gian nhưng không hỏng vì người chơi offline |
| B11 · Xưởng gốm | M2 · Làng nghề · 5×5 | Lò bầu đất nung, sân xoay, tường có hốc phơi sản phẩm | Đất sét và men từ nguồn nghề → đồ gốm trang trí D01, D34, D38, D42 theo bản vẽ | Chế tác đồ sở hữu lâu dài; không dựng vô hạn rồi bán lại lời |
| B12 · Nhà dệt chiếu | M2 · Làng nghề · 5×4 | Mái dài thoáng, khung cửi lớn, bó sợi dựng và chiếu cuốn lộ mép | Sợi cói từ nguồn nghề → D03, D41, D43, D49 theo bản vẽ; gỗ khung nếu công thức cần | Nguồn sợi được khai báo riêng, không coi lau sậy cảnh là kho nguyên liệu |
| B13 · Xưởng giấy dó | M2 · Làng nghề · 5×4 | Bể xeo thấp, khung phơi dựng chéo, mái ngói sáng nhiều ô lấy gió | Bột giấy từ nguồn nghề → D44, D51, D58 và giấy gói B15 | Không giả định cây mới thứ 31; giấy là vật liệu mua bằng xu/đơn nghề |
| B14 · Xưởng mộc mỹ nghệ | M2 · Làng nghề · 5×5 | Bàn bào ngoài hiên, khung kẹp chữ A, gỗ có mộng nhìn rõ | Gỗ/tre nghề → D04, D05, D14, D36, D48 và đồ gỗ được mở | Đồ nghề có silhouette riêng; không tái đếm Bench, HandCart, BambooFence |
| B15 · Nhà gói quà | M2 · Làng nghề · 4×3 | Mái nhỏ như cánh gấp, bàn buộc dây, giấy và khăn được treo thành lớp | Chọn đúng hai món thành phẩm + giấy gói → một gói quà có nội dung xác định | Không thay GiftBox model cũ thành loại mới; khác xưởng chế biến vì giữ thông tin món trong gói |
| B16 · Thư viện hạt giống | M1 · Sân quê · 4×4 | Nhà tám cạnh thấp, ngăn mẫu hạt mặt đứng, biểu tượng vỏ hạt mở | Nộp mẫu cây mới đã thu → trang hồ sơ, lựa chọn đề tài/giống tiếp theo | Khác Nursery ươm và GraftingBench ghép; không nhân hạt hoặc hoàn tiền giống cũ |
| B17 · Trạm kiểm đất | M2 · Sân quê · 3×4 | Bàn mẫu cắt lớp đất lớn dưới mái sáng, ống thử màu đặt trong khay | Chọn ô và đọc nhóm cây/độ phì hiện có → so sánh kế hoạch luân canh | Công cụ hiểu số liệu, không nút hồi đất tức thời; tránh lấn vai phân ủ |
| B18 · Trạm phối nước tưới | M2 · Bờ nước · 4×4 | Ba van lớn, máng chia nước, trụ đồng hồ cơ và bơm thấp | Người chơi phân lịch tưới cho nhóm ô → lượt tưới trong giới hạn đã công bố | Khác VillageWell/RainTank/IrrigationCanal; cần thiết kế hệ nước/lịch, chưa có trong Core |
| B19 · Nhà dưỡng thiên địch | M2 · Vườn trái · 4×4 | Khung nhà lưới hở, khay lá và ô kén lớn dễ đọc | Vật tư dưỡng từ nguồn nghề + lượt chờ → một đợt hỗ trợ phòng sâu có giới hạn | Dùng Ladybug hiện có làm sinh vật minh họa; nhà mới không phải BeeHive hoặc PestNet đổi màu |
| B20 · Trạm chăm tán cây | M2 · Vườn trái · 4×3 | Mái nửa vòng, giá kéo tỉa, bộ tán mẫu hai nhánh tương phản | Chọn cây lâu năm và phương án tỉa → lịch tán/nhịp quả dự báo cho chu kỳ sau | Không ghép cây/đổi giống; một lựa chọn có đánh đổi nhịp thu, không cộng buff vô hạn |
| B21 · Nhà hợp tác xã | M3 · Sân quê · 6×5 | Nhà ba gian có sân cân và bảng vòng góp hàng lớn | Chọn một dự án NPC, đóng các lô hàng → hoàn thành công trình cảnh hoặc trang nhật ký | Không thay MarketStall bán kho; bản đầu solo, co-op là nhánh triển khai sau |
| B22 · Nhà lưu niệm Mùa Vàng | M3 · Hội làng · 6×5 | Nhà sàn thấp, mái hai tầng lệch, tủ hiện vật và vệt nước bão trên cột | Gắn bản sao hiện vật/ảnh đã mở → kể lại từng chương hồi sinh | Không tiêu hủy vật phẩm độc bản; không phải gallery kỹ thuật model |
| B23 · Trường nhà nông | M1 · Sân quê · 5×4 | Lớp học hiên mở, mái lá gọn, luống minh họa nằm trên bàn cao | Thử bài luân canh/tưới/ghép trong vùng mô phỏng → bài học và bản vẽ | Không dùng ô/save thật để thi; mở lựa chọn nghề thay vì yêu cầu cày XP |
| B24 · Sân khấu đờn ca | M3 · Hội làng · 6×4 | Mái cong một nhịp, rèm ngắn, bục nhạc và hàng ghế khán giả thoáng | Góp thực phẩm/đồ nghề cho một buổi diễn → cảnh sinh hoạt và quà trang trí | Khác FestivalArch/Nồi nấu chung; không buộc online đúng giờ để nhận phần thưởng |
| B25 · Trạm chăm thú | M2 · Sân quê · 4×4 | Hiên thấp, mái tai mềm, bàn chải lớn và góc nghỉ lót cói | Chọn con vật hiện có, chải/làm sạch theo lượt → hồ sơ chăm sóc và phản ứng nhân vật | Không thêm loài mới vào bộ cây/công trình; không tự chữa mọi bệnh hoặc thay luật cho ăn |
| B26 · Nhà khách miệt vườn | M3 · Vườn trái · 6×5 | Hai phòng nhỏ lệch mái, hành lang nối, hiên nhìn tán cây | Chọn chủ đề lưu trú, chuẩn bị món/quà → lượt khách NPC và lời nhận xét về bố trí | Khác nhà dân cảnh và Hotpot; không mô phỏng khách có nhu cầu từng phút khi vắng |
| B27 · Nhà cứu hộ chim | M3 · Vườn trái · 5×4 | Mái rộng thấp, hành lang lưới thoáng, cửa thả hướng vườn | Nhận tình huống NPC, chuẩn bị vật tư → đoạn chăm/thả chim và trang quan sát | Không khuyến khích bắt chim để farm thưởng; dùng chim hiện có làm nhân vật, không bán động vật |
| B28 · Nhà tuyển nông sản | M2 · Chợ & bếp · 5×4 | Băng con lăn ngắn, ba khay sao lớn, mái tôn có cửa sáng | Chọn lô kho → tách theo loại/sao và dành trước cho đơn hoặc bếp | Giữ tổng lượng và sao; không tự nâng chất lượng, khác RiceBarn chỉ lưu hàng |
| B29 · Bưu cục đảo | M3 · Hội làng · 4×4 | Tháp thư vuông thấp, mái hiên chữ V, khe thư và túi vải | Nhận thư NPC, đính món quà/ảnh theo yêu cầu → hồi âm và câu chuyện | Không cần email/chat người thật; quà qua server/co-op chưa nằm trong bản đầu |
| B30 · Trạm xe đạp | M3 · Bờ nước · 5×3 | Mái lá cọ mỏng, giá xe cong, bản đồ vòng đường lớn | Chọn tuyến tham quan NPC giữa các điểm đã mở → hành trình và ảnh lưu niệm | Không thay CargoBoat/HandCart; tuyến hợp lệ phải có đường, không chạy xuyên ruộng |

Phân loại thực chất: B01–B10 chế biến thực phẩm; B11–B15 nghề thủ công và gói quà; B16–B20 hỗ trợ canh tác; B21–B25 đời sống làng/chăm sóc; B26–B30 khách tham quan và dịch vụ. Mỗi nhóm có thao tác khác nhau, tránh biến 30 mục thành cùng một queue đổi nhãn.

Nguồn nghề là thiết kế cần làm riêng: bảng nhà cung cấp NPC bán vật tư bằng xu, có danh mục rõ, gồm gỗ/tre, đất sét/men, sợi cói, bột giấy dó và muối thực phẩm theo nhánh. Không có tài nguyên mới tự phát trong kho, không cho khai thác cây cảnh cũ. B11–B14 có thể bán bản vẽ/chế tác theo đơn trước khi có hệ khai thác tài nguyên. Công thức nhiều hơn hai đầu vào cần mở rộng RecipeSpec hoặc tách mẻ đúng như concept tích hợp; đây chưa là dữ liệu nạp được vào Core.

## 5. 30 giống cây mới

Vòng đời đề xuất: **V** thu một vụ rồi gieo lại; **L** thu lá, cây trở về giai đoạn tán và có số lượt thu hữu hạn được công bố trước khi gieo; **Q** cây/dây lâu năm giữ thân và quay lại ra hoa/quả. Không cộng dồn sản lượng offline không giới hạn. Cây chín giữ nguyên đến khi thu theo nguyên tắc bảo vệ phiên vắng của bản mở rộng. Các nhóm sinh học dưới đây phục vụ hình và thiết kế; ánh xạ về năm nhóm luân canh hiện có phải được chốt khi triển khai.

Mỗi dòng có đủ năm giai đoạn **1 mầm → 2 non → 3 phát triển → 4 gần thu → 5 thu được**. Củ vẫn có một phần vai/cổ hoặc đất đội để nhận biết chín; không phóng toàn bộ củ nổi trên mặt đất. Thân hỗ trợ của cây giàn nằm trong footprint, không tự lấy ô bên cạnh. Cây quả bản farm giữ tán thấp và một điểm nhìn thấy quả từ góc chơi.

| ID · Cây | Mốc · vòng · nhóm hình | Năm giai đoạn phải phân biệt | Hàng thu / chuỗi đề xuất | Điểm nhận dạng khi chín |
| --- | --- | --- | --- | --- |
| C01 · Khoai lang | M1 · V · củ dây | Chồi tím → hai lá tim → dây bò cụm → đất nứt nhẹ/cổ củ → dây rậm, vai củ tím | Khoai lang → B03 tinh bột; đơn tươi | Lá tim xanh tím và củ thon, không nhầm khoai môn lá lớn |
| C02 · Khoai tây | M1 · V · củ bụi | Mầm lùn → cặp lá kép → bụi tròn → hoa nhỏ/củ phồng đất → lá già nhẹ, vai củ vàng | Khoai tây → B03; món lẩu đề xuất | Bụi đứng, lá kép và củ tròn ít mắt |
| C03 · Sắn | M1 · V · củ thân đứng | Hom bật chồi → thân đỏ thấp → lá xẻ chân vịt → thân phân nhánh → gốc dày, chùm củ hé đất | Sắn → B03; chỉ công thức chế biến được khai báo | Lá chân vịt trên cuống dài, củ dài tụ gốc |
| C04 · Lạc | M1 · V · hạt đậu | Hai lá mầm → cặp lá nhỏ → bụi xòe → hoa vàng sát gốc → đất đội, vài vỏ lạc lộ khi chọn thu | Lạc → B04/B05 | Lá nhỏ từng cặp, vỏ hạt thắt eo |
| C05 · Đậu nành | M1 · V · hạt đậu | Mầm đôi → ba lá → bụi thấp → hoa và quả non → chùm quả vàng nâu | Hạt đậu nành → B04; tương/đậu phụ qua công thức mở rộng | Quả ngắn có lông, không dùng model đậu leo hay Hà Lan |
| C06 · Mè | M1 · V · hạt dầu | Mầm nhỏ → thân bốn lá → thân thẳng phân tầng → hoa ống trắng → quả nang dựng dọc thân | Mè → B02/B04/B05 | Dãy quả nang đứng, không gắn bông lúa lên thân |
| C07 · Gừng | M1 · V · thân rễ | Mắt củ bật mầm → hai lá hẹp → cụm thân nghiêng → gốc đội đất → lá ngả nhẹ, nhánh củ nổi một phần | Gừng → B05/B08/B10 | Lá hẹp hai hàng, củ nhánh vàng nhạt |
| C08 · Nghệ | M1 · V · thân rễ | Chồi nhọn → lá cuộn → bụi lá rộng → bẹ gốc dày → đất nứt và lát nhận dạng cam trong icon | Nghệ → B08 | Lá rộng hơn gừng; cam nghệ nằm ở icon/sản phẩm, không tô cam toàn thân |
| C09 · Riềng | M1 · V · thân rễ | Chồi đỏ → lá bóng non → thân cao vừa → bẹ có sắc đỏ → cụm gốc dày và chùm hoa nhỏ | Riềng → B08/B10 | Thân thẳng, bẹ đỏ nâu, củ đốt rõ |
| C10 · Lá dứa | M1 · L · lá | Chồi bẹ → lá ngắn tỏa → bụi hình sao → lá dài dựng cong → bó lá quạt xanh đậm | Lá dứa → B05 nhánh kẹo thơm; đơn bếp | Lá dài bản vừa, không tua mảnh như sả |
| C11 · Húng quế | M1 · L · lá | Mầm đôi → bốn lá → bụi phân nhánh → đọt lá đầy → tán bóng, ngọn tím nhẹ | Húng quế → B08; rau thơm cho bếp | Lá nhọn bóng và đọt tím, hoa chỉ là chi tiết phụ |
| C12 · Tía tô | M1 · L · lá | Mầm tím → lá răng cưa → bụi tam giác → mặt lá nổi gân → lá tím xanh lớn | Tía tô → B08; đơn rau | Viền răng cưa và mặt dưới tím |
| C13 · Bạc hà | M1 · L · lá | Mầm nhỏ → lá đối → cụm thân vuông cách điệu → đọt non dày → lá xanh mát xếp đối | Bạc hà → B08; gói đồ uống đề xuất | Mép răng nhẹ, lá nhăn, màu khác húng và tía tô |
| C14 · Dưa leo | M1 · V · dây giàn | Lá mầm → tua bám → giàn thấp phủ lá → hoa vàng/quả nhỏ → quả xanh dài treo lộ | Dưa leo → B10; đơn tươi | Quả hình trụ, gai thưa nhìn được ở ảnh cận |
| C15 · Cà tím | M1 · V · quả bụi | Mầm đôi → lá nhung → bụi thân tím → hoa tím/quả non → quả dài tím bóng | Cà tím → B10; bếp đề xuất | Đài xanh ôm đầu quả tím, tán thưa hơn cà chua |
| C16 · Đậu bắp | M1 · V · quả thân đứng | Hai lá → lá xẻ → thân có đốt → hoa kem tâm sẫm → quả xanh có sống dựng lên | Đậu bắp → B10; lẩu đề xuất | Quả có gân dọc và mũi nhọn, không giống đậu leo |
| C17 · Khổ qua | M1 · V · dây giàn | Mầm → lá xẻ → tua phủ giàn → hoa/quả nhỏ → quả dài sần treo thấp | Khổ qua → B09 nhánh lát sấy; đơn tươi | Bề mặt gồ hạt lớn, xanh sáng |
| C18 · Bầu | M1 · V · dây giàn | Hai lá tròn → tua → lá lớn trên giàn → hoa trắng/quả nhỏ → quả eo nhẹ xanh nhạt | Bầu → đơn tươi; công thức lẩu mới | Quả bầu thon eo và lá tròn, không đổi màu bí đỏ |
| C19 · Mướp | M1 · V · dây giàn | Mầm → lá xẻ nông → dây phủ giàn → hoa vàng/quả non → quả dài có gân mềm | Mướp → đơn tươi; công thức lẩu mới | Quả dài cong nhẹ, gân chạy dọc khác gai dưa leo |
| C20 · Chanh dây | M2 · Q · dây lâu năm | Hom nảy → dây bám → giàn tán thoáng → hoa tròn lớn/quả xanh → quả tím tròn trong khe lá | Chanh dây → JuicePress công thức mới; B15 dùng thành phẩm | Hoa có vòng tua, quả tím tròn; không phải biến thể chanh |
| C21 · Thanh long | M2 · Q · thân mọng | Hom ba cạnh → hai nhánh → nhánh rủ trên trụ → hoa trắng/quả xanh → quả hồng có tai xanh | Thanh long → đơn quả; JuicePress công thức mới | Trụ thấp với nhánh ba cạnh rủ, quả nổi ở đầu nhánh |
| C22 · Ổi | M2 · Q · cây tán | Cây mầm → thân chạc → tán tròn nhỏ → hoa trắng/quả xanh bé → quả xanh vàng lộ dưới lá | Ổi → B09; đơn quả | Lá có gân ngang rõ, quả có đài sao nhỏ |
| C23 · Vải | M2 · Q · cây tán | Mầm → lá kép non → tán bán cầu → chùm hoa/quả xanh → chùm quả đỏ vỏ sần | Vải → JuicePress công thức mới; B15 dùng thành phẩm | Chùm đỏ dày, vỏ sần lớn đọc được ở zoom gần |
| C24 · Nhãn | M2 · Q · cây tán | Mầm → lá kép dài → tán xòe → chùm hoa/quả nhỏ → chùm quả nâu mật | Nhãn → B09; đơn quả | Quả tròn nâu trên cuống phân nhánh, tán thoáng hơn vải |
| C25 · Chôm chôm | M2 · Q · cây tán | Mầm → thân thấp → tán hơi dựng → quả xanh gai mềm → chùm đỏ với gai cong | Chôm chôm → đơn quả; thành phẩm ép mới | Gai mềm to cách điệu, không dùng texture lông dày gây nhiễu |
| C26 · Măng cụt | M3 · Q · cây tán | Mầm to → lá dày đôi → tán tầng gọn → hoa/quả xanh → quả tím sẫm có tai đài dày | Măng cụt → đơn đặc sản; quà trái cây B15 theo công thức | Lá bóng dày, quả tím có bốn tai xanh |
| C27 · Sầu riêng | M3 · Q · cây tán | Mầm → lá dài → thân chạc cao gọn → quả non trên cành chính → một vài quả gai lớn | Sầu riêng → đơn đặc sản; B05 nhánh kẹo | Quả gai lớn ở cành chịu lực; không gắn trên lá mỏng |
| C28 · Mít | M2 · Q · cây tán | Mầm → lá bầu → tán rộng gọn → quả non sát thân → quả lớn vàng xanh bám thân/cành | Mít → B09; đơn quả | Quả dài bám thân và lá bầu bóng, khác sầu riêng |
| C29 · Cà phê | M2 · Q · bụi lâu năm | Mầm đội vỏ → lá đối → bụi tầng thấp → hoa trắng/quả xanh → dãy quả đỏ ôm cành | Quả cà phê → B06 | Quả đỏ dọc cành ngang, lá gợn bóng |
| C30 · Ca cao | M2 · Q · cây tán | Mầm → lá non đỏ → tán gọn → hoa nhỏ/quả xanh bám thân → quả có rãnh vàng đỏ | Quả ca cao → B07 | Quả lớn có rãnh bám thân, không nhầm đu đủ |

Các rau mới đề xuất cho lẩu cần mapping nguyên liệu và khẩu vị riêng, chưa mặc nhiên vào 64 nguyên liệu đang có. C03 sắn chỉ đi qua chế biến trong thiết kế này. Mỗi cây luôn có đầu ra bán tươi/đơn phù hợp hoặc công thức bắt buộc được mở cùng cây, tránh mở giống rồi không thể dùng sản phẩm.

Nhịp prototype theo nhóm, tính bằng giây thực như game hiện tại: C10–C13 lá nhanh 60–120 giây đến lượt đầu, các lượt sau thử trong cùng dải; C14–C19 rau quả vừa 150–300 giây; C01–C09 củ/hạt vừa 150–300 giây, riêng C03 thử 360–720 giây; C20–C30 lâu năm thử 360–720 giây tạo tán/lượt đầu và 150–300 giây tái thu. Đây là dải thử để so mật độ việc giữa các giống, chưa là số cân bằng; giới hạn lượt lá L và sản lượng từng cây phải chốt cùng giá. Không hiểu 720 giây là nghĩa vụ quay lại đúng lúc.

## 6. 60 loại trang trí

Mọi dòng là một thiết kế có hình tổng thể riêng. Đồ kết hợp nhiều phần như D08 là **một cụm trang trí**, không đếm từng chổi thành nhiều loại. Gốc đặt ở tâm mặt nền; loại treo cần ổ gắn hợp lệ và vẫn có thumbnail nhìn độc lập. Số đo là rộng × sâu × cao (m), chưa bake.

Mốc bộ: Sân quê/Chợ & bếp mở dần ở M1; Bờ nước/Vườn trái/Làng nghề ở M2; Hội làng ở M3. Mua bằng xu hoặc mở bản vẽ qua nghề; đồ của bộ M1 được mua hoàn thiện ở NPC ngay khi mở, đến M2 mới có lựa chọn tự làm qua B11–B14, tránh khóa vòng giữa đồ và xưởng. Việc hoàn thành bộ không tăng sản lượng farm. Đồ có động tác nhẹ chỉ chạy khi gần camera; không thêm collider chuyển động. Vật cảnh nước chỉ đặt ở vùng nước cảnh được đánh dấu, không trở thành hồ cá hay máy sản xuất.

| ID · Tên | Kích thước | Hình/vật liệu và dấu riêng | Vị trí / ứng xử |
| --- | --- | --- | --- |
| D01 · Lu nước có gáo dừa | 0,9×0,9×1,1 | Lu gốm bụng lớn, nắp gỗ lệch, gáo treo móc | Góc hiên; cảnh tĩnh, không thay RainTank gameplay |
| D02 · Đôi guốc bên thềm | 0,5×0,4×0,12 | Hai guốc gỗ cong, quai vải có độ dày | Sát cửa nhưng không che điểm chạm |
| D03 · Võng khung tre | 2,4×1×1,2 | Khung chữ A, võng cói cong dưới tải, dây thắt đầu | Sân nghỉ; võng đung đưa nhẹ khi gần |
| D04 · Giá phơi nón | 1,5×0,6×1,6 | Khung chạc gỗ mang ba nón nghiêng khác góc | Vách hiên; có thể dùng hình nón nhân vật làm phần phụ |
| D05 · Cối giã gạo đòn chân | 2,4×0,9×1,2 | Đòn bẩy dài, chày đứng, cối lõm và điểm đặt chân | Sân quê; hoạt cảnh tùy chọn, không nghiền hàng |
| D06 · Nia sàng tựa vách | 1×0,35×1,1 | Đĩa tre nông đường kính lớn có gân đan nổi | Tựa vách, nghiêng cố định; khác Basket sâu |
| D07 · Lồng bàn tre | 0,8×0,8×0,5 | Vòm lưới tre có núm, mép viền vải | Trên nền hoặc ổ mặt bàn; không thêm bàn ăn HP |
| D08 · Bộ chổi rơm và ky | 0,8×0,5×1,4 | Chổi xòe chân rộng cạnh ky xúc vuông nông | Mép sân, một cụm có bóng nhận dạng |
| D09 · Ống bương gõ nước | 1,1×0,7×0,9 | Ống tre nghiêng trên chạc, chén đá bên dưới | Vườn tĩnh; nghiêng/gõ nhẹ, không cấp nước tưới |
| D10 · Sân ô ăn quan | 2,2×1,2×0,04 | Ô khắc trên gạch, sỏi nằm thành hai hàng rõ | Mặt sân; cảnh tĩnh trước, không hứa minigame |
| D11 · Giá phơi bánh tráng | 1,8×1×1,7 | Ba tầng vỉ nghiêng, bánh tròn bán trong suốt nhẹ | Cạnh B02; chỉ cảnh, sản lượng do B02 |
| D12 · Mâm ngũ quả | 0,8×0,8×0,65 | Mâm chân thấp, quả xếp chóp cân đối, viền đỏ đất | Sân hội/bếp; tái dùng mesh quả làm thành phần |
| D13 · Đòn gánh đôi quang | 2×0,8×1,3 | Cần tre cong, hai quang dây chạm nền | Bên lối chợ; không là HandCart đổi hình |
| D14 · Cối đá xay tay | 0,85×0,85×0,8 | Hai thớt đá tròn lệch tay quay, rãnh bột và chân ba cạnh | Sân bếp; khác máy xay gió công trình |
| D15 · Khuôn bánh chạm lá | 0,6×0,45×0,12 | Khuôn gỗ mở hốc lá với vân chìm rõ | Ổ mặt bàn/bệ thấp; không dùng decal phẳng thay hốc |
| D16 · Chuỗi tỏi ớt treo | 0,5×0,2×1,2 | Bím tỏi trắng chen quả ớt đỏ, dây gai có nút | Ổ treo dưới hiên; không phải cây đang thu |
| D17 · Khay bánh lá chóp | 0,75×0,55×0,4 | Khay thấp với bánh gói lá dạng chóp buộc lạt | Góc bếp; loại đồ ăn trang trí riêng |
| D18 · Biển gốm Bếp nhà | 0,9×0,12×0,45 | Viên gốm chữ nổi, hai lỗ dây treo, mép bo không đều | Gắn vách; không mở OrderBoard hoặc menu |
| D19 · Ấm đất trên bếp than | 0,75×0,7×0,7 | Ấm có vòi cong trên lò đất ba chân, than hồng nhẹ | Sân bếp; tắt sáng xa, không Point Light riêng |
| D20 · Mẹt ép đậu buộc đá | 0,9×0,7×0,6 | Khung ép vuông, vải trắng gấp mép và đá nặng phía trên | Cạnh nhà muối/gói quà; không tự tạo đậu phụ |
| D21 · Guồng nước cảnh | 2×0,8×2,2 | Vành gỗ nhiều gàu nông, trục thấp, nước rơi ít | Bờ nước cảnh; quay chậm, không tăng nước tưới |
| D22 · Thuyền thúng úp | 1,8×1,8×0,8 | Thúng tròn đan kín, mép đen, mái chèo nằm cạnh | Bãi cát; khác CargoBoat, không phương tiện |
| D23 · Cọc neo dây kép | 0,7×0,6×0,8 | Cọc gỗ sẫm đầu vát, hai vòng dây có thòng xuống | Mép bến; không mở giao thương |
| D24 · Lờ cá tre | 0,8×0,45×0,45 | Lồng thuôn có hom phễu nhìn thấy qua nan | Trên bờ khô; không bắt cá/vật nuôi |
| D25 · Vó bè thu nhỏ | 2,4×2,4×2,4 | Bốn cần cong giữ lưới vuông hõm, trụ kéo chéo | Ổ bờ nước; lưới gộp mặt, không mô phỏng vải |
| D26 · Bè lục bình hoa | 1,6×1,1×0,5 | Cụm lá tròn cuống phồng và vài hoa tím nổi mặt nước | Nước cảnh; dập dềnh nhỏ, không giống cây trồng mới |
| D27 · Cột đo con nước | 0,35×0,35×1,8 | Cọc trắng có vạch xanh đỏ và mũ gỗ | Sát nước; số liệu trang trí, không giả dự báo thật |
| D28 · Giá phao cứu sinh | 0,9×0,45×1,5 | Vòng phao trắng đỏ trên giá cong, dây buộc rõ | Bờ hồ, cạnh chỗ nghỉ |
| D29 · Bục ngắm nước bán nguyệt | 3×2×0,45 | Sàn quạt nửa tròn, lan can thấp ôm mép | Ổ bờ riêng; chỉ điểm đứng ngắm, không cầu nối/bến hàng |
| D30 · Vách khảm vỏ hàu | 1,5×0,25×1,2 | Mảng tường thấp có vỏ xếp vảy, đỉnh vòm | Bờ nước; khác một BeachShell rải, có thể tái dùng chất liệu vỏ |
| D31 · Vòm dây leo mắt cáo | 2,6×1×2,2 | Vòm gỗ nhẹ với giằng mắt cáo, dây lá thưa | Lối vào vườn; chỉ tán cảnh không thu trái C20 |
| D32 · Lều nghỉ dã ngoại | 2,4×2,2×1,8 | Vải mộc căng tam giác, cửa cuộn, thảm chạm đất | Góc vườn; không chứa khách hoặc cho buff |
| D33 · Chuông gió hạt đất | 0,45×0,45×0,9 | Bốn dây hạt gốm và lá đồng nhỏ dưới vành tre | Ổ treo; âm tắt cùng lựa chọn âm môi trường |
| D34 · Đĩa chim tắm | 0,9×0,9×0,8 | Đĩa gốm nông trên trụ chân loe, mép có dấu chân chim | Vườn; chim cảnh có thể đậu, không máng vật nuôi |
| D35 · Trụ số hàng vườn | 0,35×0,3×0,7 | Khối đá vát chóp gắn số gốm lớn và lá relief | Đầu hàng; số là tùy chọn hiển thị, không loại mới mỗi số |
| D36 · Thang hái quả chữ A | 1,1×1×1,8 | Thang gỗ xòe hai chân, bậc rộng và giỏ nhỏ treo bên | Cạnh cây; không thay thao tác thu |
| D37 · Xe cút kít hoa | 1,6×0,8×0,8 | Một bánh lớn, lòng xe cong thấp chứa hoa dại | Sân vườn; khác HandCart hai bánh chở hàng, tái dùng hoa phụ |
| D38 · Đồng hồ nắng gốm | 0,85×0,85×0,7 | Mặt đĩa khắc tia, kim tam giác đứng và bệ xoắn thấp | Chỗ sáng; bóng theo cảnh, không thay đồng hồ game |
| D39 · Tượng sóc ôm hạt | 0,6×0,6×0,85 | Tượng đất nung đuôi cuộn lớn và hạt ôm trước bụng | Mép lối vườn; tượng, không loài vật gameplay mới |
| D40 · Vòng dây leo bện | 0,9×0,2×0,9 | Vòng cành khô bện, lá xen thưa, dây cói thả | Treo cửa/vách; không cổng đủ người đi qua |
| D41 · Khung cửi trưng bày | 1,5×1,2×1,5 | Khung gỗ có sợi căng, thoi và phần chiếu dệt dở | Sân nghề; có thể tái dùng bộ máy B12, không chạy sản xuất |
| D42 · Bàn xoay gốm chân đạp | 1×1×0,9 | Hai đĩa đồng trục, bình đang nặn, bàn chân đạp | Sân B11; chỉ chuyển động minh họa khi gần |
| D43 · Guồng se sợi | 1,3×0,65×1,3 | Bánh nan đứng nối con suốt và bó sợi | Sân nghề; khác Windmill vì không mái/cánh nghiền |
| D44 · Khung xeo giấy | 1×0,8×0,25 | Khung chữ nhật lưới mịn, lớp giấy ướt mép xơ | Trên bệ/ổ bàn; hình tĩnh tránh alpha nhiều lớp |
| D45 · Bộ ống màu khoáng | 0,7×0,5×0,55 | Sáu ống tre có bột màu và muỗng gỗ, cùng khay | Sân gốm/giấy; sáu màu vẫn một loại đồ |
| D46 · Vách gạch hoa gió | 1,6×0,3×1,6 | Các ô gạch rỗng hình lá xen khung xây | Chia sân; không dùng SlatDivider đổi chất liệu |
| D47 · Khung căng mặt trống | 1,1×0,55×1,3 | Vòng da căng bằng dây trên chạc gỗ, dụng cụ nhỏ kèm | Sân nghề; công đoạn chế tác khác trống hoàn thiện D53 |
| D48 · Bộ thước thợ treo | 0,8×0,15×0,9 | Êke lớn, thước thẳng và dây dọi trên hai móc | Vách B14; không thay Toolbox/Hammer |
| D49 · Thảm cói xoắn tròn | 1,8×1,8×0,04 | Sợi cói bện xoắn từ tâm, viền hai lớp | Nền hiên; không gồm bàn HP_Tatami |
| D50 · Quạt mo ghép hoa | 1,1×0,2×1,1 | Năm quạt mo xếp cánh hoa, sống lá nổi | Vách nghề; không quay như Pinwheel |
| D51 · Đèn kéo quân | 0,75×0,75×1,2 | Lồng sáu mặt có trục hình người quay bên trong, đế rộng | Sân hội; khác FestivalLantern bằng cơ cấu/hình, sáng emissive |
| D52 · Đầu lân trên đôn | 1,2×0,9×1,3 | Mắt lân lớn, lông viền mảng mềm, miệng mở trên giá | Góc sân khấu; không nhân vật chiến đấu |
| D53 · Trống hội đại | 1,4×1×1,4 | Trống bụng cong trên giá nghiêng, đinh đồng và đôi dùi | Cạnh B24; trống hoàn thiện, không cộng thưởng khi gõ |
| D54 · Cây nêu dây tua | 0,8×0,8×3,2 | Cần tre cao hơi cong, vòng tre và dải tua dài | Điểm nhấn hội; kiểm tra che camera ở đầu lối |
| D55 · Bộ cà kheo | 0,75×0,55×2 | Hai thanh tre có bậc chân, dựng trên giá buộc | Mép sân hội; không hứa minigame di chuyển |
| D56 · Dàn cồng ba chiếc | 1,8×0,6×1,5 | Khung gỗ treo ba đĩa đồng lõm có kích cỡ khác nhau | Cạnh sân khấu, dùng như nhạc cụ trình diễn chung |
| D57 · Rối nước trên bệ | 0,6×0,6×0,9 | Tượng chú nông dân sơn mài, tay giơ quạt và đế sóng | Mép hồ cảnh/nhà lưu niệm; tĩnh trước, không người gameplay |
| D58 · Diều sáo trên giá | 1,6×0,55×1,8 | Cánh cong giấy nhẹ, bộ sáo tre và dây cuộn chân | Sân hội; gió lay nhẹ, không bay ra ngoài vùng camera |
| D59 · Khung ảnh dân gian | 2×0,45×1,8 | Tấm vẽ cảnh gánh lúa có hai lỗ khuôn mặt, chân chống sau | Khu khách; đứng chụp ảnh là tương tác tùy chọn |
| D60 · Bàn cờ tướng đá | 1,3×1,3×0,7 | Mặt bàn khắc bàn cờ và quân tròn dày, chân bốn trụ thấp | Sân hội; chưa có AI cờ, không gọi bàn HP đổi màu |

Các phần tái dùng phải được ghi trong manifest asset khi dựng: quả ở D12; nón ở D04; hoa ở D37; bộ máy nghề dùng chung giữa B11–B14 và D41–D44. Tái dùng phần cấu tạo giúp đồng nhất, không dùng nguyên prefab cũ rồi đếm là đồ mới. D51 cần thực sự có khoang quay/hình bóng; nếu chỉ đổi lồng đèn cũ thì chưa đạt một loại mới.

## 7. Phác bố cục và ba cảnh kiểm chứng đầu tiên

Đảo mở rộng đặt thành cụm vệ tinh quanh vùng đã có, không nhét vào 36 ô ruộng hoặc tự thay bố cục An Nhiên 70 × 60 m. Đây là quan hệ không gian, chưa là tọa độ build: **ruộng ↔ sân quê ↔ bếp/chợ**, **vườn trái ↔ nghề thủ công ↔ nhà khách**, **làng ↔ bờ nước ↔ tuyến xe đạp**. Mỗi cụm có sân thao tác riêng và đường quay về farm; dùng khoảng trống phân nhóm, không hàng rào che kín mọi mép.

Ba cảnh nên dựng trước để Designer kiểm tra tỷ lệ:

1. **Một bữa quê:** B01+B02+B05, hàng C04+C06+C10, D06+D11+D13+D17. Phải phân biệt bún, bánh tráng, kẹo từ góc farm mà không đọc tên.
2. **Góc vườn có nghề:** B06+B09, C20+C21+C29, D31+D33+D36+D38. Kiểm tra giàn, trụ thanh long và cây bụi không che thẻ chọn nhau.
3. **Chiều hội làng:** B24+B29, D51+D52+D53+D58, cây cũ làm nền. Kiểm tra đèn không cháy màu và đường người xem không chặn thao tác công trình.

Đây là cảnh chọn mẫu từ 120 loại, không cộng thành loại thứ 121. Khi đủ đẹp mới triển khai các họ còn lại theo cùng quy tắc. Không cần dựng toàn bộ 120 model chỉ để phát hiện tỷ lệ mái/cây sai.

## 8. Điều kiện bàn giao cho khâu sản xuất

- Công trình có front/side/back rõ, footprint và cửa thao tác không thay đổi khi nâng cấp. Máy quay/chạy tách phần cần chuyển động; phần tĩnh gộp theo material. Không tăng chi tiết nhỏ vô hạn để thay silhouette.
- 30 cây cần 150 hình giai đoạn; nếu dùng bệnh/héo theo pipeline hiện tại thì thêm 60 trạng thái dẫn xuất. Đây là số lượng việc tương lai, không khẳng định catalog đã tăng. Cây lâu năm có trạng thái sau thu nhận biết được mà không sinh loại cây mới.
- Trang trí có pivot, footprint, ổ treo/nước và vùng cản rõ. Các phần gầm/mép nhìn từ camera phải kín. Đồ tròn không dùng collider khối vuông quá lớn che vùng bên cạnh.
- Mục tiêu thử cho model ở góc chơi: xưởng khoảng 3.000–8.000 tam giác, đồ nhỏ 150–1.500, đồ nhấn tối đa khoảng 3.000, cây trưởng thành khoảng 800–3.000; đây là **ngân sách đề xuất**, chưa đo hoặc bảo đảm thiết bị. Chỉ phá ngân sách sau khi ảnh và profile chỉ ra nhu cầu. Không dùng vật liệu riêng cho từng chiếc lá hoặc gạch.
- Kiểm tra hình thật: ảnh trước/chéo/nghiêng/sau và Play từ góc farm, kèm đêm đối với đèn. Xem liên kết cột–mái, chân đồ chạm nền, quả bám thân, cảnh khi thu, dấu chọn và độ đọc tại kích thước icon.
- Về UX, mở bảng dùng FarmWindow/safeArea và guard hiện có; vào chế độ đặt đồ phải chặn gieo/thu/camera xung đột. Các tương tác mới chưa được coi là tồn tại chỉ vì có model.
- Về dữ liệu, giữ GUID cũ, ID enum/save cũ, tiền và kho thật. Chỉ chốt số catalog/prefab mới sau manifest được nghiệm thu; không cộng thẳng 120 vào prefabCount vì cây có giai đoạn, công trình có cấp và đồ có thể dùng chung mesh.

## 9. Tự rà thiết kế

Đã rà đủ B01–B30, C01–C30, D01–D60, mỗi ID có đúng một loại. Các nhánh lá, dây giàn và cây quả có silhouette khác nhau; các xưởng có nguyên liệu/đầu ra; các công trình dịch vụ có hành động riêng. B16–B30 là những hệ cần triển khai thực, không phải đổi tên model có sẵn. Đồ cảnh và mô phỏng học tập không tự phát sinh tiền, nông sản hoặc thay đổi farm thật.

Chưa dựng blockout, chưa render, chưa chạy Play/thiết bị cho nội dung mới. Hồ sơ này là thiết kế đầu vào để concept tích hợp và triển khai sau có cùng danh mục, mỹ thuật, phụ thuộc và tiêu chí nghiệm thu.
