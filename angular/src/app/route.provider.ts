import { RoutesService, eLayoutType } from '@abp/ng.core';
import { inject, provideAppInitializer } from '@angular/core';

export const APP_ROUTE_PROVIDER = [
  provideAppInitializer(() => {
    configureRoutes();
  }),
];

function configureRoutes() {
  const routes = inject(RoutesService);
  routes.add([
    {
      path: '/',
      name: '::Menu:Dashboard',
      iconClass: 'fas fa-gauge-high',
      order: 1,
      layout: eLayoutType.application,
    },
    {
      path: '/catalog',
      name: '::Menu:Catalog',
      iconClass: 'fas fa-boxes-stacked',
      order: 2,
      layout: eLayoutType.application,
    },
    { path: '/catalog/products', name: '::Menu:Products', parentName: '::Menu:Catalog', order: 1, layout: eLayoutType.application, requiredPolicy: 'CUInventory.Products' },
    { path: '/catalog/categories', name: '::Menu:Categories', parentName: '::Menu:Catalog', order: 2, layout: eLayoutType.application, requiredPolicy: 'CUInventory.Categories' },
    {
      path: '/procurement',
      name: '::Menu:Procurement',
      iconClass: 'fas fa-truck-field',
      order: 3,
      layout: eLayoutType.application,
    },
    { path: '/procurement/suppliers', name: '::Menu:Suppliers', parentName: '::Menu:Procurement', order: 1, layout: eLayoutType.application, requiredPolicy: 'CUInventory.Suppliers' },
    { path: '/procurement/purchase-orders', name: '::Menu:PurchaseOrders', parentName: '::Menu:Procurement', order: 2, layout: eLayoutType.application, requiredPolicy: 'CUInventory.PurchaseOrders' },
    {
      path: '/warehousing',
      name: '::Menu:Warehousing',
      iconClass: 'fas fa-warehouse',
      order: 4,
      layout: eLayoutType.application,
    },
    { path: '/warehousing/warehouses', name: '::Menu:Warehouses', parentName: '::Menu:Warehousing', order: 1, layout: eLayoutType.application, requiredPolicy: 'CUInventory.Warehouses' },
    { path: '/warehousing/shipments', name: '::Menu:Shipments', parentName: '::Menu:Warehousing', order: 2, layout: eLayoutType.application, requiredPolicy: 'CUInventory.Shipments' },
    {
      path: '/inventory',
      name: '::Menu:Inventory',
      iconClass: 'fas fa-cubes',
      order: 5,
      layout: eLayoutType.application,
    },
    { path: '/inventory/balances', name: '::Menu:InventoryBalances', parentName: '::Menu:Inventory', order: 1, layout: eLayoutType.application, requiredPolicy: 'CUInventory.InventoryBalances' },
    { path: '/inventory/lots', name: '::Menu:InventoryLots', parentName: '::Menu:Inventory', order: 2, layout: eLayoutType.application, requiredPolicy: 'CUInventory.InventoryLots' },
    { path: '/inventory/stock-transfers', name: '::Menu:StockTransfers', parentName: '::Menu:Inventory', order: 3, layout: eLayoutType.application, requiredPolicy: 'CUInventory.StockTransfers' },
    {
      path: '/sales',
      name: '::Menu:Sales',
      iconClass: 'fas fa-cash-register',
      order: 6,
      layout: eLayoutType.application,
      requiredPolicy: 'CUInventory.Sales',
    },
    {
      path: '/reports',
      name: '::Menu:Reports',
      iconClass: 'fas fa-chart-column',
      order: 7,
      layout: eLayoutType.application,
      requiredPolicy: 'CUInventory.Reports',
    },
  ]);
}
