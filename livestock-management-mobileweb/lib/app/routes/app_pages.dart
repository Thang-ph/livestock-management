import 'package:get/get.dart';

import '../modules/home/bindings/home_binding.dart';
import '../modules/home/views/home_view.dart';
import '../modules/insurent-form/bindings/insurent_form_binding.dart';
import '../modules/insurent-form/views/insurent_form_view.dart';
import '../modules/nop-don/bindings/nop_don_binding.dart';
import '../modules/nop-don/views/nop_don_view.dart';

part 'app_routes.dart';

class AppPages {
  AppPages._();

  static const INITIAL = Routes.HOME ;

  static final routes = [
    GetPage(
      name: _Paths.HOME ,
      page: () => const HomeView(),
      binding: HomeBinding(),
    ),
    GetPage(
      name: _Paths.INSURENT_FORM,
      page: () => const InsurentFormView(),
      binding: InsurentFormBinding(),
    ),
    GetPage(
      name: _Paths.NOP_DON + '/:id',
      page: () => const NopDonView(),
      binding: NopDonBinding(),
    ),
  ];
}
