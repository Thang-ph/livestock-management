// To parse this JSON data, do
//
//     final diseasesModel = diseasesModelFromJson(jsonString);

import 'dart:convert';

DiseasesModel diseasesModelFromJson(String str) => DiseasesModel.fromJson(json.decode(str));

String diseasesModelToJson(DiseasesModel data) => json.encode(data.toJson());

class DiseasesModel {
    String? id;
    String? name;
    String? symptom;
    String? description;
    int? defaultInsuranceDuration;
    String? type;
    DateTime? createdAt;

    DiseasesModel({
        this.id,
        this.name,
        this.symptom,
        this.description,
        this.defaultInsuranceDuration,
        this.type,
        this.createdAt,
    });

    factory DiseasesModel.fromJson(Map<String, dynamic> json) => DiseasesModel(
        id: json["id"],
        name: json["name"],
        symptom: json["symptom"],
        description: json["description"],
        defaultInsuranceDuration: json["defaultInsuranceDuration"],
        type: json["type"],
        createdAt: json["createdAt"] == null ? null : DateTime.parse(json["createdAt"]),
    );

    Map<String, dynamic> toJson() => {
        "id": id,
        "name": name,
        "symptom": symptom,
        "description": description,
        "defaultInsuranceDuration": defaultInsuranceDuration,
        "type": type,
        "createdAt": createdAt?.toIso8601String(),
    };
}
