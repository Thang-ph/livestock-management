import 'dart:convert';
import 'dart:developer';

import 'package:flutter/material.dart';
import 'package:get/get.dart';
import 'package:http/http.dart' as http;
import 'package:http_parser/http_parser.dart';
import 'package:livestock/app/model/diseases_model.dart';

import 'package:file_picker/file_picker.dart';
import 'dart:typed_data';

import 'package:livestock/app/model/noti_util.dart';

class NopDonController extends GetxController {
  //TODO: Implement NopDonController

  final count = 0.obs;
  Rx<String> selectedValue = 'Apple'.obs;
  RxList<DiseasesModel> diseaseList = <DiseasesModel>[].obs;
  RxBool isLoading = false.obs;
  Rx<DiseasesModel> selectedDisease = DiseasesModel().obs;
  Rx<Uint8List> imageBytes = Uint8List(0).obs;
  Rx<String> imageName = ''.obs;
  String liveStockId = '';
  @override
  void onInit() {
    if (Uri.base.pathSegments.isNotEmpty) {
      liveStockId = Uri.base.pathSegments[1];
      log('liveStockId: $liveStockId');
    } else {}

    fetchDiseaseList();
    super.onInit();
  }

  @override
  void onReady() {
    super.onReady();
  }

  @override
  void onClose() {
    super.onClose();
  }

  fetchDiseaseList() async {
    try {
      isLoading.value = true;
      final response = await http.get(
        Uri.parse(
          'https://api.hoptacxaluavang.site/api/disease-management/get-list-diseases',
        ),
      );
      final data = json.decode(response.body);
      log('Response Data: ${response.body}');
      if (response.statusCode == 200) {
        // log('Response Data: ${response.body}');

        final List<dynamic> dataList = data['data']['items'];
        diseaseList.value =
            dataList.map((e) => DiseasesModel.fromJson(e)).toList();

        selectedDisease.value = diseaseList.first;
      } else {
        log('Error: ${response.body}');
      }
    } catch (e) {
      log('Error: $e');
    } finally {
      isLoading.value = false;
    }
  }

  Future<void> pickImageFromCameraWeb() async {
    final result = await FilePicker.platform.pickFiles(
      type: FileType.image,
      withData: true,
      allowMultiple: false,
    );

    if (result != null && result.files.single.bytes != null) {
      imageBytes.value = result.files.single.bytes!;
      imageName.value = result.files.single.name;

      // Xử lý ảnh ở đây (upload, hiển thị, ...)
      print(
        "Đã chọn ảnh: ${imageName.value}, size: ${imageBytes.value.length} bytes",
      );
    }
  }

  Future<String> uploadImageMultipart(
    Uint8List imageBytes,
    String fileName,
  ) async {
    try {
      log('Upload Image: $fileName');
      final uri = Uri.parse(
        'https://api.hoptacxaluavang.site/api/cloudinary/upload-file',
      );

      final request = http.MultipartRequest('POST', uri);

      request.files.add(
        http.MultipartFile.fromBytes(
          'file', // trùng tên trường `file` ở BE
          imageBytes,
          filename: fileName,
          contentType: MediaType(
            'image',
            fileName.split('.').last.toLowerCase(),
          ), // hoặc 'png', 'jpg'
        ),
      );

      final streamedResponse = await request.send();

      final response = await http.Response.fromStream(streamedResponse);

      return jsonDecode(response.body)['url'];
    } catch (e) {
      log('Error33: $e');
      return '';
    }
  }

  submitNopDon(BuildContext context) async {
    try {
      isLoading.value = true;
      String imageUrl = '';
      if (imageBytes.value.isNotEmpty) {
        String url = await uploadImageMultipart(
          imageBytes.value,
          imageName.value,
        );
        imageUrl = url;
        if (imageUrl.isNotEmpty) {
          final response = await http.post(
            Uri.parse(
              'https://api.hoptacxaluavang.site/api/insurence-request/create-insurence-request-id',
            ),
            headers: {'Content-Type': 'application/json'},
            body: jsonEncode({
              "id": liveStockId,
              "diseaseId": selectedDisease.value.id,
              "otherReason": "N/A",
              "imageUris": imageUrl,
              "createdBy": "HieuNT",
            }),
          );
          log('Response 2: ${response.body}');
          log(
            'Request 2: ${jsonEncode({"id": liveStockId, "diseaseId": selectedDisease.value.id, "otherReason": "N/A", "imageUris": imageUrl, "createdBy": "HieuNT"})}',
          );

          if (response.statusCode == 200) {
            log('Response: ${response.body}');
            Get.back();
            ErrorNotifier.showError(context, 'Tạo yêu cầu thành công', isNotError: true);
          } else {
            ErrorNotifier.showError(context,'${jsonDecode(response.body)['data']??'Có gì đó không đúng'}', isNotError: false);

            log('Error: ${response.body}');
          }
        }
      }
      // final response = await http.post(
      //   headers: {'Content-Type': 'multipart/form-data'},
      //   Uri.parse(
      //     'https://api.hoptacxaluavang.site/api/insurence-request/create-insurence-request-id',
      //   ),
      //   body: jsonEncode({
      //     "id": liveStockId,
      //     "diseaseId": selectedDisease.value.id,
      //     "otherReason": "N/A",
      //     "imageUris": imageUrl,
      //     "createdBy": "HieuNT",
      //   }),
      // );
      // log(
      //   'Response: ${jsonEncode({"id": liveStockId, "diseaseId": selectedDisease.value.id, "otherReason": "N/A", "imageUris": imageUrl, "createdBy": "HieuNT"})}',
      // );
    } catch (e) {
      log('Error: $e');
    } finally {
      isLoading.value = false;
    }
  }
}
