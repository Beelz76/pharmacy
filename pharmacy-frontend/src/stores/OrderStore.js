import { defineStore } from 'pinia'

export const useOrderStore = defineStore('order', {
  state: () => ({
    selectedCity: null,
    selectedStreet: null,
    selectedPharmacy: null,
    paymentMethod: null,
    orderId: null,
    orderNumber: null,
    orderTotal: null
  }),
  persist: true,
  actions: {
    setOrderDetails({ city, street, pharmacy, method }) {
      this.selectedCity = city
      this.selectedStreet = street
      this.selectedPharmacy = pharmacy
      this.paymentMethod = method
    },
    setCreatedOrder({ id, number, total }) {
      this.orderId = id
      this.orderNumber = number
      this.orderTotal = total
    },
    resetCheckout() {
      this.selectedCity = null
      this.selectedStreet = null
      this.selectedPharmacy = null
      this.paymentMethod = null
    },
    resetCreatedOrder() {
      this.orderId = null
      this.orderNumber = null
      this.orderTotal = null
    },
    resetOrder() {
      this.resetCheckout()
      this.resetCreatedOrder()
    }
  }
})

export const useOrderNavigationStore = defineStore('orderNavigation', {
  state: () => ({
    historyPage: 1,
    restorePage: false
  }),
  actions: {
    savePage(page) {
      this.historyPage = page
      this.restorePage = true
    },
    consumeRestoreFlag() {
      const value = this.restorePage
      this.restorePage = false
      return value
    }
  }
})