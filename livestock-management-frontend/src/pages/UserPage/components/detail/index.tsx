// Detail.jsx
import { Line, Doughnut } from 'react-chartjs-2';
import {
  Chart as ChartJS,
  ArcElement,
  CategoryScale,
  LinearScale,
  LineElement,
  PointElement,
  Legend,
  Tooltip,
  Filler,
  Title
} from 'chart.js';

import './detail.css';

ChartJS.register(
  ArcElement,
  CategoryScale,
  LinearScale,
  LineElement,
  PointElement,
  Legend,
  Tooltip,
  Filler,
  Title
);

export function Detail() {
  const userRoleStats = {
    labels: ['Giám đốc', 'Quản lý', 'Nhân viên'],
    datasets: [
      {
        label: 'Tỷ lệ người dùng',
        data: [5, 20, 75],
        backgroundColor: ['#e74c3c', '#3498db', '#2ecc71'],
        borderColor: '#fff',
        borderWidth: 2
      }
    ]
  };

  const doughnutOptions = {
    responsive: true,
    cutout: '60%',
    plugins: {
      legend: {
        position: 'bottom' as const,
        labels: {
          font: { size: 14 }
        }
      },
      title: {
        display: true,
        text: 'Tỷ lệ người dùng theo vai trò',
        font: { size: 18 },
        padding: { top: 10, bottom: 10 }
      },
      tooltip: {
        callbacks: {
          label: (context) => {
            const total = context.dataset.data.reduce((a, b) => a + b, 0);
            const value = context.parsed;
            const percent = ((value / total) * 100).toFixed(1);
            return `${context.label}: ${value} (${percent}%)`;
          }
        }
      }
    }
  };

  return (
    <div className="chart-container">
      <div style={{ marginBottom: '60px', width: '50%' }}>
        <Line
          data={{
            labels: [
              'Tháng 1',
              'Tháng 2',
              'Tháng 3',
              'Tháng 4',
              'Tháng 5',
              'Tháng 6',
              'Tháng 7',
              'Tháng 8',
              'Tháng 9',
              'Tháng 10',
              'Tháng 11',
              'Tháng 12'
            ],
            datasets: [
              {
                label: 'Giám đốc',
                data: [1, 1, 2, 2, 3, 3, 4, 4, 4, 4, 5, 5],
                borderColor: '#e74c3c',
                backgroundColor: 'rgba(231, 76, 60, 0.2)',
                tension: 0.4,
                fill: true,
                pointBackgroundColor: '#e74c3c'
              },
              {
                label: 'Quản lý',
                data: [2, 2, 3, 4, 4, 5, 5, 6, 6, 6, 7, 8],
                borderColor: '#3498db',
                backgroundColor: 'rgba(52, 152, 219, 0.2)',
                tension: 0.4,
                fill: true,
                pointBackgroundColor: '#3498db'
              },
              {
                label: 'Nhân viên',
                data: [5, 6, 7, 8, 9, 10, 10, 11, 12, 13, 14, 15],
                borderColor: '#2ecc71',
                backgroundColor: 'rgba(46, 204, 113, 0.2)',
                tension: 0.4,
                fill: true,
                pointBackgroundColor: '#2ecc71'
              }
            ]
          }}
          options={{
            responsive: true,
            plugins: {
              legend: { position: 'top' },
              title: {
                display: true,
                text: 'Thống kê người dùng theo vai trò trong 12 tháng',
                font: { size: 18 }
              }
            },
            scales: {
              y: {
                beginAtZero: true,
                title: {
                  display: true,
                  text: 'Số lượng người dùng'
                }
              }
            }
          }}
        />
      </div>

      {/* Biểu đồ tròn */}
      <div style={{ maxWidth: '80%', width: '30%', margin: '0 auto' }}>
        <Doughnut data={userRoleStats} options={doughnutOptions} />
      </div>
    </div>
  );
}
