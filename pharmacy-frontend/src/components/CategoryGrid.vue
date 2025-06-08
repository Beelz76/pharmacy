<template>
  <section class="py-16 bg-white">
    <!-- Заголовок -->
    <div class="max-w-7xl mx-auto px-4 text-center">
      <p class="text-primary-600 font-semibold uppercase tracking-wide">
        Категории
      </p>
      <h2 class="text-3xl font-extrabold text-gray-900 mt-2">
        Выберите по назначению
      </h2>
    </div>

    <!-- Категории -->
    <div v-if="topLevelCategories.length" class="mt-12 max-w-7xl mx-auto px-4">
      <div
        class="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-6"
      >
        <div
          v-for="category in topLevelCategories"
          :key="category.id"
          @click="onCategoryClick(category)"
          class="group cursor-pointer bg-gray-50 border border-gray-100 rounded-xl p-6 text-center hover:bg-primary-50 hover:shadow-md transition-all duration-200 flex flex-col items-center h-full"
        >
          <!-- Иконка -->
          <div
            class="bg-primary-600 text-white p-4 rounded-full inline-flex items-center justify-center mb-4"
          >
            <i class="fas fa-capsules text-2xl"></i>
          </div>

          <!-- Название -->
          <h3 class="text-lg font-semibold text-gray-900">
            {{ category.name }}
          </h3>

          <!-- Описание -->
          <p class="mt-2 text-sm text-gray-500 line-clamp-2">
            {{ category.description }}
          </p>
        </div>
      </div>
    </div>

    <!-- Загрузка -->
    <div v-else class="flex justify-center mt-12">
      <LoadingSpinner size="lg" color="primary" class="mx-auto" />
    </div>
  </section>
</template>

<script setup>
import { computed } from "vue";
import { useRouter } from "vue-router";
import { useCategoryStore } from "../stores/CategoryStore";
import LoadingSpinner from "../components/LoadingSpinner.vue";
import { toSlug } from "../utils/slugify";

const categoryStore = useCategoryStore();
const router = useRouter();

const topLevelCategories = computed(() => categoryStore.categories);

function onCategoryClick(category) {
  categoryStore.selectCategory(category.id, category.name);
  categoryStore.fetchPropertyFilters(category.id);
  router.push({ path: `/products/catalog/${toSlug(category.name)}` });
}
</script>
