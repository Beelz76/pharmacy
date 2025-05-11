import { defineStore } from 'pinia'
import api from '../utils/axios'

export const useCategoryStore = defineStore('categories', {
  state: () => ({
    categories: [],
    loading: false,
    isLoaded: false,
    error: null
  }),

  actions: {
    async fetchCategories() {
      if (this.isLoaded || this.loading) return
      this.loading = true
      this.error = null
      try {
        const response = await api.get('/categories')
        this.categories = response.data
        this.isLoaded = true
      } catch (err) {
        this.error = err
        console.error('Ошибка загрузки категорий:', err)
      } finally {
        this.loading = false
      }
    }
  }
})
