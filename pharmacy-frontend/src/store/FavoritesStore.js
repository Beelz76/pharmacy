import { defineStore } from 'pinia'
import api from '../utils/axios'

export const useFavoritesStore = defineStore('favorites', {
  state: () => ({
    favorites: [],
    ids: [],
    favoritesCount: 0
  }),

  actions: {
    async fetchFavorites() {
      try {
        const response = await api.get('/favorites')
        this.favorites = response.data
        this.ids = response.data.map(p => p.productId)
        this.favoritesCount = this.ids.length
      } catch {
        this.favorites = []
        this.ids = []
        this.favoritesCount = 0
      }
    },

    async fetchCount() {
      try {
        const response = await api.get('/favorites/count')
        this.favoritesCount = response.data
      } catch {
        this.favoritesCount = 0
      }
    },

    async add(productId) {
      try {
        await api.post(`/favorites/${productId}`)
        if (!this.ids.includes(productId)) {
          this.ids.push(productId)
          this.favoritesCount++
        }
      } catch {}
    },

    async remove(productId) {
      try {
        await api.delete(`/favorites/${productId}`)
        this.ids = this.ids.filter(id => id !== productId)
        this.favorites = this.favorites.filter(p => p.productId !== productId)
        this.favoritesCount = this.ids.length
      } catch {}
    },

    async toggle(productId, isFavorite) {
      if (isFavorite) {
        await this.remove(productId)
      } else {
        await this.add(productId)
      }
    },

    async clear() {
      try {
        await api.delete('/favorites')
        this.favorites = []
        this.ids = []
        this.favoritesCount = 0
      } catch {}
    },

    syncFromProducts(products) {
      this.ids = []
      this.favoritesCount = 0
      for (const p of products) {
        if (p.isFavorite) {
          this.ids.push(p.id)
          this.favoritesCount++
        }
      }
    }
  }
})
