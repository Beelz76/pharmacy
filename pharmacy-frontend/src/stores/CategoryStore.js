import { defineStore } from "pinia";
import api from "../utils/axios";
import { toSlug } from "../utils/slugify";

export const useCategoryStore = defineStore("categories", {
  state: () => ({
    categories: [],
    selectedCategoryId: null,
    selectedCategoryName: "Все категории",
    parentCategoryName: null,
    loading: false,
    isLoaded: false,
    error: null,
    propertyFilters: [],
  }),

  actions: {
    async fetchCategories() {
      if (this.isLoaded || this.loading) return;
      this.loading = true;
      this.error = null;
      try {
        const response = await api.get("/categories");
        this.categories = response.data;
        this.isLoaded = true;
      } catch (err) {
        this.error = err;
        console.error("Ошибка загрузки категорий:", err);
      } finally {
        this.loading = false;
      }
    },

    async fetchPropertyFilters(categoryId) {
      try {
        const response = await api.get(`/products/filter-values/${categoryId}`);
        this.propertyFilters = response.data;
      } catch (err) {
        console.error("Ошибка при загрузке значений фильтров:", err);
        this.propertyFilters = [];
      }
    },

    findCategoryBySlug(slug, list, parentName = null) {
      for (const cat of list) {
        if (toSlug(cat.name) === slug) {
          return { match: cat, parent: parentName };
        }
        if (cat.subcategories?.length) {
          const res = this.findCategoryBySlug(
            slug,
            cat.subcategories,
            cat.name
          );
          if (res?.match) return res;
        }
      }
      return null;
    },

    async fetchCategoryBySlug(slug) {
      if (!this.isLoaded) await this.fetchCategories();
      const result = this.findCategoryBySlug(slug, this.categories);
      if (result?.match) {
        this.selectCategory(result.match.id, result.match.name, result.parent);
        await this.fetchPropertyFilters(result.match.id);
      } else {
        this.resetCategory();
      }
    },

    selectCategory(id, name, parentName = null) {
      this.selectedCategoryId = id;
      this.selectedCategoryName = name;
      this.parentCategoryName = parentName;
    },

    resetCategory() {
      this.selectedCategoryId = null;
      this.selectedCategoryName = "Все категории";
      this.parentCategoryName = null;
      this.propertyFilters = [];
    },
  },
});
