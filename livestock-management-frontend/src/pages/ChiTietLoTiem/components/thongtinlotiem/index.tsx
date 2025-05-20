'use client';

import {
  Card,
  CardContent,
  CardFooter,
  CardHeader
} from '@/components/ui/card';
import { Button } from '@/components/ui/button';

export default function VaccinationBatchManagement() {
  return (
    <div className="container mx-auto rounded-md border p-4">
      <div className="mb-6 rounded-md border p-4">
        <h2 className="mb-4 text-lg font-bold">Các Lô Tiêm Quá Hạn</h2>
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-5">
          {[1, 2, 3, 4, 5].map((batch) => (
            <Card key={`expired-${batch}`} className="border">
              <CardHeader className="bg-red-200 p-3 text-xs">
                <p>ngày đủ kiểm:</p>
                <p>
                  {batch === 1
                    ? '20/04/2023'
                    : batch === 2
                      ? '25/04/2023'
                      : batch === 3
                        ? '25/04/2023'
                        : batch === 4
                          ? '01/04/2023'
                          : '04/04/2023'}
                </p>
              </CardHeader>
              <CardContent className="bg-red-200 p-3 text-center font-medium">
                Lô Tiêm {batch}
              </CardContent>
              <CardFooter className="p-2">
                <Button className="w-full text-xs" variant="outline">
                  Thực Hiện Ngay
                </Button>
              </CardFooter>
            </Card>
          ))}
        </div>
      </div>

      <div className="grid grid-cols-1 gap-6 md:grid-cols-2">
        <div className="rounded-md border p-4">
          <h2 className="mb-4 text-lg font-bold">Các Lô Tiêm Sắp Quá Hạn</h2>
          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 md:grid-cols-2 lg:grid-cols-3">
            {[1, 2, 3, 4].map((batch) => (
              <Card key={`soon-expired-${batch}`} className="border">
                <CardHeader className="bg-yellow-100 p-3 text-xs">
                  <p>ngày đủ kiểm:</p>
                  <p>
                    {batch === 1
                      ? '07/05/2023'
                      : batch === 2
                        ? '08/05/2023'
                        : batch === 3
                          ? '10/05/2023'
                          : '11/05/2023'}
                  </p>
                </CardHeader>
                <CardContent className="bg-yellow-100 p-3 text-center font-medium">
                  Lô Tiêm {batch}
                </CardContent>
                <CardFooter className="p-2">
                  <Button className="w-full text-xs" variant="outline">
                    Thực Hiện Ngay
                  </Button>
                </CardFooter>
              </Card>
            ))}
          </div>
        </div>

        <div className="rounded-md border p-4">
          <h2 className="mb-4 text-lg font-bold">Các Lô Tiêm Sắp Tới</h2>
          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 md:grid-cols-2 lg:grid-cols-3">
            {[1, 2, 3].map((batch) => (
              <Card key={`upcoming-${batch}`} className="border">
                <CardHeader className="bg-green-100 p-3 text-xs">
                  <p>ngày đủ kiểm:</p>
                  <p>
                    {batch === 1
                      ? '15/05/2023'
                      : batch === 2
                        ? '16/05/2023'
                        : '17/05/2023'}
                  </p>
                </CardHeader>
                <CardContent className="bg-green-100 p-3 text-center font-medium">
                  Lô Tiêm {batch}
                </CardContent>
                <CardFooter className="p-2">
                  <Button className="w-full text-xs" variant="outline">
                    Thực Hiện Ngay
                  </Button>
                </CardFooter>
              </Card>
            ))}
          </div>
        </div>
      </div>
    </div>
  );
}
