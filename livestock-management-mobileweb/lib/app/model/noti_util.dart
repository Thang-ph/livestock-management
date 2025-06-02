import 'package:flutter/material.dart';

class ErrorNotifier {
  static OverlayEntry? _overlayEntry;
  static late AnimationController _controller;
  static late Animation<Offset> _offsetAnimation;

  static void showError(BuildContext context, String message, {bool isNotError = false}) {
    _removeError(); // Xóa thông báo cũ nếu có

    _controller = AnimationController(
      duration: Duration(milliseconds: 500),
      vsync: Navigator.of(context),
    );

    _offsetAnimation = Tween<Offset>(
      begin: Offset(1.5, 0), // Bắt đầu từ ngoài màn hình bên phải
      end: Offset(0, 0), // Trượt vào màn hình
    ).animate(CurvedAnimation(parent: _controller, curve: Curves.easeOut));

    _overlayEntry = _createOverlayEntry(context, message, isNotError: isNotError);
    Overlay.of(context).insert(_overlayEntry!);
    _controller.forward();

    Future.delayed(Duration(seconds: 3), () {
      _controller.reverse().then((value) => _removeError());
    });
  }

  static OverlayEntry _createOverlayEntry(BuildContext context, String message,  {bool isNotError = false}) {
    return OverlayEntry(
      builder: (context) => Positioned(
        top: 50, // Hiển thị ở góc trên bên phải
        right: 20,
        child: SlideTransition(
          position: _offsetAnimation,
          child: Material(
            color: Colors.transparent,
            child: Container(
              padding: EdgeInsets.all(12),
              decoration: BoxDecoration(
                color:isNotError?Colors.green: Colors.redAccent,
                borderRadius: BorderRadius.circular(8),
                boxShadow: [
                  BoxShadow(color: Colors.black26, blurRadius: 4),
                ],
              ),
              child: Row(
                mainAxisSize: MainAxisSize.min,
                children: [
                  Icon(Icons.error, color: Colors.white),
                  SizedBox(width: 8),
                  Text(message, style: TextStyle(color: Colors.white)),
                ],
              ),
            ),
          ),
        ),
      ),
    );
  }

  static void _removeError() {
    _overlayEntry?.remove();
    _overlayEntry = null;
  }
}
