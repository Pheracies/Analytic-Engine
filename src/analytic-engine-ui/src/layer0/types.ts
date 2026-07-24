export interface ItemRequest {
  type: string;
  amount: number;
  categories: number[]; // 0 = Weapon, 1 = Accessory, 2 = Wealth
  timestamp?: number; // Unix epoch milliseconds
  actionType?: actionType
};
export enum actionType {
    Add,
    Delete
};
// Maps numerical enums from C# to readable categories in Svelte
export const CATEGORY_MAP: Record<number, string> = {
  0: 'Weapon',
  1: 'Accessory',
  2: 'Wealth',
};

// Help track connection states
export type ConnectionState = 'Connected' | 'Disconnected' | 'Connecting' | 'Reconnecting';
