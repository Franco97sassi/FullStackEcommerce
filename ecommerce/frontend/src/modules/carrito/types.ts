export type CartItem = {
  productId: string;
  slug: string;
  name: string;
  price: number;
  quantity: number;
  stock: number;
};

export type CartProductInput = {
  productId: string;
  slug: string;
  name: string;
  price: number;
  stock: number;
};
