import 'package:get/get.dart';

import '../controllers/nop_don_controller.dart';

class NopDonBinding extends Bindings {
  @override
  void dependencies() {
    Get.lazyPut<NopDonController>(
      () => NopDonController(),
    );
  }
}
