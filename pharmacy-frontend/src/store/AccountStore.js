import { defineStore } from 'pinia'
import api from '../utils/axios'

export const useAccountStore = defineStore('account', {
  state: () => ({
    account: null,
    loading: false,
    isLoaded: false,
    error: null
  }),

  getters: {
    fullName: (state) => {
      if (!state.account) return ''
      const { lastName, firstName, patronymic } = state.account
      return [lastName, firstName, patronymic].filter(Boolean).join(' ')
    }
  },

  actions: {
    async fetchProfile(force = false) {
      if ((this.isLoaded && !force) || this.loading) return

      this.loading = true
      this.error = null
      try {
        const response = await api.get('/users/profile')
        this.account = response.data
        this.isLoaded = true
      } catch (err) {
        this.error = err
        console.error('Ошибка загрузки профиля:', err)
      } finally {
        this.loading = false
      }
    },

    async updateProfile(payload) {
      try {
        const response = await api.put('/users/profile', payload)
        this.account = { ...this.account, ...response.data }
      } catch (err) {
        this.error = err
        throw err
      }
    },

    clear() {
      this.account = null
      this.isLoaded = false
      this.error = null
    }
  }
})
