import { defineStore } from 'pinia'

export const useOrderStore = defineStore('order', {
  state: () => ({
    selectedCity: null,
    selectedStreet: null,
    selectedPharmacy: null,
    paymentMethod: null,
  }),
  persist: true,
  actions: {
    setOrderDetails({ city, street, pharmacy, method }) {
      this.selectedCity = city
      this.selectedStreet = street
      this.selectedPharmacy = pharmacy
      this.paymentMethod = method
    },
    resetOrder() {
      this.selectedCity = null
      this.selectedStreet = null
      this.selectedPharmacy = null
      this.paymentMethod = null
    }
  }
})
