# Mở rộng đảo — Nghề mới, vườn mới, nếp sống mới

Ngày thiết kế 25/10/2026; cập nhật triển khai 26/10/2026. Phạm vi là **30 công trình mới, 30 loại cây mới và 60 đồ trang trí mới**, do agent vai trò Designer thiết kế trước khi dựng model hoặc sửa gameplay. Danh mục và đặc tả mỹ thuật nằm trong [bản thiết kế Designer](Island-Expansion-Designer.md); tài liệu này quy định cách các nội dung nối thành trải nghiệm chơi.

**Trạng thái hiện tại: đã triển khai bản solo 30/30/60.** Các bảng nền thực tế và kế hoạch bên dưới ghi thời điểm thiết kế. Runtime có 60 cây, 90 mẫu đặt được, 97 công thức nhiều đầu vào, 46 mặt hàng nghề, ba mốc M1–M3 và dữ liệu `islandExpansion` nối cuối save schema 1. ID B/D dùng trong catalog riêng; C01–C30 ánh xạ sang ID cây tiếng Anh trong `IslandCropCatalog`. Chín `BuildingSpec`/16 `RecipeSpec` cũ giữ riêng.

Khu nghề được dựng cạnh phía đông của farm (tâm thế giới x=135), vùng đặt 80×56 m, lưới 0,5 m; không đổi tọa độ An Nhiên. Có kiểm đường, khoảng cách, hồ/bờ và hai điểm gá ở sân trước mỗi công trình. Mở từ Menu → Đảo trăm nghề. Model mới gồm 330 prefab; tổng Resources 965, catalog 503 loại/930 biến thể. [Gallery thực](../Art/Previews/IslandExpansion/index.html) và [ảnh Play](../Art/Previews/IslandExpansion/Play/11-All-content.png).

Các phần làm ở mức đầu tiên: cấp công trình 1–3 đổi sức chứa/tốc độ, chưa có model cấp riêng; B23 dùng ba câu hỏi thay cho ruộng mô phỏng; dịch vụ B21/B24/B26/B27/B29/B30 chốt nguyên liệu, chờ 90 giây, lưu nhật ký và tặng đồ lần đầu, chưa diễn hoạt NPC/chuyến xe thực. B25 lưu lượt chăm sóc và nghỉ 120 giây, không tăng sản lượng. Nhập bếp có 29 nông sản mới (không dùng sắn sống); bún/đậu phụ ánh xạ về ID bếp cũ, đồ thủ công không vào lẩu. Danh mục bếp tổng 122 nguyên liệu. Chưa mở online, chống chỉnh save local, cân bằng dài hạn hay benchmark điện thoại.

Model hiện là mesh tĩnh đã bake; võng, guồng nước và đèn kéo quân chưa có hoạt cảnh/đèn realtime riêng. Các chuyển động trong brief là hướng tiếp tục, không phải tính năng đã nghiệm thu.

## 1. Mục tiêu và ranh giới

Mở rộng cảm giác sống trên đảo: người chơi có thể trở thành người làm vườn, thợ thủ công, người tổ chức sinh hoạt làng hoặc chủ một tuyến trải nghiệm. Mỗi nghề phải sử dụng được sản phẩm của nghề khác, nhưng người chơi vẫn có thể chọn một hướng chính. Thêm công trình phải tạo ra quyết định hoặc hoạt động mới; thêm cây phải có đầu ra; thêm trang trí phải giúp kể chuyện về chủ nông trại.

Bản này bổ sung cho concept gốc, theo định hướng hiện tại **Unity 6.3 LTS, C#, URP, 3D hoạt hình ấm áp, Android/iOS màn hình ngang**. Đơn vị tiền là **Xu Mùa Vàng**, viết gọn **xu**, một coin lưu bằng một xu. Những đoạn web/2D, quy đổi tiền sang đồng và thời gian ví dụ trong concept cũ không dùng làm cơ sở cân bằng bản mở rộng.

### Nền thực tế đã đối chiếu

| Hạng mục | Hiện có trong project | Mục tiêu của concept này |
| --- | --- | --- |
| Cây canh tác | 30 `CropSpec`, gồm sáu cây lâu năm | Thêm 30 loại khác; mục tiêu 60 khi triển khai đủ |
| Phát triển farm | Chín `BuildingSpec`, 16 `RecipeSpec` | Thêm 30 công trình có vai trò; không đồng nhất toàn bộ với xưởng chế biến |
| Cấp farm | Tối đa 10 | Dùng chương nghề bên cạnh cấp hiện tại; không tự coi cấp 11 trở lên đã có |
| Thư viện model | 353 loại, 600 biến thể, 635 prefab Resources | Giữ nguyên trong lượt viết concept; 120 thiết kế không đồng nghĩa 120 prefab |
| Đặt công trình ngoài farm | Chủ yếu vị trí cố định | Cần hệ bố trí có lưu, kiểm tra chiếm chỗ và đường đi trước khi phát hành trang trí tự do |
| Online | Backend local, có quyền và giao dịch do server kiểm tra | Chơi solo đầy đủ trước; hợp tác online là đợt tích hợp riêng |
| An Nhiên | Profile riêng, khuôn viên 70 × 60 m | Không nhét cả 120 nội dung vào farm mẫu hoặc khởi tạo lại save |

Nguồn kiểm kê: `Assets/HarvestIsle/Scripts/Core/FarmModel.cs`, `FarmDevelopmentModel.cs`, `Assets/HarvestIsle/Resources/FullModelCatalog.json` và `AGENTS.md`. Model có trong gallery chưa chứng minh gameplay đã có. Ngược lại, một công trình đã có model như hải đăng, trạm thời tiết hoặc bàn ghép cây cũng không được tính lại thành loại mới chỉ vì bổ sung chức năng.

## 2. Vòng chơi và lựa chọn nghề

**Khám phá một nhu cầu của làng → chọn nghề → trồng nguyên liệu → làm một mẻ hoặc hoàn thành một dịch vụ → dùng/bán/giao thành phẩm → trang trí khu nghề → mở câu chuyện kế tiếp.**

Một phiên 5–15 phút gồm thu mẻ đã xong, quyết định dùng nguyên liệu vào bán thô hay chế biến, làm một đơn nghề và chỉnh một góc vườn. Người chơi có thể kết thúc sau bất kỳ bước nào; không cần đăng nhập đúng phút để giữ thành quả. Chu kỳ dài và hàng chờ giúp người chơi chuẩn bị cho phiên sau.

Ba nhánh hoạt động cùng tồn tại:

- **Sản xuất:** chọn công thức và lượng đầu vào; cân nhắc công suất, thời gian, đầu ra đang cần.
- **Dịch vụ và cộng đồng:** chuẩn bị một yêu cầu rõ ràng, chọn cách phục vụ hoặc sắp một trải nghiệm; nhận phản hồi và tiến độ chương.
- **Cảnh quan và sưu tập:** kiếm bản mẫu, bố trí theo sở thích, hoàn thành góc cảnh có câu chuyện. Không bắt người chơi mua đủ 60 đồ để sản xuất có lãi.

Không thêm một loại tiền thanh toán mới. Tay nghề và dấu mốc chương là tiến độ, không phải ví tiền. Điểm cảnh quan không được nhân lãi dây chuyền; nếu dùng làm điều kiện nội dung phụ, phải có nhiều cách đạt, công khai cách tính và giới hạn đóng góp của vật giống nhau.

### Sáu khu có đời sống riêng

| Khu đề xuất | Công trình và đồ đi cùng | Cảm giác, câu chuyện và lựa chọn |
| --- | --- | --- |
| Bếp vườn | B01–B05; D11–D20 | Mái ngói thấp, sân phơi, hơi bếp và mẹt hàng. Thu lạc để bán ngay, ép dầu hoặc dành làm kẹo; đơn bếp giúp cả cây mới lẫn lúa cũ có thêm đầu ra. |
| Vườn hương trái | B06–B10; D31–D40 | Cây ăn quả bao quanh các nhà rang/sấy nhỏ. Chọn cà phê hoặc ca cao làm nghề chính, phối với mít, gừng và lá dứa; vườn đẹp có đường đi dưới tán. |
| Xóm thợ | B11–B15; D41–D50 | Mái lệch, lò gốm, khung dệt, sân giấy. Làm đồ để bán hay giữ lại trang trí; một sản phẩm có dấu tay nghề nhưng không phải món mạnh hơn bắt buộc phải có. |
| Vườn học nghề | B16–B20; D01–D10 | Bàn mẫu đất, tủ hạt, giàn chăm cây. Người chơi thử một cách canh tác, xem trước hiệu quả, so kết quả rồi quyết định có áp dụng rộng hơn. |
| Sân làng | B21–B25; D51–D60 | Sân sinh hoạt, đờn ca và ký ức đảo. Chuẩn bị chương trình bằng sản phẩm các nghề; làm solo cùng NPC, không cần đủ người online để mở câu chuyện. |
| Đường ven nước | B26–B30; D21–D30 | Tuyến đi bộ/xe đạp qua nhà khách, bưu cục và điểm chăm chim. Khách kể lại góc họ thích; người chơi đổi tuyến trải nghiệm hoặc gói đặc sản, không chỉ tăng số giường. |

Đây là phân khu theo chủ đề để dựng mẫu và kể chuyện, không phải sáu tọa độ đã được xác nhận trên đảo. Người chơi có thể phối đồ giữa các bộ. Lối đi, khoảng nghỉ và tầm nhìn từ camera quan trọng hơn việc xây kín mỗi khu.

### Mười chuỗi mẫu để kiểm thiết kế

Các lượng dưới đây chỉ là công thức prototype; phải cân bằng cùng giá và thời gian trước khi thêm vào Core. ID C/B/D trỏ tới bản Designer. Mọi đầu vào nghề mua ngoài đều hiển thị nguồn và giá; không cần khai thác một cây cảnh để mở chuỗi.

| Chuỗi | Đầu vào → xử lý → đầu ra | Quyết định và nơi dùng |
| --- | --- | --- |
| Bữa bún vườn | 3 lúa hiện có → B01 → 2 phần bún khô | Bán trực tiếp hoặc làm đơn cùng C11 húng quế và C14 dưa leo. Khi nối bếp, ánh xạ sang nguyên liệu bún hiện có; không đếm lại bún như cây mới. |
| Mẹt bánh tráng | 2 lúa + 1 C06 mè → B02 → 2 xấp bánh tráng mè | Giữ cho đơn sân làng hoặc bán; mẻ có sân phơi nhìn thấy được, không bắt đăng nhập đúng lúc để lật bánh. |
| Bánh vườn xanh | 3 C03 sắn → B03 → 2 tinh bột sắn; 1 tinh bột + 1 C10 lá dứa + 1 đường hiện có → B05 → 2 bánh dẻo lá dứa | Chuỗi hai bước dạy chia hàng chờ; B05 nhận tinh bột từ B03 hoặc gói thử nhiệm vụ. |
| Dầu hay kẹo | 3 C04 lạc → B04 → 1 dầu lạc; hoặc 2 C04 + 1 đường → B05 → 2 kẹo lạc | Hai hướng dùng chung cây, đầu ra khác. Không cần thu thêm phụ phẩm nếu chưa có nơi dùng. |
| Gói cà phê đảo | 3 C29 quả cà phê → B06 sơ chế/rang → 1 cà phê rang; 1 cà phê rang + 1 mít sấy B09 + 1 giấy gói → B15 → 1 hộp quà | Giấy gói mua từ NPC hoặc làm ở B13. Dành thành phẩm cho nhà khách B26 hoặc đơn quà; gói quà chỉ tăng giá trị khi có người mua, không nhân tiền miễn phí. |
| Mẻ ca cao | 3 C30 quả ca cao + 1 đường → B07 → 2 thanh ca cao ngọt | Sơ chế/ủ/rang là các pha của một mẻ game. Bán hoặc phối quà với cà phê; không đòi người chơi mua thêm ba máy ngoài danh mục. |
| Gia vị vườn | 1 C07 gừng + 1 C08 nghệ + 1 C09 riềng → B08 → 2 gói gia vị | Phải mở đủ ba giống trước công thức. Đơn có thể dùng gói này; chuyển bếp lẩu chỉ sau khi định nghĩa khẩu vị và công thức hợp lệ. |
| Trái cây để dành | 3 C28 mít → B09 → 2 mít sấy; hoặc 3 C14 dưa leo + 1 muối nhà cung cấp → B10 → 2 hũ dưa muối | Có hai đầu ra bảo quản, vẫn giữ xưởng trái cây cũ và mứt dâu có vai trò riêng. |
| Góc nghề của tôi | Đất sét/chất men mua → B11 → đồ gốm D01/D34/D38; sợi cói mua → B12 → chiếu; bột giấy dó mua → B13 → giấy | Công thức décor chỉ mở sau khi có placement và kho sở hữu. Giấy/chiếu có thể dùng trong đơn nghề; các biến thể hoa văn không được tính thêm loại. |
| Một ngày ở làng | Hoàn tất đơn mẫu B21 + gói quà B15 + chọn chương trình tại B24 → lượt sinh hoạt NPC; B29 chuyển thư hồi đáp | Thưởng chương một lần, sau đó đơn tái chơi có phần thưởng giới hạn. Nhà lưu niệm B22 ghi câu chuyện; không đòi xây cả 30 công trình để thử một sự kiện. |

Chuỗi món ăn chỉ là công thức trong game. Nếu công thức B08 hoặc B15 cần nhiều hơn hai đầu vào, phải mở rộng mô hình công thức hoặc tách mẻ rõ ràng; `RecipeSpec` hiện chỉ có hai loại đầu vào nên chưa thể dùng trực tiếp cho mọi ví dụ.

## 3. Mở khóa theo chương, không dồn 120 món vào cửa hàng

Ba mốc trong bảng Designer là **M1 Bếp vườn → M2 Nghề đảo → M3 Làng hồi sinh**. Đây là tiến độ nội dung, không phải mùa thời tiết hoặc cấp farm mới. Người mới tiếp tục học vòng chơi cũ. Từ cấp farm 3, Nhật ký giới thiệu M1 qua đơn thử dùng cây đã biết. Từ cấp 5 có thể chọn nghề thứ hai trong M1; cấp 7 mở M2 với các tuyến liên nghề; cấp 10 mở M3 và chương tổng hợp. Đạt cấp chỉ cho phép bắt đầu, hoàn thành bài giới thiệu mới mở nhóm công thức tương ứng. Đây là mốc thiết kế đề xuất, chưa sửa logic mở khóa hiện tại.

Mỗi chương nghề đi qua bốn bước:

1. **Làm quen:** gặp nhu cầu của NPC, xem hình công trình và sản phẩm; nhận đủ nguyên liệu mẫu để thử một lần.
2. **Tự làm:** trồng một giống mới, làm một mẻ bằng nguyên liệu tự thu; mở công thức khởi đầu cùng lúc với giống và nơi xử lý.
3. **Chọn hướng:** chọn bán sản phẩm hoặc giữ lại làm đồ/dịch vụ; hai lựa chọn đều tiến chương, tránh ép bán món đang cần.
4. **Để lại dấu ấn:** nhận một bản mẫu trang trí hoặc chi tiết kiến trúc, dựng một góc nghề; mở đơn tái chơi có lựa chọn nguyên liệu.

Chương không yêu cầu mua tất cả công trình trước đó. Điều kiện phải là danh sách phụ thuộc không có vòng: không có cây chỉ mở sau khi giao sản phẩm của chính cây ấy, không có xưởng cần vật liệu chỉ do xưởng đó sản xuất. Nguyên liệu khởi đầu nhập bằng xu hoặc nhận trong nhiệm vụ chỉ đủ mở nghề; không tạo cửa hàng mua vô hạn rồi bán lại có lời.

Người đã cấp 10 nhận lời mời các chương phù hợp nhưng vẫn có thể làm lần lượt. Không xóa tiến độ hoặc bắt quay lại tutorial. Chương đã hoàn thành không trả thưởng mở khóa lần nữa khi đổi cảnh, tải save hoặc gửi lại lệnh.

## 4. Quy tắc cây và sản xuất

30 cây mới cần tên, hình thu hoạch và ít nhất một đầu ra ngay từ đợt chúng được mở. Nhóm luân canh là nhãn cân bằng trong game, không tuyên bố mô phỏng nông học thực tế. Cây lâu năm có vòng tái thu riêng; cây thu một lần trở về đất trống theo luật hiện có. Nhóm L trong brief (C10–C13) đề xuất tái thu lá có giới hạn: cần state số lượt còn lại và quy tắc kết thúc vụ, không mặc nhiên dùng vòng tái quả hiện tại. Không tính màu quả hoặc giai đoạn tăng trưởng thành một loại cây mới.

Mỗi cây có năm hình tăng trưởng, hai trạng thái bệnh/héo, hình nông sản và thumbnail. Nhóm thu lá phải đọc được trạng thái sẵn thu bằng độ đầy của tán; không bắt mọi cây nở hoa mới thu. Cây thu củ cần dấu hiệu ở gốc và hình sau thu. Cây có giàn cần giàn tích hợp hoặc điều kiện xây rõ ràng, không đòi một hệ hỗ trợ vô hình. Hình vườn trang trí và cây sản xuất có thể dùng chung vật liệu nhưng không dùng chung quyền thu hoạch.

### Nhịp thử nghiệm và kinh tế

Code hiện có thời gian trồng ban đầu khoảng 45–270 giây, công thức chế biến 45–100 giây. Prototype bản mới dùng ba nhịp tương đối: nhanh 60–120 giây, vừa 150–300 giây, dài 360–720 giây. Thời gian cụ thể gắn theo nhóm cây trong bảng Designer; cây lâu năm có thời gian tạo tán và thời gian tái thu tách biệt. Đây là dải thử nghiệm, không phải thông số đã cân bằng.

Tính cân bằng theo **giá trị nguyên liệu có thể bán**, không chỉ giá hạt giống. Với một mẻ:

`Lãi tăng thêm = giá bán đầu ra − giá bán trực tiếp toàn bộ đầu vào − phí mẻ`.

So sánh tiếp theo là lãi trên thời gian chiếm máy và diện tích chiếm đất. Nếu mẻ chỉ đáng làm khi có đơn đặt hàng, UI phải nói rõ giá đơn và giá bán thường. Chất lượng đầu vào không được mất âm thầm: cho chọn phẩm chất, hiển thị số lượng theo sao và giá trị bị tiêu thụ trước khi xác nhận. Ghim trong kho hiện chỉ là tùy chọn UI; chưa thể coi nó là khóa nguyên liệu cho mọi giao dịch.

Mục tiêu thử ban đầu: thành phẩm bán thường có giá trị thêm khoảng 15–35% so với bán nguyên liệu thường, đổi bằng thời gian và mặt bằng; phải đo lại bằng mô phỏng và lượt chơi. Không nhân mức này tự động qua mọi bậc: kiểm toàn chuỗi từ cây đến món cuối, đường mua–chế biến–bán, hoàn tiền và phần thưởng tái diễn để tránh vòng sinh xu vô hạn. Giữ nguyên giá và sức mua của nội dung cũ trong lượt concept.

Không tự bán hàng khi hết thời gian. Hàng chờ offline cập nhật bằng mốc thời gian có giới hạn, không chạy bù một vòng giao dịch cho từng giây vắng mặt. Đổi đồng hồ thiết bị không sinh thêm phần thưởng ở online; server sở hữu thời gian và kiểm tra điều kiện như hệ hiện tại.

## 5. Bố trí công trình và trang trí

Footprint trong bảng Designer là mét trên mặt phẳng; đó là kích thước quy hoạch đề xuất. Lưới đặt đồ đề xuất bước 0,5 m, xoay 90°. Không đổi ô đất canh tác 2 × 2 m hay vị trí ruộng cũ. Dành hành lang đi lại dự kiến tối thiểu 1,5 m, vùng chạm trước cửa 1 m; cần kiểm chứng lại bằng camera, collider và model thật.

Luồng cảm ứng: mở Xây dựng → lọc theo nghề/nhóm → chọn mẫu → xem bóng đặt → kéo hoặc xoay bằng nút → xem chi phí và lý do hợp lệ/không hợp lệ → xác nhận. Trong lúc đặt, thao tác không gieo/thu/chạm công trình phía dưới. Có nút Hủy rõ ràng; đổi tab hay mất focus hủy preview, không thanh toán.

60 đồ trang trí là 60 thiết kế riêng. Bản đổi màu, chiều xoay, cấp nâng hoặc đoạn nối sinh tự động chỉ là biến thể. Tất cả đều có ảnh, footprint, mặt quay chính, vị trí hợp lệ và chủ đề; không dùng hộp placeholder khác tên để hoàn thành số lượng.

Đồ treo/đồ trên bàn chỉ gắn vào điểm hỗ trợ hợp lệ, kiểm cả chiều cao và vật đỡ. Đồ nước chỉ đặt ở vùng nước trang trí được đánh dấu; không biến ao cá sản xuất thành nền tùy ý. Cổng và lối đi giữ tuyến truy cập; phần dây/đèn trên cao không tạo collider chặn người. Màu xanh/đỏ của preview phải có thêm icon và dòng lý do để người không phân biệt màu vẫn dùng được.

Di chuyển đồ đã sở hữu không tốn xu; cất đồ trả về kho sở hữu, đặt lại không mua lần nữa. Chỉ bán khi người chơi chọn thao tác bán riêng và nhìn thấy giá nhận. Xưởng còn hàng chờ hoặc công trình đang phục vụ phải giải quyết trạng thái trước khi cất; tuyệt đối không hủy hàng hoặc tạo bản sao hàng khi chuyển vị trí. Đồ đang được dùng làm chỗ ngồi, điểm nhiệm vụ hoặc giá treo phải có quy tắc tháo phụ thuộc rõ ràng.

Không đặt 30 công trình mới lên 36 ô An Nhiên. Prototype dùng khu trưng bày/save cô lập; sau khi khảo sát địa hình mới chọn các vùng xây trên đảo chính. Lưu tọa độ và giới hạn khu là một hạng mục triển khai riêng, không mặc nhiên kế thừa từ việc đảo đã mở rộng cảnh quan.

## 6. Phụ thuộc cần làm trước khi gọi là gameplay

| Phần | Có thể kế thừa | Phải bổ sung/kiểm chứng |
| --- | --- | --- |
| Cây mới | Catalog cây, gieo/thu, kho, thumbnail, batch và luật sao | Khai báo nhóm, tái thu, bệnh, cây có giàn; bộ 60 giống trong khay và toàn bộ caller |
| Xưởng mới | Giao dịch và hàng chờ Development | Công thức nhiều hơn hai đầu vào nếu cần; trạng thái máy/đầu ra mới; không ép dịch vụ vào `RecipeSpec` |
| Dịch vụ/nghề | Nhật ký và đơn hàng | State nghề, đặt chỗ, phiên phục vụ, điều kiện hoàn tất, chống nhận thưởng lặp |
| Trang trí ngoài trời | Model và UI xây dựng làm nền | Quyền sở hữu, placement, lưu/đọc, cất/đặt, undo trước xác nhận, vật đỡ và kiểm tra lối đi |
| Công trình sinh thái | Logic ecology hiện có | Phạm vi tác dụng, giới hạn cộng dồn, bản xem trước hiệu quả; không coi hiệu ứng đã có chỉ vì model có |
| Quán lẩu | Kho bếp và nguyên liệu hiện có | Ánh xạ ID thực phẩm và phẩm chất, catalog khẩu vị/công thức, UI và giao dịch chuyển kho |
| Nuôi cá | `fishStock`, mô hình cá hiện tại | Theo [concept nuôi cá](Fish-Farming-Concept.md); không tự biến các hồ cảnh thành hồ nuôi |
| Online | Request ID, kiểm tra quyền trên server | Giao thức cây/đồ/nghề/placement mới, đồng bộ tương thích, xung đột đặt đồ và giao dịch trùng |

Các món thủ công, thuốc, hạt giống, sợi và đồ trang trí không được chuyển vào bếp như thức ăn. Thực phẩm có thể dùng ở đơn làng trước khi được tích hợp lẩu. Các hệ tương lai phải có đường chơi solo; khách NPC hoặc đơn địa phương thay vai trò bạn bè khi chưa có dịch vụ online công khai.

Khi triển khai, dùng ID bền vững và bảng ánh xạ từ ID concept B/C/D sang ID code; không dùng chỉ số hàng làm ID lưu. Giữ enum/ID cũ, thêm trường có mặc định tương thích, đọc save cũ thiếu/null, bảo toàn tiền/kho/cây/đồ/hàng chờ. Không bump schema rồi reset farm để né migration. Đơn mới chỉ được sinh khi mọi đầu vào cần thiết đã có đường lấy hợp lệ.

## 7. Kế hoạch dựng và tiêu chí nghiệm thu

1. **Thiết kế:** đủ bảng 30/30/60, rà trùng catalog, chốt vai trò và chuỗi phụ thuộc; hoàn thành ở lượt tài liệu này.
2. **Mẫu chứng minh:** chọn một chuỗi trồng–chế biến–dùng, một công trình dịch vụ và một cụm trang trí. Dựng greybox trên save riêng, đo tỷ lệ/đường đi, rồi mới làm model chi tiết.
3. **Sản xuất nội dung theo đợt:** mở cả giống, nơi xử lý, đầu ra và đơn hướng dẫn cùng lúc. Không giao một danh mục chứa nhiều mục chưa dùng được.
4. **Tích hợp và cân bằng:** chạy giao dịch, migration, offline, batch, placement và thử tiến trình từ save mới lẫn save lâu năm.
5. **Rà hình và thiết bị:** Designer xem ảnh ba góc gồm mặt sau, năm giai đoạn cây, thumbnail và Play ngày/đêm; đo thiết bị Android/iOS trước khi công bố tối ưu.

Nghiệm thu model cần hình dạng đọc được từ camera farm, không chỉ ảnh cận. Mái/cột/rui tiếp xúc đúng, biển nghề rõ, đồ không xuyên sàn, tán cây không che điểm chạm; ảnh có vật mẫu kích thước chung. Dùng chung vật liệu/atlas khi hợp lý, tách bộ phận chuyển động; không đặt một đèn realtime hoặc ParticleSystem luôn chạy trên mỗi món trang trí.

Ngân sách sơ bộ để làm prototype: công trình thường 3.000–8.000 tam giác, điểm nhấn tối đa khoảng 12.000; cây trưởng thành 800–3.000; trang trí nhỏ 150–1.500, điểm nhấn 3.000. Đây là mục tiêu thiết kế, chưa phải benchmark và có thể điều chỉnh sau đo góc camera đông nhất. Số draw call, vật liệu, overdraw, bộ nhớ và thời gian tải phải được đo cùng số tam giác.

Khi sửa code/model thực tế: chạy kiểm tra project/model library, Core/server nếu đổi giao dịch; Unity compile; farm-check/play-interface-check và các bridge ngành liên quan. Nếu sửa khung cửa sổ, chạy thêm bộ windows/development/ranch/hotpot theo AGENTS.md. Bổ sung tình huống save cũ, giao dịch thất bại không trừ tài nguyên, yêu cầu lặp chỉ thực hiện một lần, cất/đặt không nhân đôi đồ, đóng bảng không xuyên thao tác, mở bộ 60 giống vẫn kéo/chạm đúng.

Trong lượt concept chỉ kiểm đếm, liên kết và tính nhất quán tài liệu. Không chạy bake hoặc ghi dữ liệu người chơi; không dùng kết quả tài liệu để khẳng định model, Play hay điện thoại đã đạt.
