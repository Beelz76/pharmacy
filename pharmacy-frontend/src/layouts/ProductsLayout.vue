<template>
  <div class="max-w-7xl mx-auto px-2 py-6 md:py-10">
    <div class="grid md:grid-cols-[260px_1fr] gap-8">
      <!-- Обертка фильтров с проксированием событий -->
      <ProductsSidebar @update:filters="onFiltersUpdate" />
      <router-view :filters="filters" @update:filters="onFiltersUpdate" />
    </div>
  </div>
</template>

<script setup>
import { ref } from "vue";
import ProductsSidebar from "../components/sidebars/ProductsSidebar.vue";

const filters = ref({
  isAvailable: null,
  isPrescriptionRequired: null,
  propertyFilters: {},
  categoryIds: [],
});

const onFiltersUpdate = (newFilters) => {
  filters.value = { ...newFilters };
};
</script>
