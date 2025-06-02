import 'package:flutter/material.dart';
import 'package:flutter_spinkit/flutter_spinkit.dart';
import '/app/resource/reponsive_utils.dart';

class LoadingWidget extends StatelessWidget {
  const LoadingWidget({super.key});

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: Colors.amber.withValues(alpha: 0.2),
      body: SafeArea(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Center(
              child: SizedBox(
                height: UtilsReponsive.height(100, context),
                width: UtilsReponsive.width(100, context),
                child: SpinKitFadingCircle(
                  color: Colors.amber, // Màu của nét đứt
                  size: 50.0, // Kích thước của CircularProgressIndicator
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }
}
