import { Html5QrcodeScanner } from "html5-qrcode";
import { useState, useEffect, useCallback, } from "react";
import { useParams, useNavigate } from "react-router-dom";

import "bootstrap/dist/css/bootstrap.min.css";

function ScanPage() {
  const { oldLivestockId, bartExportId, bartDetailId } = useParams();
  const [scanResult, setScanResult] = useState(null);
  const [manualCode, setManualCode] = useState("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const [showModal, setShowModal] = useState(false);
  const [livestockData, setLivestockData] = useState(null);
  const [selectedLivestockId, setSelectedLivestockId] = useState(null);

  const extractLivestockId = (qrString) => {
    const parts = qrString.split("/");
    return parts[parts.length - 1];
  };

  const handleApiCall = useCallback(async (newLivestockId) => {
    setLoading(true);
    setError(null);

    try {
      const response = await fetch(
        `https://localhost:7085/api/livestock/get-summary-info/${newLivestockId}`
      );
      if (!response.ok) {
        throw new Error("Không thể lấy thông tin vật nuôi");
      }
      const data = await response.json();
      setLivestockData(data);
      setSelectedLivestockId(newLivestockId);
      setShowModal(true);
    } catch (error) {
      setError(error.message);
    }

    setLoading(false);
  }, []);

  useEffect(() => {
    const scanner = new Html5QrcodeScanner("reader", {
      qrbox: { width: 300, height: 300 },
      fps: 10,
    });

    scanner.render(
      (result) => {
        scanner.clear();
        const livestockId = extractLivestockId(result);
        setScanResult(livestockId);
        handleApiCall(livestockId);
      },
      (err) => console.warn(err)
    );

    return () => {
      scanner.clear();
    };
  }, [handleApiCall]);

  const handleManualSubmit = () => {
    if (manualCode.trim()) {
      handleApiCall(manualCode.trim());
    }
  };

  const navigate = useNavigate();

  const handleConfirm = async () => {
    setLoading(true);
    setError(null);

    const requestBody = bartDetailId
      ? {
        batchExportId: bartExportId,
        livestockId: selectedLivestockId,
        requestedBy: "Admin",
        batchExportDetailId: bartDetailId,
      }
      : {
        batchExportId: bartExportId,
        livestockId: selectedLivestockId,
        requestedBy: "Admin",
      };

    const apiUrl = oldLivestockId == 0
      ? "https://localhost:7085/api/procurement/create-export-detail"
      : "https://localhost:7085/api/procurement/update-export-detail";

    try {
      const response = await fetch(apiUrl, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(requestBody),
      });

      if (!response.ok) {
        throw new Error("Gửi dữ liệu thất bại");
      }

      setShowModal(false);

      // Điều hướng sau khi xác nhận thành công
      navigate(`/admin/procurement/detail/export-detail/${bartExportId}`);
    } catch (error) {
      setError(error.message);
    }

    setLoading(false);
  };


  return (
    <div className="container d-flex flex-column align-items-center justify-content-center min-vh-100 p-4">
      <h1 className="mb-4 fw-bold">Quét QR nhận vật nuôi</h1>
      <div className="border p-4 bg-light rounded shadow-lg w-50 text-center">
        {!scanResult ? (
          <div id="reader" className="border rounded p-3 mx-auto mb-3"></div>
        ) : (
          <p className="text-success fw-bold">Mã vật nuôi: {scanResult}</p>
        )}
        <p className="text-muted">Quét mã QR hoặc nhập mã thẻ tai</p>
        <input
          type="text"
          value={manualCode}
          onChange={(e) => setManualCode(e.target.value)}
          className="form-control text-center mb-3"
          placeholder="Nhập mã thẻ tai"
        />
        <div className="d-flex justify-content-between">
          <button className="btn btn-outline-secondary">Hủy</button>
          <button
            className="btn btn-primary"
            onClick={handleManualSubmit}
            disabled={loading}
          >
            {loading ? "Đang xử lý..." : "Tiếp tục"}
          </button>
        </div>
        {error && <p className="text-danger mt-2">{error}</p>}
      </div>

      {showModal && livestockData && (
        <div className="modal fade show d-block" tabIndex="-1">
          <div className="modal-dialog">
            <div className="modal-content">
              <div className="modal-header">
                <h5 className="modal-title">Xác nhận chọn vật nuôi</h5>
                <button type="button" className="btn-close" onClick={() => setShowModal(false)}></button>
              </div>
              <div className="modal-body">
                <p><strong>Mã thẻ tai:</strong> {livestockData.id}</p>
                <p><strong>Mã kiểm dịch:</strong> {livestockData.inspectionCode}</p>
                <p><strong>Loài:</strong> {livestockData.species}</p>
                <p><strong>Cân nặng (kg):</strong> {livestockData.weight}</p>
                <p><strong>Màu lông:</strong> {livestockData.color}</p>
                <p><strong>Xuất xứ:</strong> {livestockData.origin}</p>
                <p><strong>Trạng thái:</strong> {livestockData.status}</p>
              </div>
              <div className="modal-footer">
                <button className="btn btn-outline-dark" onClick={() => setShowModal(false)}>
                  Hủy
                </button>
                <button className="btn btn-primary" onClick={handleConfirm} disabled={loading}>
                  {loading ? "Đang xử lý..." : "Xác nhận"}
                </button>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}

export default ScanPage;