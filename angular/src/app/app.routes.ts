import { authGuard, permissionGuard } from '@abp/ng.core';
import { Routes } from '@angular/router';

export const APP_ROUTES: Routes = [
  {
    path: '',
    pathMatch: 'full',
    canActivate: [authGuard],
    loadComponent: () => import('./features/dashboard/dashboard.component').then(c => c.DashboardComponent),
  },
  {
    path: 'catalog/products',
    canActivate: [authGuard, permissionGuard],
    data: { requiredPolicy: 'CUInventory.Products' },
    loadComponent: () => import('./features/catalog/products.component').then(c => c.ProductsComponent),
  },
  {
    path: 'catalog/categories',
    canActivate: [authGuard, permissionGuard],
    data: { requiredPolicy: 'CUInventory.Categories' },
    loadComponent: () => import('./features/catalog/categories.component').then(c => c.CategoriesComponent),
  },
  {
    path: 'procurement/suppliers',
    canActivate: [authGuard, permissionGuard],
    data: { requiredPolicy: 'CUInventory.Suppliers' },
    loadComponent: () => import('./features/procurement/suppliers.component').then(c => c.SuppliersComponent),
  },
  {
    path: 'procurement/purchase-orders',
    canActivate: [authGuard, permissionGuard],
    data: { requiredPolicy: 'CUInventory.PurchaseOrders' },
    loadComponent: () => import('./features/procurement/purchase-orders.component').then(c => c.PurchaseOrdersComponent),
  },
  {
    path: 'warehousing/warehouses',
    canActivate: [authGuard, permissionGuard],
    data: { requiredPolicy: 'CUInventory.Warehouses' },
    loadComponent: () => import('./features/warehousing/warehouses.component').then(c => c.WarehousesComponent),
  },
  {
    path: 'warehousing/shipments',
    canActivate: [authGuard, permissionGuard],
    data: { requiredPolicy: 'CUInventory.Shipments' },
    loadComponent: () => import('./features/warehousing/shipments.component').then(c => c.ShipmentsComponent),
  },
  {
    path: 'inventory/balances',
    canActivate: [authGuard, permissionGuard],
    data: { requiredPolicy: 'CUInventory.InventoryBalances' },
    loadComponent: () => import('./features/inventory/inventory-balances.component').then(c => c.InventoryBalancesComponent),
  },
  {
    path: 'inventory/lots',
    canActivate: [authGuard, permissionGuard],
    data: { requiredPolicy: 'CUInventory.InventoryLots' },
    loadComponent: () => import('./features/inventory/inventory-lots.component').then(c => c.InventoryLotsComponent),
  },
  {
    path: 'inventory/stock-transfers',
    canActivate: [authGuard, permissionGuard],
    data: { requiredPolicy: 'CUInventory.StockTransfers' },
    loadComponent: () => import('./features/inventory/stock-transfers.component').then(c => c.StockTransfersComponent),
  },
  {
    path: 'sales',
    canActivate: [authGuard, permissionGuard],
    data: { requiredPolicy: 'CUInventory.Sales' },
    loadComponent: () => import('./features/sales/sales.component').then(c => c.SalesComponent),
  },
  {
    path: 'reports',
    canActivate: [authGuard, permissionGuard],
    data: { requiredPolicy: 'CUInventory.Reports' },
    children: [
      { path: '', pathMatch: 'full', loadComponent: () => import('./features/reports/reports-hub.component').then(c => c.ReportsHubComponent) },
      { path: 'sales-by-source', loadComponent: () => import('./features/reports/sales-by-source.component').then(c => c.SalesBySourceComponent) },
      { path: 'remaining-stock', loadComponent: () => import('./features/reports/remaining-stock.component').then(c => c.RemainingStockComponent) },
      { path: 'inventory-valuation', loadComponent: () => import('./features/reports/inventory-valuation.component').then(c => c.InventoryValuationComponent) },
      { path: 'low-stock', loadComponent: () => import('./features/reports/low-stock.component').then(c => c.LowStockComponent) },
    ],
  },
  {
    path: 'account',
    loadChildren: () => import('@abp/ng.account').then(c => c.createRoutes()),
  },
  {
    path: 'identity',
    loadChildren: () => import('@abp/ng.identity').then(c => c.createRoutes()),
  },
  {
    path: 'tenant-management',
    loadChildren: () => import('@abp/ng.tenant-management').then(c => c.createRoutes()),
  },
  {
    path: 'setting-management',
    loadChildren: () => import('@abp/ng.setting-management').then(c => c.createRoutes()),
  },
];
