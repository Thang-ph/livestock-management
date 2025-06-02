import 'package:flutter/material.dart';

import 'package:get/get.dart';

import '../controllers/insurent_form_controller.dart';

class InsurentFormView extends GetView<InsurentFormController> {
  const InsurentFormView({super.key});
  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('InsurentFormView'),
        centerTitle: true,
      ),
      body: const Center(
        child: Text(
          'InsurentFormView is working',
          style: TextStyle(fontSize: 20),
        ),
      ),
    );
  }
}
