import 'dart:math';

class UtilRandom {
  static List<String> stringListImage = [
    'assets/welcome1.jpg',
    'assets/welcome2.jpg',
    'assets/welcome3.jpg',
    'assets/img2.jpg',
    'assets/img1.jpg',
    'assets/img4.jpg',
  ];
  static String getRandomString() {
    final random = Random();
    return stringListImage[random.nextInt(stringListImage.length)];
  }
}
