export interface ColumnConfig {
  /** Property path on the row; supports dot-notation e.g. 'contact.email'. */
  prop: string;
  header: string;
  sortable?: boolean;
  align?: 'start' | 'end' | 'center';
  width?: string;
  /** Name of a projected <ng-template [cuCol]="..."> to render this cell. */
  cell?: string;
  /** Built-in formatting when no custom cell template is supplied. */
  pipe?: 'date' | 'datetime' | 'money' | 'number';
}

export interface RowAction {
  key: string;
  label: string;
  icon?: string;
  tone?: 'default' | 'danger';
  /** Hide the action for rows where this returns false. */
  visible?: (row: any) => boolean;
}
