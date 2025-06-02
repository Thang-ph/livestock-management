class SpecieResponse {
  final int statusCode;
  final bool success;
  final SpecieData? data;
  final dynamic errors;
  final String message;

  SpecieResponse({
    required this.statusCode,
    required this.success,
    this.data,
    this.errors,
    required this.message,
  });

  factory SpecieResponse.fromJson(Map<String, dynamic> json) {
    return SpecieResponse(
      statusCode: json['statusCode'],
      success: json['success'],
      data: json['data'] != null ? SpecieData.fromJson(json['data']) : null,
      errors: json['errors'],
      message: json['message'],
    );
  }
}

class SpecieData {
  final int total;
  final List<String> items;
  final List<Specie> specieList;

  SpecieData({
    required this.total,
    required this.items,
    required this.specieList,
  });

  factory SpecieData.fromJson(Map<String, dynamic> json) {
    // Tạo specieList từ dữ liệu thô
    List<Specie> specieList = [];
    if (json['specieDetails'] != null) {
      specieList = List<Specie>.from(
        json['specieDetails'].map((item) => Specie.fromJson(item)),
      );
    }

    return SpecieData(
      total: json['total'],
      items: List<String>.from(json['items']),
      specieList: specieList,
    );
  }
}

class Specie {
  final String id;
  final String name;
  final int type;

  Specie({required this.id, required this.name, required this.type});

  factory Specie.fromJson(Map<String, dynamic> json) {
    return Specie(
      id: json['id'] ?? '',
      name: json['name'] ?? '',
      type: json['type'] ?? 0,
    );
  }
}

class SpecieHelper {
  // Map tên loài => specieType (phải đồng bộ giữa các trang)
  static final Map<String, int> specieTypeMap = {
    'TRÂU': 0,
    'BÒ': 1,
    'LỢN': 2,
    'GÀ': 3,
    'DÊ': 4,
    'CỪU': 5,
    'NGỰA': 6,
    'LA': 7,
    'LỪA': 8,
  };

  // Biến lưu trữ specieType hiện tại cho quy trình xác nhận

  // Phương thức lấy specieType từ tên loài
  static int getSpecieTypeFromName(String specieName) {
    return specieTypeMap[specieName.toUpperCase()] ?? 0;
  }
}
