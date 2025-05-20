export default function InventoryReport() {
  return (
    <div className="overflow-x-auto">
      <table className="w-full">
        <thead>
          <tr className="bg-gray-100">
            <th className="border p-3 text-left font-medium text-gray-600">
              Loại
            </th>
            <th className="border p-3 text-center font-medium text-gray-600">
              Tổng
            </th>
            <th className="border p-3 text-center font-medium text-gray-600">
              Ốm
            </th>
            <th className="border p-3 text-center font-medium text-gray-600">
              Đã xuất
            </th>
            <th className="border p-3 text-center font-medium text-gray-600">
              Còn lại
            </th>
          </tr>
        </thead>
        <tbody>
          <tr>
            <td className="border p-3">Bò lai Sind cái</td>
            <td className="border p-3 text-center">100</td>
            <td className="border p-3 text-center">5</td>
            <td className="border p-3 text-center">0</td>
            <td className="border p-3 text-center">95</td>
          </tr>
          <tr>
            <td className="border p-3">Bò lai Sind đực</td>
            <td className="border p-3 text-center">100</td>
            <td className="border p-3 text-center">5</td>
            <td className="border p-3 text-center">0</td>
            <td className="border p-3 text-center">95</td>
          </tr>
          <tr>
            <td className="border p-3">Bò BBB cái</td>
            <td className="border p-3 text-center">100</td>
            <td className="border p-3 text-center">5</td>
            <td className="border p-3 text-center">0</td>
            <td className="border p-3 text-center">95</td>
          </tr>
          <tr>
            <td className="border p-3">Bò BBB đực</td>
            <td className="border p-3 text-center">100</td>
            <td className="border p-3 text-center">5</td>
            <td className="border p-3 text-center">0</td>
            <td className="border p-3 text-center">95</td>
          </tr>
          <tr className="bg-gray-50 font-medium">
            <td className="border p-3">Tổng</td>
            <td className="border p-3 text-center">400</td>
            <td className="border p-3 text-center">20</td>
            <td className="border p-3 text-center">0</td>
            <td className="border p-3 text-center">380</td>
          </tr>
        </tbody>
      </table>
    </div>
  );
}
