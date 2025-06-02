import 'package:flutter/material.dart';

class HoverState extends StatefulWidget {
  const HoverState({super.key, required this.builder, required this.onTap});
  final Widget Function(bool isHover) builder;
  final Function() onTap;

  @override
  State<HoverState> createState() => _HoverStateState();
}

class _HoverStateState extends State<HoverState> {
  bool isHover = false;

  @override
  Widget build(BuildContext context) {
    return GestureDetector(
      behavior: HitTestBehavior.translucent,
      onTapDown: (details) {
        setState(() {
          isHover = true;
        });
      },
      onTapUp: (details) {
        setState(() {
          isHover = false;
        });
      },
      onTapCancel: () {
        setState(() {
          isHover = false;
        });
      },
      onTap: widget.onTap,
      child: widget.builder(isHover),
    );
  }
}
