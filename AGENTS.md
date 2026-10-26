# Harvest Isle — Hướng dẫn làm việc với project

## Đảo trăm nghề đã vào game — 26/10/2026

Người dùng đã cho triển khai sau lượt concept. Designer dựng/rà đủ **30 công trình B01–B30, 30 cây C01–C30, 60 trang trí D01–D60**. `IslandExpansionModels.*` + `IslandExpansionAssets` xuất **330 prefab mới**: 30 công trình, 60 đồ, 30 cây × (năm giai đoạn + bệnh + héo + nông sản). Resources tổng **965 prefab**, catalog **503 loại / 930 biến thể**; thêm hai studio người ngoài Resources là 967 prefab Assets. B/C/D là namespace mở rộng; cây runtime dùng ID tiếng Anh trong `IslandCropCatalog`, model `IX_Cxx`. Không tính lại asset cũ.

**Menu → Đảo trăm nghề**: Nhật ký nghề, Công trình, Làm nghề, Trang trí, Kho sở hữu, Vật tư & hàng, Dịch vụ. Khu mới tâm (135; 0,24; 0), vùng đặt 80×56 m/lưới 0,5 m, có đường và hồ; không thay bố cục/save tiến độ An Nhiên. Đặt/xoay/cất kiểm va chạm, đường, biên, hồ/bờ. Đồ treo/đồ bàn dùng hai socket sân trước xoay cùng công trình và có giá đỡ; phải tháo đồ trước khi cất/di chuyển nhà. Giới hạn 300 món sở hữu; mua/làm nhiều đồ cùng mẫu được. Công trình chỉ một bản/mẫu.

`CropCatalog.All` có **60 giống**, `Original` giữ 30 cũ. M1/M2/M3 yêu cầu cấp 3/7/10 và 0/3/8 lượt nghề hoàn thành; không nâng cap farm. Bốn cây lá C10–C13 thu ba lần; C20–C30 tái quả; sâu/bệnh/đất/phẩm chất dùng luật Harvest hiện có. Roster tuyển người vẫn 72; thợ miệt vườn nhận cây lâu năm mới, không tự thêm hồ sơ/gacha.

`FarmIslandExpansion` Core có **97 công thức, 46 mặt hàng**, hàng chờ nhiều đầu vào, trừ nguyên liệu đúng một lần và nhận theo serial; chỉ dùng hàng thường ★. Cấp nhà 1–3 tăng hàng chờ/giảm thời gian, hiện dùng cùng ngoại hình. B16 hồ sơ giống; B17 báo cáo đất; B18 lịch năm lượt tưới/5 xu, mỗi 20 giây chỉ khi đang chơi và có việc; B19 bảo vệ bằng thiên địch 45 giây sinh thái; B20 đổi tiến độ tái quả lấy +1 lần thu sau; B22 trưng bày nhật ký; B23 ba câu hỏi; B25 ghi chăm thú/cooldown 120 giây; B28 giữ riêng lô theo sao, không bị SellAll. Sáu dịch vụ còn lại chờ 90 giây, mỗi câu chuyện tặng đồ lần đầu. Dịch vụ chưa diễn hoạt NPC/xe đạp; B23 chưa có ruộng mô phỏng. Model tĩnh chưa có chuyển động võng/guồng/đèn kéo quân hay đèn realtime riêng.

Save schema 1 nối cuối `islandExpansion`: chương, đồ/vị trí/cấp, hàng nghề, hàng chờ, nhật ký, lô giữ riêng, tỉa cây và lịch tưới. Save thiếu/null/object Unity rỗng tự khởi tạo; kiểm ID/serial/capacity/geometry/số hữu hạn khi load. Không đổi giá/coins cũ, không reset farm, chưa chống sửa/quay lui save local. Bếp thêm 29 nông sản mới (loại sắn sống) và thành phẩm ăn được; tổng 122 nguyên liệu. Bún/đậu phụ mới nhập đúng ID bếp cũ; đồ thủ công không vào bếp. Mở rộng **solo-only**, chưa có command server mới.

Bridge Edit: `island-assets-sample`, `island-assets-render-sample`, `island-assets-all`, `island-assets-render-all`, `island-catalog`. BuildAll kiểm geometry toàn 330 trước bake; giữ GUID khi bake lại, lưu mesh trước prefab để tránh sai submesh/material. Play: `island-sample-check`, `island-all-check`, `island-open`. Full review dùng save cô lập, thử chuỗi lúa→bún→bếp, migration Unity, mọi 330 prefab, đặt cả 90 loại, hai kiểu giá đỡ, trồng 30 cây mới, save roundtrip, chặn input, giữ farm thật. Ảnh `Art/Previews/IslandExpansion/{Models,Play}`, gallery `index.html`; báo cáo `.tools/model-work/{expansion-*,island-*}.json`. Luôn tự xem ảnh sau bake; không lấy compile thay cho kiểm ảnh. Chưa đo trên Android/iOS hoặc cân bằng nhiều mùa.

Đã đạt Unity compile, **155 Core tests**, **35 server checks**, check_project/check_model_library; bridge farm-check, play-interface-check (60 giống), development-check (60 cây/300 giai đoạn), development-ui-preview, recruitment-check, hotpot-check, online-check và estate-check (36 raycast, thăm/quay về, giữ save). Mẫu và toàn bộ nội dung đều chạy `island-*-check`; đã tự xem bảng tổng hợp, ảnh cận ba góc và ảnh Play UI/cây khỏe–bệnh/giá đỡ. Geometry toàn 330 đạt; prefab lớn nhất 12.248 tam giác (B21), cây lớn nhất 7.732. Đây là kiểm tra Editor/desktop, chưa phải xác nhận ngân sách điện thoại.

## Lịch sử concept mở rộng 30/30/60 — 25/10/2026

Theo yêu cầu dùng Designer trước khi làm, agent vai trò Designer thiết kế `Design/Island-Expansion-Designer.md`: **30 công trình B01–B30, 30 cây C01–C30, 60 đồ trang trí D01–D60**. `Design/Island-Expansion-Concept.md` nối danh mục vào vòng chơi, mười chuỗi mẫu, ba mốc nghề M1–M3, đặt/cất đồ, kinh tế, phụ thuộc và nghiệm thu. Sáu khu chủ đề không phải tọa độ đã đặt trong scene. ID B/C/D là ID thiết kế, chưa phải ID runtime.

**Lượt 25/10 chỉ bổ sung concept; trạng thái triển khai mới nằm ở mục 26/10 phía trên.** Khi đó danh mục thực là 30 CropSpec, chín BuildingSpec, 16 RecipeSpec, cấp farm tối đa 10; model catalog 353 loại / 600 biến thể / 635 prefab Resources. Không tính lại model đã có nhưng thiếu gameplay thành nội dung mới. Lượt tài liệu không thay Core, giá, schema, save, GUID hay bố cục An Nhiên. Mục tiêu sau khi triển khai cây là 60 giống; số prefab phải tính theo giai đoạn/trạng thái/cấp thực tế, không cộng cơ học 120 vào 635.

Khi thực hiện tiếp, dùng brief Designer làm chuẩn ngoại hình và phân biệt vai trò; dựng một chuỗi mẫu trên save riêng trước, xem ảnh ba góc/Play rồi mở rộng. Placement ngoài trời, trạng thái nghề/dịch vụ, công thức trên hai đầu vào và ánh xạ nguyên liệu lẩu cần triển khai rõ, chưa mặc nhiên có. Bản concept dùng xu, không dùng quy đổi VND cũ; M1–M3 là mốc nghề, không nâng cap cấp farm. Lượt tài liệu chỉ kiểm số lượng, ID, liên kết và trùng danh mục; chưa có bake/Play/benchmark cho nội dung mới.

## Nhân công chuyên môn và lật thẻ — 25/10/2026

`WorkerCatalog` có 72 hồ sơ: hai người cho mỗi 30 cây (60), thêm ba bậc cho bốn nghề miệt vườn/tưới tiêu/chăm đất/chăn nuôi. Bốn độ hiếm có 30/19/14/9 người; giá thuê đích danh 120/360/900/1.800 xu; ca 10 việc 4/9/18/30 xu; nhịp 24/20/12/8 giây, kỹ năng thu đúng nghề 0/1/2/3. Chỉ tiêu lượt khi thao tác thành công; không tự gia hạn hoặc làm bù offline. Chọn mặc định chỉ đúng cây, có bật mọi cây với kỹ năng 0 ngoài chuyên môn. Chất lượng vẫn qua luật đất/sâu/bệnh/úng của `Harvest`.

`FarmRecruitment` giữ giao dịch Core thuần C#. `FarmState.recruitment` nối cuối schema 1: roster, ca, phân công, cooldown, huấn luyện, kết quả chờ lật, serial và hai bộ đếm bảo đảm. Save thiếu/null hoặc object Unity rỗng khởi tạo an toàn; nhân công `WorkerState` cũ giữ nguyên. Nhà cấp 1/2/3 có tổng 2/4/6 chỗ, tính cả người cũ. UI không bán hợp đồng cũ mới; giữ API cũ cho tương thích. Kết thúc hợp đồng cũ xác nhận hai bước, không hoàn cọc.

Gacha dùng xu: 30/lượt, 150/5, tỷ lệ cơ bản 60/28/10/2%; từng hồ sơ trong cùng bậc ngang nhau. Mốc 10 bảo đảm bậc 3+, mốc 50 bảo đảm bậc 4 và ưu tiên hơn; bộ đếm reset theo kết quả thực, chạy qua từng thẻ cả trong gói 5. Trùng đổi 1/3/8/20 tài liệu. Kết quả và thưởng chốt trước khi lật, serial chặn xử lý lặp; chưa lật hết không tuyển tiếp. Runtime dùng RNG crypto lấy mẫu không lệch modulo. Huấn luyện tối đa hai bậc (8 TL + 60 xu; 20 TL + 180 xu), mỗi bậc giảm 4 giây, sàn 4; bậc 2 tăng 1 kỹ năng đúng nghề, trần 3. Chưa có chống chỉnh/quay lui save local.

`FarmDevelopment.Workers` có Đội ngũ/Thuê đích danh/Lật thẻ/Hợp đồng cũ trong khung nổi, tìm tên/nghề, lọc bậc, phân trang và hiệu ứng lật. Nhà nhân công có collider mở tab 7. `FarmWorkforce.Specialists` dùng lại prefab người hiện có, sinh model cho người đã phân công và di chuyển minh họa sau giao dịch Core. Không sửa mesh người, thêm prefab hay đổi catalog model. Chưa pathfinding né công trình, chưa kiểm thử/đo hiệu năng điện thoại; chưa cân bằng qua nhiều mùa. Nhân viên quán giữ riêng; tuyển/gacha mới chỉ solo, chưa có command online.

Bridge Play `recruitment-check`: save cô lập, migration JSON Unity, tuyển/phân công, 72 thẻ, lật/tải lại pending, thẻ trùng, thu đúng cà chua/lúa mì vào kho thật, ba model, collider nhà, chặn input và giữ farm thật. Ảnh `Art/Previews/Farm/Workers`, báo cáo `.tools/model-work/recruitment-result.json`. Cần chạy Core/server, bridge này cùng farm-check/play-interface-check/development-ui-preview/online-check và tự xem ảnh. Thiết kế đầy đủ và danh sách tên: [Design/Worker-Recruitment.md](Design/Worker-Recruitment.md).

Đã đạt Unity compile, **133 Core tests**, **35 server checks**, recruitment-check/farm-check/play-interface-check/development-ui-preview/online-check và check_project. Đã xem ảnh đội ngũ, hai chuyên môn cà chua/lúa mì, lật thẻ mới/trùng và người trong Play. `workers-open` mở tab tuyển đích danh trên farm hiện tại mà không mua hay phân công người.

## Cửa sổ cửa hàng và công trình — 25/10/2026

`FarmWindow` đặt bảng IMGUI ở giữa, giới hạn 82% mỗi chiều safeArea, có viền và nền ngoài mờ nhẹ. Phát triển/xưởng, chuồng và bảng online dùng khung chung; chạm ngoài hoặc Esc gọi luồng đóng sẵn có. Chuồng bố trí lại 900×600, sáu tab và bốn thao tác trong khung. Kho bán hàng/đơn hàng UGUI tự co theo safeArea, có backdrop chặn chạm và đóng khi chạm ngoài/Esc. Giữ các guard chặn thao tác ruộng/camera khi bảng đang mở.

Quán lẩu giữ camera farm hoạt động, camera nội thất dùng viewport trùng khung 1200×720; tọa độ chọn bàn và đặt nội thất dùng cùng guiArea/guiScale. Viewport cập nhật theo độ phân giải/safeArea. Không đổi giá, Core, schema, catalog hay save của người chơi. Hai camera cùng render khi mở quán; chưa đo hiệu năng hoặc cảm ứng trên Android/iOS.

Bridge Play `windows-check` chụp bảy màn ở `Art/Previews/Farm/Windows`, kiểm tra khung safeArea, sự kiện đóng qua handler OnGUI trong ngữ cảnh GUI, backdrop UGUI, vị trí bàn trong viewport, chặn thao tác và giữ farm thật. Chờ `ShaderUtil.anythingCompiling` trước khi chụp: ảnh ngay sau lần mở quán đầu tiên có thể thiếu mesh khi Editor còn biên dịch shader. Báo cáo `.tools/model-work/windows-result.json`. Khi sửa khung chạy thêm `play-interface-check`, `farm-check`, `development-ui-preview`, `ranch-check` và `hotpot-check`; kiểm tra ảnh thực trong Play, không chỉ compile.

Đã đạt Unity compile, check_project và sáu bridge nêu trên; đã mở xem ảnh Play của xưởng, chuồng, kho, đơn hàng, nội thất quán, kho bếp và đặt/trang trí nội thất. Giữ save thật; để bảng Xây dựng mở trong An Nhiên cho người dùng xem.

## Chi tiết kiến trúc, lượt 2 — 25/10/2026

Người dùng đánh giá lượt đầu còn thiếu chi tiết. Designer tiếp tục rà ảnh và trực tiếp dựng biển nghề (`StructureModelBuilder.TradeSigns`) cho chín xưởng và sạp chợ: icon relief, viền và giá đỡ theo từng công trình. `RoofCraft` thay bề mặt mái bằng các viên ngói cong có mép chồng/độ dày, úp nóc từng đoạn, diềm và đầu rui; mái rơm có bó/lớp và gân sợi, mái sữa có sóng tôn. Bạt chợ có độ võng, đường may và diềm cong. `Craft` thêm đá/gạch vát cạnh, khung cửa/bản lề/tay kéo, vân ván, đai/chốt máy, chân trụ, bậc lên xưởng, biển khay cây và chi tiết theo nghề; giữ máy quay riêng.

Quán An Nhiên dùng mái ngói bake ở `Resources/Architecture/ShopRoof.asset` (NatureModelAsset, không phải prefab), thêm lan can, gạch góc và nền hiên. `ArchitectureRefresh` dựng cả asset này, tiếp tục giữ 76 GUID prefab và 635 prefab Resources; ghi trạng thái running trước khi làm. Các ảnh lượt đầu giữ trong `Art/Previews/AnNhien/Architecture/Round1`; ảnh hiện tại nằm ở thư mục cha. Kiểm tra lại ảnh cận ba góc và Play sau mỗi bake, nhất là tiếp xúc cột/rui với mái, bạt chợ và khả năng đọc biển. Không thay Core/giá/schema hay khởi tạo lại farm. Chưa benchmark điện thoại; hình học chi tiết hơn có thêm chi phí render.

Đã đạt Unity compile, check_project/check_model_library, estate-check (36 raycast, thu–gieo/lưu–đọc, farm-check/play-interface-check, giữ save chính). Đã tự xem ảnh và có designer rà biển trong Play. Model lớn nhất của 76 prefab đợt này là 20.440 tam giác. Ảnh kiểm tra dẫn tới hạ đầu cột hiên/thanh rui cũ dưới lớp mái mới, nối cột giếng/chuồng đúng mái và hạ giằng/đèn dưới bạt võng.

## Chi tiết kiến trúc công trình — 25/10/2026

Theo yêu cầu dùng designer, agent vai trò designer đã đọc bố cục và rà ảnh ba góc/Play. `StructureModelBuilder.Architecture` bổ sung chi tiết cho 19 loại: nhà, kho, giếng, bốn chuồng, vườn ươm/nhà kính, xưởng và sạp chợ. Có hiên/cửa chớp, cửa bên/sau, chân tường, mái che và giằng, máng thu nước, tay quay giếng, can sữa, khay trái cây, giàn phơi thảo dược. Chi tiết thêm được bake thành một mesh tĩnh mỗi model, bỏ material slot rỗng; giữ phần máy quay riêng. Cấp 1 giữ mẫu đơn giản, cấp 2–3 có chi tiết mới.

`ArchitectureRefresh.Build` / bridge Edit `architecture-build` chỉ dựng lại **76 prefab hiện có**, giữ GUID, cập nhật gallery ba góc và thumbnail; mesh/material nằm ở `Art/Structures/ArchitectureCollection.asset`. Không chạy full library bake để sửa kiến trúc. Các builder gốc đã gọi lớp chi tiết nên lần bake toàn thư viện sau vẫn giữ thay đổi. Catalog giữ 353 loại / 600 biến thể / 635 prefab Resources.

An Nhiên xoay xưởng 180° để hướng mặt tiền về góc chơi. Chuồng dùng đúng ChickenCoop/CowShed/DuckShelter/PigPen cấp 2, hai tổ ong lấy prefab BeeHive; farm thường giữ lựa chọn chuồng cũ. `FarmWorld.Architecture` thêm chi tiết quán An Nhiên, gộp theo material và giải phóng mesh cùng world. Không đổi Core, giá, save hay vị trí ô đất.

Bridge Play `architecture-play` chụp 17 ảnh cận ở `Art/Previews/AnNhien/Architecture`, kiểm tra raycast chín xưởng/sáu chuồng/quán; `estate-check` kiểm tra ruộng, save riêng, thăm/quay về, farm-check và play-interface-check. Báo cáo `.tools/model-work/architecture-{result,play-result}.json` và `estate-result.json`. Luôn mở xem ảnh mới sau bake, gồm mặt sau trong gallery. Ảnh trước sửa giữ tên `Before-*`. Chưa đo hiệu năng trên Android/iOS.

Đã đạt Unity compile, bake giữ 76 GUID/635 Resources, kiểm tra project/catalog, estate-check (gồm farm-check/play-interface-check và giữ save chính). Model nhiều tam giác nhất trong đợt bake là 11.260. Đã sửa theo ảnh: vent kho chuyển sang đầu hồi, bó thảo dược đưa ra trước bảng, chân đá ôm thân tháp, dây đèn nằm dưới bạt và cột/giằng bám mặt dưới mái dốc.

## Nông trại mặc định An Nhiên — 25/10/2026

Theo yêu cầu tạo farm riêng của trợ lý: `FarmEstate` là profile solo có bố cục cố định **70 × 60 m**, scene `Assets/HarvestIsle/Scenes/AnNhienFarm.unity`. Đây là farm mặc định của mẫu An Nhiên, không thay `FarmModel.NewState` hay profile chính. `FarmEstate.CreateState` tạo một lần 36 ô/chín giống, sáu khu nuôi × hai đơn vị, chín công trình cấp 2, 2.500 xu. Dùng các giao dịch Core hiện có để thiết lập; state cuối được Validate. `FarmSaveStore.Load` nhận fallback tùy chọn, save hợp lệ có ưu tiên; tiến độ ở `Application.persistentDataPath/AnNhienFarm`.

`FarmWorld.Estate` dựng khu quy hoạch bằng prefab sẵn có, đường/sân/hàng rào; nhà phía bắc, ruộng giữa, chuồng tây, xưởng đông, vườn cảnh đông bắc, hồ/quán/chợ tây bắc. Vườn cây ngoài ô chỉ là cảnh quan. `FarmGame.IsEstate` chọn bố cục riêng trước khi build. Runtime dùng `game.World.PositionOfPlot` cho raycast, batch, thẻ và hiệu ứng; hàm static `FarmWorld.PlotPosition` vẫn là bố cục farm thường để giữ tương thích các review. Ranch, building và HUD badge dùng tọa độ riêng của An Nhiên; không đổi số ô/giá/schema/Core.

**Menu → Thăm An Nhiên / Về đảo của tôi** lưu rồi tạm ẩn phiên gốc, mở profile riêng, quay lại khôi phục phiên/camera/ánh sáng. Không mở Online trong An Nhiên. Có thanh UGUI năm góc xem; thẻ xưởng/chuồng hiện khi zoom gần. `FarmEstateAssets.Open` tạo/mở scene và Play; bridge `estate-open` (Edit), `estate-check` (Play). Review runner sống riêng để các bài farm-check/play-interface-check có thể tắt farm đang xem mà không dừng coroutine kiểm tra. Ảnh `Art/Previews/AnNhien`, báo cáo `.tools/model-work/estate-result.json`; phải xem ảnh mới trước khi báo xong. Catalog không đổi, chưa đo trên điện thoại.

Kiểm tra đã đạt: Unity compile; 118 Core tests; 36 raycast tại vị trí mới; thu rồi gieo/lưu–đọc lại trên state thử; thăm/quay về giữ phiên gốc; farm-check/play-interface-check; check_project/check_model_library. Đã xem ảnh Play mới đủ bảy góc/trạng thái. Wrapper review xóa trạng thái kết quả cũ trước mỗi bài và đợi dọn GameObject cuối frame để không chọn nhầm farm thử đang chờ Destroy; phép so tiến độ dùng đồng hồ cố định để phân biệt tự cập nhật thời gian với thay đổi kinh tế.

## Xu Mùa Vàng — 25/10/2026

Đồng tiền riêng tên **Xu Mùa Vàng**, viết gọn **xu**. `FarmModel.Money` hiển thị trực tiếp số `coins` với dấu chấm phân nhóm + “ xu”; không còn nhân 1.000 hoặc gắn “đ”. Một coin lưu = một xu; số dư, giá, sức mua, schema và giao dịch không đổi. Thưởng nhiệm vụ khai hoang dùng chung formatter. Các đoạn lịch sử nói hiển thị VND bên dưới đã được thay thế.

`HarvestCoinBuilder` dựng model vàng viền vát, hai bông lúa và hai sóng đảo nổi trên cả hai mặt, 2.352 tam giác / ba material. `StructureModelBuilder.CreateExtra("Coin")` dùng builder này để các lần bake toàn thư viện không quay về mẫu cũ. `HarvestCoinAssets.Build` bake riêng vào `Art/Currency`, giữ GUID `Resources/Collection/Coin.prefab`, chụp chính diện/chéo/sau/cạnh vào `Art/Previews/Currency`, render icon alpha `Resources/Currency/HarvestCoin.png`, cập nhật catalog/gallery/thumbnail. HUD và `FarmCoinFly` dùng các asset này; ví co chữ khi số dư lớn.

Menu **Harvest Isle → Currency → Build Xu Mua Vang / Review Xu Mua Vang in Play**; bridge Edit **`currency-build` / `currency-check`**. `HarvestCoinReview.Begin` chạy farm-check, play-interface-check trên save riêng, kiểm tra sprite ví và chụp ví tối đa 1 tỷ xu; báo cáo `.tools/model-work/currency-{result,play-result}.json`. Khi sửa hình phải tự xem ảnh cận và ảnh Play. Catalog giữ 353 loại / 600 biến thể / 635 prefab Resources; chưa đo trên điện thoại.

Đã đạt Unity compile, 118 Core tests, farm-check, play-interface-check, check_project và check_model_library; đã mở xem ảnh chính diện/chéo/sau/cạnh, khay giống, thu hoạch và ví tối đa trong Play. Review cần Editor có giao diện để ScreenCapture xuất ảnh mới; không coi kết quả logic batch là xác nhận hình ảnh.

## Đưa cơ thể tham chiếu vào nhân vật gameplay — 15/10/2026

**Kiểm tra hình ảnh sau sửa:** người dùng yêu cầu tự xem lại kết quả trước khi báo xong. Với nhân vật phải mở ảnh cận trước/chéo/nghiêng/sau, có và không nón, cùng ảnh trong Play; không coi compile hay kiểm tra rig là đủ để xác nhận hình ảnh. Tóc ghép từ đầu cũ đã bị phản hồi không đạt. `FittedHumanHair` thay bằng bề mặt lấy từ đầu mới, cắt chân tóc/né tai, thêm độ phồng nhẹ và búi thấp nữ. Giữ hoa, khuyên và trang phục; chỉnh quai nón và phần vai/cổ áo. `humans-rebuild` chụp thêm 16 góc tại `Art/Previews/HumanHair/{Male,Female}-after.png`; ảnh `-before` lưu đối chiếu. Bridge `humans-check` chạy kiểm tra tích hợp, `humans-preview` mở xem gần trong Play.

Theo yêu cầu thay phần người và giữ outfit: hai prefab Characters và ba prefab người Collection đã dùng cơ thể/đầu/bàn tay nam–nữ MakeHuman của bản tham chiếu, rig 49 xương. `GameHumanFitting` lấy quần áo gốc từ hai builder, chỉnh vừa tỷ lệ mới và gán trọng số: giữ áo xanh/túi/khăn của nam, áo bà ba hồng dài tay của nữ, quần, nón, tóc/búi tóc, hoa/khuyên và giày/dép. Già Tư/Hải Âu giữ màu áo, kính/râu/gậy và mũ/túi riêng. Mesh tham chiếu là đầu vào dùng chung; không sửa hai prefab studio. Tỷ lệ trong game là 0,85 so với studio, không đổi GUID hoặc số prefab Resources.

`FarmerAvatar` điều khiển bộ xương mới qua `ReferenceMaleMotion` (tắt Update độc lập), giữ pha đi theo quãng đường và nối động tác tưới/trồng/thu. Loại đất/cây minh họa studio khỏi prefab game. `FarmWorld.Imported` giữ vật liệu người để không làm mất texture/alpha mắt. Baker `humans-rebuild` đồng bộ năm prefab, ảnh và thumbnail; `HumanPoseSnapshot` bake CPU các tư thế khi chụp ở Edit, có hiệu chỉnh scale. Các đoạn lịch sử dưới đây nói mẫu chưa tích hợp mô tả giai đoạn trước yêu cầu này.

Kiểm tra bằng `GameHumanReview.Begin`: năm rig/skinned mesh và biến dạng outfit; chạy lại batch-check, farm-check, town-check (10/10 người di chuyển), play-interface-check trên save cô lập. Báo cáo `.tools/model-work/human-integration-result.json`, ảnh `Art/Previews/Farmer-actions.png`, `FarmerFemale-actions.png` và `Farm/Humans-new-body.png`. Catalog vẫn 353 loại / 600 biến thể / 635 prefab Resources (637 tính hai mẫu studio). Mesh người còn nhiều tam giác; chưa tối ưu/đo trên điện thoại, chưa IK địa hình hoặc mô phỏng vải.

## Model nữ thử theo ảnh tham chiếu — 15/10/2026

Lối xem nhanh bản đã tích hợp: menu **Harvest Isle → Characters → View fitted humans in farm**, hoặc `GameHumanReview.OpenPreview`; mở hai nhân vật cạnh nhau trong Play trên farm cô lập, không chạy lại bộ kiểm tra. Trạng thái mở ghi riêng `.tools/model-work/human-preview-result.json`.

Thêm mẫu nữ trưởng thành riêng tại `Assets/HarvestIsle/Experiments/ReferenceFemale/`, dùng bốn morph nữ MakeHuman CC0 đã kiểm chứng Git blob hash theo cùng revision nguồn. Không đổi mesh/prefab bản nam. Áo thun màu đất hồng và quần xanh đậm, có viền cổ/tay/gấu; panel áo làm dịu chi tiết cơ thể. Cao khoảng 1,84 m trong studio, 112.420 tam giác / tám mesh / rig 49 xương, cùng 11 động tác với chỉnh tầm với cho tỷ lệ nữ. Chưa tối ưu điện thoại, chưa nối gameplay hay mô phỏng vải.

`python Tools/build_reference_female.py` dùng chung pipeline hình học/rig, cần numpy/scipy. `ReferenceHumanProfile` phân tách đường dẫn asset, ảnh, scene và báo cáo; wrapper `ReferenceFemaleStudy.Build/BuildMotions/Open/Review` lần lượt dựng ảnh, dựng chuyển động, mở studio và kiểm tra Play. Menu **Harvest Isle → Characters → Build and capture reference female motions / Open reference female motion studio**; bridge Edit `reference-female`, `reference-female-motion`, `reference-female-motion-open`. Đóng gói video bằng `python Tools/render_reference_male_motion.py --female`.

Ảnh ở `Art/Previews/ReferenceFemale/`, video `Motion/ReferenceFemale-clothed.mp4`; báo cáo `.tools/model-work/reference-female-{result,motion-result,play-result}.json`. Bộ kiểm tra dùng đủ 1.084 khung hình/11 động tác, ngưỡng đích IK 1,2 cm và sàn cho cả quần áo; Play kiểm tra chuyển tư thế, chọn, dừng/tiếp tục. Catalog gameplay vẫn 353 loại / 600 biến thể / 635 prefab Resources; hai prefab mẫu riêng đưa tổng Assets lên 637. Bản nữ chờ người dùng đánh giá hình thức.

## Model nam thử theo ảnh tham chiếu — 15/10/2026

**Bản sửa lần 2:** bản ghép khối lần đầu bị người dùng từ chối. Đã dựng lại từ base mesh giải phẫu MakeHuman CC0 và chỉnh tỷ lệ theo ảnh, dùng lưới liền cho mặt/mí mắt/môi, chân mày mỏng bám mặt, bàn tay có cấu trúc ngón. Nguồn và giấy phép: `Assets/HarvestIsle/Experiments/ReferenceMale/SOURCE.txt`; dữ liệu nguồn cùng SHA-256 ở `Design/References/MakeHuman/`. Đây là bản sửa để xem lại, chưa được người dùng duyệt.

Đã đọc sáu ảnh trong `Design/References/Anh_tham_chieu_nhan_vat_nam_nu/` (khung người, bước đi, động tác nam/nữ), dựng một mẫu nam tại `Assets/HarvestIsle/Experiments/ReferenceMale/`. Model cao khoảng 1,84 m, nền cơ thể 109.220 tam giác; bản mặc đồ 112.420 tam giác / tám mesh, rig 49 xương / bốn trọng số mỗi đỉnh; chưa tối ưu điện thoại. Mắt có UV/texture, da dùng vật liệu đất sét. Giữ GUID prefab/OBJ thử cũ, không thay nhân vật đang chơi, save, Core hay catalog. Resources vẫn 635 prefab / catalog 353 loại, 600 biến thể; cùng mẫu nữ hiện có hai prefab thử ngoài Resources, tổng Assets là 637 prefab.

Trang phục theo yêu cầu: áo thun ngắn tay xanh lá dịu, quần dài xanh đậm, có viền cổ/tay/gấu. `Tools/reference_male_clothes.py` dựng panel riêng có độ rộng dư từ mesh nền, nội suy trọng số và bỏ các mặt cơ thể được che kín; được gọi trong `build_reference_male_rig.py`. Đồ theo xương, không mô phỏng vật lý vải. `clothes-report.json` ghi hình học; `rig-report.json` là tổng bản mặc đồ. Video mới `Art/Previews/ReferenceMale/Motion/ReferenceMale-clothed.mp4`; ảnh tĩnh bốn góc cập nhật cùng prefab/scene, không thêm prefab gameplay.

Nguồn dựng: `Tools/build_reference_male.py` (numpy; base mesh/morph/eyes MakeHuman CC0), xuất OBJ/MTL; `ReferenceMaleStudy.Build` import, tạo prefab/material URP/scene trưng bày và render tại `Art/Previews/ReferenceMale/`. Menu **Harvest Isle → Characters → Render reference male study** hoặc bridge **`reference-male`** ở Edit Mode. Scene riêng: `Assets/HarvestIsle/Experiments/ReferenceMale/ReferenceMaleStudy.unity`. Không dùng `humans-rebuild` để dựng mẫu này.

Chuyển động: `Tools/build_reference_male_rig.py` dựng lại nguồn mesh và chuyển rig/trọng số MakeHuman CC0 sang `ReferenceMaleSkin.bytes`; `ReferenceMaleRigging` tạo SkinnedMeshRenderer. `ReferenceMaleMotion` có 11 động tác tại chỗ: nghỉ, đi, chạy, vẫy, ngồi xuống, nhảy, tưới, cuốc, thu hoạch, bê giỏ, trồng; IK hai đoạn cho tay/chân, chuyển tư thế 0,32 giây và đạo cụ. Đây là động tác procedural xem mẫu, chưa nối thao tác farm hoặc IK địa hình. Scene `ReferenceMaleMotion.unity` có nút chọn động tác, dừng/chạy, tốc độ 0,5× và xoay góc nhìn. Menu **Harvest Isle → Characters → Open reference male motion studio**; bridge **`reference-male-motion`** dựng/chụp và **`reference-male-motion-open`** mở Play, đều gọi từ Edit.

Bản chỉnh chuyển động lần 2 theo phản hồi còn thô: `.Poses.cs` chia nhịp chuẩn bị/thực hiện/hồi phục, xoay ngược vai–hông khi bước, lăn gót/mũi theo các điểm đỡ lấy từ mesh, cong ngón theo mặt phẳng lòng bàn tay và đặt cổ tay từ điểm cầm đạo cụ. Tưới có nghiêng bình/dòng nước; cuốc có nhấc–chạm–kéo; thu hoạch quỳ nhổ cây, trồng có nhịp ép đất riêng. Chuyển tư thế sửa bàn chân xuyên sàn và chỉ hiện đạo cụ khi tay gần tới tư thế mới. Cây/đất/nước là đạo cụ xem mẫu, không thêm prefab catalog. Bản này vẫn cần người dùng đánh giá hình thức.

Ảnh/video thực từ Unity ở `Art/Previews/ReferenceMale/Motion/`: `ReferenceMale-motion-v2.mp4`, `Motion-preview-v2.gif`, `All-actions.png` (tên không có v2 cũng được đồng bộ). Build/capture qua `ReferenceMaleMotionStudy.Build`: 1.084 khung/11 động tác ở đúng 30 fps, kiểm tra trọng số, biến dạng hữu hạn, sai số đích IK không quá 1,2 cm và đỉnh thân thấp nhất không xuyên sàn (tính lại bounds sau BakeMesh). `CheckContacts` đo thêm 120 mẫu/động tác. `ReferenceMaleMotionPlayReview.Begin` kiểm tra phát/chọn đủ động tác, tạm dừng/tiếp tục và sàn trong lúc chuyển tư thế ở Play. Báo cáo `.tools/model-work/reference-male-motion-result.json` và `reference-male-play-result.json`. `check_project.py` và `check_model_library.py` đạt; chưa thử điện thoại.

## Nhân vật và thao tác hàng loạt — 15/10/2026

- Model nam/nữ đổi tỷ lệ đầu, màu áo/quần; chân có khớp gối, tay có khuỷu. `FarmerAvatar` chạy pha bước theo quãng đường thật, chuyển động thân/đầu nhẹ và làm dịu khi dừng. Dân phố quay hướng trước khi bước, giảm tốc ở điểm dừng. Năm prefab người dùng rig mới; nữ trong Collection dùng model nữ, Già Tư/Hải Âu giữ phụ kiện riêng. Baker đồng bộ prefab và thumbnail, giữ GUID/số lượng catalog.
- Thanh dưới có **Gieo kéo / Thu hàng loạt / Di chuyển**. Chọn giống trong khay rồi bật Gieo kéo; giữ chuột/ngón tay quét qua đất trống, thả để gieo. Ô được chọn có dấu, hiện chi phí tối đa; đi qua lại không chọn trùng. Thiếu tiền dừng ở ô đầu tiên không mua được, chỉ trừ những ô đã gieo. Chụm hai ngón, mất focus hoặc thả trên UI hủy vùng chọn. Chế độ Di chuyển giữ kéo camera như trước.
- **Thu hàng loạt** kéo chọn cây chín rồi thả để chơi **Nhịp mùa vàng**: 6–12 nhịp, chọn đúng một trong ba giỏ khi vạch vào cửa sổ xanh ±10,5% quanh mốc 72%; nhịp tăng tốc theo số ô/lượt. Tỷ lệ đúng 45%/70%/90% cho điểm kỹ năng 1/2/3; thấp hơn là 0. Điểm đi qua luật sản lượng/phẩm chất Core hiện có. Mỗi nhịp chỉ nhận một lần chạm, bỏ lỡ tính sai. Dừng hoặc mất focus giữ nguyên cây; lúc hoàn thành kiểm tra lại cropId/plantedAt, chặn hoàn thành lặp. Thu từng ô và minigame cũ giữ nguyên.
- Hàng loạt hiện **solo-only**, không đổi schema/giá/Core/server. `FarmBatchActions` dùng giao dịch Core từng ô, không phải giao dịch nguyên lô. Sinh thái/nhân công tạm dừng trong minigame qua `IsHarvesting`.
- Bridge mới `humans-rebuild` (Edit), `batch-check` (Play). Ảnh tại `Art/Previews/Farm/Batch/`, `Art/Previews/Farmer-actions.png`, `Art/Previews/FarmerFemale-actions.png`. Chưa kiểm thử điện thoại; chưa dùng IK bám địa hình.

Kiểm tra: Unity compile; 118 Core tests, 35 server checks; bridge batch-check (kéo/chụm/hủy, chi phí, thu lặp, nhịp đúng/sai/bỏ lỡ, năm rig), play-interface-check, farm-check, online-check và town-check (10/10 người di chuyển) đạt. Kiểm tra ánh sáng đêm dùng độ giảm tương đối so với trưa và ngưỡng đủ sáng, theo palette 14/10. Farm đang chơi được giữ nguyên. Thư viện vẫn 353 loại / 600 biến thể / 635 prefab.

## Cập nhật giao diện chơi trực tiếp — 14/10/2026

`FarmHud` chia thêm `.Play`, `.Trading`, `.World`: khay kéo ngang 30 giống có thumbnail, thẻ hành động cạnh ô, thu nhanh qua `FarmGame.QuickHarvest` (điểm chuẩn 1), hiệu ứng bay về giỏ/tiếng thu có tắt, vòng XP, mục tiêu dẫn thao tác, bảng ba đơn, bán theo loại/phẩm chất/số lượng và ghim giữ hàng. Ghim là PlayerPrefs theo máy, không phải trường save hay khóa giao dịch Core. API SellAll và minigame cũ giữ tương thích; UI thường không gọi bán sạch kho. `FarmTrading.cs` là Core thuần C#, kiểm tra giao dịch trước khi trừ; đơn mới dùng serial ordersCompleted để chặn gửi lại.

`FarmCamera.GoHome` dùng size 5,8, chọn ô qua `Hud.TapPlot`; tọa độ thẻ/badge dùng safeArea và đổi pivot về tâm. Không bật gesture gieo/thu theo kéo: kéo vẫn để di chuyển camera, khay giống dùng ScrollRect. Badge xưởng/chuồng chỉ hiện khi nhìn gần và không có bảng che; `FarmDevelopment.OpenBuilding` lọc công thức. `FarmRanch.OpenPen` xem riêng từng chuồng. Màu HUD/management/hotpot ấm hơn; khẩu vị quán có chi tiết thu gọn. Cừu/vịt trang trí dời sang rừng. Không thêm prefab/catalog.

Solo dừng ecology/worker khi đọc bảng hoặc vào quán; dưới 5 vụ thu chưa tiến sinh thái bất lợi, không tự chữa bệnh có sẵn. Tăng trưởng/production vẫn theo UTC. DayNight 480 giây, đêm sáng hơn, ngày dịu hơn, directional shadow Soft. Server thêm `sellStock` (id, tool=stars, job=quantity), `choiceOrder` (index=slot, job=serial); server vẫn sở hữu đồng hồ, quyền và chống gửi lặp, chưa áp dụng quy tắc tạm dừng sinh thái solo cho online.

Kiểm tra mới: 118 Core tests / 35 server checks; bridge `play-interface-check` kiểm tra UI/30 giống/thu nhanh/giữ farm thật, ảnh `Art/Previews/Farm/Play-redesign/`. Khi đổi UI phải chạy lại bridge này và `farm-check`; khi đổi giao dịch chạy Core/server và `online-check`. Test camera Home đã cập nhật kỳ vọng 5,8. Chưa kiểm thử thiết bị di động. Các mục lịch sử bên dưới mô tả phiên bản trước.

Tài liệu khởi tạo bối cảnh cho các agent và người tiếp tục phát triển project. Cập nhật theo trạng thái được kiểm tra ngày 08/10/2026. Khi code hoặc danh mục asset thay đổi, cập nhật các thông tin tương ứng trong tài liệu này và README.

## Định hướng đã chốt

- Tên game: **Mùa Vàng — Đảo Nông Trại / Harvest Isle**.
- Game nông trại **3D**, phong cách hoạt hình ấm áp, bối cảnh nhiệt đới Việt Nam/Đông Nam Á.
- Engine **Unity 6.3 LTS**, ngôn ngữ **C#**, render bằng **URP**.
- Nền tảng mục tiêu: Android và iOS; màn hình ngang, điều khiển cảm ứng.
- Bản concept cũ từng ghi web/TypeScript và hình ảnh 2D. Người dùng đã chuyển sang Unity 3D; các lựa chọn cũ đó không còn là định hướng triển khai.
- Giao tiếp, nhãn UI và tài liệu dành cho người dùng bằng tiếng Việt. Tên class, file và định danh code dùng tiếng Anh.

## Trạng thái hiện tại

### Mở rộng đảo và dựng lại khu dân cư — 09/10/2026

`FarmWorld.IslandScale = 6` thay scale 2 của `IslandBase`: nền đảo rộng ba lần mỗi chiều, chín lần diện tích. `ExpandedHeight` cùng dùng scale mới; biển, vòng sóng, cây rải, hồ cảnh đông và bến thuyền theo đảo mới. Ruộng, chuồng, công trình chức năng và luật mở đất giữ nguyên. Đây là mở cảnh quan, không thêm ô canh tác tự do hoặc triển khai hệ nuôi cá trong concept.

`FarmWorld.Town.cs`: `TownCenter` (−29, 0.15, 9), vùng loại cây rải x −46…−15, z −7…26; 10 nhà sân vườn, sáu sạp, 10 khách, 10 đèn đường và một đèn quán. Nhà có hiên, cửa chớp, cửa bên/sau, ba mái có cửa sổ áp mái; mesh tĩnh được gộp theo material trong từng nhà, mesh tạm được giải phóng cùng world. Hai vườn nhỏ và quảng trường nối phố. `FarmHotpot` ở (−29, 0.15, 12.3), giữ collider mở quán. `FarmCamera.ShowTown` dùng cỡ 16; biên kéo ±60/±51, zoom tối đa 76, far clip 400; vị trí camera gần theo zoom để có bóng. `FarmModelPark` và promenade dời bắc 36 đơn vị, tránh chồng khu trưng bày lên nền đảo.

Bridge `town-open` mở góc xem khu phố trong Play; `town-check` kiểm tra scale đảo, đủ nhà/chợ, raycast mặt tiền quán, khách đi, ngày đêm và không đổi farm thật; thêm `Island-expanded.png`, `Island-harbor.png` và `Town-gardens.png` bên cạnh Town-day/night trong `Art/Previews/Farm`. Unity compile, `town-check`, `farm-check`, `hotpot-check`, `model-park-check` và kiểm tra cấu trúc/cú pháp đạt. Kiểm tra JSON của Model Park đi qua `new FarmModel(store.Load(...))` như luồng tải thật để normalize inline placeholder hợp đồng Hành trình của Unity; không sửa serializer/Core. Không thêm prefab hoặc đổi catalog/Core/save. Chưa đo hiệu năng trên Android/iOS.

### Thiết kế hệ nuôi cá — 09/10/2026

`Design/Fish-Farming-Concept.md` là đề xuất chi tiết về xây hồ, lọc nước/sục khí, nuôi theo lứa, bệnh/cách ly, kinh tế, UI và migration từ `LivestockState` cá cũ. **Chưa triển khai**, không đổi code/save/asset hoặc số tests. Khi thực hiện cần giữ `fishStock` cho chuỗi chế biến/quán lẩu, bảo toàn quyền sở hữu cá cũ và cập nhật các caller `State.fish.count`.

### Triển khai concept quán lẩu dài hạn — 08/10/2026

Đối chiếu `Design/Hotpot-Endless-Concept.md`, đã nối sáu trụ vào gameplay local. `HotpotEndlessRules`, `FarmHotpotBusiness`, `HotpotServiceSession` là Core thuần C#. `HotpotState.progress` và `HotpotRecipe.timesServed` nối cuối schema 1; nhận cả object thiếu/null, zero-filled và default-filled do Unity tạo khi đọc save cũ, giữ tiền/kho/đồ/hạng hình bếp cũ. `progress.initialized` phân biệt dữ liệu mới với placeholder cũ. Không thêm prefab; dùng lại thư viện hiện có.

- **Khách:** 12 archetype gồm thanh tra, vector ngọt/mặn/chua/cay, ăn chay, ngân sách, yêu cầu thêm cay/ít mặn/không hành. Profile và lượt đến tất định theo seed/chỉ số khách. Nhận/từ chối yêu cầu; nhóm 2–3 người dùng một nồi chung, chấm người thấp nhất. Order giữ bản sao công thức và dự trữ kho; nguyên liệu chỉ trừ một lần khi lên món. Khách chọn món có xét chán món, nên thực đơn rộng có tác dụng thực tế.
- **Món/nguyên liệu:** 64 nguyên liệu: 30 cây + cá/trứng/sữa/mật + 14 thành phẩm ăn được + 16 hàng nhà cung cấp. Sáu công thức chế biến mới là bơ, tương, bao gạo, trà thành phẩm, đậu phụ, viên cá; đậu phụ/viên cá phải làm ở farm rồi chuyển sang. Phân ủ, thuốc, thức ăn vật nuôi không đưa vào thực phẩm. 12 pattern đặc sản nối hình nồi và +15% giá; phát hiện nhà làm sinh bằng nền/phong cách/nguyên liệu chính. Tay nghề 10/50/200 nồi tăng giá 3%/bậc và giảm thời gian 8%/bậc; tạo biến thể không sao chép XP. Chán món riêng từng archetype có sàn 20% và hồi 10%/phút, thu ngân hỗ trợ.
- **Bếp/cao điểm:** mua nâng cấp 1–8 theo tiền và hạng, số chỗ 1/1/2/4/4/4/4/6, cấp 3 mở đun nhanh. Bắt đầu nấu rồi chờ chín mới lên món; nồi giữ chỗ đến khi phục vụ. Sóng 90 giây, nhịp khách/kiên nhẫn/mục tiêu tăng theo công thức; combo lên món trong 8 giây, lưu sóng hoàn thành và điểm tốt nhất. Đóng quán kết thúc lượt; đổi bảng quản lý tạm dừng khách/sóng.
- **Tiến trình:** hạng không còn trần author ★★★; số nồi 15/23/34/51… theo hạng, ít nhất 5 đánh giá gần đây, chất lượng và điểm không gian, rồi phục vụ thanh tra đạt yêu cầu. Bốn tầng vẫn giữ, sáu ô bàn cơ bản tương thích cũ. Mùa 14 ngày từ lúc mở quán đầu tiên hoặc nhận thưởng; hoàn thành có thể sang mùa sớm; 2–4 modifier không trùng từ pool 24, mục tiêu ×1,18. Các số vẫn chặn ở 10^9 để không tràn save, không tuyên bố vô hạn toán học.
- **Vận hành:** nhà cung cấp đổi giá theo tuần UTC/mùa/chi nhánh; UI và thanh toán cùng dùng giá hiện tại. Phí mỗi 300 giây phục vụ gồm mặt bằng và lương, thiếu tiền thành khoản chưa thanh toán và chặn nhận/serving mới. Phục vụ, phụ bếp, thu ngân cấp 1–3; Nhà nhân công mở số vị trí và cấp tương ứng cho đội quán, đội farm giữ riêng. Bán khi vắng mặt phải bật chủ động và có đủ ba vai trò; tối đa 48 nồi/8 giờ, trừ kho thật, phí hai chu kỳ/nồi, chặn nhận lại.
- **Chi nhánh:** hạng 8 + đã hoàn thành mùa + tiền; xác nhận hai bước vì reset tầng/trang trí/bếp/hạng. Giữ sổ, tay nghề, kho, nhân viên và các kỷ lục. Lặp Vườn nhà/Bãi biển/Phố núi/Chợ đêm với cảnh nền tái sử dụng prefab, màu nền/ánh sáng và phân bố khách/giá khác nhau.

UI thêm **Quản lý / Sổ tay / Cao điểm** ở thanh dưới của Tiệm lẩu; thẻ bàn hiện khẩu vị, yêu cầu, tiến độ bếp và giá thực nhận dự kiến. `FarmHotpot.BusinessUI`, `.Branches`, `.DishVisuals` nối luật và model. Runtime phục vụ qua `HotpotServiceSession`; `ServeHotpotRecipe` cũ chỉ giữ tương thích cho caller Core cũ, không dùng cho luồng khách UI mới. Online vẫn bị chặn; bảng xếp hạng online chưa triển khai vì quán chưa có giao thức online.

Kiểm tra: **116 Core tests**, 31 kiểm tra server local; bridge `hotpot-check` kiểm tra hồi quy quán/trang trí, **hotpot-endless-check** kiểm tra Unity JSON cũ, 12 pattern, bếp tám cấp, nấu rồi phục vụ, lưu/đọc lại và ba chi nhánh trên farm cô lập. Ảnh `Art/Previews/Farm/Hotpot-Endless-*.png`, `Hotpot-branch-1/2/3.png`. Chưa đo hiệu năng/cảm ứng điện thoại, chưa cân bằng dài hạn bằng chơi thử nhiều mùa.

### Khu phố Bờ Tây và ngày đêm toàn đảo — 08/10/2026

`FarmWorld.Town.cs` dựng khu phố trang trí ở mũi tây-bắc đảo, cách xa ruộng (`FarmWorld.TownCenter` (−12.6, 4.4); vùng `InTownZone` x −16.6…−8.6, z 2.5…8.0): đường phố nối về phía giếng làng, quảng trường lát đá, **chợ nông sản** ba sạp mái bạt bày nông sản thật từ `Collection/Product_*`, **bốn nhà dân** cửa sổ phát sáng ban đêm, năm cột đèn đường, ghế đá, hoa và đom đóm chỉ hiện ban đêm. Các vòng rải cây/đá/thảm thực vật tự động né vùng khu phố; hồ Pond trang trí dời từ (−11.6, 5.5) về bờ đông nam (10.8, −10.2). **Mặt tiền tiệm lẩu** (`FarmHotpot.BuildExterior`) chuyển về quảng trường (−13.8, 0.15, 5.35), nâng thành nhà hai tầng có ban công, biển hiệu viền vàng, đèn lồng, hai bàn vỉa hè có nồi và hơi nước; chạm vẫn mở quán lẩu như cũ. **Sáu kiểu khách của quán** (`FarmTownStroller`) đi qua đi lại trên phố theo tuyến waypoint, dừng nghỉ tất định theo seed, dùng prefab nhân vật hiện có, collider tắt, thuần trang trí. Menu thêm nút **"Khu phố"** đưa camera tới (hai nút zoom thu còn 60px, nhãn giữ nguyên).

`FarmDayNight` áp dụng ngày đêm cho toàn đảo: chu kỳ **240 giây**, gradient tám mốc (nửa đêm → rạng đông → trưa → hoàng hôn) điều khiển màu/cường độ/góc nắng, ambient, màu sương mù và nền trời; ban đêm bật point light đèn đường (`FarmWorld.TownLights`) và emission cửa sổ/đèn lồng qua material glow đăng ký trong `FarmWorld.TownGlows`; đom đóm trong `TownNightOnly` bật khi trời tối. Mưa làm tối nắng được tính ngay trong FarmDayNight (đọc `RainLevel`); `FarmClimate` giữ nguyên cho các farm không có DayNight. Component **chỉ gắn trong `FarmGame.Start`** cho phiên chơi thật — farm cô lập của các bridge review và Model Park giữ ánh sáng cố định như trước; chu kỳ tạm dừng khi quán lẩu mở vì nội thất tự lưu/khôi phục ambient. Không đổi Core, giá, save schema; không thêm prefab.

Bridge **`town-check`** (phải đang Play): dùng farm cô lập kiểm tra chợ ba sạp, bốn nhà dân, ≥5 đèn, ≥6 khách và có khách đang di chuyển, ép pha trưa/đêm để xác minh ambient sáng–tối, đèn đường bật và cửa sổ phát sáng; chụp `Art/Previews/Farm/Town-day.png`, `Town-night.png`; xác nhận farm thật không đổi. Báo cáo `.tools/model-work/town-result.json`. Chưa đo hiệu năng point light trên điện thoại.

### Hành trình bốn mùa và tiến trình dài hạn — 08/10/2026

**Menu → Phát triển → Hành trình** là tab thứ chín, đồng bộ giao diện dịu màu. Từ cấp 3, chọn một trong ba đề nghị; hợp đồng yêu cầu hoạt động mới sau khi nhận và hàng giao đúng loại/phẩm chất. Bốn hướng: thu vụ mới, thành phẩm từ xưởng, nông sản ba sao, phục vụ quán. Xưởng chỉ đề nghị chuỗi nguyên liệu đã mở và có công trình/vật nuôi cần thiết. Có hủy (xác nhận hai lần), không mất hàng nhưng mất tiến độ hợp đồng. 12 hợp đồng/mùa, tổng kết nhận 12 huy hiệu; bốn mùa thành một năm rồi tiếp tục sinh mục tiêu. Mùa theo nhịp chơi, không theo lịch thật, không hết hạn hoặc reset farm; khác với `FarmSeasons` trang trí 150 giây.

`FarmJourney.cs` lưu tay nghề 30 cây theo số vụ thật kể từ cập nhật, mốc 10/50/150/400/1.000, bậc 1–5 tăng tiền hợp đồng cây đó 2%/bậc. Ba dự án năm cấp dùng cả coins và huy hiệu: giảm thời gian chế biến/ươm hoặc tăng tiền hợp đồng, mỗi cấp 4%, tối đa 20%; chỉ áp dụng lượt mới. Sau bốn mùa mở ghi danh di sản bằng tiền/huy hiệu, tăng bậc trang trí trong UI, không tăng sức mạnh và chưa có tượng/model mới. Tổng mùa, danh tiếng, di sản và số vụ tiếp tục tích lũy sau khi hoàn thiện các nâng cấp. Khối lượng hợp đồng có trần, không tăng mũ theo năm.

`FarmState.journey` nối cuối schema 1, save cũ được khởi tạo không mất tiền/kho/tiến độ; không truy ngược tay nghề từ tổng thu hoạch cũ. Validation chặn danh sách trùng, serial/baseline sai, thứ tự mùa và tràn thưởng; giao dịch kiểm tra đủ trước khi trừ. Unity JsonUtility ghi inline null thành object rỗng: chỉ chấp nhận đúng dạng toàn trường rỗng và normalize sau validation, không chấp nhận hợp đồng lỗi từng phần. Hành trình có guard solo-only, chưa thêm server command; server vẫn dùng Core mới để tương thích save.

Concept, công thức kinh tế, phần đã triển khai và nhánh tương lai được phân biệt tại [GAMEPLAY-DEPTH.md](GAMEPLAY-DEPTH.md). Đây là nền tiến trình lặp dài hạn, không phải cam kết đã có nội dung độc đáo cho nhiều năm thực hoặc đã cân bằng retention. Chưa vùng đảo mới, lai giống, bố trí farm tự do hoặc co-op công trình.

Kiểm tra: **91 Core tests**, gồm mô phỏng gieo–thu/giao 480 hợp đồng qua 40 mùa (10 năm trong game), đề nghị 800 mùa, tiền/phẩm chất/nhận trùng/chuỗi nguyên liệu/đầu tư; **31 server checks**. Bridge **`journey-check`** kiểm tra JSON save cũ, hợp đồng đang nhận và đã hoàn tất, dự án, trang cuối tay nghề, đổi năm, chặn input xuyên UI và giữ farm thật. Ảnh tại `Art/Previews/Farm/Journey/`; báo cáo `.tools/model-work/journey-result.json`. Chưa đo retention hoặc thử điện thoại.

### Làm lại màn Phát triển — 08/10/2026

`FarmDevelopment.Skin.cs` dùng bảng màu trắng ngà/xanh xám và chữ xanh sẫm; header tách tiêu đề, cấp/XP và ví để hiển thị số dư lớn. Các trạng thái chữ được đặt màu rõ ràng, OnGUI lưu/khôi phục color/content/background/enabled để tránh ảnh hưởng giữa các màn IMGUI. Tám tab và API Core giữ nguyên. Tab Xây dựng chuyển thành thẻ ảnh từ `DevelopmentThumbnails`, ba hoặc sáu thẻ/trang tùy trạng thái đất; có nút phân trang, vạch cấp và nút mở khu sử dụng khi công trình đã tối đa. Hai khu đất đã mở được thu gọn thành trạng thái 36/36 ô; chưa mở vẫn hiện nhiệm vụ và điều kiện. Nút xây/nâng chỉ bật khi đủ cấp/tiền/gỗ/quặng, Core vẫn kiểm tra giao dịch. Không thêm asset model hoặc đổi giá/schema.

`FarmDevelopment.Tabs.cs` đồng bộ cả tám tab bằng thẻ và phân trang. Đơn hàng có tiến độ nhiệm vụ/quà ngày; Chế biến tách công thức và hàng chờ; Nhật ký có danh sách thành tích phân trang; Kho tách nông sản/thành phẩm/chăn nuôi (gồm mật ong và cá), tìm tên và hiển thị sao theo từng loại; 30 giống có ảnh và tìm tên; Canh tác tách chăm ô đất, ươm mới và khay đã ươm; Nhân công có thẻ nghề và sức chứa nhà. Các thao tác tiếp tục gọi API Core/online hiện có, giữ guard local-only. Không đổi giá hoặc schema.

Bridge **`development-ui-preview`** trong Play dùng farm cô lập có hàng chờ/khay ươm cả sẵn sàng và chưa xong, kiểm tra chín thumbnail, tám tab, truy cập trang cuối, các nhóm kho, chặn thao tác xuống ruộng và giữ farm thật. Ảnh ở `Art/Previews/Farm/Development-tabs/`; cuối lượt mở Xây dựng ở farm hiện tại và chụp `Development-redesign.png`. Ảnh `Development-new-farm.png` lưu từ đợt kiểm tra bố cục khai hoang trước. Báo cáo `.tools/model-work/development-ui-result.json`. Core 75 bài đạt; chưa kiểm thử cảm ứng trên điện thoại.

Chín ảnh minh họa cấp 3 được lấy từ gallery vào `Resources/DevelopmentThumbnails`; tạo lại qua bridge **`development-thumbnails`** trong Edit Mode, không ghi đè ảnh dùng chung của thư viện. UI ghi rõ cấp minh họa; cấp sở hữu hiển thị riêng trên thẻ. Không thêm prefab.

Đã xác nhận Unity compile và `development-ui-preview` đạt chín thumbnail/tám tab/chặn input/giữ farm; đã xem ảnh cả farm mới và farm cấp tối đa. Để màn Xây dựng đang mở trong Play cho người dùng thử.

### Công viên toàn bộ model và farm chơi thử — 08/10/2026

Scene riêng **`Assets/HarvestIsle/Scenes/FarmModelPark.unity`** dựng đồng thời **600 biến thể thuộc 353 loại / 25 nhóm** từ `FullModelCatalog`. Tất cả mẫu cùng tồn tại trên một bản đồ, không phân trang hoặc thay mẫu khi chuyển khu. Các vườn trưng bày có bệ, nhãn, cây, hồ nghỉ và đường dạo nối về đảo farm; kích thước model được chuẩn hóa để vừa bệ, không sửa prefab gốc. Nút Toàn bản đồ/Về trang trại/25 khu, tìm tên, chạm mẫu, xoay và xem mẫu kế tiếp phục vụ xem gần; nhãn nhỏ chỉ hiện khi zoom gần.

Farm thử có cấp 10, 36 ô đủ 30 giống chín, chín công trình cấp 3, 18 vật nuôi của sáu hệ chăn nuôi, nhà nhân công cấp 3 và quán lẩu bốn tầng có kho/công thức mẫu. Ví **999.000.000 coins = 999.000.000.000đ**, kho cây 500 mỗi loại, có gỗ/quặng và thành phẩm. Nút **Nạp tiền & kho** nạp lại ví, gỗ/quặng, nông sản và kho bếp. Các giao dịch vẫn dùng luật Core. Người chơi tự thuê nhân công để tránh tự thu mất cây trưng bày. Gallery tạm dừng cập nhật FarmGame; trở về farm tiếp tục gameplay. Bản đồ lớn là màn xem/thử trên máy tính, chưa tối ưu hoặc kiểm thử điện thoại.

Mỗi lần vào Play khởi tạo lại farm thử; file tự lưu riêng trong thư mục `ModelPark` dưới persistentDataPath, không đọc/ghi save farm chính. Không thêm prefab hoặc đổi catalog, giá và save schema. `FarmCamera` có bounds/zoom/callback chọn mẫu riêng do park cấu hình; farm thường giữ bounds và camera cũ. URP ánh sáng park dùng bản sao runtime, khôi phục pipeline khi hủy.

Mở qua **Harvest Isle → Showcase → Open all-model farm park** hoặc bridge **`model-park`** ở Edit Mode, sau đó `play`. Bridge **`model-park-check`** kiểm tra đủ 600 resource cùng active, material, 25 khu, farm đầy đủ, nạp tiền, điều hướng và JSON riêng; chụp `Art/Previews/ModelPark/Overview.png`, `Crop-garden.png`, `Buildings.png`, `Playable-farm.png`. Báo cáo tại `.tools/model-work/model-park-result.json`.

Kiểm tra cuối: **75 Core tests**, Unity compile và `model-park-check` đạt, gồm raycast chọn cả 600 bệ và cùng tỷ lệ giữa các giai đoạn/cấp của mỗi loại. `farm-check` đạt vòng gieo–tưới–thu–bán, JSON/backup và input kéo/vuốt/chụm/zoom/chặn UI của farm thường. Save chính không đổi. Scene công viên được để chạy Play cho người dùng xem thử.

### Khai hoang theo cấp, nhiệm vụ và tiền — 08/10/2026

Vào **Menu → Phát triển → Xây dựng** để xem hai khu đất, mỗi khu thêm 6 ô (24 → 30 → 36). Khu 1 yêu cầu cấp 3, thu hoạch 10 vụ, bán nông sản ít nhất một lần và 100.000đ. Khu 2 yêu cầu cấp 5, thu hoạch 30 vụ, giao 5 đơn hàng và 280.000đ; phải mở khu 1 trước. Đây là nhiệm vụ khai hoang cố định, tính tiến độ tích lũy từ đầu game, không phụ thuộc nhiệm vụ tuần hay việc nhận thưởng thành tích.

Bảng hiển thị cấp hiện tại/yêu cầu, tiến độ nhiệm vụ, giá và điều kiện còn thiếu; chỉ bật nút khi đủ cả ba điều kiện. Core kiểm tra lại trước khi trừ tiền, thêm đất và thưởng 10 XP. Không tiêu hao tiến độ nhiệm vụ. Giữ schema 1, các khu đã mở trong save cũ không bị khóa lại; chưa hỗ trợ khai hoang online. Không thêm model hoặc prefab.

Kiểm tra đợt khai hoang: **75 Core tests đạt**, Unity compile đạt; development-check trong Play đạt hai khu/36 ô, chặn nhiệm vụ chưa xong và giữ farm thật. Ảnh: [Điều kiện mở đất](Art/Previews/Farm/Land-unlock-requirements.png). Chưa kiểm thử trên điện thoại.


### Cửa hàng trang trí và thư viện chọn model — 08/10/2026

`FarmHotpot.DecorShop` dùng lưới sáu thẻ có ảnh, tìm theo tên, nhóm, lọc vừa túi tiền và sắp giá tăng dần. Hai mục **Mua đồ mới / Đồ đã có** tách mua mới với chọn đồ đang đặt trong quán. Khung phải xem model 3D qua RenderTexture, có nút xoay. `FarmHotpot.DecorPlacement` cho đặt thử trực tiếp lên vị trí sàn/tường/trần trong phòng, xem model xanh trong suốt, xoay, hủy và xác nhận. Chỉ xác nhận mới gọi giao dịch Core và lưu; chặn xác nhận lặp, vị trí đã có đồ, sai mount và sáu ô bàn dành riêng ở tầng trệt. Di chuyển miễn phí trong cùng tầng; chưa có cất kho, chuyển đồ giữa tầng hay kéo thả tự do. Đổi tab/đóng quán hủy đặt thử; camera và tài nguyên preview được khôi phục/giải phóng. Không thay giá, schema hoặc luật phục vụ.

`ModelLibraryViewer.Cards` thay danh sách chữ bằng lưới chín thẻ, tìm kiếm, nhóm, phân trang; khung model riêng có đổi biến thể, xoay và phóng/thu bằng nút cảm ứng. Thư viện vẫn chỉ xem mẫu; mua/đặt đồ ở Tiệm lẩu → Trang trí. `ModelShopThumbnails.Build` tạo 353 ảnh 192×192 trong `Resources/ModelThumbnails` từ gallery; chạy lại qua bridge **model-thumbnails** ở Edit Mode hoặc tự động sau RenderCatalog. Cây dùng ảnh trưởng thành để dễ nhận loài. Không thêm prefab; số lượng catalog không đổi.

`hotpot-check` bổ sung kiểm tra đặt thử/hủy không đổi tiền và số đồ, mua đúng một lần, di chuyển miễn phí và chặn vị trí sai trên farm cô lập. Ảnh: `Art/Previews/Farm/Hotpot-shop.png`, `Hotpot-placement.png`, `Hotpot-owned.png`. Bridge **library-cards-check** kiểm tra thumbnail, đổi model/biến thể, khôi phục camera và không sửa farm; ảnh `Art/Previews/Library/Unity-library-cards.png`. Chưa kiểm thử cảm ứng hay hiệu năng Android/iOS.

### Tăng mật độ cây trên ruộng — 08/10/2026

`FarmPlot.BuildCropCluster` hiện đặt **16 cây/ô (4×4)** cho cây mùa vụ thường, **20 cây/ô (4 hàng × 5)** cho ngô/mía/lúa/lúa mì/sả. Khoảng cách tâm 0,24; nhóm hàng dùng bước ngang 0,18. Giảm jitter và scale chín từ 0,56 xuống 0,52 để luống dày nhưng vẫn đọc được từng cây. Áp dụng đủ năm giai đoạn và cả bệnh/héo; sáu cây ăn quả lớn vẫn một cây/ô. Đây là mật độ hình ảnh, không đổi sản lượng thu hoạch, giá giống hay save. Gallery model vẫn trưng bày từng prefab riêng. Bridge `farm-density-preview` chụp ruộng 24 ô trên farm cô lập tại `Art/Previews/Farm/Dense-crop-field.png` và `Dense-crop-detail.png`, rồi khôi phục farm thật. Không thêm prefab; tổng catalog không đổi. Chưa đo hiệu năng trên điện thoại.

### Thay toàn bộ cây theo bộ mẫu năm giai đoạn — 08/10/2026

Đã dựng lại **30 loại × 5 giai đoạn = 150 prefab tăng trưởng**, cùng **60 prefab bệnh/héo** theo 10 ảnh trong `Design/References/Cay_trong_30_loai_5_giai_doan` (giải nén từ file người dùng ở Downloads). Model Unity dạng hoạt hình low-poly, không phải mesh trích xuất từ ảnh. Cây có gốc tại mặt đất, không gộp khối đất tham chiếu vào prefab; ruộng tiếp tục dùng DirtTile/WetDirtTile.

`ReferenceCropBuilder` tạo hình riêng cho nhóm củ/lá/hạt/giàn leo/nấm/cây ăn quả. Giai đoạn 3 phát triển thân lá; giai đoạn 4 có hoa/quả non hoặc gần trưởng thành; giai đoạn 5 sẵn sàng thu hoạch. `FarmPlot` và nhãn `FarmHud` dùng các mốc tiến độ 0%, 25%, 50%, 75%, 100%; `FarmWorld.Crop` tải đủ 1–5. Không đổi thời gian trồng, luật thu hoạch/tái quả, kinh tế hay save schema 1. Giữ GUID 120 prefab cũ và thêm 30 prefab `_5`; bệnh/héo được bake lại sau vật liệu cây khỏe để không mất material.

Bridge **`crops-five`** trong Edit Mode chạy toàn bộ bake, đồng bộ trạng thái bệnh/héo, cập nhật catalog/gallery và mở scene trưng bày. Menu **Harvest Isle → Crops → Rebuild all five-stage reference crops** tương đương. Không chỉ chạy builder của riêng lúa/chín cây đầu rồi bỏ qua bản bệnh/héo. Bridge `crop-garden` chỉ dựng lại scene từ prefab hiện có; `crop-garden-check` trong Play kiểm tra 150 model, hình học khác nhau giữa năm giai đoạn, mesh/material của cả 60 trạng thái bệnh/héo và 30 lựa chọn camera. `development-check` kiểm tra từng giai đoạn trên FarmPlot thật của cả 30 giống bằng farm cô lập.

[Xem đủ 30 loại và năm giai đoạn](Art/Previews/CropGrowth/index.html) · [Thư viện model](Art/Previews/Library/index.html). Danh mục được xác minh bằng `Tools/check_model_library.py`: **353 loại / 600 biến thể / 1.800 góc nhìn / 635 prefab**. Chưa đo hiệu năng trên Android/iOS.

Kiểm tra đợt thay cây: **72 Core tests đạt**, Unity compile đạt; `crop-garden-check` đạt 150 hình tăng trưởng khác nhau theo từng loại + 60 prefab bệnh/héo, model lớn nhất 25.248 tam giác; `development-check` đạt đủ năm giai đoạn của 30 giống, thu/tái quả/save và giữ farm thật; `farm-check` đạt 15 hình qua ba vòng gieo–tưới–thu–bán, JSON/backup/input. Đồng hồ của `farm-check` được cố định ngoài lễ hội để kỳ vọng giá thường không phụ thuộc giờ chạy.



### Component nguyên liệu, nồi lẩu và bếp tám cấp — 08/10/2026

Đã bổ sung **56 prefab** theo ba bảng tham chiếu: **12 dụng cụ**, **24 khay nguyên liệu/gia vị**, **12 nồi lẩu thành phẩm**, **8 cấp bếp**. Prefab ở `Assets/HarvestIsle/Resources/HotpotKitchen/`, mesh/material riêng ở `Art/HotpotKitchen`, ảnh thu nhỏ ở `Resources/HotpotKitchenThumbnails`. Dụng cụ gồm bếp gas mini/bếp từ, nồi inox/song vị/bốn ngăn/đất, nắp/nồi hầm/vá/vá vớt/kẹp/bát đũa. Nồi thành phẩm gồm Thái, Tứ Xuyên, cà chua, nấm, hải sản, bò, gà lá é, cá chua cay, dê, cua đồng, Sukiyaki và Shabu-shabu. Đây là model hoạt hình theo mẫu; không phải asset chân thực từ ảnh.

`HotpotKitchenCatalog` chứa ID/nhãn/nhóm/resource và ingredientKey dùng để nối hình ảnh. Mỗi prefab có **`HotpotKitchenItem`**: metadata, kích thước, `contents` và `sockets`; `ShowContents(bool)` bật/tắt thức ăn/nước dùng. **`HotpotDishDisplay.SetDish(0..11)`** đổi nồi thành phẩm. **`HotpotKitchenLevelView.SetLevel(1..8)`** đổi prefab bếp, lặp lại cùng cấp không tạo lại object; cấp ngoài khoảng trả false và giữ model. Điểm gắn nồi/effect theo cấp lần lượt 1/1/2/4/4/4/4/6. Pivot mặt sàn/đáy đĩa/nồi ở gốc; collider root có thể chọn trong Editor, tắt khi component đổi mẫu trong quán.

Phòng lẩu đã dùng bếp bake thay cụm range dựng primitive. Hạng quán hiện tại 0–2 ánh xạ hình bếp cấp 1–3; các cấp 4–8 xem trong Library hoặc dùng API đổi cấp. Component chỉ đổi hình: **chưa thêm luật mua/nâng bếp tám cấp, công thức vùng miền, hoặc thêm nguyên liệu mới vào kho/nhà cung cấp**. Các ingredientKey chưa có trong Core là metadata, không phải hàng mua được. Không đổi giá, kho, công thức đang lưu hay schema 1. Phần bếp được loại khỏi gộp mesh phòng để đổi cấp không để lại model cũ.

Builder tái sử dụng hình học trong `HotpotFurnitureBuilder.Kitchen.cs`/`.Dishes.cs`; `HotpotKitchenAssets` bake mesh/material/prefab giữ GUID. Bridge **`hotpot-kitchen-assets`** ở Edit Mode tạo thư viện; **`hotpot-kitchen-check`** trong Play kiểm tra 56 resource/thumbnail, bật tắt Contents, đổi đủ tám cấp và 12 món, chặn chỉ số sai và xác nhận farm thật không đổi. Gallery riêng: [nguyên liệu và dụng cụ](Art/Previews/HotpotKitchen/Ingredients-tools.png), [12 nồi lẩu](Art/Previews/HotpotKitchen/Dishes.png), [8 cấp bếp](Art/Previews/HotpotKitchen/Kitchen-levels.png), [gallery có tên và ba góc nhìn](Art/Previews/HotpotKitchen/index.html). Bếp được gộp thành một loại có tám biến thể trong Library. Tổng thư viện: **353 loại / 600 biến thể / 1.800 góc nhìn / 635 prefab**. Core vẫn 72 bài; chưa kiểm thử thiết bị Android/iOS.

### Bộ component trang trí quán lẩu — 08/10/2026

Đã bake **52 prefab 3D theo hai bảng tham chiếu: 16 bộ bàn ăn và 36 đồ trang trí/phục vụ**. Danh mục `HotpotFurnitureCatalog` nối thêm kind 8–59, giữ nguyên tám kind cũ và schema 1. `HotpotFurnitureView` chứa kind, kích thước, điểm ngồi và nhóm Serving riêng; các bộ bàn gộp cả ghế, còn đồ phục vụ chỉ trang trí, chưa tạo chức năng máy mới. Vật liệu URP và mesh được bake vào `Art/HotpotFurniture`, prefab ở `Resources/HotpotFurniture`; không sinh lại mesh khi mua. Builder gồm `HotpotFurnitureBuilder.cs`/`.Decor.cs`; `HotpotFurnitureAssets` giữ GUID khi bake lại và gộp mesh theo vật liệu. Các mẫu là phong cách hoạt hình, chưa phải hình ảnh chân thực như tham chiếu.

**Menu → Tiệm lẩu → Trang trí**: cửa hàng có ảnh thu nhỏ, năm nhóm và phân trang. Chọn món rồi chọn ô trống để mua; chọn món đã đặt rồi ô trống để chuyển, chọn lại để xoay. Mỗi tầng có **12 ô sàn, 8 ô tường, 6 ô trần**; Core chặn sai loại vị trí, mua trùng, thiếu tiền và bản lưu sai kind/slot. Tường gắn theo hướng vách và ẩn cùng vách khi xoay camera. Bàn mới có điểm ngồi riêng và ẩn thức ăn đến lúc lên món. Sáu ô đầu tầng trệt vẫn dành cho bàn theo hạng. Chưa kéo thả tự do, cất đồ hay tháo riêng từng ghế.

Bridge **`hotpot-assets`** (Edit Mode) bake/render 52 mẫu, tạo 52 ảnh thu nhỏ, gallery và báo cáo `.tools/model-work/hotpot-assets-result.json`. Gallery riêng: [52 mẫu quán lẩu](Art/Previews/HotpotFurniture/index.html), [16 bộ bàn](Art/Previews/HotpotFurniture/Dining.png), [36 đồ trang trí](Art/Previews/HotpotFurniture/Decor.png). Gallery chung hiện **353 loại / 600 biến thể / 1.800 góc nhìn / 635 prefab**, xác minh từ catalog và filesystem. `hotpot-check` tải và instantiate cả 52 mẫu trong Play, kiểm tra thumbnail/điểm ngồi, mua và JSON từng kind; chụp phòng trưng bày cô lập tại `Art/Previews/Farm/Hotpot-furniture-room.png`. Core: **72 bài**. Chưa đo hiệu năng hoặc kiểm thử cảm ứng trên điện thoại.

### Nội thất quán theo ảnh tham chiếu — 08/10/2026

`FarmHotpot.Interior.cs` dựng nội thất theo hướng quán Việt ấm áp: gạch đất nung, vách kem/khung gỗ và tranh phong cảnh, bếp inox mở có bồn rửa/tủ lạnh/quầy ra món, quầy lễ tân, bảng phấn, hàng rào/chậu dương xỉ, đèn mây với ánh sáng vàng. Bàn vuông có chân/vai gỗ, ghế nan, bếp âm, bát/đũa, khay rau và nồi có hơi nước khi phục vụ. Khách dùng prefab nhân vật hiện có, tạo tư thế ngồi; đầu bếp là trang trí, chưa tự phục vụ. Đây là hình ảnh hoạt hình trong Unity, chưa đạt vật liệu/nhân vật chân thực như ảnh mẫu.

`FarmHotpot.Skin.cs` vẽ HUD xanh sẫm bo góc, nút biểu tượng ở góc dưới trái (Phòng ăn/Nhập hàng/Nấu món/Thực đơn/Trang trí), ví và nút về farm phía trên. Chạm vùng bàn để chọn; thẻ bàn góc dưới phải hiện món/thời gian và nút Lên món. Bàn trống không phủ nhãn lớn; bàn chờ có huy hiệu nhỏ. Thông báo thao tác hiện sáu giây. Các bảng nhập hàng/công thức giữ đầy đủ chức năng. Không đổi Core, save, giá hoặc nguồn nguyên liệu.

Phần tĩnh được gộp mesh theo material; không gộp rig, tường ẩn theo góc nhìn hoặc mesh không đọc được. Cache phòng khi đóng/mở, chỉ dựng lại khi thay bố cục/số tầng/hạng bàn; giải phóng mesh/material/texture tạm khi hủy. Camera nằm trong tầm bóng đổ URP; ánh sáng môi trường farm được khôi phục khi đóng quán. Chưa đo hiệu năng trên điện thoại. Đợt dựng phòng ban đầu không thêm prefab; bộ 52 prefab mới được mô tả ở trên. `hotpot-check` bổ sung kiểm tra khôi phục ánh sáng và tái sử dụng phòng, chụp ảnh thực tế; các bàn bổ sung có khách trong ảnh chỉ là trạng thái trình diễn trên farm kiểm tra cô lập.

### Quán lẩu 3D và công thức tự phối — 08/10/2026

Quán đã chuyển từ phòng ăn IMGUI 2D sang nội thất 3D với camera riêng; các bảng điều khiển vẫn dùng IMGUI. Có tối đa bốn tầng (tầng cuối là sân thượng), 26 vị trí sàn/tường/trần mỗi tầng; tám mẫu cũ và 52 prefab mới trong danh mục Trang trí. Bàn cơ bản 4–6 theo hạng cũ được giữ; sáu vị trí đầu tầng trệt dành cho chúng. Đồ mua đặt/xoay/di chuyển miễn phí sau khi mua, theo ô cố định có lối đi; chưa kéo thả tự do, cất đồ hoặc bố trí ghế riêng. Mở tầng 2/3/4 cần 15/40/100 nồi và 300/1.200/4.000 coins. Sức chứa bàn hiện cùng một đơn/nồi; giá bàn cao mới khác hình thức, chưa có khách VIP/khẩu phần nhóm.

`FarmHotpotModel.cs` giữ luật Core. `FarmState.hotpot` nối cuối schema 1, save cũ null được khởi tạo kho/sổ/bố cục rỗng, giữ tiền và `hotpotServed`. Kho bếp tách khỏi kho farm; chuyển nông sản trừ hàng thật, giữ sao và không bị Bán hàng ngoài farm lấy mất. 15 loại từ farm (12 cây, cá/trứng/sữa); nhà cung cấp chỉ bán nước nền, gia vị, thịt gà/bò, tôm/tôm hùm/bào ngư. Chặn mua nông sản qua nhà cung cấp. Cá/trứng/sữa chỉ phẩm chất thường.

Mỗi công thức 2–6 nguyên liệu khác nhau, 1–3 phần mỗi loại, bắt buộc có hàng farm; chọn một trong ba phong cách, ba nền, lượng mặn/cay 0–3, ba cách nấu. Mỗi nồi tốn 1 nước nền + (1+mặn+cay) gia vị. Nấu thử tốn nguyên liệu đúng một nồi, lưu bản sao độc lập vào sổ tối đa 24 món; phải chủ động đưa lên thực đơn mới bán. Có tạo biến thể và xóa món đã ngừng bán. Điểm 0–100: vị 30, phối hợp 25, tỷ lệ 20, kỹ thuật 15, nguồn farm/phẩm chất 10; phản hồi tất định, không gọi AI và không phải kiểm chứng vị giác thực tế. Giá = ceil(giá trị nguyên liệu gồm sao × (1,35 + điểm × 0,006) × (1 + 0,1 × hạng cũ)). Độ ngon quán là điểm trung bình các nồi đã phục vụ, lưu `reviews`/`tasteTotal`.

Khách chỉ gọi món đang bán và có đủ kho sau khi tính các bàn chờ; đổi bảng quản lý tạm dừng khách/timer, tất cả tầng vận hành khi ở Phòng ăn. UI chặn nấu thử khi còn đơn chờ và ngừng bán món có đơn chờ. Phục vụ trừ đúng kho bếp, +6 XP, tăng counter cũ cho nhiệm vụ/thành tích; API `ServeHotpot(items)` cũ trả lỗi để không bỏ qua bước công thức. Đơn/khách còn là trạng thái phiên, không ghi save; chưa nhân viên quán, tự vận chuyển, khẩu vị từng khách hoặc lợi nhuận offline. Online bị chặn ngay khi mở quán.

`FarmHotpot.Room.cs` dựng phòng bằng primitive dùng material cache khi mở/đổi bố cục; không bake thêm prefab hay sửa catalog. `FarmHotpot.UI.cs` có Phòng ăn/Nhập hàng/Nấu món/Thực đơn/Trang trí. `FarmCamera` chặn input khi mở quán. Bridge `hotpot-check` dùng farm cô lập kiểm tra nhập hàng/công thức/JSON/4 tầng/bố cục/chống phục vụ trùng/chặn thao tác ruộng; `hotpot-open` mở quán thật. Ảnh trong `Art/Previews/Farm/Hotpot-3D.png`, `Hotpot-recipes.png`, `Hotpot-supply.png`, `Hotpot-decoration.png`. Kiểm tra điện thoại vẫn chưa thực hiện.


HUD đã thu gọn: FarmHud chỉ giữ status, Kho/Menu và thanh bốn công cụ. Menu chứa chăn nuôi, camera, hướng dẫn/tiến độ/lưu; giống mở thành khay rồi tự đóng khi chọn. Bảng chi tiết chỉ hiện khi SelectedPlot >= 0 và không có popup; nút × gọi ClearSelection, bỏ cả viền chọn. Kho/menu/hướng dẫn/giống loại trừ nhau; chọn ô trên thế giới đóng popup. Toast sáu giây, không chặn raycast; gieo thành công khi ví còn dưới 12.000đ sẽ kèm nhắc giữ tiền Trị bệnh. Sparkle hành động phân màu: xanh nước = tưới, xanh lá = gieo, vàng = thu hoạch. Tiền hiển thị bằng đồng Việt Nam qua `FarmModel.Money` (1 coin lưu trong save = 1.000đ, save giữ coins như cũ). HUD restyle theo hướng Hay Day, toàn bộ vẽ bằng code trong FarmHud: sprite bo tròn bake bevel sáng-trên/tối-dưới dùng chung mọi panel/nút, sprite tròn cho huy hiệu cấp (vàng viền + xanh lá, số cấp cập nhật theo model.Level) và icon xu; pill tiền nâu sẫm chữ trắng có Outline/Shadow, số tiền nảy nhẹ khi thay đổi; chip thời tiết đổi màu kèm dự báo đếm ngược; nút nền màu tự thêm viền chữ sẫm theo màu nền. Giữ nguyên tên GameObject/nhãn nút vì FarmLoopReview bấm theo text/name. FarmLoopReview kiểm tra chuyển bảng, đóng chi tiết và bán qua Kho; bridge `farm-ui-preview` chụp năm trạng thái UI, kết thúc ở HUD gọn, không thao tác kinh tế.

### Gameplay nông trại

Chống úng: `FarmTool.BuildDrain` nối cuối enum; `PlotState.drainageBuilt` bool mặc định false, giữ schema 1. Giá 12 coins/ô (hiển thị 12.000đ), chặn thiếu tiền/mua trùng; rãnh vĩnh viễn nhân nước vào ×0.25 và thoát thêm .005/giây, kết hợp Drain miễn phí 45 giây. `FloodYieldLoss` trừ 1 từ .55, trừ 2 từ .85; cộng phạt bệnh, floor sản lượng 1. Chỉ xét nước lúc thu, chưa tích lũy tổn thương do úng. FarmPlot dùng lại prefab Collection/DrainStraight cạnh ô, tắt collider. UI chi tiết thêm nút xây, tình trạng bảo vệ và phạt. 44 core tests; farm-check kiểm tra mua qua UI, model, JSON và so sánh ô có/không rãnh qua mưa. Bridge farm-drainage-preview chụp before/after trong farm cô lập, không sửa kinh tế của farm thật.

Scene chính là `Assets/HarvestIsle/Scenes/HarvestIsle.unity`.

Lõi hiện có gieo, tưới, thu hoạch, bán, kho, tiền, tăng trưởng offline và local save. Nông trại khởi đầu với **24 ô** và **120 coins** (hiển thị 120.000đ). Cả 30 cây đã nối vào gieo/thu/kho và native model, mở theo cấp 1–10; ba giống đầu giữ nguyên chỉ số. Sáu cây ăn quả là cây lâu năm: giữ cây sau thu hoạch, tái ra quả trong 60% thời gian, chặt mới giải phóng ô. Thời gian ngắn phục vụ thử nghiệm.

Ruộng gameplay dùng prefab đất xới `DirtTile`/`WetDirtTile`, cây native năm giai đoạn, camera mở gần ruộng. Từ 10/2026 FarmPlot dựng cây mùa vụ thành cụm nhỏ phủ kín luống kiểu Hay Day (`BuildCropCluster`, jitter/xoay/scale theo seed cố định): cây cao (corn/sugarcane/rice/wheat/lemongrass) xếp 4 hàng × 5 cây thẳng luống, còn lại lưới 4×4, giai đoạn chín scale lớn hơn; sáu cây ăn quả (mango/coconut/orange/lemon/banana/papaya) vẫn một model lớn mỗi ô; model bệnh/héo cũng theo cụm. Mỗi instance được `FarmWorld.Vivify` thay material bằng bản sao tăng bão hòa (+28%)/sáng (+8%) cache theo material gốc — không ghi ngược vào prefab bake; cả luống đung đưa nhẹ bằng `FarmCropSway`. Bảng ô đất có nút gieo/tưới/thu hoạch theo trạng thái; hướng dẫn theo dõi riêng gieo, tưới, thu và bán. Hai cờ `hasWatered`/`hasSold` bổ sung vào schema 1, mặc định false khi đọc save cũ. Không đổi giá ba giống hoặc xóa save.

Bridge `farm` mở scene chính, `play` chạy, `farm-check` thử ba vòng chơi qua controller/nút UI, 15 model giai đoạn, lưu/đọc lại, tăng trưởng offline và khôi phục backup trong thư mục thử riêng. Test không sửa nông trại thật; báo cáo `.tools/model-work/farm-loop-result.json`, ảnh `Art/Previews/Farm/Playable-farm.png`. `FarmGame.InitializeSession` nhận store/clock để cô lập kiểm tra; runtime mặc định vẫn dùng UTC và persistentDataPath.

Đã có nền phát triển solo trong `FarmDevelopmentModel` + `FarmDevelopment`: 9 công trình có giá/nâng cấp 1–3 và model, 10 công thức, hàng chờ nối tiếp có UTC/offline, kho thành phẩm/bán riêng, đơn hàng NPC, XP/cấp, quà đăng nhập và 5 trang nhật ký khởi đầu. Vườn ươm 2 khay/cấp, nhà kính che 4 ô/cấp, dùng phân ủ/thảo mộc tại ô đã chọn. Core `FarmAgronomy` có 5 nhóm, nhớ 2 vụ, độc canh, đậu hồi đất, ba chị em, cà rốt–hành và cúc vạn thọ. Không đồng nghĩa với hoàn tất toàn concept; bảng bên dưới chỉ rõ còn thiếu.

Save vẫn schema 1: inventory cho phép 3–30 mục có đủ ba id cũ, constructor bổ sung mục mới; development null khởi tạo mới. Các list phát triển được validation, chống job trùng/nhận trùng/quá slot/tiền tràn. Không đổi enum cũ, không xóa farm. FarmCamera/FarmGame chặn thao tác thế giới khi Development đang mở. Công trình mua nằm phía nam đảo, click để mở chế biến/canh tác. Bridge `development-check` dùng farm cô lập kiểm tra 30 model, 27 nâng cấp, chuỗi bột/bánh/đơn, JSON và chặn thao tác xuyên modal; `development-open` mở menu phát triển trên farm thật. 44 core tests; Editor review chưa thay cho kiểm thử điện thoại.

Chăn nuôi: `FarmLivestock.cs` bổ sung LivestockState chicken/cow và kho eggs/milk riêng, giữ schema 1; inventory hiện được mở rộng thành 30 loại cây. Save cũ thiếu hai state được khởi tạo thành chuồng trống sau validation. `FarmRanch.cs` dựng hai chuồng, model vật nuôi sở hữu, modal quản lý; gà/bò trang trí cũ trong FarmWorld đã bỏ để không nhầm quyền sở hữu. Các loài khác vẫn trang trí. `ranch-check` chạy vòng mua–cho ăn–thu–bán và save/reload bằng bản lưu riêng; sau test khôi phục farm và ánh sáng. Core tests hiện 44 bài. Tham số kinh tế trong GAMEPLAY-DEPTH.md.

Mở rộng lớn 10/2026 — bốn hệ mới, đều local-only (guard `Local` báo "chưa hỗ trợ online"):

1. **Khai hoang đất**: `FarmWorld.PlotPosition` xếp hàng 0 ở bắc, ruộng mọc xuống nam (farm cũ thấy ruộng lật trục bắc–nam một lần, trạng thái từng ô giữ nguyên). `FarmModel.UnlockParcel` mở 2 parcel × 6 ô (24→30→36, giá 100/280 coins), validation chấp nhận 24/30/36 ô; `MaxPlots=36`; mọi vòng lặp Core dùng `State.plots.Count`. Parcel chưa mở hiện biển gỗ + dây khoanh (`SyncParcelMarkers`), nút nằm đầu tab Xây dựng, sau khi mở gọi `World.SyncPlots`. Hàng rào nam dời xuống z −5.5, đường ngang scenery bỏ.
2. **Tiệm lẩu**: xem cập nhật 08/10/2026 ở đầu tài liệu: nội thất 3D, bốn tầng, kho bếp và công thức tự phối; mô tả phòng ăn 2D/tổ hợp ngẫu nhiên trước đây đã được thay thế.
3. **Nhân công**: `workerHouse` (0–3) và `workers` (kind/level) nối cuối FarmState, giữ schema 1, save cũ mặc định 0/rỗng. Nhà cấp N chứa N người và quyết định cấp người mới thuê; Tạp vụ chỉ tưới (việc nhỏ), Thợ trồng trọt cấp 2+ tự thu hoạch (điểm kỹ năng 1), Thợ chăn nuôi thu trứng/sữa và cấp 2+ cho ăn bằng ngô. `FarmGame` gọi `RunWorkers` mỗi 4 giây khi đang chơi solo (không chạy offline). UI ở tab "Nhân công" trong Phát triển (8 tab, nút 128px); `FarmWorkforce` dựng nhà theo cấp ở (6.3, −4.7), avatar người làm từ model có sẵn (ElderTu/FarmerFemale/Farmer) và ba dân làng ambient (Hải Âu ở bến, Già làng Tư bên lửa trại, nữ nông dân dạo đường).
4. **Mùa** (`FarmSeasons`): chu kỳ trang trí 150 giây/mùa xuân→hè→thu; chỉ nhuộm material xanh của cây thiên nhiên theo whitelist tên ("Tree/Palm/Fern/Shrub/Bamboo/Hill/Wildflowers/Grafted/Forest"), thu ngả vàng + particle lá rơi; không đụng cây trồng, nhân vật hay luật chơi; toast báo chuyển mùa.

5. **Cây lâu năm**: sáu cây ăn quả (`FarmModel.IsPerennial`) không bị xóa khi thu — `PlotState.harvests` (nối cuối, schema 1) tăng dần, từ vụ hai cây tái ra quả trong **60% thời gian**; cây lâu năm đứng ngoài luân canh/độ phì (lịch sử vụ đóng băng tới khi chặt). `ChopPerennial` giải phóng ô và ghi cây vào lịch sử vụ; nút "Chặt cây lâu năm" ở tab Canh tác. Thu hoạch còn có hiệu ứng `FarmCoinFly`: ba đồng xu vàng bay vòng cung từ ô về góc ví trên màn hình.

6. **Nông sản theo sao**: `fineGoods`/`primeGoods` (nối cuối FarmState) chứa hàng ★★/★★★; thu hoạch chấm sao theo điều kiện (3 lượt đúng + độ phì 2 + sạch bệnh/úng/sâu → ★★★; ≥2 lượt + sạch → ★★), giá bán ×1,35/×1,8 (`StarUnitPrice`), bán kèm Bán hàng và hiển thị ở tab Kho đầy đủ. Chế biến/đơn hàng/lẩu vẫn dùng kho thường (`Count`).
7. **Sâu bệnh + minigame bắt sâu**: `PlotState.pest` (0/1) sinh tất định khi nắng khô (hash climateTime/index), −1 sản lượng tới khi bắt; `CatchPests(index, caught)` cần 2/3 con, 3/3 được bảo vệ 30 giây; minigame chạm sâu 3 lượt trong `FarmFieldGame`, nút đỏ trong bảng ô đất, ốc sên hiện trên ô (Collection/Snail). `IsHarvesting` giờ gồm cả FieldGame.
8. **Minigame cấy mạ**: gieo giống có khay ươm sẵn sàng mở thanh canh vạch (`FarmFieldGame`); trúng vùng xanh → `Apply(..., transplantScore:1)` cho đầu đà 35% thay 25%.
9. **Vịt + heo**: `LivestockCatalog` nối duck/pig, state `pig`/`duck` nối cuối FarmState; vịt đẻ trứng chung kho eggs, **heo ủ phân** vào kho thành phẩm compost (khép vòng độ phì); FarmRanch 4 chuồng (vịt cạnh chuồng gà, heo cạnh chuồng bò), UI lưới 2×2; online chặn vịt/heo. Thợ chăn nuôi chăm cả 4 chuồng.
10. **Thành tích**: 10 mốc trên counter sẵn có (`FarmModel.Achievements`), nhận một lần qua bitmask `achievementsClaimed`, thưởng tiền + 15 XP, UI trong tab Nhật ký.

11. **Ong + Ao cá**: `LivestockCatalog` nối bee/fish (state `bee`/`fish` + kho `honey`/`fishStock` nối cuối FarmState). Thả ong **miễn phí** nhưng cần ≥2 ô đang trồng nhóm Hoa/Đậu (`FloweringPlots`); 120 giây ra 2 mật (18/món). **Ong thụ phấn**: có ong + có hoa → nhóm Quả +1 sản lượng khi thu (trong `HarvestYield`). Cá: 60 mua, 5 thức ăn, 140 giây ra 2 cá tươi (8/món). FarmRanch giờ 6 chuồng lưới 2×3 (tổ ong tự dựng bằng primitive, ao cá dùng prefab Pond, cư dân ong = Collection/Bee + FarmFlutter, cá = Koi + FarmFishSwim); 4 chuồng mới chặn online.
12. **Nhiệm vụ tuần**: 3 nhiệm vụ sinh tất định theo tuần UTC (`EnsureQuests` gọi trong `ObserveTime`; `questWeek`/`quests` nối cuối FarmState): thu hoạch/nồi lẩu/thành phẩm/đơn hàng với baseline đầu tuần, thưởng = target × hệ số, +20 XP, nhận một lần mỗi tuần; UI đầu tab Đơn hàng.
13. **Lễ hội Đom Đóm** (`FestivalActive`): 2 phút mỗi chu kỳ 10 phút UTC — giá Bán hàng ×1,2 (tính trong SellValue/SellAll), khách lẩu đến nhanh gấp đôi, toast báo mở/đóng (không toast khi vào game giữa lễ hội).

**Quy tắc mới**: không dùng PowerShell `Set-Content`/regex rewrite trên file nguồn có tiếng Việt — đã từng gây double-encoding toàn bộ chuỗi trong FarmGame.cs (đã sửa 10/2026); sửa hàng loạt bằng Edit tool hoặc script Python đọc/ghi UTF-8.

Core tests hiện **72 bài**; `development-check` mở rộng xác minh landParcels/hotpot/workers, chu kỳ tái quả + chặt của cả 6 cây lâu năm, kho sao, vịt/heo + phân ủ, ong (chặn khi thiếu hoa → thả → thu mật), ao cá, 3 nhiệm vụ tuần nhận một lần, nhận thành tích một lần, và chụp `Hotpot-interior.png`. Các bài quán lẩu mới kiểm tra kho nhập, phẩm chất, bộ chấm công thức, giao dịch nguyên tử, mở tầng/bố trí đồ và xóa công thức; bridge `hotpot-check` kiểm tra riêng vòng quán 3D.

Cập nhật: bệnh cây và úng cục bộ đã nối cho 30 giống, chưa ngập toàn đảo. Luật chi tiết ở `GAMEPLAY-DEPTH.md`. Core partial `FarmEcology.cs` mô phỏng theo bước một giây chỉ khi đang chơi, không dùng UTC để tạo thiệt hại offline; `HarvestChallenge.cs` chứa ba kiểu chấm điểm. Các trường mới nước/bệnh/bảo vệ/climateTime mặc định 0, giữ schema 1. FarmGame mở FarmHarvestGame trước khi thu; FarmClimate tạo mưa, FarmPlot hiện vũng nước và prefab bệnh/héo. `farm-check` hoàn tất cả ba mini-game, kiểm tra hủy không mất cây và đọc lại trạng thái sinh thái từ JSON. `Tools/test-core.ps1` compile tất cả file Core.

### Model và scene chạy thử

Riêng gameplay FarmWorld đặt IslandBase scale (2,1,2), diện tích gấp 4; không scale nhà/cây/ruộng hoặc prefab đảo dùng chung của playground. `FarmWorld.Expansion.cs` lấy Height(x/2,z/2), thêm cây biên, hai hồ và đất trống. Harbor dịch (-9,0,-5.8); boat bobbing dùng local position dưới harbor. Camera kéo tới ±20/±17, zoom tối đa 28, có Toàn đảo/Về ruộng. `farm-check` kiểm tra bounds và raycast đất mới ngoài biên cũ.

Scene `Assets/HarvestIsle/Scenes/AnimalPlayground.unity` chạy độc lập với kinh tế/save của scene chính. Có **14 nhân vật**: một nông dân và 13 loài; 11 nhân vật trên cạn đi quanh đảo, ba loài cá bơi trong ao/hồ.

Snapshot danh mục hiện tại:

| Nhóm | Nội dung |
| --- | --- |
| Toàn project | 635 prefab |
| Gallery chung | 353 loại, 600 biến thể, 1.800 góc nhìn |
| Cây | 30 loại: 24 cây mùa vụ và 6 cây ăn quả |
| Trạng thái cây | 5 giai đoạn tăng trưởng + bệnh + héo cho mỗi cây |
| Cấp công trình/máy | 30 loại có cấp 1–2–3 |
| Nhân vật | Nông dân, nữ nông dân, Già làng Tư, Thương nhân Hải Âu, Đom Đóm Vàng |
| Sinh vật | 13 loài vật nuôi/cá/chim và 8 loài nhỏ, sâu bệnh/thiên địch |
| Sản phẩm | 30 nông sản và 15 thành phẩm |
| Các nhóm khác | Địa hình, cây cảnh/rừng, phòng hộ, dụng cụ, trang trí, cấy ghép, thời tiết/lễ hội và đồ hư hỏng |

30 prefab gốc của các công trình/máy vẫn được giữ bên cạnh các cấp, nên tổng prefab lớn hơn số biến thể trong gallery. Dùng `Art/Previews/Library/models.json` và `coverage.json` để xác nhận số lượng mới nhất; không duy trì số lượng chỉ bằng sửa chữ trong tài liệu.

Scene chạy thử có bảng đổi cấp, nút chuyển sang đảo trưng bày 16 mẫu sản xuất/giao thương và **Xem TOÀN BỘ MODEL**. Thư viện cho phép lọc, tìm kiếm, chuyển biến thể, xoay và zoom.

### Thời tiết

`PlaygroundWeather` có năm chế độ Nắng/Gió/Mưa/Bão/Tuyết, gồm particle, cây dừa lay, gợn nước, mặt đất đổi màu khi ướt, chớp và âm thanh tổng hợp. Có nút bật/tắt tiếng và **Sét thử**. Hiệu ứng tạm ẩn khi mở thư viện model và khôi phục khi trở lại scene.

PlaygroundWeather vẫn là demo hình ảnh/âm thanh độc lập. Scene gameplay dùng FarmClimate/FarmEcology để tạo mưa, úng ô và bệnh ảnh hưởng sản lượng; chưa có hư nhà, ngập toàn đảo, lịch mùa dài hạn hoặc mô phỏng server.

## Cấu trúc quan trọng

| Đường dẫn | Vai trò |
| --- | --- |
| `Assets/HarvestIsle/Scripts/Core/` | Domain độc lập Unity: cây, kho, tiền, lệnh và kiểm tra dữ liệu |
| `Assets/HarvestIsle/Scripts/Runtime/` | Gameplay, UI, camera, save, rig, chuyển động, thư viện và thời tiết |
| `Assets/HarvestIsle/Scripts/Editor/` | Cấu hình project, build, sinh mesh/prefab, render gallery và công cụ kiểm tra |
| `Assets/HarvestIsle/Resources/` | Prefab/material/catalog được runtime tải |
| `Assets/HarvestIsle/Art/` | Asset mesh/material sinh ra và shader URP |
| `Assets/HarvestIsle/Scenes/` | Scene game chính và scene chạy thử |
| `Art/Previews/` | Ảnh render, gallery HTML, manifest và danh sách CSV |
| `Tools/Templates/` | Template gallery; sửa template trước khi sinh lại HTML |
| `Tools/Tests/`, `Tools/test-core.ps1` | Kiểm tra domain bằng compiler .NET trên Windows |
| `Tools/check_project.py` | Cấu trúc, GUID, asset nguồn và cú pháp C# nếu có Tree-sitter |
| `Tools/check_model_library.py` | Độ đầy đủ và ảnh của thư viện model |
| `Tools/asset-manifest.json` | Nguồn, giấy phép và SHA-256 asset Quaternius |
| `IOS-READINESS.md` | Đánh giá và checklist đưa prototype lên iOS |

Các file runtime chủ chốt: `FarmModel.cs` nằm trong Core; `FarmGame.cs`, `FarmWorld.cs`, `FarmWorld.Scenery.cs` (trang trí thuần hình ảnh: mây trôi, chim lượn, bọt sóng, sương chân trời, props bãi biển/thảm thực vật, ong/đom đóm, và bố cục làng kiểu Hay Day — đường đất DirtPath nhuộm nâu quanh ruộng, chuồng cừu rào gỗ có cừu/vịt trang trí, vườn cây ghép 2×3 phía đông bắc, cổng lễ hội/lồng đèn/bảng đơn/lửa trại/tháp canh/kho làng, cụm dụng cụ và luống hoa; tất cả tránh vùng FarmRanch/FarmDevelopment InClearing, collider tắt, không đụng kinh tế), `FarmWorld.Town.cs` (khu phố Bờ Tây: chợ/nhà dân/đèn đường/khách dạo phố, vùng InTownZone), `FarmDayNight.cs` (chu kỳ ngày đêm toàn đảo, chỉ phiên chơi thật), `FarmPlot.cs`, `FarmHud.cs`, `FarmCamera.cs`, `FarmSaveStore.cs`, `FarmerAvatar.cs`, `FarmAnimalAvatar.cs`, `PlaygroundDirector.cs`, `ModelLibraryViewer.cs`, `PlaygroundWeather.cs`.

## Môi trường và cách chạy

- Thư mục hiện tại trên máy phát triển: `D:\game-claude`.
- PowerShell trên Windows.
- Unity đang dùng: **6000.3.25f1**; nguồn xác nhận là `ProjectSettings/ProjectVersion.txt`.
- Editor hiện được cài ở `C:\Users\PC\AppData\Local\Unity\Editors\6000.3.25f1\Editor\Unity.exe`. Đây là đường dẫn máy hiện tại, không hardcode vào runtime.
- Package chính: URP `17.3.0`, Input System `1.17.0`, UGUI `2.0.0`; các module built-in gồm audio, physics, particle system và screen capture.
- Máy đã có bộ Android SDK/NDK/OpenJDK. Chưa xác nhận APK hoặc build iOS chạy trên thiết bị thật.

Mở project bằng Unity Hub hoặc Editor, rồi chọn menu dưới **Harvest Isle**:

- **1. Configure project**, **2. Open game**: cấu hình và scene gameplay.
- **Animals → Open roaming playground and Play**: 14 nhân vật chạy thử.
- **Library → Open complete model gallery**: thư viện ảnh toàn bộ model.
- **Weather → Open playground with weather and Play**: scene có thời tiết, mặc định Mưa.
- **3. Build Android APK**, **4. Build Windows demo**: lệnh build hiện có.

`ProjectSetup.Configure()` hiện đặt danh sách build chỉ gồm scene game chính. Muốn xuất bản demo chứa playground/thời tiết/thư viện phải chọn scene đó trong cấu hình build phù hợp.

## Làm việc với Unity Editor đang mở

Kiểm tra tiến trình Unity và project đang mở trước khi chạy lệnh. Không mở batch process hoặc Editor thứ hai trên cùng project đang bị Editor giữ khóa.

`EditorWorkBridge.cs` hỗ trợ xử lý công việc trong Editor hiện tại qua file `.tools/model-work/request.json`. Đợi Unity import/compile thành công trước khi gửi lệnh dùng code vừa sửa.

Ví dụ yêu cầu Play bằng PowerShell:

```powershell
New-Item -ItemType Directory -Path .tools/model-work -Force | Out-Null
Set-Content -LiteralPath .tools/model-work/request.json -Value '{"command":"play"}' -Encoding utf8
```

Các lệnh hiện có:

| Lệnh | Tác dụng |
| --- | --- |
| `play` | Yêu cầu vào Play, không bake lại asset |
| `refresh` | Yêu cầu AssetDatabase.Refresh trong Editor hiện tại; đợi compile/import xong trước lệnh tiếp theo, không cần đưa Unity lên foreground |
| `crops-five` | Edit Mode: bake đủ 150 giai đoạn và 60 bản bệnh/héo, cập nhật gallery và mở vườn mẫu |
| `crop-garden-check` | Trong Play của CropGrowthShowcase: kiểm tra hình học/vật liệu và đủ 150 cây + 60 trạng thái |
| `hotpot-kitchen-assets` | Edit Mode: bake 56 prefab nguyên liệu/dụng cụ/nồi/bếp, cập nhật gallery và thumbnail |
| `hotpot-kitchen-check` | Trong Play: thử 56 component, đổi 8 cấp bếp/12 nồi và xác nhận không đổi kinh tế |
| `structures` | Dựng/render bộ công trình ban đầu và cập nhật scene |
| `levels` | Dựng/render cấp của 9 công trình ban đầu |
| `production` | Dựng/render 16 mẫu sản xuất/giao thương và thêm đảo trưng bày |
| `complete` | Dựng toàn bộ bộ bổ sung, render thư viện và thêm màn xem model |
| `weather` | Tạo material thời tiết, thêm controller vào scene và yêu cầu Play |
| `library-check` | Trong Play: thử tải 600 biến thể; kết thúc ở màn thư viện |
| `weather-check` | Trong Play: thử năm chế độ, chụp ảnh và xác nhận sấm; kết thúc ở Mưa |
| `production-view` | Trong Play: kiểm tra chuyển camera giữa đảo chính/khu sản xuất |
| `town-check` | Trong Play: farm cô lập kiểm tra khu phố (chợ/nhà/đèn/khách đi lại) và chu kỳ ngày đêm, chụp Town-day/Town-night |

Các lệnh dựng asset có thể dừng Play rồi mở lại. Các lệnh kiểm tra có điều kiện **phải đang Play**; không gửi chúng khi Editor đang ở Edit Mode. Gửi tuần tự, không ghi đè request khi đang có `active.json`.

Kết quả thường nằm ở `.tools/model-work/result.json`; kiểm tra thư viện ở `library-result.json`, thời tiết ở `.tools/weather/status.json`. Xem log Editor và thời gian cập nhật của kết quả để tránh đọc báo cáo cũ. Sau domain reload, yêu cầu Play qua lệnh `play` nếu callback mở Play chưa chạy.

Khi người dùng đang làm việc ở cửa sổ khác, ưu tiên bridge; không gửi phím/click vào desktop nếu chưa xác nhận đúng cửa sổ Unity. Không gửi phím sau khi gọi lấy foreground thất bại. Sau công việc mà người dùng yêu cầu xem trong Play, để scene phù hợp tiếp tục chạy.

## Quy tắc sửa code và asset

Vũng úng FarmPlot dùng bốn prefab `Resources/Effects/Puddles`, bake bởi `PuddleAssets` qua bridge `puddles` (mở scene chính, sau đó gửi `play`). Mesh phẳng 257 vertex, mép feather bằng vertex alpha; shader URP `FarmPuddle` có gợn mưa theo MaterialPropertyBlock, không thêm collider hay material riêng mỗi ô; alpha tối đa 0.62 để luống và cây con vẫn đọc được qua mặt nước. Cao độ -0.030 đến -0.009 nằm giữa rãnh và đỉnh luống; ngưỡng hiện vẫn 0.55, không đổi luật sinh thái/save. `farm-check` xác nhận mặt phẳng, shader, raycast collider và thoát nước; ảnh toàn ruộng `Waterlogging-disease.png`, cận cảnh `Puddles-detail.png`. Bốn prefab là hiệu ứng gameplay, không thêm loại vào gallery; tổng project 635 prefab, catalog prefabCount đồng bộ theo filesystem.

FarmPlot chỉ hiện một viền chọn bám theo đường bao bo góc của đất xới; không vẽ vòng tròn chọn hoặc vòng tròn quanh tất cả cây chín. Viền chuyển vàng khi ô được chọn có cây chín. Bridge `farm-selection-preview` chọn ô ở chế độ Xem ô và chụp ảnh, không gieo/thu hay đổi tiền.

Toàn bộ 29/30 giống đã tạo hình lại kiểu Hay Day trong 10/2026 (lúa giữ builder riêng `RiceModelBuilder` v2 vốn đã chi tiết): bộ 9 giống trong `CropCollectionBuilder` (Version 4) — cà rốt cụm 3 củ vai cam lộ đất, ngô cao 1.22 bắp lớn hạt nổi cờ vàng, bí đỏ quả chính + quả phụ + tua cuốn, cà chua 6 quả lớn, hành củ to, đậu leo trái dài 4 hạt, dưa hấu to hơn, mía 4 cây cao 1.18, cải rổ dày; 20 giống trong `CompleteCropBuilder` — đậu 7 trái, dâu 7 quả mọng có hạt, cúc vạn thọ hoa lớn, ớt sai trái đỏ, sả/rau muống/khoai môn/sen/nấm/bắp cải/tỏi/củ cải dày và to hơn, lúa mì 9 bông trĩu, trà tuyết thêm hoa trắng, xoài/cam/chanh quả lớn, dừa 7 trái, chuối buồng 4 nải, đu đủ quả to. Màu cam chung đổi sang `F28C2B` trong `CropMesh.Colors`. `CropCollectionAssets.Ensure()` tự bake lại khi tăng `Version`. **Ràng buộc bắt buộc**: `CropCollectionAssets.Rebuild()` hủy material cũ của bộ này, làm prefab `_Diseased`/`_Wilted` (dẫn xuất từ stage 4) mất tham chiếu và hiện magenta — sau mọi lần bake bộ crop phải chạy bridge `complete` để dựng lại states và render Library. Gallery phụ `Art/Previews/Crops` (menu Crops → Render full crop collection) chưa render lại theo hình mới; nếu chạy menu đó thì sau đó cũng phải chạy `complete` vì nó gọi lại Rebuild().

Đất trồng `DirtTile`/`WetDirtTile` được sinh bằng `EnvironmentModelBuilder.Soil.cs`: mặt liền, bo góc, luống xới và normals mượt; giữ kích thước 2 × 2, đỉnh luống gần Y=0. Bridge `crop-soil` cập nhật environment/preview/catalog và mở lại CropGrowthShowcase; gửi `play` sau khi hoàn tất.

- Giữ domain trong Core độc lập Unity. Code có `UnityEditor` phải nằm trong Editor assembly.
- Không bỏ logic kiểm tra tiền/kho/save để làm một thao tác hiển thị thành công. Giữ tương thích dữ liệu save và giá trị enum đã lưu; giá trị mới nối vào cuối khi cần.
- Dữ liệu người chơi ghi qua `Application.persistentDataPath`; không dùng thư mục project hoặc app bundle làm nơi ghi save trên điện thoại.
- Prefab đã bake là nguồn runtime chính. Không dựng lại toàn bộ mesh mỗi frame hoặc mỗi lần tải scene khi đã có prefab.
- Model sinh bằng code: sửa builder tương ứng rồi bake/render. Rebuild ghi đè chỉnh sửa thủ công trên prefab sinh ra.
- Giữ GUID và file `.meta`; ưu tiên cập nhật asset hiện có. Không xóa asset/GUID rồi tạo lại chỉ để đổi hình dáng.
- Đảm bảo gốc model, tỷ lệ, collider, rig và material phù hợp; các giai đoạn/cấp phải có hình dáng khác nhau và được render cùng tỷ lệ để so sánh.
- Giữ nguyên asset Quaternius CC0, giấy phép và manifest nguồn; chúng là bộ dự phòng. Không sửa checksum để che thay đổi asset nguồn.
- Không đưa đường dẫn riêng của máy, thư mục `.tools`, công cụ bake hoặc thông tin kỹ thuật kiểm tra vào luồng chơi phát hành.
- Sửa template khi thay giao diện gallery; đồng bộ catalog, preview và danh sách sau khi thay danh mục model.
- `Library/`, `Temp/`, `Logs/`, `UserSettings/`, `Builds/` và `.tools/` là dữ liệu/cache/output bị ignore. Không thêm chúng vào git. Không xóa cache hoặc dữ liệu người chơi như một bước mặc định.
- Phân biệt rõ đã kiểm tra cú pháp, đã compile Unity, đã chạy Play và đã chạy trên điện thoại. Không dùng một kết quả để khẳng định tất cả bước còn lại đã đạt.

## Kiểm tra phù hợp với thay đổi

Chạy tại thư mục gốc project:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File Tools/test-core.ps1
python Tools/check_project.py
python Tools/check_model_library.py
git diff --check
```

- Sửa Core/kinh tế/save: chạy core tests và kiểm tra hành vi liên quan.
- Sửa C#/package/shader: đợi Unity compile/import, kiểm tra Console/log. Tree-sitter chỉ kiểm tra cú pháp, không kiểm tra Unity API hoặc shader.
- Sửa model/catalog: bake đúng bộ, kiểm tra mesh/material/collider/rig, xem ảnh render và kiểm tra gallery/biến thể. Khi danh mục đổi, cập nhật kiểm tra thay vì giữ số cũ.
- Sửa thời tiết: thử chuyển chế độ, mưa/tuyết, cây lay, chớp/sấm, mute và chuyển vào/ra thư viện model; xác nhận quay về Nắng không giữ hiệu ứng cũ.
- Sửa mobile input/UI: kiểm tra kéo/chạm/chụm, vùng an toàn và nút không truyền thao tác xuống thế giới.
- Sửa nền tảng/build: cần build và chạy trên nền tảng đó. Kết quả Editor trên Windows không xác nhận iOS/Android.

## Việc tiếp theo đã biết

### Đối chiếu toàn concept theo yêu cầu tiếp tục triển khai

Concept gốc được giữ ở `Design/Concept-v0.5-original.txt`. Chuyển Unity 3D là quyết định mới hơn, thay phần web/2D. Yêu cầu hiện tại là triển khai toàn concept, không chỉ thêm model. Các dòng thiếu dưới đây vẫn là công việc còn lại, không được đánh dấu hoàn tất khi chỉ có dữ liệu/mesh.

| Hệ thống | Trạng thái gameplay |
| --- | --- |
| 30 giống, kho, mở theo cấp | Có chu kỳ gieo/thu và model; 6 cây ăn quả đã là cây lâu năm tái ra quả; chưa mùa vụ/địa hình riêng từng giống |
| Luân canh, xen canh | 5 nhóm/2 vụ, vài cặp buff, ong thụ phấn +1 nhóm Quả; chưa cặp kỵ, UI gợi ý trên ô |
| Độ phì | Có bón phân/đậu/độc canh; đã có kho nông sản ★★/★★★ và giá theo sao |
| Vườn ươm | Có khay, ươm/cấy; đã có minigame cấy mạ (35% khi cấy khéo) |
| Ghép cành | Chưa có gameplay |
| Chăn nuôi | Gà/bò/vịt/heo/ong/cá (heo ủ phân, ong thụ phấn); còn tâm trạng/tên, vuốt ve, thiên địch |
| Chế biến | 10 công thức/9 công trình, queue/offline; chưa đủ chuỗi sản phẩm concept |
| Xây dựng | Mua/nâng 1–3 trên vị trí cố định; chưa bố trí lưới tự do/trang trí |
| Thời tiết/úng | Mưa theo chu kỳ ngắn, rãnh, cây chịu nước, nhà kính; chưa mùa tuần, luống cao, dòng nước về ao, giới hạn nước tưới |
| Dịch hại/thiên tai | Bệnh chung + sâu phá và minigame bắt sâu; chưa đủ sáu dịch hại, tát nước, sương muối/hạn/rét/bão hư công trình |
| Nhật ký/tiến trình | XP/cấp, 5 trang đầu, quà ngày, đơn NPC, 10 achievements, nhiệm vụ tuần xoay vòng; chưa mở vùng/tay nghề |
| Lưới an toàn | Còn tối thiểu 1 nông sản; chưa 3 ngày bảo hộ/bảo hiểm/tái chế cây chết/chế độ nông nhàn |
| Online/co-op | Có backend local SQLite/auth, luật Core trên máy chủ, đảo nhóm qua mã mời, tưới/thoát nước giúp bạn và Unity client; chưa realtime, chat, quà/chợ/đơn làng/công trình chung hoặc triển khai Internet |
| Sự kiện | Lễ hội Đom Đóm định kỳ (+20% giá, khách lẩu đông); chưa châu chấu, lũ chung, Bão Xám, season pass |
| Phát hành | Chưa quảng cáo/IAP, audio hoàn chỉnh, tối ưu và kiểm thử thiết bị/store |

Người dùng đã chọn chuẩn bị máy chủ local trước. `Server/start-local.ps1` chạy backend Windows/Node 24 tại loopback 8787; SQLite riêng trong `Server/.data`, không commit. `Server/README.md` ghi cách chạy và giới hạn. `FarmOnlineHub` giữ token trong bộ nhớ, tạo FarmGame online riêng và khôi phục solo khi thoát. Online gửi lệnh có requestId; server kiểm tra Core, chống nhận trùng. Chưa triển khai dịch vụ trả phí/Internet. Không mô tả backend local là cloud save công khai.

Kiểm tra local ngày 07/10/2026: 44 core tests, 31 server tests, Unity `farm-check`, `development-check` và `online-check` đều complete. Module `com.unity.modules.unitywebrequest` đã resolve/compile. Báo cáo online ở `.tools/model-work/online-result.json`; ảnh `Art/Previews/Farm/Online-farm.png` và `Online-hub.png`. Server tests có restart SQLite và 2 tài khoản; Unity review dùng tài khoản review riêng, xác nhận solo không đổi. Đây chưa phải kiểm thử mobile, mạng WAN hoặc tải nhiều người.

Điều khiển farm: enum `FarmTool.Inspect` nối cuối để giữ giá trị cũ; mở game mặc định xem ô, chọn giống chuyển sang Gieo. FarmCamera chặn vuốt nhanh ngay tại End, theo touchId, bỏ chuột mô phỏng sau touch, khóa cả chu kỳ pinch khởi đầu trên UI và hủy gesture khi mất focus/pause/disable. HUD có Xem ô và nút zoom. `farm-check` còn chạy kiểm tra các nhánh input bằng tọa độ/raycast thật trong Editor và ghi `farm-input-result.json`; chưa kiểm chứng phần cứng cảm ứng.

1. Nối danh mục model vào gameplay theo yêu cầu tiếp theo của người dùng, gồm cây/sản phẩm, chế biến, vật nuôi và nâng cấp.
2. Hoàn thiện điều khiển cảm ứng và giao diện mobile cho các màn demo cần đưa lên thiết bị.
3. Giới hạn khối chụp ảnh/ghi báo cáo của `PlaygroundDirector` cho Editor hoặc chuyển đường dẫn phù hợp khi dùng trong player. Hiện khối này vẫn ghi vào `.tools` dựa trên `Application.dataPath`.
4. Chuẩn bị build iOS theo `IOS-READINESS.md`; chưa có bản build/kiểm thử iPhone.
5. Đo hiệu năng/RAM/thời gian tải trên điện thoại thật, đặc biệt khi có nhiều cây hoặc mưa/bão.
6. Backend local đã có tài khoản/lưu máy chủ/đảo nhóm/giúp bạn; còn triển khai Internet, realtime, chat, giao dịch và co-op đầy đủ. Xem `Server/README.md` trước khi sửa giao thức; chạy cả server tests và bridge `online-check` khi thay đổi.

Các mục này là bối cảnh công việc còn lại, không phải yêu cầu tự động triển khai toàn bộ khi chưa có nhiệm vụ tương ứng.


## Cập nhật concept đảo và vật nuôi

- Nền đảo có sườn dốc liên tục từ đồng bằng lên núi phía sau. `IslandTerrainProfile.Height` chia sẻ cao độ cho builder và bố trí cảnh.
- `EnvironmentModelBuilder.Landscape.cs` tạo nền, thảm thực vật và cây; `SoftGround.shader` làm mềm màu đất/cỏ/đá và thêm hạt bề mặt.
- Bộ Environment có 33 prefab, gồm thêm 8 đất và 7 cây. Toàn thư viện có 353 loại / 600 biến thể / 635 prefab.
- Bridge `landscape` bake/render môi trường và cập nhật playground/catalog. `landscape-check` trong Play kiểm tra sườn núi bằng raycast và chụp ảnh `Art/Previews/Environment/Unity-landscape.png`, để cảnh ở chế độ Nắng.
- `RenderCatalog` hỗ trợ cập nhật riêng môi trường hoặc danh sách `changedIds`, giữ preview các nhóm khác.
- Trâu, bò, heo dùng builder `AnimalModelBuilder.Livestock.cs` với tỷ lệ tròn, chân ngắn, mắt sáng, tai/sừng mềm. Giữ rig và GUID prefab. Bridge `livestock` bake/render động vật và đồng bộ catalog; `livestock-check` trong Play kiểm tra 192 mẫu tư thế của ba con vật.
- Rà soát cả 22 mẫu: `animals-review` bake 13 loài chính cùng 9 sinh vật nhỏ/Đom Đóm Vàng, cập nhật catalog và `Art/Previews/Animals/review.html`. `animals-check` trong Play kiểm tra 832 mẫu rig và 144 mẫu chuyển động sinh vật nhỏ.
- `AnimalModelBuilder.Ambient.cs` tạo 9 sinh vật nhỏ. Bake riêng dùng `Art/Animals/SmallCreatures.asset`, giữ GUID prefab Collection; rebuild đầy đủ cũng gọi cùng builder. `AmbientCreatureMotion` ghi nhớ vị trí sau khi bố trí và giữ góc nghỉ của cánh.
- Scene `CropGrowthShowcase.unity` trưng bày 30 loại × 5 giai đoạn = 150 cây, mỗi loại một hàng, tỷ lệ prefab giữ nguyên. `CropGrowthShowcaseAssets` tạo scene và gallery `Art/Previews/CropGrowth/index.html`; `CropGrowthShowcase` xử lý chọn hàng/camera. Bridge `crop-garden` dựng scene, `play` để chạy, `crop-garden-check` kiểm tra 150 cây cùng 30 lựa chọn camera. Đây là scene trưng bày độc lập, không sửa build settings hoặc save gameplay.
