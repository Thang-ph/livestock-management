import 'dart:developer';

import 'package:flutter/material.dart';

class SmartPagingWidget extends StatelessWidget {
  final int totalPages;
  final int currentPage;
  final Function(int) onPageChanged;

  const SmartPagingWidget({
    super.key,
    required this.totalPages,
    required this.currentPage,
    required this.onPageChanged,
  });

  List<int> _getVisiblePages() {
    List<int> pages = [];

    if (totalPages <= 7) {
      pages = List.generate(totalPages, (index) => index + 1);
    } else {
      pages.add(1);

      if (currentPage > 4) pages.add(-1);

      for (int i = currentPage - 1; i <= currentPage + 1; i++) {
        if (i > 1 && i < totalPages) pages.add(i);
      }

      if (currentPage < totalPages - 3) pages.add(-1);

      pages.add(totalPages);
    }

    return pages;
  }

  @override
  Widget build(BuildContext context) {
    List<int> visiblePages = _getVisiblePages();
    log('Current page: $currentPage');
    return Row(
      mainAxisAlignment: MainAxisAlignment.center,
      children: [
        Expanded(
          child: Row(
            children: [
               Center(
                child: InkWell(
                  onTap:
                      currentPage > 1 ? () => onPageChanged(1) : null,
                  child: Icon(Icons.keyboard_double_arrow_left),
                ),
              ),
              Center(
                child: InkWell(
                  onTap:
                      currentPage > 1 ? () => onPageChanged(currentPage - 1) : null,
                  child: Icon(Icons.chevron_left),
                ),
              ),
            ],
          ),
        ),
        Expanded(
            flex: 2,
            child: Row(
              mainAxisAlignment: MainAxisAlignment.center,
              children: [
                for (int page in visiblePages)
                  page == -1
                      ? Padding(
                          padding: const EdgeInsets.symmetric(horizontal: 5),
                          child: Text(
                            "...",
                          ), // Dấu "..."
                        )
                      : GestureDetector(
                          onTap: () => onPageChanged(page),
                          child: Container(
                           decoration: BoxDecoration(borderRadius: BorderRadius.circular(5),  color:currentPage == page
                                    ? Colors.indigo.shade900:null,),
                            padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 5),
                            child: Text(
                              '$page',
                              style: TextStyle().copyWith(
                                color: currentPage == page
                                    ? Colors.white
                                        : Colors.black
                              ),
                            ),
                          ),
                        ),
              ],
            )),
        Expanded(
          child: Row(
            crossAxisAlignment: CrossAxisAlignment.end,
            mainAxisAlignment: MainAxisAlignment.end,
            children: [
              Center(
                child: InkWell(
                  onTap: currentPage < totalPages
                      ? () => onPageChanged(currentPage + 1)
                      : null,
                  child: Icon(Icons.chevron_right)
                ),
              ),
               Center(
                child: InkWell(
                  onTap:
                     currentPage < totalPages ? () => onPageChanged(totalPages) : null,
                  child: Icon(Icons.keyboard_double_arrow_right),
                ),
              ),
            ],
          ),
        ),
      ],
    );
  }
}
