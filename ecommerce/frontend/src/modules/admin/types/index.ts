export type AdminCategory = {
  id: string;
  name: string;
  slug: string;
  description?: string | null;
  isActive: boolean;
  productCount: number;
};

export type AdminProduct = {
  id: string;
  name: string;
  slug: string;
  description?: string | null;
  price: number;
  stock: number;
  isActive: boolean;
  categoryId: string;
  categoryName: string;
};

export type AdminOrder = {
  id: string;
  createdAtUtc: string;
  status: string;
  totalAmount: number;
  userId: string;
  userEmail: string;
  itemCount: number;
};

export type AdminUser = {
  id: string;
  fullName: string;
  email: string;
  role: string;
  isActive: boolean;
  createdAtUtc: string;
  ordersCount: number;
};
