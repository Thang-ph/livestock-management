// To parse this JSON data, do
//
//     final liveStockInfo = liveStockInfoFromJson(jsonString);

import 'dart:convert';

LiveStockInfo liveStockInfoFromJson(String str) => LiveStockInfo.fromJson(json.decode(str));

String liveStockInfoToJson(LiveStockInfo data) => json.encode(data.toJson());

class LiveStockInfo {
    String? livestockId;
    String? inspectionCode;
    String? specieId;
    String? specieType;
    String? specieName;
    String? livestockStatus;
    String? color;
    double? weight;
    String? origin;
    String? barnId;
    String? barnName;
    DateTime? importDate;
    double? importWeight;
    DateTime? exportDate;
    double? exportWeight;
    DateTime? lastUpdatedAt;
    String? lastUpdatedBy;
    List<LivestockVaccinatedDisease>? livestockVaccinatedDiseases;
    List<LivestockCurrentDisease>? livestockCurrentDiseases;

    LiveStockInfo({
        this.livestockId,
        this.inspectionCode,
        this.specieId,
        this.specieType,
        this.specieName,
        this.livestockStatus,
        this.color,
        this.weight,
        this.origin,
        this.barnId,
        this.barnName,
        this.importDate,
        this.importWeight,
        this.exportDate,
        this.exportWeight,
        this.lastUpdatedAt,
        this.lastUpdatedBy,
        this.livestockVaccinatedDiseases,
        this.livestockCurrentDiseases,
    });

    factory LiveStockInfo.fromJson(Map<String, dynamic> json) => LiveStockInfo(
        livestockId: json["livestockId"],
        inspectionCode: json["inspectionCode"],
        specieId: json["specieId"],
        specieType: json["specieType"],
        specieName: json["specieName"],
        livestockStatus: json["livestockStatus"],
        color: json["color"],
        weight: json["weight"]?.toDouble(),
        origin: json["origin"],
        barnId: json["barnId"],
        barnName: json["barnName"],
        importDate: json["importDate"] == null ? null : DateTime.parse(json["importDate"]),
        importWeight: json["importWeight"]?.toDouble(),
        exportDate: json["exportDate"] == null ? null : DateTime.parse(json["exportDate"]),
        exportWeight: json["exportWeight"]?.toDouble(),
        lastUpdatedAt: json["lastUpdatedAt"] == null ? null : DateTime.parse(json["lastUpdatedAt"]),
        lastUpdatedBy: json["lastUpdatedBy"],
        livestockVaccinatedDiseases: json["livestockVaccinatedDiseases"] == null ? [] : List<LivestockVaccinatedDisease>.from(json["livestockVaccinatedDiseases"]!.map((x) => LivestockVaccinatedDisease.fromJson(x))),
        livestockCurrentDiseases: json["livestockCurrentDiseases"] == null ? [] : List<LivestockCurrentDisease>.from(json["livestockCurrentDiseases"]!.map((x) => LivestockCurrentDisease.fromJson(x))),
    );

    Map<String, dynamic> toJson() => {
        "livestockId": livestockId,
        "inspectionCode": inspectionCode,
        "specieId": specieId,
        "specieType": specieType,
        "specieName": specieName,
        "livestockStatus": livestockStatus,
        "color": color,
        "weight": weight,
        "origin": origin,
        "barnId": barnId,
        "barnName": barnName,
        "importDate": importDate?.toIso8601String(),
        "importWeight": importWeight,
        "exportDate": exportDate?.toIso8601String(),
        "exportWeight": exportWeight,
        "lastUpdatedAt": lastUpdatedAt?.toIso8601String(),
        "lastUpdatedBy": lastUpdatedBy,
        "livestockVaccinatedDiseases": livestockVaccinatedDiseases == null ? [] : List<dynamic>.from(livestockVaccinatedDiseases!.map((x) => x.toJson())),
        "livestockCurrentDiseases": livestockCurrentDiseases == null ? [] : List<dynamic>.from(livestockCurrentDiseases!.map((x) => x.toJson())),
    };
}

class LivestockCurrentDisease {
    String? diseaseId;
    String? diseaseName;
    String? status;
    DateTime? startDate;
    DateTime? endDate;

    LivestockCurrentDisease({
        this.diseaseId,
        this.diseaseName,
        this.status,
        this.startDate,
        this.endDate,
    });

    factory LivestockCurrentDisease.fromJson(Map<String, dynamic> json) => LivestockCurrentDisease(
        diseaseId: json["diseaseId"],
        diseaseName: json["diseaseName"],
        status: json["status"],
        startDate: json["startDate"] == null ? null : DateTime.parse(json["startDate"]),
        endDate: json["endDate"] == null ? null : DateTime.parse(json["endDate"]),
    );

    Map<String, dynamic> toJson() => {
        "diseaseId": diseaseId,
        "diseaseName": diseaseName,
        "status": status,
        "startDate": startDate?.toIso8601String(),
        "endDate": endDate?.toIso8601String(),
    };
}

class LivestockVaccinatedDisease {
    String? diseaseId;
    String? diseaseName;
    DateTime? lastVaccinatedAt;

    LivestockVaccinatedDisease({
        this.diseaseId,
        this.diseaseName,
        this.lastVaccinatedAt,
    });

    factory LivestockVaccinatedDisease.fromJson(Map<String, dynamic> json) => LivestockVaccinatedDisease(
        diseaseId: json["diseaseId"],
        diseaseName: json["diseaseName"],
        lastVaccinatedAt: json["lastVaccinatedAt"] == null ? null : DateTime.parse(json["lastVaccinatedAt"]),
    );

    Map<String, dynamic> toJson() => {
        "diseaseId": diseaseId,
        "diseaseName": diseaseName,
        "lastVaccinatedAt": lastVaccinatedAt?.toIso8601String(),
    };
}
