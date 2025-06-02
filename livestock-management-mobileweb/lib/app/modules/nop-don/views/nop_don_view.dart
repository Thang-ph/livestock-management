import 'dart:developer';
import 'dart:typed_data';

import 'package:flutter/material.dart';

import 'package:get/get.dart';
import 'package:livestock/app/resource/form_field_widget.dart';
import 'package:livestock/app/resource/reponsive_utils.dart';
import 'package:livestock/app/resource/text_style.dart';
import 'package:livestock/app/routes/app_pages.dart';

import '../controllers/nop_don_controller.dart';

class NopDonView extends GetView<NopDonController> {
  const NopDonView({super.key});
  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: Colors.white,
      body: SafeArea(
        child: Obx(
          () =>
              controller.isLoading.value
                  ? const Center(child: CircularProgressIndicator())
                  : SingleChildScrollView(
                    padding: EdgeInsets.symmetric(
                      horizontal: UtilsReponsive.width(10, context),
                      vertical: UtilsReponsive.height(10, context),
                    ),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.center,
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
                                  border: Border.all(
                                    color: Colors.blue,
                                    width: 2,
                                  ),
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
                        TextConstant.titleH2(
                          context,
                          text: 'YÊU CẦU BẢO HÀNH',
                          color: Colors.black,
                          fontWeight: FontWeight.w600,
                        ),

                        RCSpacing.heightSized(context: context, size: 50),

                        Row(
                          children: [
                            Expanded(
                              child: TextConstant.subTile2(
                                context,
                                text: 'Loại bệnh',
                                color: Colors.black,
                                fontWeight: FontWeight.w600,
                              ),
                            ),
                            Expanded(
                              flex: 2,
                              child: Obx(
                                () => DropdownButton(
                                  value: controller.selectedDisease.value,
                                  items:
                                      controller.diseaseList.value
                                          .map(
                                            (e) => DropdownMenuItem(
                                              value: e,
                                              child: Text(e.name ?? ''),
                                            ),
                                          )
                                          .toList(),
                                  onChanged: (value) {
                                    controller.selectedDisease.value = value!;
                                  },
                                ),
                              ),
                            ),
                          ],
                        ),
                        RCSpacing.heightSized(context: context, size: 10),

                        Row(
                          children: [
                            Expanded(
                              child: TextConstant.subTile2(
                                context,
                                text: 'Lý do khác',
                                color: Colors.black,
                                fontWeight: FontWeight.w600,
                              ),
                            ),
                            Expanded(
                              flex: 2,
                              child: FormFieldWidget(
                                setValueFunc: (v) {},
                                borderColor: Colors.black,
                                maxLine: 5,
                                padding: 10,
                                paddingVerti: 10,
                                radiusBorder: 10,
                              ),
                            ),
                          ],
                        ),
                        RCSpacing.heightSized(context: context, size: 10),
                        RCSpacing.heightSized(context: context, size: 10),

                        Row(
                          children: [
                            Expanded(
                              child: TextConstant.subTile2(
                                context,
                                text: 'Hình ảnh',
                                color: Colors.black,
                                fontWeight: FontWeight.w600,
                              ),
                            ),
                            Expanded(
                              flex: 2,
                              child: GestureDetector(
                                onTap: () {
                                  controller.pickImageFromCameraWeb();
                                },
                                child: Container(
                                  padding: EdgeInsets.symmetric(
                                    horizontal: 10,
                                    vertical: 10,
                                  ),
                                  decoration: BoxDecoration(
                                    borderRadius: BorderRadius.circular(10),
                                    border: Border.all(
                                      color: Colors.black,
                                      width: 1,
                                    ),
                                  ),
                                  child: Obx(
                                    () =>
                                        controller.imageBytes.value.isNotEmpty
                                            ? Row(
                                              mainAxisAlignment:
                                                  MainAxisAlignment.center,
                                              children: [
                                                Image.memory(
                                                  controller.imageBytes.value,
                                                  height: UtilsReponsive.height(
                                                    100,
                                                    context,
                                                  ),
                                                  width: UtilsReponsive.width(
                                                    100,
                                                    context,
                                                  ),
                                                ),
                                                ElevatedButton(
                                                  onPressed: () {
                                                    controller
                                                        .imageBytes
                                                        .value = Uint8List(0);
                                                    controller.imageName.value =
                                                        '';
                                                    // controller.uploadImageMultipart(controller.imageBytes.value, controller.imageName.value);
                                                  },
                                                  child: TextConstant.subTile2(
                                                    context,
                                                    text: 'Xóa',
                                                    color: Colors.red,
                                                    fontWeight: FontWeight.w600,
                                                  ),
                                                ),
                                              ],
                                            )
                                            : Row(
                                              mainAxisAlignment:
                                                  MainAxisAlignment.center,
                                              children: [
                                                Icon(Icons.photo_camera),
                                                Text('Thêm hình ảnh'),
                                              ],
                                            ),
                                  ),
                                ),
                              ),
                            ),
                          ],
                        ),
                        RCSpacing.heightSized(context: context, size: 40),
                        ElevatedButton(
                                          style: ElevatedButton.styleFrom(
                                            backgroundColor: Colors.blue,
                                            foregroundColor: Colors.white,
                                            shape: RoundedRectangleBorder(
                                              borderRadius:
                                                  BorderRadius.circular(10),
                                            ),
                                          ),
                                      onPressed: () async {
                                           log('Nta');
                         await controller.submitNopDon(context);
                                      },   
                          child: Padding(
                            padding:  EdgeInsets.symmetric(vertical: UtilsReponsive.height(10, context)),
                            child: Row(
                              mainAxisAlignment: MainAxisAlignment.center,
                              children: [
                                Icon(Icons.send, color: Colors.white),
                                SizedBox(
                                  width: UtilsReponsive.width(10, context),
                                ),
                                TextConstant.subTile2(
                                  context,
                                  text: 'Gửi đơn',
                                  fontWeight: FontWeight.w600,
                                ),
                              ],
                            ),
                          ),
                        ),
                      ],
                    ),
                  ),
        ),
      ),
    );
  }
}
