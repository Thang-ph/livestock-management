'use client';

import type React from 'react';

import { useState } from 'react';
import { useGetSpecieType } from '@/queries/admin.query';
import { useCreateMaKiemDich } from '@/queries/makiemdich.query';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Checkbox } from '@/components/ui/checkbox';
import { Label } from '@/components/ui/label';
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
  CardFooter
} from '@/components/ui/card';
import { useToast } from '@/components/ui/use-toast';

export const Add = () => {
  const { data: speciesType, isPending: pendingGet } = useGetSpecieType();
  const { mutateAsync: createMaKiemDich, isPending } = useCreateMaKiemDich();
  const { toast } = useToast();

  const [startCode, setStartCode] = useState('');
  const [endCode, setEndCode] = useState('');
  const [selectedSpecies, setSelectedSpecies] = useState<string[]>([]);

  const handleSpeciesChange = (species: string) => {
    setSelectedSpecies((prev) =>
      prev.includes(species)
        ? prev.filter((item) => item !== species)
        : [...prev, species]
    );
  };

  const validateCode = (code: string): boolean => {
    // Check if code is exactly 6 digits
    return /^\d{6}$/.test(code);
  };

  const handleSubmit = async () => {
    // Validate that both codes are provided
    if (!startCode || !endCode) {
      toast({
        title: 'Lỗi',
        description: 'Vui lòng nhập đầy đủ khoảng mã',
        variant: 'destructive'
      });
      return;
    }

    // Validate that both codes are 6 digits
    if (!validateCode(startCode) || !validateCode(endCode)) {
      toast({
        title: 'Lỗi',
        description: 'Mã kiểm dịch phải là 6 chữ số',
        variant: 'destructive'
      });
      return;
    }

    // Validate that end code is greater than or equal to start code
    if (Number.parseInt(endCode) < Number.parseInt(startCode)) {
      toast({
        title: 'Lỗi',
        description: 'Mã kết thúc phải lớn hơn hoặc bằng mã bắt đầu',
        variant: 'destructive'
      });
      return;
    }

    // Validate that at least one species is selected
    if (selectedSpecies.length === 0) {
      toast({
        title: 'Lỗi',
        description: 'Vui lòng chọn ít nhất một loại vật nuôi',
        variant: 'destructive'
      });
      return;
    }

    try {
      await createMaKiemDich({
        startCode,
        endCode,
        specieTypeList: selectedSpecies
      });

      toast({
        title: 'Thành công',
        description: 'Tạo mới mã kiểm dịch thành công',
        variant: 'success'
      });

      // Reset form
      setStartCode('');
      setEndCode('');
      setSelectedSpecies([]);
    } catch (error) {
      toast({
        title: 'Lỗi',
        description: 'Có lỗi xảy ra khi tạo mã kiểm dịch',
        variant: 'destructive'
      });
    }
  };

  // Create a grid of checkboxes with 3 columns
  const renderSpeciesCheckboxes = () => {
    const rows = [] as any;
    const itemsPerRow = 3;

    for (let i = 0; i < speciesType.length; i += itemsPerRow) {
      const rowItems = speciesType.slice(i, i + itemsPerRow);
      rows.push(
        <div key={i} className="flex gap-8">
          {rowItems.map((species: any) => (
            <div key={species} className="flex items-center space-x-2">
              <Checkbox
                id={species}
                checked={selectedSpecies.includes(species)}
                onCheckedChange={() => handleSpeciesChange(species)}
              />
              <Label htmlFor={species} className="text-sm font-medium">
                {species}
              </Label>
            </div>
          ))}
        </div>
      );
    }

    return rows;
  };

  // Add real-time validation for input fields
  const handleCodeChange = (
    value: string,
    setter: React.Dispatch<React.SetStateAction<string>>
  ) => {
    // Only allow numeric input
    const numericValue = value.replace(/\D/g, '');

    // Limit to 6 digits
    const limitedValue = numericValue.slice(0, 6);

    setter(limitedValue);
  };

  if (isPending || pendingGet) {
    return (
      <Card className="mx-auto w-full max-w-md">
        <CardHeader>
          <CardTitle className="text-center">Đang tải...</CardTitle>
        </CardHeader>
      </Card>
    );
  }

  return (
    <div className="space-y-6">
      <Card className="mx-auto w-full max-w-md border-t-4 border-t-primary shadow-md">
        <CardHeader className="pb-2">
          <CardTitle className="text-center text-xl">
            Tạo mới mã kiểm dịch
          </CardTitle>
          <p className="text-center text-sm text-muted-foreground">
            Nhập thông tin để tạo mã kiểm dịch mới
          </p>
        </CardHeader>
        <CardContent className="space-y-6">
          <div className="space-y-4">
            <div className="flex items-center gap-2">
              <h3 className="font-medium">Chọn loại vật nuôi:</h3>
              <span className="text-xs text-muted-foreground">(Bắt buộc)</span>
            </div>
            <div className="space-y-3 rounded-md border bg-muted/30 p-3">
              {renderSpeciesCheckboxes()}
            </div>
          </div>

          <div className="space-y-4">
            <div className="flex items-center gap-2">
              <h3 className="font-medium">Khoảng mã:</h3>
              <span className="text-xs text-muted-foreground">(Bắt buộc)</span>
            </div>
            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label htmlFor="startCode" className="flex items-center gap-1">
                  Từ:
                  <span className="text-xs text-muted-foreground">
                    (6 chữ số)
                  </span>
                </Label>
                <Input
                  id="startCode"
                  value={startCode}
                  onChange={(e) =>
                    handleCodeChange(e.target.value, setStartCode)
                  }
                  placeholder="100000"
                  maxLength={6}
                  className="transition-all focus:ring-2 focus:ring-primary/20"
                />
                {startCode && !validateCode(startCode) && (
                  <p className="mt-1 text-xs text-red-500">
                    Mã phải là 6 chữ số
                  </p>
                )}
              </div>
              <div className="space-y-2">
                <Label htmlFor="endCode" className="flex items-center gap-1">
                  Đến:
                  <span className="text-xs text-muted-foreground">
                    (6 chữ số)
                  </span>
                </Label>
                <Input
                  id="endCode"
                  value={endCode}
                  onChange={(e) => handleCodeChange(e.target.value, setEndCode)}
                  placeholder="200000"
                  maxLength={6}
                  className="transition-all focus:ring-2 focus:ring-primary/20"
                />
                {endCode && !validateCode(endCode) && (
                  <p className="mt-1 text-xs text-red-500">
                    Mã phải là 6 chữ số
                  </p>
                )}
              </div>
            </div>
          </div>

          {startCode &&
            endCode &&
            validateCode(startCode) &&
            validateCode(endCode) && (
              <div className="rounded-md border bg-muted/30 p-3">
                <h4 className="mb-2 text-sm font-medium">
                  Thông tin mã kiểm dịch:
                </h4>
                <div className="grid grid-cols-2 gap-2 text-sm">
                  <div>
                    Mã bắt đầu: <span className="font-medium">{startCode}</span>
                  </div>
                  <div>
                    Mã kết thúc: <span className="font-medium">{endCode}</span>
                  </div>
                  <div>
                    Số lượng mã:{' '}
                    <span className="font-medium">
                      {Number(endCode) - Number(startCode) + 1}
                    </span>
                  </div>
                  <div>
                    Loại vật nuôi:{' '}
                    <span className="font-medium">
                      {selectedSpecies.length}
                    </span>
                  </div>
                </div>
              </div>
            )}
        </CardContent>
        <CardFooter className="flex flex-col gap-3">
          <Button
            className="w-full"
            onClick={handleSubmit}
            disabled={isPending}
            size="lg"
          >
            {isPending ? 'ĐANG XỬ LÝ...' : 'TẠO MỚI MÃ KIỂM DỊCH'}
          </Button>
          <p className="text-center text-xs text-muted-foreground">
            Mã kiểm dịch sẽ được tạo cho tất cả loại vật nuôi đã chọn
          </p>
        </CardFooter>
      </Card>

      <Card className="mx-auto w-full max-w-md shadow-sm">
        <CardHeader className="pb-2">
          <CardTitle className="text-base">Hướng dẫn sử dụng</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="space-y-3 text-sm">
            <div className="flex gap-2">
              <div className="flex h-5 w-5 items-center justify-center rounded-full bg-primary/10 text-xs text-primary">
                1
              </div>
              <p>Chọn ít nhất một loại vật nuôi cần tạo mã kiểm dịch</p>
            </div>
            <div className="flex gap-2">
              <div className="flex h-5 w-5 items-center justify-center rounded-full bg-primary/10 text-xs text-primary">
                2
              </div>
              <p>Nhập khoảng mã bắt đầu và kết thúc (6 chữ số)</p>
            </div>
            <div className="flex gap-2">
              <div className="flex h-5 w-5 items-center justify-center rounded-full bg-primary/10 text-xs text-primary">
                3
              </div>
              <p>Nhấn nút "TẠO MỚI MÃ KIỂM DỊCH" để hoàn tất</p>
            </div>
          </div>
        </CardContent>
      </Card>
    </div>
  );
};
