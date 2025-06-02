import 'dart:convert';
import 'dart:developer';

import 'package:get/get.dart';
import 'package:http/http.dart' as http;
import 'package:livestock/app/data/live_stock_info.dart';
import 'package:livestock/app/data/specie_model.dart';
import 'package:universal_html/html.dart' as html;
class HomeController extends GetxController {
  //TODO: Implement HomeController

  final count = 0.obs;
  Rx<String> selectedValue = 'Apple'.obs;
  RxList<String> species = <String>[].obs;
  Rx<String> selectedSpecie = 'Chọn giống vật nuôi'.obs;
  RxBool isLoading = false.obs;
  RxBool isCheckTab = false.obs;
  Rx<String> inspectionCode = ''.obs;
  Rx<String> idLivestock = ''.obs;
  Rx<int> specie = 0.obs;
  Rx<LiveStockInfo> liveStockInfo = LiveStockInfo().obs;
  Rx<String> errorMessage = ''.obs;

  @override
  void onInit() {
    if (Uri.base.pathSegments.isNotEmpty ) {
      idLivestock.value = Uri.base.pathSegments.first;
      fetchDetailLiveStock();
    } else {}

    _fetchSpecies();
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

  fetchDetailLiveStock() async {
     errorMessage.value = '';
    try {
    log(
      'Selected Specie: ${SpecieHelper.getSpecieTypeFromName(selectedSpecie.value)}',
    );
    final params = {
      if (idLivestock.value.isNotEmpty) 'livestockId': idLivestock.value,
      if (inspectionCode.value.isNotEmpty)
        'inspectionCode': inspectionCode.value,
      if (idLivestock.value.isEmpty)
        'specieType':
            SpecieHelper.getSpecieTypeFromName(selectedSpecie.value).toString(),
    };
    final uri = Uri.parse(
      'https://api.hoptacxaluavang.site/api/livestock-management/get-livestock-details',
    ).replace(queryParameters: params);
    final response = await http.get(uri);
    final responseData = json.decode(response.body);
    log('Response Data: ${response.body}');
    log('Response Data: ${params}');
    final data = LiveStockInfo.fromJson(responseData['data']);
    if (data.livestockId != null) {
        html.window.history.pushState(null, 'ID ${data.livestockId}', '/${data.livestockId}');
      liveStockInfo.value = data;
    } else {}
    } catch (e) {
      liveStockInfo.value = LiveStockInfo();
      errorMessage.value = 'Không tìm thấy loài vật';
      log('Error: $e');
    } finally {
      isLoading.value = false;
    }
  }

  Future<void> _fetchSpecies() async {
    isLoading.value = true;
    try {
      final response = await http.get(
        Uri.parse(
          'https://api.hoptacxaluavang.site/api/inspection-code-range/get-all-specie',
        ),
        headers: {'Content-Type': 'application/json'},
      );

      final responseData = json.decode(response.body);
      log('Response Data: ${response.body}');
      final data = SpecieResponse.fromJson(responseData);
      if (data.success) {
        log('Data: ${data.data?.specieList}');
        species.value = data.data?.items ?? [];
        selectedSpecie.value = species.first;
      } else {
        log('Error: ${data.message}');
      }
    } catch (e) {
      log('Error: $e');
    } finally {
      isLoading.value = false;
    }
  }

  void increment() => count.value++;
}
