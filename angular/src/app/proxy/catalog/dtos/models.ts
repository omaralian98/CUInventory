import type { FullAuditedEntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';

export interface CategoryDto extends FullAuditedEntityDto<string> {
  name?: string;
  isActive?: boolean;
  orderIndex?: number;
  concurrencyStamp?: string;
}

export interface CreateCategoryDto {
  name: string;
  isActive?: boolean;
  orderIndex?: number;
}

export interface CreateProductDto {
  name: string;
  description?: string | null;
  sku?: string | null;
  isService?: boolean;
  categoryId?: string | null;
  isActive?: boolean;
  orderIndex?: number;
}

export interface GetCategoryListDto extends PagedAndSortedResultRequestDto {
  filter?: string | null;
  includeInactive?: boolean;
}

export interface GetProductListDto extends PagedAndSortedResultRequestDto {
  filter?: string | null;
  includeInactive?: boolean;
  categoryId?: string | null;
}

export interface ProductDto extends FullAuditedEntityDto<string> {
  name?: string;
  description?: string | null;
  sku?: string | null;
  isService?: boolean;
  categoryId?: string | null;
  isActive?: boolean;
  orderIndex?: number;
  concurrencyStamp?: string;
}

export interface UpdateCategoryDto {
  name: string;
  isActive?: boolean;
  orderIndex?: number;
  concurrencyStamp?: string;
}

export interface UpdateProductDto {
  name: string;
  description?: string | null;
  sku?: string | null;
  isService?: boolean;
  categoryId?: string | null;
  isActive?: boolean;
  orderIndex?: number;
  concurrencyStamp?: string;
}
