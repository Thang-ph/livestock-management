'use client';

import type React from 'react';

import { useState } from 'react';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Textarea } from '@/components/ui/textarea';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
  DialogFooter
} from '@/components/ui/dialog';
import { Pencil } from 'lucide-react';
import { Label } from '@/components/ui/label';
import { useGetSpecie } from '@/queries/admin.query';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue
} from '@/components/ui/select';

interface BiddingEditSimpleProps {
  biddingData: any;
  onSubmit: (data: any) => void;
}

export function BiddingEditSimple({
  biddingData,
  onSubmit
}: BiddingEditSimpleProps) {
  const [open, setOpen] = useState(false);
  const [formData, setFormData] = useState({
    code: biddingData.code || '',
    name: biddingData.name || '',
    owner: biddingData.owner || '',
    expiredDuration: biddingData.expiredDuration || 0,
    description: biddingData.description || '',
    details: biddingData.details?.map((detail: any) => ({
      id: detail.id || '',
      speciesId: detail.speciesId || '',
      requiredWeightMin: detail.requiredWeightMin || 0,
      requiredWeightMax: detail.requiredWeightMax || 0,
      requiredAgeMin: detail.requiredAgeMin || 0,
      requiredAgeMax: detail.requiredAgeMax || 0,
      requiredInsuranceDuration:
        detail.requiredInsuranceDuration || detail.requiredInsurance || 0,
      requiredQuantity: detail.requiredQuantity || 0,
      description: detail.description || ''
    })) || [
      {
        id: '',
        speciesId: '',
        requiredWeightMin: 0,
        requiredWeightMax: 0,
        requiredAgeMin: 0,
        requiredAgeMax: 0,
        requiredInsuranceDuration: 0,
        requiredQuantity: 0,
        description: ''
      }
    ],
    requestedBy: biddingData.createdBy || ''
  });

  const { data: speciesData, isPending: isLoadingSpecies } = useGetSpecie();

  const handleChange = (
    e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>
  ) => {
    const { name, value } = e.target;
    setFormData((prev) => ({
      ...prev,
      [name]: value
    }));
  };

  const handleDetailChange = (index: number, field: string, value: any) => {
    setFormData((prev) => {
      const updatedDetails = [...prev.details];
      updatedDetails[index] = {
        ...updatedDetails[index],
        [field]: value
      };
      return {
        ...prev,
        details: updatedDetails
      };
    });
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    console.log('Submitting data:', formData);
    onSubmit(formData);
    setOpen(false);
  };

  return (
    <Dialog open={open} onOpenChange={setOpen}>
      <DialogTrigger asChild>
        <Button variant="outline" size="sm" className="gap-1">
          <Pencil className="h-4 w-4" />
          Chỉnh sửa
        </Button>
      </DialogTrigger>
      <DialogContent className="max-h-[90vh] max-w-3xl overflow-y-auto">
        <DialogHeader>
          <DialogTitle>Chỉnh sửa gói thầu</DialogTitle>
        </DialogHeader>
        <form onSubmit={handleSubmit} className="space-y-6">
          <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
            <div className="space-y-2">
              <Label htmlFor="code">Mã gói thầu</Label>
              <Input
                id="code"
                name="code"
                value={formData.code}
                onChange={handleChange}
                required
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="name">Tên gói thầu</Label>
              <Input
                id="name"
                name="name"
                value={formData.name}
                onChange={handleChange}
                required
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="owner">Bên mời thầu</Label>
              <Input
                id="owner"
                name="owner"
                value={formData.owner}
                onChange={handleChange}
                required
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="expiredDuration">
                Thời gian thực hiện (ngày)
              </Label>
              <Input
                id="expiredDuration"
                name="expiredDuration"
                type="number"
                value={formData.expiredDuration}
                onChange={handleChange}
                required
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="requestedBy">Người yêu cầu</Label>
              <Input
                id="requestedBy"
                name="requestedBy"
                value={formData.requestedBy}
                onChange={handleChange}
                required
              />
            </div>
          </div>

          <div className="space-y-2">
            <Label htmlFor="description">Mô tả</Label>
            <Textarea
              id="description"
              name="description"
              rows={3}
              value={formData.description}
              onChange={handleChange}
            />
          </div>

          <div className="rounded-md border p-4">
            <h3 className="mb-4 font-medium">Chi tiết yêu cầu kỹ thuật</h3>
            {formData.details.map((detail, index) => (
              <div key={index} className="mb-6 space-y-4">
                <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
                  <div className="space-y-2">
                    <Label htmlFor={`detail-speciesId-${index}`}>
                      Danh mục hàng hóa
                    </Label>
                    <Select
                      value={detail.speciesId}
                      onValueChange={(value) =>
                        handleDetailChange(index, 'speciesId', value)
                      }
                    >
                      <SelectTrigger id={`detail-speciesId-${index}`}>
                        <SelectValue placeholder="Chọn danh mục hàng hóa" />
                      </SelectTrigger>
                      <SelectContent>
                        {isLoadingSpecies ? (
                          <SelectItem value="loading" disabled>
                            Đang tải...
                          </SelectItem>
                        ) : (
                          speciesData.data?.map((species: any) => (
                            <SelectItem key={species.id} value={species.id}>
                              {species.name}
                            </SelectItem>
                          ))
                        )}
                      </SelectContent>
                    </Select>
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor={`detail-quantity-${index}`}>
                      Số lượng (con)
                    </Label>
                    <Input
                      id={`detail-quantity-${index}`}
                      type="number"
                      value={detail.requiredQuantity}
                      onChange={(e) =>
                        handleDetailChange(
                          index,
                          'requiredQuantity',
                          Number(e.target.value)
                        )
                      }
                      required
                    />
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor={`detail-weightMin-${index}`}>
                      Cân nặng tối thiểu (kg)
                    </Label>
                    <Input
                      id={`detail-weightMin-${index}`}
                      type="number"
                      value={detail.requiredWeightMin}
                      onChange={(e) =>
                        handleDetailChange(
                          index,
                          'requiredWeightMin',
                          Number(e.target.value)
                        )
                      }
                      required
                    />
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor={`detail-weightMax-${index}`}>
                      Cân nặng tối đa (kg)
                    </Label>
                    <Input
                      id={`detail-weightMax-${index}`}
                      type="number"
                      value={detail.requiredWeightMax}
                      onChange={(e) =>
                        handleDetailChange(
                          index,
                          'requiredWeightMax',
                          Number(e.target.value)
                        )
                      }
                      required
                    />
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor={`detail-ageMin-${index}`}>
                      Tuổi tối thiểu (tháng)
                    </Label>
                    <Input
                      id={`detail-ageMin-${index}`}
                      type="number"
                      value={detail.requiredAgeMin}
                      onChange={(e) =>
                        handleDetailChange(
                          index,
                          'requiredAgeMin',
                          Number(e.target.value)
                        )
                      }
                      required
                    />
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor={`detail-ageMax-${index}`}>
                      Tuổi tối đa (tháng)
                    </Label>
                    <Input
                      id={`detail-ageMax-${index}`}
                      type="number"
                      value={detail.requiredAgeMax}
                      onChange={(e) =>
                        handleDetailChange(
                          index,
                          'requiredAgeMax',
                          Number(e.target.value)
                        )
                      }
                      required
                    />
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor={`detail-insurance-${index}`}>
                      Thời gian bảo hành (ngày)
                    </Label>
                    <Input
                      id={`detail-insurance-${index}`}
                      type="number"
                      value={detail.requiredInsuranceDuration}
                      onChange={(e) =>
                        handleDetailChange(
                          index,
                          'requiredInsuranceDuration',
                          Number(e.target.value)
                        )
                      }
                      required
                    />
                  </div>
                </div>
                <div className="space-y-2">
                  <Label htmlFor={`detail-description-${index}`}>
                    Yêu cầu khác
                  </Label>
                  <Textarea
                    id={`detail-description-${index}`}
                    rows={2}
                    value={detail.description}
                    onChange={(e) =>
                      handleDetailChange(index, 'description', e.target.value)
                    }
                  />
                </div>
              </div>
            ))}
          </div>

          <DialogFooter>
            <Button
              type="button"
              variant="outline"
              onClick={() => setOpen(false)}
            >
              Hủy
            </Button>
            <Button type="submit">Lưu thay đổi</Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
