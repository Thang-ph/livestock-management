'use client';

import { useState, useEffect } from 'react';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter
} from '@/components/ui/dialog';
import { Button } from '@/components/ui/button';
import { Checkbox } from '@/components/ui/checkbox';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';

interface Disease {
  id: string;
  name: string;
}

interface SelectedDisease {
  diseaseId: string;
  insuranceDuration: number;
}

interface DiseaseWarrantyDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  diseases: Disease[];
  selectedDiseases: SelectedDisease[];
  onConfirm: (selectedDiseases: SelectedDisease[]) => void;
}

export function DiseaseWarrantyDialog({
  open,
  onOpenChange,
  diseases,
  selectedDiseases,
  onConfirm
}: DiseaseWarrantyDialogProps) {
  const [localSelectedDiseases, setLocalSelectedDiseases] = useState<
    SelectedDisease[]
  >([]);

  useEffect(() => {
    setLocalSelectedDiseases(selectedDiseases);
  }, [selectedDiseases, open]);

  const handleCheckboxChange = (diseaseId: string, checked: boolean) => {
    if (checked) {
      // Add disease if not already in the list
      if (!localSelectedDiseases.some((d) => d.diseaseId === diseaseId)) {
        setLocalSelectedDiseases([
          ...localSelectedDiseases,
          { diseaseId, insuranceDuration: 21 } // Default to 21 days
        ]);
      }
    } else {
      // Remove disease
      setLocalSelectedDiseases(
        localSelectedDiseases.filter((d) => d.diseaseId !== diseaseId)
      );
    }
  };

  const handleDurationChange = (diseaseId: string, duration: number) => {
    setLocalSelectedDiseases(
      localSelectedDiseases.map((d) =>
        d.diseaseId === diseaseId ? { ...d, insuranceDuration: duration } : d
      )
    );
  };

  const handleConfirm = () => {
    onConfirm(localSelectedDiseases);
    onOpenChange(false);
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle className="text-center font-bold">
            Bảo Hành Theo Bệnh
          </DialogTitle>
        </DialogHeader>
        <div className="rounded-md border border-gray-300">
          <div className="grid grid-cols-2 border-b border-gray-300 bg-gray-100">
            <div className="border-r border-gray-300 p-2 font-medium">
              Tên Bệnh
            </div>
            <div className="p-2 text-center font-medium">
              Thời gian bảo hành
              <br />
              (ngày)
            </div>
          </div>
          <div className="max-h-[400px] overflow-y-auto">
            {diseases.map((disease) => {
              const selected = localSelectedDiseases.find(
                (d) => d.diseaseId === disease.id
              );
              const duration = selected?.insuranceDuration || 21;

              return (
                <div
                  key={disease.id}
                  className="grid grid-cols-2 border-b border-gray-300 last:border-b-0"
                >
                  <div className="flex items-center gap-2 border-r border-gray-300 p-2">
                    <Checkbox
                      id={`disease-${disease.id}`}
                      checked={!!selected}
                      onCheckedChange={(checked) =>
                        handleCheckboxChange(disease.id, !!checked)
                      }
                    />
                    <Label
                      htmlFor={`disease-${disease.id}`}
                      className="cursor-pointer"
                    >
                      {disease.name}
                    </Label>
                  </div>
                  <div className="flex justify-center p-2">
                    <div className="flex items-center rounded border border-gray-300">
                      <Input
                        type="number"
                        value={duration}
                        onChange={(e) =>
                          handleDurationChange(
                            disease.id,
                            Number.parseInt(e.target.value) || 0
                          )
                        }
                        className="w-16 border-0 text-center focus-visible:ring-0"
                        disabled={!selected}
                      />
                    </div>
                  </div>
                </div>
              );
            })}
          </div>
        </div>
        <DialogFooter className="flex justify-center">
          <Button
            onClick={handleConfirm}
            className="bg-gray-800 hover:bg-gray-700"
          >
            Xác nhận
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
