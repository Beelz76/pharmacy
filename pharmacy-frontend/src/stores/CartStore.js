import { defineStore } from "pinia";
import api from "../utils/axios";
import { useAuthStore } from "./AuthStore";
import ProductService from "../services/ProductService";

export const useCartStore = defineStore("cart", {
  state: () => ({
    items: [],
    totalPrice: 0,
    cartCount: 0,
    quantityById: {},
  }),

  getters: {
    isEmpty: (state) => state.items.length === 0,
  },

  persist: {
    paths: ["items", "totalPrice", "cartCount", "quantityById"],
  },

  actions: {
    async fetchCart() {
      try {
        const response = await api.get("/cart");
        this.items = response.data.items;
        this.totalPrice = response.data.totalPrice;
        this.cartCount = this.items.reduce(
          (sum, item) => sum + item.quantity,
          0
        );
        this.quantityById = {};
        for (const item of this.items) {
          this.quantityById[item.productId] = item.quantity;
        }
      } catch {
        this.items = [];
        this.totalPrice = 0;
        this.cartCount = 0;
        this.quantityById = {};
      }
    },

    async fetchCartCount() {
      try {
        const response = await api.get("/cart/count");
        this.cartCount = response.data;
      } catch {
        this.cartCount = 0;
      }
    },

    async addToCart(productId, productData = null) {
      const auth = useAuthStore();
      if (auth.isAuthenticated) {
        try {
          await api.post(`/cart/${productId}`);
          await this.fetchCart();
        } catch {}
        return;
      }

      if (!productData) {
        try {
          productData = await ProductService.getById(productId);
        } catch {}
      }

      const existing = this.items.find((i) => i.productId === productId);
      if (existing) {
        existing.quantity += 1;
        existing.totalPrice = existing.quantity * existing.unitPrice;
      } else if (productData) {
        this.items.push({
          productId: productData.id,
          name: productData.name,
          description: productData.description,
          manufacturerName: productData.manufacturer?.name || "",
          manufacturerCountry: productData.manufacturer?.country || "",
          quantity: 1,
          unitPrice: productData.price,
          totalPrice: productData.price,
          imageUrl: productData.images?.[0] || null,
          isAvailable: productData.isAvailable,
          isPrescriptionRequired: productData.isPrescriptionRequired,
        });
      } else {
        this.items.push({
          productId,
          quantity: 1,
          unitPrice: 0,
          totalPrice: 0,
        });
      }

      this.quantityById[productId] = (this.quantityById[productId] || 0) + 1;
      this.cartCount++;
      this.totalPrice = this.items.reduce((sum, i) => sum + i.totalPrice, 0);
    },

    async setQuantity(productId, quantity, productData = null) {
      const auth = useAuthStore();
      if (auth.isAuthenticated) {
        try {
          await api.put("/cart/set-quantity", { productId, quantity });
          await this.fetchCart();
        } catch {}
        return;
      }

      let item = this.items.find((i) => i.productId === productId);
      if (!item && quantity > 0) {
        if (!productData) {
          try {
            productData = await ProductService.getById(productId);
          } catch {}
        }
        if (productData) {
          item = {
            productId: productData.id,
            name: productData.name,
            description: productData.description,
            manufacturerName: productData.manufacturer?.name || "",
            manufacturerCountry: productData.manufacturer?.country || "",
            quantity: quantity,
            unitPrice: productData.price,
            totalPrice: productData.price * quantity,
            imageUrl: productData.images?.[0] || null,
            isAvailable: productData.isAvailable,
            isPrescriptionRequired: productData.isPrescriptionRequired,
          };
          this.items.push(item);
        }
      } else if (item) {
        item.quantity = quantity;
        item.totalPrice = item.unitPrice * quantity;
        if (quantity <= 0) {
          this.items = this.items.filter((i) => i.productId !== productId);
        }
      }

      if (quantity > 0) {
        this.quantityById[productId] = quantity;
      } else {
        delete this.quantityById[productId];
      }
      this.cartCount = this.items.reduce((sum, i) => sum + i.quantity, 0);
      this.totalPrice = this.items.reduce((sum, i) => sum + i.totalPrice, 0);
    },

    async increment(productId, productData = null) {
      await this.addToCart(productId, productData);
    },

    async decrement(productId) {
      const auth = useAuthStore();
      if (auth.isAuthenticated) {
        try {
          await api.put(`/cart/${productId}`);
          await this.fetchCart();
        } catch {}
        return;
      }

      const item = this.items.find((i) => i.productId === productId);
      if (!item) return;
      item.quantity -= 1;
      item.totalPrice = item.unitPrice * item.quantity;
      if (item.quantity <= 0) {
        this.items = this.items.filter((i) => i.productId !== productId);
        delete this.quantityById[productId];
      } else {
        this.quantityById[productId] = item.quantity;
      }
      this.cartCount = Math.max(this.cartCount - 1, 0);
      this.totalPrice = this.items.reduce((sum, i) => sum + i.totalPrice, 0);
    },

    async removeItem(productId) {
      const auth = useAuthStore();
      if (auth.isAuthenticated) {
        try {
          await api.delete(`/cart/${productId}`);
          await this.fetchCart();
        } catch {}
        return;
      }
      const item = this.items.find((i) => i.productId === productId);
      if (!item) return;
      this.items = this.items.filter((i) => i.productId !== productId);
      delete this.quantityById[productId];
      this.cartCount = this.items.reduce((sum, i) => sum + i.quantity, 0);
      this.totalPrice = this.items.reduce((sum, i) => sum + i.totalPrice, 0);
    },

    async clearCart() {
      const auth = useAuthStore();
      if (auth.isAuthenticated) {
        try {
          await api.delete("/cart/clear");
          await this.fetchCart();
        } catch {}
        return;
      }
      this.resetCart();
    },

    resetCart() {
      this.items = [];
      this.totalPrice = 0;
      this.cartCount = 0;
      this.quantityById = {};
    },

    syncFromProducts(products) {
      this.quantityById = {};
      this.cartCount = 0;
      for (const p of products) {
        if (p.cartQuantity > 0) {
          this.quantityById[p.id] = p.cartQuantity;
          this.cartCount += p.cartQuantity;
        }
      }
    },

    async syncToServer() {
      const auth = useAuthStore();
      if (!auth.isAuthenticated || this.items.length === 0) return;

      const payload = this.items.map((i) => ({
        productId: i.productId,
        quantity: i.quantity,
      }));
      try {
        await api.post("/cart/bulk", { items: payload });
        await this.fetchCart();
      } catch {}
    },
  },
});
