import 'package:get/get.dart';

import '../controllers/insurent_form_controller.dart';

class InsurentFormBinding extends Bindings {
  @override
  void dependencies() {
    Get.lazyPut<InsurentFormController>(
      () => InsurentFormController(),
    );
  }
}
