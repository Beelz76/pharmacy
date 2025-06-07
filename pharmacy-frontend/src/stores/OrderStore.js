import { defineStore } from "pinia";

export const useOrderStore = defineStore("order", {
  state: () => ({
    selectedCity: null,
    selectedStreet: null,
    selectedPharmacy: null,
    selectedAddress: null,
    selectedAddressId: null,
    isDelivery: false,
    paymentMethod: null,
    deliveryComment: null,
    orderId: null,
    orderNumber: null,
    orderTotal: null,
  }),
  persist: true,
  actions: {
    setOrderDetails({
      city,
      street,
      pharmacy,
      address,
      addressId,
      isDelivery,
      method,
      deliveryComment,
    }) {
      this.selectedCity = city;
      this.selectedStreet = street;
      this.selectedPharmacy = pharmacy;
      this.selectedAddress = address;
      this.selectedAddressId = addressId;
      this.isDelivery = isDelivery;
      this.paymentMethod = method;
      this.deliveryComment = deliveryComment;
    },
    setCreatedOrder({ id, number, total }) {
      this.orderId = id;
      this.orderNumber = number;
      this.orderTotal = total;
    },
    resetCheckout() {
      this.selectedCity = null;
      this.selectedStreet = null;
      this.selectedPharmacy = null;
      this.selectedAddress = null;
      this.selectedAddressId = null;
      this.isDelivery = false;
      this.paymentMethod = null;
      this.deliveryComment = null;
    },
    resetCreatedOrder() {
      this.orderId = null;
      this.orderNumber = null;
      this.orderTotal = null;
    },
    resetOrder() {
      this.resetCheckout();
      this.resetCreatedOrder();
    },
  },
});

export const useOrderNavigationStore = defineStore("orderNavigation", {
  state: () => ({
    historyPage: 1,
    restorePage: false,
  }),
  actions: {
    savePage(page) {
      this.historyPage = page;
      this.restorePage = true;
    },
    consumeRestoreFlag() {
      const value = this.restorePage;
      this.restorePage = false;
      return value;
    },
  },
});
