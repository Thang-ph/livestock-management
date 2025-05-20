'use client';

import { Checkbox } from '@/components/ui/checkbox';
import { cn } from '@/lib/utils';

const days = [
  '16/03',
  '17/03',
  '18/03',
  '19/03',
  '20/03',
  '21/03',
  '22/03',
  '23/03'
];

const bidPackages = [
  {
    id: 1,
    name: 'Gói thầu bò 3B',
    checked: true,
    startDay: 0,
    endDay: 1,
    progress: '12/20',
    status: 'danger'
  },
  {
    id: 2,
    name: 'Gói thầu bò lại Sind',
    checked: true,
    startDay: 0,
    endDay: 3,
    progress: '30/60',
    status: 'warning'
  },
  {
    id: 3,
    name: 'Gói thầu bò lại Sind 1',
    checked: true,
    startDay: 1,
    endDay: 5,
    progress: '20/60',
    status: 'success'
  },
  {
    id: 4,
    name: 'Gói thầu 3B đực',
    checked: true,
    startDay: 0,
    endDay: 0,
    progress: '8/8',
    status: 'completed'
  },
  {
    id: 5,
    name: 'Gói thầu 3B cái',
    checked: true,
    startDay: 2,
    endDay: 7,
    progress: '0/40',
    status: 'success'
  },
  {
    id: 6,
    name: 'Gói thầu 3B 2',
    checked: true,
    startDay: 4,
    endDay: 7,
    progress: '0/100',
    status: 'success'
  }
];

export default function BidPackageGantt() {
  return (
    <div className="overflow-x-auto">
      <div className="min-w-[800px]">
        <div className="grid grid-cols-9 border-b">
          <div className="border-r p-2 font-medium">Gói thầu</div>
          {days.map((day, index) => (
            <div key={day} className="p-2 text-center font-medium">
              {day}
            </div>
          ))}
        </div>

        {bidPackages.map((pkg) => (
          <div
            key={pkg.id}
            className="grid grid-cols-9 border-b hover:bg-gray-50"
          >
            <div className="flex items-center gap-2 border-r p-2">
              <Checkbox checked={pkg.checked} />
              <span>{pkg.name}</span>
            </div>

            {Array.from({ length: 8 }).map((_, dayIndex) => {
              const isInRange =
                dayIndex >= pkg.startDay && dayIndex <= pkg.endDay;
              const isFirstDay = dayIndex === pkg.startDay;

              return (
                <div key={dayIndex} className="relative p-2">
                  {isInRange && (
                    <div
                      className={cn(
                        'absolute inset-y-1 left-0 right-0 mx-1 flex items-center justify-center text-xs text-white',
                        {
                          'rounded-l-md bg-red-500':
                            pkg.status === 'danger' && isFirstDay,
                          'bg-red-500': pkg.status === 'danger' && !isFirstDay,
                          'rounded-l-md bg-orange-400':
                            pkg.status === 'warning' && isFirstDay,
                          'bg-orange-400':
                            pkg.status === 'warning' && !isFirstDay,
                          'rounded-l-md bg-green-500':
                            pkg.status === 'success' && isFirstDay,
                          'bg-green-500':
                            pkg.status === 'success' && !isFirstDay,
                          'rounded-l-md bg-blue-500':
                            pkg.status === 'completed' && isFirstDay,
                          'bg-blue-500':
                            pkg.status === 'completed' && !isFirstDay,
                          'rounded-r-md': dayIndex === pkg.endDay
                        }
                      )}
                    >
                      {isFirstDay && pkg.progress}
                    </div>
                  )}
                </div>
              );
            })}
          </div>
        ))}

        <div className="grid grid-cols-4 gap-4 p-3">
          <div className="flex items-center gap-2">
            <div className="h-4 w-4 bg-red-500"></div>
            <span className="text-sm">Còn dưới 7 ngày</span>
          </div>
          <div className="flex items-center gap-2">
            <div className="h-4 w-4 bg-orange-400"></div>
            <span className="text-sm">Còn dưới 15 ngày</span>
          </div>
          <div className="flex items-center gap-2">
            <div className="h-4 w-4 bg-green-500"></div>
            <span className="text-sm">Còn dưới 30 ngày</span>
          </div>
          <div className="flex items-center gap-2">
            <div className="h-4 w-4 bg-blue-500"></div>
            <span className="text-sm">Đã hoàn thành</span>
          </div>
        </div>
      </div>
    </div>
  );
}
