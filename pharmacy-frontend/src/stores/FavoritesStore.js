import { defineStore } from "pinia";
import api from "../utils/axios";
import { useAuthStore } from "./AuthStore";
import ProductService from "../services/ProductService";

export const useFavoritesStore = defineStore("favorites", {
  state: () => ({
    favorites: [],
    ids: [],
    favoritesCount: 0,
  }),

  persist: {
    paths: ["ids", "favoritesCount"],
  },

  actions: {
    async fetchFavorites() {
      const auth = useAuthStore();
      if (auth.isAuthenticated) {
        try {
          const response = await api.get("/favorites");
          this.favorites = response.data;
          this.ids = response.data.map((p) => p.productId);
          this.favoritesCount = this.ids.length;
        } catch {
          this.favorites = [];
          this.ids = [];
          this.favoritesCount = 0;
        }
      } else {
        this.favorites = [];
        for (const id of this.ids) {
          try {
            const p = await ProductService.getById(id);
            this.favorites.push({
              productId: p.id,
              name: p.name,
              description: p.description,
              manufacturerName: p.manufacturer?.name || "",
              manufacturerCountry: p.manufacturer?.country || "",
              price: p.price,
              imageUrl: p.images?.[0]?.url || null,
              isAvailable: p.isAvailable,
              isPrescriptionRequired: p.isPrescriptionRequired,
              quantityInCart: 0,
            });
          } catch {}
        }
        this.favoritesCount = this.ids.length;
      }
    },

    async fetchCount() {
      const auth = useAuthStore();
      if (auth.isAuthenticated) {
        try {
          const response = await api.get("/favorites/count");
          this.favoritesCount = response.data;
        } catch {
          this.favoritesCount = 0;
        }
      } else {
        this.favoritesCount = this.ids.length;
      }
    },

    async add(productId, productData = null) {
      const auth = useAuthStore();
      if (auth.isAuthenticated) {
        try {
          await api.post(`/favorites/${productId}`);
          if (!this.ids.includes(productId)) {
            this.ids.push(productId);
            this.favoritesCount++;
          }
        } catch {}
        return;
      }

      if (!this.ids.includes(productId)) {
        this.ids.push(productId);
        this.favoritesCount++;
        if (!productData) {
          try {
            productData = await ProductService.getById(productId);
          } catch {}
        }
        if (productData) {
          this.favorites.push({
            productId: productData.id,
            name: productData.name,
            description: productData.description,
            manufacturerName: productData.manufacturer?.name || "",
            manufacturerCountry: productData.manufacturer?.country || "",
            price: productData.price,
            imageUrl: productData.images?.[0]?.url || null,
            isAvailable: productData.isAvailable,
            isPrescriptionRequired: productData.isPrescriptionRequired,
            quantityInCart: 0,
          });
        }
      }
    },

    async remove(productId) {
      const auth = useAuthStore();
      if (auth.isAuthenticated) {
        try {
          await api.delete(`/favorites/${productId}`);
        } catch {}
      }
      this.ids = this.ids.filter((id) => id !== productId);
      this.favorites = this.favorites.filter((p) => p.productId !== productId);
      this.favoritesCount = this.ids.length;
    },

    async toggle(productId, isFavorite) {
      if (isFavorite) {
        await this.remove(productId);
      } else {
        await this.add(productId);
      }
    },

    async clear() {
      const auth = useAuthStore();
      if (auth.isAuthenticated) {
        try {
          await api.delete("/favorites");
        } catch {}
      }
      this.favorites = [];
      this.ids = [];
      this.favoritesCount = 0;
    },

    syncFromProducts(products) {
      this.ids = [];
      this.favoritesCount = 0;
      for (const p of products) {
        if (p.isFavorite) {
          this.ids.push(p.id);
          this.favoritesCount++;
        }
      }
    },

    async syncToServer() {
      const auth = useAuthStore();
      if (!auth.isAuthenticated || this.ids.length === 0) return;

      try {
        await api.post("/favorites/bulk", { productIds: this.ids });
        await this.fetchFavorites();
      } catch {}
    },
  },
});
