'use client';

import { useState } from 'react';
import { X, Clock, PinIcon, Eye } from 'lucide-react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import {
  Tooltip,
  TooltipContent,
  TooltipProvider,
  TooltipTrigger
} from '@/components/ui/tooltip';

export const Pin = () => {
  const [sections, setSections] = useState([
    {
      id: 'pinned',
      title: 'Ghim',
      color: 'bg-blue-50 border-blue-200',
      titleColor: 'text-blue-700',
      items: [
        { id: 1, name: 'Lô Nhập 1', date: '07/05/2023', status: 'normal' },
        { id: 2, name: 'Lô Nhập 2', date: '07/05/2023', status: 'normal' },
        { id: 4, name: 'Lô Nhập 4', date: '07/05/2023', status: 'warning' }
      ]
    },
    {
      id: 'overdue',
      title: 'Quá Hạn',
      color: 'bg-red-50 border-red-200',
      titleColor: 'text-red-700',
      items: [
        { id: 5, name: 'Lô Nhập 1', date: '07/05/2023', status: 'danger' },
        { id: 6, name: 'Lô Nhập 3', date: '07/05/2023', status: 'danger' },
        { id: 7, name: 'Lô Nhập 5', date: '07/05/2023', status: 'danger' },
        { id: 8, name: 'Lô Nhập 6', date: '07/05/2023', status: 'danger' }
      ]
    },
    {
      id: 'missing',
      title: 'Đang Bị Thiếu',
      color: 'bg-orange-50 border-orange-200',
      titleColor: 'text-orange-700',
      items: [
        { id: 9, name: 'Lô Nhập 2', date: '07/05/2023', status: 'warning' }
      ]
    },
    {
      id: 'soon-overdue',
      title: 'Sắp Quá Hạn',
      color: 'bg-yellow-50 border-yellow-200',
      titleColor: 'text-yellow-700',
      items: [
        { id: 10, name: 'Lô Nhập 7', date: '07/05/2023', status: 'warning' },
        { id: 11, name: 'Lô Nhập 8', date: '07/05/2023', status: 'warning' },
        { id: 12, name: 'Lô Nhập 9', date: '07/05/2023', status: 'warning' },
        { id: 13, name: 'Lô Nhập 4', date: '07/05/2023', status: 'warning' }
      ]
    },
    {
      id: 'upcoming',
      title: 'Sắp Tới',
      color: 'bg-green-50 border-green-200',
      titleColor: 'text-green-700',
      items: []
    }
  ]);

  const removeItem = (sectionId: string, itemId: number) => {
    setSections(
      sections.map((section) => {
        if (section.id === sectionId) {
          return {
            ...section,
            items: section.items.filter((item) => item.id !== itemId)
          };
        }
        return section;
      })
    );
  };

  const unpinItem = (sectionId: string, itemId: number) => {
    console.log(`Unpinning item ${itemId} from section ${sectionId}`);
    removeItem(sectionId, itemId);
  };

  const viewDetails = (sectionId: string, itemId: number) => {
    console.log(`Viewing details for item ${itemId} in section ${sectionId}`);
  };

  return (
    <TooltipProvider>
      <div className=" mx-auto p-4">
        <div className="grid grid-cols-1 gap-6 md:grid-cols-2 lg:grid-cols-3">
          {sections.map((section) => (
            <Card
              key={section.id}
              className={`shadow-md transition-shadow duration-300 hover:shadow-lg ${section.color}`}
            >
              <CardHeader className="pb-2">
                <CardTitle
                  className={`text-lg font-bold ${section.titleColor} flex items-center`}
                >
                  {section.id === 'pinned' && (
                    <PinIcon className="mr-2 h-4 w-4" />
                  )}
                  {section.id === 'overdue' && (
                    <Clock className="mr-2 h-4 w-4" />
                  )}
                  {section.title}
                </CardTitle>
              </CardHeader>
              <CardContent className="space-y-3">
                {section.items.length > 0 ? (
                  section.items.map((item) => (
                    <div
                      key={item.id}
                      className={`relative rounded-md border p-3 ${
                        item.status === 'danger'
                          ? 'border-red-300 bg-red-100'
                          : item.status === 'warning'
                            ? 'border-yellow-300 bg-yellow-100'
                            : 'border-gray-200 bg-white'
                      } transition-all duration-200 hover:shadow-sm`}
                    >
                      <Tooltip>
                        <TooltipTrigger asChild>
                          <Button
                            variant="ghost"
                            size="icon"
                            className="absolute right-1 top-1 h-6 w-6 rounded-full bg-white hover:bg-gray-200"
                            onClick={() => removeItem(section.id, item.id)}
                          >
                            <X className="h-4 w-4" />
                            <span className="sr-only">Remove</span>
                          </Button>
                        </TooltipTrigger>
                        <TooltipContent>
                          <p>Xóa</p>
                        </TooltipContent>
                      </Tooltip>

                      <div className="mb-3 mt-1">
                        <h3 className="font-medium text-gray-800">
                          {item.name}
                        </h3>
                        <div className="mt-1 flex items-center text-xs text-gray-600">
                          <Clock className="mr-1 h-3 w-3" />
                          {item.date}
                        </div>
                      </div>

                      <div className="flex space-x-2">
                        <Button
                          size="sm"
                          variant="outline"
                          className="flex-1 border-gray-300 text-xs hover:bg-gray-100 hover:text-gray-800"
                          onClick={() => unpinItem(section.id, item.id)}
                        >
                          <PinIcon className="mr-1 h-3 w-3" />
                          Bỏ Ghim
                        </Button>
                        <Button
                          size="sm"
                          className="flex-1 bg-blue-600 text-xs hover:bg-blue-700"
                          onClick={() => viewDetails(section.id, item.id)}
                        >
                          <Eye className="mr-1 h-3 w-3" />
                          Xem Chi Tiết
                        </Button>
                      </div>
                    </div>
                  ))
                ) : (
                  <div className="py-10 text-center italic text-gray-500">
                    {section.id === 'upcoming'
                      ? 'Hiện chưa có lô'
                      : 'Không có mục nào'}
                  </div>
                )}
              </CardContent>
            </Card>
          ))}
        </div>
      </div>
    </TooltipProvider>
  );
};

export default Pin;
