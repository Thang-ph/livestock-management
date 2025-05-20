'use client';

import { useEffect, useRef } from 'react';

export default function DonutCharts() {
  const importChartRef = useRef<HTMLCanvasElement>(null);
  const exportChartRef = useRef<HTMLCanvasElement>(null);

  useEffect(() => {
    if (importChartRef.current && exportChartRef.current) {
      drawDonutChart(
        importChartRef.current,
        10,
        [{ value: 50, color: '#4338ca', label: 'Bò lai Sind cái' }],
        '10'
      );

      drawDonutChart(
        exportChartRef.current,
        20,
        [{ value: 20, color: '#ef4444' }],
        '20'
      );
    }
  }, []);

  const drawDonutChart = (
    canvas: HTMLCanvasElement,
    total: number,
    data: Array<{ value: number; color: string; label?: string }>,
    centerText: string
  ) => {
    const ctx = canvas.getContext('2d');
    if (!ctx) return;

    const width = canvas.width;
    const height = canvas.height;
    const radius = (Math.min(width, height) / 2) * 0.8;
    const centerX = width / 2;
    const centerY = height / 2;
    const donutWidth = radius * 0.4;

    // Clear canvas
    ctx.clearRect(0, 0, width, height);

    // Draw background circle
    ctx.beginPath();
    ctx.arc(centerX, centerY, radius, 0, Math.PI * 2);
    ctx.fillStyle = '#e5e7eb';
    ctx.fill();

    // Draw data segments
    let startAngle = -Math.PI / 2;

    data.forEach((segment) => {
      const segmentAngle = (segment.value / total) * Math.PI * 2;

      ctx.beginPath();
      ctx.arc(centerX, centerY, radius, startAngle, startAngle + segmentAngle);
      ctx.arc(
        centerX,
        centerY,
        radius - donutWidth,
        startAngle + segmentAngle,
        startAngle,
        true
      );
      ctx.closePath();
      ctx.fillStyle = segment.color;
      ctx.fill();

      startAngle += segmentAngle;
    });

    // Draw inner circle (white)
    ctx.beginPath();
    ctx.arc(centerX, centerY, radius - donutWidth, 0, Math.PI * 2);
    ctx.fillStyle = 'white';
    ctx.fill();

    // Draw center text
    ctx.font = 'bold 20px Arial';
    ctx.fillStyle = '#000';
    ctx.textAlign = 'center';
    ctx.textBaseline = 'middle';
    ctx.fillText(centerText, centerX, centerY);

    // Draw label if provided
    if (data[0].label) {
      ctx.font = '12px Arial';
      ctx.fillStyle = '#6b7280';
      ctx.textAlign = 'center';
      ctx.textBaseline = 'middle';
      ctx.fillText(
        `${data[0].label} ${data[0].value} (${Math.round((data[0].value / total) * 100)}%)`,
        centerX,
        centerY + 40
      );
    }
  };

  return (
    <div className="grid grid-cols-2 gap-4 p-4">
      <div className="flex flex-col items-center">
        <canvas ref={importChartRef} width={200} height={200}></canvas>
        <p className="mt-2 text-center">Số lượng nhập</p>
      </div>
      <div className="flex flex-col items-center">
        <canvas ref={exportChartRef} width={200} height={200}></canvas>
        <p className="mt-2 text-center">Số lượng xuất</p>
      </div>
    </div>
  );
}
