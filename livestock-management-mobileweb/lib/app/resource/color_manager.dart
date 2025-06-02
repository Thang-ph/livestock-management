import 'package:flutter/material.dart';

class ColorsManager {
  static Color primary = Color(0xffF0A740);
  static const Color scaffoldBg = Color(0xff060318);
  static const Color bgLight1 = Color(0xff333646);
  static const Color bgLight2 = Color(0xff424657);
  static const Color textFieldBg = Color(0xffC8C9CE);
  static const Color hintDark = Color(0xff010105);
  static const Color yellowSecondary = Color(0xffFFC25C);
  static const Color yellowPrimary = Color(0xffFFAF29);
  static const Color whitePrimary = Color(0xffEAEAEB);
  static const Color whiteSecondary = Color(0xffC8C9CE);
}

extension HexColor on Color {
  static Color fromHex(String hexColorString) {
    hexColorString = hexColorString.replaceAll("#", '');
    if (hexColorString.length == 6) {
      hexColorString = "FF$hexColorString";
    }
    return Color(int.parse(hexColorString, radix: 16));
  }
}
