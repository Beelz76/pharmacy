import { defineStore } from "pinia";
import { getManufacturers } from "../services/ManufacturerService";

export const useManufacturerStore = defineStore("manufacturers", {
  state: () => ({
    list: [],
    countries: [],
    isLoaded: false,
    loading: false,
    error: null,
  }),
  actions: {
    async fetchManufacturers() {
      if (this.isLoaded || this.loading) return;
      this.loading = true;
      this.error = null;
      try {
        const data = await getManufacturers();
        this.list = data;
        this.countries = [...new Set(data.map((m) => m.country))].sort();
        this.isLoaded = true;
      } catch (err) {
        this.error = err;
      } finally {
        this.loading = false;
      }
    },
  },
});
