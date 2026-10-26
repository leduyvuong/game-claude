# Harvest Isle — Chuẩn bị bản iOS

Cập nhật: 07/10/2026.

## Kết luận hiện tại

Project Unity/C# này có thể làm nền cho bản Android và iOS. Hiện đã chạy thử trong Unity Editor trên Windows; **chưa build hoặc kiểm thử trên iPhone/iPad**. Việc chạy được trong Editor chưa xác nhận hiệu năng, cảm ứng, shader hoặc khả năng phát hành trên App Store.

Tài liệu này ghi lại trạng thái và việc cần làm. Các mục chưa đánh dấu vẫn chưa được triển khai hoặc xác nhận.

## Project đang có

| Hạng mục | Trạng thái |
| --- | --- |
| Engine | Unity 6.3 LTS, `6000.3.25f1` |
| Ngôn ngữ và render | C#, Universal Render Pipeline, Input System |
| Scene game chính | `Assets/HarvestIsle/Scenes/HarvestIsle.unity` |
| Scene xem thử | `Assets/HarvestIsle/Scenes/AnimalPlayground.unity` |
| Model prototype | 477 prefab; 237 loại, 447 biến thể trong gallery |
| Cây và công trình | 30 cây có giai đoạn/bệnh/héo; 30 công trình/máy có 3 cấp |
| Thời tiết chạy thử | Nắng, gió, mưa, bão, tuyết; có cây lay, chớp và âm thanh |
| Nền tảng đã kiểm tra | Play Mode trong Editor trên Windows |
| Bản iOS | Chưa có bản build hoặc kiểm thử thiết bị |

Model và hiệu ứng mới chủ yếu phục vụ prototype. Danh mục cây, sản phẩm, cấp công trình và thời tiết chưa được nối đầy đủ vào kinh tế, save và gameplay.

## Các điểm cần chỉnh trong code

### Điều khiển cảm ứng

`Assets/HarvestIsle/Scripts/Runtime/FarmCamera.cs` của scene game chính đã có xử lý chạm, kéo và chụm hai ngón qua Enhanced Touch.

Scene chạy thử vẫn có các phần phụ thuộc chuột/bàn phím:

- `PlaygroundCamera.cs`: kéo và zoom bằng `Mouse.current`.
- `ModelLibraryViewer.cs`: zoom bằng cuộn chuột.
- `PlaygroundDirector.cs`: phím Space để tạm dừng, cùng nút trên màn hình.

Cần bổ sung cảm ứng cho scene chạy thử nếu đưa scene này lên iPhone, đồng thời kiểm tra thao tác không xuyên qua bảng thời tiết, bảng cấp công trình hoặc thư viện model. Giao diện thử hiện dùng `OnGUI`; cần đánh giá kích thước nút, bố cục và vùng an toàn trên điện thoại.

### Ghi file chẩn đoán

`PlaygroundDirector.cs` đang ghi `runtime-status.json` và ảnh `Live.png` vào thư mục `.tools/playground`, suy ra từ `Application.dataPath`. Đây là đường dẫn phục vụ kiểm tra trong Editor, không phù hợp để dùng làm nơi ghi dữ liệu trong app iOS.

Hướng xử lý:

- Giới hạn khối ghi báo cáo/ảnh thử bằng `#if UNITY_EDITOR`.
- Nếu cần ghi dữ liệu trong bản chạy trên điện thoại, dùng `Application.persistentDataPath`.
- Không đưa việc tự chụp ảnh kiểm tra vào luồng chơi chính.

Save nông trại hiện được khởi tạo qua `Application.persistentDataPath`; vẫn cần thử lưu, đóng app và mở lại trên thiết bị thật.

### Cấu hình và scene build

`Assets/HarvestIsle/Scripts/Editor/ProjectSetup.cs` hiện có cấu hình và lệnh build Android/Windows; chưa có quy trình build iOS riêng.

Cần bổ sung cấu hình iOS: bundle identifier, version/build number, IL2CPP, ký ứng dụng và xuất project Xcode. Lệnh cấu hình hiện đặt danh sách build chỉ gồm scene game chính. Scene chạy thử có thời tiết/thư viện model cần được chọn riêng nếu muốn xuất một bản demo chứa các nội dung đó.

Không đưa scene xem mẫu và toàn bộ nội dung kiểm tra vào bản phát hành mặc định nếu không phục vụ người chơi.

## Công cụ cần cho iOS

- Cài **iOS Build Support** cho phiên bản Unity sử dụng.
- Unity xuất project Xcode; hoàn tất build/ký ứng dụng trên **Mac có Xcode**, hoặc dùng dịch vụ như **Unity Build Automation**.
- Cần Apple Developer Program để phân phối qua App Store. Tài khoản Apple ID miễn phí có thể phục vụ một số hình thức thử trực tiếp qua Xcode.
- Theo yêu cầu Apple đang áp dụng từ **28/04/2026**, bản gửi lên App Store Connect phải build bằng **Xcode 26 trở lên với iOS 26 SDK trở lên**. Đây là yêu cầu SDK build, không đồng nghĩa game bắt buộc chỉ chạy trên iOS 26.

Nguồn chính thức: [Thiết lập iOS trong Unity 6.3](https://docs.unity.com/en-us/engine/6000.3/manual/platform-specific/iphone/getting-started/ios-environment-setup), [Yêu cầu SDK của Apple](https://developer.apple.com/news/upcoming-requirements/?id=04282026a), [Gửi ứng dụng lên App Store](https://developer.apple.com/app-store/submitting/).

## Checklist trước khi đưa lên thiết bị

- [ ] Chốt scene và nội dung của bản demo iOS.
- [ ] Cài iOS Build Support và chuẩn bị Mac/Xcode hoặc dịch vụ build.
- [ ] Bổ sung cấu hình/lệnh xuất project iOS.
- [ ] Giới hạn phần ghi ảnh/log thử cho Editor.
- [ ] Hoàn thiện kéo, chạm, chụm zoom và chặn thao tác xuyên UI.
- [ ] Kiểm tra giao diện ngang, vùng tai thỏ/Dynamic Island và nút cảm ứng.
- [ ] Build thành công, ký và chạy trên iPhone thật.
- [ ] Kiểm tra shader/ánh sáng/particle bằng Metal trên thiết bị.
- [ ] Đo FPS, RAM, thời gian tải, nhiệt độ và mức dùng pin khi mưa/bão.
- [ ] Kiểm tra lưu game, tạm dừng, chuyển nền và mở lại app.
- [ ] Kiểm tra âm thanh và việc chuyển giữa scene/thư viện model/thời tiết.
- [ ] Chuẩn bị biểu tượng, thông tin quyền riêng tư và metadata phát hành theo tính năng thực tế của app.
- [ ] Kiểm thử qua TestFlight trước khi gửi App Review.

## Tài liệu và công cụ trong repo

- [README.md](README.md): chạy prototype, danh mục asset và menu Unity.
- [Gallery toàn bộ model](Art/Previews/Library/index.html).
- [Danh sách model CSV](Art/Previews/Library/model-list.csv).
- [Gallery thời tiết](Art/Previews/Weather/index.html).
- `python Tools/check_project.py`: cấu trúc, GUID, checksum asset nguồn và cú pháp C#.
- `python Tools/check_model_library.py`: độ đầy đủ của danh mục model/prefab/ảnh render.

Các kiểm tra trên không thay thế build iOS, kiểm tra shader trên Metal hoặc đo hiệu năng trên iPhone.
