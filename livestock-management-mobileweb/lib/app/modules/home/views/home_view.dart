import 'dart:developer';

import 'package:flutter/material.dart';

import 'package:get/get.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:intl/intl.dart';
import 'package:livestock/app/model/live_stock_info.dart';
import 'package:livestock/app/model/specie_model.dart';
import 'package:livestock/app/resource/form_field_widget.dart';
import 'package:livestock/app/resource/reponsive_utils.dart';
import 'package:livestock/app/resource/text_style.dart';
import 'package:livestock/app/resource/util_common.dart';
import 'package:livestock/app/routes/app_pages.dart';

import '../controllers/home_controller.dart';

class HomeView extends GetView<HomeController> {
  const HomeView({super.key});
  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: Colors.white,
      body: SafeArea(
        child: SingleChildScrollView(
          padding: EdgeInsets.symmetric(
            horizontal: UtilsReponsive.width(10, context),
            vertical: UtilsReponsive.height(10, context),
          ),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Row(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  GestureDetector(
                    onTap: () {
                      Get.offAllNamed(Routes.HOME);
                    },
                    child: Container(
                      clipBehavior: Clip.hardEdge,
                      height: UtilsReponsive.height(70, context),
                      width: UtilsReponsive.width(70, context),
                      decoration: BoxDecoration(
                        border: Border.all(color: Colors.blue, width: 2),
                        shape: BoxShape.circle,
                        image: DecorationImage(
                          image: AssetImage('assets/logo.jpg'),
                          fit: BoxFit.cover,
                        ),
                      ),
                    ),
                  ),
                  SizedBox(width: UtilsReponsive.width(10, context)),
                  TextConstant.titleH2(
                    context,
                    text: 'Hợp tác xã',
                    color: Colors.black,
                    fontWeight: FontWeight.w600,
                  ),
                ],
              ),
              RCSpacing.heightSized(context: context),
              Obx(
                () =>
                    controller.liveStockInfo.value.livestockId == null
                        ? Column(
                          children: [
                            // Center(
                            //   child: TextConstant.titleH2(
                            //     context,
                            //     text: 'Tra cứu thông tin',
                            //     color: Colors.black,
                            //     fontWeight: FontWeight.w600,
                            //   ),
                            // ),
                            SizedBox(
                              height: UtilsReponsive.height(30, context),
                            ),
                            Obx(
                              () => Column(
                                children: [
                                  RCSpacing.heightSized(context: context),
                                  GestureDetector(
                                    onTap: () {
                                      controller.isCheckTab.value = false;
                                    },
                                    child: Container(
                                      decoration: UtilCommon.shadowBox(
                                        context,
                                        colorBg:
                                            !controller.isCheckTab.value
                                                ? null
                                                : Colors.grey,
                                        isActive: !controller.isCheckTab.value,
                                        colorSd: Colors.grey,
                                      ),
                                      alignment: Alignment.center,
                                      padding: EdgeInsets.symmetric(
                                        vertical: UtilsReponsive.height(
                                          15,
                                          context,
                                        ),
                                      ),
                                      child: TextConstant.subTile3(
                                        context,
                                        text: 'Tra cứu thông tin',
                                        color:
                                            !controller.isCheckTab.value
                                                ? Colors.white
                                                : Colors.black,
                                      ),
                                    ),
                                  ),
                                ],
                              ),
                            ),
                            RCSpacing.heightSized(context: context, size: 50),
                            Obx(
                              () =>
                                  controller.isCheckTab.value
                                      ? SizedBox()
                                      : Column(
                                        children: [
                                          Row(
                                            children: [
                                              Expanded(
                                                child: Column(
                                                  crossAxisAlignment:
                                                      CrossAxisAlignment.start,
                                                  children: [
                                                    TextConstant.titleH3(
                                                      context,
                                                      text: 'Mã kiểm dịch',
                                                      color: Colors.black,
                                                      fontWeight:
                                                          FontWeight.w600,
                                                    ),
                                                    SizedBox(
                                                      height:
                                                          UtilsReponsive.height(
                                                            10,
                                                            context,
                                                          ),
                                                    ),
                                                    FormFieldWidget(
                                                      padding: 15,
                                                      setValueFunc: (v) {
                                                        controller
                                                            .inspectionCode
                                                            .value = v;
                                                      },
                                                      radiusBorder: 10,
                                                    ),
                                                  ],
                                                ),
                                              ),
                                              RCSpacing.withSized(
                                                context: context,
                                                size: 20,
                                              ),
                                              Expanded(
                                                child: Column(
                                                  crossAxisAlignment:
                                                      CrossAxisAlignment.start,

                                                  children: [
                                                    TextConstant.titleH3(
                                                      context,
                                                      text: 'Giống vật nuôi',
                                                      color: Colors.black,
                                                      fontWeight:
                                                          FontWeight.w600,
                                                    ),
                                                    SizedBox(
                                                      height:
                                                          UtilsReponsive.height(
                                                            10,
                                                            context,
                                                          ),
                                                    ),

                                                    Obx(
                                                      () =>
                                                          controller
                                                                  .isLoading
                                                                  .value
                                                              ? CircularProgressIndicator()
                                                              : DropdownButton<
                                                                String
                                                              >(
                                                                value:
                                                                    controller
                                                                        .selectedSpecie
                                                                        .value,
                                                                items:
                                                                    controller
                                                                        .species
                                                                        .value
                                                                        .map(
                                                                          (
                                                                            e,
                                                                          ) => DropdownMenuItem<
                                                                            String
                                                                          >(
                                                                            value:
                                                                                e,
                                                                            child: Text(
                                                                              e,
                                                                            ),
                                                                          ),
                                                                        )
                                                                        .toList(),
                                                                onChanged: (v) {
                                                                  controller
                                                                      .selectedSpecie
                                                                      .value = v!;
                                                                },
                                                              ),
                                                    ),
                                                  ],
                                                ),
                                              ),
                                            ],
                                          ),
                                          RCSpacing.heightSized(
                                            context: context,
                                            size: 20,
                                          ),
                                          Obx(
                                            () =>
                                                controller
                                                        .errorMessage
                                                        .value
                                                        .isNotEmpty
                                                    ? TextConstant.subTile3(
                                                      context,
                                                      text:
                                                          controller
                                                              .errorMessage
                                                              .value,
                                                      color: Colors.red,
                                                    )
                                                    : SizedBox(),
                                          ),
                                          RCSpacing.heightSized(
                                            context: context,
                                            size: 20,
                                          ),
                                          ElevatedButton(
                                            style: ElevatedButton.styleFrom(
                                              backgroundColor: Colors.blue,
                                              foregroundColor: Colors.white,
                                              shape: RoundedRectangleBorder(
                                                borderRadius:
                                                    BorderRadius.circular(10),
                                              ),
                                            ),
                                            onPressed: () {
                                              controller.idLivestock.value = '';
                                              controller.fetchDetailLiveStock();
                                            },
                                            child: TextConstant.subTile2(
                                              context,
                                              text: 'Tìm kiếm',
                                            ),
                                          ),
                                        ],
                                      ),
                            ),
                          ],
                        )
                        : Column(
                          mainAxisAlignment: MainAxisAlignment.center,
                          children: [
                            Center(
                              child: TextConstant.titleH1(
                                context,
                                text: 'Thông tin vật nuôi',
                                color: Colors.black,
                                fontWeight: FontWeight.w600,
                              ),
                            ),
                            RCSpacing.heightSized(context: context),
                            GestureDetector(
                              onTap: () {
                                controller.liveStockInfo.value =
                                    LiveStockInfo();
                              },
                              child: Row(
                                children: [
                                  Icon(Icons.qr_code_scanner),
                                  TextConstant.subTile3(
                                    context,
                                    text: 'Kiểm tra thông tin khác',
                                    color: Colors.blue,
                                    fontWeight: FontWeight.w500,
                                  ),
                                ],
                              ),
                            ),
                            SizedBox(
                              height: UtilsReponsive.height(30, context),
                            ),
                            Row(
                              mainAxisAlignment: MainAxisAlignment.center,
                              children: [
                                TextConstant.titleH3(
                                  context,
                                  text: 'Mã kiểm dịch',
                                  color: Colors.black,
                                  fontWeight: FontWeight.bold,
                                ),
                                SizedBox(
                                  width: UtilsReponsive.width(10, context),
                                ),
                                TextConstant.subTile3(
                                  context,
                                  text:
                                      '${controller.liveStockInfo.value.inspectionCode}',
                                  color: Colors.black,
                                  fontWeight: FontWeight.w500,
                                ),
                              ],
                            ),
                            RCSpacing.heightSized(context: context),
                            Row(
                              mainAxisAlignment: MainAxisAlignment.center,
                              children: [
                                TextConstant.titleH3(
                                  context,
                                  text: 'Giống',
                                  color: Colors.black,
                                  fontWeight: FontWeight.bold,
                                ),
                                SizedBox(
                                  width: UtilsReponsive.width(10, context),
                                ),
                                TextConstant.subTile3(
                                  context,
                                  text:
                                      '${controller.liveStockInfo.value.specieName}',
                                  color: Colors.black,
                                  fontWeight: FontWeight.w500,
                                ),
                              ],
                            ),
                            RCSpacing.heightSized(context: context),
                            Container(
                              width: double.infinity,
                              decoration: UtilCommon.shadowBox(
                                context,
                                colorBg: Colors.white,
                                isActive: true,
                                colorSd: Colors.black,
                                radiusBorder: 5,
                              ),
                              padding: EdgeInsets.symmetric(
                                horizontal: UtilsReponsive.width(10, context),
                                vertical: UtilsReponsive.height(10, context),
                              ),
                              child: Column(
                                crossAxisAlignment: CrossAxisAlignment.start,
                                children: [
                                  Row(
                                    mainAxisAlignment:
                                        MainAxisAlignment.spaceBetween,
                                    children: [
                                      TextConstant.titleH3(
                                        context,
                                        text: 'Thông tin định danh',
                                        color: Colors.blue.shade900,
                                        fontWeight: FontWeight.bold,
                                      ),
                                      GestureDetector(
                                        onTap: () {
                                          Get.toNamed(
                                            Routes.NOP_DON +
                                                '/${controller.liveStockInfo.value.livestockId}',
                                          );
                                        },
                                        child: Container(
                                          padding: EdgeInsets.symmetric(
                                            horizontal: UtilsReponsive.width(
                                              10,
                                              context,
                                            ),
                                            vertical: UtilsReponsive.height(
                                              5,
                                              context,
                                            ),
                                          ),
                                          decoration: BoxDecoration(
                                            color: Colors.blue,
                                            borderRadius: BorderRadius.circular(
                                              10,
                                            ),
                                          ),
                                          child: Row(
                                            children: [
                                              Icon(
                                                Icons.outbound_rounded,
                                                color: Colors.white,
                                              ),
                                              SizedBox(
                                                width: UtilsReponsive.width(
                                                  5,
                                                  context,
                                                ),
                                              ),
                                              TextConstant.subTile3(
                                                context,
                                                text: 'Yêu cầu bảo hành',
                                                fontWeight: FontWeight.w500,
                                              ),
                                            ],
                                          ),
                                        ),
                                      ),
                                    ],
                                  ),
                                  SizedBox(
                                    height: UtilsReponsive.height(10, context),
                                  ),
                                  _rowText(
                                    context,
                                    'Màu lông',
                                    controller.liveStockInfo.value.color ??
                                        '--',
                                  ),
                                  _rowText(
                                    context,
                                    'Loài',
                                    controller.liveStockInfo.value.specieName ??
                                        '--',
                                  ),
                                  _rowText(
                                    context,
                                    'Trọng lượng',
                                    (controller.liveStockInfo.value.weight
                                                ?.toString() ??
                                            '--') +
                                        ' kg',
                                  ),
                                  _rowText(
                                    context,
                                    'Nguồn gốc',
                                    controller.liveStockInfo.value.origin ??
                                        '--',
                                  ),
                                  _rowText(
                                    context,
                                    'Nhà chăn nuôi',
                                    controller.liveStockInfo.value.barnName ??
                                        '--',
                                  ),
                                  _rowText(
                                    context,
                                    'Ngày nhập',
                                    controller.liveStockInfo.value.importDate !=
                                            null
                                        ? DateFormat('dd/MM/yyyy').format(
                                          controller
                                              .liveStockInfo
                                              .value
                                              .importDate!,
                                        )
                                        : '--',
                                  ),
                                  _rowText(
                                    context,
                                    'Ngày xuất',
                                    controller.liveStockInfo.value.exportDate !=
                                            null
                                        ? DateFormat('dd/MM/yyyy').format(
                                          controller
                                              .liveStockInfo
                                              .value
                                              .exportDate!,
                                        )
                                        : '',
                                  ),
                                ],
                              ),
                            ),
                            RCSpacing.heightSized(context: context, size: 20),
                            Container(
                              width: double.infinity,
                              decoration: UtilCommon.shadowBox(
                                context,
                                colorBg: Colors.white,
                                isActive: true,
                                colorSd: Colors.black,
                                radiusBorder: 5,
                              ),
                              padding: EdgeInsets.symmetric(
                                horizontal: UtilsReponsive.width(10, context),
                                vertical: UtilsReponsive.height(10, context),
                              ),
                              child: Column(
                                crossAxisAlignment: CrossAxisAlignment.start,
                                children: [
                                  TextConstant.titleH3(
                                    context,
                                    text: 'Các bệnh đã tiêm phòng',
                                    color: Colors.blue.shade900,
                                    fontWeight: FontWeight.bold,
                                  ),
                                  SizedBox(
                                    height: UtilsReponsive.height(10, context),
                                  ),
                                  Row(
                                    mainAxisAlignment:
                                        MainAxisAlignment.spaceAround,
                                    children: [
                                      Expanded(
                                        child: Center(
                                          child: TextConstant.titleH3(
                                            context,
                                            text: 'Bệnh',
                                            color: Colors.black,
                                            fontWeight: FontWeight.bold,
                                          ),
                                        ),
                                      ),
                                      Expanded(
                                        child: Center(
                                          child: TextConstant.titleH3(
                                            context,
                                            text: 'Ngày tiêm gần nhất',
                                            color: Colors.black,
                                            fontWeight: FontWeight.bold,
                                          ),
                                        ),
                                      ),
                                    ],
                                  ),
                                  RCSpacing.heightSized(context: context),
                                  ...controller
                                          .liveStockInfo
                                          .value
                                          .livestockVaccinatedDiseases
                                          ?.map(
                                            (e) => Padding(
                                              padding: EdgeInsets.symmetric(
                                                vertical: UtilsReponsive.height(
                                                  5,
                                                  context,
                                                ),
                                              ),
                                              child: Row(
                                                mainAxisAlignment:
                                                    MainAxisAlignment
                                                        .spaceAround,
                                                children: [
                                                  Expanded(
                                                    child: Center(
                                                      child:
                                                          TextConstant.titleH3(
                                                            context,
                                                            text:
                                                                e.diseaseName ??
                                                                '',
                                                            color: Colors.black,
                                                            fontWeight:
                                                                FontWeight.w500,
                                                          ),
                                                    ),
                                                  ),
                                                  Expanded(
                                                    child: Center(
                                                      child: TextConstant.titleH3(
                                                        context,
                                                        text:
                                                            e.lastVaccinatedAt !=
                                                                    null
                                                                ? DateFormat(
                                                                  'dd/MM/yyyy',
                                                                ).format(
                                                                  e.lastVaccinatedAt!,
                                                                )
                                                                : '--',
                                                        color: Colors.black,
                                                      ),
                                                    ),
                                                  ),
                                                ],
                                              ),
                                            ),
                                          )
                                          .toList() ??
                                      [],
                                ],
                              ),
                            ),
                          ],
                        ),
              ),
            ],
          ),
        ),
      ),
    );
  }

  _rowText(BuildContext context, String title, String value) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 8.0),
      child: Row(
        children: [
          TextConstant.titleH3(
            context,
            text: title + ': ',
            color: Colors.black,
            fontWeight: FontWeight.bold,
          ),
          RCSpacing.withSized(context: context),
          Expanded(
            child: TextConstant.titleH3(
              context,
              text: value,
              color: Colors.black,
              fontWeight: FontWeight.w500,
            ),
          ),
        ],
      ),
    );
  }
}
