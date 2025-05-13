import { defineStore } from 'pinia'
import api from '../utils/axios'

export const useCartStore = defineStore('cart', {
  state: () => ({
    items: [],
    totalPrice: 0,
    cartCount: 0,
    quantityById: {}
  }),

  getters: {
    isEmpty: (state) => state.items.length === 0
  },

  persist: {
    paths: ['items', 'totalPrice', 'cartCount'] // сохраняем только нужные поля
  },

  actions: {
    async fetchCart() {
      try {
        const response = await api.get('/cart')
        this.items = response.data.items
        this.totalPrice = response.data.totalPrice
        this.cartCount = this.items.reduce((sum, item) => sum + item.quantity, 0)
        this.quantityById = {}
        for (const item of this.items) {
          this.quantityById[item.productId] = item.quantity
        }
      } catch {
        this.items = []
        this.totalPrice = 0
        this.cartCount = 0
        this.quantityById = {}
      }
    },

    async fetchCartCount() {
      try {
        const response = await api.get('/cart/count')
        this.cartCount = response.data
      } catch {
        this.cartCount = 0
      }
    },

    async addToCart(productId) {
      try {
        await api.post(`/cart/${productId}`)
        await this.fetchCart()
      } catch {}
    },

    async setQuantity(productId, quantity) {
      try {
        await api.put('/cart/set-quantity', { productId, quantity })
        await this.fetchCart()
      } catch {}
    },

    async increment(productId) {
      await this.addToCart(productId)
    },

    async decrement(productId) {
      try {
        await api.put(`/cart/${productId}`)
        await this.fetchCart()
      } catch {}
    },

    async removeItem(productId) {
      try {
        await api.delete(`/cart/${productId}`)
        await this.fetchCart()
      } catch {}
    },

    async clearCart() {
      try {
        await api.delete('/cart/clear')
        await this.fetchCart()
      } catch {}
    },

    resetCart() {
      this.items = []
      this.totalPrice = 0
      this.cartCount = 0
      this.quantityById = {}
    },

    syncFromProducts(products) {
      this.quantityById = {}
      this.cartCount = 0
      for (const p of products) {
        if (p.cartQuantity > 0) {
          this.quantityById[p.id] = p.cartQuantity
          this.cartCount += p.cartQuantity
        }
      }
    }
  }
})
